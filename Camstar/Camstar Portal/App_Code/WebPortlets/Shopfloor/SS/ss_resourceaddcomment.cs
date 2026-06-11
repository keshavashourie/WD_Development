/* Copyright 2019 Siemens */
using System;
using System.Data;
using System.Web;
using System.Linq;
using System.Collections.Generic;

using Camstar.WebPortal.FormsFramework.WebGridControls;
using OM = Camstar.WCF.ObjectStack;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.Utilities;
using Camstar.WCF.ObjectStack;
using Camstar.WCF.Services;

using SEMI.AppCode;
using System.Web.UI.WebControls;

/// <summary>
/// The code for the move non standard virtual page.  Resolves the lot based on the 
/// selection id entered and populates the lot details grid.
/// </summary>
/// 
namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class SS_ResourceAddComments : MatrixWebPart
    {
        protected JQDataGrid _gridItemData
        {
            get { return FindCamstarControl("ResourceAddComments_CommentsHistory") as JQDataGrid; }
        }
        protected CWC.TextBox mostRecentComment
        {
            get { return FindCamstarControl("ResourceAddComments_Comments") as CWC.TextBox; }
        }
        protected CWC.NamedObject addCommentResource
        {
            get { return FindCamstarControl("ResourceAddComments_Resource") as CWC.NamedObject; }
        }
        protected CWC.TextBox computerNameField
        {
            get { return FindCamstarControl("ResourceAddComments_ComputerName") as CWC.TextBox; }
        }
        protected CWC.NamedObject _ndoEmployee
        {
            get { return FindCamstarControl("ResourceAddComments_Employee") as CWC.NamedObject; }
        }

        //
        // TODO: Add constructor logic here
        //        
        public SS_ResourceAddComments()
        {

        }
        public override void GetInputData(Service serviceData)
        {
            base.GetInputData(serviceData);
            if (_ndoEmployee.Data != null)
            {
                (serviceData as ResourceAddComments).Employee = new NamedObjectRef();
                (serviceData as ResourceAddComments).Employee.Name = _ndoEmployee.Data.ToString();
            }
        }
        protected override void OnLoad(EventArgs e)
        {
            try
            {
                base.OnLoad(e);
                if (!Page.IsPostBack)                
                    computerNameField.Data = SEMI.AppCode.UIUtility.GetComputerName(this);


              
                    Personalization.UIAction[] actUIActions = Page.ActionDispatcher.PageActions();
                    foreach (Personalization.UIAction act in actUIActions)
                    {                        
                        if (act.Name.ToUpper() == "CLOSEACTION")
                        {
                            if (!Page.IsAJAXFloatingFrame)
                            {
                                act.IsHidden = true;
                                act.IsDisabled = true;
                            }
                        }

                        if (act.Name.ToUpper() == "CLEARBUTTON")
                        {
                            if (Page.IsAJAXFloatingFrame)
                            {
                                act.IsHidden = true;
                                act.IsDisabled = true;
                            }
                        }
                    }
                
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        public void SelectedResource_DataChanged(object sender, EventArgs e)
        {
            if (addCommentResource.Data == null)
                return;

            // SS_ResourceAddComments_ItemData[] oExistingItemList = (_gridItemData.GridContext as BoundContext).Data as SS_ResourceAddComments_ItemData[];
            List<SS_ResourceAddComments_ItemData> oNewItemList = new List<SS_ResourceAddComments_ItemData>();

            ClearResourceDetailsGrid();
            // addCommentResource.Data = null;
            DisplayMessage(new ResultStatus("", true));

            if (addCommentResource.Data != null)
            {
                FrameworkSession fs = FrameworkManagerUtil.GetFrameworkSession();

                ResourceAddCommentsService svc = new ResourceAddCommentsService(fs.CurrentUserProfile);
                ResourceAddComments txn = new ResourceAddComments();
                ResourceAddComments_Info txnInfo = new ResourceAddComments_Info();

                txn.Resource = new NamedObjectRef(addCommentResource.Text);

                txnInfo.CommentsHistory = new OM.Info();
                txnInfo.CommentsHistory.RequestSelectionValues = true;

                ResourceAddComments_Request req = new ResourceAddComments_Request();
                ResourceAddComments_Result res = new ResourceAddComments_Result();

                req.Info = txnInfo;

                ResultStatus rs = svc.GetEnvironment(txn, req, out res);

                if (rs.IsSuccess)
                {
                    if (res.Environment.CommentsHistory.SelectionValues.Rows != null)
                    {
                        Row[] rsRows = new Row[res.Environment.CommentsHistory.SelectionValues.Rows.Count()];
                        int i = 0;
                        foreach (Row ResourceComment in res.Environment.CommentsHistory.SelectionValues.Rows)
                        {
                            SS_ResourceAddComments_ItemData oCurrentRow = new SS_ResourceAddComments_ItemData();

                            oCurrentRow.CommentNumberItem = ResourceComment.Values[0].ToString();
                            oCurrentRow.CommentItem = ResourceComment.Values[1].ToString();
                            oCurrentRow.TxnDateItem = ResourceComment.Values[2].ToString();
                            oCurrentRow.UsernameItem = ResourceComment.Values[3].ToString();

                            oNewItemList.Add(oCurrentRow);

                            if (i == 0)
                                mostRecentComment.TextControl.Text = ResourceComment.Values[1].ToString();

                            i++;
                        }
                        (_gridItemData.GridContext as BoundContext).Data = oNewItemList.ToArray();
                        _gridItemData.BoundContext.LoadData();
                        CamstarWebControl.SetRenderToClient(_gridItemData);
                    }
                }
            }
        }
        private void ClearResourceDetailsGrid()
        {
            //Whack any previous data            
            _gridItemData.ClearData();
            mostRecentComment.ClearData();
        }
        public override void WebPartCustomAction(object sender, Personalization.CustomActionEventArgs e)
        {
            base.WebPartCustomAction(sender, e);

            ClearResourceDetailsGrid();
            Page.ClearValues();
            // selectedResourceTextBox.ClearData();
            DisplayMessage(new ResultStatus("", true));

        } // WebPartCustomAction(object sender, CustomActionEventArgs e)
        public override void PostExecute(ResultStatus status, Service serviceData)
        {
            base.PostExecute(status, serviceData);

            if (status.IsSuccess == true)
            {
                ClearResourceDetailsGrid();
                // selectedResourceTextBox.ClearData();
            }
        }
        private class SS_ResourceAddComments_ItemData
        {
            private string sCommentNumber;
            private string sComment;
            private string sTxnDate;
            private string sUsername;

            public SS_ResourceAddComments_ItemData()
            { }

            public string CommentNumberItem
            {
                get { return sCommentNumber; }
                set { sCommentNumber = value; }
            }

            public string CommentItem
            {
                get { return sComment; }
                set { sComment = value; }
            }

            public string TxnDateItem
            {
                get { return sTxnDate; }
                set { sTxnDate = value; }
            }

            public string UsernameItem
            {
                get { return sUsername; }
                set { sUsername = value; }
            }
        }
    }
}



