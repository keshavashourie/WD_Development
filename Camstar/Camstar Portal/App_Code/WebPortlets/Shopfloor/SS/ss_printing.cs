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
/// Summary description for SS_Sorting
/// </summary>
namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class SS_Printing: MatrixWebPart
    {
        protected CWC.NamedObject _ndoPrintingSetup { get { return Page.FindCamstarControl("Printing_PrintingSetup") as CWC.NamedObject; } }        
        protected JQDataGrid _gridPrintingParameters { get { return Page.FindCamstarControl("Printing_ParametersSelection") as JQDataGrid; } }
        protected CWC.TextBox _txtNumberOfCopies { get { return Page.FindCamstarControl("Printing_NumberOfCopies") as CWC.TextBox; } }
        protected CWC.NamedObject _ndoPrintingType { get { return Page.FindCamstarControl("Printing_PrintingType") as CWC.NamedObject; } }
        protected CWC.TextBox _txtPrintingeSetupDesc { get { return Page.FindCamstarControl("Printing_PrintingSetupDescription") as CWC.TextBox; } }
        protected CWC.NamedObject _ndoPrinter { get { return Page.FindCamstarControl("Printing_Printer") as CWC.NamedObject; } }

        //---------------------------------------------------
        //
        //---------------------------------------------------
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            _ndoPrintingSetup.DataChanged += new EventHandler(_ndoPrintingSetup_DataChanged);
        }

        //---------------------------------------------------
        //
        //---------------------------------------------------
        void _ndoPrintingSetup_DataChanged(object sender, EventArgs e)
        {
            if (_ndoPrintingSetup.Data != null)
            {
                FetchPrintingDetails();
            }
        }

        //---------------------------------------------------
        //
        //---------------------------------------------------
        private void FetchPrintingDetails()
        {
            var fs = FrameworkManagerUtil.GetFrameworkSession();
            OM.Printing oServiceData = new OM.Printing();
            Printing_Info oServiceInfo = new Printing_Info();
            Printing_Request oRequest = new Printing_Request();
            Printing_Result oResult = new Printing_Result();
            PrintingService oService = new PrintingService(fs.CurrentUserProfile);
            ResultStatus oResultStatus = new ResultStatus();
            
            oServiceData.PrintingSetup = _ndoPrintingSetup.Data as NamedObjectRef;
            oServiceData.ComputerName = SEMI.AppCode.UIUtility.GetComputerName(this);
            oServiceInfo.PrintingType = FieldInfoUtil.RequestValue();
            oServiceInfo.PrintingSetupDescription = FieldInfoUtil.RequestValue();
            oServiceInfo.NumberOfCopies = FieldInfoUtil.RequestValue();
            oServiceInfo.ParametersSelection = new PrintingSetupParametersChanges_Info();
            oServiceInfo.ParametersSelection.ParamName = FieldInfoUtil.RequestValue();
            oServiceInfo.ParametersSelection.DisplayText = FieldInfoUtil.RequestValue();
            oServiceInfo.ParametersSelection.DefaultValue = FieldInfoUtil.RequestValue();
            oServiceInfo.ParametersSelection.ParamType = FieldInfoUtil.RequestValue();

            oServiceInfo.Printer = FieldInfoUtil.RequestSelectionValue();

            oRequest.Info = oServiceInfo;

            oResultStatus = oService.ResolveSelectionId(oServiceData, oRequest, out oResult);
            if (oResultStatus.IsSuccess)
            {                        
                if (oResult.Value.ParametersSelection != null)
                {
                    (_gridPrintingParameters.GridContext as BoundContext).Data = oResult.Value.ParametersSelection.ToArray();
                    _gridPrintingParameters.BoundContext.LoadData();
                    CamstarWebControl.SetRenderToClient(_gridPrintingParameters);
                }
                else
                {
                    _gridPrintingParameters.BoundContext.ClearData();
                }

                _txtPrintingeSetupDesc.Data = oResult.Value.PrintingSetupDescription;
                _ndoPrintingType.Data = oResult.Value.PrintingType;
                _txtNumberOfCopies.Data = oResult.Value.NumberOfCopies;

                if (oResult.Environment.Printer.SelectionValues != null)
                {
                    _ndoPrinter.SetSelectionValues(oResult.Environment.Printer.SelectionValues);
                    _ndoPrinter.Data = oResult.Environment.Printer.SelectionValues.Rows[0].Values[0];
                }
            }
            else
            {
                DisplayMessage(oResultStatus);
            }
        }

        //---------------------------------------------------
        //
        //---------------------------------------------------
        public override void GetInputData(Service serviceData)
        {
            base.GetInputData(serviceData);

            if (serviceData is OM.Printing)
            {
                PrintingSetupParametersChanges[] oParameters = (_gridPrintingParameters.GridContext as BoundContext).Data as PrintingSetupParametersChanges[];
                if (oParameters != null)
                {
                    foreach (PrintingSetupParametersChanges oParam in oParameters)
                    {
                        oParam.ListItemIndex = null;
                        oParam.Self = null;
                        oParam.ListItemAction = ListItemAction.Add;
                    }
                }

                (serviceData as OM.Printing).Parameters = oParameters;
            }
        }

        public override void WebPartCustomAction(object sender, Personalization.CustomActionEventArgs e)
        {
            base.WebPartCustomAction(sender, e);
            var action = e.Action as Camstar.WebPortal.Personalization.CustomAction;

            if (action != null && action.Parameters == "Reset")
            {
                Page.ShopfloorReset(sender, e);
                _ndoPrinter.DropDownControl.Items.Clear();
            }
        }       
    }
}



