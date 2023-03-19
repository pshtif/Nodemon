/*
 *	Created by:  Peter @sHTiF Stefcek
 */

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Nodemon
{
    public class ConsoleGUI
    {
        public static bool visible = false;
        
        private static Vector2 _scrollPosition;
        private static string _command;
        private static bool _commandLineEnabled = false;

        private static List<ConsoleMessage> _newMessages = new List<ConsoleMessage>();
        private static List<ConsoleMessage> _messages = new List<ConsoleMessage>();

        private static List<ConsoleCommand> _commands = new List<ConsoleCommand>();

        public static void AddMessage(ConsoleMessage p_message)
        {
            _newMessages.Add(p_message);
        }

        public static void AddCommand(ConsoleCommand p_command)
        {
            _commands.Add(p_command);
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
            string commandString = commandSplit[0];

            bool verbose = commandSplit.Contains("--verbose");
            commandSplit.Remove("--verbose");

            var command = _commands.Find(c => c.Command == commandString);

            if (command != null)
            {
                command.Action?.Invoke();
            }
            else
            {
                switch (commandString)
                {
                    case "close":
                        visible = false;
                        break;
                    default:
                        AddMessage(new ConsoleMessage("Unknown command " + command, Color.red));
                        break;
                }
            }

            _command = "";
        }
    }
}