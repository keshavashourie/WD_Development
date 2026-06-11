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
    public class SS_WaferMapDetailsPopup : MatrixWebPart
    {
        protected CWC.TextBox _txtPrimaryServiceType { get { return Page.FindCamstarControl("PrimarySvcType") as CWC.TextBox; } }
        protected JQDataGrid _gridWaferMapDetails { get { return Page.FindCamstarControl("Wafers_WaferMapDetails") as JQDataGrid; } }
        protected DataEnvelopControl _envDataCollection { get { return Page.FindCamstarControl("EnvDataCollection") as DataEnvelopControl; } }

        //-----------------------------------------
        // On Popup closed event
        //-----------------------------------------
        protected override void OnLoad(EventArgs e)
        {
            try
            {
                base.OnLoad(e);
                Page.CollectDataContract();
                if (!Page.IsPostBack)
                {
                    int fieldsCount = (_gridWaferMapDetails.GridContext as BoundContext).Fields.Count;
                    if (_txtPrimaryServiceType.Data.ToString() == "LotBonus" || _txtPrimaryServiceType.Data.ToString() == "LotReject")
                    {
                        for (int i = 0; i < fieldsCount; i++)
                        {
                            string fieldID = (_gridWaferMapDetails.GridContext as BoundContext).Fields[i].ID;
                            if (fieldID.Equals("FromXLocation") ||
                                fieldID.Equals("FromYLocation") ||
                                fieldID.Equals("ToXLocation") ||
                                fieldID.Equals("ToYLocation"))
                            {
                                (_gridWaferMapDetails.GridContext as BoundContext).Fields[i].Visible = false;
                            }
                        }
                    }
                    else
                    {
                        for (int i = 0; i < fieldsCount; i++)
                        {
                            string fieldID = (_gridWaferMapDetails.GridContext as BoundContext).Fields[i].ID;
                            if (fieldID.Equals("XLocation") || fieldID.Equals("YLocation"))
                            {
                                (_gridWaferMapDetails.GridContext as BoundContext).Fields[i].Visible = false;
                            }
                        }
                    }

                    if (Page.DataContract.GetValueByName("Popup_WaferMapDetails_DM") != null)
                    {
                        WaferMapDetails[] existingWaferMapDetails = Page.DataContract.GetValueByName("Popup_WaferMapDetails_DM") as WaferMapDetails[];
                        JQDataGrid theGrid = _gridWaferMapDetails;
                        int countQty = Convert.ToInt32(Page.DataContract.GetValueByName("Popup_Quantity_DM").ToString());
                        DataTable newDataTable = new DataTable();
                        newDataTable.Columns.Add("No");
                        newDataTable.Columns.Add("XLocation");
                        newDataTable.Columns.Add("YLocation");
                        newDataTable.Columns.Add("FromXLocation");
                        newDataTable.Columns.Add("FromYLocation");
                        newDataTable.Columns.Add("ToXLocation");
                        newDataTable.Columns.Add("ToYLocation");
                        for (int i = 0; i < countQty; i++)
                        {
                            DataRow newRow = newDataTable.NewRow();
                            newRow["No"] = Convert.ToString(i + 1);
                            if (i < existingWaferMapDetails.Count())
                            {
                                newRow["XLocation"] = existingWaferMapDetails[i].XLocation;
                                newRow["YLocation"] = existingWaferMapDetails[i].YLocation;
                                newRow["FromXLocation"] = existingWaferMapDetails[i].FromXLocation;
                                newRow["FromYLocation"] = existingWaferMapDetails[i].FromYLocation;
                                newRow["ToXLocation"] = existingWaferMapDetails[i].ToXLocation;
                                newRow["ToYLocation"] = existingWaferMapDetails[i].ToYLocation;
                            }
                            newDataTable.Rows.Add(newRow);
                        }
                        SEMI.AppCode.GridUtility.ItemListGrid_BindDataTable(this, newDataTable, ref theGrid);
                    }
                    else
                    {
                        JQDataGrid theGrid = _gridWaferMapDetails;
                        int countQty = Convert.ToInt32(Page.DataContract.GetValueByName("Popup_Quantity_DM").ToString());
                        DataTable newDataTable = new DataTable();
                        newDataTable.Columns.Add("No");
                        newDataTable.Columns.Add("XLocation");
                        newDataTable.Columns.Add("YLocation");
                        newDataTable.Columns.Add("FromXLocation");
                        newDataTable.Columns.Add("FromYLocation");
                        newDataTable.Columns.Add("ToXLocation");
                        newDataTable.Columns.Add("ToYLocation");
                        for (int i = 0; i < countQty; i++)
                        {
                            DataRow newRow = newDataTable.NewRow();
                            newRow["No"] = Convert.ToString(i + 1);
                            newDataTable.Rows.Add(newRow);
                        }
                        SEMI.AppCode.GridUtility.ItemListGrid_BindDataTable(this, newDataTable, ref theGrid);
                    }
                    CamstarWebControl.SetRenderToClient(_gridWaferMapDetails);
                }
            }
            catch (Exception ex)
            {
                Page.StatusBar.WriteError(ex.Message.ToString());
            }
        }

        //-----------------------------------------
        // On Popup closed event
        //-----------------------------------------
        public void ClosePopup()
        {
            try
            {
                JQDataGrid theGrid = _gridWaferMapDetails;
                var newWaferMaps = new List<WaferMapDetails>();
                DataTable existingDataTable = new DataTable();
                if (_gridWaferMapDetails.TotalRowCount > 0)
                {
                    (theGrid.GridContext as BoundContext).GenerateFullExcelData(theGrid.TotalRowCount, out existingDataTable);
                    foreach (DataRow dr in existingDataTable.Rows)
                    {
                        WaferMapDetails newWaferMap = new WaferMapDetails();
                        if (dr.Field<string>("XLocation") != "")
                            newWaferMap.XLocation = Convert.ToInt32(dr.Field<string>("XLocation"));
                        if (dr.Field<string>("YLocation") != "")
                            newWaferMap.YLocation = Convert.ToInt32(dr.Field<string>("YLocation"));
                        if (dr.Field<string>("FromXLocation") != "")
                            newWaferMap.FromXLocation = Convert.ToInt32(dr.Field<string>("FromXLocation"));
                        if (dr.Field<string>("FromYLocation") != "")
                            newWaferMap.FromYLocation = Convert.ToInt32(dr.Field<string>("FromYLocation"));
                        if (dr.Field<string>("ToXLocation") != "")
                            newWaferMap.ToXLocation = Convert.ToInt32(dr.Field<string>("ToXLocation"));
                        if (dr.Field<string>("ToYLocation") != "")
                            newWaferMap.ToYLocation = Convert.ToInt32(dr.Field<string>("ToYLocation"));
                        newWaferMaps.Add(newWaferMap);
                    }
                }
                _envDataCollection.SS_WaferMapDetails = newWaferMaps.ToArray();
                Page.CollectDataContractByName("Popup_WaferMapDetails_DM");
                Page.DistributeDataContract();
                Page.CloseFloatingFrameOnSubmit(new ResultStatus());
            }
            catch (Exception ex)
            {
                Page.StatusBar.WriteError(ex.Message.ToString());
            }
        }

        //---------------------------------------------------
        // Web part custom action
        //---------------------------------------------------
        public override void WebPartCustomAction(object sender, Personalization.CustomActionEventArgs e)
        {
            base.WebPartCustomAction(sender, e);
            var action = e.Action as CustomAction;
            if (action != null)
            {
                switch (action.Parameters)
                {
                    case "Submit":
                        {
                            ClosePopup();
                            break;
                        }
                }
            }
        } // WebPartCustomAction(object sender, CustomActionEventArgs e)
    }
}



