// Copyright Siemens 2024

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using System.Web.UI;
using Camstar.WCF.Services;
using Camstar.WebPortal.Constants;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WebPortal.Utilities;
using Camstar.WebPortal.WCFUtilities;
using Camstar.WebPortal.FormsFramework.Utilities;
using OM = Camstar.WCF.ObjectStack;
using System.Web.Services;

public partial class DownloadFile : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (Page.Session["UserName"] == null || Page.Session["UserName"].ToString() == "")
        {
            ClientScript.RegisterStartupScript(typeof(string), "fileNotLoeaded",
                string.Format("parent.alert('{0}');", "You are not authorized to access this file"), true);
        }
        else
        {
            if (Request.QueryString["uploadFile"] != null)
            {
                if (QueryStringViolatesSecurityPolicy())
                {
                    RegisterAlertWhenQueryStringViolatesSecurityPolicy();
                    return;
                }

                var path = "";
                var message = "";
                var status = 0;
                if (Request.Files.Count > 0)
                {
                    string outPath;
                    if (FileInput.UploadFile(Request.Files[0], out outPath))
                    {
                        FileInput.UploadFilePath = outPath;
                        path = Path.GetFileName(outPath);
                        status = 1;
                    }
                    else
                    {
                        message = outPath;
                    }
                }
                else if (Session[
                             Request.QueryString[QueryStringConstants.CallStackKey] +
                             SessionConstants.ErrorStatusMessage] != null)
                {
                    message = (Session[
                        Request.QueryString[QueryStringConstants.CallStackKey] +
                        SessionConstants.ErrorStatusMessage] as string);
                }
                else
                {
                    message = "No files to upload or incorrect request.";
                }

                var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                Response.ClearContent();
                Response.ContentType = "text/plain";
                Response.Write(serializer.Serialize(new { status, path, message }));
                Response.End();
            }

            bool isDownloadSucceed = false;
            if (!string.IsNullOrEmpty(Request.QueryString["viewdocfile"]))
            {
                var fileName = HttpUtility.UrlEncode(Request.QueryString["viewdocfile"]);
                var originalFileName = HttpUtility.UrlDecode(GetOrigFileNameFromQueryString(Request.QueryString.ToString()));// Request.QueryString["origfilename"];
                isDownloadSucceed = DownloadAttachement(fileName, originalFileName); ;
            }

            OM.ResultStatus txnResult = null;
            if (!string.IsNullOrEmpty(Request.QueryString["retrieveviewdocfile"])) // SPC stuff.
            {
                var docName = Request.QueryString["retrieveviewdocfile"];
                var cskey = Request.QueryString["cskey"];
                var cstack = new CallStack(cskey);
                var documents =
                    cstack.Context.LocalSession["DownloadFileDocuments"] as Dictionary<string, OM.RevisionedObjectRef>;
                if (documents.ContainsKey(docName))
                {
                    var docRef = documents[docName];
                    var docMaintInfo = AttachmentExecutor.GetDocumentInfo(docRef.Name, docRef.Revision,
                        CurrentUserProfile(), out txnResult);
                    if (txnResult.IsSuccess)
                        isDownloadSucceed = DownloadDocumentMaint(docMaintInfo);
                }
            }
            else if (!string.IsNullOrEmpty(Request.QueryString["docMaintName"])) // download document maint object.
            {
                var docName = Request.QueryString["docMaintName"];
                var docRev = Request.QueryString["docMaintRev"];
                var docMaintInfo =
                    AttachmentExecutor.GetDocumentInfo(docName, docRev, CurrentUserProfile(), out txnResult);
                if (txnResult.IsSuccess)
                    isDownloadSucceed = DownloadDocumentMaint(docMaintInfo);
            }
            else if (!string.IsNullOrEmpty(Request.QueryString["docMaintURL"])) // download document url with authentication.
            {
                var docMaintInfo = new DocumentRefInfo { URI = Request.QueryString["docMaintURL"] };
                var docAuth = Request.QueryString["docMaintAuth"];
                if (!string.IsNullOrEmpty(docAuth))
                {
                    if (string.Equals(docAuth, "basic", StringComparison.CurrentCultureIgnoreCase))
                        docMaintInfo.AuthenticationType = OM.AuthenticationTypeEnum.Basic;
                }

                isDownloadSucceed = DownloadDocumentMaint(docMaintInfo);
            }

            int tempID;
            if (int.TryParse(Request.QueryString["AttachmentsID"], out tempID) && tempID == 0)
            {
                //dispose the memorystream object
                using (MemoryStream stream = new MemoryStream(Page.Session["DownloadData"] as byte[], false))
                {
                    string err = string.Empty;
                    isDownloadSucceed = OutputFile("", Request.QueryString["Name"], stream, out err);
                    //Only clear the session if we are successful on downloading the file
                    if (isDownloadSucceed)
                    {
                        Page.Session["DownloadData"] = null;
                    }
                }
            }
            else
            {
                OM.DocAttachmentsTxn data = new OM.DocAttachmentsTxn();
                OM.DocAttachmentsTxn txnOutput = null;
                data.DocDetail = new Camstar.WCF.ObjectStack.AttachedDocDetail();

                if (!string.IsNullOrEmpty(Request.QueryString["Name"]) &&
                    !string.IsNullOrEmpty(Request.QueryString["AttachmentsID"]))
                {
                    data.DocDetail.Name = Request.QueryString["Name"];
                    data.DocDetail.Version = string.IsNullOrWhiteSpace(Request.QueryString["Version"])
                        ? string.Empty
                        : Request.QueryString["Version"];

                    if (SetAttachmentsID(ref data, Request.QueryString["AttachmentsID"]))
                        txnResult = AttachmentExecutor.ViewAttachment(data, CurrentUserProfile(), out txnOutput);
                }

                if (txnResult != null && txnOutput != null)
                {
                    if (txnResult.IsSuccess)
                    {
                        if (txnOutput.DocDetail.File != null)
                        {
                            if (!String.IsNullOrEmpty(txnOutput.DocDetail.File.Value))
                            {
                                string err = string.Empty;

                                string originalName = string.Empty;

                                if (txnOutput.DocDetail.OriginalFileName != null)
                                {
                                    if (!String.IsNullOrEmpty(txnOutput.DocDetail.OriginalFileName.Value))
                                    {
                                        originalName = txnOutput.DocDetail.OriginalFileName.Value;
                                    } //if
                                } //if

                                FileStream readerStream = null;

                                if (FileMgmtUtil.DownloadFileFromFileShare(HttpUtility.UrlDecode(txnOutput.DocDetail.File.Value),
                                    out readerStream, out err))
                                {
                                    try
                                    {
                                        string decodeOriginalFileName = HttpUtility.UrlDecode(originalName);
                                        isDownloadSucceed = OutputFile(txnOutput.DocDetail.File.Value, decodeOriginalFileName, readerStream, out err);
                                    } //try
                                    finally
                                    {
                                        FileMgmtUtil.DeleteFile(txnOutput.DocDetail.File.Value, out err);
                                    } //finally
                                } //if
                            } //if
                        } //if
                    }
                    else
                    {
                        ClientScript.RegisterStartupScript(typeof(string), "fileNotLoeaded",
                            string.Format("parent.alert('{0}');", txnResult), true);
                    }
                }
            }

            if (isDownloadSucceed)
                HttpContext.Current.Response.End();
        }
    }

    private string GetOrigFileNameFromQueryString(string src)
    {
        const string origFileNameParam = "origfilename=";
        const int origFileNameParamLength = 13;

        int startIndex = src.LastIndexOf(origFileNameParam) + origFileNameParamLength;
        string result = src.Substring(startIndex, src.Length - startIndex);
        result = result.Replace("+", "%2B"); // otherwise the Decode converts pluses to spaces

        return result;
    }
    private bool DownloadDocumentMaint(DocumentRefInfo documentRefInfo)
    {
        bool isDownloadSucceed = false;
        var auth = documentRefInfo.AuthenticationType;
        if (auth == OM.AuthenticationTypeEnum.None)
        {
            var fileName = RetrieveFileNameFromDocument(documentRefInfo.DocumentRef);
            isDownloadSucceed = DownloadAttachement(fileName, null);
        }
        else if (auth == OM.AuthenticationTypeEnum.Basic)
        {
            if (string.IsNullOrEmpty(documentRefInfo.Credentials))
                documentRefInfo.Credentials = AttachmentExecutor.GetDocCredentials(auth, CurrentUserProfile());

            var request = (HttpWebRequest)WebRequest.Create(documentRefInfo.URI);
            request.PreAuthenticate = true;
            request.Headers.Add("Authorization", "Basic " + documentRefInfo.Credentials);

            string errorMessage = string.Empty;
            try
            {
                bool result;
                using (var response = request.GetResponse())
                {
                    var readerStream = response.GetResponseStream();
                    result = OutputFile(null, documentRefInfo.FileName, readerStream, out errorMessage, response.ContentLength);
                }

                if (result && string.IsNullOrEmpty(errorMessage))
                    HttpContext.Current.Response.End();
                else if (!string.IsNullOrEmpty(errorMessage))
                    ClientScript.RegisterStartupScript(typeof(string), "download", string.Format("alert('{0}');", errorMessage), true);
                else
                    ClientScript.RegisterStartupScript(typeof(string), "download", string.Format("window.open('{0}');", documentRefInfo.FileName), true);
                isDownloadSucceed = result;

            } //try
            catch (Exception ex)
            {
                errorMessage = ex.Message;
            }
            if (!string.IsNullOrEmpty(errorMessage))
            {
                //This was put here due to the char "'" exiting the alert before displayed. This happens when the server can not be resolved.
                errorMessage = errorMessage.Replace("'", "");
                ClientScript.RegisterStartupScript(typeof(string), "download", string.Format("alert('{0}');", errorMessage), true);
            }
        }
        return isDownloadSucceed;
    }

    private string ResolveFilePath(string path)
    {
        var isServerFile = (path.IndexOf("http") >= 0);
        string configFolder = CamstarPortalSection.Settings.DefaultSettings.UploadDirectory;
        var result = "";

        //Resolve file path base on if it is local file or a server file
        if (isServerFile)
        {
            int pos = -1;
            for (int i = 0; i < 3; i++)
            {
                pos = path.IndexOf('/', pos + 1);
                if (pos == -1) break;
            }
            if (pos != -1)
                result = Server.MapPath(path.Substring(pos, path.Length - pos));
            else
                ClientScript.RegisterStartupScript(typeof(string), "download", string.Format("alert('Server {0} Path is not found.');", path), true);
        }
        else
        {
            if (Directory.Exists(configFolder))
            {
                var dirInfo = new DirectoryInfo(configFolder);
                result = Path.Combine(dirInfo.FullName, path);
            }
            else
                ClientScript.RegisterStartupScript(typeof(string), "download", string.Format("alert('Shared {0} folder is not found.');", HttpUtility.JavaScriptStringEncode(configFolder)), true);
        }
        return result;
    }

    private string RetrieveFileNameFromDocument(OM.RevisionedObjectRef documentRef)
    {
        var fileName = string.Empty;
        var data = new OM.DocumentMaint { ObjectToChange = documentRef };
        var request = new DocumentMaint_Request();
        request.Info = new OM.DocumentMaint_Info();
        request.Info.ObjectChanges = new OM.DocumentChanges_Info();
        request.Info.ObjectChanges.FileName = new OM.Info(true);
        request.Info.ObjectChanges.Identifier = new OM.Info(true);

        DocumentMaint_Result result;
        var session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
        var serv = new DocumentMaintService(session.CurrentUserProfile);
        var resultStatus = serv.Load(data, request, out result);
        if (resultStatus.IsSuccess)
        {
            var documentIdentifier = (string)result.Value.ObjectChanges.Identifier;
            var isHTTPFile = false;
            if (documentIdentifier != null)
            {
                isHTTPFile = documentIdentifier.StartsWith("http://") || documentIdentifier.StartsWith("https://");
            }
            if (documentIdentifier != null && isHTTPFile)
            {
                fileName = documentIdentifier;
            }
            else
            {
                var configFolder = CamstarPortalSection.Settings.DefaultSettings.UploadDirectory;
                if (Directory.Exists(configFolder))
                {
                    var dirInfo = new DirectoryInfo(configFolder);
                    var data1 = new OM.DocumentMaint { ObjectChanges = new OM.DocumentChanges { FileLocation = dirInfo.FullName } };

                    serv.BeginTransaction();
                    serv.Load(data);
                    serv.DownloadFile(data1);
                    resultStatus = serv.CommitTransaction(request, out result);

                    if (resultStatus.IsSuccess && result != null)
                    {
                        fileName = result.Value.ObjectChanges.FileName != null ? result.Value.ObjectChanges.FileName.Value : null;
                    }
                }
            }
        }
        return fileName;
    }

    private bool DownloadAttachement(string fileName, string originalFileName)
    {
        var result = false;
        var fullPath = !string.IsNullOrEmpty(originalFileName) ? ResolveFilePath(originalFileName) : ResolveFilePath(fileName);
        string actualFileName = HttpUtility.UrlDecode(fullPath);
        string decodeOriginalFileName = !string.IsNullOrEmpty(originalFileName) ? HttpUtility.UrlDecode(originalFileName) : string.Empty;
        string err;
        FileStream readerStream;
        if (FileMgmtUtil.DownloadFileFromFileShare(actualFileName, out readerStream, out err))
        {
            try
            {
                result = OutputFile(fullPath, decodeOriginalFileName, readerStream, out err);
            } //try
            catch (Exception ex)
            {
                err = ex.Message;
            }

        }//if
        if (result && string.IsNullOrEmpty(err))
            HttpContext.Current.Response.End();
        else if (!string.IsNullOrEmpty(err))
            ClientScript.RegisterStartupScript(typeof(string), "download", string.Format("alert('{0}');", err), true);
        else
            ClientScript.RegisterStartupScript(typeof(string), "download", string.Format("window.open('{0}');", fileName), true);
        return result;
    }

    private bool OutputFile(string filePathAndName, string originalFileName, Stream readerStream, out string error, long length = -1)
    {
        bool cancelStreaming = false;
        error = string.Empty;

        try
        {
            byte[] array = new byte[1024];

            System.Web.HttpContext.Current.Response.Clear();
            System.Web.HttpContext.Current.Response.ContentType = "application/octet-stream";
            System.Web.HttpContext.Current.Response.BufferOutput = true;

            string header = "attachment; filename=\"";
            if (!String.IsNullOrEmpty(originalFileName))
            {
                header += originalFileName;
            }//if
            else
            {
                header += Path.GetFileName(FileMgmtUtil.ExtractFileName(filePathAndName));
            }//else
            header += "\"";

            System.Web.HttpContext.Current.Response.AddHeader("Content-Disposition", header);
            
            if (readerStream != null)
            {
                var memoryStream = readerStream as MemoryStream;
                if (memoryStream != null)
                    length = memoryStream.Length;
            }
            if (length == -1)//&& !string.IsNullOrEmpty(filePathAndName) && !string.IsNullOrEmpty(actualFileName))
            {
                if (!string.IsNullOrEmpty(filePathAndName))
                {                   
                    length = GetFileLength(filePathAndName);
                }
                if (length == -1 && !string.IsNullOrEmpty(originalFileName))
                    length = GetFileLength(originalFileName);              
            }
            if (length != -1)
                System.Web.HttpContext.Current.Response.AddHeader("Content-Length", length.ToString());

            int count = -1;
            while (count != 0)
            {
                //Check is client cancel downloading file
                if (!System.Web.HttpContext.Current.Response.IsClientConnected)
                {
                    //System.Web.HttpContext.Current.Response.Status

                    cancelStreaming = true;
                    break;
                }//if

                count = readerStream.Read(array, 0, 1024);
                if (count < 1024)
                {
                    if (count > 0)
                    {
                        byte[] rest = new byte[count];
                        Array.Copy(array, rest, count);
                        System.Web.HttpContext.Current.Response.BinaryWrite(rest);
                        System.Web.HttpContext.Current.Response.Flush(); //Temp Comment
                        continue;
                    } //if
                    break;
                } //if
                System.Web.HttpContext.Current.Response.BinaryWrite(array);
                System.Web.HttpContext.Current.Response.Flush(); //Temp Comment
            }//while
            readerStream.Close();
        }//try
        catch (System.Exception e)
        {
            error = e.Message;
            cancelStreaming = true;
        }//catch
        return !cancelStreaming;
    }//OutputFile

    private long GetFileLength(string filePathAndName)
    {
        long length = -1;
        try
        {
            var fullPath = ResolveFilePath(filePathAndName);
            string actualFileName = HttpUtility.UrlDecode(fullPath);
            length = new System.IO.FileInfo(actualFileName).Length;
        }
        catch { }
        return length;
    }
    private OM.UserProfile CurrentUserProfile()
    {
        FrameworkSession session = FrameworkManagerUtil.GetFrameworkSession(Page.Session);
        return session.CurrentUserProfile;
    }//CurrentUserProfile

    private bool SetAttachmentsID(ref OM.DocAttachmentsTxn data, string attachmentsID)
    {
        bool eventSaved = false;

        if (!string.IsNullOrEmpty(attachmentsID))
        {
            OM.DocAttachments dAttachments = new Camstar.WCF.ObjectStack.DocAttachments();
            dAttachments.Self = new OM.BaseObjectRef(attachmentsID);
            data.Attachments = dAttachments;
            eventSaved = true;
        }//if
        return eventSaved;
    }//SetAttachmentsID 

    private bool QueryStringViolatesSecurityPolicy()
    {
        if (QueryStringHasSpecialSymbols()
            && QueryStringDoesntContainsURL()
            || QueryStringContainsFilePathManipulators())
        {
            return true;
        }
        return false;
    }

    private bool QueryStringHasSpecialSymbols()
    {
        return Request.QueryString.ToString().Contains("%");
    }

    private bool QueryStringDoesntContainsURL()
    {
        return !(Request.Url.ToString().Contains("docMaintURL="));
    }

    private bool QueryStringContainsFilePathManipulators()
    {
        return Request.QueryString.ToString().Contains("..");
    }

    private void RegisterAlertWhenQueryStringViolatesSecurityPolicy()
    {
        ClientScript.RegisterStartupScript(typeof(string), "fileIsNotLoaded",
                    string.Format("parent.alert('{0}');",
                    "The query string of the request violates the security policy of the application"),
                    true);
    }
    [WebMethod]
    public static void SaveChart(string html, string name)
    {
        var fs = FrameworkManagerUtil.GetFrameworkSession();
        var objSvc = new QueryService(fs.CurrentUserProfile);
        var objOptions = new OM.QueryOptions();
        objOptions.QueryType = OM.QueryType.User;
        objOptions.StartRow = 1;
        string _strInlineSPCOutputFolder = @"Camstar\InlineSPC\Chart Output";
        string strSPCDailyFolder = string.Format("{0:yyyyMMdd}", DateTime.Now);
        string strNewSPCFolder;
        var objRS = objSvc.ExecuteAdHoc("Select TValue From InsiteSiteInfo Where TName = 'PathBox'", objOptions, out var recordSet);

        if (objRS != null && objRS.IsSuccess && recordSet.Rows!=null && recordSet.Rows.Length > 0)
        {
            strNewSPCFolder = recordSet?.Rows[0]?.Values[0]+@"\"+strSPCDailyFolder;
        }  
        else
        {      
            strNewSPCFolder = System.Environment.GetFolderPath(System.Environment.SpecialFolder.CommonApplicationData) + @"\" + _strInlineSPCOutputFolder + @"\" + strSPCDailyFolder;
        }
        try
        {
            if (!Directory.Exists(strNewSPCFolder))
            {
                Directory.CreateDirectory(strNewSPCFolder);
            }
        }
        catch (Exception )
        {
        }
        string subDir = string.Format(@"{0:HHmmssFFF}", DateTime.Now);
        string relativeOutputFileDir = string.Format(@"{0}/{1}.htm", strSPCDailyFolder, name);
        string physicalOutputFileDir = System.IO.Path.Combine(strNewSPCFolder);

        System.IO.File.WriteAllText(string.Format(@"{0}\{1}_{2}.htm", physicalOutputFileDir, subDir, name), html);
       
    }
}
