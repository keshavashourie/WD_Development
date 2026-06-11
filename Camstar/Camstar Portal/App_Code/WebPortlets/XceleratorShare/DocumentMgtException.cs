using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for DocumentMgtException
/// </summary>


namespace Camstar.WebPortal.WebPortlets.XceleratorShare
{
    public class DocumentMgtException : Exception
    {
        public DocumentMgtException()
        {
        }
        public DocumentMgtException(string message) : base(message)
        {
        }
    }
}