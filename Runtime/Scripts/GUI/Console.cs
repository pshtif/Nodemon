/*
 *	Created by:  Peter @sHTiF Stefcek
 */

using System;
using System.Collections.Generic;
using System.Linq;
using OdinSerializer.Utilities;
using UnityEngine;

namespace Nodemon
{
    public struct ConsoleMessage
    {
        public string message;
        public DateTime time;
        public Color color;

        public ConsoleMessage(string p_message, Color p_color)
        {
            message = p_message;
            time = DateTime.Now;
            color = p_color;
        }
    }
    
    public class Console
    {
        public static void Add(string p_message, Color? p_color = null)
        {
            var message = new ConsoleMessage(p_message, p_color.HasValue ? p_color.Value : Color.white);
            ConsoleGUI.AddMessage(message);
        }
        
        public static void HookUnityLog()
        {
            Application.logMessageReceivedThreaded -= HandleUnityLog;
            Application.logMessageReceivedThreaded += HandleUnityLog;
        }

        private static void HandleUnityLog(string p_logString, string p_stackTrace, LogType p_type)
        {
            ConsoleMessage message;
            
            switch (p_type)
            {
                case LogType.Error:
                case LogType.Assert:
                case LogType.Exception:
                    message = new ConsoleMessage(p_logString, Color.red);
                    break;
                case LogType.Warning:
                    message = new ConsoleMessage(p_logString, Color.yellow);
                    break;
                default:
                    message = new ConsoleMessage(p_logString, Color.white); 
                    break;
            }
            
            ConsoleGUI.AddMessage(message);
        }
    }
    
    public class ConsoleGUI
    {
        public static bool visible = false;
        
        private static Vector2 _scrollPosition;
        private static string _command;
        private static bool _commandLineEnabled = false;

        private static List<ConsoleMessage> _newMessages = new List<ConsoleMessage>();
        private static List<ConsoleMessage> _messages = new List<ConsoleMessage>();

        public static void AddMessage(ConsoleMessage p_message)
        {
            _newMessages.Add(p_message);
        }

        public static void DrawGUI(Rect p_rect)
        {
            if (Event.current.type == EventType.Layout && _newMessages.Count>0)
            {
                _messages.AddRange(_newMessages);
                _newMessages.Clear();
                _scrollPosition = new Vector2(0, int.MaxValue);
            }
            
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
            
            UniversalGUIUtils.DrawTitle("Console", 13, 2);

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

            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, scrollViewStyle,
                GUILayout.ExpandWidth(true), GUILayout.Height(p_rect.height - (_commandLineEnabled ? 96 : 72)));
            GUILayout.BeginVertical();
            
            foreach (var message in _messages)
            {
                GUI.color = message.color;
                UniversalGUILayout.Label("["+message.time.ToString("HH:mm:ss")+"] "+message.message);
                GUI.color = Color.white;
            }
            
            GUILayout.EndVertical();
            GUILayout.EndScrollView();

            if (_commandLineEnabled)
            {
                GUILayout.Space(4);
                
                _command = GUILayout.TextField(_command);
                if (Event.current.keyCode == KeyCode.Return && !_command.IsNullOrWhitespace())
                {
                    ExecuteCommand();
                }
            }
            
            GUILayout.Space(2);

            GUILayout.BeginHorizontal();
            
            if (UniversalGUILayout.Button("COMMAND LINE", GUILayout.Height(24)))
            {
                _commandLineEnabled = !_commandLineEnabled;
            }
            
            if (UniversalGUILayout.Button("CLEAR", GUILayout.Height(24)))
            {
                _messages.Clear();
            }
            
            GUILayout.EndHorizontal();
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
                    AddMessage(new ConsoleMessage("Unknown command "+command,Color.red));
                    break;
            }

            _command = "";
        }
    }
}