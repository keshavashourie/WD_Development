/* Copyright 2019 Siemens */
using System;
using System.Collections.Generic;
using System.Linq;
using Camstar.WebPortal.WebPortlets;
using Camstar.WCF.ObjectStack;
using Camstar.WebPortal.Utilities;
using Camstar.WCF.Services;
using System.Web;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using Camstar.WebPortal.FormsFramework.WebControls;
using CGC = Camstar.WebPortal.FormsFramework.WebGridControls;
using CWP = Camstar.WebPortal.Personalization;

/// <summary>
/// Summary description for SS_OnlineTraveler
/// </summary>

namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class SS_OnlineTraveler : MatrixWebPart
    {
        protected TextBox _txtSelectionId { get { return Page.FindCamstarControl("OnlineTraveler_SelectionId") as TextBox; } }
        protected JQDataGrid _gridTraveler { get { return Page.FindCamstarControl("OnlineTraveler_Grid") as JQDataGrid; } }
        protected JQDataGrid _gridTravelerDetails { get { return Page.FindCamstarControl("OnlineTravelerDetails_Grid") as JQDataGrid; } }        

        //-----------------------------------------
        //
        //-----------------------------------------
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            
            if (Page.IsPostBack)
            {
                _txtSelectionId.DataChanged += delegate { LoadDependentControls(); };
                //Camstar.WebPortal.FormsFramework.CamstarWebControl.SetRenderToClient(_gridTraveler);
               
            }
            else
            {
                LoadDependentControls();
                SEMI.AppCode.UIUtility.MaximizePopUp(this);
            }

            if (!Page.IsAJAXFloatingFrame)
            {
                Personalization.UIAction[] actUIActions = Page.ActionDispatcher.PageActions();
                foreach (Personalization.UIAction act in actUIActions)
                {
                    if (act.Name.ToUpper() == "CLOSE")
                    {
                        act.IsHidden = true;
                        act.IsDisabled = true;
                    }
                }
            }
        }
      
        //-----------------------------------------
        //
        //-----------------------------------------
        private void LoadDependentControls()
        {
            if (_txtSelectionId == null)
                throw new ApplicationException("The control is not found");

            if (_txtSelectionId.Data != null)
            {
                Page.StatusBar.ClearMessage();
                ViewContainerStatus inputData = new ViewContainerStatus { Container = new ContainerRef(_txtSelectionId.Data.ToString()) };
                ViewContainerStatus_Info info = new ViewContainerStatus_Info
                {
                    Workflow = FieldInfoUtil.RequestValue(),
                    Step = FieldInfoUtil.RequestValue(),
                    NextStep = FieldInfoUtil.RequestValue(),
                    Owner = FieldInfoUtil.RequestValue(),
                    ProcessSpecObjectType = FieldInfoUtil.RequestValue(),
                    ProcessSpec = FieldInfoUtil.RequestValue(),
                    ProcessSpecRevision = FieldInfoUtil.RequestValue(),
                    Product = FieldInfoUtil.RequestValue(),
                    WIPStatus = FieldInfoUtil.RequestValue(),
                    WIPType = FieldInfoUtil.RequestValue(),
                    WIPYieldResult = FieldInfoUtil.RequestValue(),
                    Qty = FieldInfoUtil.RequestValue(),
                    Qty2 = FieldInfoUtil.RequestValue()
                };
                UserProfile profile = HttpContext.Current.Session[Camstar.WebPortal.Constants.SessionConstants.UserProfile] as UserProfile;
                ViewContainerStatusService serv = new ViewContainerStatusService(profile);
                ViewContainerStatus_Result result = null;
                ResultStatus resultStatus = serv.ExecuteTransaction(inputData, new ViewContainerStatus_Request { Info = info }, out result);
                if (resultStatus.IsSuccess)
                {
                    DisplayValues(result.Value);
                    (_gridTraveler.GridContext as QueryContext).ClearData();
                }
                else
                {
                    this.DisplayMessage(resultStatus);
                }
                             
            }
            else
            {
                Page.ShopfloorReset(null, null);
            }
        } // LoadDependentControls
    }
}



