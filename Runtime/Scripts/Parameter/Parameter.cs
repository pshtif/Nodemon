/*
 *	Created by:  Peter @sHTiF Stefcek
 */

using System;
using System.Collections.Generic;
using System.Reflection;
using Nodemon.NCalc;
using UniversalGUI;

namespace Nodemon
{

    [Serializable]
    public abstract class Parameter
    {
        abstract protected object objectValue { get; set; }
        
        public object value
        {
            get { return objectValue; }
            set { objectValue = value; }
        }
        
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
        
        public virtual Type GetValueType()
        {
            var fieldInfo = GetValueFieldInfo();
            return fieldInfo != null ? fieldInfo.FieldType : null;
        }

        public static Parameter Create(Type p_type)
        {
            return (Parameter)Activator.CreateInstance(typeof(Parameter<>).MakeGenericType(p_type),
                new object[] { p_type.GetDefaultValue() });
        }

        public static Parameter<T> Create<T>()
        {
            return new Parameter<T>(default(T));
        }
        
        public object GetValue<K>(IParameterResolver p_resolver, Type p_parameterType, IAttributeDataCollection<K> p_collection = null, int p_index = 0, bool p_referenced = false) where K : DataAttribute
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
                    return null;
                }
                
                object expressionValue = ExpressionEvaluator.EvaluateTypedExpression(expression, p_parameterType, p_resolver, p_referenced, p_collection, p_index);
                if (ExpressionEvaluator.hasErrorInEvaluation)
                {
                    errorMessage = ExpressionEvaluator.errorMessage;
                    hasErrorInEvaluation = true;
                }
                else
                {
                    hasErrorInEvaluation = false;
                }
        
                return expressionValue;
            }
        
            hasErrorInEvaluation = false;
            return value;
        }
    }

    [Serializable]
    public class Parameter<T> : Parameter
    {
        protected T _value;
        
        public new T value
        {
            get
            {
                return _value; 
            }
            set
            {
               _value = value;
            }
        }
        
        protected override object objectValue
        {
            get { return value; }
            set { this.value = (T)value; }
        }

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

        public override Type GetValueType()
        {
            return typeof(T);
        }
        
        public T GetValue(IParameterResolver p_resolver, bool p_referenced = false)
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
                expression = expression.RemoveWhitespace();
                if (string.IsNullOrEmpty(expression))
                {
                    errorMessage = "Expression cannot be empty returning default value.";
                    return default(T);
                }
                
                T expressionValue = ExpressionEvaluator.EvaluateExpression<T,DataAttribute>(expression, p_resolver, p_referenced, NodeFlowData<DataAttribute>.EMPTY);
                if (ExpressionEvaluator.hasErrorInEvaluation)
                {
                    errorMessage = ExpressionEvaluator.errorMessage;
                    hasErrorInEvaluation = true;
                }
                else
                {
                    hasErrorInEvaluation = false;
                }

                return expressionValue;
            }

            hasErrorInEvaluation = false;
            return _value;
        }

        public T GetValue<K>(IParameterResolver p_resolver, IAttributeDataCollection<K> p_collection = null, int p_index = 0, bool p_referenced = false) where K : DataAttribute
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
                
                T expressionValue = ExpressionEvaluator.EvaluateExpression<T,K>(expression, p_resolver, p_referenced, p_collection, p_index);
                if (ExpressionEvaluator.hasErrorInEvaluation)
                {
                    errorMessage = ExpressionEvaluator.errorMessage;
                    hasErrorInEvaluation = true;
                }
                else
                {
                    hasErrorInEvaluation = false;
                }

                return expressionValue;
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