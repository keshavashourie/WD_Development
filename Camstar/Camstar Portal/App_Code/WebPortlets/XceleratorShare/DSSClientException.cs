using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for DSSClientException
/// </summary>


namespace Camstar.WebPortal.WebPortlets.XceleratorShare
{
    public class DSSClientException : Exception
    {
        public DSSClientException()
        {
        }
        public DSSClientException(string message) : base(message)
        {
        }
    }
}