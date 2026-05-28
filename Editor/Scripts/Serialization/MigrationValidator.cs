/*
 *	Created by:  Peter @sHTiF Stefcek
 */

using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Nodemon
{
    public static class MigrationValidator
    {
        [InitializeOnLoadMethod]
        static void Validate()
        {
            var types = MigrationUtils.GetAllMigratableTypes();
            
            var typeMap = new Dictionary<string, Type>();

            foreach (var type in types)
            {
                if (!type.IsDefined(typeof(SerializedIdAttribute)))
                {
                    Debug.LogError("IMigratable type: "+type+" needs to have [SerializedIdAttribute]");
                }
                
                string name = MigrationUtils.GetTypeName(type);

                if (!typeMap.ContainsKey(name))
                {
                    typeMap.Add(name, type);
                }
                else
                {
                    Debug.LogError("Type: ("+type+") uses same name: "+name+" as type: "+typeMap[name]);
                }
            }
        }
    }
}