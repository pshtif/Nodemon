/*
 *	Created by:  Peter @sHTiF Stefcek
 */

using System;
using NCalc;
using UnityEngine;

namespace Nodemon
{
    
    public class ExpressionFunctions
    {
        static public string errorMessage;
        
        private static bool Create<T>(FunctionArgs p_args)
        {
            // Debug.Log("Create<T>");
            object[] evalParams = p_args.EvaluateParameters();

            if (typeof(T) == typeof(Vector2))
            {
                if (evalParams.Length != 2)
                {
                    errorMessage = "Invalid parameters for Create function of type "+typeof(T);
                    return false;
                }

                p_args.HasResult = true;
                p_args.Result = new Vector2(Convert.ToSingle(evalParams[0]), Convert.ToSingle(evalParams[1]));
                return true;
            } 
            
            if (typeof(T) == typeof(Vector3))
            {
                if (evalParams.Length != 3)
                {
                    errorMessage = "Invalid parameters for Create function of type " + typeof(T);
                    return false;
                }

                p_args.HasResult = true;
                p_args.Result = new Vector3(Convert.ToSingle(evalParams[0]), Convert.ToSingle(evalParams[1]), Convert.ToSingle(evalParams[2]));
                return true;
            }
            
            errorMessage = "Create function for type " + typeof(T)+" is not implemented.";
            return false;
        }
        
        private static bool Vector2(FunctionArgs p_args)
        {
            if (p_args.Parameters.Length != 2)
            {
                errorMessage = "Invalid parameters in Vector3 function.";
                return false;
            }
            
            object[] evalParams = p_args.EvaluateParameters();

            p_args.HasResult = true;
            p_args.Result = new Vector2(Convert.ToSingle(evalParams[0]), Convert.ToSingle(evalParams[1]));
            return true;
        }
        
        /**
         *  Create a Vector3 value
         */
        private static bool Vector3(FunctionArgs p_args)
        {
            if (p_args.Parameters.Length != 3)
            {
                errorMessage = "Invalid parameters in Vector3 function.";
                return false;
            }
            
            object[] evalParams = p_args.EvaluateParameters();

            p_args.HasResult = true;
            p_args.Result = new Vector3(Convert.ToSingle(evalParams[0]), Convert.ToSingle(evalParams[1]), Convert.ToSingle(evalParams[2]));
            return true;
        }

        private static bool Add<T>(FunctionArgs p_args)
        {
            object[] evalParams = p_args.EvaluateParameters();
            if (evalParams.Length != 2)
            {
                errorMessage = "Invalid parameters for Add function.";
                return false;
            }
            
            if (typeof(T) == typeof(Vector3) && evalParams[1].GetType() == typeof(Vector3))
            {
                p_args.HasResult = true;
                p_args.Result = (Vector3) evalParams[0] + (Vector3) evalParams[1];
                return true;
            }

            errorMessage = "Add function for type " + typeof(T) + " is not implemented.";
            return false;
        }

        private static bool Random<T>(FunctionArgs p_args)
        {
            object[] evalParams = p_args.EvaluateParameters();
            
            if (p_args.Parameters.Length != 2)
            {
                errorMessage = "Invalid parameters for Random function of type " + typeof(T);
                return false;
            }
            
            if (typeof(T) == typeof(Vector3))
            {
                p_args.HasResult = true;
                p_args.Result = new Vector3(
                    UnityEngine.Random.Range(Convert.ToSingle(evalParams[0]), Convert.ToSingle(evalParams[1])),
                    UnityEngine.Random.Range(Convert.ToSingle(evalParams[0]), Convert.ToSingle(evalParams[1])),
                    UnityEngine.Random.Range(Convert.ToSingle(evalParams[0]), Convert.ToSingle(evalParams[1])));
                return true;
            }

            if (typeof(T) == typeof(Int32))
            {
                p_args.HasResult = true;
                p_args.Result = UnityEngine.Random.Range(Convert.ToInt32(evalParams[0]), Convert.ToInt32(evalParams[1]));
                return true;
            }
            
            if (typeof(T) == typeof(float))
            {
                p_args.HasResult = true;
                p_args.Result = UnityEngine.Random.Range(Convert.ToSingle(evalParams[0]), Convert.ToSingle(evalParams[1]));
                return true;
            }

            errorMessage = "Random function for type " + typeof(T) + " is not implemented.";
            return false;
        }
        
        private static bool RandomInt(FunctionArgs p_args)
        {
            object[] evalParams = p_args.EvaluateParameters();
            
            if (p_args.Parameters.Length != 2)
            {
                errorMessage = "Invalid number of parameters ("+p_args.Parameters.Length+") for RandomInt function";
                return false;
            }
            
            p_args.HasResult = true;
            p_args.Result = UnityEngine.Random.Range(Convert.ToInt32(evalParams[0]), Convert.ToInt32(evalParams[1]));
            return true;
        }
        
