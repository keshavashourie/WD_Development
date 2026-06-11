using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Siemens.PLM.SDK.Lifecycle.Design;
using Siemens.PLM.SDK.Lifecycle.Design.Model;
using System.Threading.Tasks;
using System.Reflection;
using Camstar.WebPortal.FormsFramework.Utilities;
using Siemens.PLM.SDK.Core.Client;

/// <summary>
/// Summary description for McadClient
/// </summary>

namespace Camstar.WebPortal.WebPortlets.XceleratorShare
{
    public class McadClient : IDisposable
    {
        private ILifecycleDesignClient mcadClient;
        private bool disposed = false;
        public McadClient(string accessKey, string secretAccessKey)
        {
            try
            {
                var clientConfiguration = new LifecycleDesignConfiguration ()
                {
                    ACCESS_KEY_ID = accessKey,
                    SECRET_ACCESS_KEY = secretAccessKey,
                    Design = LCSSetting.MCAD
                };
                mcadClient = LifecycleDesignClientFactory.GetLifecycleDesignClient(clientConfiguration);

            }
            catch (Exception)
            {
                mcadClient = null;
                throw new MCADDesignException(ErrorMessage.MCADClientConnectionError);
            }
                      
        }

        ~McadClient()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
            {
                return;
            }
            mcadClient = null;
            disposed = true;
        }

        public CreateDesignOutput CreateDesignAsync(string collaborationspaceId, string containerType, string containerId, CreateDesignInput createDesignInput)
        {
            try
            {
                var result = new BulkApiResponse<CreateDesignOutput>();
                var taskStat = Task.Run(async () => result = await mcadClient.CreateDesignsAsync(collaborationspaceId, containerType, containerId, createDesignInput));
                taskStat.Wait();

                if (result is null || result.Results is null || result.Results.First().Data is null || result.Results.First().Data.Attachments is null)
                {
                    throw new MCADDesignException(MethodBase.GetCurrentMethod().Name + "  " + ErrorMessage.MCADcreateDesignValidateOutputError);
                }
                else if (result.ErrorIndices.Any())
                {
                    throw new MCADDesignException(MethodBase.GetCurrentMethod().Name + "  " + ErrorMessage.MCADcreateDesignError);
                }

                return result.Results.First().Data;

            }
            catch (Exception exception)
            {
               
                throw exception;
            }

        }

        public string DeleteDesignAsync(string collaborationspaceId, string containerType, string containerId, string urn)
        {
            try
            {
                List<string> Urns = new List<string> { urn };
                DeleteDesignRevisionsInput deleteDesignRevisionsInput = new DeleteDesignRevisionsInput(null, Urns);
                var result = new DeleteDesignRevisionsOutput();
                var taskStat = Task.Run(async () => result = await mcadClient.DeleteDesignRevisionsAsync(collaborationspaceId, containerType, containerId, deleteDesignRevisionsInput));
                taskStat.Wait();
                return result.TaskId;
            }
            catch (Exception)
            {
                throw new MCADDesignException(MethodBase.GetCurrentMethod().Name + "  " + ErrorMessage.MCADdeleteDesignError);
            }
        }

        public string GetDownloadTicket(string collabspaceId, string containerType, string containerId, string urn)
        {
            GetDesignRevisionsInput getDownloadTicketInput = new GetDesignRevisionsInput
            {
                AttachmentTypesToReturn = new List<string> { "Master" },
                Urns = new List<string> { urn },
                FetchDownloadTicket = true,
                DownloadTicketType = "Standard",
                OutputAdhocProperties = false,
                OutputProperties = new List<string> { "*" }
            };
            string downloadTicket = string.Empty;
            var response = this.mcadClient.GetDesignRevisions(collabspaceId, containerType, containerId, getDownloadTicketInput);
            if (response.Results[0].Errors?.Count > 0)
            {
                if (response.Results[0].Errors.First().ErrorCode.Equals(ErrorMessage.RevisionNotFound))
                {
                    throw new Exception(ErrorMessage.FileNotFound);
                }
                throw new Exception(response.Results[0].Errors.First().ErrorCode);
            }
            return response.Results[0].Data.Attachments[0].DownloadTicket;
        }
    }
}