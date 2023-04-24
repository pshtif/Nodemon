/*
 *	Created by:  Peter @sHTiF Stefcek
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Nodemon.Attributes;
using UnityEngine;
using UniversalGUI;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;
using SerializationUtility = OdinSerializer.SerializationUtility;

namespace Nodemon
{
    public abstract class NodeModelBase : IReferencable
    {
        public string Id => id;
        
        [TitledGroup("Advanced", 1000, true)]
        public string id;

        [TitledGroup("Advanced", 1000, true)]
        public bool cacheOutput;
        
        [TitledGroup("Advanced", 1000, true)]
        public bool bypass = false;
        
        [TitledGroup("Advanced", 1000, true)]
        public bool template = false;

        [TitledGroup("Advanced", 1000, true)]
        public string comment;

        [Seed]
        [TitledGroup("Advanced", 1000, true)]
        public Parameter<int> seed;
        
        private int groupsMinized = -1;
        
        public NodeModelBase Clone(IExposedPropertyTable p_controller)
        {
            // Doing a shallow copy
            var clone = (NodeModelBase)SerializationUtility.CreateCopy(this);

            // Exposed references are not copied in serialization as they are external Unity references so they will refer to the same exposed reference instance not just the unity object reference, we need to copy them additionally
            FieldInfo[] fields = clone.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public);
            fields.ToList().FindAll(f => f.FieldType.IsGenericType && f.FieldType.GetGenericTypeDefinition() == typeof(ExposedReference<>)).ForEach(f =>
            {
                Type exposedRefType = typeof(ExposedReference<>).MakeGenericType(f.FieldType.GenericTypeArguments[0]);

                if (p_controller != null)
                {
                    var curExposedRef = f.GetValue(clone);
                    Object exposedValue = (Object) curExposedRef.GetType().GetMethod("Resolve")
                        .Invoke(curExposedRef, new object[] {p_controller});

                    var clonedExposedRef = Activator.CreateInstance(exposedRefType);
                    PropertyName newExposedName = new PropertyName(Random.Range(1000000000, 9999999999).ToString());
                    p_controller.SetReferenceValue(newExposedName, exposedValue);
                    clonedExposedRef.GetType().GetField("exposedName")
                        .SetValue(clonedExposedRef, newExposedName);
                    f.SetValue(clone, clonedExposedRef);
                }
            });

            return clone;
        }
        
        public virtual bool DrawInspector(IViewOwner p_owner)
        {
            bool initializeMinimization = false;
            if (groupsMinized == -1)
            {
                initializeMinimization = true;
                groupsMinized = 0;
            }
            
            GUILayout.Space(2);
            
            GUIStyle minStyle = GUIStyle.none;
            minStyle.normal.textColor = Color.white;
            minStyle.fontSize = 16;
            
            bool invalidate = false;
            if (initializeMinimization && (groupsMinized & 1) == 0)
            {
                groupsMinized += 1;
            }

            var isMinimized = (groupsMinized & 1) != 0;
            if (HasCustomInspector())
            {
                UniGUIUtils.DrawMinimizableTitle("  " + GetCustomInspectorName(), ref isMinimized, 13);

                if (isMinimized != ((groupsMinized & 1) != 0))
                {
                    groupsMinized = (groupsMinized & 1) == 0
                        ? groupsMinized + 1
                        : groupsMinized - 1;
                }

                if (!isMinimized)
                {
                    invalidate = invalidate || DrawCustomInspector(p_owner);
                }
            }

            var fields = this.GetType().GetFields();
            Array.Sort(fields, NodeSort.GroupSort);
            string lastGroup = "";
            bool lastGroupMinimized = false;
            int groupIndex = 1;
            foreach (var field in fields)
            {
                if (field.GetCustomAttribute<HideInInspector>() != null)
                    continue;
                if (field.IsConstant()) 
                    continue;

                TitledGroupAttribute ga = field.GetCustomAttribute<TitledGroupAttribute>();
                string currentGroup = ga != null ? ga.Group : "Properties";
                if (currentGroup != lastGroup)
                {
                    int groupMask = (int)Math.Pow(2, groupIndex);
                    groupIndex++;
                    if (initializeMinimization && ga != null && ga.Minimized && (groupsMinized & groupMask) == 0)
                    {
                        groupsMinized += groupMask;
                    }

                    isMinimized = (groupsMinized & groupMask) != 0;
                    UniGUIUtils.DrawMinimizableTitle("  " + currentGroup, ref isMinimized, 13);

                    if (isMinimized != ((groupsMinized & groupMask) != 0))
                    {
                        groupsMinized = (groupsMinized & groupMask) == 0
                            ? groupsMinized + groupMask
                            : groupsMinized - groupMask;
                    }

                    lastGroup = currentGroup;
                    lastGroupMinimized = (groupsMinized & groupMask) != 0;
                }

                if (lastGroupMinimized)
                    continue;

                invalidate = invalidate || GUIProperties.PropertyField(field, this, this);
            }

            return invalidate;
        }

        public bool HasCustomInspector()
        {
            return GetType().GetMethod("DrawCustomInspector").DeclaringType != typeof(NodeModelBase);
        }

        public virtual string GetCustomInspectorName()
        {
            return "Properties";
        }
        
        public virtual bool DrawCustomInspector(IViewOwner p_owner)
        {
            return false;
        }

        public List<string> GetExposedGUIDs()
        {
            return GetType().GetFields().ToList().FindAll(f =>
                    f.FieldType.IsGenericType && f.FieldType.GetGenericTypeDefinition() == typeof(ExposedReference<>))
                .Select(
                    (f, i) => f.GetValue(this).GetType().GetField("exposedName").GetValue(f.GetValue(this)).ToString())
                .ToList();
        }
        
        public void ValidateSerialization()
        {
            var fields = this.GetType().GetFields();
            foreach (var field in fields)
            {
                if (!GUIProperties.IsParameterProperty(field))
                    continue;
                
                if ((Parameter)field.GetValue(this) == null)
                {
                    if (!RecreateParameter(field))
                    {
                        Debug.LogWarning("Recreation of parameter property failed.");
                    }
                    else
                    {
                        Debug.LogWarning("Recreation of parameter property succeeded.");
                    }
                }
            }
        }
        
        bool RecreateParameter(FieldInfo p_fieldInfo)
        {
            Debug.LogWarning("Serialization error on parametrized property "+p_fieldInfo.Name+" encountered on model "+this+", recreating parameter to default values.");
            var genericType = p_fieldInfo.FieldType.GenericTypeArguments[0];
            var parameterType = typeof(Parameter<>).MakeGenericType(genericType);
            var parameter = Activator.CreateInstance(parameterType, genericType.GetDefaultValue());

            p_fieldInfo.SetValue(this, parameter);

            return p_fieldInfo.GetValue(this) != null;
        }
    }
}