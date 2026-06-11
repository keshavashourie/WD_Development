// Copyright Siemens 2024  

using Camstar.WebPortal.FormsFramework.WebGridControls;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WebPortal.FormsFramework.Utilities;
using System.Web;
using Camstar.WCF.Services;
using Camstar.WCF.ObjectStack;
using System.Web.UI.WebControls;
using System;
using System.Collections.Generic;
using Camstar.WebPortal.FormsFramework;

namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class Slitting : MatrixWebPart
    {
        protected virtual CWC.CheckBox AutoNumber
        {
            get { return Page.FindCamstarControl("Slit_AutoNumber") as CWC.CheckBox; }
        }

        protected virtual CWC.NamedObject NumberingRule
        {
            get { return Page.FindCamstarControl("Slitting_NumberingRule") as CWC.NamedObject; }
        }
        protected virtual CWC.TextBox SlitCount
        {
            get { return Page.FindCamstarControl("Slitting_SlitCount") as CWC.TextBox; }
        }

        protected virtual JQDataGrid ToContainersGrid
        {
            get { return Page.FindCamstarControl("ToContainerGrid") as JQDataGrid; }
        }

        protected virtual Button AddToGridButton
        {
            get
            {
                return Page.FindCamstarControl("AddToGridButton") as Button;
            }
        }

        protected virtual CWC.TextBox ContainerQty
        {
            get
            {
                return Page.FindCamstarControl("ContainerStatus_Qty") as CWC.TextBox;
            }
        }

        protected virtual CWC.Container HiddenSelectedContainer
        {
            get { return Page.FindCamstarControl("HiddenSelectedContainer") as CWC.Container; }
        }

        protected virtual string ToContainerNameRequiredErrorMessage
        {
            get
            {
                return FrameworkManagerUtil.GetLabelCache(HttpContext.Current.Session).GetLabelByName("SplitQty_ToContainerNameRequired").Value;
            }
        }

        private const string _SequenceNumPlaceholder = "seq_num";
        protected string _NumRuleFormat = string.Empty;

        private SplitDetails CollectControls( string _numberingRuleFormat) => new SplitDetails
        {
            Qty = new Primitive<double>(double.Parse(ContainerQty.Data.ToString())),
            ToContainerName = _numberingRuleFormat
        };

        public override bool PreExecute(Info serviceInfo, Service serviceData)
        {
            bool status = base.PreExecute(serviceInfo, serviceData);

            if (AutoNumber.IsChecked && NumberingRule.Data == null)
            {
                status = false;
                var labelCache = FrameworkManagerUtil.GetLabelCache(HttpContext.Current.Session);
                Page.DisplayWarning(labelCache.GetLabelByName("Slitting_NoNumberingRuleDefined").Value);
            }

            if (!AutoNumber.IsChecked)
            {
                SplitDetails[] rows = ToContainersGrid.Data as SplitDetails[];
                foreach (var row in rows)
                {
                    if ( row.ToContainerName != null && string.IsNullOrEmpty(row.ToContainerName.Value))
                    {
                        Page.DisplayMessage(new ResultStatus(ToContainerNameRequiredErrorMessage, false));
                        status = false;
                    }
                    
                }
            }

            return status;
        }

        protected override void OnLoad(System.EventArgs e)
        {
            base.OnLoad(e);

            AutoNumber.CheckControl.CheckedChanged += CheckControl_CheckedChanged;
            AddToGridButton.Click += AddToGridButton_Click;
			HiddenSelectedContainer.DataChanged += HiddenSelectedContainer_DataChanged;

            if (!Page.IsPostBack && HiddenSelectedContainer.Data != null)
            {
                HiddenSelectedContainer.LoadOrClearDependentValues();
            }
            SetToContainersGridControl();
        }

        protected virtual void CheckControl_CheckedChanged(object sender, System.EventArgs e)
        {
            ToContainersGrid.ClearData();
            if (ToContainersGrid.Data != null)
            {
                SplitDetails[] rows = ToContainersGrid.Data as SplitDetails[];
                foreach (var row in rows)
                    row.ToContainerName = null;
            }         
        }
		
        protected virtual void HiddenSelectedContainer_DataChanged(object sender, System.EventArgs e)
        {
            ToContainersGrid.ClearData();   
        }		

        protected virtual void AddToGridButton_Click(object sender, EventArgs e)
        {
            var i_SlitCount = SlitCount.Data == null ? 0 : int.Parse(SlitCount.Data.ToString());
            var allContainers = ToContainersGrid.Data as List<SplitDetails>;

            if (AutoNumber.IsChecked && NumberingRule.Data != null)
            {
                _NumRuleFormat = GetNumRuleFormat(NumberingRule.Data.ToString());
            }

            if (ContainerQty != null && i_SlitCount > 0)
            {
                for (int i = 0; i < i_SlitCount; i++)
                {
                    var obj = CollectControls(_NumRuleFormat);
                    if (allContainers == null)
                    {
                        allContainers = new List<SplitDetails>();
                    }
                    allContainers.Add(obj);
                }
                ToContainersGrid.Data = allContainers.ToArray();
            }
        }

        private void SetToContainersGridControl()
        {
            
            if (AutoNumber.IsChecked)
            {
                Personalization.GridDataSettingsBase toContainersGridSettings = new Personalization.GridDataSettingsBase();

                Personalization.JQNavigatorAction[] gridNavigatorActions = new Personalization.JQNavigatorAction[5];

                gridNavigatorActions[0] = new Personalization.JQNavigatorAction
                {
                    Action = Personalization.JQGridNavActionType.Add,
                    Enable = false,
                    Visible = false
                };
                gridNavigatorActions[1] = new Personalization.JQNavigatorAction
                {
                    Action = Personalization.JQGridNavActionType.Delete,
                    Enable = false,
                    Visible = false
                };
                gridNavigatorActions[2] = new Personalization.JQNavigatorAction
                {
                    Action = Personalization.JQGridNavActionType.Excel,
                    Enable = true,
                    Visible = true
                };
                gridNavigatorActions[3] = new Personalization.JQNavigatorAction
                {
                    Action = Personalization.JQGridNavActionType.Refresh,
                    Enable = false,
                    Visible = false
                };
                gridNavigatorActions[4] = new Personalization.JQNavigatorAction
                {
                    Action = Personalization.JQGridNavActionType.Edit,
                    Enable = false,
                    Visible = false
                };
                toContainersGridSettings = ToContainersGrid.Settings;
                ToContainersGrid.GridContext.EditingMode = Personalization.JQEditingModes.Disabled;
                toContainersGridSettings.NavigatorActions = gridNavigatorActions;
                ToContainersGrid.Settings = toContainersGridSettings;
            }
            else {
                Personalization.JQNavigatorAction[] gridNavigatorActions = new Personalization.JQNavigatorAction[2];
                gridNavigatorActions[0] = new Personalization.JQNavigatorAction
                {
                    Action = Personalization.JQGridNavActionType.Refresh,
                    Enable = false,
                    Visible = false
                };
                gridNavigatorActions[1] = new Personalization.JQNavigatorAction
                {
                    Action = Personalization.JQGridNavActionType.Edit,
                    Enable = false,
                    Visible = false
                };
                ToContainersGrid.Settings.NavigatorActions = gridNavigatorActions;
                ToContainersGrid.GridContext.EditingMode = Personalization.JQEditingModes.Inline;
            }
            CamstarWebControl.SetRenderToClient(ToContainersGrid);
            ToContainersGrid.GridContext.RenderToClient = true;
        }

        public override void GetInputData(Service serviceData)
        {
            base.GetInputData(serviceData);
            if (AutoNumber.IsChecked)
            {
                var data = (serviceData as WCF.ObjectStack.Slitting);

                if (data != null)
                {
                    foreach (var ToContainerDetailsItem in data.ToContainerDetails)
                    {
                        ToContainerDetailsItem.ToContainerName = null;
                    }
                }
            }
        }

        public override void WebPartCustomAction(object sender, Personalization.CustomActionEventArgs e)
        {
            base.WebPartCustomAction(sender, e);
            var action = e.Action as Camstar.WebPortal.Personalization.CustomAction;

            if (action != null && action.Parameters == "ClearAll")
            {
                ClearAll();
            }
        }

        public override void PostExecute(ResultStatus status, Service serviceData)
        {
            base.PostExecute(status, serviceData);
            if (status.IsSuccess)
            {
                ClearAll();
            }
        }

        protected virtual void ClearAll()
        {
            AutoNumber.ClearData();
            SetToContainersGridControl();
            Page.ShopfloorReset(null, null);
        }

        protected virtual string GetNumRuleFormat(string numberingRule)
        {
            string prefix = string.Empty;
            string suffix = string.Empty;

            if (numberingRule != null)
            {
                var session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
                if (session != null)
                {
                    var service = new NumberingRuleMaintService(session.CurrentUserProfile);
                    var serviceData = new NumberingRuleMaint
                    {
                        ObjectToChange = new NamedObjectRef(numberingRule, "NumberingRule")
                    };

                    var request = new NumberingRuleMaint_Request
                    {
                        Info = new NumberingRuleMaint_Info
                        {
                            ObjectToChange = new Info(true),
                            ObjectChanges = new NumberingRuleChanges_Info
                            {
                                Prefix = new Info(true),
                                Suffix = new Info(true)
                            }
                        }
                    };

                    var result = new NumberingRuleMaint_Result();
                    ResultStatus status = service.Load(serviceData, request, out result);
                    if (status != null && status.IsSuccess)
                    {
                        prefix = result.Value.ObjectChanges.Prefix != null ? Convert.ToString(result.Value.ObjectChanges.Prefix.Value) : string.Empty;
                        suffix = result.Value.ObjectChanges.Suffix != null ? Convert.ToString(result.Value.ObjectChanges.Suffix.Value) : string.Empty;

                        if (!string.IsNullOrEmpty(prefix) && prefix.Substring(0, 1) == "\"" && prefix.Substring(prefix.Length - 1, 1) == "\"")
                            prefix = prefix.Substring(1, prefix.Length - 2);

                        if (!string.IsNullOrEmpty(suffix) && suffix.Substring(0, 1) == "\"" && suffix.Substring(suffix.Length - 1, 1) == "\"")
                            suffix = suffix.Substring(1, suffix.Length - 2);
                    }

                }
            }
            else
                return string.Empty;


            return prefix + _SequenceNumPlaceholder + suffix;
        }

    }
}
