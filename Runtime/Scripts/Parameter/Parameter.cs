/*
 *	Created by:  Peter @sHTiF Stefcek
 */

using System;
using System.Collections.Generic;
using System.Reflection;
using Dash.NCalc;

namespace Nodemon
{

    [Serializable]
    public abstract class Parameter
    {
        public bool isExpression = false;
        
        public string expression;

        // True if last evaluation was erroneous
        public bool hasErrorInEvaluation = false;
        public string errorMessage;
        
        public abstract bool IsDefault();
        
        public abstract FieldInfo GetValueFieldInfo();
        
        static protected List<Parameter> _referenceChain = new List<Parameter>();
        
        public bool IsInReferenceChain(Parameter p_parameter)
        {
            return _referenceChain.Contains(p_parameter);
        }
    }

    [Serializable]
    public class Parameter<T> : Parameter
    {
        protected T _value;

        [NonSerialized] 
        private string _previousExpression;

        [NonSerialized] 
        private Expression cachedExpression;

        public override bool IsDefault()
        {
            return EqualityComparer<T>.Default.Equals(_value, default(T));
        } 
        
        public Parameter(T p_value)
        {
            _value = p_value;
        }

        public override FieldInfo GetValueFieldInfo()
        {
            return GetType().GetField("_value", BindingFlags.NonPublic | BindingFlags.Instance);
        }
        
        public T GetValue(IParameterResolver p_resolver, IAttributeDataCollection p_collection = null, bool p_referenced = false)
        {
            if (!p_referenced)
            {
                _referenceChain.Clear();
            }
            else
            {
                _referenceChain.Add(this);
            }
            
            if (isExpression)
            {
                // TODO Maybe do this directly after editing?
                expression = expression.RemoveWhitespace();
                if (string.IsNullOrEmpty(expression))
                {
                    errorMessage = "Expression cannot be empty returning default value.";
                    return default(T);
                }
                
                T value = ExpressionEvaluator.EvaluateExpression<T>(expression, p_resolver, p_referenced, p_collection);
                if (ExpressionEvaluator.hasErrorInEvaluation)
                {
                    errorMessage = ExpressionEvaluator.errorMessage;
                    hasErrorInEvaluation = true;
                }
                else
                {
                    hasErrorInEvaluation = false;
                }

                return value;
            }

            hasErrorInEvaluation = false;
            return _value;
        }

        public void SetValue(T p_value)
        {
            _value = p_value;
        }
        
        public override string ToString()
        {
            return isExpression ? expression : _value != null ? _value.ToString() : "NULL";
        }
    }
}