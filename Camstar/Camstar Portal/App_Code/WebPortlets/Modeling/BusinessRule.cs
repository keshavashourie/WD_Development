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
    public class BusinessRule : MatrixWebPart
    {
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            HandlersGrid.GridContext.RowUpdated += HandlersGrid_GridContext_RowUpdated;
            HandlersGrid.GridContext.RowDeleted += HandlersGrid_GridContext_RowDeleted;
            HandlersGrid.RowUpdated += HandlersGrid_RowUpdated;
            HandlersGrid.RowDeleted += RowDeleted;

            var postbackSource = Page.Request.Params["__EVENTTARGET"];
            if (Page.IsPostBack && !string.IsNullOrEmpty(postbackSource))
            {
                CamstarWebControl.SetRenderToClient(ParametersGrid);
            }
        }

        protected virtual ResponseData RowDeleted(object sender, JQGridEventArgs args)
        {
            return args.Response;
        }

        ResponseData HandlersGrid_RowUpdated(object sender, JQGridEventArgs args)
        {
           return args.Response;
        }

        protected virtual ResponseData HandlersGrid_GridContext_RowDeleted(object sender, JQGridEventArgs args)
        {
            RemoveParametersAssociatedWithDeletedHandlers();
            return args.Response;
        }

        protected virtual ResponseData HandlersGrid_GridContext_RowUpdated(object sender, JQGridEventArgs args)
        {
            var handlersGridData = HandlersGrid.Data as NamedObjectRef[];
            if (handlersGridData != null)
            {
                var index = Convert.ToInt32(args.State.RowID);
                if (!handlersGridData[index].IsNullOrEmpty())
                {
                    // if we change an existing handler we should remove parameters associated with previous one
                    if (index < handlersGridData.Length)
                    {
                        RemoveParametersAssociatedWithDeletedHandlers();
                    }
                    // add new handler
                    LoadParamSpecValues(new[] { handlersGridData[index] as NamedObjectRef });
                }
            }
            return args.Response;
        }

        public override void DisplayValues(Service serviceData)
        {
            base.DisplayValues(serviceData);
            var handlers = HandlersGrid.Data as NamedObjectRef[];
            LoadParamSpecValues(handlers);
        }

        public override void GetInputData(Service serviceData)
        {
            base.GetInputData(serviceData);

            BusinessRuleDataChanges data = (serviceData as BusinessRuleMaint).ObjectChanges.Data;
            data.FieldAction = null;
            if (data.Parameters != null)
            {
                foreach (var param in data.Parameters)
                {
                    if (param.ParamSpec != null && (param.ParamSpec.Parent as NamedSubentityRef)==null)
                    {
                        string handlerName = param.ParamSpec.Parent.ToString();
                        NamedObjectRef BRuleParent = WSObjectRef.AssignNamedObject(handlerName, "BusinessRuleHandler");
                        NamedSubentityRef BRuleParentData = WSObjectRef.AssignNamedSubentity(handlerName, BRuleParent, "BusinessRuleHandlerData");
                        param.ParamSpec.Parent = BRuleParentData;
                    }
                }
            }
        }

        protected virtual void RemoveParametersAssociatedWithDeletedHandlers()
        {
            var handlers = HandlersGrid.Data as NamedObjectRef[];
            var parameters = ParametersGrid.Data as BizRuleParameterChanges[];

            if ((handlers != null) && (parameters != null))
            {
                var resultParametersList = new List<BizRuleParameterChanges>();
                Array.ForEach<NamedObjectRef>(handlers, handler => resultParametersList.AddRange(parameters.Where(n => n.HandlerName == handler.Name)));

                ParametersGrid.Data = resultParametersList.ToArray();
            }
        }

        protected virtual void LoadParamSpecValues(NamedObjectRef[] handlers)
        {
            if (handlers != null)
            {
                var service = new BusinessRuleMaintService(FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile);
                var data = new BusinessRuleMaint()
                    {
                        ObjectChanges = new BusinessRuleChanges()
                            {
                                Data = new BusinessRuleDataChanges()
                                    {
                                        Handlers = handlers,
                                        Parameters = new BizRuleParameterChanges[] { new BizRuleParameterChanges() },
                                        FieldAction = Camstar.WCF.ObjectStack.Action.Create
                                    }
                            }
                    };
                var request = new BusinessRuleMaint_Request()
                    {
                        Info = new BusinessRuleMaint_Info()
                            {
                                ObjectChanges = new BusinessRuleChanges_Info()
                                    {
                                        Data = new BusinessRuleDataChanges_Info()
                                            {
                                                Parameters = new BizRuleParameterChanges_Info()
                                                    {
                                                        ParamSpec = FieldInfoUtil.RequestSelectionValue()
                                                    }
                                            }
                                    }
                            }
                    };
                var result = new BusinessRuleMaint_Result();

                ResultStatus rs = service.GetEnvironment(data, request, out result);

                if (rs.IsSuccess)
                {
                    PopulateParametersGrid(result.Environment.ObjectChanges.Data.Parameters.ParamSpec.SelectionValues);
                }
            }
        }

        protected virtual void PopulateParametersGrid(RecordSet parametersSelectionData)
        {
            if (parametersSelectionData != null)
            {
                var existingParameters = GetExistingParametersList(parametersSelectionData);

                InsertNewParameters(parametersSelectionData, existingParameters);

                UpdateDataTypes(parametersSelectionData);
            }
        }

        protected virtual int GetColumnId(RecordSet data, string name)
        {
            return data.Headers.ToList().IndexOf(data.Headers.FirstOrDefault(header => header.Name.Equals(name)));
        }

        protected virtual List<String> GetExistingParametersList(RecordSet data)
        {
            var existingParameters = new List<String>();
            var parametersGridData = ParametersGrid.Data as BizRuleParameterChanges[];
            if (parametersGridData != null)
            {
                foreach (var row in parametersGridData)
                {
                    BizRuleHandlerParameterChanges handlerParameter = GetHandlerParameter(data, row.HandlerName.Value, row.ParamSpecName.Value);
                    if (!handlerParameter.IsNullOrEmpty())
                    {
                        existingParameters.Add(handlerParameter.Self.ID);
                    }
                }
            }
            return existingParameters;
        }

        /// <summary>
        /// Get BizRuleHandlerParameterChanges instance with DataType filled
        /// </summary>
        protected virtual BizRuleHandlerParameterChanges GetHandlerParameter(RecordSet data, String handlerName, String paramName)
        {
            BizRuleHandlerParameterChanges result = null;
            DataRow[] handlerParameterData =
                data.GetAsDataTable().Select("HandlerName" + "='" + handlerName + "' and " + "HandlerParamName" + "='" + paramName + "'");
            if (handlerParameterData.Length > 0)
            {
                result = new BizRuleHandlerParameterChanges();
                result.Self = new BaseObjectRef();
                result.Self.ID = handlerParameterData[0].ItemArray[GetColumnId(data, ParamId)].ToString();
                result.Name = handlerParameterData[0].ItemArray[GetColumnId(data, ParamName)].ToString();
                result.DataTypeName = handlerParameterData[0].ItemArray[GetColumnId(data, DataTypeName)].ToString();
                result.Parent = new NamedObjectRef();
            }
            return result;
        }

        /// <summary>
        /// Insert new rows for new handlers
        /// store remaining data from selection values to fill new rows later
        /// </summary>
        protected virtual void InsertNewParameters(RecordSet parametersSelectionData, List<String> existingParameters)
        {
            foreach (var row in parametersSelectionData.Rows)
            {
                var parameter = new BizRuleParameterChanges();
                parameter.Self = new BaseObjectRef();
                parameter.Self.ID = row.Values[GetColumnId(parametersSelectionData, ParamId)];

                if (!existingParameters.Contains(parameter.Self.ID))
                {
                    parameter.HandlerName = row.Values[GetColumnId(parametersSelectionData, HandlerName)];
                    parameter.ListItemAction = ListItemAction.Add;
                    parameter.ParamSpecName = row.Values[GetColumnId(parametersSelectionData, ParamName)];
                    parameter.ParamSpec = WSObjectRef.AssignNamedSubentity(parameter.ParamSpecName.Value,
                                                                           WSObjectRef.AssignNamedObject(
                                                                               parameter.HandlerName.Value));
                    InsertParameterIntoGrid(parameter);
                }

            }
        }

        protected virtual void InsertParameterIntoGrid(BizRuleParameterChanges parameterToInsert)
        {
            var parameters = ParametersGrid.Data as BizRuleParameterChanges[];
            if (parameters != null)
            {
                Array.Resize(ref parameters, parameters.Length + 1);
                parameters[parameters.Length - 1] = parameterToInsert;
            }
            else
            {
                parameters = new BizRuleParameterChanges[1] { parameterToInsert };
            }
            ParametersGrid.Data = parameters;
        }

        /// <summary>
        /// Update parameter's data type
        /// </summary>
        protected virtual void UpdateDataTypes(RecordSet parametersSelectionData)
        {
            var rows = ParametersGrid.Data as BizRuleParameterChanges[];
            if (rows != null)
            {
                var boundParametersContext = ParametersGrid.GridContext as BoundContext;
                if (boundParametersContext != null)
                {
                    if (boundParametersContext.UnboundData == null)
                    {
                        boundParametersContext.UnboundData = new Dictionary<UnboundKey, object>();
                    }
                    for (var i = 0; i < rows.Length; ++i)
                    {
                        var rowId = boundParametersContext.MakeAutoRowId(i);
                        var key = new UnboundKey() {Row = rowId, Column = DataTypeName};

                        var parameter = parametersSelectionData.Rows
                            .FirstOrDefault(r => (r.Values[GetColumnId(parametersSelectionData, ParamName)] == rows[i].ParamSpecName
                                && r.Values[GetColumnId(parametersSelectionData, HandlerName)] == rows[i].HandlerName));

                        if (!boundParametersContext.UnboundData.ContainsKey(key))
                        {
                            boundParametersContext.UnboundData.Add(key, parameter
                                                                       .Values[GetColumnId(parametersSelectionData, DataTypeName)]);
                        }
                    }
                }
            }
        }

        protected virtual JQDataGrid HandlersGrid
        {
            get { return Page.FindCamstarControl("Data_Handlers") as JQDataGrid; }
        }

        protected virtual JQDataGrid ParametersGrid
        {
            get { return Page.FindCamstarControl("Data_Parameters") as JQDataGrid; }
        }

        private const string HandlerName = "HandlerName";
        private const string ParamName = "HandlerParamName";
        private const string ParamId = "HandlerParamId";
        private const string DataTypeName = "DataType";

    }
}
