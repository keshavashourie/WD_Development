/* Copyright 2023 Siemens */
using Camstar.WCF.ObjectStack;
using Camstar.WebPortal.Personalization;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using SEMI.AppCode;

namespace Camstar.WebPortal.WebPortlets.Parts
{
    public class SS_PartRequestAssign : PartRequestAssign
    {
        //Controls declaration
        private CWC.CheckBox _chkAutoComplete { get { return Page.FindCamstarControl("AutoCompleteField") as CWC.CheckBox; } }
        protected DataEnvelopControl _envPartRequestAssign { get { return Page.FindCamstarControl("PartRequest_Envelope") as DataEnvelopControl; } }
        
        //----------------------------
        // Override base post execute
        //----------------------------
        public override void PostExecute(ResultStatus status, Service serviceData)
        {
            base.PostExecute(status, serviceData);
            string sCompletionMessage = status.Message;
            if (_chkAutoComplete.IsChecked)
                DisplayAlerts(status, out sCompletionMessage);
        }

        //------------------------
        // Display alerts function
        //------------------------
        public void DisplayAlerts(ResultStatus status, out string CompletionMessage)
        {
            // check for alert messages
            string[] sAlertMessages;
            string sCompletionMessage;

            if (status.IsSuccess)
            {
                bool bAlertMsg = SEMI.AppCode.UIUtility.AlertMessagesAvailable(status.Message, out sAlertMessages, out sCompletionMessage);
                status.Message = sCompletionMessage;

                if (bAlertMsg)
                {
                    SEMI.AppCode.DataPacket oData = new DataPacket();
                    oData.IsAlertMessageAvailable = false;
                    oData.AlertMessages = sAlertMessages;
                    oData.ResultStatusMessage = status.Message;
                    _envPartRequestAssign.SS_DataPacket = oData;

                    if (oData.AlertMessages != null)
                    {
                        PopupWIPMessages(false);
                    }
                    if (oData.ResultStatusMessage != null)
                    {
                        Page.StatusBar.WriteSuccess(oData.ResultStatusMessage);
                    }
                }
            }
            CompletionMessage = status.Message;
        }

        //-------------------------
        // Display alerts function
        //-------------------------
        public void DisplayAlerts(string[] AlertMessages)
        {
            SEMI.AppCode.DataPacket oData = new DataPacket();
            oData.IsAlertMessageAvailable = false;
            oData.AlertMessages = AlertMessages;
            _envPartRequestAssign.SS_DataPacket = oData;
        }

        //------------------------
        // Open Alert Message Popup Page
        //------------------------
        public virtual void PopupWIPMessages(bool EndResponse = false)
        {
            Personalization.FloatPageOpenAction objAction = new FloatPageOpenAction();
            objAction.PageName = "SS_AlertMessagePopupVP";

            objAction.FrameLocation = new UIFloatingPageLocation();
            objAction.FrameLocation.Width = 430;
            objAction.FrameLocation.Height = 230;
            objAction.EndResponse = EndResponse;
            objAction.ShowButtons = false;

            UIComponentDataContractLink[] objLinks = new UIComponentDataContractLink[1];
            objLinks[0] = new UIComponentDataContractLink();
            objLinks[0].SourceMember = "PartRequestAssign_Envelope";
            objLinks[0].TargetMember = "envelopAlertInDM";
            objAction.DataContractMap = new UIComponentDataContractMap();
            objAction.DataContractMap.Links = objLinks;

            SEMI.AppCode.UIUtility.SetHorizonAlertPopupFrameLocation(this, objAction);
        }  // ShowAlerts
    }
}




