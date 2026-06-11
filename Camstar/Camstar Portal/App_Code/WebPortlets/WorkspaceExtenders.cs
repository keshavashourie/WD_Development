using Camstar.WCF.ObjectStack;
using Camstar.WebPortal.WCFUtilities;
using System.Web;

/************************************************************************
 * This .cs file stores any "extension" classes for core classes. These
 * classes are abstract classes. 
 * 
 * The WorkspaceExtenderFactory will load an instance of a class that inherits 
 * from a class defined here.
************************************************************************ */

/// <summary>
/// Load JT files using the isImageMaint service. This class integrates with the
/// JtToBodConvertHandler class to provide custom image loading. The WorkspaceExtenderFactory
/// will instantiate this class.
/// </summary>
/// <seealso cref="WorkspaceExtenderFactory"/>
public abstract class JtToBodConvertHandlerExtender
{
    public abstract DocumentRefInfo GetDocInfo(RevisionedObjectRef docRef, HttpContext context, out ResultStatus txnResult);
    public abstract void DownloadDocumentLocal(DocumentRefInfo docInfo, string filePath, HttpContext context);
}
