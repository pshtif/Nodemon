/*
 *	Created by:  Peter @sHTiF Stefcek
 */

#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using OdinSerializer;
using UnityEditor;
using UnityEngine;

namespace Nodemon.Editor
{
    [Serializable]
    public class AOTConfig : ScriptableObject, ISerializationCallbackReceiver
    {
        public static string name = "NodemonAOTConfig"; 
        
        public static AOTConfig Create()
        {
            AOTConfig config = (AOTConfig) AssetDatabase.LoadAssetAtPath("Assets/Resources/Editor/" + name + ".asset",
                typeof(AOTConfig));
            
            if (config == null)
            {
                config = ScriptableObject.CreateInstance<AOTConfig>();
                if (config != null)
                {
                    if (!AssetDatabase.IsValidFolder("Assets/Resources"))
                    {
                        AssetDatabase.CreateFolder("Assets", "Resources");
                        AssetDatabase.CreateFolder("Assets/Resources", "Editor");
                    } 
                    else if (!AssetDatabase.IsValidFolder("Assets/Resources/Editor"))
                    {
                        AssetDatabase.CreateFolder("Assets/Resources", "Editor");
                    }
                    AssetDatabase.CreateAsset(config, "Assets/Resources/Editor/" + name + ".asset");
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }
            }

            return config;
        }
        
        public string AOTAssemblyPath = "Assets/Plugins";
        public string AOTAssemblyName = "NodemonAOTAssembly"; 
        public DateTime AOTAssemblyGeneratedTime;

        public List<Type> aotTypes = new List<Type>();

        #region SERIALIZATION

        [SerializeField, HideInInspector]
        private SerializationData _serializationData;
        
        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            using (var cachedContext = OdinSerializer.Utilities.Cache<DeserializationContext>.Claim())
            {
                cachedContext.Value.Config.SerializationPolicy = SerializationPolicies.Everything;
                UnitySerializationUtility.DeserializeUnityObject(this, ref _serializationData, cachedContext.Value);
            }
        }
        
        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            using (var cachedContext = OdinSerializer.Utilities.Cache<SerializationContext>.Claim())
            {
                cachedContext.Value.Config.SerializationPolicy = SerializationPolicies.Everything;
                UnitySerializationUtility.SerializeUnityObject(this, ref _serializationData,
                    serializeUnityFields: true, context: cachedContext.Value);
            }
        }
        
        #endregion
    }
}
#endif