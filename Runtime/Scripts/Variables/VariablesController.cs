/*
 *	Created by:  Peter @sHTiF Stefcek
 */

#if MACHINA_ODIN
using OdinSerializer;
using OdinSerializer.Utilities;
#endif
using UnityEngine;

namespace Nodemon
{
#if MACHINA_ODIN
    public class VariablesController : MonoBehaviour, ISerializationCallbackReceiver, ISupportsPrefabSerialization, IVariableBindable
#else
    public class VariablesController : MonoBehaviour, ISerializationCallbackReceiver, IVariableBindable
#endif
    {

        [SerializeField] 
        protected Variables _variables;

        public Variables Variables
        {
            get
            {
                if (_variables == null) _variables = new Variables();

                return _variables;
            }
        }


        private void Awake()
        {
            Variables.Initialize(this);
            
            //MachinaCore.Instance.SetGlobalVariables(this);
        }

        public void MarkDirty()
        {
            #if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
            #endif
        }
        
        private void OnDestroy()
        {
            //MachinaCore.Instance.SetGlobalVariables(null);
        }

#if MACHINA_ODIN
        [SerializeField, HideInInspector]
        private SerializationData _serializationData;
        
        SerializationData ISupportsPrefabSerialization.SerializationData { get { return this._serializationData; } set { this._serializationData = value; } }
        
        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            //Debug.Log("OnAfterDeserialize");
            using (var cachedContext = Cache<DeserializationContext>.Claim())
            {
                cachedContext.Value.Config.SerializationPolicy = SerializationPolicies.Everything;
                UnitySerializationUtility.DeserializeUnityObject(this, ref _serializationData, cachedContext.Value);
            }
        }
        
        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            //Debug.Log("OnBeforeSerialize");
            using (var cachedContext = Cache<SerializationContext>.Claim())
            {
                cachedContext.Value.Config.SerializationPolicy = SerializationPolicies.Everything;
                UnitySerializationUtility.SerializeUnityObject(this, ref _serializationData, serializeUnityFields: true, context: cachedContext.Value);
            }
        }
#else
        [SerializeField, HideInInspector]
        private SerializedBlob _serializationData;

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            GraphSerialization.Default.Deserialize(this, ref _serializationData);
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            GraphSerialization.Default.Serialize(this, ref _serializationData);
        }
#endif
    }
}