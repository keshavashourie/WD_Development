/* Copyright 2019 Siemens */
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
using System.Web.UI.WebControls;

/// <summary>
/// Summary description for SS_ProcessSpecParam
/// </summary>
namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class SS_ProcessSpecParam : MatrixWebPart
    {
        #region Properties

        // TextBoxs
        CWC.TextBox _txtSelectionIdField { get { return Page.FindCamstarControl("SelectionId") as CWC.TextBox; } }
        // ContainerList
        CWC.ContainerList _ContainerField { get { return Page.FindCamstarControl("Container") as CWC.ContainerList; } }
        // JQDataGrids
        JQDataGrid _gridProcessSpecParamField { get { return Page.FindCamstarControl("ProcessSpecDetailsParams") as JQDataGrid; } }     

        #endregion
        
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
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

                if (_txtSelectionIdField.TextControl.Text != "")
                {
                    if (_ContainerField.Data != null)
                    {
                        oServiceData.Container = new ContainerRef();
                        oServiceData.Container.Name = _ContainerField.Data.ToString();
                    }
                    else
                    {
                        oServiceData.SelectionId = _txtSelectionIdField.Data.ToString();
                    }

                    _gridProcessSpecParamField.ClearData();

                    oServiceInfo.ProcessSpecParamInfo = new ProcessSpecParamInfo_Info();
                    oServiceInfo.ProcessSpecParamInfo.ParamName = FieldInfoUtil.RequestValue();
                    oServiceInfo.ProcessSpecParamInfo.ParamValue = FieldInfoUtil.RequestValue();
                   
                    //Set the request
                    WIPMain_Request oServiceRequest = new WIPMain_Request();
                    oServiceRequest.Info = oServiceInfo;

                    // Set the status
                    WIPMain_Result oServiceResult = new WIPMain_Result();

                    // execute the request selection values
                    ResultStatus objRS = oService.ResolveSelectionId(oServiceData, oServiceRequest, out oServiceResult);

                    if (objRS.IsSuccess)
                    {         

                        if (oServiceResult.Value.ProcessSpecParamInfo != null)
                        {
                            foreach (ProcessSpecParamInfo _ProcessSpecParam in oServiceResult.Value.ProcessSpecParamInfo)
                            {
                                // int iNewRowCount = _gridProcessSpecParamField.BoundContext.GetTotalRows();
                                // (_gridProcessSpecParamField.GridContext as ItemDataContext).MakeAutoRowId(iNewRowCount);
                                // string id = (_gridProcessSpecParamField.GridContext as ItemDataContext).AddNewRow(iNewRowCount.ToString());
                                // object processSpecParamDetails = ((_gridProcessSpecParamField.GridContext as ItemDataContext).Data as Array).GetValue(iNewRowCount);
                                // (processSpecParamDetails as ProcessSpecParamInfo).ParamName = _ProcessSpecParam.ParamName;
                                // (processSpecParamDetails as ProcessSpecParamInfo).ParamValue = _ProcessSpecParam.ParamValue;
                                // _gridProcessSpecParamField.GridContext.AdjustCurrentPage(id);
                                // CamstarWebControl.SetRenderToClient(_gridProcessSpecParamField);
								ProcessSpecParam_AddNewRow((string)_ProcessSpecParam.ParamName, (string)_ProcessSpecParam.ParamValue);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
		
		public void ProcessSpecParam_AddNewRow(string ParamName, string ParamValue)
        {
            try
            {
                JQDataGrid _gridDetails = _gridProcessSpecParamField;
                ProcessSpecParamInfo[] oNewDetail = new ProcessSpecParamInfo[1];
                oNewDetail[0] = new ProcessSpecParamInfo();
                oNewDetail[0].ParamName = ParamName;
                oNewDetail[0].ParamValue = ParamValue;
                ProcessSpecParamInfo[] oExisting = (_gridDetails.GridContext as BoundContext).Data as ProcessSpecParamInfo[];
                if (oExisting != null)
                {
                    bool isUnique = true;
                    for (int i = 0; i < oExisting.Length; i++)
                    {
                        if (oExisting[i].ParamName.Equals(oNewDetail[0].ParamName))
                        {
                            isUnique = false;
                        }
                    }
                    if (isUnique)
                    {
                        ProcessSpecParamInfo[] oMerged = new ProcessSpecParamInfo[oExisting.Length + 1];
                        Array.Copy(oExisting, oMerged, oExisting.Length);
                        Array.Copy(oNewDetail, 0, oMerged, oExisting.Length, 1);
                        (_gridDetails.GridContext as BoundContext).Data = oMerged.ToArray();
                    }
                }
                else
                {
                    (_gridDetails.GridContext as BoundContext).Data = oNewDetail.ToArray();
                }
                _gridDetails.BoundContext.LoadData();
                CamstarWebControl.SetRenderToClient(_gridDetails);
            }
            catch (Exception ex)
            { }
        }
    }
}



