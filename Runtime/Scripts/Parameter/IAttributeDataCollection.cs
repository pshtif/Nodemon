/*
 *	Created by:  Peter @sHTiF Stefcek
 */

namespace Nodemon
{
    public interface IAttributeDataCollection
    {
        bool HasAttribute(string p_name);

        T GetAttribute<T>(string p_name);

        object GetAttribute(string p_name);
    }
}