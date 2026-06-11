// © 2017 Siemens Product Lifecycle Management Software Inc.
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using Camstar.WCF.ObjectStack;
using OM = Camstar.WCF.ObjectStack;
using Camstar.WCF.Services;
using Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WebPortal.FormsFramework.WebControls.PickLists;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.Utilities;
using Camstar.WebPortal.WCFUtilities;
using Camstar.WebPortal.FormsFramework.HtmlControls;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.PortalFramework;

/// <summary>
///Code behind for the expression builder page.
/// </summary>

namespace Camstar.WebPortal.WebPortlets.Modeling
{
    public class CIOExpressionBuilder : MatrixWebPart
    {//WebPartPageBase

        #region Properties

        string dataContract = string.Empty;

        static CDOSubFields objSubField;

        protected virtual CWC.Button btnCancel
        {
            get { return Page.FindCamstarControl("btnCancel") as CWC.Button; }
        }

        protected virtual CWC.Button btnClear
        {
            get { return Page.FindCamstarControl("btnClear") as CWC.Button; }
        }

        protected virtual CWC.Button btnSave
        {
            get { return Page.FindCamstarControl("btnSave") as CWC.Button; }
        }

        public virtual CWC.DropDownList selectedSeviceType
        {
            get { return Page.FindCamstarControl("Service_ServiceNameListItem") as CWC.DropDownList; }
        }

        public virtual CWC.DropDownList Service_Function
        {
            get { return Page.FindCamstarControl("Service_Function") as CWC.DropDownList; }
        }

        public virtual CWC.DropDownList CIOListProcessor
        {
            get { return Page.FindCamstarControl("ddListProcessorName") as CWC.DropDownList; }
        }

        public virtual CWC.TextBox txtSubObjectTypeProxy
        {
            get { return Page.FindCamstarControl("txtSubObjectTypeProxy") as CWC.TextBox; }
        }

        public virtual CWC.TextBox txtExpression
        {
            get { return Page.FindCamstarControl("txtExpression") as CWC.TextBox; }
        }

        public virtual CWC.TextBox txtSubParentProxy
        {
            get { return Page.FindCamstarControl("txtSubParentProxy") as CWC.TextBox; }
        }

        public virtual CWC.TextBox txtSubFieldNameProxy
        {
            get { return Page.FindCamstarControl("txtSubFieldNameProxy") as CWC.TextBox; }
        }

        public virtual CWC.TextBox txtSubFieldTypeProxy
        {
            get { return Page.FindCamstarControl("txtSubFieldTypeProxy") as CWC.TextBox; }
        }

        public virtual CWC.TextBox txtFunctionDescription
        {
            get { return Page.FindCamstarControl("txtFunctionDescription") as CWC.TextBox; }
        }

        public virtual CWC.TextBox txtBuilder
        {
            get { return Page.FindCamstarControl("txtBuilder") as CWC.TextBox; }
        }

        public virtual CWC.TextBox txtFunctionBuilder
        {
            get { return Page.FindCamstarControl("txtFunctionBuilder") as CWC.TextBox; }
        }

        public virtual CWC.TextBox txtSubFieldsDescription
        {
            get { return Page.FindCamstarControl("CDOSubFields_Description") as CWC.TextBox; }
        }

        public virtual CWC.CheckBox chkSubFieldIsListProxy
        {
            get { return Page.FindCamstarControl("chkSubFieldIsListProxy") as CWC.CheckBox; }
        }

        public virtual CWC.CheckBox chkIsInherited
        {
            get { return Page.FindCamstarControl("chkIsInherited") as CWC.CheckBox; }
        }

        public virtual CWC.CheckBox chkUseCurrentService
        {
            get { return Page.FindCamstarControl("chkUseCurrentService") as CWC.CheckBox; }
        }

        protected virtual JQDataGrid gridCDOSubFields
        {
            get
            {
                return Page.FindCamstarControl("Service_CDOSubFields") as JQDataGrid;
            }
        }

        protected virtual JQDataGrid gridFunctionDetail
        {
            get
            {
                return Page.FindCamstarControl("Service_FunctionDetail") as JQDataGrid;
            }
        }

        static List<FunctionDetails> listOfFunctions;

        static Dictionary<int, CDOSubFields> listEBO;
        #endregion

        #region constructor

        public CIOExpressionBuilder()
        {

        }

        protected override IEnumerable<ScriptReference> GetScriptReferences()
        {
            yield return new ScriptReference("~/Scripts/CIO.js");
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (listEBO == null || txtExpression.Data == null || txtExpression.Data.Equals(""))
            {
                if (listEBO != null)
                    listEBO = null;
                listEBO = new Dictionary<int, CDOSubFields>();
            }
            if (Page.DataContract != null && Page.DataContract.GetValueByName("InExpressionText") != null)
            {
                dataContract = Page.DataContract.GetValueByName<string>("InExpressionText").ToString();
                if (!string.IsNullOrWhiteSpace(dataContract))
                {
                    if (txtExpression.Data == null)
                    {
                        txtExpression.Data = dataContract;
                        Page.DataContract.SetValueByName("InExpressionText", null);
                    }
                }
            }

            gridCDOSubFields.RowSelected += new JQGridEventHandler(gridCDOSubFields_OnRowSelected);
            if (gridCDOSubFields.SelectedRowIDs != null)
            {
                if (gridCDOSubFields.SelectedRowIDs.Length > 0)
                {

                }
            }
        }


