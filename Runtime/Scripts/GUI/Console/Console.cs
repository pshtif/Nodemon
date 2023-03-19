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
    public class Console
    {
        public static void Add(string p_message, Color? p_color = null)
        {
            var message = new ConsoleMessage(p_message, p_color.HasValue ? p_color.Value : Color.white);
            ConsoleGUI.AddMessage(message);
        }

        public static void AddCommand(ConsoleCommand p_command)
        {
            ConsoleGUI.AddCommand(p_command);
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
}