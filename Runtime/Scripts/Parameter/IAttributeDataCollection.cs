/*
 *	Created by:  Peter @sHTiF Stefcek
 */

namespace Nodemon
{
    public interface IAttributeDataCollection
    {
        bool HasAttribute(string p_name);
        
        K GetAttributeValue<K>(string p_name);
        
        object GetAttributeValue(string p_name);
    }
    
    public interface IAttributeDataCollection<T> : IAttributeDataCollection
    {
        T GetAttribute(string p_name);
    }
}