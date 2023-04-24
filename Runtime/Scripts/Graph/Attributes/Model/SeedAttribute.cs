/*
 *	Created by:  Peter @sHTiF Stefcek
 */

using System;

namespace Nodemon.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public class SeedAttribute : Attribute
    {
        public float minValue = 0;
        public float maxValue = 0;
        
        public SeedAttribute(float p_minValue = 0, float p_maxValue = 1000000)
        {
            minValue = p_minValue;
            maxValue = p_maxValue;
        }
    }
}