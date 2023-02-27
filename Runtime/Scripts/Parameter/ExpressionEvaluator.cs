/*
 *	Created by:  Peter @sHTiF Stefcek
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Dash.NCalc;
using UnityEngine;

namespace Nodemon
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ExpressionFunctionsAttribute : Attribute
    {
    }
    
    public class ExpressionEvaluator
    {
        public static bool enableCustomExpressionClasses = false;
        
        public static bool hasErrorInEvaluation { get; protected set; } = false;
        public static string errorMessage;
        
        private static Dictionary<string, MethodInfo> _methodCache = new Dictionary<string, MethodInfo>();

        private static Dictionary<string,Expression> _cachedExpressions;

        public static void ClearCache()
        {
            _cachedExpressions = new Dictionary<string, Expression>();
        }

        public static object EvaluateTypedExpression(string p_expression, Type p_returnType, IParameterResolver p_resolver, IAttributeDataCollection p_collection = null)
        {
            MethodInfo method = typeof(ExpressionEvaluator).GetMethod("EvaluateExpression", BindingFlags.Public | BindingFlags.Static);
            MethodInfo generic = method.MakeGenericMethod(p_returnType);
            return generic.Invoke(null, new object[] { p_expression, p_resolver, p_collection });
        }
        
        public static object EvaluateUntypedExpression(string p_expression, IParameterResolver p_resolver, IAttributeDataCollection p_collection, bool p_referenced)
        {
            hasErrorInEvaluation = false;
            if (_cachedExpressions == null) _cachedExpressions = new Dictionary<string, Expression>();

            Expression cachedExpression;
            if (!_cachedExpressions.ContainsKey(p_expression))
            {
                cachedExpression = new Expression(p_expression);
                _cachedExpressions.Add(p_expression, cachedExpression);
            }
            else
            {
                cachedExpression = _cachedExpressions[p_expression];
            }
            
            EvaluateFunctionHandler evalFunction = (name, args) => EvaluateFunction(name, args);
            cachedExpression.EvaluateFunction += evalFunction; 
            EvaluateParameterHandler evalParam = (name, args) => EvaluateParameter(name, args, p_resolver, p_referenced, p_collection);
            cachedExpression.EvaluateParameter += evalParam;
            
            object obj = null;
            try
            {
                obj = cachedExpression.Evaluate();
            }
            catch (Exception e)
            {
                errorMessage = e.Message;
                hasErrorInEvaluation = true;
            }
            
            cachedExpression.EvaluateFunction -= evalFunction;
            cachedExpression.EvaluateParameter -= evalParam;

            return obj;
        }
        
        public static T EvaluateExpression<T>(string p_expression, IParameterResolver p_resolver, bool p_referenced, IAttributeDataCollection p_collection = null)
        {
            hasErrorInEvaluation = false;
            if (_cachedExpressions == null) _cachedExpressions = new Dictionary<string, Expression>();

            Expression cachedExpression;
            if (!_cachedExpressions.ContainsKey(p_expression))
            {
                cachedExpression = new Expression(p_expression);
                _cachedExpressions.Add(p_expression, cachedExpression);
            }
            else
            {
                cachedExpression = _cachedExpressions[p_expression];
            }

            EvaluateFunctionHandler evalFunction = (name, args) => EvaluateFunction<T>(name, args);
            cachedExpression.EvaluateFunction += evalFunction; 
            EvaluateParameterHandler evalParam = (name, args) => EvaluateParameter(name, args, p_resolver, p_referenced, p_collection);
            cachedExpression.EvaluateParameter += evalParam;
            
            object obj = null;
            try
            {
                obj = cachedExpression.Evaluate();
            }
            catch (Exception e)
            {
                // If we already have a known error it is that one that is the internal problem so don't overwrite it
                if (!hasErrorInEvaluation)
                {
                    errorMessage = e.Message;
                    hasErrorInEvaluation = true;
                }
            }

            cachedExpression.EvaluateFunction -= evalFunction;
            cachedExpression.EvaluateParameter -= evalParam;
            
            if (obj != null)
            {
                Type returnType = obj.GetType();
                if (typeof(T).IsAssignableFrom(returnType))
                {
                    return (T) obj;    
                }

                // Explicit Double to Single casting (may add as option)
                if (typeof(T) == typeof(float) && returnType == typeof(Double))
                {
                    return (T) Convert.ChangeType(obj, typeof(T));
                }
                
                if (typeof(T).IsImplicitlyAssignableFrom(returnType))
                {
                    return (T) Convert.ChangeType(obj, typeof(T));
                }

                if (typeof(T) == typeof(string))
                {
                    return (T) (object) obj.ToString();
                }

                Debug.LogWarning("Invalid expression casting " + obj.GetType() + " and " + typeof(T));
            }


            return default(T);
        }

        static void EvaluateParameter(string p_name, ParameterArgs p_args, IParameterResolver p_resolver, bool p_referenced, IAttributeDataCollection p_collection)
        {
            if (p_resolver == null)
            {
                hasErrorInEvaluation = true;
                errorMessage = "No parameter resolver provided to resolve parameter " + p_name;
            }
            else
            {
                p_args.Result = p_resolver.Resolve(p_name, p_referenced, p_collection);

                if (!hasErrorInEvaluation && p_resolver.hasErrorInResolving)
                {
                    errorMessage = p_resolver.errorMessage;
                }
                
                hasErrorInEvaluation = hasErrorInEvaluation || p_resolver.hasErrorInResolving;
            }
        }

        static void EvaluateFunction<T>(string p_name, FunctionArgs p_args)
        {
            //Debug.Log("EvaluateFunction("+p_name+","+p_args+")");
            MethodInfo methodInfo = null;

            if (_methodCache.ContainsKey(p_name))
            {
                methodInfo = _methodCache[p_name];
            } else {
                methodInfo = typeof(ExpressionFunctions).GetMethod(p_name,
                    BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public);
                
                if (methodInfo == null)
                {
                    methodInfo = GetCustomFunction(p_name);
                }
                
                if (methodInfo != null) _methodCache.Add(p_name, methodInfo);
            }

            if (methodInfo != null)
            {
                // Can be generic using parameter type
                if (methodInfo.IsGenericMethod)
                {
                    methodInfo = methodInfo.MakeGenericMethod(typeof(T));
                }
                
                // TODO maybe typize the args but it would probably just slow things down
                bool success = (bool)methodInfo.Invoke(null, new object[] {p_args});
                if (!hasErrorInEvaluation && !success)
                {
                    errorMessage = ExpressionFunctions.errorMessage;
                }
                hasErrorInEvaluation = hasErrorInEvaluation || !success;
            }
            else
            {
                if (!hasErrorInEvaluation)
                {
                    errorMessage = "Function " + p_name + " not found";
                }

                hasErrorInEvaluation = true;
            }
        }
        
        static void EvaluateFunction(string p_name, FunctionArgs p_args)
        {
            MethodInfo methodInfo = null;
            if (_methodCache.ContainsKey(p_name))
            {
                methodInfo = _methodCache[p_name];
            }
            else
            {
                methodInfo = typeof(ExpressionFunctions).GetMethod(p_name,
                    BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public);

                if (methodInfo == null && enableCustomExpressionClasses)
                {
                    methodInfo = GetCustomFunction(p_name);
                }
                
                if (methodInfo != null) _methodCache.Add(p_name, methodInfo);
            }

            if (methodInfo != null)
            {
                // TODO maybe typize the args but it would probably just slow things down
                bool success = (bool)methodInfo.Invoke(null, new object[] {p_args});
                if (!hasErrorInEvaluation && !success)
                {
                    errorMessage = ExpressionFunctions.errorMessage;
                }
                
                hasErrorInEvaluation = hasErrorInEvaluation || !success;
            }
            else
            {
                if (!hasErrorInEvaluation)
                {
                    errorMessage = "Function " + p_name + " not found";
                }

                hasErrorInEvaluation = true;
            }
        }

        private static bool _cacheFetched = false;
        private static List<Type> _cachedCustomFunctionClasses;
        
        static MethodInfo GetCustomFunction(string p_name)
        {
            if (!_cacheFetched)
            {
                _cacheFetched = true;
                _cachedCustomFunctionClasses = AppDomain.CurrentDomain.GetAssemblies().SelectMany(a =>
                        a.GetTypes().Where(t => t.IsDefined(typeof(ExpressionFunctionsAttribute))))
                    .ToList();
            }

            if (_cachedCustomFunctionClasses == null || _cachedCustomFunctionClasses.Count == 0)
                return null;

            MethodInfo methodInfo = null;
            foreach (Type type in _cachedCustomFunctionClasses)
            {
                methodInfo = type.GetMethod(p_name,
                    BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public);
                
                if (methodInfo != null)
                    break;
            }

            return methodInfo;
        }
    }
}