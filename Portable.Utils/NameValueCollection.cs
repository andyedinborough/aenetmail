using System;
using System.Collections.Generic;

namespace Portable.Utils
{
    public class NameValueCollection : Dictionary<string, string>
    {
        public IEnumerable<string> AllKeys
        {
            get { return Keys; }
        }

        public new string this[String key]
        {
            get
            {
                string value;
                if (TryGetValue(key, out value))
                    return value;
                return null;
            }
            set
            {
                if (ContainsKey(key))
                    base[key] = value;
                else
                    Add(key, value);
            }
        }
    }
}
