using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Camstar.WebPortal.WebPortlets.XceleratorShare
{
    public class ListProjectContentExceptioin : Exception
    {
        public ListProjectContentExceptioin()
        { }

        public ListProjectContentExceptioin(string message) : base(message)
        { }
    }
}