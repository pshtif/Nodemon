/*
 *	Created by:  Peter @sHTiF Stefcek
 */

using System;

namespace Nodemon.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public class OrderAttribute : Attribute
    {
        public int Order { get; }

        public OrderAttribute(int p_order)
        {
            Order = p_order;
        }
    }
}