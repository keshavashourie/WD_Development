/* Copyright 2023 Siemens */
using System;
using Camstar.WebPortal.Personalization;
using OM = Camstar.WCF.ObjectStack;
using SEMI.AppCode;

namespace Camstar.WebPortal.WebPortlets.Parts
{

    public class SS_PartRequestMain : PartRequestMain
    {
        //Controls declaration
        protected DataEnvelopControl _envPartRequestMain { get { return Page.FindCamstarControl("PartRequestMain_Envelope") as DataEnvelopControl; } }
        //------------------------
        //
        //------------------------
        public void OnPopupClose()
        {
            if (Page.DataContract.GetValueByName("PartRequestMain_Envelope") != null)
            {
                DataPacket oData = Page.DataContract.GetValueByName("PartRequestMain_Envelope") as DataPacket;
                if (oData.AlertMessages != null)
                {
                    PopupWIPMessages(false);
                }
                if (oData.ResultStatusMessage != null)
                {
                    Page.StatusBar.WriteSuccess(oData.ResultStatusMessage);
                }
            }
            UpdateParams();
        }

        //------------------------
        //
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
            objLinks[0].SourceMember = "PartRequestMain_Envelope";
            objLinks[0].TargetMember = "envelopAlertInDM";
            objAction.DataContractMap = new UIComponentDataContractMap();
            objAction.DataContractMap.Links = objLinks;

            //UIComponentDataContractReturnLink[] objReturnLinks = new UIComponentDataContractReturnLink[1];
            //objReturnLinks[0] = new UIComponentDataContractReturnLink();
            //objReturnLinks[0].SourceMember = "envelopAlertOutDM";
            //objReturnLinks[0].TargetMember = "PartRequestMain_Envelope";
            //objAction.DataContractReturnMap = new UIComponentDataContractReturnMap();
            //objAction.DataContractReturnMap.ReturnLinks = objReturnLinks;

            SEMI.AppCode.UIUtility.SetHorizonAlertPopupFrameLocation(this, objAction);
        }  // ShowAlerts


        //-----------------------
        // Display alerts
        //-----------------------
        public void DisplayAlerts(OM.ResultStatus status, out string CompletionMessage)
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
                    _envPartRequestMain.SS_DataPacket = oData;

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

        //------------------------
        //
        //------------------------
        public override void PostExecute(OM.ResultStatus status, OM.Service serviceData)
        {
            base.PostExecute(status, serviceData);

            string sCompletionMessage = status.Message;

            if (serviceData is OM.PartRequestComplete)
                DisplayAlerts(status, out sCompletionMessage);
        }

        //------------------------
        //
        //------------------------
        protected override void OnLoad(EventArgs e) //Page on load event
        {
            base.OnLoad(e);
            if (Page.EventArgument == "FloatingFrameSubmitParentPostBackArgument") // From javascript after submited
            {
                if (SEMI.AppCode.UIUtility.IsPopupClose(this))
                {
                    OnPopupClose();
                    Page.DataContract.SetValueByName("PartRequestMain_Envelope", null);
                }
            }
        }
    }
}





