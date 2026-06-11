using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for CollabspaceNotExistsException
/// </summary>
namespace Camstar.WebPortal.WebPortlets.XceleratorShare
{
    public class CollabspaceNotExistsException : Exception
    {
        public CollabspaceNotExistsException()
        {
        }
        public CollabspaceNotExistsException(string message) : base(message)
        {
        }
    }
}
