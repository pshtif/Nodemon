/*
 *	Created by:  Peter @sHTiF Stefcek
 *
 *  The serializer SEAM. Graphs, variables and configs used to call OdinSerializer
 *  directly from their ISerializationCallbackReceiver hooks; they now call
 *  GraphSerialization.Default, whose contract is deliberately Odin-shaped:
 *  fill a Unity-serializable blob from an object, populate an object back from
 *  the blob, byte round-trips with a side table of UnityEngine.Object refs, and
 *  a deep copy. Unity's undo snapshots the blob exactly as it snapshotted Odin's
 *  SerializationData, so nothing above the seam changes.
 *
 *  Define MACHINA_ODIN to compile the original Odin paths instead (each call site
 *  keeps them verbatim behind the define); without it the Json.NET implementation
 *  below is the only serializer in the build. Json.NET ships with Unity
 *  (com.unity.nuget.newtonsoft-json, MIT) — no third-party notice, no Apache.
 */

#if !MACHINA_ODIN
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Nodemon
{
    /// <summary>Stands in for OdinSerializer.DataFormat at the call sites that name
    /// a format; the Json.NET path has exactly one.</summary>
    public enum DataFormat
    {
        Binary,
        JSON,
    }

    /// <summary>What Unity serializes (and Undo snapshots) on the object's behalf:
    /// the graph as JSON text plus the UnityEngine.Object references it points at,
    /// by index. Mirrors Odin's SerializationData in role, not in format.</summary>
    [Serializable]
    public struct SerializedBlob
    {
        public string json;
        public List<Object> references;
    }

    public interface IGraphSerializer
    {
        /// <summary>Fill <paramref name="p_blob"/> from every serializable field of
        /// <paramref name="p_target"/>, Unity-serializable ones included.</summary>
        void Serialize(object p_target, ref SerializedBlob p_blob);

        /// <summary>Populate <paramref name="p_target"/> in place from the blob.
        /// Collections are REPLACED, not appended to. A blob with no json is a no-op.</summary>
        void Deserialize(object p_target, ref SerializedBlob p_blob);

        /// <summary>Byte form of <see cref="Serialize"/> with the reference side table
        /// exposed — the SubGraph binding and Clone paths key on it.</summary>
        byte[] ToBytes(object p_target, ref List<Object> p_references);

        void FromBytes(object p_target, byte[] p_bytes, ref List<Object> p_references);

        /// <summary>A deep copy: plain data is duplicated, UnityEngine.Object
        /// references stay shared — Odin's CreateCopy semantics.</summary>
        T DeepCopy<T>(T p_source) where T : class;
    }

    public static class GraphSerialization
    {
        public static IGraphSerializer Default { get; set; } = new JsonGraphSerializer();
    }

    /// <summary>
    /// Json.NET behind the seam. The parts that make it behave like Odin did:
    ///  - FIELDS, all of them, public and private, up the hierarchy, stopping at the
    ///    UnityEngine.Object base classes; [NonSerialized], delegates and pointers skip.
    ///  - $type on polymorphic members (NodeBase subclasses, object-typed slots),
    ///    $id/$ref so a node referenced by two connections stays ONE node.
    ///  - UnityEngine.Object references never inline: they become an index into the
    ///    side table, and a reference to the object being serialized itself becomes
    ///    a sentinel that resolves to whatever object is being populated — which is
    ///    how Clone lands its self-references on the clone.
    ///  - Types that merely implement IEnumerable (Variables, NodeFlowData) serialize
    ///    as objects, not arrays; only real collections are arrays.
    ///  - Classes without a parameterless constructor are created uninitialized.
    /// </summary>
    public sealed class JsonGraphSerializer : IGraphSerializer
    {
        // ---- per-call context: the side table and the root, for the reference converter ----

        sealed class Context
        {
            public object Root;
            public List<Object> Refs;
            /// <summary>Reading: slot 0 of the list is the object that was serialized,
            /// and resolves to whatever is being populated now (the twin, the clone,
            /// or the original) — without touching the list.</summary>
            public bool RootAtZero;
        }

        [ThreadStatic] static Context s_context;

        /// <summary>Odin's contract, which SubGraph binding relies on: the object being
        /// serialized is ITSELF entry 0 of the reference list (it looks itself up by
        /// index — `_selfReferenceIndex` — and Clone swaps that entry for the clone).
        /// The WRITER inserts it; the READER never mutates the list — an insert on read
        /// shifted every index by one and handed a Material slot the parent graph.</summary>
        static Context Enter(object p_root, List<Object> p_refs, bool p_writing)
        {
            var previous = s_context;
            var refs = p_refs ?? new List<Object>();
            bool rootIsUnity = p_root is Object;
            if (p_writing && rootIsUnity && !refs.Contains((Object)p_root)) refs.Insert(0, (Object)p_root);
            s_context = new Context { Root = p_root, Refs = refs, RootAtZero = rootIsUnity && refs.Count > 0 };
            return previous;
        }

        static void Leave(Context p_previous) => s_context = p_previous;

        // ---- the settings, cached per root type (the resolver caches contracts) ----

        static readonly Dictionary<Type, JsonSerializer> s_serializers = new Dictionary<Type, JsonSerializer>();

        static JsonSerializer SerializerFor(Type p_rootType)
        {
            if (!s_serializers.TryGetValue(p_rootType, out var s))
            {
                s = JsonSerializer.Create(new JsonSerializerSettings
                {
                    ContractResolver = new FieldContractResolver(p_rootType),
                    TypeNameHandling = TypeNameHandling.Auto,
                    TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
                    PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                    // Serialize, not Ignore: every ordinary object is $id/$ref-tracked, so a
                    // loop resolves to a $ref BEFORE this check; the only untracked objects are
                    // Unity references, whose converters never recurse. Ignore, by contrast,
                    // dropped a node's `_graph` (the root, on the stack) — and, once the
                    // property name was already written, left the writer in an invalid state:
                    // the converter path re-checks with a null property, so per-property
                    // overrides never reach it.
                    ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
                    ObjectCreationHandling = ObjectCreationHandling.Replace,
                    MissingMemberHandling = MissingMemberHandling.Ignore,
                    ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
                    DateParseHandling = DateParseHandling.None,
                    Formatting = Formatting.None,
                    Converters = { new SystemTypeConverter() },
                });
                s_serializers[p_rootType] = s;
            }
            return s;
        }

        // ---- IGraphSerializer ----

        public void Serialize(object p_target, ref SerializedBlob p_blob)
        {
            if (p_target == null) return;
            var refs = new List<Object>();
            var previous = Enter(p_target, refs, true);
            try
            {
                var sb = new StringBuilder(4096);
                using (var sw = new StringWriter(sb))
                using (var jw = new JsonTextWriter(sw))
                {
                    SerializerFor(p_target.GetType()).Serialize(jw, p_target, p_target.GetType());
                }
                p_blob.json = sb.ToString();
                p_blob.references = refs;
            }
            finally { Leave(previous); }
        }

        public void Deserialize(object p_target, ref SerializedBlob p_blob)
        {
            if (p_target == null || string.IsNullOrEmpty(p_blob.json)) return;
            var previous = Enter(p_target, p_blob.references, false);
            try
            {
                using (var sr = new StringReader(p_blob.json))
                using (var jr = new JsonTextReader(sr))
                {
                    SerializerFor(p_target.GetType()).Populate(jr, p_target);
                }
            }
            finally { Leave(previous); }
        }

        public byte[] ToBytes(object p_target, ref List<Object> p_references)
        {
            var blob = new SerializedBlob();
            Serialize(p_target, ref blob);
            p_references = blob.references ?? new List<Object>();
            return blob.json == null ? Array.Empty<byte>() : Encoding.UTF8.GetBytes(blob.json);
        }

        public void FromBytes(object p_target, byte[] p_bytes, ref List<Object> p_references)
        {
            if (p_bytes == null || p_bytes.Length == 0) return;
            var blob = new SerializedBlob { json = Encoding.UTF8.GetString(p_bytes), references = p_references };
            Deserialize(p_target, ref blob);
        }

        public T DeepCopy<T>(T p_source) where T : class
        {
            if (p_source == null) return null;
            var refs = new List<Object>();
            var previous = Enter(null, refs, true);
            try
            {
                var type = p_source.GetType();
                var serializer = SerializerFor(type);
                var sb = new StringBuilder(1024);
                using (var sw = new StringWriter(sb))
                using (var jw = new JsonTextWriter(sw))
                {
                    serializer.Serialize(jw, p_source, type);
                }
                using (var sr = new StringReader(sb.ToString()))
                using (var jr = new JsonTextReader(sr))
                {
                    return (T)serializer.Deserialize(jr, type);
                }
            }
            finally { Leave(previous); }
        }

        // ---- the contract resolver: fields, Odin-style ----

        sealed class FieldContractResolver : DefaultContractResolver
        {
            readonly Type _rootType;
            readonly UnityObjectReferenceConverter _refConverter = new UnityObjectReferenceConverter();
            readonly PolymorphicSlotConverter _slotConverter = new PolymorphicSlotConverter();

            public FieldContractResolver(Type p_rootType) { _rootType = p_rootType; }

            static bool IsUnityObject(Type t) => typeof(Object).IsAssignableFrom(t);

            /// <summary>A real collection gets Json.NET's array/dictionary treatment;
            /// a class that merely enumerates itself is an object with fields.</summary>
            static bool IsRealCollection(Type t)
            {
                if (t.IsArray) return true;
                if (typeof(IDictionary).IsAssignableFrom(t) || typeof(IList).IsAssignableFrom(t)) return true;
                foreach (var i in t.GetInterfaces())
                {
                    if (!i.IsGenericType) continue;
                    var d = i.GetGenericTypeDefinition();
                    if (d == typeof(ICollection<>) || d == typeof(IDictionary<,>) || d == typeof(IReadOnlyDictionary<,>))
                        return true;
                }
                return false;
            }

            protected override JsonContract CreateContract(Type objectType)
            {
                JsonContract contract;
                bool objectLike = !objectType.IsPrimitive && !objectType.IsEnum && objectType != typeof(string)
                                  && !IsRealCollection(objectType) && !typeof(Type).IsAssignableFrom(objectType)
                                  && !typeof(Delegate).IsAssignableFrom(objectType)
                                  && typeof(IEnumerable).IsAssignableFrom(objectType);
                contract = objectLike ? CreateObjectContract(objectType) : base.CreateContract(objectType);

                // A UnityEngine.Object anywhere but at the root is a REFERENCE, never inlined.
                if (IsUnityObject(objectType) && objectType != _rootType)
                    contract.Converter = _refConverter;
                // And NEVER part of $id/$ref tracking: Json.NET consults the reference
                // table before any property converter, so a node's `_graph` pointing
                // back at the root came out as {"$ref":"1"} — an object token handed to
                // a converter expecting an integer, which then left the reader stranded
                // mid-object. The side table is a Unity object's identity; $ref is not.
                if (IsUnityObject(objectType))
                    contract.IsReference = false;

                // Untyped slots inside collections (Dictionary<string, object>, List<object>)
                // have no field to hang a converter on — attach it to the ITEMS, so a
                // Material stored under structured["material"] survives as a reference.
                if (contract is JsonDictionaryContract dc && dc.DictionaryValueType != null
                    && (dc.DictionaryValueType == typeof(object) || dc.DictionaryValueType.IsInterface))
                    dc.ItemConverter = _slotConverter;
                if (contract is JsonArrayContract ac && ac.CollectionItemType != null
                    && (ac.CollectionItemType == typeof(object) || ac.CollectionItemType.IsInterface))
                    ac.ItemConverter = _slotConverter;

                if (contract is JsonObjectContract oc && oc.DefaultCreator == null && !objectType.IsAbstract && !objectType.IsInterface)
                {
                    oc.DefaultCreator = () => FormatterServices.GetUninitializedObject(objectType);
                    oc.DefaultCreatorNonPublic = true;
                }
                return contract;
            }

            protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
            {
                var props = new List<JsonProperty>();
                var names = new HashSet<string>();
                // Walk base-first so a shadowed field keeps the simple name at the base.
                var chain = new List<Type>();
                for (var t = type; t != null && t != typeof(object); t = t.BaseType)
                {
                    // Stop at UnityEngine's own base classes (Object, ScriptableObject,
                    // MonoBehaviour…): their fields are the native handle, not data.
                    if (IsUnityObject(t) && t.Assembly == typeof(Object).Assembly) break;
                    chain.Add(t);
                }
                chain.Reverse();

                foreach (var t in chain)
                {
                    foreach (var f in t.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
                    {
                        if (f.IsNotSerialized || f.IsDefined(typeof(NonSerializedAttribute), false)) continue;
                        if (f.IsDefined(typeof(JsonIgnoreAttribute), false)) continue;
                        var ft = f.FieldType;
                        if (typeof(Delegate).IsAssignableFrom(ft) || ft.IsPointer || ft == typeof(IntPtr) || ft == typeof(UIntPtr)) continue;
                        // The blob field is the OUTPUT of this serializer, never its input —
                        // Odin skipped its SerializationData the same way. Serializing it
                        // would nest last frame's blob inside this one, growing forever.
                        if (ft == typeof(SerializedBlob)) continue;

                        string name = f.Name;
                        // Auto-property backing field: <Name>k__BackingField → Name.
                        if (name.Length > 2 && name[0] == '<')
                        {
                            int end = name.IndexOf('>');
                            if (end > 1) name = name.Substring(1, end - 1);
                        }
                        if (!names.Add(name)) { name = t.Name + "." + name; names.Add(name); }

                        var p = base.CreateProperty(f, memberSerialization);
                        p.PropertyName = name;
                        p.Readable = true;
                        p.Writable = true;
                        p.Ignored = false;
                        p.ShouldSerialize = null;
                        // Unity references go through the converters — and must bypass
                        // Json.NET's circular-reference check: a node's `_graph` IS the
                        // root being serialized, and with the default Ignore handling the
                        // check dropped the property before the converter could write the
                        // sentinel. The converters never recurse, so Serialize is safe here.
                        if (IsUnityObject(ft)) { p.Converter = _refConverter; p.ReferenceLoopHandling = ReferenceLoopHandling.Serialize; }
                        else if (ft.IsInterface || ft == typeof(object)) { p.Converter = _slotConverter; p.ReferenceLoopHandling = ReferenceLoopHandling.Serialize; }
                        else if (ft.IsGenericType && typeof(IEnumerable).IsAssignableFrom(ft))
                        {
                            // List<Texture> and friends: convert the ITEMS.
                            foreach (var arg in ft.GetGenericArguments())
                                if (IsUnityObject(arg)) { p.ItemConverter = _refConverter; p.ItemReferenceLoopHandling = ReferenceLoopHandling.Serialize; break; }
                        }
                        else if (ft.IsArray && IsUnityObject(ft.GetElementType())) { p.ItemConverter = _refConverter; p.ItemReferenceLoopHandling = ReferenceLoopHandling.Serialize; }
                        props.Add(p);
                    }
                }
                return props;
            }
        }

        // ---- UnityEngine.Object → side-table index; the root → sentinel ----

        const long kRootSentinel = -2;

        sealed class UnityObjectReferenceConverter : JsonConverter
        {
            public override bool CanConvert(Type objectType) => typeof(Object).IsAssignableFrom(objectType);

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                var ctx = s_context;
                var o = value as Object;
                if (ctx == null || value == null || (o == null && !ReferenceEquals(value, ctx.Root))) { writer.WriteNull(); return; } // Unity's ==: destroyed reads as null
                int i = ctx.Refs.IndexOf(o);
                if (i < 0) { i = ctx.Refs.Count; ctx.Refs.Add(o); }
                writer.WriteValue(i);
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                if (reader.TokenType == JsonToken.Null) return null;
                var ctx = s_context;
                long i;
                if (reader.TokenType == JsonToken.StartObject)
                {
                    // Defensive: consume the whole object so a surprise never strands
                    // the reader; honour a tagged {"$uref":n} if that is what it is.
                    var jo = Newtonsoft.Json.Linq.JObject.Load(reader);
                    if (!jo.TryGetValue("$uref", out var tagged)) return null;
                    i = tagged.ToObject<long>();
                }
                else i = Convert.ToInt64(reader.Value);
                if (ctx == null) return null;
                if (i == kRootSentinel || (i == 0 && ctx.RootAtZero)) return ctx.Root;
                return i >= 0 && i < ctx.Refs.Count ? ctx.Refs[(int)i] : null;
            }
        }

        // ---- interface / object slots: a Unity ref becomes a tagged object, anything else
        //      goes through normal typed serialization ($type carries the runtime type) ----

        sealed class PolymorphicSlotConverter : JsonConverter
        {
            const string kRef = "$uref";

            public override bool CanConvert(Type objectType) => true;

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                if (value == null) { writer.WriteNull(); return; }
                var ctx = s_context;
                if (value is Object o && ctx != null)
                {
                    if (o == null && !ReferenceEquals(value, ctx.Root)) { writer.WriteNull(); return; }
                    long i = ctx.Refs.IndexOf(o); if (i < 0) { i = ctx.Refs.Count; ctx.Refs.Add(o); }
                    writer.WriteStartObject();
                    writer.WritePropertyName(kRef);
                    writer.WriteValue(i);
                    writer.WriteEndObject();
                    return;
                }
                // Declared as object: Auto type-naming emits $type for the runtime type.
                serializer.Serialize(writer, value, typeof(object));
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                if (reader.TokenType == JsonToken.Null) return null;
                if (reader.TokenType == JsonToken.StartObject)
                {
                    // Peek without losing the ability to hand a normal object to the serializer.
                    var token = Newtonsoft.Json.Linq.JToken.ReadFrom(reader);
                    if (token is Newtonsoft.Json.Linq.JObject jo && jo.TryGetValue(kRef, out var idx))
                    {
                        var ctx = s_context; long i = idx.ToObject<long>();
                        if (ctx == null) return null;
                        if (i == kRootSentinel || (i == 0 && ctx.RootAtZero)) return ctx.Root;
                        return i >= 0 && i < ctx.Refs.Count ? ctx.Refs[(int)i] : null;
                    }
                    using (var sub = token.CreateReader()) { sub.Read(); return serializer.Deserialize(sub, typeof(object)); }
                }
                return serializer.Deserialize(reader, typeof(object));
            }
        }

        // ---- System.Type as its assembly-qualified name ----

        sealed class SystemTypeConverter : JsonConverter
        {
            public override bool CanConvert(Type objectType) => typeof(Type).IsAssignableFrom(objectType);

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                if (value is Type t) writer.WriteValue(t.AssemblyQualifiedName); else writer.WriteNull();
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                if (reader.TokenType == JsonToken.Null) return null;
                return Type.GetType((string)reader.Value, false);
            }
        }
    }
}
#endif
