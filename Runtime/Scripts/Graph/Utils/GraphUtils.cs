/*
 *	Created by:  Peter @sHTiF Stefcek
 */

using System.Collections.Generic;
using System.IO;
using System.Linq;
using OdinSerializer;
using UnityEngine;

namespace Nodemon
{
    public class GraphUtils
    {
        public static string AddChildPath(string p_path, string p_subPath)
        {
            return p_path + (p_path.Length>0 ?  "/" : "") + p_subPath;
        }
        
        public static T GetGraphAtPath<T>(T p_graph, string p_path) where T : GraphBase
        {
            if (string.IsNullOrWhiteSpace(p_path))
                return p_graph;

            List<string> split = p_path.Split('/').ToList();
            T currentGraph = p_graph;
            foreach (string id in split)
            {
                ISubGraphBase node = currentGraph.GetNodeById(id) as ISubGraphBase;
                if (node == null)
                {
                    Debug.LogWarning("Cannot retrieve subgraph at invalid graph path " + p_path);
                    return null;
                }

                currentGraph = (T)node.GetSubGraphBase();
            }

            return currentGraph;
        }
        
        public static string GetParentPath(string p_path)
        {
            if (string.IsNullOrWhiteSpace(p_path) || p_path.IndexOf("/") == -1)
                return "";

            return p_path.Substring(0, p_path.LastIndexOf("/"));
        }
        
        public static bool IsSubGraph(string p_path)
        {
            return !string.IsNullOrEmpty(p_path);
        }
        
        public static void ImportJSON(GraphBase p_graph)
        {
#if UNITY_EDITOR
            string path = UnityEditor.EditorUtility.OpenFilePanel("Load JSON", Application.dataPath, "json");

            byte[] content = File.ReadAllBytes(path);
            List<Object> references = new Object[] {p_graph}.ToList(); // refactor to serialize to acquire all correct ones before deserialization
            p_graph.DeserializeFromBytes(content, DataFormat.JSON, ref references);
#endif
        }

        public static void ExportJSON(GraphBase p_graph)
        {
#if UNITY_EDITOR
            var path = UnityEditor.EditorUtility.SaveFilePanel(
                "Export Graph JSON",
                Application.dataPath,
                "",
                "json");

            List<Object> references = new List<Object>();
            byte[] bytes = p_graph.SerializeToBytes(DataFormat.JSON, ref references);
            File.WriteAllBytes(path, bytes);
#endif
        }
        
        public static T CreateGraphAsAssetFile<T>(T p_graph = null) where T : GraphBase
        {
#if UNITY_EDITOR
            var path = UnityEditor.EditorUtility.SaveFilePanelInProject(
                "Create Machina graph",
                "MachinaGraph",
                "asset",
                "Enter name for new Machina graph.");
            
            if (path.Length != 0)
            {
                return CreateGraphAsAssetFromPath(path, p_graph);
            }
#endif            
            return null;
        }
        
        public static T CreateGraphAsAssetFromPath<T>(string p_path, T p_graph) where T : GraphBase
        {
            if (p_graph == null)
                p_graph = CreateEmptyGraph<T>();

#if UNITY_EDITOR
            UnityEditor.AssetDatabase.CreateAsset(p_graph, p_path);
            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();
#endif

            return p_graph;
        }
        
        public static T CreateGraphAsAsset<T>() where T : GraphBase
        {
            T graph = ScriptableObject.CreateInstance<T>();
            
#if UNITY_EDITOR
            UnityEditor.AssetDatabase.CreateAsset(graph, "Assets/MachinaGraph.asset");
            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();
#endif

            return graph;
        }
        
        public static T CreateEmptyGraph<T>() where T : GraphBase
        {
            return ScriptableObject.CreateInstance<T>();
        }
        
        public static T LoadGraph<T>() where T : GraphBase
        {
#if UNITY_EDITOR
            string graphPath = UnityEditor.EditorUtility.OpenFilePanel("Load Graph", Application.dataPath, "");
            int appPathLength = Application.dataPath.Length;
            graphPath = graphPath.Substring(appPathLength - 6);

            return (T)UnityEditor.AssetDatabase.LoadAssetAtPath(graphPath, typeof(T));
#else
            return null;
#endif
        }
    }
}