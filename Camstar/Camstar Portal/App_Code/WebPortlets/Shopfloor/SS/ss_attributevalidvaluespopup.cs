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
using OM = Camstar.WCF.ObjectStack;
using SEMI.AppCode;

namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class SS_AttributeValidValuesPopUp : MatrixWebPart
    {
        protected CWC.TextBox _txtAttribute { get { return Page.FindCamstarControl("SelectedAttribute") as CWC.TextBox; } }
        protected CWC.TextBox _txtValue { get { return Page.FindCamstarControl("SelectedAttributeValue") as CWC.TextBox; } }
        protected CWC.TextBox _txtRevision { get { return Page.FindCamstarControl("SelectedAttributeRevision") as CWC.TextBox; } }
        protected JQDataGrid _gridValidValues { get { return Page.FindCamstarControl("ValidValuesGrid") as JQDataGrid; } }

        //---------------------------------------
        // Filter valid values according to the selected attribute
        //---------------------------------------
        public void FilterValidValues()
        {
            string selectedAttribute = Page.DataContract.GetValueByName("PopUp_SelectedAttribute").ToString();
            DataTable validValuesDT = Page.DataContract.GetValueByName("PopUp_ValidValues") as DataTable;
            DataTable newValidValuesDT = new DataTable();
            int countValidValuesColumn = validValuesDT.Columns.Count;
            for (int c = 0; c < countValidValuesColumn; c++)
            {
                newValidValuesDT.Columns.Add(validValuesDT.Columns[c].ColumnName, validValuesDT.Columns[c].DataType);
            }
            int countValidValues = validValuesDT.Rows.Count;
            for (int i = 0; i < countValidValues; i++)
            {
                if (validValuesDT.Rows[i].Field<String>("Attribute").ToString() == selectedAttribute)
                {
                    DataRow dtRow = newValidValuesDT.NewRow();
                    dtRow.SetField("Attribute", validValuesDT.Rows[i].Field<String>("Attribute").ToString());
                    dtRow.SetField("AttributeValue", validValuesDT.Rows[i].Field<String>("AttributeValue").ToString());
                    if (validValuesDT.Rows[i].Field<String>("AttributeRevision") != null)
                        dtRow.SetField("AttributeRevision", validValuesDT.Rows[i].Field<String>("AttributeRevision").ToString());
                    newValidValuesDT.Rows.Add(dtRow);
                }
            }
            _gridValidValues.ClearData();
            _gridValidValues.Data = newValidValuesDT;
            _gridValidValues.OriginalData = newValidValuesDT;
        }

        //---------------------------------------
        // Web Part Custom Action
        //---------------------------------------
        public override void WebPartCustomAction(object sender, Personalization.CustomActionEventArgs e)
        {
            base.WebPartCustomAction(sender, e);
            var action = e.Action as Personalization.CustomAction;
            if (action != null)
            {
                switch (action.Parameters)
                {
                    case "Close":
                        {
                            Page.CloseFloatingFrame(true);
                            break;
                        }
                    case "OK":
                        {
                            if (_gridValidValues.GridContext.SelectedRowID != null)
                            {
                                _txtValue.Data = (_gridValidValues.GridContext as DataGridContext).SelectedRowsTable.Rows[0].Field<String>("AttributeValue");
                                _txtRevision.Data = (_gridValidValues.GridContext as DataGridContext).SelectedRowsTable.Rows[0].Field<String>("AttributeRevision");
                            }
                            Page.CloseFloatingFrameOnSubmit(new ResultStatus());
                            break;
                        }
                }
            }
        } // WebPartCustomAction

        //---------------------------------------------------
        // Override OnLoad event
        //---------------------------------------------------
        protected override void OnLoad(EventArgs e)
        {
            try
            {
                base.OnLoad(e);
                if (!Page.IsPostBack)
                    FilterValidValues();
            }
            catch (Exception ex)
            {
                Page.StatusBar.WriteError(ex.Message.ToString());
            }
        }
    }
}



