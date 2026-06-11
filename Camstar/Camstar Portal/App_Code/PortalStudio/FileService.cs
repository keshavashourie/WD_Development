// Copyright Siemens 2023  
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.ServiceModel;
using Camstar.WebPortal.Personalization;
using Camstar.WebPortal.Utilities;
using Camstar.WebPortal.PortalConfiguration;

namespace WebClientPortal
{
    /// <summary>
    /// This functionality is for upload/download of small files. It does not intend to stream large files (Silverlight does not support streamed transport).
    /// </summary>
    public partial class PortalStudioService
    {
        [OperationContract]
        public virtual Camstar.WCF.ObjectStack.ResultStatus Upload(string filename, string directory, byte[] content)
        {
            bool status = false;
            string message = string.Empty;

            if (IsSessionValid() && VerifyRBACPermission())
            {
                try
                {
                    if (filename.IndexOf("settings.xml", 0, StringComparison.InvariantCultureIgnoreCase) != -1)
                    {
                        // Update intelligence settings - the password must be crypted
                        bool contentChanged = false;
                        var settings = CamstarPortalSection.GetPortalSettingsFromBytes(content);
                        if (settings != null && settings.IntelligenceSettings != null)
                        {
                            var intSettings = settings.IntelligenceSettings;
                            if (intSettings.DataSources != null)
                            {
                                intSettings.DataSources.ToList().ForEach(ds =>
                                {
                                    if (!string.IsNullOrEmpty(ds.LoginPasswordDecrypted))
                                    {
                                        ds.LoginPassword = Camstar.Util.CryptUtil.Encrypt(ds.LoginPasswordDecrypted);
                                        ds.LoginPasswordDecrypted = null;
                                        contentChanged = true;
                                    }
                                });
                            }
                        }
                        if( contentChanged )
                        {
                            content = CamstarPortalSection.GetBytesFromPortalSettings(settings);
                        }
                    }

                    var file = directory + "/" + Camstar.WebPortal.Constants.FolderConstants.UserResource + "/" + filename;
                    File.WriteAllBytes(HttpContext.Current.Server.MapPath(file), content);
                    status = true;
                }
                catch (Exception e)
                {
                    message = e.Message;
                }
            }

            return new Camstar.WCF.ObjectStack.ResultStatus(message, status);
        }

        [OperationContract]
        public virtual Camstar.WCF.ObjectStack.ResultStatus Download(string filename, string directory, out byte[] content)
        {
            bool status = false;
            string message = string.Empty;
            content = null;
            if (IsSessionValid() && VerifyRBACPermission())
            {
                try
                {
                    string file = directory + "/" + Camstar.WebPortal.Constants.FolderConstants.UserResource + "/" + filename;
                    content = File.ReadAllBytes(HttpContext.Current.Server.MapPath(file));
                    status = true;
                }
                catch (Exception e)
                {
                    message = e.Message;
                }
            }

            return new Camstar.WCF.ObjectStack.ResultStatus(message, status);
        }

        [OperationContract]
        public virtual Camstar.WCF.ObjectStack.ResultStatus GetImages(out ImageFile[] images)
        {
            bool status = false;
            string message = string.Empty;
            images = new ImageFile[0];

            if (IsSessionValid())
            {
                try
                {
                    List<ImageFile> imagelist = new List<ImageFile>();
                    foreach (string file in Directory.GetFiles(HttpContext.Current.Server.MapPath(Camstar.WebPortal.Constants.FolderConstants.Images + "/" + Camstar.WebPortal.Constants.FolderConstants.UserResource)))
                    {
                        string name = System.IO.Path.GetFileName(file);
                        ImageFile image = new ImageFile() { Name = name };
                        image.Source = Camstar.WebPortal.Constants.FolderConstants.Images + "/" + Camstar.WebPortal.Constants.FolderConstants.UserResource + "/" + name;
                        imagelist.Add(image);
                    }
                    images = imagelist.ToArray();
                    status = true;
                }
                catch (Exception e)
                {
                    message = e.Message;
                }
            }

            return new Camstar.WCF.ObjectStack.ResultStatus(message, status);
        }

        [OperationContract]
        public virtual Camstar.WCF.ObjectStack.ResultStatus GetSounds(out ImageFile[] sounds)
        {
            bool status = false;
            string message = string.Empty;
            sounds = new ImageFile[0];
            string noteImage = string.Format("{0}/Icons/note.png", Camstar.WebPortal.Constants.FolderConstants.Images);
            if (IsSessionValid())
            {
                try
                {
                    var userSoundsDir = string.Format("{0}/{1}", Camstar.WebPortal.Constants.FolderConstants.Sounds, Camstar.WebPortal.Constants.FolderConstants.UserResource);
                    sounds = Directory.GetFiles(HttpContext.Current.Server.MapPath(userSoundsDir)).Select(Path.GetFileName).Select(name => new ImageFile
                        {
                            Name = name,
                            Source = noteImage
                        }).ToArray();
                    status = true;
                }
                catch (Exception e)
                {
                    message = e.Message;
                }
            }
            return new Camstar.WCF.ObjectStack.ResultStatus(message, status);
        }
    }
}
