// © Siemens 2021
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Web;

using Camstar.WCF.ObjectStack;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.Utilities;
using Camstar.WebPortal.PortalConfiguration;
using Camstar.WCF.Services;
using Camstar.WebPortal.Constants;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.Personalization;
using Camstar.Util;

namespace Camstar.Portal
{

    /// <summary>
    /// Summary description for DocumentService
    /// </summary>
    [ServiceContract(Namespace = "")]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public partial class DocumentService
    {
        [OperationContract]
        public byte[] LoadDocument(string sessionID, string userName, string documentID, string name, string revision, out HttpStatusCode status, out string mimeType)
        {
            //const string ProcedureName = "LoadDocumentID";
            byte[] data = null;
            status = HttpStatusCode.OK;
            mimeType = string.Empty;
            try
            {
                if (!string.IsNullOrEmpty(documentID) || !string.IsNullOrEmpty(name))
                {
                    string folder = GetTempDownloadFolder();

                    DocumentMaintService svc = new DocumentMaintService(GetUserProfile(sessionID, userName));
                    DocumentMaint_Result result = new DocumentMaint_Result();
                    DocumentMaint_Request request = new DocumentMaint_Request();

                    DocumentMaint maint = new DocumentMaint();
                    maint.ObjectToChange = new RevisionedObjectRef();
                    maint.ObjectChanges = new DocumentChanges();
                    maint.ObjectChanges.FileLocation = folder;
                    if (!string.IsNullOrEmpty(documentID))
                    {
                        maint.ObjectToChange.ID = documentID;
                        maint.ObjectChanges.Identifier = documentID;
                    }
                    else
                    {
                        maint.ObjectToChange.Name = name;
                        maint.ObjectChanges.Name = name;
                        if (!string.IsNullOrEmpty(revision))
                        {
                            maint.ObjectToChange.Revision = revision;
                            maint.ObjectChanges.Revision = revision;
                        }
                        else
                        {
                            maint.ObjectChanges.IsRevOfRcd = true;
                            maint.ObjectToChange.RevisionOfRecord = true;
                        }
                    }
                    request.Info = new DocumentMaint_Info();
                    request.Info.RequestValue = true;

                    ResultStatus resStatus = svc.DownloadFile(maint, request, out result);
                    if (resStatus.IsSuccess)
                    {
                        data = ReadFile(folder, out mimeType);
                    }
                    else
                    {
                        status = HttpStatusCode.NoContent;
                        System.IO.Directory.Delete(folder);
                    }
                }
            }
            catch //(System.Exception e)
            {
                status = HttpStatusCode.InternalServerError;
            }
            return data;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sessionid"
        /// <param name="name"></param>
        /// <param name="revision"></param>
        /// <param name="status"></param>
        /// <param name="mimeType"></param>
        /// <returns></returns>
        public byte[] LoadImage(string sessionid, string userName, string name, string revision, out HttpStatusCode status, out string mimeType)
        {
            byte[] data = null;
            status = HttpStatusCode.OK;
            mimeType = string.Empty;

            try
            {
                string rev = string.IsNullOrEmpty(revision) ? "RevisionOfRecord" : revision;
                string folder = GetTempDownloadFolder();

                isImageMaintService svc = new isImageMaintService(GetUserProfile(sessionid, userName));
                isImageMaint_Result result = new isImageMaint_Result();
                isImageMaint_Request request = new isImageMaint_Request();

                isImageMaint maint = new isImageMaint();
                maint.ObjectToChange = new RevisionedObjectRef();
                maint.ObjectChanges = new isImageChanges();
                maint.ObjectChanges.FileLocation = folder;
                maint.ObjectToChange.Name = name;
                maint.ObjectChanges.Name = name;
                if (!string.IsNullOrEmpty(revision))
                {
                    maint.ObjectToChange.Revision = revision;
                    maint.ObjectChanges.Revision = revision;
                }
                else
                {
                    maint.ObjectChanges.IsRevOfRcd = true;
                    maint.ObjectToChange.RevisionOfRecord = true;
                }

                request.Info = new isImageMaint_Info();
                request.Info.RequestValue = true;

                ResultStatus resStatus = svc.DownloadFile(maint, request, out result);
                if (resStatus.IsSuccess)
                {
                    data = ReadFile(folder, out mimeType);
                }
                else
                {
                    maint.ObjectToChange.Revision = string.Empty;
                    maint.ObjectChanges.Revision = string.Empty;
                    maint.ObjectChanges.IsRevOfRcd = true;
                    maint.ObjectToChange.RevisionOfRecord = true;

                    resStatus = svc.DownloadFile(maint, request, out result);
                    if (resStatus.IsSuccess)
                    {
                        data = ReadFile(folder, out mimeType);
                    }
                    else
                    {
                        status = HttpStatusCode.NoContent;
                        Directory.Delete(folder);
                    }
                }
            }
            catch
            {
                status = HttpStatusCode.InternalServerError;
            }
            return data;
        }
    
    #region Protected Methods
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    protected UserProfile GetUserProfile(string sessionID, string userName)
        {
            UserProfile profile = new UserProfile();
            profile.Name = userName;
            profile.SessionID = new EncryptedField(sessionID, true);
            profile.UTCOffset = DateTimeOffset.Now.Offset;
            return profile;
        }

        /// <summary>
        /// Create a generated-name folder under the configured download folder
        /// </summary>
        /// <returns></returns>
        protected string GetTempDownloadFolder()
        {
            // Get/create download folder configured in AppSettings
            string downloadFolder = Utilities.ValidPath(Camstar.WebPortal.Utilities.CamstarPortalSection.Settings.DefaultSettings.UploadDirectory);
            if (!System.IO.Directory.Exists(downloadFolder))
            {
                System.IO.Directory.CreateDirectory(downloadFolder);
            }

            // Create temp folder under the download folder
            string tempFolder = string.Format(@"{0}{1}", downloadFolder, System.Guid.NewGuid());
            if (!System.IO.Directory.Exists(tempFolder))
            {
                System.IO.Directory.CreateDirectory(tempFolder);
            }
            return tempFolder;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="directory"></param>
        /// <returns></returns>
        /// <see cref="ReadFile(string, out string)"/>
        protected byte[] ReadFile(string directory)
        {
            string mimeType = string.Empty;
            return ReadFile(directory, out mimeType);
        }

        /// <summary>
        /// Return the binary contents of the first file in the given directory.  Delete the first file and given directory after reading.
        /// </summary>
        /// <param name="directory"></param>
        /// <param name="mimeType"></param>
        /// <returns></returns>
        protected byte[] ReadFile(string directory, out string mimeType)
        {
            byte[] data = null;
            mimeType = Utilities.GetMimeType(string.Empty);
            string[] files = Directory.GetFiles(directory);
            if (files.Length > 0)
            {
                FileStream streamReader = null;
                try
                {
                    mimeType = Utilities.GetMimeType(Utilities.GetFileExtension(files[0]));
                    streamReader = new FileStream(files[0], FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    using (BinaryReader reader = new BinaryReader(streamReader))
                    {
                        data = reader.ReadBytes((int)streamReader.Length);
                        //streamReader = null;
                    }
                }
                finally
                {
                    if (streamReader != null)
                    {
                        streamReader.Dispose();
                    }
                }
                File.Delete(files[0]);
            }

            Directory.Delete(directory, true);

            return data;
        }
        #endregion Protected Methods
    }
}