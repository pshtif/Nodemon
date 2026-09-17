/*
 *	Created by:  Peter @sHTiF Stefcek
 */

using UnityEngine;

namespace Nodemon
{
    public class VariablesController : MonoBehaviour, ISerializationCallbackReceiver, IVariableBindable
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
    }
}