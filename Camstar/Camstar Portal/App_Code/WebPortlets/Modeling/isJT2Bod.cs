using Camstar.WCF.ObjectStack;
using Camstar.WCF.Services;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.Utilities;
using Camstar.WebPortal.WCFUtilities;
using System;
using System.IO;
using System.Web;
using Path = System.IO.Path;
using Camstar.WebPortal.WebPortlets;


/// <summary>
/// Load JT files using the isImageMaint service. This class integrates with the
/// JtToBodConvertHandler class to provide custom image loading. The WorkspaceExtenderFactory
/// will instantiate this class.
/// </summary>
/// <seealso cref="WorkspaceExtenderFactory"/>
[WorkspaceExtender(WorkspaceCode.Industry)]
public class isJtToBodConvertHandlerExtension : JtToBodConvertHandlerExtender
{
    protected UserProfile CurrentUserProfile(HttpContext context)
    {
        var session = FrameworkManagerUtil.GetFrameworkSession(context.Session);
        return session.CurrentUserProfile;
    }

    public override DocumentRefInfo GetDocInfo(RevisionedObjectRef docRef, HttpContext context, out ResultStatus txnResult)
    {
        DocumentRefInfo info = GetImageInfo(docRef.Name, docRef.Revision, CurrentUserProfile(context), out txnResult);
        return info;
    }

    private DocumentRefInfo GetImageInfo(string imageName, string imageRev, UserProfile profile, out ResultStatus resultStatus)
    {
        var docInfo = new DocumentRefInfo();
        var imageRef = new RevisionedObjectRef(imageName, imageRev);

        if (string.IsNullOrEmpty(imageRev))
            imageRef.RevisionOfRecord = true;

        docInfo.DocumentRef = imageRef;
        resultStatus = new ResultStatus();

        if (profile != null)
        {
            var data = new isImageMaint { ObjectToChange = imageRef };
            var request = new isImageMaint_Request
            {
                Info = new isImageMaint_Info
                {
                    ObjectChanges = new isImageChanges_Info
                    {
                        Identifier = new Info(true),
                        AuthenticationType = new Info(true),
                        BrowseMode = new Info(true)
                    }
                }
            };

            isImageMaint_Result result;
            var imageMaintService = new isImageMaintService(profile);
            resultStatus = imageMaintService.Load(data, request, out result);

            if (resultStatus.IsSuccess)
            {
                var authType = AuthenticationTypeEnum.None;
                if (result.Value.ObjectChanges.AuthenticationType != null)
                    authType = (AuthenticationTypeEnum)result.Value.ObjectChanges.AuthenticationType;

                docInfo.URI = (string)result.Value.ObjectChanges.Identifier;
                docInfo.AuthenticationType = authType;
                docInfo.Credentials = GetCredentials(authType, profile);
                if (result.Value.ObjectChanges.BrowseMode != null)
                    docInfo.BrowseMode = (BrowseModeEnum)result.Value.ObjectChanges.BrowseMode;
            }
        }

        return docInfo;
    }

    public override void DownloadDocumentLocal(DocumentRefInfo docInfo, string filePath, HttpContext context)
    {
        var updloadFolder = CamstarPortalSection.Settings.DefaultSettings.UploadDirectory;
        if (Directory.Exists(updloadFolder))
        {
            var dirInfo = new DirectoryInfo(updloadFolder);
            var imageData = new isImageMaint { ObjectChanges = new isImageChanges { FileLocation = dirInfo.FullName } };
            var session = FrameworkManagerUtil.GetFrameworkSession(context.Session);
            var xInfo = new isImageChanges_Info();
            var fileName = string.Empty;
            var data = new isImageMaint { ObjectToChange = docInfo.DocumentRef };
            var request = new isImageMaint_Request
            {
                Info = new isImageMaint_Info { ObjectChanges = new isImageChanges_Info { FileName = new Info(true) } }
            };

            isImageMaint_Result result;
            var serv = new isImageMaintService(session.CurrentUserProfile);
            serv.BeginTransaction();
            serv.Load(data);
            serv.DownloadFile(imageData);
            var resultStatus = serv.CommitTransaction(request, out result);

            if (resultStatus.IsSuccess && result != null)
                fileName = result.Value.ObjectChanges.FileName != null ? result.Value.ObjectChanges.FileName.Value : null;
            if (string.IsNullOrEmpty(fileName))
                fileName = docInfo.FileName;

            var uploadedFilePath = Path.Combine(dirInfo.FullName, fileName);
            File.Copy(uploadedFilePath, filePath);
        }
    }

    public string GetCredentials(AuthenticationTypeEnum authenticationType, UserProfile profile)
    {
        string credentials = string.Empty;
        if (authenticationType == AuthenticationTypeEnum.Basic)
        {
            string userName = string.Empty;
            string userPwd = string.Empty;

            if (profile != null)
            {
                userName = profile.Name;
                if (HttpContext.Current != null)
                {
                    var currentUserPassword = profile.Password;
                    userPwd = EncryptedField.GetPlainValue(currentUserPassword);
                }

                var employee = new EmployeeMaint { ObjectToChange = new NamedObjectRef(userName) };
                var request = new EmployeeMaint_Request
                {
                    Info = new EmployeeMaint_Info
                    {
                        ObjectChanges = new EmployeeChanges_Info
                        {
                            DocManagerUser = new Info(true),
                            DocManagerPassword = new Info(true)
                        }
                    }
                };

                var employeeMaintService = new EmployeeMaintService(profile);
                EmployeeMaint_Result result;
                var resultStatus = employeeMaintService.Load(employee, request, out result);
                if (resultStatus.IsSuccess)
                {
                    if (result.Value.ObjectChanges.DocManagerUser != null)
                    {
                        if (!string.IsNullOrEmpty(result.Value.ObjectChanges.DocManagerUser.Value))
                            userName = result.Value.ObjectChanges.DocManagerUser.Value;

                        if (result.Value.ObjectChanges.DocManagerPassword != null)
                        {
                            if (!string.IsNullOrEmpty(result.Value.ObjectChanges.DocManagerPassword.Value))
                            {
                                var encrypted =
                                    new EncryptedField(result.Value.ObjectChanges.DocManagerPassword.Value, true);
                                userPwd = EncryptedField.GetPlainValue(encrypted);
                            }
                        }
                    }
                }
            }

            credentials = Convert.ToBase64String(System.Text.Encoding.Default.GetBytes(string.Format("{0}:{1}", userName, userPwd)));
        }

        return credentials;
    }

}
