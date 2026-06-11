using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for FileUploadException
/// </summary>


namespace Camstar.WebPortal.WebPortlets.XceleratorShare
{
    public class FileUploadException : Exception
    {
        public FileUploadException()
        {
        }
        public FileUploadException(string message) : base(message)
        {
        }
    }
}