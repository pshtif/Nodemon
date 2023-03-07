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
        public DateTime time;
        public Color color;

        public ConsoleMessage(string p_message, Color p_color)
        {
            message = p_message;
            time = DateTime.Now;
            color = p_color;
        }
    }
}