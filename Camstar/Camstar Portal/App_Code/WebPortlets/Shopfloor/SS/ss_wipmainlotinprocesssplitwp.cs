/* Copyright 2019 Siemens */
using System;
using System.Data;
using System.Web;
using System.Linq;
using System.Collections.Generic;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.Utilities;
using Camstar.WCF.ObjectStack;
using Camstar.WCF.Services;
using OM = Camstar.WCF.ObjectStack;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WebPortal.Personalization;


/// <summary>
/// The code for the move non standard virtual page.  Resolves the lot based on the 
/// selection id entered and populates the lot details grid.
/// </summary>
/// 
namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class SS_WipMainLotInProcessSplit : MatrixWebPart
    {
        private CWC.TextBox selectionIdTextBox { get { return FindCamstarControl("LotInProcessSplit_SelectionId") as CWC.TextBox; } }
        private CWC.TextBox maxInProcQtyTextBox { get { return FindCamstarControl("LotInProcessSplit_MaxInProcessQty") as CWC.TextBox; } }
        private CWC.ContainerList inProcSplitStdContainer { get { return FindCamstarControl("LotInProcessSplit_Container") as CWC.ContainerList; } }
        private JQDataGrid lotSplitDetailsGrid { get { return FindCamstarControl("LotInProcessSplit_Details") as JQDataGrid; } }
        private CWC.NamedObject equipmentControl { get { return FindCamstarControl("LotInProcessSplit_Equipment") as CWC.NamedObject; } }
        private CWC.NamedObject processTypeControl { get { return FindCamstarControl("LotInProcessSplit_ProcessType") as CWC.NamedObject; } }
        private CWC.CheckBox _chkIsActive { get { return FindCamstarControl("LotInProcessSplit_IsActive") as CWC.CheckBox; } }
        private CWC.Button _btnSubmit { get { return Page.FindCamstarControl("SubmitButton") as CWC.Button; } }
        private CWC.Button _btnReset { get { return Page.FindCamstarControl("ResetButton") as CWC.Button; } }
        private CWC.CheckBox _chkIsPopupField { get { return Page.FindCamstarControl("LotInProcessSplit_IsPopup") as CWC.CheckBox; } }
        private bool bIsPopup = false;

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // additional logic for CR019 EquipmentWIPMain (or where the web part is referenced into a standalone page)
            if (Page.IsAJAXFloatingFrame)
            {
                // hide the submit and reset buttons
                _btnSubmit.Visible = false;
                _btnSubmit.Enabled = false;
                _btnReset.Visible = false;
                _btnReset.Enabled = false;
                if (!Page.IsPostBack)
                    SelectionIdControl_DataChanged();
            }
        }

        public void SelectionIdControl_DataChanged()
        {
            if (!selectionIdTextBox.IsEmpty)
            {
                inProcSplitStdContainer.Data = null;
                GetSelectionId();
            }
        }

        //------------------------------------------------------
        /// Triggered when the web part is loaded
        //------------------------------------------------------
        public void SelectionIdControl_DataChanged(object sender, EventArgs e)
        {
            if (!selectionIdTextBox.IsEmpty)
            {
                inProcSplitStdContainer.Data = null;
                GetSelectionId();
            }
        }

        //------------------------------------------------------
        /// resolves the container name (lot name) from the value of the selection id field
        //------------------------------------------------------
        private Boolean GetSelectionId()
        {
            try
            {
                bool bExecute = false;
                if (_chkIsActive != null)
                    bExecute = (_chkIsActive.CheckControl.Checked || Page.IsAJAXFloatingFrame);
                if (bExecute)
                {
                    lotSplitDetailsGrid.ClearData();

                    OM.LotInProcessSplit_Info txnInfo = new OM.LotInProcessSplit_Info();
                    txnInfo.SelectionContainer = FieldInfoUtil.RequestValue();
                    txnInfo.MaxInProcessQty = FieldInfoUtil.RequestValue();
                    txnInfo.DetailsSelection = new SplitLotDetails_Info();
                    txnInfo.DetailsSelection.ToContainerName = FieldInfoUtil.RequestValue();
                    txnInfo.DetailsSelection.InProcessQty = FieldInfoUtil.RequestValue();
                    txnInfo.DetailsSelection.QtyModifiable = FieldInfoUtil.RequestValue();

                    UserProfile profile = HttpContext.Current.Session[Camstar.WebPortal.Constants.SessionConstants.UserProfile] as UserProfile;

                    LotInProcessSplitService svc = new LotInProcessSplitService(profile);
                    LotInProcessSplit svcData = new LotInProcessSplit();
                    svcData.SelectionId = selectionIdTextBox.Data.ToString();
                    svcData.Equipment = (OM.NamedObjectRef)equipmentControl.Data;
                    svcData.ProcessType = (OM.NamedObjectRef)processTypeControl.Data;

                    LotInProcessSplit_Request reqData = new LotInProcessSplit_Request();
                    reqData.Info = txnInfo;
                    LotInProcessSplit_Result resultData = new LotInProcessSplit_Result();

                    LotInProcessSplit txn = new LotInProcessSplit();
                    ResultStatus results = svc.ResolveSelectionId(svcData, reqData, out resultData);

                    if (results.IsSuccess)
                    {
                        inProcSplitStdContainer.Data = resultData.Value.SelectionContainer;
                        lotSplitDetailsGrid.Data = resultData.Value.DetailsSelection;
                        maxInProcQtyTextBox.Data = resultData.Value.MaxInProcessQty;

                        if (resultData.Value.DetailsSelection != null)
                        {
                            if (resultData.Value.DetailsSelection[0].QtyModifiable == true)
                                lotSplitDetailsGrid.Settings.Columns[1].Editable = (bool)resultData.Value.DetailsSelection[0].QtyModifiable;
                            else
                            {
                                (lotSplitDetailsGrid.Settings as GridDataSettingsItemList).EditorSettings.EditingMode = JQEditingModes.Disabled;
                                lotSplitDetailsGrid.ApplyFieldPersonalization();
                            }
                        }

                        return true;
                    }
                    else
                    {
                        DisplayMessage(results);
                        return false;
                    }
                }
                else
                    return false;

            }
            catch (Exception ex)
            {
                DisplayMessage(new ResultStatus(ex.Message, false));
                return false;
            }

        } //end Get Selection Id

        //------------------------------------------------------
        /// Overridden manually sets the details based on a row being selected.
        //------------------------------------------------------
        public override void GetInputData(Service serviceData)
        {
            bool bExecute = false;
            if (_chkIsActive != null)
                bExecute = (_chkIsActive.CheckControl.Checked || Page.IsAJAXFloatingFrame);

            if (bExecute)
            {
                base.GetInputData(serviceData);

                //let the app server toss an error if the user did not select at least
                //one lot to split
                if (serviceData is OM.LotInProcessSplit)
                {
                    if (lotSplitDetailsGrid.SelectedRowIDs == null)
                        return;

                    List<SplitLotDetails> detailsToAdd = new List<SplitLotDetails>();
                    foreach (SplitLotDetails dtl in (serviceData as LotInProcessSplit).DetailsSelection)
                    {
                        foreach (var rowID in lotSplitDetailsGrid.SelectedRowIDs)
                        {
                            var selectedRow = lotSplitDetailsGrid.GridContext.GetCell(rowID.ToString(), "ToContainerName");
                            if (selectedRow != null && selectedRow.ToString().Equals(dtl.ToContainerName.ToString()))
                            {
                                SplitLotDetails dtlToAdd = new SplitLotDetails();
                                dtlToAdd.ListItemAction = OM.ListItemAction.Add;
                                dtlToAdd.ToContainerName = dtl.ToContainerName;
                                dtlToAdd.InProcessQty = dtl.InProcessQty;
                                detailsToAdd.Add(dtlToAdd);
                            }
                        }
                    }

                    (serviceData as LotInProcessSplit).Details = new SplitLotDetails[detailsToAdd.Count];
                    (serviceData as LotInProcessSplit).Details = detailsToAdd.ToArray();
                    (serviceData as LotInProcessSplit).DetailsSelection = null;
                }
            }
        }

        //------------------------------------------------------
        /// kills any error message and resets the web part
        /// as if it had just loaded
        //------------------------------------------------------
        public void ClearForm()
        {
            DisplayMessage(new ResultStatus("", true));
            GetSelectionId();
        }

        //------------------------------------------------------
        //
        //------------------------------------------------------
        public override void PostExecute(ResultStatus status, Service serviceData)
        {
            base.PostExecute(status, serviceData);
            if (status.IsSuccess)
            {
                GetSelectionId();
            }
        }
    }
}



