using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for Attachment
/// </summary>
namespace Camstar.WebPortal.WebPortlets.XceleratorShare
{
    public class Attachment
    {
        public string CollabsapceId { get; private set; }
        public string ContainerType { get; private set; }
        public string ContainerId { get; private set; }
        public string Urn { get; private set; }
        public AttachmentDomain Domain { get; private set; }
        public string Url { get; private set; }
        public string ContainerName { get; private set; }
        public string ParentFolder { get; private set; }
        public Attachment(string collabspaceId, string containerType, string containerId, string urn, AttachmentDomain domain, string containerName, string parentFolder, string url = "")
        {
            CollabsapceId = collabspaceId;
            ContainerType = containerType;
            ContainerId = containerId;
            Domain = domain;
            Urn = urn;
            Url = url;
            ContainerName = containerName;
            ParentFolder = parentFolder;
        }
    }
}
