using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Camstar.WebPortal.WebPortlets.XceleratorShare
{
    public class DuplicateFileUploadException : Exception
    {
        public DuplicateFileUploadException(string fileName) : base($"File {fileName} already exists in the project")
        {

        }
    }
}