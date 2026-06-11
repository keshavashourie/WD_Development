using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for MCADDesignException
/// </summary>

namespace Camstar.WebPortal.WebPortlets.XceleratorShare
{
    public class MCADDesignException : Exception
    {
        public MCADDesignException()
        {
        }
        public MCADDesignException(string message) : base(message)
        {
        }
    }
}
