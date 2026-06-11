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
    public class SS_LotAddComments : MatrixWebPart
	{
        protected JQDataGrid _gridItemData {
            get { return FindCamstarControl("LotAddComments_CommentsHistory") as JQDataGrid; }
        }
       // protected CWC.TextBox selectedLotTextBox {
       //     get { return FindCamstarControl("LotAddComments_SelectedLot") as CWC.TextBox; }
       // }
        protected CWC.TextBox mostRecentComment {
            get { return FindCamstarControl("LotAddComments_Comments") as CWC.TextBox; }
        }
        protected CWC.ContainerList addCommentLot {
            get { return FindCamstarControl("LotAddComments_Container") as CWC.ContainerList; }
        }
        protected CWC.TextBox computerNameField
        {
            get { return FindCamstarControl("LotAddComments_ComputerName") as CWC.TextBox; }
        }
        protected CWC.NamedObject _ndoEmployee
        {
            get { return FindCamstarControl("LotAddComments_Employee") as CWC.NamedObject; }
        }
        
        public override void GetInputData(Service serviceData)
        {
            base.GetInputData(serviceData);
            if (_ndoEmployee.Data != null)
            {
                (serviceData as LotAddComments).Employee = new NamedObjectRef();
                (serviceData as LotAddComments).Employee.Name = _ndoEmployee.Data.ToString();
            }
        }

        //
		// TODO: Add constructor logic here
		//        
        public SS_LotAddComments()
		{
			
		}
        protected override void OnLoad(EventArgs e)
        {
            try
            {
                base.OnLoad(e);
                if (!Page.IsPostBack)
                    computerNameField.Data = SEMI.AppCode.UIUtility.GetComputerName(this);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        public void SelectedLot_DataChanged(object sender, EventArgs e)
        {
            if (addCommentLot.IsEmpty)
            {
                mostRecentComment.ClearData();
                _gridItemData.ClearData();
                return;
            }

            // SS_LotAddComments_ItemData[] oExistingItemList = (_gridItemData.GridContext as BoundContext).Data as SS_LotAddComments_ItemData[];
            List<SS_LotAddComments_ItemData> oNewItemList = new List<SS_LotAddComments_ItemData>();            

            ClearLotDetailsGrid();
            // addCommentLot.Data = null;
            DisplayMessage(new ResultStatus("", true));

            if (GetSelectionId() && addCommentLot.Data != null)
            {
                FrameworkSession fs = FrameworkManagerUtil.GetFrameworkSession();

                LotAddCommentsService svc = new LotAddCommentsService(fs.CurrentUserProfile);
                LotAddComments txn = new LotAddComments();
                LotAddComments_Info txnInfo = new LotAddComments_Info();

                txn.Container = new ContainerRef(addCommentLot.Text);

                txnInfo.CommentsHistory = new OM.Info();
                txnInfo.CommentsHistory.RequestSelectionValues = true;

                LotAddComments_Request req = new LotAddComments_Request();
                LotAddComments_Result res = new LotAddComments_Result();

                req.Info = txnInfo;

                ResultStatus rs = svc.GetEnvironment(txn, req, out res);

                if (rs.IsSuccess)
                {
                    if (res.Environment.CommentsHistory.SelectionValues.Rows != null)
                    {
                        Row[] rsRows = new Row[res.Environment.CommentsHistory.SelectionValues.Rows.Count()];
                        int i = 0;
                        foreach (Row lotComment in res.Environment.CommentsHistory.SelectionValues.Rows)
                        {
                            SS_LotAddComments_ItemData oCurrentRow = new SS_LotAddComments_ItemData();

                            oCurrentRow.CommentNumberItem = lotComment.Values[0].ToString();
                            oCurrentRow.CommentItem = lotComment.Values[1].ToString();
                            oCurrentRow.TxnDateItem = lotComment.Values[2].ToString();
                            oCurrentRow.UsernameItem = lotComment.Values[3].ToString();

                            oNewItemList.Add(oCurrentRow);

                            if (i == 0)
                                mostRecentComment.TextControl.Text = lotComment.Values[1].ToString();

                            i++;
                        }
                        (_gridItemData.GridContext as BoundContext).Data = oNewItemList.ToArray();
                        _gridItemData.BoundContext.LoadData();
                        CamstarWebControl.SetRenderToClient(_gridItemData);
                    }
                }
            }
        }
        private Boolean GetSelectionId()
        {
            try
            {
                OM.LotAddComments_Info txnInfo = new OM.LotAddComments_Info();
                //the split bins check box is hidden if the container is at an item processing spec                                
                txnInfo.SelectionContainer = FieldInfoUtil.RequestValue();

                UserProfile profile = HttpContext.Current.Session[Camstar.WebPortal.Constants.SessionConstants.UserProfile] as UserProfile;

                LotAddCommentsService svc = new LotAddCommentsService(profile);
                LotAddComments svcData = new LotAddComments();
                svcData.SelectionId = addCommentLot.Text; 

                LotAddComments_Request reqData = new LotAddComments_Request();
                reqData.Info = txnInfo;
                LotAddComments_Result resultData = new LotAddComments_Result();

                LotAddComments txn = new LotAddComments();
                ResultStatus results = svc.ResolveSelectionId(svcData, reqData, out resultData);                                                                              

                if (results.IsSuccess)
                {
                    addCommentLot.Data = resultData.Value.SelectionContainer;                    
                    return true;
                }
                else
                {
                    DisplayMessage(results);
                    return false;
                }

            }
            catch (Exception ex)
            {
                DisplayMessage(new ResultStatus(ex.Message, false));
                return false;
            }

        } //end Get Selection Id
        private void ClearLotDetailsGrid()
        {            
            //Whack any previous data            
            _gridItemData.ClearData();
            mostRecentComment.ClearData();
        }
        public override void WebPartCustomAction(object sender, Personalization.CustomActionEventArgs e)
        {
            base.WebPartCustomAction(sender, e);

            ClearLotDetailsGrid();
            Page.ClearValues();
            addCommentLot.ClearData(); 
            //selectedLotTextBox.ClearData();
            DisplayMessage(new ResultStatus("", true));

        } // WebPartCustomAction(object sender, CustomActionEventArgs e)
        public override void PostExecute(ResultStatus status, Service serviceData)
        {
            base.PostExecute(status, serviceData);

            if (status.IsSuccess == true)
            {
                ClearLotDetailsGrid();
                addCommentLot.ClearData(); 
               // selectedLotTextBox.ClearData();
            }
        }
        private class SS_LotAddComments_ItemData
        {            
            private string sCommentNumber;
            private string sComment;
            private string sTxnDate;
            private string sUsername;            

            public SS_LotAddComments_ItemData()
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



