/*
 *	Created by:  Peter @sHTiF Stefcek
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using UnityEngine;

namespace Nodemon
{
    [Serializable]
    public class Variables
    {
        public int Count => _variables.Count;

        [NonSerialized] 
        protected Dictionary<string, Variable> _lookupDictionary;

        [SerializeField] 
        public List<Variable> _variables = new List<Variable>();

        public List<Variable> Get()
        {
            return _variables;
        }

        public void Initialize(GameObject p_target)
        {
            _variables.ForEach(v => v.InitializeBinding(p_target));
            _variables.ForEach(v => v.InitializeLookup(p_target));
        }

        public void ClearVariables()
        {
            _lookupDictionary = new Dictionary<string, Variable>();
            _variables = new List<Variable>();
        }

        public bool HasVariable(string p_name)
        {
            if (_lookupDictionary == null) InvalidateLookup();

            return _lookupDictionary.ContainsKey(p_name);
        }

        public Variable GetVariable(string p_name)
        {
            if (!HasVariable(p_name))
                return null;

            return _lookupDictionary[p_name];
        }

        public Variable<T> GetVariable<T>(string p_name)
        {
            if (!HasVariable(p_name))
                return null;

            return (Variable<T>) _lookupDictionary[p_name];
        }

        public void AddVariableByType(Type p_type, string p_name, [CanBeNull] object p_value)
        {
            if (HasVariable(p_name))
                return;

            MethodInfo method = this.GetType().GetMethod("AddVariable");
            MethodInfo generic = method.MakeGenericMethod(p_type);
            generic.Invoke(this, new object[] {p_name, p_value});
        }

        public void AddVariable<T>(string p_name, [CanBeNull] T p_value)
        {
            if (HasVariable(p_name))
                return;

            Variable<T> variable = new Variable<T>(p_name, p_value);
            _variables.Add(variable);
            InvalidateLookup();
        }

        public void SetVariable<T>(string p_name, [CanBeNull] T p_value)
        {
            if (HasVariable(p_name))
            {
                ((Variable<T>) _lookupDictionary[p_name]).value = p_value;
            }
            else
            {
                Variable<T> variable = new Variable<T>(p_name, p_value);
                _variables.Add(variable);
                InvalidateLookup();
            }
        }

        public void PasteVariable(Variable p_variable, GameObject p_target)
        {
            p_variable.Rename(GetUniqueName(p_variable.Name));
            _variables.Add(p_variable);
            if (p_target != null)
            {
                p_variable.InitializeBinding(p_target);
            }

            InvalidateLookup();
        }

        public void RemoveVariable(string p_name)
        {
            _variables.RemoveAll(v => v.Name == p_name);
        }

        // Renaming in dictionary is tricky but still better than having list as renaming is sporadic
        public bool RenameVariable(string p_oldName, string p_newName)
        {
            var variable = _variables.Find(v => v.Name == p_oldName);
            variable.Rename(GetUniqueName(p_newName));
            InvalidateLookup();

            return true;
        }

        private void InvalidateLookup()
        {
            _lookupDictionary = new Dictionary<string, Variable>();
            foreach (Variable variable in _variables)
            {
                _lookupDictionary.Add(variable.Name, variable);
            }
        }

        private string GetUniqueName(string p_name)
        {
            while (HasVariable(p_name))
            {
                string number = string.Concat(p_name.Reverse().TakeWhile(char.IsNumber).Reverse());
                p_name = p_name.Substring(0, p_name.Length - number.Length) +
                         (string.IsNullOrEmpty(number) ? 1 : (Int32.Parse(number) + 1));
            }

            return p_name;
        }

#if UNITY_EDITOR
        public void AddNewVariable(Type p_type)
        {
            string name = "new" + p_type.ToString().Substring(p_type.ToString().LastIndexOf(".") + 1);

            AddVariableByType((Type) p_type, GetUniqueName(name), null);
        }
#endif
    }
}