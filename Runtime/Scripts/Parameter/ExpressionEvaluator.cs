/*
 *	Created by:  Peter @sHTiF Stefcek
 */

using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Nodemon
{
    /// <summary>
    /// Expression evaluation hub. The actual expression language is provided by an installed
    /// <see cref="Backend"/> delegate — Nodemon itself ships without one, so consumers must
    /// register a backend at startup (e.g. Machina installs a wrangle/VEX-subset backend).
    /// Until a backend is installed, expressions resolve to <c>default(T)</c> and an error is
    /// reported via <see cref="hasErrorInEvaluation"/>.
    /// </summary>
    public class ExpressionEvaluator
    {
        /// <summary>
        /// Pluggable expression backend. Receives the source string, the requested return
        /// type, and a per-call <c>resolve(name, index)</c> callback that closes over the
        /// caller's <see cref="IParameterResolver"/> and attribute collection. Returns the
        /// raw evaluated object; <see cref="EvaluateExpression{T,K}"/> converts to T at the
        /// boundary.
        /// </summary>
        public delegate object ExpressionBackend(string expression, Type returnType, Func<string, int, object> resolve, out bool hadError, out string errorMessage);

        public static ExpressionBackend Backend;

        public static bool hasErrorInEvaluation { get; protected set; } = false;
        public static string errorMessage;

        private static Dictionary<Type, MethodInfo> _typedMethodsCache = new Dictionary<Type, MethodInfo>();

        public static void ClearCache()
        {
            // No managed cache anymore — the backend owns any compiled-program caching.
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

        public static T EvaluateExpression<T,K>(string p_expression, IParameterResolver p_resolver, bool p_referenced, IAttributeDataCollection<K> p_collection = null, int p_index = 0) where K : DataAttribute
        {
            hasErrorInEvaluation = false;
            errorMessage = null;

            if (Backend == null)
            {
                hasErrorInEvaluation = true;
                errorMessage = "No expression backend installed.";
                return default(T);
            }

            // Closure that the backend calls back into for parameter resolution. Closes over
            // the caller's generic K so the backend doesn't have to be generic itself.
            Func<string, int, object> resolveCallback = (name, idx) =>
            {
                if (p_resolver == null)
                {
                    if (!hasErrorInEvaluation)
                    {
                        hasErrorInEvaluation = true;
                        errorMessage = "No parameter resolver provided to resolve parameter " + name;
                    }
                    return null;
                }
                object r = p_resolver.Resolve(name, p_referenced, p_collection, idx);
                if (!hasErrorInEvaluation && p_resolver.hasErrorInResolving)
                {
                    hasErrorInEvaluation = true;
                    errorMessage = p_resolver.errorMessage;
                }
                return r;
            };

            object obj;
            try
            {
                obj = Backend(p_expression, typeof(T), resolveCallback, out bool backendErr, out string backendMsg);
                if (backendErr && !hasErrorInEvaluation)
                {
                    hasErrorInEvaluation = true;
                    errorMessage = backendMsg;
                }
            }
            catch (Exception e)
            {
                if (!hasErrorInEvaluation)
                {
                    errorMessage = e.Message;
                    hasErrorInEvaluation = true;
                }
                return default(T);
            }

            if (obj != null)
            {
                Type returnType = obj.GetType();
                if (typeof(T).IsAssignableFrom(returnType))
                {
                    return (T) obj;
                }

                // Common numeric conversions — backend may hand us double/float/int.
                if (typeof(T) == typeof(float) && (returnType == typeof(double) || returnType == typeof(int)))
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
    }
}
