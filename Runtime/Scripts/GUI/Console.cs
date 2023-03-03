/*
 *	Created by:  Peter @sHTiF Stefcek
 */

using System.Collections.Generic;
using System.Linq;
using OdinSerializer.Utilities;
using UnityEngine;

namespace Nodemon
{
    public class Console
    {
        public static void Add(string p_message, Color? p_color = null)
        {
            Color color = p_color == null ? Color.white : p_color.Value;
            ConsoleGUI.AddMessage((p_message, color));
        }
    }
    
    public class ConsoleGUI
    {
        public static bool visible = false;
        
        private static Vector2 _scrollPosition;
        private static string _command;

        public static List<(string,Color)> messages = new List<(string,Color)>();

        public static void AddMessage((string, Color) p_message)
        {
            messages.Add(p_message);
            _scrollPosition = new Vector2(0, int.MaxValue);
        }

        public static void DrawGUI(Rect p_rect)
        {
            if (Event.current.type == EventType.KeyDown)
            {
                if (Event.current.keyCode == KeyCode.C && Event.current.control)
                    visible = !visible;
            }
            
            if (!visible)
                return;
            
            var style = new GUIStyle();
            style.normal.background = TextureUtils.GetColorTexture(new Color(.25f, .25f, .25f, 1));
            GUILayout.BeginArea(p_rect, style);
            
            UniversalGUIUtils.DrawTitle("Console", 13);

            var titleStyle = new GUIStyle();
            titleStyle.alignment = TextAnchor.MiddleLeft;
            titleStyle.padding.left = 5;
            titleStyle.normal.textColor = new Color(1, .5f, 0);
            titleStyle.fontStyle = FontStyle.Bold;
            titleStyle.fontSize = 16;
            
            var infoStyle = new GUIStyle();
            infoStyle.normal.textColor = Color.gray;
            infoStyle.alignment = TextAnchor.MiddleLeft;
            infoStyle.padding.left = 5;

            var scrollViewStyle = UniversalGUI.Skin.scrollView;
            scrollViewStyle.normal.background = TextureUtils.GetColorTexture(new Color(.1f, .1f, .1f));
            
            GUILayout.Space(4);

            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, scrollViewStyle,
                GUILayout.ExpandWidth(true), GUILayout.Height(p_rect.height - 100));
            GUILayout.BeginVertical();
            
            foreach (var message in messages)
            {
                GUI.color = message.Item2;
                UniversalGUILayout.Label(message.Item1);
                GUI.color = Color.white;
            }
            
            GUILayout.EndVertical();
            GUILayout.EndScrollView();
            
            GUILayout.Space(4);

            _command = GUILayout.TextField(_command);
            if (Event.current.keyCode == KeyCode.Return && !_command.IsNullOrWhitespace())
            {
                ExecuteCommand();
            }

            GUILayout.Space(4);
            if (UniversalGUILayout.Button("CLEAR", GUILayout.Height(24)))
            {
                messages = new List<(string, Color)>();
            }
            
            GUILayout.EndArea();
        }

        static void ExecuteCommand()
        {
            var commandSplit = _command.Split(' ').ToList();
            string command = commandSplit[0];

            bool verbose = commandSplit.Contains("--verbose");
            commandSplit.Remove("--verbose");

            switch (command)
            {
                case "close":
                    visible = false;
                    break;
                default:
                    AddMessage(("Unknown command "+command,Color.red));
                    break;
            }

            _command = "";
        }
    }
}