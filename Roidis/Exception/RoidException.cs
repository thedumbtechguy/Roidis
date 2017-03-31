using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roidis.Exception
{
    public class RoidException : System.Exception
    {
        public RoidException() : base() { }

        public RoidException(string message) : base(message) { }

        public RoidException(string message, System.Exception innerException) : base(message, innerException) { }
    }
}
