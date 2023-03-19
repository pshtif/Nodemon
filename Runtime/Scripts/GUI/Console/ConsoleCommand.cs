/*
 *	Created by:  Peter @sHTiF Stefcek
 */

using System;

namespace Nodemon
{
    public class ConsoleCommand
    {
        public string Command { get; private set; }
        public Action Action { get; private set; }
        
        public ConsoleCommand(string p_command, Action p_action)
        {
            Command = p_command;
            Action = p_action;
        }
    }
}