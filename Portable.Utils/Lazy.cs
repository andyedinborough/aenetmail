using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Portable.Utils
{
    public class Lazy<T>
    {
        private Func<T> _foo;
 
        public Lazy(Func<T> foo)
        {
            _foo = foo;
        }
    }
}