        /// <summary>
        /// On select of the grid
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        private ResponseData gridCDOSubFields_OnRowSelected(object sender, JQGridEventArgs args)
        {
            if (gridCDOSubFields.SelectedRowIDs.Length > 0)
            {
                if (objSubField != null)
                    objSubField = null;
                objSubField = (CDOSubFields)gridCDOSubFields.GridContext.GetItem(gridCDOSubFields.SelectedRowID);
                onAddToServiceBuilder();
                return new StatusData(true, "Row selected");
            }
            else
            {

                return new StatusData(false, "Row not selected");
            }
        }


        public void OnAddServiceObjectToExpression()
        {
            if (txtBuilder.Data != null)
            {
                if (txtExpression.Data == null)
                {
                    txtExpression.Data += string.Format("{0}", txtBuilder.Data.ToString());
                }
                else
                {
                    txtExpression.Data += string.Format(".{0}", txtBuilder.Data.ToString());
                }
                ClearServiceValues();
            }
        }

        public void onAddToServiceBuilder()
        {
            if (objSubField == null)
                return;

            if (objSubField.Name != null)
            {
                int buildPath = listEBO.Count + 1;
                //check if exist already TODO
                listEBO.Add(buildPath, objSubField);
               
                if (txtBuilder.Data == null)
                {
                    txtBuilder.Data += string.Format("{0}", objSubField.Name.ToString());
                }
                else
                {
                    txtBuilder.Data += string.Format(".{0}", objSubField.Name.ToString());
                }               
                selectedSeviceType.Data = objSubField.ObjectType.ToString();
                objSubField = null;
            }

        }

        public void OnCheckCurrentService()
        {
            if (chkUseCurrentService.IsChecked)
            {
                if (txtBuilder.Data != null && !txtBuilder.Data.ToString().Contains("GetCurrentService()"))
                {
                    txtBuilder.Data = "GetCurrentService()." + txtBuilder.Data;
                }
                else if (txtBuilder.Data == null)
                {
                    txtBuilder.Data = "GetCurrentService().";
                }
            }
            else
            {
                if(txtBuilder.Data.ToString().Contains("GetCurrentService()"))
                txtBuilder.Data = txtBuilder.Data.ToString().Replace("GetCurrentService().", "");
            }
        }

        public void OnAddFunction()
        {
            if(txtFunctionBuilder.Data != null)
            txtExpression.Data = string.Format("{0}", txtFunctionBuilder.Data.ToString());
            ClearFunction();
        }

        private void ClearServiceValues()
        {
            txtBuilder.ClearData();
            selectedSeviceType.ClearData();
            gridCDOSubFields.BoundContext.ClearData();
            txtSubFieldsDescription.ClearData();
            chkUseCurrentService.IsChecked = false;
        }

        private void ClearFunction()
        {
            Service_Function.ClearData();
            gridFunctionDetail.BoundContext.ClearData();
            txtFunctionBuilder.ClearData();
            CIOListProcessor.ClearData();
            txtFunctionDescription.ClearData();
            listOfFunctions = null;
        }

        public void onAddServiceObjectToFunction()
        {
            int indexOfLastChar = 0;
            if (txtBuilder.Data != null)
            {
                if (listOfFunctions != null)
                {
                    List<FunctionDetails> tempList = new List<FunctionDetails>();
                    bool hasEntry = false;
                    int listCount = 0;
                    listCount = listOfFunctions.Count;
                    foreach (FunctionDetails fd in listOfFunctions)
                    {
                        if (fd.Param == null && !hasEntry)
                        {
                            fd.Param = txtBuilder.Data.ToString();
                            if (txtFunctionBuilder.Data.ToString().Contains(","))
                            {
                                indexOfLastChar = txtFunctionBuilder.Data.ToString().LastIndexOf(",");
                            }
                            else
                            {
                                indexOfLastChar = txtFunctionBuilder.Data.ToString().LastIndexOf("(");
                            }
                            string tempFunctionBuilder = txtFunctionBuilder.Data.ToString();
                            int stringCount = 0;
                            stringCount = tempFunctionBuilder.Count(c => c == ',');

                            if (listCount == stringCount + 1)
                            {
                                txtFunctionBuilder.Data = txtFunctionBuilder.Data.ToString().Insert(indexOfLastChar + 1, string.Format("{0}", txtBuilder.Data.ToString()));
                            }
                            else
                            {
                                txtFunctionBuilder.Data = txtFunctionBuilder.Data.ToString().Insert(indexOfLastChar + 1, string.Format("{0},", txtBuilder.Data.ToString()));
                            }
                            hasEntry = true;
                        }
                        tempList.Add(fd);
                    }
                    listOfFunctions = null;
                    listOfFunctions = tempList;
                }
                ClearServiceValues();
            }
        }
        private void onAddFunctionBuilder(List<FunctionDetails> list)
        {
            if (Service_Function.Data != null)
            {
                txtFunctionBuilder.Data = string.Format("{0}()", Service_Function.Data.ToString());
                if (listOfFunctions != null)
                { listOfFunctions = null; }
                listOfFunctions = list;
            }
        }

