using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace Camstar.WebPortal.WebPortlets.XceleratorShare
{
    public class CollabhubClientException : Exception
    {
        public CollabhubClientException()
        {
        }
        public CollabhubClientException(string message) : base(message)
        {
        }
    }
}