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
using CGC = Camstar.WebPortal.FormsFramework.WebGridControls;

namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class SS_EquipmentSelectionPopUp : MatrixWebPart
    {
        //Controls declaration
        private CWC.NamedObject _ndoResourceField { get { return Page.FindCamstarControl("ResourceField") as CWC.NamedObject; } }
        private CWC.RevisionedObject _rdoSpecField { get { return Page.FindCamstarControl("SpecField") as CWC.RevisionedObject; } }
        private CWC.TextBox _txtHiddenNoResultsFound { get { return Page.FindCamstarControl("HiddenNoResultsFound") as CWC.TextBox; } }
        private JQDataGrid _gridQueryGrid { get { return Page.FindCamstarControl("QueryGrid") as JQDataGrid; } }
        private SEMI.AppCode.DataEnvelopControl _envSelectedLots { get { return Page.FindCamstarControl("HiddenSelectedLotsList") as SEMI.AppCode.DataEnvelopControl; } }

        //-----------------------------------------
        // Fetch Results
        //-----------------------------------------
        bool ResultsGrid_BeforeQueryExecution(QueryState queryState)
        {
            try
            {
                var ValidationResult = ValidateInputData(null);
                if (ValidationResult.IsSuccess)
                {
                    //Selected Spec
                    string selectedSpec = "";
                    if (_rdoSpecField.Data != null)
                        selectedSpec = _rdoSpecField.Data.ToString();
                    string[] splitSpec = null;
                    if (selectedSpec != null)
                        splitSpec = selectedSpec.Split(':');

                    //Add parameters
                    QueryParameter[] paramChanges = new QueryParameter[3];

                    paramChanges[0] = new QueryParameter();
                    paramChanges[0].Name = "EQUIPMENT";
                    paramChanges[0].Value = "";
                    if (_ndoResourceField.Data != null)
                        paramChanges[0].Value = _ndoResourceField.Data.ToString();

                    paramChanges[1] = new QueryParameter();
                    paramChanges[1].Name = "SPECNAMEVAR";
                    paramChanges[1].Value = "";
                    if (splitSpec != null)
                        paramChanges[1].Value = splitSpec[0];

                    paramChanges[2] = new QueryParameter();
                    paramChanges[2].Name = "SPECREV";
                    paramChanges[2].Value = "";
                    if (splitSpec != null)
                        if (splitSpec.Count() > 1)
                            paramChanges[2].Value = splitSpec[1];

                    queryState.Parameters = paramChanges;
                    queryState.Options.QueryType = WCF.ObjectStack.QueryType.User;
                    return true;
                }
                else
                {
                    Page.StatusBar.WriteStatus(ValidationResult);
                    return false;
                }
            }
            catch (Exception ex) //Catch errors
            {
                Page.StatusBar.WriteError(ex.Message.ToString());
                return false;
            }
        }

        //-----------------------------------------
        // Return Error message when no result found
        //-----------------------------------------
        bool ResultsGrid_AfterQueryExecution(QueryState queryState)
        {
            try
            {
                if (queryState.returnedRecords != null)
                {
                    if (queryState.returnedRecords.Headers != null && queryState.returnedRecords.Rows == null)
                    {
                        if (_txtHiddenNoResultsFound.Data != null)
                        {
                            if (_txtHiddenNoResultsFound.Data.ToString().Equals("Clicked"))
                            {
                                string labelText = "";
                                var labelCache = FrameworkManagerUtil.GetLabelCache(System.Web.HttpContext.Current.Session);
                                if (labelCache != null)
                                {
                                    var label = labelCache.GetLabelByName("LotSearch_NoResultsFound");
                                    if (label != null)
                                        labelText = label.Value;
                                }
                                Page.StatusBar.WriteError(labelText);
                            }
                        }
                    }
                }
                _txtHiddenNoResultsFound.ClearData();
                return true;
            }
            catch (Exception ex) //Catch errors
            {
                Page.StatusBar.WriteError(ex.Message.ToString());
                return false;
            }
        }

        //-----------------------------------------
        // Collect selected lots from the grid
        //-----------------------------------------
        public void CollectSelectedLot()
        {
            try
            {
                if (_gridQueryGrid.GridContext.SelectedRowID != null)
                {
                    int countSelectedLots = _gridQueryGrid.GridContext.SelectedRowIDs.Count;
                    List<string> selectedLots = new List<string>();
                    for (int i = 0; i < countSelectedLots; i++)
                    {
                        selectedLots.Add((_gridQueryGrid.GridContext as DataGridContext).SelectedRowsTable.Rows[i].Field<String>("Lot"));
                    }
                    _envSelectedLots.SS_ContainersList = selectedLots.ToArray();
                }
            }
            catch (Exception ex)
            {
                Page.StatusBar.WriteError(ex.Message.ToString());
            }
        }

        //-----------------------------------------
        // Set dummy data to indicate button click event
        //-----------------------------------------
        public void SetIsButtonClicked()
        {
            try
            {
                _txtHiddenNoResultsFound.Data = "Clicked";
                (_gridQueryGrid.GridContext as CGC.QueryContext).LoadData();
            }
            catch (Exception ex) //Catch errors
            {
                Page.StatusBar.WriteError(ex.Message.ToString());
            }
        }
        public override ValidationStatus ValidateInputData(Service serviceData)
        {
            ValidationStatus ValidationStatus= base.ValidateInputData(serviceData);
            ValidationStatus.Add(_ndoResourceField.Validate());
            ValidationStatus.Add(_rdoSpecField.Validate());

            return ValidationStatus;
        }

        //-----------------------------------------
        // Override OnLoad event
        //-----------------------------------------
        protected override void OnLoad(EventArgs e)
        {
            try
            {
                base.OnLoad(e);
                Page.CollectDataContract();
                (_gridQueryGrid.GridContext as CGC.QueryContext).BeforeQueryExecution += ResultsGrid_BeforeQueryExecution;
                (_gridQueryGrid.GridContext as CGC.QueryContext).AfterQueryExecution += ResultsGrid_AfterQueryExecution;
                if (this.Page.IsPostBack)
                {
                    CollectSelectedLot();
                }
                else
                {
                    SEMI.AppCode.UIUtility.MaximizePopUp(this);
                }
            }
            catch (Exception ex)
            {
                Page.StatusBar.WriteError(ex.Message.ToString());
            }
        }
    }
}



