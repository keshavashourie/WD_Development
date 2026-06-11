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
/// <summary>
/// Summary description for SS_SetTestProgram
/// </summary>
namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class SS_SetTestProgram: MatrixWebPart
    {
        protected CWC.TextBox _txtSelectionID { get { return Page.FindCamstarControl("SetTestProgram_SelectionId") as CWC.TextBox; } }
        protected CWC.TextBox _txtComputerName { get { return Page.FindCamstarControl("SetTestProgram_ComputerName") as CWC.TextBox; } }
        protected CWC.NamedObject _ndoEmployee { get { return Page.FindCamstarControl("SetTestProgram_SelectionId") as CWC.NamedObject; } }
        protected ContainerListGrid _contContainer { get { return Page.FindCamstarControl("SetTestProgram_Container") as ContainerListGrid; } }
        protected JQDataGrid _gridTestParams { get { return Page.FindCamstarControl("SetTestProgram_AvailableTestParams") as JQDataGrid; } }
        protected CWC.CheckBox _chkIsActive { get { return Page.FindCamstarControl("SetTestProgram_IsActive") as CWC.CheckBox; } }

        protected bool _bIsPopup { get { return Page.IsAJAXFloatingFrame; } }
        protected CWC.Button _btnSubmit { get { return Page.FindCamstarControl("SubmitButton") as CWC.Button; } }
        protected CWC.Button _btnReset { get { return Page.FindCamstarControl("ResetButton") as CWC.Button; } }

        //---------------------------------------------------
        //
        //---------------------------------------------------
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (!Page.IsPostBack)
            {
                if (_bIsPopup)
                {
                    _btnSubmit.Visible = false;
                    _btnSubmit.Enabled = false;
                    _btnReset.Visible = false;
                    _btnReset.Visible = false;

                    FetchTestProgramsDetails();
                }
            }
            if (!_bIsPopup)
            _txtSelectionID.DataChanged += new EventHandler(_txtSelectionID_DataChanged);
        }

        //---------------------------------------------------
        //
        //---------------------------------------------------
        void _txtSelectionID_DataChanged(object sender, EventArgs e)
        {
            if (_txtSelectionID.Data != null)
            {                
                FetchTestProgramsDetails();
            }            
        }

        //---------------------------------------------------
        //
        //---------------------------------------------------
        public void FetchTestProgramsDetails()
        {
            try 
            {
                bool bExecute = false;
                if (_chkIsActive != null)
                    bExecute = _chkIsActive.CheckControl.Checked;
                if (bExecute || _bIsPopup)
                {
                    var fs = FrameworkManagerUtil.GetFrameworkSession();
                    SetTestProgram oServiceData = new SetTestProgram();
                    SetTestProgram_Info oServiceInfo = new SetTestProgram_Info();
                    SetTestProgram_Request oRequest = new SetTestProgram_Request();
                    SetTestProgram_Result oResult = new SetTestProgram_Result();
                    SetTestProgramService oService = new SetTestProgramService(fs.CurrentUserProfile);
                    ResultStatus oResultStatus = new ResultStatus();

                    oServiceData.SelectionId = _txtSelectionID.Data.ToString();

                    oServiceInfo.Containers = FieldInfoUtil.RequestValue();
                    oServiceInfo.AvailableTestParams = new SetTestProgramDetails_Info();
                    oServiceInfo.AvailableTestParams.InUse = FieldInfoUtil.RequestValue();
                    oServiceInfo.AvailableTestParams.ProcessType = FieldInfoUtil.RequestValue();
                    oServiceInfo.AvailableTestParams.TestProgramName = FieldInfoUtil.RequestValue();
                    oServiceInfo.AvailableTestParams.TestProgramMajorRevision = FieldInfoUtil.RequestValue();
                    oServiceInfo.AvailableTestParams.TestProgramMinorRevision = FieldInfoUtil.RequestValue();
                    oServiceInfo.AvailableTestParams.TestCode = FieldInfoUtil.RequestValue();
                    oServiceInfo.AvailableTestParams.TestTemperature = FieldInfoUtil.RequestValue();

                    oRequest.Info = oServiceInfo;

                    oResultStatus = oService.ResolveSelectionId(oServiceData, oRequest, out oResult);
                    if (oResultStatus.IsSuccess)
                    {
                        if (oResult.Value.Containers != null)
                            if (oResult.Value.Containers.Length > 0)
                                _contContainer.Data = oResult.Value.Containers[0];

                        if (oResult.Value.AvailableTestParams != null)
                        {
                            (_gridTestParams.GridContext as BoundContext).Data = oResult.Value.AvailableTestParams.ToArray();
                            _gridTestParams.BoundContext.LoadData();
                            CamstarWebControl.SetRenderToClient(_gridTestParams);
                        }
                        else
                        {
                            _gridTestParams.BoundContext.ClearData();
                        }
                    }
                    else
                    {
                        DisplayMessage(oResultStatus);
                    }
                }
            }
            catch (Exception ex)
            {
                DisplayMessage(new ResultStatus(ex.Message.ToString(), false));
            }
        } // FetchTestParams

        //---------------------------------------------------
        //
        //---------------------------------------------------
        public override void GetInputData(Service serviceData)
        {
             bool bExecute = false;
            if (_chkIsActive != null)
                bExecute = _chkIsActive.CheckControl.Checked;

            if (bExecute || _bIsPopup)
            {
                base.GetInputData(serviceData);

                if (serviceData is OM.SetTestProgram)
                {
                    SetTestProgramDetails[] oAvailableTestPrograms = (_gridTestParams.GridContext as BoundContext).Data as SetTestProgramDetails[];
                    List<SetTestProgramDetails> oSelectedTestPrograms = new List<SetTestProgramDetails>();
                    foreach (SetTestProgramDetails oTestProgram in oAvailableTestPrograms)
                    {
                        if (bool.Parse(oTestProgram.InUse.ToString()))
                        {
                            SetTestProgramDetails oSelectedProgram = new SetTestProgramDetails();
                            oSelectedProgram.ProcessType = oTestProgram.ProcessType;
                            oSelectedProgram.TestProgramName = oTestProgram.TestProgramName;
                            oSelectedProgram.TestProgramMajorRevision = oTestProgram.TestProgramMajorRevision;
                            oSelectedProgram.TestProgramMinorRevision = oTestProgram.TestProgramMajorRevision;
                            oSelectedProgram.TestCode = oTestProgram.TestCode;
                            oSelectedProgram.TestTemperature = oTestProgram.TestTemperature;
                            oSelectedProgram.ListItemAction = ListItemAction.Add;
                            oSelectedTestPrograms.Add(oSelectedProgram);
                        }
                    }

                    (serviceData as SetTestProgram).SelectedTestParams = new SetTestProgramDetails[oSelectedTestPrograms.Count];
                    (serviceData as SetTestProgram).SelectedTestParams = oSelectedTestPrograms.ToArray();
                }
            }
        }

        //---------------------------------------------------
        //
        //---------------------------------------------------
        public void ReloadState()
        {
            if (_txtSelectionID.Data != null)
            {
                FetchTestProgramsDetails();
            }
            else 
            {
                Page.ClearValues();
                _gridTestParams.Clear();
                _gridTestParams.ClearSelectionValues();
            }
        }
    }
}



