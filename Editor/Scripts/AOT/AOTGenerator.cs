/*
 *	Created by:  Peter @sHTiF Stefcek
 */

#if UNITY_EDITOR
using System;
using System.IO;
using System.Collections.Generic;
using OdinSerializer.Editor;

namespace Nodemon.Editor
{
    public class AOTGenerator
    {
        public static void GenerateDLL(List<Type> p_types, string p_assemblyPath, string p_assemblyName, bool p_generateLinkXml = true, bool p_includeOdin = false)
        {
            if (p_generateLinkXml)
            {
                if (p_includeOdin)
                {
                    File.WriteAllText(p_assemblyPath + "/link.xml",
                        @"<linker>                    
                         <assembly fullname=""" + p_assemblyName + @""" preserve=""all""/>
                         <assembly fullname=""OdinSerializer"" preserve=""all""/>
                      </linker>");
                }
                else
                {
                    File.WriteAllText(p_assemblyPath + "/link.xml",
                        @"<linker>                    
                         <assembly fullname=""" + p_assemblyName + @""" preserve=""all""/>
                      </linker>");
                }
            }
            
            AOTSupportUtilities.GenerateDLL(p_assemblyPath, p_assemblyName, p_types, false);
        }
    }
}
#endif