// © 2022 Siemens Product Lifecycle Management Software Inc.
using System;
using System.Web;
using System.Web.UI;
using Camstar.WebPortal.FormsFramework;
using System.Collections.Generic;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using Camstar.WCF.Services;
using Camstar.WCF.ObjectStack;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using System.Runtime.Serialization;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Camstar.WebPortal.WCFUtilities;

namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class isOEEEqpHistoryWP : MatrixWebPart
    {

        #region Controls
 
        protected virtual CWC.TextBox _resourceField { get { return FindCamstarControl("isResourceOEEInquiry_Resource") as CWC.TextBox; } }

        #endregion

        #region ClientScript Section

        protected override IEnumerable<ScriptReference> GetScriptReferences()
        {
            yield return new ScriptReference("~/scripts/jquery/nv.d3.min.js");
            yield return new ScriptReference("~/scripts/user/Industry Solutions/isOEEEqpHistoryWP.js");
        }

        #endregion

        #region Protected Functions
        protected override void OnPreRender(EventArgs e)
        {
            ScriptManager.RegisterStartupScript(this, this.GetType(), "initializeisOEEEqpHistoryWP", $"isOEEEqpHistoryWP.initialize();", true);
            RenderToClient = true;

            base.OnPreRender(e);
        }

        protected override void OnLoad(EventArgs e)
        {
            //Page.Header.Controls.Add(new LiteralControl("<link rel=\"stylesheet\" type=\"text/css\" href=\"" + ResolveUrl("~/Themes/User/Industry Solutions/isOEEEqpHist.css") + "\" />"));
            ReloadData();
            base.OnLoad(e);

        } // OnLoad(EventArgs e)

        #endregion

        #region Public methods

        public static bool LoadOEEDetails(AjaxTransition transition)
        {
            ClientControllerState parms = null;

            using (Stream str = new MemoryStream(Encoding.UTF8.GetBytes(transition.CommandParameters)))
            {
                var ser = new System.Runtime.Serialization.Json.DataContractJsonSerializer(typeof(ClientControllerState));
                parms = ser.ReadObject(str) as ClientControllerState;
            }

            //Initialize Service & Objects
            FrameworkSession session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
            var oService = new WCF.Services.isResourceOEEInquiryService(session.CurrentUserProfile);
            var oServiceData = new isResourceOEEInquiry();
            var oRequest = new WCF.Services.isResourceOEEInquiry_Request();
            var oResult = new WCF.Services.isResourceOEEInquiry_Result();
            var oResultStatus = new ResultStatus();

            isResourceOEEInquiry_Info oServiceInfo = new isResourceOEEInquiry_Info();
            //Prepare data to be submitted

            bool hasResource = false;
            if (!string.IsNullOrEmpty(parms.Equipment))
            {
                hasResource = true;
                (oServiceData as isResourceOEEInquiry).isResource = new NamedObjectRef(parms.Equipment);
            }
            else if (!string.IsNullOrEmpty(parms.Resource))
            {
                hasResource = true;
                (oServiceData as isResourceOEEInquiry).isResource = new NamedObjectRef(parms.Resource);
            }
            string txnData = null;
            if (hasResource)
            {

                int year = DateTime.Now.Year;
                (oServiceData as isResourceOEEInquiry).isStartTime = parms.StartTime != null ? new Primitive<DateTime>(new DateTime(year, 1, 1)) : null;
                (oServiceData as isResourceOEEInquiry).isEndTime = parms.EndTime != null ? new Primitive<DateTime>(new DateTime(year, 12, 31)) : null;

                //Add request information
                oServiceInfo.isResourceOEEInquiryDetails = new isResourceOEEInquiryDetails_Info();
                oServiceInfo.isResourceOEEInquiryDetails.isResourceName = new Info(true);
                oServiceInfo.isResourceOEEInquiryDetails.isAvailability = new Info(true);
                oServiceInfo.isResourceOEEInquiryDetails.isPerformance = new Info(true);
                oServiceInfo.isResourceOEEInquiryDetails.isOEE = new Info(true);
                oServiceInfo.isResourceOEEInquiryDetails.isQuality = new Info(true);
                oRequest.Info = oServiceInfo;

                //Execute Transaction 
                oResultStatus = oService.GetEnvironment(oServiceData, oRequest, out oResult);
                
                //Result
                if (oResultStatus.IsSuccess)
                {
                    //-----------------------------------------------------
                    // store the return information into the TxnDataList
                    //-----------------------------------------------------
                    var oResultValue = oResult.Value as isResourceOEEInquiry;

                    if (oResultValue.isResourceOEEInquiryDetails != null)
                    {
                        txnData = JsonConvert.SerializeObject(oResultValue.isResourceOEEInquiryDetails, Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                    }


                }
            }

            transition.Response = new[] { new ResponseSection(ResponseType.Command, transition.ID, txnData) };
            return true;
        }

        private void ReloadData()
        {
            object lineAssignmentResource = Page.SessionDataContract.GetValueByName(Camstar.WebPortal.Constants.DataMemberConstants.Resource);
            _resourceField.Data = (lineAssignmentResource != null && lineAssignmentResource.ToString() != "") ? lineAssignmentResource.ToString() : null;
        }
        #endregion

        #region Public Class
        [DataContract]
        public class ClientControllerState
        {
            [DataMember]
            public string Equipment { get; set; }
            [DataMember]
            public string Resource { get; set; }
            [DataMember]
            public string StartTime { get; set; }
            [DataMember]
            public string EndTime { get; set; }
            
        }
        #endregion

    }
}
