/* Copyright 2022 Siemens */
using System;
using System.Collections.Generic;
using System.Linq;
using Camstar.WebPortal.WebPortlets;
using Camstar.WCF.ObjectStack;
using Camstar.WebPortal.Utilities;
using Camstar.WCF.Services;
using System.Web;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using Camstar.WebPortal.FormsFramework.WebControls;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;

/// <summary>
/// Summary description for SS_LotAttributes
/// </summary>
namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class SS_LotAttributes : MatrixWebPart
    {
        #region Properties

        // TextBoxs
        CWC.TextBox _txtSelectionIdField { get { return Page.FindCamstarControl("GUIUtility_SelectionId") as CWC.TextBox; } }
        CWC.TextBox _txtLotIdField { get { return Page.FindCamstarControl("GUIUtility_LotId") as CWC.TextBox; } }
        CWC.TextBox _txtLotAttributesStartNameField { get { return Page.FindCamstarControl("GUIUtility_LotAttributesStartName") as CWC.TextBox; } }
        CWC.TextBox _txtLotAttributesStopNameField { get { return Page.FindCamstarControl("GUIUtility_LotAttributesStopName") as CWC.TextBox; } }
        // JQDataGrids
        JQDataGrid _gridLotAttributesFields { get { return Page.FindCamstarControl("GUIUtility_LotAttributes") as JQDataGrid; } }

        #endregion

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
			if (!Page.IsPostBack)
            {
                SEMI.AppCode.UIUtility.MaximizePopUp(this);
            }
        }

        public void RefreshButton_Click(object sender, EventArgs e)
        {
            try
            {
                // get the session and user profile
                var fs = FrameworkManagerUtil.GetFrameworkSession();

                GUIUtilityService oService = new GUIUtilityService(fs.CurrentUserProfile);
                GUIUtility oServiceData = new GUIUtility();
                GUIUtility_Info oServiceInfo = new GUIUtility_Info();
                ResultStatus oResultStatus = new ResultStatus();

                if (_txtLotIdField.Data == null)
                {
                    oServiceData.SelectionId = _txtSelectionIdField.Data.ToString();
                    oServiceInfo.SelectionContainer = FieldInfoUtil.RequestValue();
                }
                else
                {
                    oServiceData.Container = new ContainerRef();
                    oServiceData.Container.Name = _txtLotIdField.Data.ToString();
                }

                if (_txtLotAttributesStartNameField.Data != null)
                    oServiceData.LotAttributesStartName = _txtLotAttributesStartNameField.Data.ToString();

                if (_txtLotAttributesStopNameField.Data != null)
                    oServiceData.LotAttributesStopName = _txtLotAttributesStopNameField.Data.ToString();

                oServiceInfo.LotAttributes = new ServiceAttrsDetails_Info();
                oServiceInfo.LotAttributes.Attribute = FieldInfoUtil.RequestValue();
                oServiceInfo.LotAttributes.AttributeValue = FieldInfoUtil.RequestValue();
                oServiceInfo.LotAttributes.AttributeRevision = FieldInfoUtil.RequestValue();
                oServiceInfo.LotAttributes.FieldType = FieldInfoUtil.RequestValue();

                // Set the request
                GUIUtility_Request oServiceRequest = new GUIUtility_Request();
                oServiceRequest.Info = oServiceInfo;

                // Set the status
                GUIUtility_Result oServiceResult = new GUIUtility_Result();

                // execute the request selection values
                ResultStatus objRS = oService.ResolveSelectionId(oServiceData, oServiceRequest, out oServiceResult);

                if (objRS.IsSuccess)
                {
                    if (_txtLotIdField.Data == null)
                        _txtLotIdField.Data = oServiceResult.Value.SelectionContainer.Name.ToString();

                    // bind the results to the grid.
                    _gridLotAttributesFields.ClearData();
                    (_gridLotAttributesFields.GridContext as BoundContext).Data = oServiceResult.Value.LotAttributes;
                    _gridLotAttributesFields.BoundContext.LoadData();
                    CamstarWebControl.SetRenderToClient(_gridLotAttributesFields);
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
    }
}



