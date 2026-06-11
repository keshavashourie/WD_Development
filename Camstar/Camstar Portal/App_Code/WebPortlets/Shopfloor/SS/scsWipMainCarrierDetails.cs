/* Copyright 2019 Siemens */
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Camstar.WCF.Services;
using Camstar.WCF.ObjectStack;
using Camstar.WebPortal.Utilities;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.Constants;
using Camstar.WebPortal.WCFUtilities;
using Camstar.WebPortal.Personalization;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;

/// <summary>
/// Summary description for scsCarrierDetails
/// </summary>
namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class scsWipMainCarrierDetails : MatrixWebPart
    {
        #region Properties

        // TextBoxs
        private CWC.TextBox _txtSelectionIdField { get { return Page.FindCamstarControl("Main_SelectedLot") as CWC.TextBox; } }
        // JQDataGrids
        private JQDataGrid _gridCarrierData { get { return Page.FindCamstarControl("WIPMain_CarriersSelection") as JQDataGrid; } }     

        #endregion
        
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            //Page.DataContract.SetValueByName("WIPMain_SelectedLotId_DM", _txtSelectionIdField.Data);
            FetchData();
        }

        private void FetchData()
        {
            try
            {
                // get the session and user profile
                var fs = FrameworkManagerUtil.GetFrameworkSession();

                WIPMainService oService = new WIPMainService(fs.CurrentUserProfile);
                WIPMain oServiceData = new WIPMain();
                WIPMain_Info oServiceInfo = new WIPMain_Info();
                ResultStatus oResultStatus = new ResultStatus();

                //var svcType = WCFObject.CreateObjectType(sServiceType + "Service");
                //var svcConstructor = svcType.GetConstructor(new Type[] { typeof(UserProfile) });
                //var oService = svcConstructor.Invoke(new object[] { fs.CurrentUserProfile });

                //var oServiceData = CreateServiceData(sServiceType);

                //var info = CreateServiceInfo(sServiceType);
                //var oServiceInfo = info as WIPMain_Info;

                if (_txtSelectionIdField.TextControl.Text != "")
                {
                    oServiceData.SelectionId = _txtSelectionIdField.Data.ToString();

                    _gridCarrierData.ClearData();


                    oServiceInfo.Containers = FieldInfoUtil.RequestValue();
                    oServiceInfo.CarriersSelection = new CarriersSelection_Info();
                    oServiceInfo.CarriersSelection.Carrier = FieldInfoUtil.RequestValue();
                    oServiceInfo.CarriersSelection.ContainerName = FieldInfoUtil.RequestValue();


                    //Set the request
                    WIPMain_Request oServiceRequest = new WIPMain_Request();
                    oServiceRequest.Info = oServiceInfo;

                    // Set the status
                    WIPMain_Result oServiceResult = new WIPMain_Result();
                    //var oResultValue = oServiceResult.Value as WIPMain;

                    // execute the request selection values
                    ResultStatus objRS = oService.ResolveSelectionId(oServiceData, oServiceRequest, out oServiceResult);

                    if (objRS.IsSuccess)
                    {

                        if (oServiceResult.Value.CarriersSelection != null)
                        {
                            (_gridCarrierData.GridContext as BoundContext).Data = oServiceResult.Value.CarriersSelection;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

    }
}



