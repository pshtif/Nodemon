/*
 *	Created by:  Peter @sHTiF Stefcek
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using OdinSerializer.Utilities;
using Nodemon.Attributes;
using UnityEngine;
using UniversalGUI;
using GUI = UnityEngine.GUI;
using Object = System.Object;

namespace Nodemon
{
    public class GUIProperties
    {
        static public Color PARAMETER_COLOR = new Color(0.4f,.8f,1f);

        public static bool supportReferencing = false;

        private static string _propertyReference;

        private static int labelWidth = 150;

        public static int fieldWidth = 0;

        // Hard cap on a single parameter row's width. Set by the inspector view
        // to (inspectorRect.width - vertical scrollbar - padding) so the outer
        // BeginHorizontal in ParameterProperty / SeedProperty can lock the
        // row to a known width and prevent any nested layout (e.g. ValueProperty's
        // inner BeginHorizontal for Float / Color / Boolean) from reporting a
        // preferred width that triggers the scrollview's horizontal scrollbar.
        // 0 means "no cap" — fall back to default ExpandWidth behavior.
        public static int rowWidth = 0;

        private static GUIStyle _parameterButtonStyle;
        private static GUIStyle ParameterButtonStyle
        {
            get
            {
                if (_parameterButtonStyle == null)
                {
                    _parameterButtonStyle = new GUIStyle();
                    _parameterButtonStyle.padding.top = 2;
                }

                return _parameterButtonStyle;
            }
        }

        // static public void Separator(int p_thickness, int p_paddingTop, int p_paddingBottom, Color p_color)
        // {
        //     #if UNITY_EDITOR
        //     Rect rect = UnityEditor.EditorGUILayout.GetControlRect(GUILayout.Height(p_paddingTop+p_paddingBottom+p_thickness));
        //     rect.height = p_thickness;
        //     rect.y+=p_paddingTop;
        //     rect.x-=2;
        //     rect.width +=6;
        //     UnityEditor.EditorGUI.DrawRect(rect, p_color);
        //     #else
        //     Debug.LogWarning("Separator not implemented for runtime.");
        //     #endif
        // }
        
        public static void Separator() {
            var lastRect = GUILayoutUtility.GetLastRect();
            GUILayout.Space(7);
            GUI.color = new Color(0, 0, 0, 0.3f);
            GUI.DrawTexture(Rect.MinMaxRect(lastRect.xMin, lastRect.yMax + 4, lastRect.xMax, lastRect.yMax + 6), Texture2D.whiteTexture);
            GUI.color = Color.white;
        }
        
        static public bool PropertyField(string p_name, Object p_object, BindingFlags p_bindingFlags = BindingFlags.Default)
        {
            var field = p_object.GetType().GetField(p_name, p_bindingFlags);
            return PropertyField(field, p_object, null);
        }
        
        static public bool PropertyField(FieldInfo p_fieldInfo, Object p_fieldObject, IReferencable p_reference, FieldInfo p_parentInfo = null, Object p_parentObject = null, IExposedPropertyTable p_propertyTable = null)
        {
            if (!IsValid(p_fieldInfo))
                return false;

            if (IsHidden(p_fieldInfo))
                return false;
            
            if (!MeetsDependencies(p_fieldInfo, p_fieldObject))
                return false;
            
            var label = GetFieldName(p_parentInfo != null ? p_parentInfo : p_fieldInfo);
            
            if (IsSeedProperty(p_fieldInfo))
                return SeedProperty(label, p_fieldInfo, p_fieldObject, p_reference);
                
            if (IsParameterProperty(p_fieldInfo))
                return ParameterProperty(label, p_fieldInfo, p_fieldObject, p_reference);
            
            if (p_fieldInfo.FieldType == typeof(Type))
            {
                return SupportedTypeProperty(label, p_fieldInfo, p_fieldObject);
            }

            if (IsEnumProperty(p_fieldInfo))
                return EnumProperty(label, p_fieldInfo, p_fieldObject);
            
            if (IsUnityObjectProperty(p_fieldInfo))
                return UnityObjectProperty(label, p_fieldInfo, p_fieldObject);

            if (IsExposedReferenceProperty(p_fieldInfo))
                return ExposedReferenceProperty(label, p_fieldInfo, p_fieldObject, p_reference, p_propertyTable);
            
            if (IsEditAsStringProperty(p_fieldInfo, p_parentInfo))
                return EditAsStringProperty(p_fieldInfo, p_fieldObject, label, p_reference, p_parentInfo, p_parentObject);
            
            return ValueProperty(label, p_fieldInfo, p_fieldObject, p_reference, p_parentInfo);
        }

        private static bool IsValid(FieldInfo p_fieldInfo)
        {
            if (p_fieldInfo == null)
            {
                Debug.LogWarning("Field info cannot be null!");
                return false;
            }

            return true;
        }

        private static GUIContent GetFieldName(FieldInfo p_nameInfo)
        {
            string nameString = UniGUI.NicifyString(p_nameInfo.Name);//ObjectNames.NicifyVariableName(p_nameInfo.Name);
            nameString = nameString.Substring(0, 1).ToUpper() + nameString.Substring(1);
            
            TooltipAttribute tooltipAttribute = p_nameInfo.GetCustomAttribute<TooltipAttribute>();
            return tooltipAttribute == null ? new GUIContent(nameString) : new GUIContent(nameString, tooltipAttribute.tooltip);
        }
        
        static bool IsEditAsStringProperty(FieldInfo p_fieldInfo, FieldInfo p_parentInfo)
        {
            EditAsStringAttribute editAsStringAttribute = p_parentInfo == null
                ? p_fieldInfo.GetCustomAttribute<EditAsStringAttribute>()
                : p_parentInfo.GetCustomAttribute<EditAsStringAttribute>();
            return editAsStringAttribute != null;
        }

        private static string editAsStringCache; 
        
        static bool EditAsStringProperty(FieldInfo p_fieldInfo, Object p_fieldObject, GUIContent p_name,
            IReferencable p_reference, FieldInfo p_parameterInfo = null, Object p_parameterObject = null)
        {
            FieldInfo referenceInfo = p_parameterInfo != null ? p_parameterInfo : p_fieldInfo;
            EditAsStringAttribute editAsStringAttribute = p_parameterInfo == null
                ? p_fieldInfo.GetCustomAttribute<EditAsStringAttribute>()
                : p_parameterInfo.GetCustomAttribute<EditAsStringAttribute>();
            
            var readMethodInfo = p_parameterObject == null
                ? p_fieldObject.GetType().GetMethod(editAsStringAttribute.ReadMethodName,
                    BindingFlags.Instance | BindingFlags.NonPublic)
                : p_parameterObject.GetType().GetMethod(editAsStringAttribute.ReadMethodName,
                    BindingFlags.Instance | BindingFlags.NonPublic);

            var writeMethodInfo = p_parameterObject == null
                ? p_fieldObject.GetType().GetMethod(editAsStringAttribute.WriteMethodName,
                    BindingFlags.Instance | BindingFlags.NonPublic)
                : p_parameterObject.GetType().GetMethod(editAsStringAttribute.WriteMethodName,
                    BindingFlags.Instance | BindingFlags.NonPublic);
            
            if (readMethodInfo == null || writeMethodInfo == null)
            {
                Debug.LogError("Invalid methods for EditAsString attribute ("+editAsStringAttribute.ReadMethodName+","+editAsStringAttribute.WriteMethodName+")");
            }
            
            UniGUI.BeginChangeCheck();
            GUILayout.BeginHorizontal();
            GUILayout.Label(p_name, GUILayout.Width(160));
            
            HandleReferencing(p_reference, referenceInfo, false, p_parameterInfo == null ? null : (Parameter)p_fieldObject);
            
            if (GUI.GetNameOfFocusedControl() != p_fieldInfo.Name)
            {
                editAsStringCache = (string)readMethodInfo.Invoke(
                    p_parameterObject == null ? p_fieldObject : p_parameterObject,
                    new object[0]);
            }
            
            GUI.SetNextControlName(p_fieldInfo.Name);
            editAsStringCache = GUILayout.TextArea(editAsStringCache);
            
            GUILayout.EndHorizontal();

            if (UniGUI.EndChangeCheck())
            {
                writeMethodInfo.Invoke(p_parameterObject == null ? p_fieldObject : p_parameterObject,
                    new object[] { editAsStringCache });
                return true;
            }

            return false;
        }
        
        public static bool IsSeedProperty(FieldInfo p_fieldInfo)
        {
            var seedAttribute = p_fieldInfo.GetCustomAttribute<SeedAttribute>();
            return seedAttribute != null;
        }

        static bool SeedProperty(GUIContent p_label, FieldInfo p_fieldInfo, Object p_fieldObject, IReferencable p_reference)
        {
            bool invalidate = false;
            var seedAttribute = p_fieldInfo.GetCustomAttribute<SeedAttribute>();

            if (IsParameterProperty(p_fieldInfo))
            {
                // Shrink fieldWidth for the duration of this row so the extra
                // retry icon fits without overflow. Restore right after — every
                // other row in the inspector keeps the default 190.
                int savedFieldWidth = fieldWidth;
                fieldWidth = savedFieldWidth - 40;

                // Use ParameterProperty's row layout and inject the retry icon
                // as a trailing extra. The menu icon stays the rightmost element.
                ParameterProperty(p_label, p_fieldInfo, p_fieldObject, p_reference, () =>
                {
                    if (GUILayout.Button(TextureUtils.GetTexture("Icons/retry_icon"), ParameterButtonStyle, GUILayout.Height(18), GUILayout.Width(18)))
                    {
                        var random = new System.Random();
                        if (p_fieldInfo.FieldType == typeof(Parameter<int>))
                        {
                            Parameter<int> intParameter = (Parameter<int>)p_fieldInfo.GetValue(p_fieldObject);
                            intParameter.value = random.Next((int)seedAttribute.minValue, (int)seedAttribute.maxValue);
                        } else if (p_fieldInfo.FieldType == typeof(Parameter<float>))
                        {
                            Parameter<float> floatParameter = (Parameter<float>)p_fieldInfo.GetValue(p_fieldObject);
                            floatParameter.value =
                                (float)random.NextDouble() * (seedAttribute.maxValue - seedAttribute.minValue) +
                                seedAttribute.minValue;
                        } else if (p_fieldInfo.FieldType == typeof(Parameter<double>))
                        {
                            Parameter<double> doubleParameter = (Parameter<double>)p_fieldInfo.GetValue(p_fieldObject);
                            doubleParameter.value =
                                random.NextDouble() * (seedAttribute.maxValue - seedAttribute.minValue) +
                                seedAttribute.minValue;
                        }
                        invalidate = true;
                    }
                });

                fieldWidth = savedFieldWidth;
            }
            else
            {
                if (rowWidth > 0)
                    GUILayout.BeginHorizontal(GUILayout.Width(rowWidth));
                else
                    GUILayout.BeginHorizontal();
                ValueProperty(p_label, p_fieldInfo, p_fieldObject, p_reference);
                GUILayout.FlexibleSpace();

                if (GUILayout.Button(TextureUtils.GetTexture("Icons/retry_icon"), ParameterButtonStyle, GUILayout.Height(18), GUILayout.Width(18)))
                {
                    var random = new System.Random();
                    if (p_fieldInfo.FieldType == typeof(int))
                    {
                        p_fieldInfo.SetValue(p_fieldObject,
                            random.Next((int)seedAttribute.minValue, (int)seedAttribute.maxValue));
                    } else if (p_fieldInfo.FieldType == typeof(float))
                    {
                        p_fieldInfo.SetValue(p_fieldObject,
                            (float)random.NextDouble() * (seedAttribute.maxValue - seedAttribute.minValue) +
                            seedAttribute.minValue);
                    } else if (p_fieldInfo.FieldType == typeof(double))
                    {
                        p_fieldInfo.SetValue(p_fieldObject,
                            random.NextDouble() * (seedAttribute.maxValue - seedAttribute.minValue) +
                            seedAttribute.minValue);
                    }
                    invalidate = true;
                }
                GUILayout.EndHorizontal();
            }

            return invalidate;
        } 
        
        public static bool IsParameterProperty(FieldInfo p_fieldInfo)
        {
            return typeof(Parameter).IsAssignableFrom(p_fieldInfo.FieldType);
        }
        
        static bool ParameterProperty(GUIContent p_label, FieldInfo p_fieldInfo, Object p_fieldObject, IReferencable p_reference)
        {
            return ParameterProperty(p_label, p_fieldInfo, p_fieldObject, p_reference, null);
        }

        /// <summary>
        /// Shared row layout for any Parameter field. <paramref name="drawTrailingExtras"/>
        /// runs after FlexibleSpace and before the parameter-menu icon, so callers
        /// (e.g. SeedProperty's retry icon) can stack icons that sit just left of
        /// the menu icon while the menu icon stays at the row's right edge. Pass
        /// null for the normal one-icon row.
        /// </summary>
        static bool ParameterProperty(GUIContent p_label, FieldInfo p_fieldInfo, Object p_fieldObject, IReferencable p_reference, System.Action drawTrailingExtras)
        {
            Parameter parameter = (Parameter)p_fieldInfo.GetValue(p_fieldObject);

            if (parameter == null)
            {
                RecreateParameter(p_fieldInfo, p_fieldObject);
                return true;
            }

            UniGUI.BeginChangeCheck();

            if (rowWidth > 0)
                GUILayout.BeginHorizontal(GUILayout.Width(rowWidth));
            else
                GUILayout.BeginHorizontal();

            if (parameter.isExpression)
            {
                GUI.color = PARAMETER_COLOR;
                UniGUILayout.Label(p_label, GUILayout.Width(labelWidth));
                HandleReferencing(p_reference, p_fieldInfo, false, parameter);
                // Fixed width keeps long expressions from blowing out the row
                // and forcing horizontal scroll on the whole inspector. The
                // TextField scrolls its own content internally.
                parameter.expression = UniGUILayout.TextField(parameter.expression,
                    GUILayout.Width(fieldWidth > 0 ? fieldWidth : 190));
                GUI.color = Color.white;
            }
            else
            {
                PropertyField(parameter.GetValueFieldInfo(), parameter, p_reference, p_fieldInfo, p_fieldObject);
            }

            // FlexibleSpace pushes the trailing icons to the right edge of the
            // row. The menu icon is always last; any extras (Seed's retry
            // icon, etc.) sit between FlexibleSpace and the menu icon.
            GUILayout.FlexibleSpace();

            drawTrailingExtras?.Invoke();

            GUI.color = parameter.isExpression ? PARAMETER_COLOR : Color.gray;
            if (GUILayout.Button(TextureUtils.GetTexture("Icons/parameter_icon"), ParameterButtonStyle, GUILayout.Height(18), GUILayout.Width(18)))
            {
                var menu = ParameterMenu.Get(parameter, p_fieldInfo.Name, p_fieldObject);
                UniGUIGenericMenuPopup.Show(menu, "", Event.current.mousePosition, 240, 300, false, false);
            }
            GUI.color = Color.white;
            GUILayout.Space(1);
            GUILayout.EndHorizontal();

            GUILayout.Space(4);

            return UniGUI.EndChangeCheck();
        }
        
        static void RecreateParameter(FieldInfo p_fieldInfo, Object p_fieldObject)
        {
            var genericType = p_fieldInfo.FieldType.GenericTypeArguments[0];
            var parameterType = typeof(Parameter<>).MakeGenericType(genericType);
            var parameter = Activator.CreateInstance(parameterType, genericType.GetDefaultValue());

            p_fieldInfo.SetValue(p_fieldObject, parameter);
        }

        static public bool IsHidden(FieldInfo p_fieldInfo)
        {
            HideInInspector hideInInspectorAttribute = p_fieldInfo.GetCustomAttribute<HideInInspector>();
            return hideInInspectorAttribute != null;
        }
        
        static public bool MeetsDependencies(FieldInfo p_fieldInfo, Object p_fieldObject)
        {
            IEnumerable<DependencyAttribute> attributes = p_fieldInfo.GetCustomAttributes<DependencyAttribute>();
            foreach (DependencyAttribute attribute in attributes)
            {
                FieldInfo dependencyField = p_fieldObject.GetType().GetField(attribute.DependencyName);
                if (dependencyField != null && dependencyField.GetValue(p_fieldObject) != null && attribute.Value.ToString() != dependencyField.GetValue(p_fieldObject).ToString())
                    return false;
            }

            bool single = false;
            IEnumerable<DependencySingleAttribute> singleAttributes = p_fieldInfo.GetCustomAttributes<DependencySingleAttribute>();
            foreach (DependencySingleAttribute attribute in singleAttributes)
            {
                FieldInfo dependencyField = p_fieldObject.GetType().GetField(attribute.DependencyName);
                if (dependencyField != null)
                {
                    if (attribute.Value.ToString() == dependencyField.GetValue(p_fieldObject).ToString())
                    {
                        single = true;
                        break;
                    }
                }
            }

            if (!single && singleAttributes.Count() > 0)
                return false;

            return true;
        }
        
        static bool SupportedTypeProperty(GUIContent p_label, FieldInfo p_fieldInfo, Object p_fieldObject)
        {
            Type value = (Type)p_fieldInfo.GetValue(p_fieldObject);

            UniGUI.BeginChangeCheck();
            
            GUILayout.BeginHorizontal();
            
            GUILayout.Label(p_label, GUILayout.Width(labelWidth));

            var mousePosition = Event.current.mousePosition;

            var types = SupportedTypes.GetSupportedTypes();
            var names = SupportedTypes.GetSupportedTypeNames();
            var index = Array.IndexOf(types, value);
            
            var newIndex = UniGUILayout.Popup(GUIContent.none, index, names);
            
            // if (GUILayout.Button(value == null ? "NONE" : value.Name))
            // {
            //     SupportedTypes.Show((type) =>
            //     {
            //         if (type != value)
            //         {
            //             p_fieldInfo.SetValue(p_fieldObject, type);
            //         }    
            //     }, mousePosition);
            //     
            //     return true;
            // }

            GUILayout.EndHorizontal();
            
            if (UniGUI.EndChangeCheck())
            {
                p_fieldInfo.SetValue(p_fieldObject, types[newIndex]);
                return true;
            }

            return false;
        }

        static bool IsEnumProperty(FieldInfo p_fieldInfo)
        {
            return p_fieldInfo.FieldType.IsEnum;
        }

        static bool EnumProperty(GUIContent p_label, FieldInfo p_fieldInfo, Object p_fieldObject)
        {
            UniGUI.BeginChangeCheck();

            // Outer BeginHorizontal constrained to labelWidth + fieldWidth so the
            // EnumPopup button doesn't expand to fill the inspector and shift the
            // trailing parameter icon. Label takes labelWidth, EnumPopup gets the
            // remaining fieldWidth.
            GUILayout.BeginHorizontal(GUILayout.Width(labelWidth + (fieldWidth > 0 ? fieldWidth : 190)));
            GUILayout.Label(p_label, GUILayout.Width(labelWidth));
            var newValue = UniGUILayout.EnumPopup((Enum) p_fieldInfo.GetValue(p_fieldObject));
            GUILayout.EndHorizontal();

            if (UniGUI.EndChangeCheck())
            {
                p_fieldInfo.SetValue(p_fieldObject, newValue);
                return true;
            }

            return false;
        }
        
        static bool IsUnityObjectProperty(FieldInfo p_fieldInfo)
        {
            return typeof(UnityEngine.Object).IsAssignableFrom(p_fieldInfo.FieldType);
        }
        
        static bool UnityObjectProperty(GUIContent p_label, FieldInfo p_fieldInfo, Object p_fieldObject)
        {
            UniGUI.BeginChangeCheck();
            
            GUILayout.BeginHorizontal();
            GUILayout.Label(p_label, GUILayout.Width(labelWidth));
            
            var newValue = UniGUILayout.ObjectField((UnityEngine.Object) p_fieldInfo.GetValue(p_fieldObject),
                p_fieldInfo.FieldType, false);

            GUILayout.EndHorizontal();
            
            if (UniGUI.EndChangeCheck())
            {
                p_fieldInfo.SetValue(p_fieldObject, newValue);
                return true;
            }

            return false;
        }
        
        static bool IsExposedReferenceProperty(FieldInfo p_fieldInfo)
        {
            return p_fieldInfo.FieldType.IsGenericType &&
                   p_fieldInfo.FieldType.GetGenericTypeDefinition() == typeof(ExposedReference<>);
        }
        
        static bool ExposedReferenceProperty(GUIContent p_label, FieldInfo p_fieldInfo, Object p_fieldObject, IReferencable p_reference, IExposedPropertyTable p_propertyTable)
        {
            #if UNITY_EDITOR
            var exposedReference = p_fieldInfo.GetValue(p_fieldObject);
            
            PropertyName exposedName = (PropertyName)exposedReference.GetType().GetField("exposedName").GetValue(exposedReference);
            bool isDefault = PropertyName.IsNullOrEmpty(exposedName);
            
            GUILayout.BeginHorizontal();
            GUILayout.Label(p_label, GUILayout.Width(labelWidth));
            HandleReferencing(p_reference, p_fieldInfo);
            //EditorGUI.BeginChangeCheck();
            
            UnityEngine.Object exposedValue = (UnityEngine.Object)exposedReference.GetType().GetMethod("Resolve")
                .Invoke(exposedReference, new object[] {p_propertyTable});
            var newValue = UnityEditor.EditorGUILayout.ObjectField(exposedValue, p_fieldInfo.FieldType.GetGenericArguments()[0], true);
            
            GUILayout.EndHorizontal();
            
            if (UnityEditor.EditorGUI.EndChangeCheck())
            {
                if (p_propertyTable != null)
                {
                    UnityEditor.Undo.RegisterCompleteObjectUndo(p_propertyTable as UnityEngine.Object, "Set Exposed Property");
                }
            
                if (!isDefault)
                {
                    if (newValue == null)
                    {
                        p_propertyTable.ClearReferenceValue(exposedName);   
                        exposedReference.GetType().GetField("exposedName").SetValue(exposedReference, null);
                        p_fieldInfo.SetValue(p_fieldObject, exposedReference);
                    }
                    else
                    {
                        p_propertyTable.SetReferenceValue(exposedName, newValue);
                    }
                }
                else
                {
                    if (newValue != null)
                    {
                        PropertyName newExposedName = new PropertyName(UnityEngine.GUID.Generate().ToString());
                        exposedReference.GetType().GetField("exposedName")
                            .SetValue(exposedReference, newExposedName);
                        p_propertyTable.SetReferenceValue(newExposedName, newValue);
                        p_fieldInfo.SetValue(p_fieldObject, exposedReference);
                    }
                }
            
                return true;
            }
            #else
            GUILayout.Label("Exposed reference field not supported at runtime.");
            #endif

            return false;
        }
        
        protected static void HandleReferencing(IReferencable p_reference, FieldInfo p_fieldInfo, bool p_directExpression = false, Parameter p_parameter = null)
        {
            if (!supportReferencing)
                return;
            
            if (GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition) &&
                Event.current.button == 1 && Event.current.type == EventType.MouseDown)
            {
                UniGUIGenericMenu menu = new UniGUIGenericMenu();
                
                menu.AddItem(new GUIContent("Copy reference"), false,
                    () =>
                    {
                        _propertyReference = "[$" + p_reference.Id + "." + p_fieldInfo.Name + "]";
                    });
                
                if (p_parameter != null && !string.IsNullOrEmpty(_propertyReference))
                {
                    menu.AddItem(new GUIContent("Paste reference"), false,
                        () =>
                        {
                            p_parameter.isExpression = true;
                            p_parameter.expression = _propertyReference;
                        });
                }
                
                if (p_directExpression && !string.IsNullOrEmpty(_propertyReference))
                {
                    menu.AddItem(new GUIContent("Paste reference"), false,
                        () => { p_fieldInfo.SetValue(p_reference, _propertyReference); });
                }
                
                UniGUIGenericMenuPopup.Show(menu, "",  Event.current.mousePosition, 240, 300, false, false);
            }
        }
        
        static bool ValueProperty(GUIContent p_label, FieldInfo p_fieldInfo, Object p_fieldObject, IReferencable p_reference, FieldInfo p_parameterInfo = null, Object p_parentObject = null)
        {
            FieldInfo referenceInfo = p_parameterInfo != null ? p_parameterInfo : p_fieldInfo;
            RangeAttribute range = p_parameterInfo == null
                ? p_fieldInfo.GetAttribute<RangeAttribute>()
                : p_parameterInfo.GetAttribute<RangeAttribute>();

            string type = p_fieldInfo.FieldType.ToString();
            switch (type)
            {
                case "System.String":
                {
                    // Multiline-attributed strings get a TextArea instead of the
                    // default TextField. Kept inside its own brace block so the
                    // local vars don't leak into the switch's shared scope.
                    var multiline = (p_parameterInfo == null ? p_fieldInfo : p_parameterInfo)
                        .GetAttribute<Nodemon.Attributes.MultilineAttribute>();
                    if (multiline != null)
                    {
                        UniGUI.BeginChangeCheck();
                        UniGUILayout.Label(p_label);
                        var newText = GUILayout.TextArea(
                            (string)p_fieldInfo.GetValue(p_fieldObject),
                            GUILayout.Height(multiline.lines * 16f));
                        if (UniGUI.EndChangeCheck())
                        {
                            p_fieldInfo.SetValue(p_fieldObject, newText);
                            return true;
                        }
                        return false;
                    }

                    UniGUI.BeginChangeCheck();

                    GUILayout.BeginHorizontal();
                    UniGUILayout.Label(p_label, GUILayout.Width(labelWidth));
                    HandleReferencing(p_reference, referenceInfo, false, p_parameterInfo == null ? null : (Parameter)p_fieldObject);
                    // Fixed width so a long string doesn't expand the row past
                    // the inspector's width. TextField scrolls internally.
                    var stringValue = UniGUILayout.TextField((String) p_fieldInfo.GetValue(p_fieldObject),
                        GUILayout.Width(fieldWidth > 0 ? fieldWidth : 190));
                    GUILayout.EndHorizontal();

                    if (UniGUI.EndChangeCheck())
                    {
                        p_fieldInfo.SetValue(p_fieldObject, stringValue);
                        return true;
                    }
                    return false;
                }
                
                case "System.Int32":
                    UniGUI.BeginChangeCheck();
                    
                    if (p_parentObject == null) GUILayout.BeginHorizontal();
                    
                    UniGUILayout.Label(p_label,  GUILayout.Width(labelWidth));
                    HandleReferencing(p_reference, referenceInfo, false, p_parameterInfo == null ? null : (Parameter)p_fieldObject);

                    int intValue = 0;

                    if (range == null)
                    {
                        intValue = UniGUILayout.IntField((int) p_fieldInfo.GetValue(p_fieldObject),
                            GUILayout.Width(fieldWidth > 0 ? fieldWidth : 190));
                    }
                    else
                    {
                        // intValue = EditorGUILayout.IntSlider((int) p_fieldInfo.GetValue(p_fieldObject), (int)range.min,
                        //     (int)range.max);
                    }

                    if (p_parentObject == null) GUILayout.EndHorizontal();

                    if (UniGUI.EndChangeCheck())
                    {
                         p_fieldInfo.SetValue(p_fieldObject, intValue);
                         return true;
                    }
                    return false;
                
                case "System.Byte":
                    UniGUI.BeginChangeCheck();
                    
                    if (p_parentObject == null) GUILayout.BeginHorizontal();
                    
                    UniGUILayout.Label(p_label,  GUILayout.Width(labelWidth));
                    HandleReferencing(p_reference, referenceInfo, false, p_parameterInfo == null ? null : (Parameter)p_fieldObject);

                    byte byteValue = 0;

                    if (range == null)
                    {
                        byteValue = (byte)UniGUILayout.IntField((byte) p_fieldInfo.GetValue(p_fieldObject),
                            GUILayout.Width(fieldWidth > 0 ? fieldWidth : 190));
                    }
                    else
                    {
                        // byteValue = (byte)EditorGUILayout.IntSlider((int) p_fieldInfo.GetValue(p_fieldObject), (int)range.min,
                        //     (int)range.max);
                    }

                    if (p_parentObject == null) GUILayout.EndHorizontal();

                    if (UniGUI.EndChangeCheck())
                    {
                        p_fieldInfo.SetValue(p_fieldObject, byteValue);
                        return true;
                    }
                    return false;
                
                case "System.Single":
                    UniGUI.BeginChangeCheck();
                    
                    GUILayout.BeginHorizontal();
                    UniGUILayout.Label(p_label,  GUILayout.Width(labelWidth));
                    HandleReferencing(p_reference, referenceInfo, false, p_parameterInfo == null ? null : (Parameter)p_fieldObject);

                    float singleValue = 0;
                    if (range == null)
                    {
                        singleValue = UniGUILayout.FloatField((float) p_fieldInfo.GetValue(p_fieldObject),
                            GUILayout.Width(fieldWidth > 0 ? fieldWidth : 190));
                    }
                    else
                    {
                        // singleValue = EditorGUILayout.Slider((float) p_fieldInfo.GetValue(p_fieldObject), range.min,
                        //     range.max);
                    }
                    
                    GUILayout.EndHorizontal();

                    if (UniGUI.EndChangeCheck())
                    {
                        p_fieldInfo.SetValue(p_fieldObject, singleValue);
                        return true;
                    }
                    return false;
                
                case "System.Boolean":
                    GUILayout.BeginHorizontal();
                    UniGUILayout.Label(p_label, GUILayout.Width(labelWidth));
                    HandleReferencing(p_reference, referenceInfo, false, p_parameterInfo == null ? null : (Parameter)p_fieldObject);
                    bool originalValue = (bool)p_fieldInfo.GetValue(p_fieldObject); 
                    var newValue = UniGUILayout.Toggle("", originalValue);
                    GUILayout.EndHorizontal();

                    if (newValue != originalValue)
                    {
                        p_fieldInfo.SetValue(p_fieldObject, newValue);
                        return true;
                    }

                    return false;
                
                case "UnityEngine.Vector2":
                    UniGUI.BeginChangeCheck();

                    HandleReferencing(p_reference, referenceInfo, false, p_parameterInfo == null ? null : (Parameter)p_fieldObject);
                    // Constrain the Vector2Field's outer BeginHorizontal to
                    // labelWidth + fieldWidth so the row matches the size of
                    // single-line parameter fields and the trailing icon stays
                    // pinned at the same x position across modes.
                    var vector2Value = UniGUILayout.Vector2Field(p_label, (Vector2) p_fieldInfo.GetValue(p_fieldObject),
                        GUILayout.Width(labelWidth + (fieldWidth > 0 ? fieldWidth : 190)));

                    if (UniGUI.EndChangeCheck())
                    {
                        p_fieldInfo.SetValue(p_fieldObject, vector2Value);
                        return true;
                    }
                    return false;

                case "UnityEngine.Vector3":
                    UniGUI.BeginChangeCheck();

                    HandleReferencing(p_reference, referenceInfo, false, p_parameterInfo == null ? null : (Parameter)p_fieldObject);
                    var vector3Value = UniGUILayout.Vector3Field(p_label, (Vector3) p_fieldInfo.GetValue(p_fieldObject),
                        fieldWidth > 0 ? fieldWidth : 190,
                        GUILayout.Width(labelWidth + (fieldWidth > 0 ? fieldWidth : 190)));

                    if (UniGUI.EndChangeCheck())
                    {
                        p_fieldInfo.SetValue(p_fieldObject, vector3Value);
                        return true;
                    }
                    return false;

                case "UnityEngine.Vector4":
                    UniGUI.BeginChangeCheck();

                    HandleReferencing(p_reference, referenceInfo, false, p_parameterInfo == null ? null : (Parameter)p_fieldObject);
                    var vector4Value = UniGUILayout.Vector4Field(p_label, (Vector4) p_fieldInfo.GetValue(p_fieldObject),
                        GUILayout.Width(labelWidth + (fieldWidth > 0 ? fieldWidth : 190)));

                    if (UniGUI.EndChangeCheck())
                    {
                        p_fieldInfo.SetValue(p_fieldObject, vector4Value);
                        return true;
                    }
                    return false;

                case "UnityEngine.Color":
                    UniGUI.BeginChangeCheck();

                    GUILayout.BeginHorizontal();
                    GUILayout.Label(p_label, GUILayout.Width(labelWidth));
                    HandleReferencing(p_reference, referenceInfo, false, p_parameterInfo == null ? null : (Parameter)p_fieldObject);
                    var colorValue = UniGUILayout.ColorField((Color) p_fieldInfo.GetValue(p_fieldObject),
                        GUILayout.Width(fieldWidth > 0 ? fieldWidth : 190));
                    GUILayout.EndHorizontal();
                    
                    if (UniGUI.EndChangeCheck())
                    {
                        p_fieldInfo.SetValue(p_fieldObject, colorValue);
                        return true;
                    }
                    return false;
                
                default:
                    Debug.Log(type + " type inspection not implemented. Field: " + p_fieldInfo.Name);
                    return false;
            }
        }
    }
}