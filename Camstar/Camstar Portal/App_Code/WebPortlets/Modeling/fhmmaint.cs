//Copyright Siemens 2023
using Camstar.WCF.ObjectStack;
using Camstar.WebPortal.Constants;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using WebClientPortal;

namespace Camstar.WebPortal.WebPortlets.Modeling
{

    /// <summary>
    /// Maint pages for objects found in the Factory Hierarchy Model
    /// </summary>
    public class FhmMaint : SwacPageBase
    {
        protected SwacPageBase SwacWP { get { return Page.FindCamstarControl("SwacWP") as SwacPageBase; } }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // Set DataContract value to track what button (Action) triggered the postback
            if (Page.EventArgument == ("CopyBtn" + EventArgumentConstants.ArgumentDelimeter + EventArgumentConstants.UIAction))
            {
                Page.PortalContext.DataContract.SetValueByName("ActionName", "Copy");
            }
            else if (Page.EventArgument == ("DeleteBtn" + EventArgumentConstants.ArgumentDelimeter + EventArgumentConstants.UIAction))
            {
                Page.PortalContext.DataContract.SetValueByName("ActionName", "Delete");
            }
        }

        /// <summary>
        /// Use SRC to Remove Resource Settings associated with the given Resource
        /// </summary>
        /// <param name="resourceId"></param>
        protected void DeleteResourceSettings(string resourceId)
        {
            // Construct URL
            if (!string.IsNullOrEmpty(SwacComponentUrl))
            {
                bool getUrlSuccess = GetSrcApi(out string factoryName, out string settingsName, out string srcUrl);
                if (!getUrlSuccess)
                    return;

                if (string.IsNullOrEmpty(srcUrl))
                {
                    ShowGetApiUrlError(factoryName, settingsName, srcUrl);
                    return;
                }

                // make API call
                HttpClient client = new HttpClient();
                UriBuilder uri = new UriBuilder(srcUrl);
                client.BaseAddress = uri.Uri;

                try
                {
                    Task<HttpResponseMessage> responseTask = client.DeleteAsync($"api/ResourceSettings/{resourceId}/");
                    responseTask.Wait();

                    HttpResponseMessage responseMsg = responseTask.Result;

                    if (responseMsg.IsSuccessStatusCode == false)
                    {
                        DisplayMessage(new ResultStatus() { Message = responseMsg.ToString(), IsSuccess = false });
                    }
                }
                catch (Exception ex)
                {
                    AlertNestedException(ex, client.BaseAddress.ToString());
                }
            }
        }

        /// <summary>
        /// Use SRC to copy all Resource Settings for the "from" Resource to the "to" Resource
        /// </summary>
        /// <param name="fromResourceId"></param>
        /// <param name="toResourceId"></param>
        /// <param name="factoryLevel"></param>
        protected void CopyResourceSettings(string fromResourceId, string toResourceId, string factoryLevel)
        {
            // Construct URL
            if (!string.IsNullOrEmpty(SwacComponentUrl))
            {
                
                if (!GetSrcApi(out string factoryName, out string  settingsName, out string srcUrl))
                    return;

                if (string.IsNullOrEmpty(srcUrl))
                {
                    ShowGetApiUrlError(factoryName, settingsName, srcUrl);
                    return;
                }

                // make API call
                HttpClient client = new HttpClient();
                UriBuilder uri = new UriBuilder(srcUrl);
                client.BaseAddress = uri.Uri;

                // Create content data for API call
                var payload = new Dictionary<string, string>
                {
                    {"fromResourceId", fromResourceId},
                    {"toResourceId", toResourceId},
                    {"factoryLevel", factoryLevel}  // Ex: for factory, factorylevel will be "SITE"
                };

                var requestUri = $"api/ResourceSettings/CopyResourceSettings/";
                string reqData = JsonConvert.SerializeObject(payload);
                HttpContent content = new StringContent(reqData, Encoding.UTF8, "application/json");

                try
                {
                    Task<HttpResponseMessage> responseTask = client.PostAsync(requestUri, content);
                    responseTask.Wait();

                    HttpResponseMessage responseMsg = responseTask.Result;

                    if (responseMsg.IsSuccessStatusCode == false)
                    {
                        DisplayMessage(new ResultStatus() { Message = responseMsg.ToString(), IsSuccess = false });
                    }
                }
                catch (Exception ex)
                {
                    AlertNestedException(ex, client.BaseAddress.ToString());
                }
            }
        }

        /// <summary>
        /// Get the innermost exception and render its message in an alert() to the client side
        /// </summary>
        /// <param name="ex"></param>
        protected void AlertNestedException(Exception ex, string url = "")
        {
            string errMsg = FactoryHierarchyService.GetNestedExceptionMessage(ex, url);

            // alert with new lines will not display, so show the innermost exception message                
            var alertScript = $"alert('{HttpUtility.JavaScriptStringEncode(errMsg)}','Warning', null);";
            ScriptManager.RegisterStartupScript(this, Page.GetType(), "AlertNestedException", alertScript, true);
        }

        /// <summary>
        /// We could not get an API URL for SRC.  Show an error message.
        /// </summary>
        /// <param name="factoryName"></param>
        /// <param name="settingsName"></param>
        /// <param name="srcApiUrl"></param>
        protected void ShowGetApiUrlError(string factoryName, string settingsName, string srcApiUrl)
        {
            string errMsg = FactoryHierarchyService.GetApiUrlErrorMessage(factoryName, settingsName, srcApiUrl);

            // show message
            var alertScript = $"alert('{errMsg}','Warning', null);";
            ScriptManager.RegisterStartupScript(this, Page.GetType(), "ShowGetApiUrlError", alertScript, true);
            // This does nothing
            //Page.DisplayWarning(errMsg);
        }

        /// <summary>
        /// Use current Employee to find Factory -> ShopFloorSettings -> SRC API URL
        /// </summary>
        /// <returns>success/fail of the service call</returns>
        protected bool GetSrcApi(out string factoryName, out string settingsName, out string srcApiUrl)
        {
            var resultStatus = FactoryHierarchyService.GetSrcApi(out factoryName, out settingsName, out srcApiUrl);

            if(!resultStatus.IsSuccess)
            {
                DisplayMessage(resultStatus);
            }

            return resultStatus.IsSuccess;
        }
     
    }
}