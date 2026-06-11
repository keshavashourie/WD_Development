/* Copyright 2019 Siemens */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Text;
using System.Web;
using System.Web.UI;

using Camstar.WCF.ObjectStack;
using Camstar.WCF.Services;
using Camstar.WebPortal.Utilities;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using OM = Camstar.WCF.ObjectStack;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.Constants;
using Camstar.WebPortal.WCFUtilities;
using Camstar.WebPortal.Personalization;
using SEMI.AppCode;

/// <summary>
/// Summary description for SS_AlertMessagePopup
/// </summary>
namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class SS_AlertMessagePopup : MatrixWebPart
    {
        protected DataEnvelopControl _envDataEnvelop { get { return Page.FindCamstarControl("Envelop") as DataEnvelopControl; } }
        protected CWC.TextBox _txtTotalMessage { get { return Page.FindCamstarControl("TotalMessage") as CWC.TextBox; } }
        protected CWC.TextBox _txtCurrentMessageIndex { get { return Page.FindCamstarControl("CurrentMessageIndex") as CWC.TextBox; } }
        protected CWC.TextBox _txtMessage { get { return Page.FindCamstarControl("Message") as CWC.TextBox; } }
        protected CWC.Button _btnPrevMsg { get { return Page.FindCamstarControl("PrevMsg") as CWC.Button; } }
        protected CWC.Button _btnNextMsg { get { return Page.FindCamstarControl("NextMsg") as CWC.Button; } }
        protected CWC.Button _btnClosePopup { get { return Page.FindCamstarControl("BtnClose") as CWC.Button; } }
        protected CWC.Label _lblCurrentMessageLabel { get { return Page.FindCamstarControl("CurrentMessageLabel") as CWC.Label; } }
        protected CWC.Label _lblTotalMessageLabel { get { return Page.FindCamstarControl("TotalMessageLabel") as CWC.Label; } }

        //-----------------------------------------
        //
        //-----------------------------------------
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (!Page.IsPostBack)
            {
                AddDataEnvelopDataMember();

                // add the data contract member        
                if (Page.DataContract.GetValueByName("envelopAlertInDM") != null)
                    _envDataEnvelop.SS_DataPacket = Page.DataContract.GetValueByName("envelopAlertInDM") as DataPacket;

                InitAlertMessagePopup();
                ShowAlertMessage();
            }
        } // OnLoad

        //-----------------------------------------
        //
        //-----------------------------------------
        private void AddDataEnvelopDataMember()
        {
            int intConfiguredDataMemberCount = 0;

            if (Page.DataContract != null)
            {
                if (Page.DataContract.DataMembers != null)
                    intConfiguredDataMemberCount = Page.DataContract.DataMembers.Length;
            }
            else
                Page.DataContract = new UIComponentDataContract();

            // manually add the dataContractMember since the custom control's property does not show up at design time
            UIComponentDataMember[] objPageDataMembers = new UIComponentDataMember[intConfiguredDataMemberCount + 1];
            int intDMIndex = 0;

            if (Page.DataContract.DataMembers != null)
            {
                foreach (UIComponentDataMember objDM in Page.DataContract.DataMembers)
                {
                    objPageDataMembers[intDMIndex] = new UIComponentDataMember();
                    objPageDataMembers[intDMIndex] = objDM;
                    intDMIndex++;
                }
            }

            // add the new envelop data member
            objPageDataMembers[intDMIndex] = new UIComponentDataMember();
            objPageDataMembers[intDMIndex].Key = "AlertMessageWP.Envelop";
            objPageDataMembers[intDMIndex].Name = "envelopAlertOutDM";
            objPageDataMembers[intDMIndex].ConnectionType = DataMemberConnectionType.Control;
            objPageDataMembers[intDMIndex].Property = "SS_DataPacket";

            Page.DataContract.DataMembers = objPageDataMembers;
        } // AddDataEnvelopDataMember

        //-----------------------------------------
        //
        //-----------------------------------------
        public void InitAlertMessagePopup()
        {
            int intTotalMessages = 0;
            int intCurrentIndex = -1;

            if (_envDataEnvelop.SS_DataPacket != null)
            {
                if (_envDataEnvelop.SS_DataPacket.AlertMessages != null)
                {
                    intTotalMessages = _envDataEnvelop.SS_DataPacket.AlertMessages.Length;
                    intCurrentIndex = 0;                   
                }
            }

            _txtTotalMessage.Data = intTotalMessages.ToString();
            _txtCurrentMessageIndex.Data = intCurrentIndex.ToString();

            _lblTotalMessageLabel.Text = intTotalMessages.ToString();
            _lblCurrentMessageLabel.Text = (intCurrentIndex + 1).ToString();
        } // InitAlertMessagePopup

        //-----------------------------------------
        //
        //-----------------------------------------
        public void ShowAlertMessage()
        {
            int intCurrentIndex = int.Parse(_txtCurrentMessageIndex.Data.ToString());
            int intTotalMessages = int.Parse(_txtTotalMessage.Data.ToString());

            _lblCurrentMessageLabel.Text = (intCurrentIndex + 1).ToString();

            if (intCurrentIndex >= 0)
            {
                _txtMessage.Data = _envDataEnvelop.SS_DataPacket.AlertMessages[intCurrentIndex].ToString();
                if (intCurrentIndex > 0)
                    _btnPrevMsg.Enabled = true;
                else
                    _btnPrevMsg.Enabled = false;

                if ((intCurrentIndex + 1) < intTotalMessages)
                    _btnNextMsg.Enabled = true;
                else
                    _btnNextMsg.Enabled = false;

                if ((intCurrentIndex + 1) == intTotalMessages)
                    _btnClosePopup.Enabled = true;
                else
                    _btnClosePopup.Enabled = false;
            }
        } // ShowAlertMessage

        //-----------------------------------------
        //
        //-----------------------------------------
        public void PrevMsgClick()
        {
            int intCurrentIndex = int.Parse(_txtCurrentMessageIndex.Data.ToString());
            intCurrentIndex--;
            _txtCurrentMessageIndex.Data = intCurrentIndex.ToString();
            ShowAlertMessage();
        } // PrevMsgClick

        //-----------------------------------------
        //
        //-----------------------------------------
        public void NextMsgClick()
        {
            int intCurrentIndex = int.Parse(_txtCurrentMessageIndex.Data.ToString());
            intCurrentIndex++;
            _txtCurrentMessageIndex.Data = intCurrentIndex.ToString();
            ShowAlertMessage();
        } // NextMsgClick

        //-----------------------------------------
        //
        //-----------------------------------------
        public void CloseButtonClick()
        {
            _envDataEnvelop.SS_DataPacket.AlertMessages = null;
            _envDataEnvelop.SS_DataPacket.IsAlertMessageAvailable = false;
            ScriptManager.RegisterStartupScript(Page.Form, GetType(), "ClosePopup", "window.parent.__page._isTabContainerPage=null;window.parent.CloseFloatingFrame(true);", true);
        } // CloseButtonClick
    }
}



