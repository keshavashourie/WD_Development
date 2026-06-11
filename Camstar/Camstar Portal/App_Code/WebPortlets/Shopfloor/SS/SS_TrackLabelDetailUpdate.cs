/* Copyright 2019 Siemens */
using System;
using System.Collections;
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
/// Summary description for SS_TrackLabelDetailUpdate
/// </summary>

namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class SS_TrackLabelDetailUpdate : MatrixWebPart
    {
        protected CWC.TextBox _txtComputerName { get { return Page.FindCamstarControl("ss_TrackLabelDetailUpdate_ComputerName") as CWC.TextBox; } }
        protected CWC.TextBox _txtLabelID { get { return Page.FindCamstarControl("ss_TrackLabelDetailUpdate_LabelID") as CWC.TextBox; } }
        protected CWC.TextBox _txtLabelQty { get { return Page.FindCamstarControl("ss_TrackLabelDetailUpdate_LabelQty") as CWC.TextBox; } }
        protected CWC.TextBox _txtPackedIntoID { get { return Page.FindCamstarControl("ss_TrackLabelDetailUpdate_PackedIntoID") as CWC.TextBox; } }

        protected CWC.NamedObject _ndoEmployee { get { return Page.FindCamstarControl("ss_TrackLabelDetailUpdate_Employee") as CWC.NamedObject; } }

        protected CWC.NamedSubentity _subTrackLabelDetails { get { return Page.FindCamstarControl("ss_TrackLabelDetailUpdate_ss_TrackLabelDetails") as CWC.NamedSubentity; } }

        protected CWC.DropDownList _ddlLabelStatus { get { return Page.FindCamstarControl("ss_TrackLabelDetailUpdate_LabelStatus") as CWC.DropDownList; } }

        protected CWC.CheckBox _chkPrintOnUpdate { get { return Page.FindCamstarControl("ss_TrackLabelDetailUpdate_PrintOnUpdate") as CWC.CheckBox; } }
        protected CWC.CheckBox _chkApplyStatus { get { return Page.FindCamstarControl("ss_TrackLabelDetailUpdate_ApplyStatus") as CWC.CheckBox; } }

        protected JQDataGrid _gridLabelSource { get { return Page.FindCamstarControl("ss_TrackLabelDetailUpdate_ss_LabelSource") as JQDataGrid; } }

        //---------------------------------------------------
        // On Load Event
        //---------------------------------------------------
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            _txtLabelID.DataChanged += new EventHandler(_txtChildLabel_DataChanged);
            //_ddlLabelStatus.DataChanged += new EventHandler(_ddlLabelStatus_DataChanged);
            _txtComputerName.TextControl.Text = SEMI.AppCode.UIUtility.GetComputerName(this);
        }

        //---------------------------------------------------
        // Child Label
        //---------------------------------------------------
        void _txtChildLabel_DataChanged(object sender, EventArgs e)
        {
            try
            {
                if (_txtLabelID.Data != null)
                    FetchLabelInfo(_txtLabelID.Data.ToString());
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        //---------------------------------------------------
        // Lot ID Data Changed
        //---------------------------------------------------
        void _ddlLabelStatus_DataChanged(object sender, EventArgs e)
        {
            try
            {
                if (_ddlLabelStatus.Data != null)
                {
                    if (_ddlLabelStatus.Data.ToString() == "3")
                        _chkApplyStatus.Visible = true;
                    else
                    {
                        _chkApplyStatus.Visible = false;
                        _chkApplyStatus.ClearData();
                    }
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        //---------------------------------------------------
        // Fetch Label Info
        //---------------------------------------------------
        private void FetchLabelInfo(string sLabelID)
        {
            try
            {
                // get the session and user profile
                var fs = FrameworkManagerUtil.GetFrameworkSession();
               
                // Get the service
                ss_TrackLabelDetailUpdateService Svc = new ss_TrackLabelDetailUpdateService(fs.CurrentUserProfile);
                ss_TrackLabelDetailUpdate SvcData = new ss_TrackLabelDetailUpdate();
                ss_TrackLabelDetailUpdate_Info SvcInfo = new ss_TrackLabelDetailUpdate_Info();
                ss_TrackLabelDetailUpdate_Request ReqData = new ss_TrackLabelDetailUpdate_Request();
                ss_TrackLabelDetailUpdate_Result ResData = new ss_TrackLabelDetailUpdate_Result();

                SvcData.LabelID = sLabelID;
                //SvcInfo.ss_TrackLabelDetails = new ss_TrackLabelDetails_Info();
                //SvcInfo.ss_TrackLabelDetails.RequestValue = true;
                SvcInfo.LabelStatus = new Info(true);
                SvcInfo.LabelQty = new Info(true);
                SvcInfo.PackedIntoID = new Info(true);
                SvcInfo.ss_LabelSource = new ss_TrackLabelDetailsSource_Info();
                SvcInfo.ss_LabelSource.SourceID = new Info(true);
                SvcInfo.ss_LabelSource.Qty = new Info(true);

                ReqData.Info = SvcInfo;
                //Execute Request 
                ResultStatus Results = Svc.ss_TrackLabelDetailUpdate_FetchLabelInfo(SvcData, ReqData, out ResData);

                //Result
                if (Results.IsSuccess)
                {
                    Page.StatusBar.ClearMessage();
                    if (ResData.Value.LabelStatus != null)
                        _ddlLabelStatus.Data = ResData.Value.LabelStatus.Value;
                    else
                        _ddlLabelStatus.ClearData();

                    if (ResData.Value.LabelQty != null)
                        _txtLabelQty.Data = ResData.Value.LabelQty;
                    else
                        _txtLabelQty.ClearData();

                    if (ResData.Value.PackedIntoID != null)
                        _txtPackedIntoID.Data = ResData.Value.PackedIntoID;
                    else
                        _txtPackedIntoID.ClearData();

                    if (ResData.Value.ss_LabelSource != null)
                        _gridLabelSource.Data = ResData.Value.ss_LabelSource;
                    else
                        _gridLabelSource.ClearData();
                }
                else
                {
                    DisplayMessage(Results);
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        //---------------------------------------------------
        // GetInputData
        //---------------------------------------------------
        public override void GetInputData(Service serviceData)
        {
            base.GetInputData(serviceData);
            ss_TrackLabelDetailsSource[] oLabelSource = _gridLabelSource.Data as ss_TrackLabelDetailsSource[];
            if (oLabelSource != null)
            {
                (serviceData as ss_TrackLabelDetailUpdate).LabelSourcesDetails = new ss_LabelSourcesDetails[oLabelSource.Length];
                for (int i = 0; i < oLabelSource.Length; i++)
                {
                    (serviceData as ss_TrackLabelDetailUpdate).LabelSourcesDetails[i] = new ss_LabelSourcesDetails();
                    (serviceData as ss_TrackLabelDetailUpdate).LabelSourcesDetails[i].SourceID = oLabelSource[i].SourceID;
                    (serviceData as ss_TrackLabelDetailUpdate).LabelSourcesDetails[i].Qty = oLabelSource[i].Qty;
                }
            }
        }
    }
}



