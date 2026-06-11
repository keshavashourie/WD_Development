// Copyright Siemens 2023  
using System;
using System.Activities.Expressions;
using Camstar.WCF.ObjectStack;
using Camstar.WCF.Services;
using Camstar.WebPortal.Utilities;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.WCFUtilities;
using System.Data;
using System.Data.Linq;
using System.Collections.Generic;
using System.Linq;
using DocumentFormat.OpenXml.Wordprocessing;

namespace Camstar.WebPortal.WebPortlets.Modeling
{
    public class TrainingPlan : MatrixWebPart
    {
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            DetailsGrid.GridContext.RowUpdated += GridContext_RowUpdated;
            DetailsGrid.GridContext.RowDeleted += GridContext_RowDeleted;
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            if (TrainingDetailType.Data == null && TrainingDetailType.CustomListValues != null)
                TrainingDetailType.Data = TrainingDetailType.CustomListValues[0].Value;
        }

        protected virtual void UpdateUnboundData(string rowId, string value)
        {
            if ((DetailsGrid.GridContext as BoundContext).UnboundData == null)
                (DetailsGrid.GridContext as BoundContext).UnboundData = new Dictionary<UnboundKey, object>();

            UnboundKey key = new UnboundKey() { Row = rowId, Column = "TrainingPlanType" };
            if ((DetailsGrid.GridContext as BoundContext).UnboundData.ContainsKey(key))
                (DetailsGrid.GridContext as BoundContext).UnboundData.Remove(key);

            (DetailsGrid.GridContext as BoundContext).UnboundData.Add(key, value);
        }

        protected virtual ResponseData GridContext_RowDeleted(object sender, JQGridEventArgs args)
        {
            var response = args.Response as DirectUpdateData;

            (DetailsGrid.GridContext as BoundContext).UnboundData.Clear();

            var rows = DetailsGrid.Data as TrainingPlanDetailChanges[];
            for (int i = 0; i < rows.Length; i++)
            {
                string rowId = (DetailsGrid.GridContext as BoundContext).MakeAutoRowId(i);
                var item = TrainingDetailType.CustomListValues.FirstOrDefault(value => value.Value == rows[i].GetType().Name);
                if (item != null)
                    UpdateUnboundData(rowId, item.DisplayName);
            }
            args.State.Action = "Reload";
            args.Cancel = true;

            return DetailsGrid.GridContext.Reload(args.State);
        }


        protected virtual ResponseData GridContext_RowUpdated(object sender, JQGridEventArgs args)
        {
            var response = args.Response as DirectUpdateData;
            if (args.State.Action == "SaveDataRow" && args.State.AddRow)
            {
                UpdateUnboundData(args.State.RowID, TrainingDetailType.Text);

                Type t = Type.GetType(string.Format(WCFClientAssemblyQualifiedPrefixName, _WCFNamespace, TrainingDetailType.Data));
                var detail = Activator.CreateInstance(t);

                (DetailsGrid.GridContext as ItemDataContext).SetItem(args.State.RowID, detail);

                args.State.Action = "Reload";
                args.Cancel = true;
            }

            return DetailsGrid.GridContext.Reload(args.State);
        }

        public override void DisplayValues(Service serviceData)
        {
            base.DisplayValues(serviceData);

            var rows = DetailsGrid.Data as TrainingPlanDetailChanges[];
            for (int i = 0; i < rows.Length; i++)
            {
                string rowId = (DetailsGrid.GridContext as BoundContext).MakeAutoRowId(i);
                object v = null;
                UnboundKey key;
                key.Row = rowId;
                key.Column = "TrainingRequirement";
                v = rows[i] is TrainingReqPlanDetailChanges ? (rows[i] as TrainingReqPlanDetailChanges).TrainingRequirement : null;
                if (_unbound.ContainsKey(key))
                    _unbound[key] = v;
                else
                    _unbound.Add(key, v);

                key.Column = "TrainingPlan";
                v = rows[i] is SubTrainingPlanDetailChanges ? (rows[i] as SubTrainingPlanDetailChanges).TrainingPlan : null;
                if (_unbound.ContainsKey(key))
                    _unbound[key] = v;
                else
                    _unbound.Add(key, v);

                string trainingPlanTypeText = TrainingDetailType.CustomListValues.FirstOrDefault(value => value.Value == rows[i].Self.CDOTypeName).DisplayName;
                UpdateUnboundData(rowId, trainingPlanTypeText);
            }
        }

        protected virtual JQDataGrid DetailsGrid
        {
            get { return Page.FindCamstarControl("ObjectChanges_Details") as JQDataGrid; }
        }

        protected virtual DropDownList TrainingDetailType
        {
            get { return Page.FindCamstarControl("TrainingDetailTypeField") as DropDownList; }
        }

        protected virtual Dictionary<UnboundKey, object> _unbound
        {
            get
            {
                if( (DetailsGrid.GridContext as BoundContext).UnboundData == null)
                    (DetailsGrid.GridContext as BoundContext).UnboundData = new Dictionary<UnboundKey, object>();

                return (DetailsGrid.GridContext as BoundContext).UnboundData;
            }
        }

        private const string WCFClientAssemblyQualifiedPrefixName = "{0}.{1}, Camstar.WCFClient";
        private const string _WCFNamespace = "Camstar.WCF.ObjectStack";

    }
}
