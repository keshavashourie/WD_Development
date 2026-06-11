// Copyright Siemens 2023
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

namespace Camstar.WebPortal.WebPortlets.Jobs
{

    /// <summary>
    /// Summary description for SelectionValuesPopup
    /// </summary>
    public class SelectionValuesPopup: MatrixWebPart
    {
        private CWC.TextBox _KeyField { get { return FindCamstarControl("KeyField") as CWC.TextBox; } }
        private CWC.TextBox _RowID { get { return FindCamstarControl("RowID") as CWC.TextBox; } }
        private CWC.TextBox _ValueField { get { return FindCamstarControl("ValueField") as CWC.TextBox; } }
        private CWC.TextBox _RevisionField { get { return FindCamstarControl("RevisionField") as CWC.TextBox; } }
        private CWC.TextBox _NameFilterField { get { return FindCamstarControl("FilterField") as CWC.TextBox; } }

        private CWC.CheckBox _IsNameAvail { get { return FindCamstarControl("IsNameAvail") as CWC.CheckBox; } }
        private CWC.CheckBox _IsRevisionAvail { get { return FindCamstarControl("IsRevisionAvail") as CWC.CheckBox; } }
        private CWC.CheckBox _IsRORAvail { get { return FindCamstarControl("IsRORAvail") as CWC.CheckBox; } }
        private JQDataGrid _SelectionValuesField {get { return FindCamstarControl("SelectionValuesField") as JQDataGrid; }}


        //---------------------------------------
        //
        //---------------------------------------
        protected override void OnLoad(EventArgs e)
        {                       
            
            base.OnLoad(e);

            Page.CollectDataContract();            

            if (!Page.IsPostBack)
                LoadSelectionValues();

            _SelectionValuesField.RowSelected += new JQGridEventHandler(_SelectionValuesField_RowSelected);

            if (Page.PortalContext.DataContract.GetValueByName("Main_Grid_RowId") != null)
                _RowID.Data = Page.PortalContext.DataContract.GetValueByName("Main_Grid_RowId");

            if (Page.PortalContext.DataContract.GetValueByName("SelVal_Grid_RowId") != null)
                _RowID.Data = Page.PortalContext.DataContract.GetValueByName("SelVal_Grid_RowId");
        }

        //---------------------------------------
        //
        //---------------------------------------
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

        }
        //---------------------------------------
        //
        //---------------------------------------
        public ResponseData _SelectionValuesField_RowSelected(object sender, JQGridEventArgs args)
        {
            string strSelectedRow = args.Context.SelectedRowID;

            if (_IsNameAvail.IsChecked)                     
                _ValueField.Data  = (_SelectionValuesField.BoundContext).GetSelectedCell("Name") as string;                
            
            if (_IsRevisionAvail.IsChecked)
                _RevisionField.Data = (_SelectionValuesField.BoundContext).GetSelectedCell("Revision") as string;            
            
            return args.Response;
        } // _SelectionValuesField_RowSelected


        //---------------------------------------
        //
        //---------------------------------------
        public void LoadSelectionValues()
        {
            var fs = FrameworkManagerUtil.GetFrameworkSession();
            QueryService objSvc = new QueryService(fs.CurrentUserProfile);

            // set the query options
            OM.QueryOptions objQueryOptions = new OM.QueryOptions();
            // objQueryOptions.QueryType = OM.QueryType.User;

            objQueryOptions.RowSetSize = 201;
            objQueryOptions.StartRow = 1;            

            // set the query parameters
            int intTotalParams = 3;
            OM.QueryParameters objQueryParameters = new OM.QueryParameters();
            OM.QueryParameter[] objParameters = new OM.QueryParameter[intTotalParams];

            objParameters[0] = new OM.QueryParameter();
            objParameters[0].Name = "NameFilter";
            string strFilterValue = "%";
            if (_NameFilterField.TextControl.Text == "")
            {
                strFilterValue = "%";
            }
            else
            {
                if (_NameFilterField.TextControl.Text.Contains('%'))
                    strFilterValue = _NameFilterField.TextControl.Text;
                else
                    strFilterValue = _NameFilterField.TextControl.Text + "%";
            }
            objParameters[0].Value = strFilterValue;

            objParameters[1] = new OM.QueryParameter();
            objParameters[1].Name = "STARTROWNUM";
            objParameters[1].Value = "1";

            objParameters[2] = new OM.QueryParameter();
            objParameters[2].Name = "STOPROWNUM";
            objParameters[2].Value = "1000";

            objQueryParameters.Parameters = objParameters;

            OM.RecordSet objRecordSet = new OM.RecordSet();
            OM.ResultStatus objRS = objSvc.Execute("_JobTxn_SelValEx_" + _KeyField.TextControl.Text, objQueryParameters, objQueryOptions, out objRecordSet);
            if (objRS.IsSuccess)
            {

                _SelectionValuesField.ClearData();
                _SelectionValuesField.OriginalData = null;

                foreach (OM.Header objHeader in objRecordSet.Headers)
                {
                    switch (objHeader.Name.ToUpper())
                    {
                        case "NAME":
                            _IsNameAvail.CheckControl.Checked = true;
                            break;
                        case "REVISION" :
                            _IsRevisionAvail.CheckControl.Checked = true;
                            break;
                        case "ISROR" :
                            _IsRORAvail.CheckControl.Checked = true;
                            break;
                    }
                    DataColumn dc = new DataColumn(objHeader.Name);
                    _SelectionValuesField.AddField(dc);
                }

                _SelectionValuesField.ClearData();
                _SelectionValuesField.OriginalData  = objRecordSet.GetAsDataTable();    
                (_SelectionValuesField.GridContext as BoundContext).Data = objRecordSet.GetAsDataTable();                
                _SelectionValuesField.BoundContext.LoadData();
                _SelectionValuesField.GridContext.AdjustCurrentPage(0);
                CamstarWebControl.SetRenderToClient(_SelectionValuesField);
               
            }
            else
            {
                DisplayMessage(objRS);
            }                       

        } // LoadSelectionValues

       

        //---------------------------------------
        //
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
                }
            }
        } // WebPartCustomAction(object sender, CustomActionEventArgs e)      

    }

}
