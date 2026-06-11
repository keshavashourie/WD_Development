<%-- Copyright Siemens 2023   --%>
<%@ WebHandler Language="C#" Class="JtToBodConvertHandler" %>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Web;
using Camstar.WCF.ObjectStack;
using Camstar.WCF.Services;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.Utilities;
using Camstar.WebPortal.WCFUtilities;
using Environment = System.Environment;
using Path = System.IO.Path;

public class JtToBodConvertHandler : IHttpHandler, System.Web.SessionState.IRequiresSessionState
{
    #region IHttpHandler Members

    bool IHttpHandler.IsReusable
    {
        get { return false; }
    }

    void IHttpHandler.ProcessRequest(HttpContext context)
    {
        var jobID = Guid.NewGuid();
        string filePath = string.Empty;

        string modelsDirPath = context.Request.MapPath(@"CamstarUploads\Models\");

        if (!Directory.Exists(modelsDirPath))
            Directory.CreateDirectory(modelsDirPath);

        if (context.Request.Files.Count > 0)
        {
            // retrieve the posted csv file
            var jtFile = context.Request.Files["jt"];

            // save the file to disk so the CMD line util can access it
            filePath = Path.Combine(modelsDirPath, string.Format("{0:n}.jt", jobID));
            jtFile.SaveAs(filePath);
        }
        else
        {
            var docName = context.Request["docName"];
            var docRev = context.Request["docRev"];
            bool hasCustomWorkspaceLogic = false;
            JtToBodConvertHandlerExtender workspaceCustomLogic = null;

            if (!string.IsNullOrEmpty(docName))
            {
                var docRef = new RevisionedObjectRef(docName);
                if (!string.IsNullOrEmpty(docRev))
                    docRef.Revision = docRev;
                else
                    docRef.RevisionOfRecord = true;

                var result = new ResultStatus();
                var docInfo = GetDocInfo(docRef, context, out result);
                if (result.IsSuccess == false)
                {
                    workspaceCustomLogic = Camstar.WebPortal.WebPortlets.WorkspaceExtenderFactory.GetImplementation<JtToBodConvertHandlerExtender>();
                    if (workspaceCustomLogic != null)
                    {
                        docInfo = workspaceCustomLogic.GetDocInfo(docRef, context, out result);
                        hasCustomWorkspaceLogic = true;
                    }
                }

                if (result.IsSuccess)
                {
                    filePath = Path.Combine(modelsDirPath, string.Format("{0:n}.jt", jobID));
                    if (docInfo.IsRemote)
                        DownloadDocumentRemote(docInfo, filePath, context);
                    else if (hasCustomWorkspaceLogic) // local
                        workspaceCustomLogic.DownloadDocumentLocal(docInfo, filePath, context);
                    else // local
                        DownloadDocumentLocal(docInfo, filePath, context);
                }
            }
        }

        if (string.Equals(Path.GetExtension(filePath), ".jt", StringComparison.CurrentCultureIgnoreCase))
        {
            var dirName = Path.Combine(modelsDirPath, string.Format("{0:n}", jobID));
            Directory.CreateDirectory(dirName);

            var psi = new ProcessStartInfo(context.Request.MapPath("Tools/jt2bod/jt2bod.exe"),
                string.Format("-noLog \"{0}\" \"{1}\"", filePath, dirName))
            {
                WorkingDirectory = Environment.CurrentDirectory,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using (var process = new Process { StartInfo = psi })
            {
                // delegate for writing the process output to the response output
                Action<Object, DataReceivedEventArgs> dataReceived = ((sender, e) =>
                {
                    if (e.Data != null)
                    // sometimes a random event is received with null data, not sure why - I prefer to leave it out
                    {
                        //context.Response.Write(e.Data);
                        //context.Response.Write(Environment.NewLine);
                        //context.Response.Flush();
                    }
                });

                process.OutputDataReceived += new DataReceivedEventHandler(dataReceived);
                process.ErrorDataReceived += new DataReceivedEventHandler(dataReceived);

                // use text/plain so line breaks and any other whitespace formatting is preserved
                context.Response.ContentType = "text/plain";

                // start the process and start reading the standard and error outputs
                process.Start();
                process.BeginErrorReadLine();
                process.BeginOutputReadLine();

                // wait for the process to exit
                process.WaitForExit();

                if (context.Session["jtResources"] == null)
                    context.Session["jtResources"] = new JtResourcesList();

                var jtResources = context.Session["jtResources"] as JtResourcesList;
                jtResources.Add(string.Format("{0:n}", jobID));

                // an exit code other than 0 generally means an error
                if (process.ExitCode != 0)
                    context.Response.StatusCode = 500;
                else
                    context.Response.Write(string.Format("{0:n}", jobID));
            }
        }
    }

    private DocumentRefInfo GetDocInfo(RevisionedObjectRef docRef, HttpContext context, out ResultStatus txnResult)
    {
        return AttachmentExecutor.GetDocumentInfo(docRef.Name, docRef.Revision, CurrentUserProfile(context), out txnResult);
    }

    private ResultStatus DownloadDocumentRemote(DocumentRefInfo docInfo, string filePath, HttpContext context)
    {
        var txnResult = new ResultStatus();
        var auth = docInfo.AuthenticationType;
        var request = (HttpWebRequest)WebRequest.Create(docInfo.URI);

        if (auth == AuthenticationTypeEnum.Basic)
        {
            if (string.IsNullOrEmpty(docInfo.Credentials))
                docInfo.Credentials = AttachmentExecutor.GetDocCredentials(auth, CurrentUserProfile(context));

            request.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true; // ignores client SSL certificates.
            request.PreAuthenticate = true;
            request.Headers.Add("Authorization", "Basic " + docInfo.Credentials);
        }

        using (var response = request.GetResponse())
        {
            using (var readerStream = response.GetResponseStream())
            {
                if (readerStream != null)
                {
                    using (var destFileStream = File.Create(filePath, 1024))
                        readerStream.CopyTo(destFileStream, 1024);
                }
            }
        }

        return txnResult;
    }

    private void DownloadDocumentLocal(DocumentRefInfo docInfo, string filePath, HttpContext context)
    {
        var configFolder = CamstarPortalSection.Settings.DefaultSettings.UploadDirectory;
        if (Directory.Exists(configFolder))
        {
            var dirInfo = new DirectoryInfo(configFolder);
            var data1 = new DocumentMaint { ObjectChanges = new DocumentChanges { FileLocation = dirInfo.FullName } };
            var session = FrameworkManagerUtil.GetFrameworkSession(context.Session);

            var fileName = string.Empty;
            var data = new DocumentMaint { ObjectToChange = docInfo.DocumentRef };
            var request = new DocumentMaint_Request
            {
                Info = new DocumentMaint_Info {ObjectChanges = new DocumentChanges_Info {FileName = new Info(true)}}
            };
            DocumentMaint_Result result;
            var serv = new DocumentMaintService(session.CurrentUserProfile);
            serv.BeginTransaction();
            serv.Load(data);
            serv.DownloadFile(data1);
            var resultStatus = serv.CommitTransaction(request, out result);

            if (resultStatus.IsSuccess && result != null)
                fileName = result.Value.ObjectChanges.FileName != null ? result.Value.ObjectChanges.FileName.Value : null;
            if (string.IsNullOrEmpty(fileName))
                fileName = docInfo.FileName;

            var uploadedFilePath = Path.Combine(dirInfo.FullName, fileName);
            File.Copy(uploadedFilePath, filePath);
        }
    }

    private UserProfile CurrentUserProfile(HttpContext context)
    {
        var session = FrameworkManagerUtil.GetFrameworkSession(context.Session);
        return session.CurrentUserProfile;
    }

    public class JtResourcesList : List<string>, IDisposable
    {
        public void Dispose()
        {
            if (Count > 0)
            {
                var jtStoragePath = HttpContext.Current.Request.MapPath(@"CamstarUploads\Models\");
                if (Directory.Exists(jtStoragePath))
                {
                    foreach (var jtItem in this)
                    {
                        var dirPath = Path.Combine(jtStoragePath, jtItem);
                        var filePath = (Path.Combine(jtStoragePath, string.Format("{0}.jt", jtItem)));

                        if (Directory.Exists(dirPath))
                            Directory.Delete(dirPath, true);
                        if (File.Exists(filePath))
                            File.Delete(filePath);
                    }
                }
            }
        }
    }

    #endregion
}
