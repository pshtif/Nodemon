/*
 *	Created by:  Peter @sHTiF Stefcek
 */

using System;
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

        public static void ClearCache()
        {
            // No managed cache anymore — the backend owns any compiled-program caching.
        }

        /// <summary>
        /// Untyped entry point — runtime-typed return value. Used by the non-generic
        /// <see cref="Parameter"/> base class which only has the parameter type as
        /// reflection info. Goes straight to the backend with no <c>MakeGenericMethod</c>
        /// reflection (and so is IL2CPP / iOS-safe for any T).
        /// </summary>
        public static object EvaluateTypedExpression<K>(string p_expression, Type p_returnType, IParameterResolver p_resolver, bool p_referenced, IAttributeDataCollection<K> p_collection = null, int p_index = 0) where K : DataAttribute
        {
            hasErrorInEvaluation = false;
            errorMessage = null;

            if (Backend == null)
            {
                hasErrorInEvaluation = true;
                errorMessage = "No expression backend installed.";
                return null;
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

            try
            {
                object raw = Backend(p_expression, p_returnType, resolveCallback, out bool backendErr, out string backendMsg);
                if (backendErr && !hasErrorInEvaluation)
                {
                    hasErrorInEvaluation = true;
                    errorMessage = backendMsg;
                }
                return ConvertResult(raw, p_returnType);
            }
            catch (Exception e)
            {
                if (!hasErrorInEvaluation)
                {
                    errorMessage = e.Message;
                    hasErrorInEvaluation = true;
                }
                return null;
            }
        }

        /// <summary>
        /// Typed convenience wrapper over <see cref="EvaluateTypedExpression{K}"/>. Returns T
        /// directly so callers don't have to unbox. The actual work — and the only place the
        /// backend is invoked — is the untyped path.
        /// </summary>
        public static T EvaluateExpression<T,K>(string p_expression, IParameterResolver p_resolver, bool p_referenced, IAttributeDataCollection<K> p_collection = null, int p_index = 0) where K : DataAttribute
        {
            object r = EvaluateTypedExpression(p_expression, typeof(T), p_resolver, p_referenced, p_collection, p_index);
            return r is T t ? t : default(T);
        }

        /// <summary>
        /// Coerces the backend's raw output to the requested return type. Handles common
        /// numeric conversions (double → float, int → float, etc.) and falls back to
        /// <c>Convert.ChangeType</c> for implicit casts. Returns null on no match.
        /// </summary>
        static object ConvertResult(object obj, Type returnType)
        {
            if (obj == null) return null;

            Type srcType = obj.GetType();
            if (returnType.IsAssignableFrom(srcType)) return obj;

            // Common numeric conversions — backend may hand us double/float/int.
            if (returnType == typeof(float) && (srcType == typeof(double) || srcType == typeof(int)))
                return Convert.ChangeType(obj, returnType);

            if (returnType.IsImplicitlyAssignableFrom(srcType))
                return Convert.ChangeType(obj, returnType);

            if (returnType == typeof(string))
                return obj.ToString();

            Debug.LogWarning("Invalid expression casting " + srcType + " and " + returnType);
            return null;
        }
    }
}
