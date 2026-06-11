// Copyright Siemens 2023  
using System;

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

namespace Camstar.WebPortal.WebPortlets.Modeling
{
    public class BusinessRuleHandler : MatrixWebPart
    {
        protected virtual JQDataGrid ParametersGrid
        {
            get { return Page.FindCamstarControl("HandlerData_Parameters") as JQDataGrid; }
        }

        protected virtual DropDownList ServiceType
        {
            get { return Page.FindCamstarControl("HandlerData_ServiceType") as DropDownList; }
        }

        protected virtual DropDownList BizHandlerTypeProDropDownList
        {
            get { return Page.FindCamstarControl("HandlerData_BizRuleHandlerType") as DropDownList; }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (!Page.IsPostBack)
                LoadObjectTypes();
        }

        public override void GetInputData(Service serviceData)
        {
            var unboundData = (ParametersGrid.GridContext as BoundContext).UnboundData;
            if (unboundData != null)
            {
                var rows = ParametersGrid.Data as BizRuleHandlerParameterChanges[];
                for (int i = 0; i < rows.Length; i++)
                {
                    string rowId = (ParametersGrid.GridContext as BoundContext).MakeAutoRowId(i);

                    var row = rows[i];
                    if (row.DataType == DataTypeEnum.Object)
                    {
                        var rowdata = unboundData[new UnboundKey() { Row = rowId, Column = "ObjectDefaultValue" }];
                        if (rowdata != null)
                        {
                            string instanceId = rowdata.ToString();
                            row.DefaultValue = instanceId;
                        }
                    }
                }
            }

            base.GetInputData(serviceData);

            var data = new WCFObject(serviceData);

            data.SetValue("ObjectChanges.HandlerData.BizRuleHandlerType", (BizRuleHandlerTypeEnum)BizHandlerTypeProDropDownList.Data);
            (serviceData as BusinessRuleHandlerMaint).ObjectChanges.HandlerData.FieldAction = null;

        }

        public override void DisplayValues(Service serviceData)
        {
            base.DisplayValues(serviceData);

            var rows = ParametersGrid.Data as BizRuleHandlerParameterChanges[];
            for (int i = 0; i < rows.Length; i++)
            {
                string rowId = (ParametersGrid.GridContext as BoundContext).MakeAutoRowId(i);

                var row = rows[i];
                if (row.DataType == DataTypeEnum.Object)
                {
                    if ((ParametersGrid.GridContext as BoundContext).UnboundData == null)
                        (ParametersGrid.GridContext as BoundContext).UnboundData = new Dictionary<UnboundKey, object>();

                    UnboundKey key = new UnboundKey() { Row = rowId, Column = "ObjectDefaultValue" };
                    if (!(ParametersGrid.GridContext as BoundContext).UnboundData.ContainsKey(key))
                        (ParametersGrid.GridContext as BoundContext).UnboundData.Add(key, row.DefaultValue);
                    row.DefaultValue = row.ObjectDisplayValue;

                    RecordSet objectTypes = new CallStack(Page.CallStackKey).Context.LocalSession["ObjectTypes"] as RecordSet;
                    if (objectTypes != null)
                    {
                        var selValRow = objectTypes.Rows.FirstOrDefault(n => n.Values[2] == row.ObjectTypeName);
                        if (selValRow != null)
                            row.ObjectType = Int32.Parse(selValRow.Values[1]);
                    }
                }
            }
        }

        public virtual void LoadObjectTypes()
        {
            var service = new BusinessRuleHandlerMaintService(FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile);
            var data = new BusinessRuleHandlerMaint()
                        {
                            ObjectChanges = new BusinessRuleHandlerChanges()
                            {
                                HandlerData = new BusinessRuleHandlerDataChanges()
                                {
                                    FieldAction = Camstar.WCF.ObjectStack.Action.Create,
                                    Parameters = new BizRuleHandlerParameterChanges[] { new BizRuleHandlerParameterChanges() }
                                }
                            }
                        };

            var request = new BusinessRuleHandlerMaint_Request()
                        {
                            Info = new BusinessRuleHandlerMaint_Info()
                                {
                                    ObjectChanges = new BusinessRuleHandlerChanges_Info()
                                    {
                                        HandlerData = new BusinessRuleHandlerDataChanges_Info()
                                            {
                                                Parameters = new BizRuleHandlerParameterChanges_Info()
                                                    {
                                                        ObjectType = FieldInfoUtil.RequestSelectionValue()
                                                    }
                                            }
                                    }
                                }
                        };

            BusinessRuleHandlerMaint_Result result = null;
            ResultStatus rs = service.GetEnvironment(data, request, out result);
            if (rs.IsSuccess)
                new CallStack(Page.CallStackKey).Context.LocalSession.Add("ObjectTypes", result.Environment.ObjectChanges.HandlerData.Parameters.ObjectType.SelectionValues);
        }
    }
}
