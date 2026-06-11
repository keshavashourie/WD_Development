using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using Siemens.PLM.SDK.Lifecycle.Collab;
using Siemens.PLM.SDK.Lifecycle.Collab.Model;
using System.Configuration;
using System.Reflection;
using Newtonsoft.Json.Linq;
using Camstar.WebPortal.FormsFramework.Utilities;

/// <summary>
/// Namespace responsible for communication with XceleratorShare APIs
/// </summary>
namespace Camstar.WebPortal.WebPortlets.XceleratorShare
{
    public class LCSClient
    {
        private ILifecycleCollabClient lcsClient;

        public LCSClient(string accesskey, string secretAccessKey)
        {
            var clientConfiguration = new LifecycleCollabConfiguration()
            {
                ACCESS_KEY_ID = accesskey,
                SECRET_ACCESS_KEY = secretAccessKey,
                Collab = LCSSetting.COLLAB
            };
            this.lcsClient = LifecycleCollabClientFactory.GetLifecycleCollabClient(clientConfiguration);
        }

        public string GetCollaborationSpaceAsync()
        {
            var result = new List<CollaborationspaceInfo>();
            var taskStat = Task.Run(async () => result = await lcsClient.ListCollaborationspacesAsync());
            taskStat.Wait();

            if (result.Count > 0)
            {
                return result.FirstOrDefault().CollaborationspaceId;
            }
            else
            {
                throw new CollabhubClientException(MethodBase.GetCurrentMethod().Name + "  " + ErrorMessage.LCSClientConnctionError);
            }
        }


        public List<ListProjectOutput> ListProjectAsync(string collaborationspaceId, List<string> roleList = null, List<SortField> sortFields = null,
            List<string> outputProperties = null)
        {
            List<ListProjectOutput> projects = new List<ListProjectOutput>();
            try
            {
                bool getProjects = true;
                int limit = 50;
                int skip = 0;
                if (outputProperties == null)
                {
                    outputProperties = new List<string> { "*" };
                }
                if (sortFields == null)
                {
                    sortFields = new List<SortField>()
                        {
                            new SortField()
                            {
                                FieldName = "name",
                                SortDirection = SortDirection.Ascending
                            }
                        };
                }
                while (getProjects)
                {
                    var input = new ListProjectsInput
                    {
                        RoleFilters = roleList,
                        SearchOptions = new SearchOptionsForContainer
                        {
                            Limit = limit,
                            Skip = skip,
                            SortFields = sortFields,
                            OutputProperties = outputProperties
                        }
                    };
                    var result = new ListProjectsOutput();
                    var taskStat = Task.Run(async () => result = await lcsClient.ListProjectsAsync(collaborationspaceId, input));
                    taskStat.Wait();
                    projects.AddRange(result.Projects.ToList());
                    getProjects = result.Projects.Count == limit;
                    skip += limit;
                }
            }
            catch (Exception ex)
            {
                throw new CollabhubClientException(MethodBase.GetCurrentMethod().Name + "  " + ErrorMessage.LCSProjectLoadingError + " : " + ex.Message);
            }
            return projects;
        }

        public List<XShareDocument> ListProjectContentAsync(string collabspaceId, string projectId, string parentFolderId)
        {
            try
            {
                List<XShareDocument> documents = new List<XShareDocument>();
                SearchObjectsOutput result = new SearchObjectsOutput();
                var taskStat = Task.Run(async () => result = await lcsClient.ListContents2Async(collabspaceId, "project", projectId,
                    new ListContents2Input()
                    {
                        Folder = parentFolderId,
                        OutputProperties = new List<string> { "*" }
                    }
                ));
                taskStat.Wait();

                foreach (var item in result.Results)
                {
                    documents.Add(item.ToObject<XShareDocument>());
                }
                return documents;
            }
            catch (Exception exception)
            {
                throw new ListProjectContentExceptioin(exception.Message);
            }
        }
    }
}