        private static bool RandomV3(FunctionArgs p_args)
        {
            if (p_args.Parameters.Length != 0 && p_args.Parameters.Length != 2 && p_args.Parameters.Length != 6)
            {
                errorMessage = "Invalid number of parameters in RandomV3 function "+p_args.Parameters.Length;
                return false;
            }
                
            object[] evalParams = p_args.EvaluateParameters();

            float random = 0;
            switch (evalParams.Length)
            {
                case 0:
                    p_args.HasResult = true;
                    random = UnityEngine.Random.Range(0, 1f);
                    p_args.Result = new Vector3(random, random, random);
                    return true;

                case 2:
                    p_args.HasResult = true;
                    random =
                        UnityEngine.Random.Range(Convert.ToSingle(evalParams[0]), Convert.ToSingle(evalParams[1]));
                    p_args.Result = new Vector3(random, random, random);
                    return true;
                case 6:
                    p_args.HasResult = true;
                    p_args.Result = new Vector3(
                        UnityEngine.Random.Range(Convert.ToSingle(evalParams[0]), Convert.ToSingle(evalParams[1])),
                        UnityEngine.Random.Range(Convert.ToSingle(evalParams[2]), Convert.ToSingle(evalParams[3])),
                        UnityEngine.Random.Range(Convert.ToSingle(evalParams[4]), Convert.ToSingle(evalParams[5])));
                    return true;
            }

            errorMessage = "Unknown error in function RandomV3";
            return false;
        }
        
        private static bool RandomV2(FunctionArgs p_args)
        {
            if (p_args.Parameters.Length != 0 && p_args.Parameters.Length != 2 && p_args.Parameters.Length != 4)
            {
                errorMessage = "Invalid number of parameters in RandomV2 function "+p_args.Parameters.Length;
                return false;
            }
                
            object[] evalParams = p_args.EvaluateParameters();

            switch (evalParams.Length)
            {
                case 0:
                    p_args.HasResult = true;
                    p_args.Result = new Vector2(UnityEngine.Random.Range(0, 1), UnityEngine.Random.Range(0, 1));
                    return true;

                case 2:
                    p_args.HasResult = true;
                    p_args.Result = new Vector2(
                        UnityEngine.Random.Range(Convert.ToSingle(evalParams[0]), Convert.ToSingle(evalParams[1])),
                        UnityEngine.Random.Range(Convert.ToSingle(evalParams[0]), Convert.ToSingle(evalParams[1])));
                    return true;
                case 6:
                    p_args.HasResult = true;
                    p_args.Result = new Vector2(
                        UnityEngine.Random.Range(Convert.ToSingle(evalParams[0]), Convert.ToSingle(evalParams[1])),
                        UnityEngine.Random.Range(Convert.ToSingle(evalParams[2]), Convert.ToSingle(evalParams[3])));
                    return true;
            }

            errorMessage = "Unknown error in function RandomV3";
            return false;
        }

        private static bool Ceil<T>(FunctionArgs p_args)
        {
            object[] evalParams = p_args.EvaluateParameters();
            if (evalParams.Length != 1)
            {
                errorMessage = "Invalid parameters for Ceil function.";
                return false;
            }
            
            if (typeof(T) == typeof(float) || typeof(T) == typeof(int) || typeof(T) == typeof(double))
            {
                if (p_args.Parameters.Length != 1)
                {
                    errorMessage = "Invalid parameters for Ceil function of type " + typeof(T);
                    return false;
                }
                
                p_args.HasResult = true;
                p_args.Result = Mathf.CeilToInt(Convert.ToSingle(evalParams[0]));
                return true;
            }

            errorMessage = "Ceil function for type " + typeof(T) + " is not implemented.";
            return false;
        }
        
        private static bool RandomF(FunctionArgs p_args)
        {
            object[] evalParams = p_args.EvaluateParameters();
            
            if (p_args.Parameters.Length != 2 && p_args.Parameters.Length != 3)
            {
                errorMessage = "Invalid parameters for RandomF function.";
                return false;
            }
            
            p_args.HasResult = true;
            p_args.Result = UnityEngine.Random.Range(Convert.ToSingle(evalParams[0]), Convert.ToSingle(evalParams[1]));

            return true;
        }
        
        private static bool Scale(FunctionArgs p_args)
        {
            object[] evalParams = p_args.EvaluateParameters();
            
            if (evalParams.Length != 2)
            {
                errorMessage = "Invalid parameters for Scale function.";
                return false;
            }

            if (evalParams[0] == null)
            {
                errorMessage = "Invalid parameters for Scale function. First parameter is null.";
                return false;
            }

            if (evalParams[0].GetType() == typeof(Vector2))
            {
                p_args.HasResult = true;
                p_args.Result = (Vector2)evalParams[0] * Convert.ToSingle(evalParams[1]);
                return true;
            } 
            
            if (evalParams[0].GetType() == typeof(Vector3))
            {
                p_args.HasResult = true;
                p_args.Result = (Vector3)evalParams[0] * Convert.ToSingle(evalParams[1]);
                return true;
            }

            errorMessage = "Scale function for type " + evalParams[0].GetType() + " is not implemented.";
            return false;
        }

        private static bool Get(FunctionArgs p_args)
        {
            object[] evalParams = p_args.EvaluateParameters();

            evalParams = p_args.EvaluateParameters();

            if (evalParams.Length != 2)
            {
                errorMessage = "Invalid parameters for Get function.";
                return false;
            }

            p_args.HasResult = true;
            p_args.Result = ((Array)evalParams[0]).GetValue((int)evalParams[1]);
            return true;
        } 
    }
}