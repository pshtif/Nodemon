/*
 *	Created by:  Peter @sHTiF Stefcek
 */

using System.Collections.Generic;
using UnityEngine;

namespace Nodemon
{
    public class UniversalGUITextureManager
    {
        static private Dictionary<string, Texture> _cache = new Dictionary<string, Texture>();

        public static Texture GetTexture(string p_name)
        {
            if (_cache.ContainsKey(p_name))
                return _cache[p_name];

            Texture texture = Resources.Load<Texture>("Textures/"+p_name);
            if (texture != null)
            {
                _cache.Add(p_name, texture);
            }
            else
            {
                Debug.Log("Texture not found "+p_name);
            }

            return texture;
        }   
    }
}