using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Camstar.WebPortal.WebPortlets.XceleratorShare
{
    /// <summary>
    /// Summary description for MCADExt
    /// </summary>
    public class MCADExt
    {
        public MCADExt()
        {
            //
            // TODO: Add constructor logic here
            //
        }
        public List<Element> elements { get; set; }
    }

    public class Element
    {

        public string name { get; set; }
        public string parentType { get; set; }
        public List<string> fileExtensions { get; set; }
        public List<Property> properties { get; set; }
        public List<RelatedFile> relatedFiles { get; set; }
        public string fileExtensionPattern { get; set; }

    }

    public class RelatedFile
    {
        public string datasetTypeName { get; set; }
        public string alias { get; set; }
        public int cardinality { get; set; }
        public bool generated { get; set; }
        public string typeName { get; set; }

    }

    public class Property
    {
        public string name { get; set; }
        public string displayName { get; set; }
        public string description { get; set; }
        public string dataType { get; set; }
        public bool isArray { get; set; }
        public bool required { get; set; }
        public bool isDisplayable { get; set; }
        public string mutablity { get; set; }
        public string typeName { get; set; }
        public int? maxStringLength { get; set; }
        public string LOV { get; set; }

    }

    public static class MCADExtensions
    {

        public static List<Element> elements { get; set; }
    }
}