// Copyright Siemens 2023  
using System.Collections.Generic;
using System.Linq;
using Camstar.WebPortal.FormsFramework.WebControls.PickLists;
using Camstar.WebPortal.Personalization;
using CamstarPortal.WebControls;
using OM = Camstar.WCF.ObjectStack;
using Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using System;
using Camstar.WebPortal.WCFUtilities;

namespace Camstar.WebPortal.WebPortlets
{
    public class SpecPathDetailsWebPart : MatrixWebPart
    {
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            Page.LoadComplete += Page_LoadComplete;
        }

        protected virtual void Page_LoadComplete(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
                DisplayDetails();
        }

        protected virtual OM.PathChanges DisplayDetails()
        {
            var details = InPopupData;
            if (!Page.IsPostBack)
            {
                if (details == null)
                    return null;

                var serviceData = Page.CreateServiceData(PrimaryServiceType);
                var wcfObjHelper = new WCFObject(serviceData) { ReplaceValue = true };
                var type = wcfObjHelper.GetFieldType(SubentityFieldExpression);
                var arr = WCFObject.CloneArray(new[] { details }, type);
                wcfObjHelper.SetValue(SubentityFieldExpression, arr);

                DisplayValues(serviceData);
            }
            return details;
        }

        public override void WebPartCustomAction(object sender, Personalization.CustomActionEventArgs e)
        {
            base.WebPartCustomAction(sender, e);
            var action = e.Action as CustomAction;
            if (action != null)
            {
                switch (action.Parameters)
                {
                    case "NotifyParent":
                        {
                            CollectInputData();
                            Page.CloseFloatingFrame(this, e);
                            break;
                        }
                }
            }
        } // WebPartCustomAction(object sender, CustomActionEventArgs e)

        public virtual void CollectInputData()
        {
            var serviceData = Page.CreateServiceData(PrimaryServiceType);

            var details = InPopupData;
            if (details != null)
            {
                var wcfObjHelper = new WCFObject(serviceData) { ReplaceValue = true };
                var type = wcfObjHelper.GetFieldType(SubentityFieldExpression);
                var arr = WCFObject.CloneArray(new[] { details }, type);
                wcfObjHelper.SetValue(SubentityFieldExpression, arr);
            }

            base.GetInputData(serviceData);

            var pathChanges = new OM.PathChanges();
            var wcfObj = new WCFObject(serviceData);
            var pathDetailsList = wcfObj.GetValue(SubentityFieldExpression) as OM.PathChanges[];
            if (pathDetailsList != null && pathDetailsList.Length > 0)
                pathChanges = pathDetailsList[0];

            Page.DataContract.SetValueByName("PathChanges", pathChanges);
        }

        public override void GetInputData(OM.Service serviceData)
        {
            var details = InPopupData;
            if (details != null)
            {
                var wcfObjHelper = new WCFObject(serviceData) { ReplaceValue = true };
                var type = wcfObjHelper.GetFieldType(SubentityFieldExpression);
                var arr = WCFObject.CloneArray(new[] { details }, type);
                wcfObjHelper.SetValue(SubentityFieldExpression, arr);
            }

            base.GetInputData(serviceData);

            var pathChanges = new OM.PathChanges();
            var wcfObj = new WCFObject(serviceData);
            var pathDetailsList = wcfObj.GetValue(SubentityFieldExpression) as OM.PathChanges[];
            if (pathDetailsList != null && pathDetailsList.Length > 0)
                pathChanges = pathDetailsList[0];

            Page.DataContract.SetValueByName("PathChanges", pathChanges);
        }

        protected virtual string SubentityFieldExpression
        {
            get
            {
                return Page.DataContract.DataMembers.SingleOrDefault(m => m.Name == "ReEntryStep") == null ? pathFieldExpression : reworkFieldExpression;
            }
        }

        protected virtual OM.PathChanges InPopupData
        {
            get { return Page.DataContract.GetValueByName<OM.PathChanges>("PathChanges"); }
        }

        protected virtual UIAction OKAction { get { return Page.ActionDispatcher.PageActions().FirstOrDefault(a => a.Name == "OKPopup"); } }
        protected virtual UIAction CloseAction { get { return Page.ActionDispatcher.PageActions().FirstOrDefault(a => a.Name == "ClosePopup"); } }

        private const string pathFieldExpression = "ObjectChanges.Steps.Paths";
        private const string reworkFieldExpression = "ObjectChanges.Steps.ReworkPaths";
    }
}
