using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Portable.Utils
{
    public class NameValueCollection : Dictionary<string, string>
    {
        public IEnumerable<string> AllKeys
        {
            get { return Keys; }
        }
    }
}
