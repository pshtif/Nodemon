/*
 *	Created by:  Peter @sHTiF Stefcek
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Nodemon.NCalc;
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
        
        private static Dictionary<string, MethodInfo> _functionMethodsCache = new Dictionary<string, MethodInfo>();

        private static Dictionary<string,Expression> _expressionsCache;

        private static Dictionary<Type, MethodInfo> _typedMethodsCache = new Dictionary<Type, MethodInfo>();

        public static void ClearCache()
        {
            _expressionsCache = new Dictionary<string, Expression>();
        }

        public static object EvaluateTypedExpression<K>(string p_expression, Type p_returnType, IParameterResolver p_resolver, bool p_referenced, IAttributeDataCollection<K> p_collection = null, int p_index = 0) where K : DataAttribute
        {
            MethodInfo typed;
            if (_typedMethodsCache.ContainsKey(p_returnType))
            {
                typed = _typedMethodsCache[p_returnType];
            }
            else
            {
                MethodInfo method = typeof(ExpressionEvaluator).GetMethod("EvaluateExpression",
                    BindingFlags.Public | BindingFlags.Static);
                typed = method.MakeGenericMethod(p_returnType, typeof(K));
                _typedMethodsCache[p_returnType] = typed;
            }

            return typed.Invoke(null, new object[] { p_expression, p_resolver, p_referenced, p_collection, p_index });
        }

        // private static List<String> ExtractSubExpressions(string p_expression)
        // {
        //     Debug.Log("ExtractSubExpressions");
        //     Regex regex = new Regex(@"\[(?=.*?\()(.*?\((.*?)\).*?)\]");
        //     MatchCollection matches = regex.Matches(p_expression);
        //     foreach (Match match in matches) {
        //         Debug.Log(match.Groups.Count);
        //         string expressionInsideSquareBrackets = match.Groups[1].Value;
        //         string expressionInsideParentheses = match.Groups[2].Value;
        //         Debug.Log("Expression inside square brackets: " + expressionInsideSquareBrackets);
        //         Debug.Log("Expression inside parentheses: " + expressionInsideParentheses);
        //     }
        //
        //     return null;
        // }

        public static T EvaluateExpression<T,K>(string p_expression, IParameterResolver p_resolver, bool p_referenced, IAttributeDataCollection<K> p_collection = null, int p_index = 0) where K : DataAttribute
        {
            hasErrorInEvaluation = false;
            if (_expressionsCache == null) _expressionsCache = new Dictionary<string, Expression>();

            Expression cachedExpression;
            if (!_expressionsCache.ContainsKey(p_expression))
            {
                cachedExpression = new Expression(p_expression);
                _expressionsCache.Add(p_expression, cachedExpression);
            }
            else
            {
                cachedExpression = _expressionsCache[p_expression];
            }

            EvaluateFunctionHandler evalFunction = (name, args) => EvaluateFunction<T>(name, args);
            cachedExpression.EvaluateFunction += evalFunction; 
            EvaluateParameterHandler evalParam = (name, args) => EvaluateParameter(name, args, p_resolver, p_referenced, p_collection, p_index);
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

        static void EvaluateParameter<K>(string p_name, ParameterArgs p_args, IParameterResolver p_resolver, bool p_referenced, IAttributeDataCollection<K> p_collection, int p_index) where K : DataAttribute
        {
            if (p_resolver == null)
            {
                hasErrorInEvaluation = true;
                errorMessage = "No parameter resolver provided to resolve parameter " + p_name;
            }
            else
            {
                p_args.Result = p_resolver.Resolve(p_name, p_referenced, p_collection, p_index);

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

            if (_functionMethodsCache.ContainsKey(p_name))
            {
                methodInfo = _functionMethodsCache[p_name];
            } else {
                methodInfo = typeof(ExpressionFunctions).GetMethod(p_name,
                    BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public);
                
                if (methodInfo == null)
                {
                    methodInfo = GetCustomFunction(p_name);
                }
                
                if (methodInfo != null) _functionMethodsCache.Add(p_name, methodInfo);
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

        private static bool _cacheFetched = false;
        private static List<Type> _cachedCustomFunctionClasses;
        
        static MethodInfo GetCustomFunction(string p_name)
        {
            if (!_cacheFetched)
            {
                _cacheFetched = true;
                _cachedCustomFunctionClasses = AppDomain.CurrentDomain.GetAssemblies().SelectMany(a =>
                        a.GetTypes().Where(t => t.IsDefined(typeof(ExpressionFunctionsAttribute), true)))
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