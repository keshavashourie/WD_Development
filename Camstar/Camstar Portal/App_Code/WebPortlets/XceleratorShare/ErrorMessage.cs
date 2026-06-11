using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for ErrorMessage
/// </summary>
public static class ErrorMessage
{
    public const string MCADcreateDesignError = "Error while create Design.";
    public const string MCADcreateDesignValidateOutputError = "Error while validate to create Design output .";
    public const string DOCcreateDocError = "Error while create Document.";
    public const string DOCcreateDocValidateOutputError = "Error while validate to create Document output.";
    public const string MCADdeleteDesignError = "Error while Delete Design.";
    public const string DOCdeleteDocumentError = "Error while Delete Document.";
    public const string FileUploadFileExtError = "Invalid file to upload.";
    public const string DSSClientConnectionError = "Error while Connecting to DSS client";
    public const string DSSClientInvalidContentError = "Invalid content type, Please contact your administrator!";
    public const string DocClientConnectionError = "Error while Connecting to Document Management client";
    public const string MCADClientConnectionError = "Error while Connecting to MCAD Domain client";
    public const string CollhabClientConnectionError = "Error while Connecting to Collabhub client";
    public const string LCSClientConnctionError = "SAM Account doesn't exists, Please contact your administrator!";
    public const string LCSProjectLoadingError = "Error in loading projects";
    public const string fileuploadURNError = "Invalid document urn format";
    public const string fileuploadProjectError = "Error while populating projects.";
    public const string InvalidFileName = "Special characters * < > : ? | \\ / \" are not allowed in Document Name";
    public const string FileNotFound = "Requested file is not available";
    public const string DownloadFailure = "Something went wrong while downloading file, please try again!";
    public const string RevisionNotFound = "REVISION_NOT_FOUND";
    public const string InvalidSAMLogin = "Please login via SAM Authentication and try again";
    public const string XceleratorShareDisabled = "Please enable Xcelerator Share in Portal Studio and try again";
    public const string MissingXShareLicense = "Xcelerator Share license is missing";
    public const string RefreshTokenExtError = "Error in refresh access key for SAM";
}