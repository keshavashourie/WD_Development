using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Representation of Document inside Project
/// </summary>

namespace Camstar.WebPortal.WebPortlets.XceleratorShare
{
    public class XShareDocument
    {
        public string id;
        public Properties properties;
        public string typeName;
        public string domainName;
        public string urn;
        public string consistencyStamp;

        public class Properties
        {
            public string name;
            public string description;
        }
        public XShareDocument()
        {

        }
    }
}