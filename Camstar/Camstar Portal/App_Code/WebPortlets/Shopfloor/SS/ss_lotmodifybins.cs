/* Copyright 2020 Siemens */
using Camstar.WebPortal.FormsFramework.Utilities;
using System.Data;
using System.Web;
using System.Linq;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using OM = Camstar.WCF.ObjectStack;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using System;
using Camstar.WebPortal.Personalization;
using Camstar.WebPortal.Utilities;
using Camstar.WCF.ObjectStack;
using Camstar.WCF.Services;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// The code for the Set Carrier virtual page.
/// </summary>
/// 
namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class SS_LotModifyBins : scsShopfloorBase
    {
        #region Private Properites

        private CWC.TextBox SelectionIdTextBox { get { return FindCamstarControl("LotModifyBins_SelectionId") as CWC.TextBox; } }
        private JQDataGrid lotDetailsGrid { get { return FindCamstarControl("LotDetailsGrid") as JQDataGrid; } }
        private CWC.TextBox ComputerNameTextBox { get { return FindCamstarControl("LotModifyBins_ComputerName") as CWC.TextBox; } }
        //		private JQDataGrid ContainersFieldGrid	{	get { return FindCamstarControl("ContainersField") as JQDataGrid; }	}
        private CWC.TextBox CommentsTextBox { get { return Page.FindCamstarControl("LotModifyBins_Comments") as CWC.TextBox; } }
        private CWC.ContainerList modifyBinsContainer { get { return FindCamstarControl("LotModifyBins_Container") as CWC.ContainerList; } }
        private JQDataGrid lotModifyBinsGrid { get { return FindCamstarControl("LotModifyBins_Bins") as JQDataGrid; } }
        private CWC.Button lotInfoButton { get { return FindCamstarControl("LotModifyBins_LotAttributesButton") as CWC.Button; } }
        private CWC.CheckBox chkIsWaferProcessing { get { return Page.FindCamstarControl("LotModifyBins_IsWaferProcessing") as CWC.CheckBox; } }
        private CWC.Button LotAttributesBttn { get { return Page.FindCamstarControl("LotAttributesPopup") as CWC.Button; } }

        #endregion

        #region Methods

        /// <summary>
        /// constructor for the SS_SetCarrier class
        /// </summary>
        public SS_LotModifyBins()
        {

        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            chkIsWaferProcessing.Hidden = true;

            var lotDetailsGrid = (Page.FindCamstarControl("LotDetailsGrid")) as JQDataGrid;
            var lotModifyBins = (Page.FindCamstarControl("LotModifyBins_Bins")) as JQDataGrid;

            if (IsResponsive)
            {
                if (lotDetailsGrid.Settings.Automation == null)
                    lotDetailsGrid.Settings.Automation = new GridAutomation();

                lotDetailsGrid.Settings.Automation.ShrinkColumnWidthToFit = false;

                if (lotModifyBins.Settings.Automation == null)
                    lotModifyBins.Settings.Automation = new GridAutomation();

                lotModifyBins.Settings.Automation.ShrinkColumnWidthToFit = false;
            }

            if (IsHorizon())
            {
                lotInfoButton.Visible = false;
                LotAttributesBttn.Visible = true;
            }
            else
            {
                lotInfoButton.Visible = true;
                LotAttributesBttn.Visible = false;
            }

            if (!Page.IsPostBack)
            {
                ComputerNameTextBox.TextControl.Text = SEMI.AppCode.UIUtility.GetComputerName(this);
            }
            else
                if (SEMI.AppCode.UIUtility.IsPopupClose(this))
            {

                if (Page.DataContract.GetValueByName("LotListDM") != null)
                {
                    //gets the list of returned lot from the popup form
                    string[] sContainers = Page.DataContract.GetValueByName("LotListDM") as string[];
                    if (sContainers.Length > 0)
                    {
                        SelectionIdTextBox.Data = sContainers[0];
                        SelectionIdField_DataChanged();
                    }

                    Page.DataContract.SetValueByName("LotListDM", null);
                }
            }
        }

        /// <summary>
        /// Triggered when a lot / carrier etc... is entered on the page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void SelectionIdField_DataChanged()
        {
            try
            {
                modifyBinsContainer.ClearData();
                DisplayMessage(new ResultStatus("", true));

                if (SelectionIdTextBox.Data != null)
                {
                    if (SelectionIdTextBox.IsEmpty)
                    {
                        lotInfoButton.Enabled = false;
                        return;
                    }

                    if (GetSelectionId() && modifyBinsContainer.Data != null)
                    {
                        //then populate the lot details grid
                        //GetLotQuerySelection(PrimaryServiceType, moveNonStdContainer.Data.ToString());
                        JQDataGrid _gridLotSelectionX = lotDetailsGrid;
                        SEMI.AppCode.UIUtility.GetLotQuerySelection(this, PrimaryServiceType, modifyBinsContainer.Data.ToString(), true, ref _gridLotSelectionX, "LotDetailsGrid");
                        GetBins();
                        lotInfoButton.Enabled = true;
                    }

                }
                else
                    lotInfoButton.Enabled = false;
            }
            catch (Exception Ex)
            {
                DisplayMessage(new OM.ResultStatus(Ex.TargetSite.Name + "(): " + Ex.Message, false));
            }
        }

        /// <summary>
        /// resolves the container name (lot name) from the value of the selection id field
        /// </summary>
        /// <returns></returns>
        private Boolean GetSelectionId()
        {

            try
            {
                OM.LotModifyBins_Info txnInfo = new OM.LotModifyBins_Info();
                //the split bins check box is hidden if the container is at an item processing spec
                txnInfo.IsWaferProcessing = FieldInfoUtil.RequestValue();
                txnInfo.SelectionContainer = FieldInfoUtil.RequestValue();

                UserProfile profile = HttpContext.Current.Session[Camstar.WebPortal.Constants.SessionConstants.UserProfile] as UserProfile;

                LotModifyBinsService svc = new LotModifyBinsService(profile);
                LotModifyBins svcData = new LotModifyBins();
                svcData.SelectionId = SelectionIdTextBox.Data.ToString();

                LotModifyBins_Request reqData = new LotModifyBins_Request();
                reqData.Info = txnInfo;
                LotModifyBins_Result resultData = new LotModifyBins_Result();

                LotModifyBins txn = new LotModifyBins();
                ResultStatus results = svc.ResolveSelectionId(svcData, reqData, out resultData);

                if (results.IsSuccess)
                {
                    modifyBinsContainer.Data = resultData.Value.SelectionContainer;
                    lotInfoButton.Enabled = true;
                    return true;
                }
                else
                {
                    DisplayMessage(results);
                    return false;
                }

            }
            catch (Exception ex)
            {
                DisplayMessage(new ResultStatus(ex.Message, false));
                return false;
            }

        } //end Get Selection Id

        /// <summary>
        /// Goes and fetches the bins for the entered lot
        /// </summary>
        /// <returns></returns>
        private Boolean GetBins()
        {
            try
            {
                OM.LotModifyBins_Info txnInfo = new OM.LotModifyBins_Info();

                txnInfo.LotBins = new LotBins_Info();
                txnInfo.SelectionContainer = FieldInfoUtil.RequestValue();
                txnInfo.LotBins.Bin = FieldInfoUtil.RequestValue();
                txnInfo.LotBins.BinCategory = FieldInfoUtil.RequestValue();
                txnInfo.LotBins.BinComment = FieldInfoUtil.RequestValue();
                txnInfo.LotBins.BinLotId = FieldInfoUtil.RequestValue();
                txnInfo.LotBins.BinProduct = FieldInfoUtil.RequestValue();
                txnInfo.LotBins.BinQty = FieldInfoUtil.RequestValue();
                txnInfo.LotBins.WaferScribeNumber = FieldInfoUtil.RequestValue();
                txnInfo.LotBins.ToWaferScribeNumber = FieldInfoUtil.RequestValue();
                txnInfo.LotBins.ProcessType = FieldInfoUtil.RequestValue();
                txnInfo.LotBins.Spec = FieldInfoUtil.RequestValue();
                txnInfo.LotBins.SplitBeforeMoveOut = FieldInfoUtil.RequestValue();
                txnInfo.LotBins.AutoMoveOut = FieldInfoUtil.RequestValue();
                txnInfo.LotBins.TxnTimestamp = FieldInfoUtil.RequestValue();
                txnInfo.LotBins.Username = FieldInfoUtil.RequestValue();
                txnInfo.IsWaferProcessing = FieldInfoUtil.RequestValue();

                UserProfile profile = HttpContext.Current.Session[Camstar.WebPortal.Constants.SessionConstants.UserProfile] as UserProfile;

                LotModifyBinsService svc = new LotModifyBinsService(profile);
                LotModifyBins svcData = new LotModifyBins();
                svcData.Container = (ContainerRef)modifyBinsContainer.Data;

                LotModifyBins_Request reqData = new LotModifyBins_Request();
                reqData.Info = txnInfo;
                LotModifyBins_Result resultData = new LotModifyBins_Result();

                LotModifyBins txn = new LotModifyBins();
                ResultStatus results = svc.GetEnvironment(svcData, new LotModifyBins_Request { Info = txnInfo }, out resultData);

                if (results.IsSuccess)
                {
                    if (resultData.Value != null)
                    {
                        if (resultData.Value.LotBins != null)
                        {
                            List<LotBinsDetails> bins = new List<LotBinsDetails>();
                            foreach (LotBins lotBinItem in resultData.Value.LotBins)
                            {
                                bins.Add(new LotBinsDetails
                                {
                                    Bin = lotBinItem.Bin
                                        ,
                                    BinCategory = lotBinItem.BinCategory
                                        ,
                                    BinComment = lotBinItem.BinComment
                                        ,
                                    BinLotId = lotBinItem.BinLotId
                                        ,
                                    BinProduct = lotBinItem.BinProduct
                                        ,
                                    BinQty = (int)lotBinItem.BinQty
                                        ,
                                    ProcessType = lotBinItem.ProcessType
                                        ,
                                    Spec = lotBinItem.Spec
                                        ,
                                    SplitBeforeMoveOut = lotBinItem.SplitBeforeMoveOut
                                        ,
                                    AutoMoveOut = lotBinItem.AutoMoveOut
                                        ,
                                    TxnDate = lotBinItem.TxnTimestamp
                                        ,
                                    Username = lotBinItem.Username
                                        ,
                                    WaferScribeNumber = lotBinItem.WaferScribeNumber
                                        ,
                                    ToWaferScribeNumber = lotBinItem.ToWaferScribeNumber
                                });
                            }

                            //chkIsWaferProcessing.Data = bIsWaferProcessing.ToString();

                            (lotModifyBinsGrid.GridContext as BoundContext).Data = bins.ToArray();
                            lotModifyBinsGrid.BoundContext.LoadData();
                        }

                        bool bIsWaferProcessing = false;
                        try { bIsWaferProcessing = bool.Parse(resultData.Value.IsWaferProcessing.ToString()); }
                        catch { bIsWaferProcessing = false; }

                        chkIsWaferProcessing.CheckControl.Checked = bIsWaferProcessing;
                        chkIsWaferProcessing.Data = bIsWaferProcessing.ToString();

                    }
                    return true;
                }
                else
                {
                    DisplayMessage(results);
                    return false;
                }

            }
            catch (Exception ex)
            {
                DisplayMessage(new ResultStatus(ex.Message, false));
                return false;
            }

        } //end GetBins

        /// <summary>
        /// simply clears the grid
        /// </summary>
        private void ClearLotDetailsGrid()
        {
            //Whack any previous data
            lotModifyBinsGrid.ClearData();
            lotDetailsGrid.ClearData();

            // Whack the headers, and add back the magical space col (for looks)
            if (lotDetailsGrid.BoundContext.Fields.Count > 0)
            {
                JQFieldCollection objFieldClear = new JQFieldCollection();
                lotDetailsGrid.BoundContext.Fields = objFieldClear;
                lotDetailsGrid.BoundContext.Fields.Add(new JQField("_spacer") { LabelText = "&nbsp;", Visible = true });
            }
        }

        /// <summary>
        /// Clears the query grid
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public override void WebPartCustomAction(object sender, Personalization.CustomActionEventArgs e)
        {
            base.WebPartCustomAction(sender, e);

            ClearLotDetailsGrid();
            SelectionIdTextBox.ClearData();
            chkIsWaferProcessing.CheckControl.Checked = false;
            DisplayMessage(new ResultStatus("", true));

        } // WebPartCustomAction(object sender, CustomActionEventArgs e)

        /// <summary>
        /// Just tidies up the form
        /// </summary>
        /// <param name="status"></param>
        /// <param name="serviceData"></param>
        public override void PostExecute(ResultStatus status, Service serviceData)
        {
            base.PostExecute(status, serviceData);

            if (status.IsSuccess == true)
            {
                ClearLotDetailsGrid();
                SelectionIdTextBox.ClearData();
                CommentsTextBox.ClearData();
            }
        }

        #endregion
    }

}


