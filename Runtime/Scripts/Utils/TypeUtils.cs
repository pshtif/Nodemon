/*
 *	Created by:  Peter @sHTiF Stefcek
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using OdinSerializer.Utilities;

namespace Nodemon
{
    static public class TypeUtils
    {
        private static Type[] _typeCache;
        private static Assembly[] _assemblyCache;

        public static List<Type> GetAllAssignableTypes(Type p_type)
        {
            return AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes())
                .Where(x => p_type.IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract).ToList();
        }

        public static Type[] GetAllTypes() {
            if ( _typeCache != null ) {
                return _typeCache;
            }

            var assemblies = GetAllAssemblies();

            var result = new List<Type>();

            assemblies.Where(a => !a.IsDynamic).ForEach(a => result.AddRange(a.GetExportedTypes()));

            var typeDictionary = new Dictionary<string, Type>();
            foreach (var type in result)
            {
                if (!typeDictionary.ContainsKey(type.FullName))
                {
                    typeDictionary.Add(type.FullName, type);
                }
            }

            return _typeCache = result.OrderBy(t => t.Namespace).ThenBy(t => t.Name).ToArray();
        }
        
        private static Assembly[] GetAllAssemblies()
        {
            return _assemblyCache != null ? _assemblyCache : _assemblyCache = AppDomain.CurrentDomain.GetAssemblies();   
        } 
        
        public static Type GetTypeByName(string p_typeName)
        {
            var type = Type.GetType(p_typeName);
            
            if(type != null)
                return type;

            var assemblyName = p_typeName.Substring(0, p_typeName.LastIndexOf('.'));
            
            var assembly = Assembly.Load(assemblyName);
            if( assembly == null )
                return null;
            
            return assembly.GetType(p_typeName);
        }

        public static string GetTypeNameWithoutAssembly(string p_type)
        {
            return p_type.Substring(p_type.LastIndexOf('.')+1);
        }
    }
}