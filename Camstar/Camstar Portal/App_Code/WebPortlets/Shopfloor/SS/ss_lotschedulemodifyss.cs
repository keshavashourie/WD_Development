/* Copyright 2025 Siemens */
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
/// Summary description for SS_LotUnTerminate
/// </summary>
namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class SS_LotScheduleModifySS : MatrixWebPart
    {
        #region Properties

        protected CWC.TextBox _txtSelectionId { get { return Page.FindCamstarControl("LotScheduleModify_SelectionId") as CWC.TextBox; } }
        protected CWC.TextBox txtComputerName { get { return Page.FindCamstarControl("LotScheduleModify_ComputerNameField") as CWC.TextBox; } }
        protected Camstar.WebPortal.FormsFramework.WebControls.ContainerList containerSelectionValues { get { return Page.FindCamstarControl("ContainerField") as Camstar.WebPortal.FormsFramework.WebControls.ContainerList; } }
        protected CWC.NamedSubentity _subScheduleData { get { return Page.FindCamstarControl("LotScheduleModify_ScheduleData") as CWC.NamedSubentity; } }
        SEMI.AppCode.DataEnvelopControl _envSelectedLots { get { return Page.FindCamstarControl("LotScheduleModify_SelectedLotsList") as SEMI.AppCode.DataEnvelopControl; } }
        protected CWC.NamedObject _SSPlanRef { get { return Page.FindCamstarControl("LotScheduleModify_SSPlanRef") as CWC.NamedObject; } }
        protected CWC.RevisionedObject _ProcessSpec { get { return Page.FindCamstarControl("LotScheduleModify_ProcessSpec") as CWC.RevisionedObject; } }
        protected CWC.TextBox _txtSSPopupKey { get { return Page.FindCamstarControl("SSPopupKey") as CWC.TextBox; } }
		protected CWC.RevisionedObject _SupplementarySpec { get { return Page.FindCamstarControl("LotScheduleModify_SupplementarySpec") as CWC.RevisionedObject; } }
        protected CWC.TextBox _txtSelectedRowId { get { return Page.FindCamstarControl("SelectedRowId") as CWC.TextBox; } }
        protected CWC.TextBox _txtToFromPopupKey { get { return Page.FindCamstarControl("ToFromPopupKey") as CWC.TextBox; } }
        protected JQDataGrid _gridSSDetails { get { return Page.FindCamstarControl("LotScheduleModify_SSDetails") as JQDataGrid; } }
        protected JQDataGrid _gridSpecsSelection { get { return Page.FindCamstarControl("LotScheduleModify_SpecsSelection") as JQDataGrid; } }
		JQDataGrid _gridSSByStepDetails { get { return Page.FindCamstarControl("LotScheduleModify_SSByStepDetails") as JQDataGrid; } }

        #endregion

        #region DataChangeEvents

        /// <summary>
        /// Gets Schedule data to bind data to dropdown
        /// </summary>
        public void GetScheduleData()
        {
            try
            {
                if (_txtSelectionId.Data != null)
                {
                    FrameworkSession fs = FrameworkManagerUtil.GetFrameworkSession();
                    LotScheduleModifyService svc = new LotScheduleModifyService(fs.CurrentUserProfile);
                    LotScheduleModify txn = new LotScheduleModify();
                    LotScheduleModify_Info txnInfo = new LotScheduleModify_Info();

                    //request of data to be retrived.	
                    txn.Container = new ContainerRef(_txtSelectionId.TextControl.Text);

                    //What field would we like data
                    txnInfo.ScheduleData = new OM.Info();
                    txnInfo.ScheduleData.RequestSelectionValues = true;

                    //Request object
                    LotScheduleModify_Request req = new LotScheduleModify_Request();
                    //Result object
                    LotScheduleModify_Result res = new LotScheduleModify_Result();

                    req.Info = txnInfo;

                    ResultStatus rs = svc.GetEnvironment(txn, req, out res);

                    if (rs.IsSuccess)
                    {
                        _subScheduleData.DropDownControl.Items.Clear();
                        if (res.Environment.ScheduleData.SelectionValues.Rows != null)
                        {
                            //int i = 0;
                            foreach (Row data in res.Environment.ScheduleData.SelectionValues.Rows)
                            {
                                _subScheduleData.DropDownControl.Items.Add(data.Values[0].ToString());
                                //i++;
                            }
                            ScheduleData_DataChanged();
                        }
                        else
                        {
                            ResultStatus errorMessage = new ResultStatus(_txtSelectionId.TextControl.Text + ": is not a vaild lot or has not been scheduled!", false);
                            DisplayMessage(errorMessage);
                        }
                    }
                    else
                    {
                        DisplayMessage(rs);
                    }
                }
            }
            catch (Exception Ex)
            {
                DisplayMessage(new OM.ResultStatus(Ex.TargetSite.Name + "(): " + Ex.Message, false));
            }
            finally
            {
                _txtSelectionId.Focus();
            }
        }

        /// <summary>
        /// This is an event that is fire when data is change in the SelectionId field is changed.
        /// </summary>
        public void ScheduleData_DataChanged()
        {

            try
            {
                if (_txtSelectionId.Data != null && _subScheduleData.Data != null)
                {
                    //clear the clear message
                    Page.StatusBar.ClearMessage();

                    // get the session and user profile
                    var fs = FrameworkManagerUtil.GetFrameworkSession();

                    // Get the service type of the page
                    string sServiceType = Page.PrimaryServiceType;

                    // Run proper constructor. We need to be dynamic with the primary service type
                    var svcType = WCFObject.CreateObjectType(sServiceType + "Service");
                    //create a request object
                    var oRequest = WCFObject.CreateObject(sServiceType + "_Request");
                    var svcConstructor = svcType.GetConstructor(new Type[] { typeof(UserProfile) });
                    var oService = svcConstructor.Invoke(new object[] { fs.CurrentUserProfile });

                    //retriving data dynamically for Flexibility of the page.
                    var oServiceData = CreateServiceData(sServiceType);

                    var info = CreateServiceInfo(sServiceType);
                    var oServiceInfo = info as LotScheduleModify_Info;
                    (oRequest as Request).Info = oServiceInfo;

                    //setting the schedulingData id for retrival of data.
                    (oServiceData as LotScheduleModify).ScheduleData = new SubentityRef();
					(oServiceData as LotScheduleModify).ScheduleData.ID = _subScheduleData.DropDownControl.Items[_subScheduleData.DropDownControl.SelectedIndex].Text;

                    //request of data to be retrived.
                    oServiceInfo.Product = FieldInfoUtil.RequestValue();
                    oServiceInfo.ProcessSpec = FieldInfoUtil.RequestValue();
                    oServiceInfo.ProcessSpec = new OM.Info();
                    oServiceInfo.ProcessSpec.RequestSelectionValues = true;
                    oServiceInfo.Containers = FieldInfoUtil.RequestValue();
					//// Request SS Info
					oServiceInfo.SSPlanRef = FieldInfoUtil.RequestValue();
					oServiceInfo.ScheduleSS = new ScheduleSS_Info();
					oServiceInfo.ScheduleSS.MoveOutFromSpec = FieldInfoUtil.RequestValue();
					oServiceInfo.ScheduleSS.MoveOutFromStep = FieldInfoUtil.RequestValue();
					oServiceInfo.ScheduleSS.LotId = FieldInfoUtil.RequestValue();
					oServiceInfo.ScheduleSS.SS = FieldInfoUtil.RequestValue();
					oServiceInfo.ScheduleSS.MoveOutToSpec = FieldInfoUtil.RequestValue();
					oServiceInfo.ScheduleSS.MoveBackFromSpec = FieldInfoUtil.RequestValue();
					oServiceInfo.ScheduleSS.MoveBackToSpec = FieldInfoUtil.RequestValue();
					oServiceInfo.ScheduleSS.MoveOutToStep = FieldInfoUtil.RequestValue();
					oServiceInfo.ScheduleSS.MoveBackFromStep = FieldInfoUtil.RequestValue();
					oServiceInfo.ScheduleSS.MoveBackToStep = FieldInfoUtil.RequestValue();
					oServiceInfo.ScheduleSS.IgnoreMainLot = FieldInfoUtil.RequestValue();
					oServiceInfo.ScheduleSS.RPTBulkDelta = FieldInfoUtil.RequestValue();
					oServiceInfo.ScheduleSS.RPTUnitDelta = FieldInfoUtil.RequestValue();

                    // Request Test Params Info
                    oServiceInfo.ScheduleTestParams = new ScheduleTestParams_Info();
                    oServiceInfo.ScheduleTestParams.ProcessSpec = FieldInfoUtil.RequestValue();
                    oServiceInfo.ScheduleTestParams.Spec = FieldInfoUtil.RequestValue();
                    oServiceInfo.ScheduleTestParams.ProcessType = FieldInfoUtil.RequestValue();
                    oServiceInfo.ScheduleTestParams.TestProgramName = FieldInfoUtil.RequestValue();
                    oServiceInfo.ScheduleTestParams.TestProgramMajorRevision = FieldInfoUtil.RequestValue();
                    oServiceInfo.ScheduleTestParams.TestProgramMinorRevision = FieldInfoUtil.RequestValue();
                    oServiceInfo.ScheduleTestParams.TestCode = FieldInfoUtil.RequestValue();
                    oServiceInfo.ScheduleTestParams.TestTemperature = FieldInfoUtil.RequestValue();

                    // init the result object
                    Result oResult = new Result();

                    // execute to request the value
                    ResultStatus resultStatus = (oService as IShopFloorBase).GetEnvironment(oServiceData, (oRequest as Request), out oResult);

                    if (resultStatus.IsSuccess)
                    {
                        DisplayValues((oResult.Value as LotScheduleModify));

                        //manuelly populated dropdown.
                        _ProcessSpec.SetSelectionValues((oResult.Environment as LotScheduleModify_Environment).ProcessSpec.SelectionValues);

                        ////Population of grids
                        //SS Details
                        //Casting different data types in a list. Recevicing one data type and submitting another.					
						if ((oResult.Value as LotScheduleModify).ScheduleSS != null)
						{
							JQDataGrid SSDetailsGrid = Page.FindCamstarControl("LotScheduleModify_SSDetails") as JQDataGrid;
							JQDataGrid SSDetailsByStepGrid = Page.FindCamstarControl("LotScheduleModify_SSByStepDetails") as JQDataGrid;
							List<ScheduleSSDetails> ssItems = new List<ScheduleSSDetails>();
							List<ScheduleSSDetailsEx> ssItemsByStep = new List<ScheduleSSDetailsEx>();
							foreach (ScheduleSS ssItem in (oResult.Value as LotScheduleModify).ScheduleSS)
							{
								if (ssItem.MoveOutFromSpec != null)
								{
									ssItems.Add(new ScheduleSSDetails
									{
										SS = ssItem.SS
										,MoveBackFromSpec = (ssItem.MoveBackFromSpec != null ? ssItem.MoveBackFromSpec : null)
										,MoveBackToSpec = (ssItem.MoveBackToSpec != null ? ssItem.MoveBackToSpec : null)
										,MoveOutFromSpec = (ssItem.MoveOutFromSpec != null ? ssItem.MoveOutFromSpec : null)
										,MoveOutToSpec = (ssItem.MoveOutToSpec != null ? ssItem.MoveOutToSpec : null)
										,LotId = (ssItem.LotId != null ? ssItem.LotId : null)
										,IgnoreMainLot = (ssItem.IgnoreMainLot != null ? ssItem.IgnoreMainLot : true)
										,RPTBulkDelta = (ssItem.RPTBulkDelta != null ? ssItem.RPTBulkDelta : null)
										,RPTUnitDelta = (ssItem.RPTUnitDelta != null ? ssItem.RPTUnitDelta : null)

									});
								}

								else if (ssItem.MoveOutFromStep != null)
								{
									ssItemsByStep.Add(new ScheduleSSDetailsEx
									{
										SS = ssItem.SS
										,
										MoveBackFromStep = (ssItem.MoveBackFromStep != null ? ssItem.MoveBackFromStep : null)
										,
										strMoveBackFromStep = (ssItem.MoveBackFromStep != null ? ssItem.MoveBackFromStep.Name : "")
										,
										MoveBackFromStepWorkflow = (ssItem.MoveBackFromStep != null ? ssItem.MoveBackFromStep.Parent as RevisionedObjectRef : null)

										,
										MoveBackToStep = (ssItem.MoveBackToStep != null ? ssItem.MoveBackToStep : null)
										,
										strMoveBackToStep = (ssItem.MoveBackToStep != null ? ssItem.MoveBackToStep.Name : "")
										,
										MoveBackToStepWorkflow = (ssItem.MoveBackToStep != null ? ssItem.MoveBackToStep.Parent as RevisionedObjectRef : null)

										,
										MoveOutFromStep = (ssItem.MoveOutFromStep != null ? ssItem.MoveOutFromStep : null)
										,
										strMoveOutFromStep = (ssItem.MoveOutFromStep != null ? ssItem.MoveOutFromStep.Name : "")
										,
										MoveOutFromStepWorkflow = (ssItem.MoveOutFromStep != null ? ssItem.MoveOutFromStep.Parent as RevisionedObjectRef : null)

										,
										MoveOutToStep = (ssItem.MoveOutToStep != null ? ssItem.MoveOutToStep : null)
										,
										strMoveOutToStep = (ssItem.MoveOutToStep != null ? ssItem.MoveOutToStep.Name : "")
										,
										MoveOutToStepWorkflow = (ssItem.MoveOutToStep != null ? ssItem.MoveOutToStep.Parent as RevisionedObjectRef : null)

										,
										LotId = (ssItem.LotId != null ? ssItem.LotId : null)
										,
										IgnoreMainLot = (ssItem.IgnoreMainLot != null ? ssItem.IgnoreMainLot : true)
										,
										RPTBulkDelta = (ssItem.RPTBulkDelta != null ? ssItem.RPTBulkDelta : null)
										,
										RPTUnitDelta = (ssItem.RPTUnitDelta != null ? ssItem.RPTUnitDelta : null)

									});
								}
							}

							(SSDetailsGrid.GridContext as BoundContext).Data = ssItems.ToArray();
							SSDetailsGrid.BoundContext.LoadData();
							(SSDetailsByStepGrid.GridContext as BoundContext).Data = ssItemsByStep.ToArray();
							SSDetailsByStepGrid.BoundContext.LoadData();
						}

                        //TestParamsDetails Grid
                        //Casting different data types in a list. Recevicing one data type and submitting another.					
                        if ((oResult.Value as LotScheduleModify).ScheduleTestParams != null)
                        {
                            JQDataGrid TestParamsDetailsGrid = Page.FindCamstarControl("LotScheduleModify_TestParamsDetails") as JQDataGrid;
                            List<ScheduleTestParamsDetails> Items = new List<ScheduleTestParamsDetails>();
                            foreach (ScheduleTestParams Item in (oResult.Value as LotScheduleModify).ScheduleTestParams)
                            {
                                Items.Add(new ScheduleTestParamsDetails
                                {
                                    ProcessSpec = Item.ProcessSpec
                                    ,
                                    ProcessType = Item.ProcessType
                                    ,
                                    Spec = Item.Spec
                                    ,
                                    TestProgramName = Item.TestProgramName
                                    ,
                                    TestProgramMajorRevision = Item.TestProgramMajorRevision
                                    ,
                                    TestProgramMinorRevision = Item.TestProgramMinorRevision
                                    ,
                                    TestTemperature = Item.TestTemperature
                                    ,
                                    TestCode = Item.TestCode

                                });
                            }

                            (TestParamsDetailsGrid.GridContext as BoundContext).Data = Items.ToArray();
                            TestParamsDetailsGrid.BoundContext.LoadData();

                            //Select all row in grid by default
                            int countRows = TestParamsDetailsGrid.GridContext.DataWindow.Rows.Count;

                            for (int row = 0; row < countRows; row++)
                            {
                                TestParamsDetailsGrid.GridContext.SelectRow(TestParamsDetailsGrid.GridContext.GetRowId(row), true);
                            }
                        }

                        //Schedule In Process Split
                        //Casting different data types in a list. Recevicing one data type and submitting another.					
                        if ((oResult.Value as LotScheduleModify).ScheduleInProcessSplit != null)
                        {
                            JQDataGrid grid = Page.FindCamstarControl("LotScheduleModify_InProcessSplitDetails") as JQDataGrid;
                            List<ScheduleInProcessSplitDetails> Items = new List<ScheduleInProcessSplitDetails>();
                            foreach (ScheduleInProcessSplit Item in (oResult.Value as LotScheduleModify).ScheduleInProcessSplit)
                            {
                                Items.Add(new ScheduleInProcessSplitDetails
                                {
                                    Spec = Item.Spec
                                    ,
                                    LotId = Item.LotId
                                    ,
                                    Qty = Item.Qty
                                    ,
                                    QtyModifiable = Item.QtyModifiable
                                    ,
                                    PrintingRequired = Item.PrintingRequired
                                    ,
                                    Status = Item.Status
                                });
                            }


                            (grid.GridContext as BoundContext).Data = Items.ToArray();
                            grid.BoundContext.LoadData();
                        }

                        //Packing Details
                        //Casting different data types in a list. Recevicing one data type and submitting another.					
                        if ((oResult.Value as LotScheduleModify).SchedulePacking != null)
                        {
                            JQDataGrid grid = Page.FindCamstarControl("LotScheduleModify_PackingDetails") as JQDataGrid;
                            List<SchedulePackingDetails> Items = new List<SchedulePackingDetails>();
                            foreach (SchedulePacking Item in (oResult.Value as LotScheduleModify).SchedulePacking)
                            {
                                Items.Add(new SchedulePackingDetails
                                {
                                    Spec = Item.Spec
                                    ,
                                    LotId = Item.LotId
                                    ,
                                    Qty = Item.Qty
                                    ,
                                    Status = Item.Status
                                });
                            }


                            (grid.GridContext as BoundContext).Data = Items.ToArray();
                            grid.BoundContext.LoadData();
                        }

                    }
                    else
                        DisplayMessage(resultStatus);
                }
            }
            catch (Exception Ex)
            {
                DisplayMessage(new OM.ResultStatus(Ex.TargetSite.Name + "(): " + Ex.Message, false));
            }
            finally
            {
                _txtSelectionId.Focus();
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Clears data from the form
        /// </summary>
        private void ClearData()
        {
            Page.ClearValues();
            _txtSelectionId.Focus();
            containerSelectionValues.ClearData();
            _subScheduleData.DropDownControl.Items.Clear();
            _envSelectedLots.ClearData();
            _gridSpecsSelection.ClearData();
            _gridSSDetails.ClearData();
            OnLoad(null);
        }

        //-----------------------------------------
        // Get Schedule SS Specs
        //-----------------------------------------
        public virtual void GetScheduleSSSpecs(string ServiceType, bool IsSS, string ProcessSpecName, string ProcessSpecRevision)
        {
            try
            {
                // Prepare service
                var fs = FrameworkManagerUtil.GetFrameworkSession();
                ResultStatus oServiceResult = new ResultStatus(null, false);
                string sServiceType = "LotScheduleModify";
                if (Page.PrimaryServiceType != null)
                    sServiceType = Page.PrimaryServiceType.ToString();

                // Run proper constructor. We need to be dynamic with the primary service type
                var svcType = WCFObject.CreateObjectType(sServiceType + "Service");
                //create a request object
                var oRequest = WCFObject.CreateObject(sServiceType + "_Request");
                var svcConstructor = svcType.GetConstructor(new Type[] { typeof(UserProfile) });
                var oService = svcConstructor.Invoke(new object[] { fs.CurrentUserProfile });

                //retriving data dynamically for Flexibility of the page.
                var oServiceData = CreateServiceData(sServiceType);

                var info = CreateServiceInfo(sServiceType);
                var oServiceInfo = info as LotScheduleModify_Info;
                (oRequest as Request).Info = oServiceInfo;

                if (IsSS)
                {
                    (oServiceData as LotScheduleModify).SS = new RevisionedObjectRef();
                    (oServiceData as LotScheduleModify).SS.Name = ProcessSpecName;
                    if (ProcessSpecRevision != "")
                    {
                        (oServiceData as LotScheduleModify).SS.Revision = ProcessSpecRevision;
                        (oServiceData as LotScheduleModify).SS.RevisionOfRecord = false;
                    }
                    else
                    {
                        (oServiceData as LotScheduleModify).SS.RevisionOfRecord = true;
                    }
                }
                else
                {
                    (oServiceData as LotScheduleModify).ProcessSpec = new RevisionedObjectRef();
                    (oServiceData as LotScheduleModify).ProcessSpec.Name = ProcessSpecName;
                    if (ProcessSpecRevision != "")
                    {
                        (oServiceData as LotScheduleModify).ProcessSpec.Revision = ProcessSpecRevision;
                        (oServiceData as LotScheduleModify).ProcessSpec.RevisionOfRecord = false;
                    }
                    else
                    {
                        (oServiceData as LotScheduleModify).ProcessSpec.RevisionOfRecord = true;
                    }
                }

                // Request Selection Value
                if (IsSS)
                    oServiceInfo.SSMoveOutToSpec = FieldInfoUtil.RequestSelectionValue();
                else
                    oServiceInfo.SSMoveOutFromSpec = FieldInfoUtil.RequestSelectionValue();

                // Perform the request
                Result oResult = new Result();
                ResultStatus oResultStatus = new ResultStatus();

                oResultStatus = (oService as IShopFloorBase).GetEnvironment(oServiceData, (oRequest as Request), out oResult);

                if (oResultStatus.IsSuccess)
                {
                    if (IsSS)
                    {
                        _gridSpecsSelection.ClearData();
                        _gridSpecsSelection.Data = (oResult.Environment as LotScheduleModify_Environment).SSMoveOutToSpec.SelectionValues.GetAsExplicitlyDataTable();
                        _gridSpecsSelection.OriginalData = (oResult.Environment as LotScheduleModify_Environment).SSMoveOutToSpec.SelectionValues.GetAsExplicitlyDataTable();
                    }
                    else
                    {
                        _gridSpecsSelection.ClearData();
                        _gridSpecsSelection.Data = (oResult.Environment as LotScheduleModify_Environment).SSMoveOutFromSpec.SelectionValues.GetAsExplicitlyDataTable();
                        _gridSpecsSelection.OriginalData = (oResult.Environment as LotScheduleModify_Environment).SSMoveOutFromSpec.SelectionValues.GetAsExplicitlyDataTable();
                    }
                }
            }
            catch (Exception ex)
            {
                Page.StatusBar.WriteError(ex.Message.ToString());
            }
        }

        //-----------------------------------------
        // Popup Data Selection
        //-----------------------------------------
        public virtual void PopupDataSelection(string Key, bool EndResponse = false)
        {
            try
            {
				if (Page.DataContract.GetValueByName("LotScheduleModify_GridRowIdDM") != "" || Page.DataContract.GetValueByName("LotScheduleModify_StepGridRowIdDM") != "")
                {
                    bool bIsSS = false;
                    string sSSKey = "";
                    string sSSValue = "";
					if (Key == "MoveOutFromSpec" || Key == "MoveBackToSpec" || Key == "MoveOutFromStep" || Key == "MoveBackToStep")
                    {
                        bIsSS = false;
                        if (_ProcessSpec.Data != null)
                            sSSValue = _ProcessSpec.Data.ToString();
                    }
                    else
					{
						bIsSS = true;
						if (Page.EventTarget.Contains(_gridSSDetails.ID))
						{
							if (_gridSSDetails.GridContext.GetCell(Page.DataContract.GetValueByName("LotScheduleModify_GridRowIdDM").ToString(), "SS") != null)
								sSSValue = _gridSSDetails.GridContext.GetCell(Page.DataContract.GetValueByName("LotScheduleModify_GridRowIdDM").ToString(), "SS").ToString();
						}
						else if (Page.EventTarget.Contains(_gridSSByStepDetails.ID))
						{
							if (_gridSSByStepDetails.GridContext.GetCell(Page.DataContract.GetValueByName("LotScheduleModify_StepGridRowIdDM").ToString(), "SS") != null)
								sSSValue = _gridSSByStepDetails.GridContext.GetCell(Page.DataContract.GetValueByName("LotScheduleModify_StepGridRowIdDM").ToString(), "SS").ToString();
						}
					}
                    if (sSSValue != "")
                    {
                        string[] splitsSSValue = sSSValue.Split(':');
                        if (splitsSSValue.Length > 1)
                            GetScheduleSSSpecs(Page.PrimaryServiceType, bIsSS, splitsSSValue[0], splitsSSValue[1]);
                        else
                            GetScheduleSSSpecs(Page.PrimaryServiceType, bIsSS, sSSValue, "");

						if (_gridSpecsSelection.Data != null || _SupplementarySpec.Data != null)
                        {
                            Camstar.WebPortal.Personalization.FloatPageOpenAction objAction = new FloatPageOpenAction();
                            objAction.PageName = "SS_DataSelectionValuesPopupVP";

							int i;
							if (Key == "MoveOutFromStep" || Key == "MoveOutToStep" || Key == "MoveBackFromStep" || Key == "MoveBackToStep")
								i = 7;
							else
								i = 3;

                            UIComponentDataContractLink[] objLinks = new UIComponentDataContractLink[i];

							if (Key == "MoveOutFromStep" || Key == "MoveBackToStep")
							{
								Page.DataContract.SetValueByName("LotScheduleModify_PrimaryService", this.PrimaryServiceType);
								objAction.PageName = "SS_WorkflowStepSelectionPopupVP";
								objLinks[0] = new UIComponentDataContractLink();
								objLinks[0].SourceMember = "LotScheduleModify_PrimaryService";
								objLinks[0].TargetMember = "Popup_PrimaryService";
								objLinks[1] = new UIComponentDataContractLink();
								objLinks[1].SourceMember = "LotScheduleModify_ProcessSpecDM";
								objLinks[1].TargetMember = "Popup_ProcessSpecDM";
								objLinks[2] = new UIComponentDataContractLink();
								objLinks[2].SourceMember = "LotScheduleModify_ProcessSpeRevDM";
								objLinks[2].TargetMember = "Popup_ProcessSpecRevDM";
								objLinks[3] = new UIComponentDataContractLink();
								objLinks[3].SourceMember = "LotScheduleModify_ProcessSpecIsRORDM";
								objLinks[3].TargetMember = "Popup_ProcessSpeIsRORDM";

								objLinks[4] = new UIComponentDataContractLink();
								objLinks[5] = new UIComponentDataContractLink();
								objLinks[6] = new UIComponentDataContractLink();

								if (Page.DataContract.GetValueByName("LotScheduleModify_IsSSWorkflow") != null)
								{
									var SelectedSSWorkflow = Page.DataContract.GetValueByName("LotScheduleModify_IsSSWorkflow") as RevisionedObjectRef;
									var SelectedSSStep = Page.DataContract.GetValueByName("LotScheduleModify_IsSSStep").ToString();
									Page.DataContract.SetValueByName("LotScheduleModify_SelectedSSWorkflow", SelectedSSWorkflow.Name);
									Page.DataContract.SetValueByName("LotScheduleModify_SelectedSSWorkflowRev", SelectedSSWorkflow.Revision);
									Page.DataContract.SetValueByName("LotScheduleModify_SSSelectedStep", SelectedSSStep);

									objLinks[4].SourceMember = "LotScheduleModify_SelectedSSWorkflow";
									objLinks[5].SourceMember = "LotScheduleModify_SelectedSSWorkflowRev";
									objLinks[6].SourceMember = "LotScheduleModify_SSSelectedStep";
								}
								else
								{
									Page.DataContract.SetValueByName("LotScheduleModify_SelectedSSWorkflow", null);
									Page.DataContract.SetValueByName("LotScheduleModify_SelectedSSWorkflowRev", null);
									Page.DataContract.SetValueByName("LotScheduleModify_SSSelectedStep", null);

									objLinks[4].SourceMember = "LotScheduleModify_SelectedSSWorkflow";
									objLinks[5].SourceMember = "LotScheduleModify_SelectedSSWorkflowRev";
									objLinks[6].SourceMember = "LotScheduleModify_SSSelectedStep";
								}
								objLinks[4].TargetMember = "Popup_Workflow";
								objLinks[5].TargetMember = "Popup_WorkflowRev";
								objLinks[6].TargetMember = "Popup_SSReturnedSelectedStep";

								Page.PortalContext.DataContract.SetValueByName("LotScheduleModify_IsSSWorkflow", null);
								Page.PortalContext.DataContract.SetValueByName("LotScheduleModify_IsSSStep", null);

								objAction.FrameLocation = new UIFloatingPageLocation();
								objAction.FrameLocation.Width = 510;
								objAction.FrameLocation.Height = 480;
								objAction.EndResponse = false;
							}
							else if (Key == "MoveOutToStep" || Key == "MoveBackFromStep")
							{
								_SupplementarySpec.Data = sSSValue;
								Page.DataContract.SetValueByName("LotScheduleModify_PrimaryService", this.PrimaryServiceType);
								objAction.PageName = "SS_WorkflowStepSelectionPopupVP";
								objLinks[0] = new UIComponentDataContractLink();
								objLinks[0].SourceMember = "LotScheduleModify_PrimaryService";
								objLinks[0].TargetMember = "Popup_PrimaryService";
								objLinks[1] = new UIComponentDataContractLink();
								objLinks[1].SourceMember = "LotScheduleModify_SupplementarySpecDM";
								objLinks[1].TargetMember = "Popup_ProcessSpecDM";
								objLinks[2] = new UIComponentDataContractLink();
								objLinks[2].SourceMember = "LotScheduleModify_SupplementarySpecRevDM";
								objLinks[2].TargetMember = "Popup_ProcessSpeRevDM";
								objLinks[3] = new UIComponentDataContractLink();
								objLinks[3].SourceMember = "LotScheduleModify_SupplementarySpecIsRORDM";
								objLinks[3].TargetMember = "Popup_ProcessSpecIsRORDM";
								objLinks[4] = new UIComponentDataContractLink();
								objLinks[5] = new UIComponentDataContractLink();
								objLinks[6] = new UIComponentDataContractLink();

								if (Page.DataContract.GetValueByName("LotScheduleModify_IsSSWorkflow") != null)
								{
									var SelectedSSWorkflow = Page.DataContract.GetValueByName("LotScheduleModify_IsSSWorkflow") as RevisionedObjectRef;
									var SelectedSSStep = Page.DataContract.GetValueByName("LotScheduleModify_IsSSStep").ToString();
									Page.DataContract.SetValueByName("LotScheduleModify_SelectedSSWorkflow", SelectedSSWorkflow.Name);
									Page.DataContract.SetValueByName("LotScheduleModify_SelectedSSWorkflowRev", SelectedSSWorkflow.Revision);
									Page.DataContract.SetValueByName("LotScheduleModify_SSSelectedStep", SelectedSSStep);

									objLinks[4].SourceMember = "LotScheduleModify_SelectedSSWorkflow";
									objLinks[5].SourceMember = "LotScheduleModify_SelectedSSWorkflowRev";
									objLinks[6].SourceMember = "LotScheduleModify_SSSelectedStep";
								}
								else
								{
									Page.DataContract.SetValueByName("LotScheduleModify_SelectedSSWorkflow", null);
									Page.DataContract.SetValueByName("LotScheduleModify_SelectedSSWorkflowRev", null);
									Page.DataContract.SetValueByName("LotScheduleModify_SSSelectedStep", null);

									objLinks[4].SourceMember = "LotScheduleModify_SelectedSSWorkflow";
									objLinks[5].SourceMember = "LotScheduleModify_SelectedSSWorkflowRev";
									objLinks[6].SourceMember = "LotScheduleModify_SSSelectedStep";
								}
								objLinks[4].TargetMember = "Popup_Workflow";
								objLinks[5].TargetMember = "Popup_WorkflowRev";
								objLinks[6].TargetMember = "Popup_SSReturnedSelectedStep";

								Page.PortalContext.DataContract.SetValueByName("LotScheduleModify_IsSSWorkflow", null);
								Page.PortalContext.DataContract.SetValueByName("LotScheduleModify_IsSSStep", null);

								objAction.FrameLocation = new UIFloatingPageLocation();
								objAction.FrameLocation.Width = 510;
								objAction.FrameLocation.Height = 480;
								objAction.EndResponse = false;
							}
							else
							{
								objAction.PageName = "SS_DataSelectionValuesPopupVP";
								objLinks[0] = new UIComponentDataContractLink();
								objLinks[0].SourceMember = "LotScheduleModify_ToFromPopupKey";
								objLinks[0].TargetMember = "Popup_KeyDM";
								objLinks[1] = new UIComponentDataContractLink();
								objLinks[1].SourceMember = "LotScheduleModify_SpecsSelectionDM";
								objLinks[1].TargetMember = "Popup_SpecsSelectionDM";
								objLinks[2] = new UIComponentDataContractLink();
								objLinks[2].SourceMember = "LotScheduleModify_IsSSDM";
								objLinks[2].TargetMember = "Popup_IsSSDM";

								objAction.FrameLocation = new UIFloatingPageLocation();
								objAction.FrameLocation.Width = 380;
								objAction.FrameLocation.Height = 480;
								objAction.EndResponse = false;
							}
							
                            UIComponentDataContractReturnLink[] objReturnLinks = new UIComponentDataContractReturnLink[2];
                            objReturnLinks[0] = new UIComponentDataContractReturnLink();
                            objReturnLinks[1] = new UIComponentDataContractReturnLink();
                            if (Key == "MoveOutFromSpec")
                            {
                                objReturnLinks[0].SourceMember = "Popup_ReturnSelectedSpecDM";
                                objReturnLinks[0].TargetMember = "LotScheduleModify_ReturnedMoveOutFromSpec";
                                objReturnLinks[1].SourceMember = "Popup_ReturnSelectedSpecRevDM";
                                objReturnLinks[1].TargetMember = "LotScheduleModify_ReturnedMoveOutFromSpecRev";
                            }
                            else if (Key == "MoveOutToSpec")
                            {
                                objReturnLinks[0].SourceMember = "Popup_ReturnSelectedSpecDM";
                                objReturnLinks[0].TargetMember = "LotScheduleModify_ReturnedMoveOutToSpec";
                                objReturnLinks[1].SourceMember = "Popup_ReturnSelectedSpecRevDM";
                                objReturnLinks[1].TargetMember = "LotScheduleModify_ReturnedMoveOutToSpecRev";
                            }
                            else if (Key == "MoveBackFromSpec")
                            {
                                objReturnLinks[0].SourceMember = "Popup_ReturnSelectedSpecDM";
                                objReturnLinks[0].TargetMember = "LotScheduleModify_ReturnedMoveBackFromSpec";
                                objReturnLinks[1].SourceMember = "Popup_ReturnSelectedSpecRevDM";
                                objReturnLinks[1].TargetMember = "LotScheduleModify_ReturnedMoveBackFromSpecRev";
                            }
                            else if (Key == "MoveBackToSpec")
                            {
                                objReturnLinks[0].SourceMember = "Popup_ReturnSelectedSpecDM";
                                objReturnLinks[0].TargetMember = "LotScheduleModify_ReturnedMoveBackToSpec";
                                objReturnLinks[1].SourceMember = "Popup_ReturnSelectedSpecRevDM";
                                objReturnLinks[1].TargetMember = "LotScheduleModify_ReturnedMoveBackToSpecRev";
                            }
							else if (Key == "MoveOutFromStep")
							{
								objReturnLinks[0].SourceMember = "Popup_ReturnedSelectedStep";
								objReturnLinks[0].TargetMember = "LotScheduleModify_ReturnedMoveOutFromStep";
								objReturnLinks[1].SourceMember = "Popup_ReturnedSelectedWorkflow";
								objReturnLinks[1].TargetMember = "LotScheduleModify_ReturnedWorkflow";
							}
							else if (Key == "MoveOutToStep")
							{
								objReturnLinks[0].SourceMember = "Popup_ReturnedSelectedStep";
								objReturnLinks[0].TargetMember = "LotScheduleModify_ReturnedMoveOutToStep";
								objReturnLinks[1].SourceMember = "Popup_ReturnedSelectedWorkflow";
								objReturnLinks[1].TargetMember = "LotScheduleModify_ReturnedSSWorkflow";
							}
							else if (Key == "MoveBackFromStep")
							{
								objReturnLinks[0].SourceMember = "Popup_ReturnedSelectedStep";
								objReturnLinks[0].TargetMember = "LotScheduleModify_ReturnedMoveBackFromStep";
								objReturnLinks[1].SourceMember = "Popup_ReturnedSelectedWorkflow";
								objReturnLinks[1].TargetMember = "LotScheduleModify_ReturnedSSWorkflow";
							}
							else if (Key == "MoveBackToStep")
							{
								objReturnLinks[0].SourceMember = "Popup_ReturnedSelectedStep";
								objReturnLinks[0].TargetMember = "LotScheduleModify_ReturnedMoveBackToStep";
								objReturnLinks[1].SourceMember = "Popup_ReturnedSelectedWorkflow";
								objReturnLinks[1].TargetMember = "LotScheduleModify_ReturnedWorkflow";
							}

							objAction.DataContractMap = new UIComponentDataContractMap();
							objAction.DataContractMap.Links = objLinks;
							objAction.DataContractReturnMap = new UIComponentDataContractReturnMap();
							objAction.DataContractReturnMap.ReturnLinks = objReturnLinks;

							Page.ActionDispatcher.ExecuteAction(objAction);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Page.StatusBar.WriteError(ex.Message.ToString());
            }
        }

        #endregion

        #region Page Events


        /// <summary>
        /// This event is fired when the submit action button is clicked.
        /// </summary>
        /// <param name="serviceData"></param>
        public override void GetInputData(Service serviceData)
        {
            base.GetInputData(serviceData);

            try
            {
                //Manual Submission of data
                //InProcessSplitDetails
                JQDataGrid grid = Page.FindCamstarControl("LotScheduleModify_InProcessSplitDetails") as JQDataGrid;
                if (grid != null)
                {
                    ScheduleInProcessSplitDetails[] items = (grid.GridContext as BoundContext).Data as ScheduleInProcessSplitDetails[];

                    if (items != null)
                    {
                        (serviceData as LotScheduleModify).InProcessSplitDetails = items;//newItems
                    }
                }

                //PackingDetails
                JQDataGrid gridPacking = Page.FindCamstarControl("LotScheduleModify_PackingDetails") as JQDataGrid;
                if (gridPacking != null)
                {
                    SchedulePackingDetails[] items = (gridPacking.GridContext as BoundContext).Data as SchedulePackingDetails[];

                    if (items != null)
                    {
                        (serviceData as LotScheduleModify).PackingDetails = items;//newItems
                    }
                }

                //TestParamsDetails
                JQDataGrid TestParamsDetailsGrid = Page.FindCamstarControl("LotScheduleModify_TestParamsDetails") as JQDataGrid;

                var selectedItems = TestParamsDetailsGrid.GridContext.GetSelectedItems(false);
                if (selectedItems != null)
                {
                    ScheduleTestParamsDetails[] newItems = selectedItems.Cast<ScheduleTestParamsDetails>().ToArray();
                    (serviceData as LotScheduleModify).TestParamsDetails = newItems;//newWafers;
                }
                //END Manual Submission of data

                (serviceData as LotScheduleModify).Container = new ContainerRef(_txtSelectionId.TextControl.Text);

                //SS Details
                if (_gridSSDetails.Data != null)
                {
                    ScheduleSSDetails[] getSSDetails = _gridSSDetails.Data as ScheduleSSDetails[];
					ScheduleSSDetailsEx[] getSSDetailsByStep = _gridSSByStepDetails.Data as ScheduleSSDetailsEx[];

					int iSSBySpecCount = 0;
					int iSSByStepCount = 0;
					if (getSSDetails != null)
						iSSBySpecCount = getSSDetails.Length;
					if (getSSDetailsByStep != null)
						iSSByStepCount = getSSDetailsByStep.Length;

					(serviceData as LotScheduleModify).SSDetails = new ScheduleSSDetails[iSSBySpecCount + iSSByStepCount];

					//Submit SS Details
					int index = 0;

					for (int gridSSBySpecIndex = 0; gridSSBySpecIndex < iSSBySpecCount; gridSSBySpecIndex++, index++)
					{
						(serviceData as LotScheduleModify).SSDetails[index] = new ScheduleSSDetails();
						(serviceData as LotScheduleModify).SSDetails[index].SS = getSSDetails[gridSSBySpecIndex].SS;
						(serviceData as LotScheduleModify).SSDetails[index].MoveOutFromSpec = getSSDetails[gridSSBySpecIndex].MoveOutFromSpec;
						(serviceData as LotScheduleModify).SSDetails[index].MoveOutToSpec = getSSDetails[gridSSBySpecIndex].MoveOutToSpec;
						(serviceData as LotScheduleModify).SSDetails[index].MoveBackFromSpec = getSSDetails[gridSSBySpecIndex].MoveBackFromSpec;
						(serviceData as LotScheduleModify).SSDetails[index].MoveBackToSpec = getSSDetails[gridSSBySpecIndex].MoveBackToSpec;
						(serviceData as LotScheduleModify).SSDetails[index].IgnoreMainLot = getSSDetails[gridSSBySpecIndex].IgnoreMainLot;
						(serviceData as LotScheduleModify).SSDetails[index].RPTBulkDelta = getSSDetails[gridSSBySpecIndex].RPTBulkDelta;
						(serviceData as LotScheduleModify).SSDetails[index].RPTUnitDelta = getSSDetails[gridSSBySpecIndex].RPTUnitDelta;
						(serviceData as LotScheduleModify).SSDetails[index].UseStep = false;
						if (_gridSSDetails.GridContext.GetCell(gridSSBySpecIndex, "LotId") != null)
							if (_gridSSDetails.GridContext.GetCell(gridSSBySpecIndex, "LotId").ToString() != "")
								(serviceData as LotScheduleModify).SSDetails[index].LotId = _gridSSDetails.GridContext.GetCell(gridSSBySpecIndex, "LotId").ToString();
					}
					for (int gridSSByStepIndex = 0; gridSSByStepIndex < iSSByStepCount; gridSSByStepIndex++, index++)
					{
						(serviceData as LotScheduleModify).SSDetails[index] = new ScheduleSSDetails();
						(serviceData as LotScheduleModify).SSDetails[index].SS = getSSDetailsByStep[gridSSByStepIndex].SS;
						(serviceData as LotScheduleModify).SSDetails[index].MoveOutFromStep = getSSDetailsByStep[gridSSByStepIndex].MoveOutFromStep;
						(serviceData as LotScheduleModify).SSDetails[index].MoveOutToStep = getSSDetailsByStep[gridSSByStepIndex].MoveOutToStep;
						(serviceData as LotScheduleModify).SSDetails[index].MoveBackFromStep = getSSDetailsByStep[gridSSByStepIndex].MoveBackFromStep;
						(serviceData as LotScheduleModify).SSDetails[index].MoveBackToStep = getSSDetailsByStep[gridSSByStepIndex].MoveBackToStep;
						(serviceData as LotScheduleModify).SSDetails[index].IgnoreMainLot = getSSDetailsByStep[gridSSByStepIndex].IgnoreMainLot;
						(serviceData as LotScheduleModify).SSDetails[index].RPTBulkDelta = getSSDetailsByStep[gridSSByStepIndex].RPTBulkDelta;
						(serviceData as LotScheduleModify).SSDetails[index].RPTUnitDelta = getSSDetailsByStep[gridSSByStepIndex].RPTUnitDelta;
						(serviceData as LotScheduleModify).SSDetails[index].UseStep = true;
						if (_gridSSByStepDetails.GridContext.GetCell(gridSSByStepIndex, "LotId") != null)
							if (_gridSSByStepDetails.GridContext.GetCell(gridSSByStepIndex, "LotId").ToString() != "")
								(serviceData as LotScheduleModify).SSDetails[index].LotId = _gridSSByStepDetails.GridContext.GetCell(gridSSByStepIndex, "LotId").ToString();
					}
                }

                //Get the input data
                base.GetInputData(serviceData);
                (serviceData as LotScheduleModify).ScheduleData = new SubentityRef();
                (serviceData as LotScheduleModify).ScheduleData.ID = _subScheduleData.DropDownControl.SelectedValue;
            }
            catch (Exception Ex)
            {
                DisplayMessage(new OM.ResultStatus(Ex.TargetSite.Name + "(): " + Ex.Message, false));
            }
        }

        /// <summary>
        /// On page load
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            //Clear Display message
            DisplayMessage(new ResultStatus("", true));

            // Get Computername
            txtComputerName.Data = SEMI.AppCode.UIUtility.GetComputerName(this);

            // If it is not postback
            if (!Page.IsPostBack)
            {
                if (Page.DataContract.GetValueByName("LotScheduleModify_PrimarySvcTypeTxtDM") != null && Page.DataContract.GetValueByName("LotScheduleModify_IsPopup") != null)
                {
                    string primarysvctype = Page.DataContract.GetValueByName("LotScheduleModify_PrimarySvcTypeTxtDM").ToString();
                    this.Page.PrimaryServiceType = primarysvctype;
                    GetScheduleData();
                    Page.ActionDispatcher.GetActionByName("CancelBtn").IsHidden = false;
                    Personalization.UIAction[] actUIActions = Page.ActionDispatcher.PageActions();
                    foreach (Personalization.UIAction act in actUIActions)
                    {
                        if (act.Name.ToUpper() == "SUBMITACTION")
                        {
                            (act as Personalization.SubmitAction).ServiceName = Page.DataContract.GetValueByName("LotScheduleModify_PrimarySvcTypeTxtDM").ToString();
                        }
                    }
                }
            }

            // Add ss popup key
            string ssKey = Page.PrimaryServiceType;
            switch (ssKey)
            {
                case "WaferLotScheModify":
                case "WaferSortLotScheModify":
                case "BackGrindLotScheModify":
                case "AssemblyLotScheModify":
                case "AssemblyCarrierLotScheModify":
                case "TestLotScheModify":
                case "FinalTestLotScheModify":
                    _txtSSPopupKey.Data = ssKey.Replace("LotScheModify", "") + "SS";
                    break;
                case "AssemblyMotherLotScheModify":
                case "AssemblySubLotScheModify":
                    _txtSSPopupKey.Data = ssKey.Replace("ScheModify", "") + "SS";
                    break;
                default:
                    _txtSSPopupKey.Data = "SupplementarySpec";
                    break;
            }

            // Add To From popup key
            _txtToFromPopupKey.Data = "MoveOutToSpec";

            // Add Selected Row Id
			if (Page.DataContract.GetValueByName("LotScheduleModify_GridRowIdDM") != null)
				_txtSelectedRowId.Data = Page.DataContract.GetValueByName("LotScheduleModify_GridRowIdDM").ToString();
			else if (Page.DataContract.GetValueByName("LotScheduleModify_StepGridRowIdDM") != null)
				_txtSelectedRowId.Data = Page.DataContract.GetValueByName("LotScheduleModify_StepGridRowIdDM").ToString();



            // Check if it is a pop up close, get the return result
            if (SEMI.AppCode.UIUtility.IsPopupClose(this))
            {
                if (_txtSelectedRowId.Data != null)
                {
                    int selectedRowId = Convert.ToInt32(_txtSelectedRowId.Data.ToString());
                    string selectedSpec = "";
                    string selectedSpecRev = "";
					CWC.NamedSubentity selectedStep;
					RevisionedObjectRef selectedWorkflow;
                    string attrVal = "";
                    string attrRev = "";
					
                    ScheduleSSDetails[] getSSPlanDetails = _gridSSDetails.Data as ScheduleSSDetails[];
					ScheduleSSDetailsEx[] getSSPlanDetailsByStep = _gridSSByStepDetails.Data as ScheduleSSDetailsEx[];

					if (Page.PortalContext.DataContract.GetValueByName<string>("LotScheduleModify_ReturnedSSValue") != null)
					{
						attrVal = Page.PortalContext.DataContract.GetValueByName<string>("LotScheduleModify_ReturnedSSValue").ToString();
						attrRev = Page.PortalContext.DataContract.GetValueByName<string>("LotScheduleModify_ReturnedSSRevision").ToString();
						if (Page.PortalContext.DataContract.GetValueByName("LotScheduleModify_GridRowIdDM") != null)
						{
							getSSPlanDetails[selectedRowId].SS = new RevisionedObjectRef();
							getSSPlanDetails[selectedRowId].SS.Name = attrVal;
							getSSPlanDetails[selectedRowId].SS.Revision = attrRev;
						}
						else if (Page.PortalContext.DataContract.GetValueByName("LotScheduleModify_StepGridRowIdDM") != null)
						{
							if (getSSPlanDetailsByStep[selectedRowId].SS != null && (getSSPlanDetailsByStep[selectedRowId].SS.Name != attrVal || getSSPlanDetailsByStep[selectedRowId].SS.Revision != attrRev))
							{
								getSSPlanDetailsByStep[selectedRowId].MoveOutToStep = null;
								getSSPlanDetailsByStep[selectedRowId].strMoveOutToStep = null;
								getSSPlanDetailsByStep[selectedRowId].MoveOutToStepWorkflow = null;
								getSSPlanDetailsByStep[selectedRowId].MoveBackFromStep = null;
								getSSPlanDetailsByStep[selectedRowId].strMoveBackFromStep = null;
								getSSPlanDetailsByStep[selectedRowId].MoveBackFromStepWorkflow = null;
							}
							getSSPlanDetailsByStep[selectedRowId].SS = new RevisionedObjectRef();
							getSSPlanDetailsByStep[selectedRowId].SS.Name = attrVal;
							getSSPlanDetailsByStep[selectedRowId].SS.Revision = attrRev;
							_SupplementarySpec.Data = _gridSSByStepDetails.GridContext.GetCell(_txtSelectedRowId.Data.ToString(), "SS");
						}
						Page.PortalContext.DataContract.SetValueByName("LotScheduleModify_ReturnedSSValue", null);
						Page.PortalContext.DataContract.SetValueByName("LotScheduleModify_ReturnedSSRevision", null);
					}

                    else if (Page.PortalContext.DataContract.GetValueByName<string>("LotScheduleModify_ReturnedMoveOutFromSpec") != null)
                    {
                        selectedSpec = Page.PortalContext.DataContract.GetValueByName<string>("LotScheduleModify_ReturnedMoveOutFromSpec").ToString();
                        selectedSpecRev = Page.PortalContext.DataContract.GetValueByName<string>("LotScheduleModify_ReturnedMoveOutFromSpecRev").ToString();
                        getSSPlanDetails[selectedRowId].MoveOutFromSpec = new RevisionedObjectRef();
                        getSSPlanDetails[selectedRowId].MoveOutFromSpec.Name = selectedSpec;
                        getSSPlanDetails[selectedRowId].MoveOutFromSpec.Revision = selectedSpecRev;
                        Page.PortalContext.DataContract.SetValueByName("LotScheduleModify_ReturnedMoveOutFromSpec", null);
                        Page.PortalContext.DataContract.SetValueByName("LotScheduleModify_ReturnedMoveOutFromSpecRev", null);
                    }
                    else if (Page.PortalContext.DataContract.GetValueByName<string>("LotScheduleModify_ReturnedMoveOutToSpec") != null)
                    {
                        selectedSpec = Page.PortalContext.DataContract.GetValueByName<string>("LotScheduleModify_ReturnedMoveOutToSpec").ToString();
                        selectedSpecRev = Page.PortalContext.DataContract.GetValueByName<string>("LotScheduleModify_ReturnedMoveOutToSpecRev").ToString();
                        getSSPlanDetails[selectedRowId].MoveOutToSpec = new RevisionedObjectRef();
                        getSSPlanDetails[selectedRowId].MoveOutToSpec.Name = selectedSpec;
                        getSSPlanDetails[selectedRowId].MoveOutToSpec.Revision = selectedSpecRev;
                        Page.PortalContext.DataContract.SetValueByName("LotScheduleModify_ReturnedMoveOutToSpec", null);
                        Page.PortalContext.DataContract.SetValueByName("LotScheduleModify_ReturnedMoveOutToSpecRev", null);
                    }
                    else if (Page.PortalContext.DataContract.GetValueByName<string>("LotScheduleModify_ReturnedMoveBackFromSpec") != null)
                    {
                        selectedSpec = Page.PortalContext.DataContract.GetValueByName<string>("LotScheduleModify_ReturnedMoveBackFromSpec").ToString();
                        selectedSpecRev = Page.PortalContext.DataContract.GetValueByName<string>("LotScheduleModify_ReturnedMoveBackFromSpecRev").ToString();
                        getSSPlanDetails[selectedRowId].MoveBackFromSpec = new RevisionedObjectRef();
                        getSSPlanDetails[selectedRowId].MoveBackFromSpec.Name = selectedSpec;
                        getSSPlanDetails[selectedRowId].MoveBackFromSpec.Revision = selectedSpecRev;
                        Page.PortalContext.DataContract.SetValueByName("LotScheduleModify_ReturnedMoveBackFromSpec", null);
                        Page.PortalContext.DataContract.SetValueByName("LotScheduleModify_ReturnedMoveBackFromSpecRev", null);
                    }
                    else if (Page.PortalContext.DataContract.GetValueByName<string>("LotScheduleModify_ReturnedMoveBackToSpec") != null)
                    {
                        selectedSpec = Page.PortalContext.DataContract.GetValueByName<string>("LotScheduleModify_ReturnedMoveBackToSpec").ToString();
                        selectedSpecRev = Page.PortalContext.DataContract.GetValueByName<string>("LotScheduleModify_ReturnedMoveBackToSpecRev").ToString();
                        getSSPlanDetails[selectedRowId].MoveBackToSpec = new RevisionedObjectRef();
                        getSSPlanDetails[selectedRowId].MoveBackToSpec.Name = selectedSpec;
                        getSSPlanDetails[selectedRowId].MoveBackToSpec.Revision = selectedSpecRev;
                        Page.PortalContext.DataContract.SetValueByName("LotScheduleModify_ReturnedMoveBackToSpec", null);
                        Page.PortalContext.DataContract.SetValueByName("LotScheduleModify_ReturnedMoveBackToSpecRev", null);
                    }
					else if (Page.PortalContext.DataContract.GetValueByName("LotScheduleModify_ReturnedMoveOutFromStep") != null)
					{
						selectedStep = Page.PortalContext.DataContract.GetValueByName("LotScheduleModify_ReturnedMoveOutFromStep") as Camstar.WebPortal.FormsFramework.WebControls.NamedSubentity;
						selectedWorkflow = Page.PortalContext.DataContract.GetValueByName("LotScheduleModify_ReturnedWorkflow") as RevisionedObjectRef;
						getSSPlanDetailsByStep[selectedRowId].MoveOutFromStep = new NamedSubentityRef();
						getSSPlanDetailsByStep[selectedRowId].MoveOutFromStep = (selectedStep as Camstar.WebPortal.FormsFramework.WebControls.NamedSubentity).Data as NamedSubentityRef;
						getSSPlanDetailsByStep[selectedRowId].MoveOutFromStep.Parent = selectedWorkflow as BaseObjectRef;
						getSSPlanDetailsByStep[selectedRowId].strMoveOutFromStep = (selectedStep.Data as NamedSubentityRef).Name;
						getSSPlanDetailsByStep[selectedRowId].MoveOutFromStepWorkflow = selectedWorkflow as RevisionedObjectRef;
						Page.PortalContext.DataContract.SetValueByName("LotScheduleModify_ReturnedMoveOutFromStep", null);
						Page.PortalContext.DataContract.SetValueByName("LotScheduleModify_ReturnedWorkflow", null);
					}
					else if (Page.PortalContext.DataContract.GetValueByName("LotScheduleModify_ReturnedMoveOutToStep") != null)
					{
						selectedStep = Page.PortalContext.DataContract.GetValueByName("LotScheduleModify_ReturnedMoveOutToStep") as Camstar.WebPortal.FormsFramework.WebControls.NamedSubentity;
						selectedWorkflow = Page.PortalContext.DataContract.GetValueByName("LotScheduleModify_ReturnedSSWorkflow") as RevisionedObjectRef;
						getSSPlanDetailsByStep[selectedRowId].MoveOutToStep = new NamedSubentityRef();
						getSSPlanDetailsByStep[selectedRowId].MoveOutToStep = (selectedStep as Camstar.WebPortal.FormsFramework.WebControls.NamedSubentity).Data as NamedSubentityRef;
						getSSPlanDetailsByStep[selectedRowId].MoveOutToStep.Parent = selectedWorkflow as BaseObjectRef;
						getSSPlanDetailsByStep[selectedRowId].strMoveOutToStep = (selectedStep.Data as NamedSubentityRef).Name;
						getSSPlanDetailsByStep[selectedRowId].MoveOutToStepWorkflow = selectedWorkflow as RevisionedObjectRef;
						Page.PortalContext.DataContract.SetValueByName("LotScheduleModify_ReturnedMoveOutToStep", null);
						Page.PortalContext.DataContract.SetValueByName("LotScheduleModify_ReturnedSSWorkflow", null);
					}
					else if (Page.PortalContext.DataContract.GetValueByName("LotScheduleModify_ReturnedMoveBackFromStep") != null)
					{
						selectedStep = Page.PortalContext.DataContract.GetValueByName("LotScheduleModify_ReturnedMoveBackFromStep") as Camstar.WebPortal.FormsFramework.WebControls.NamedSubentity;
						selectedWorkflow = Page.PortalContext.DataContract.GetValueByName("LotScheduleModify_ReturnedSSWorkflow") as RevisionedObjectRef;
						getSSPlanDetailsByStep[selectedRowId].MoveBackFromStep = new NamedSubentityRef();
						getSSPlanDetailsByStep[selectedRowId].MoveBackFromStep = (selectedStep as Camstar.WebPortal.FormsFramework.WebControls.NamedSubentity).Data as NamedSubentityRef;
						getSSPlanDetailsByStep[selectedRowId].MoveBackFromStep.Parent = selectedWorkflow as BaseObjectRef;
						getSSPlanDetailsByStep[selectedRowId].strMoveBackFromStep = (selectedStep.Data as NamedSubentityRef).Name;
						getSSPlanDetailsByStep[selectedRowId].MoveBackFromStepWorkflow = selectedWorkflow as RevisionedObjectRef;
						Page.PortalContext.DataContract.SetValueByName("LotScheduleModify_ReturnedMoveBackFromStep", null);
						Page.PortalContext.DataContract.SetValueByName("LotScheduleModify_ReturnedSSWorkflow", null);
					}
					else if (Page.PortalContext.DataContract.GetValueByName("LotScheduleModify_ReturnedMoveBackToStep") != null)
					{
						selectedStep = Page.PortalContext.DataContract.GetValueByName("LotScheduleModify_ReturnedMoveBackToStep") as Camstar.WebPortal.FormsFramework.WebControls.NamedSubentity;
						selectedWorkflow = Page.PortalContext.DataContract.GetValueByName("LotScheduleModify_ReturnedWorkflow") as RevisionedObjectRef;
						getSSPlanDetailsByStep[selectedRowId].MoveBackToStep = new NamedSubentityRef();
						getSSPlanDetailsByStep[selectedRowId].MoveBackToStep = (selectedStep as Camstar.WebPortal.FormsFramework.WebControls.NamedSubentity).Data as NamedSubentityRef;
						getSSPlanDetailsByStep[selectedRowId].MoveBackToStep.Parent = selectedWorkflow as BaseObjectRef;
						getSSPlanDetailsByStep[selectedRowId].strMoveBackToStep = (selectedStep.Data as NamedSubentityRef).Name;
						getSSPlanDetailsByStep[selectedRowId].MoveBackToStepWorkflow = selectedWorkflow as RevisionedObjectRef;
						Page.PortalContext.DataContract.SetValueByName("LotScheduleModify_ReturnedMoveBackToStep", null);
						Page.PortalContext.DataContract.SetValueByName("LotScheduleModify_ReturnedWorkflow", null);
					}


                    _gridSSDetails.ClearData();
                    _gridSSDetails.Data = getSSPlanDetails;
                    _gridSSDetails.OriginalData = getSSPlanDetails;
					_gridSSByStepDetails.ClearData();
					_gridSSByStepDetails.Data = getSSPlanDetailsByStep;
					_gridSSByStepDetails.OriginalData = getSSPlanDetailsByStep;
					Page.PortalContext.DataContract.SetValueByName("LotSchedule_GridRowIdDM", null);
					Page.PortalContext.DataContract.SetValueByName("LotSchedule_StepGridRowIdDM", null);
                }
            }

            if (Page.IsPostBack)
            {
                // Check if it is a pop up close, get the return result
                if (SEMI.AppCode.UIUtility.IsPopupClose(this))
                {
                    if (_envSelectedLots != null)

                        // manually initialize the containers list data contract since it is not triggered when we do a manual popup close
                        if (Page.DataContract.GetValueByName("LotSchedule_SelectedLotsListDM") != null)
                        {
                            //gets the list of returned lot from the popup form
                            _envSelectedLots.SS_ContainersList = Page.DataContract.GetValueByName("LotSchedule_SelectedLotsListDM") as string[];
                        }

                    if (_envSelectedLots.SS_ContainersList != null)
                    {
                        string[] sContainers;
                        sContainers = _envSelectedLots.SS_ContainersList;
                        _txtSelectionId.TextControl.Text = sContainers[0].ToString();
                        GetScheduleData();
                    }
                    //nullify the containers list
                    _envSelectedLots.SS_ContainersList = null;
                }
            }
        }

        /// <summary>
        /// Resets the page value to default
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public override void WebPartCustomAction(object sender, Personalization.CustomActionEventArgs e)
        {
            base.WebPartCustomAction(sender, e);
            var action = e.Action as Camstar.WebPortal.Personalization.CustomAction;

            if (action != null && action.Parameters == "Reset")
            {
                ClearData();
            }
            else if (action != null && action.Parameters == "MoveOutFromSpec")
            {
                PopupDataSelection("MoveOutFromSpec");
            }
            else if (action != null && action.Parameters == "MoveOutToSpec")
            {
                PopupDataSelection("MoveOutToSpec");
            }
            else if (action != null && action.Parameters == "MoveBackFromSpec")
            {
                PopupDataSelection("MoveBackFromSpec");
            }
            else if (action != null && action.Parameters == "MoveBackToSpec")
            {
                PopupDataSelection("MoveBackToSpec");
            }
			else if (action != null && action.Parameters == "MoveOutFromStep")
			{
				PopupDataSelection("MoveOutFromStep");
			}
			else if (action != null && action.Parameters == "MoveOutToStep")
			{
				PopupDataSelection("MoveOutToStep");
			}
			else if (action != null && action.Parameters == "MoveBackFromStep")
			{
				PopupDataSelection("MoveBackFromStep");
			}
			else if (action != null && action.Parameters == "MoveBackToStep")
			{
				PopupDataSelection("MoveBackToStep");
			}
        }

        /// <summary>
        /// Excutes after the submit button is clicked
        /// </summary>
        /// <param name="status"></param>
        /// <param name="serviceData"></param>
        public override void PostExecute(OM.ResultStatus status, OM.Service serviceData)
        {
            base.PostExecute(status, serviceData);
            if (status.IsSuccess)
            {
                ClearData();
                DisplayMessage(status);
            }
            else
            {
                OnLoad(null);
            }
        }

        /// <summary>
        /// On click of Add Details
        /// </summary>
        public void AddSSPlanRefClick()
        {
            //If no data do not add.
            if (_SSPlanRef.Data != null)
            {
                try
                {
                    //clear the clear message
                    Page.StatusBar.ClearMessage();

                    // get the session and user profile
                    var fs = FrameworkManagerUtil.GetFrameworkSession();

                    // Get the service type of the page
                    string sServiceType = Page.PrimaryServiceType;

                    // Run proper constructor. We need to be dynamic with the primary service type
                    var svcType = WCFObject.CreateObjectType(sServiceType + "Service");
                    //create a request object
                    var oRequest = WCFObject.CreateObject(sServiceType + "_Request");
                    var svcConstructor = svcType.GetConstructor(new Type[] { typeof(UserProfile) });
                    var oService = svcConstructor.Invoke(new object[] { fs.CurrentUserProfile });

                    //retriving data dynamically for Flexibility of the page.
                    var data = CreateServiceData(sServiceType);
                    var oServiceData = data as LotScheduleModify;

                    var info = CreateServiceInfo(sServiceType);
                    var oServiceInfo = info as LotScheduleModify_Info;

                    //request of data to be retrived.						
                    oServiceData.SSPlanRef = new NamedObjectRef();
                    oServiceData.SSPlanRef.Name = _SSPlanRef.TextEditControl.Text;
                    oServiceInfo.SSPlanRefDetails = new ScheduleSSPlanDetails_Info();
                    oServiceInfo.SSPlanRefDetails.SS = FieldInfoUtil.RequestValue();
                    oServiceInfo.SSPlanRefDetails.MoveOutFromSpec = FieldInfoUtil.RequestValue();
                    oServiceInfo.SSPlanRefDetails.MoveOutToSpec = FieldInfoUtil.RequestValue();
                    oServiceInfo.SSPlanRefDetails.MoveBackFromSpec = FieldInfoUtil.RequestValue();
                    oServiceInfo.SSPlanRefDetails.MoveBackToSpec = FieldInfoUtil.RequestValue();
                    oServiceInfo.SSPlanRefDetails.IgnoreMainLot = FieldInfoUtil.RequestValue();
					oServiceInfo.SSPlanRefDetails.RPTBulkDelta = FieldInfoUtil.RequestValue();
					oServiceInfo.SSPlanRefDetails.RPTUnitDelta = FieldInfoUtil.RequestValue();

                    (oRequest as Request).Info = oServiceInfo;
                    // init the result object
                    Result oResult = new Result();

                    // execute to request the value
                    ResultStatus resultStatus = (oService as IShopFloorBase).GetEnvironment(oServiceData, (oRequest as Request), out oResult);

                    if (resultStatus.IsSuccess)
                    {
                        //SS Details
                        //Casting different data types in a list. Recevicing one data type and submitting another.					
                        if ((oResult.Value as LotScheduleModify).SSPlanRefDetails != null)
                        {
                            JQDataGrid SSDetailsGrid = Page.FindCamstarControl("LotScheduleModify_SSDetails") as JQDataGrid;
                            List<ScheduleSSDetails> ssItems = new List<ScheduleSSDetails>();
                            if (SSDetailsGrid.Data != null)
                            {
                                ScheduleSSDetails[] oExistingList = (SSDetailsGrid.GridContext as BoundContext).Data as ScheduleSSDetails[];
                                ssItems.AddRange(oExistingList);
                            }

                            foreach (ScheduleSSPlanDetails ssItem in (oResult.Value as LotScheduleModify).SSPlanRefDetails)
                            {
                                ssItems.Add(new ScheduleSSDetails
                                {
                                    SS = ssItem.SS
                                    ,
                                    MoveBackFromSpec = ssItem.MoveBackFromSpec
                                    ,
                                    MoveBackToSpec = ssItem.MoveBackToSpec
                                    ,
                                    MoveOutFromSpec = ssItem.MoveOutFromSpec
                                    ,
                                    MoveOutToSpec = ssItem.MoveOutToSpec
                                    ,
                                    IgnoreMainLot = ssItem.IgnoreMainLot
									,
									RPTBulkDelta = ssItem.RPTBulkDelta
									,
									RPTUnitDelta = ssItem.RPTUnitDelta

                                });
                            }

                            (SSDetailsGrid.GridContext as BoundContext).Data = ssItems.ToArray();
                            SSDetailsGrid.BoundContext.LoadData();
                            CamstarWebControl.SetRenderToClient(SSDetailsGrid);
                        }
                    }
                    else
                    {
                        DisplayMessage(resultStatus);
                    }
                }
                catch (Exception Ex)
                {
                    DisplayMessage(new OM.ResultStatus(Ex.TargetSite.Name + "(): " + Ex.Message, false));
                }
            }
        }


        #endregion

    }
}



