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
/// Summary description for SS_RestartMaxTimeWindow
/// </summary>
namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class SS_RestartMaxTimeWindow : scsShopfloorBase
    {
        #region Properties

            protected CWC.TextBox _txtSelectionId { get { return Page.FindCamstarControl("SelectionIdField") as CWC.TextBox; } }
            protected JQDataGrid _gridContainers { get { return Page.FindCamstarControl("ContainersField") as JQDataGrid; } }
            protected CWC.TextBox txtComputerName { get { return Page.FindCamstarControl("ComputerNameField") as CWC.TextBox; } }
            private SEMI.AppCode.DataEnvelopControl _envSelectedLots { get { return Page.FindCamstarControl("SelectedLotsList") as SEMI.AppCode.DataEnvelopControl; } }

        #endregion
        
        #region Page Events

            public void SelectionIdField_DataChanged()
            {
                try
                {
                    if (_txtSelectionId.Data != null)
                    {
                        Page.StatusBar.ClearMessage();

                        // get the session and user profile
                        var fs = FrameworkManagerUtil.GetFrameworkSession();

                        // init the service, service data and service info objects
                        RestartMaxTimeWindowService objSvc = new RestartMaxTimeWindowService(fs.CurrentUserProfile);
                        RestartMaxTimeWindow objSvcData = new RestartMaxTimeWindow { SelectionId = (string)_txtSelectionId.Data };
                        RestartMaxTimeWindow_Info objSvcInfo = new RestartMaxTimeWindow_Info { Containers = FieldInfoUtil.RequestValue() };

                        // init the result object
                        RestartMaxTimeWindow_Result objResult = new RestartMaxTimeWindow_Result();

                        // execute to request the value
                        ResultStatus resultStatus = objSvc.ResolveSelectionId(objSvcData, new RestartMaxTimeWindow_Request { Info = objSvcInfo }, out objResult);

                        if (resultStatus.IsSuccess)
                        {
                            JQDataGrid _gridContainers = Page.FindCamstarControl("ContainersField") as JQDataGrid;
                            foreach (OM.ContainerRef oContainer in objResult.Value.Containers)
                            {
                                // check if lot not exist in the grid else add to the grid
                                string strSelectedGridId = (_gridContainers.GridContext as BoundContext).GetRowIdByCellValue("Lot", oContainer.Name.ToString());

                                if (string.IsNullOrEmpty(strSelectedGridId))
                                {
                                    SEMI.AppCode.UIUtility.GetLotQuerySelection(this, "RestartMaxTimeWindow", oContainer.Name.ToString(), false, ref _gridContainers, "ContainersField",true);
                                }
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
                    _txtSelectionId.TextControl.Text = "";
                    _txtSelectionId.Focus();
                }
            }

            public override void GetInputData(Service serviceData)
            {
                base.GetInputData(serviceData);

                if (_gridContainers.Data != null)
                {
                    OM.RestartMaxTimeWindow svcData = serviceData as OM.RestartMaxTimeWindow;

                    int intTotalContainers = _gridContainers.BoundContext.GetTotalRows();

                    svcData.Containers = new ContainerRef[intTotalContainers];

                    // collect the containers
                    for (int x = 0; x < intTotalContainers; x++)
                    {
                        svcData.Containers[x] = new ContainerRef();
                        svcData.Containers[x].Name = _gridContainers.GridContext.GetCell(x.ToString().PadLeft(6,'0'), "Lot").ToString();
                    }
                }
            }

            protected override void OnLoad(EventArgs e)
            {
                base.OnLoad(e);
                txtComputerName.TextControl.Text = SEMI.AppCode.UIUtility.GetComputerName(this);
                
                if (Page.IsPostBack)
                    // Check if it is a pop up close, get the return result
                    if (SEMI.AppCode.UIUtility.IsPopupClose(this))
                    {
                        if (_envSelectedLots != null)
                            // manually initialize the containers list data contract since it is not triggered when we do a manual popup close
                            if (Page.DataContract.GetValueByName("RestartMaxTimeWindow_SelectedLotsListDM") != null)
                                _envSelectedLots.SS_ContainersList = Page.DataContract.GetValueByName("RestartMaxTimeWindow_SelectedLotsListDM") as string[];

                        if (_envSelectedLots.SS_ContainersList != null)
                        {
                            string[] sContainers;
                            sContainers = _envSelectedLots.SS_ContainersList;

                            JQDataGrid _gridContainers = Page.FindCamstarControl("ContainersField") as JQDataGrid;
                            foreach (string sContainersItem in sContainers)
                            {
                                // check if lot not exist in the grid else add to the grid
                                string strSelectedGridId = (_gridContainers.GridContext as BoundContext).GetRowIdByCellValue("Lot", sContainersItem);

                                if (string.IsNullOrEmpty(strSelectedGridId))
                                {
                                    SEMI.AppCode.UIUtility.GetLotQuerySelection(this, "RestartMaxTimeWindow", sContainersItem, false, ref _gridContainers, "ContainersField",true);
                                }
                            }
                            //nullify the containers list
                            _envSelectedLots.SS_ContainersList = null;
                        }
                    }
            }

            public override void WebPartCustomAction(object sender, Personalization.CustomActionEventArgs e)
            {
                base.WebPartCustomAction(sender, e);
                var action = e.Action as Camstar.WebPortal.Personalization.CustomAction;

                if (action != null && action.Parameters == "Reset")
                {
                    Page.ClearValues();
                    _gridContainers.ClearData();
                    _txtSelectionId.Focus();
                }
            }

            public override void PostExecute(OM.ResultStatus status, OM.Service serviceData)
            {
                base.PostExecute(status, serviceData);
                if (status.IsSuccess)
                {
                    Page.ClearValues();
                    _gridContainers.ClearData();
                    _txtSelectionId.Focus();
                }
            }

        #endregion

    }
}



