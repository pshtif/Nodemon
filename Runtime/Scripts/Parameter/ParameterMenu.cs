/*
 *	Created by:  Peter @sHTiF Stefcek
 */

using System;
using System.Reflection;
using UnityEngine;
using UniversalGUI;
using Object = UnityEngine.Object;

namespace Nodemon
{
    public class ParameterMenu
    {
        public static UniGUIGenericMenu Get(Parameter p_parameter, string p_name, object p_object)
        {
            UniGUIGenericMenu menu = new UniGUIGenericMenu();

            if (p_parameter.isExpression)
            {
                menu.AddItem(new GUIContent("Value"), false, () =>
                {
                    p_parameter.isExpression = false;
                    p_parameter.expression = "";
                });   
            }
            else
            {
                menu.AddItem(new GUIContent("Expression"), false, () =>
                {
                    p_parameter.SetValueToDefault();
                    p_parameter.isExpression = true;
                });
            }

            // Type type = p_parameter.GetValueType();
            // Variable[] variables = p_variables.GetAllVariablesOfType(type);
            //
            // foreach (var variable in variables)
            // {
            //     menu.AddItem(new GUIContent("Variable/"+variable.Name), false, () =>
            //     {
            //         p_parameter.isExpression = true;
            //         p_parameter.expression = variable.Name;
            //     });
            // }
            //
            // menu.AddItem(new GUIContent("Promote to Variable"), false, () =>
            // {
            //     p_parameter.isExpression = true;
            //     var variable = p_variables.AddNewVariable(type);
            //     p_parameter.expression = variable.Name;
            // });

            if (p_parameter.isExpression)
            {
                menu.AddItem(new GUIContent("Expression Editor"), false, () =>
                {
                    ExpressionEditorWindow.InitExpressionEditorWindow(p_parameter);
                });

                // menu.AddSeparator("");
                
                // if (p_parameter.IsDebug())
                // {
                //     menu.AddItem(new GUIContent("Disable Debug"), false,
                //         () => { p_parameter.SetDebug(false); });
                // }
                // else
                // {
                //     NodeModelBase model = p_object as NodeModelBase;
                //     menu.AddItem(new GUIContent("Enable Debug"), false, () => { p_parameter.SetDebug(true, p_name, model?.id); });
                // }
            }

            return menu;
        }
        
        public static UniGUIGenericMenu Get(FieldInfo p_useExpressionField, FieldInfo p_expressionField, FieldInfo p_directField, Object p_object, Type p_type)
        {
            UniGUIGenericMenu menu = new UniGUIGenericMenu();

            if ((bool)p_useExpressionField.GetValue(p_object))
            {
                menu.AddItem(new GUIContent("Direct Value"), false, () =>
                {
                    p_expressionField.SetValue(p_object, "");
                    p_useExpressionField.SetValue(p_object, false);
                });   
            }
            else
            {
                menu.AddItem(new GUIContent("Custom Expression"), false, () =>
                {
                    p_directField.SetValue(p_object, p_type.GetDefaultValue());
                    p_useExpressionField.SetValue(p_object, true);
                });
            }
            
            // Variable[] variables = p_variables.GetAllVariablesOfType(p_type);
            //
            // foreach (var variable in variables)
            // {
            //     menu.AddItem(new GUIContent("Variable/"+variable.Name), false, () =>
            //     {
            //         p_useExpressionField.SetValue(p_object, true);
            //         p_expressionField.SetValue(p_object, variable.Name);
            //     });
            // }
            //
            // menu.AddItem(new GUIContent("Promote to Variable"), false, () =>
            // {
            //     p_useExpressionField.SetValue(p_object, true);
            //     var variable = p_variables.AddNewVariable(p_type);
            //     p_expressionField.SetValue(p_object, variable.Name);
            // });

            return menu;
        }
    }
}