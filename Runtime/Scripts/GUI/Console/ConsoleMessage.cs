/*
 *	Created by:  Peter @sHTiF Stefcek
 */

using System;
using UnityEngine;

namespace Nodemon
{
    public struct ConsoleMessage
    {
        public string message;
        public string extendedMessage;
        public DateTime time;
        public Color color;

        public ConsoleMessage(string p_message, string p_extendedMessage, Color p_color)
        {
            message = p_message;
            extendedMessage = p_extendedMessage;
            time = DateTime.Now;
            color = p_color;
        }
    }
}