        public void OnDatachangetxtExpression()
        {
            if (txtExpression.Data == null || txtExpression.Data.Equals(""))
            {
                listEBO.Clear();
            }
        }

        #endregion

        public void OnSelectService_ServiceNameListItem()
        {
            LoadSubFields();
        }

        public void OnSelectFunction()
        {
            LoadFunctionDetails();
        }

        public void onCheckIsInherited()
        {
            LoadSubFields();
        }

        /// <summary>
        /// This is called with a selection of the service is selected
        /// </summary>
        public void LoadSubFields()
        {
            if (selectedSeviceType.Data == null)
                return;
            gridCDOSubFields.ClearData();
            FrameworkSession session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
            var service = new ServiceService(session.CurrentUserProfile);
            var serviceData = new Service();
            var request = new Service_Request();
            var result = new Service_Result();
            var resultStatus = new ResultStatus();

            serviceData.ServiceNameListItem = selectedSeviceType.Data.ToString();

            Service_Info info = new Service_Info();
            info.RequestValue = true;
            info.CDOSubFields = new CDOSubFields_Info
            {
                RequestValue = true,
                Name = new Info(true),
                IsList = new Info(true),
                FieldParent = new Info(true),
                Type = new Info(true),
                ObjectType = new Info(true),
                Description = new Info(true)

            };
            request.Info = info;

            if (chkIsInherited.IsChecked)
            {
                resultStatus = service.Service_ResolveSubFieldsWithInherited(serviceData, request, out result);
            }
            else
            {
                resultStatus = service.Service_ResolveSubFields(serviceData, request, out result);
            }

            if (resultStatus.IsSuccess)
            {
                if (result.Value != null)
                {
                    gridCDOSubFields.BoundContext.Data = result.Value.CDOSubFields;
                    CamstarWebControl.SetRenderToClient(gridCDOSubFields);
                }
            }
            else
            {
            }
        }

        /// <summary>
        /// This is called with a selection of the service is selected
        /// </summary>
        public void LoadFunctionDetails()
        {
            if (Service_Function.Data == null)
                return;
            gridFunctionDetail.ClearData();
            FrameworkSession session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
            var service = new ServiceService(session.CurrentUserProfile);
            var serviceData = new Service();
            var request = new Service_Request();
            var result = new Service_Result();
            var resultStatus = new ResultStatus();

            serviceData.Function = Service_Function.Data.ToString();

            Service_Info info = new Service_Info();
            info.RequestValue = true;
            info.FunctionDetails = new FunctionDetails_Info
            {
                RequestValue = true,
                Name = new Info(true),
                Description = new Info(true),
                Required = new Info(true),
                DefaultValue = new Info(true),
                Type = new Info(true)
            };
            request.Info = info;

            resultStatus = service.Service_ResolveFunctionDetails(serviceData, request, out result);

            if (resultStatus.IsSuccess)
            {
                if (result.Value != null)
                {
                    gridFunctionDetail.BoundContext.Data = result.Value.FunctionDetails;
                    CamstarWebControl.SetRenderToClient(gridFunctionDetail);
                }
            }
            if (result != null && result.Value != null && result.Value.FunctionDetails != null)
            {
                onAddFunctionBuilder(result.Value.FunctionDetails.ToList());
            }
            else
            {
                txtFunctionBuilder.Data = string.Format("{0}()", Service_Function.Data.ToString());
            }
        }

        public void OnCIOListProcessor_DataChanged()
        {
            if (CIOListProcessor.Data != null)
            {
                txtFunctionBuilder.Data = "Findlistitem(CIOListProcessResults, Name, \"" + CIOListProcessor.Data.ToString().Split(':')[0] + "\").CIOListResult";
                CIOListProcessor.ClearData();
            }
        }

        public void OnbtnClear()
        {
            txtExpression.ClearData();
        }

        public void SaveExpression()
        {
            if (txtExpression.Data != null)
            {               
                Page.DataContract.SetValueByName("OutExpressionText", txtExpression.Data.ToString());
            }
            LabelCache labelCache = FrameworkManagerUtil.GetLabelCache(Page.Session);
            OM.Label Lbl = null;
            Lbl = labelCache.GetLabelByName("CIO_ExpressionBuilderCompletionLabel");
            Page.CloseFloatingFrameOnSubmit(new ResultStatus() { Message = Lbl.Value, IsSuccess = true });
        }

        public void Close()
        {
            this.Page.CloseFloatingFrame(true);
        }

        public void Clear()
        {
            txtExpression.ClearData();
        }
    }
}
