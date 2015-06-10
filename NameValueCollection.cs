using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;

namespace AE.Net.Mail
{
    public class NameValueCollection : System.Dynamic.DynamicObject
    {
        private Dictionary<string, object> _values = new Dictionary<string, object>(StringComparer.CurrentCultureIgnoreCase);

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            _values.TryGetValue(binder.Name, out result);
            return true;
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            return Set(binder.Name, value);
        }

        public override bool TrySetIndex(SetIndexBinder binder, object[] indexes, object value)
        {
            var name = indexes[0] as string;
            return Set(name, value);
        }

        public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
        {
            _values.TryGetValue(indexes[0] as string, out result);
            return true;
        }

        private bool Set(string name, object value)
        {
            if (_values.ContainsKey(name)) _values[name] = value;
            else _values.Add(name, value);
            return true;
        }
    }
}