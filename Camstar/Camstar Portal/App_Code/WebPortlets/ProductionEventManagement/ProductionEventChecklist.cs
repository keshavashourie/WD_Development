// Copyright Siemens 2024 
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using Camstar.WCF.ObjectStack;
using Camstar.WCF.Services;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WebPortal.PortalFramework;
using Camstar.WebPortal.WebPortlets;
using OM = Camstar.WCF.ObjectStack;

/// <summary>
/// Summary description for ProductionEventChecklist
/// </summary>
namespace Camstar.WebPortal.WebPortlets
{
    public class ProductionEventChecklist : MatrixWebPart
    {
        #region Properties

        protected virtual Button UpdateChecklist
        {
            get { return Page.FindCamstarControl("UpdateChecklist") as Button; }
        }

        protected virtual Camstar.WebPortal.FormsFramework.WebControls.NamedObject InstanceID
        {
            get
            {
                return Page.FindCamstarControl("InstanceID") as Camstar.WebPortal.FormsFramework.WebControls.NamedObject;
            }
        }
        protected virtual CamstarPortal.WebControls.Checklist ChecklistControl
        {
            get { return Page.FindCamstarControl("ChecklistControl") as CamstarPortal.WebControls.Checklist; }
        }

        protected virtual JQTabContainer TabContainer { get { return Page.FindCamstarControl("Tabs") as JQTabContainer; } }

        #endregion

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            UpdateChecklist.Click += UpdateChecklist_Click;

        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            if (TabContainer.SelectedItem.Name == "Checklist")
                LoadChecklists();
        }

        protected virtual void LoadChecklists()
        {
            var session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
            if (session != null)
            {
                var service = new UpdateEventChecklistService(session.CurrentUserProfile);
                var serviceData = new UpdateEventChecklist();

                serviceData.QualityObject = new NamedObjectRef()
                {
                    CDOTypeName = "Event",
                    Name = InstanceID.Data.ToString()
                };

                var request = new UpdateEventChecklist_Request()
                {
                    Info = new UpdateEventChecklist_Info()
                    {
                        QualityObject = new Info(true),
                        ExecuteChecklist = new ExecuteChecklist_Info()
                        {
                            ChecklistInstructions = new Info(true),
                            ServiceDetails = new ExecuteChecklistDetail_Info() {RequestValue = true}
                        }
                    }
                };

                var result = new UpdateEventChecklist_Result();

                ResultStatus resultStatus = service.Load(serviceData, request, out result);

                if (resultStatus != null && resultStatus.IsSuccess)
                {
                    ChecklistControl.Data = result.Value.ExecuteChecklist;
                    LoadDropdownList(result.Value.ExecuteChecklist);
                }
                else
                {
                    DisplayMessage(resultStatus);
                }
            }
        }

        private void UpdateChecklist_Click(object sender, EventArgs e)
        {
            var session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
            if (session != null)
            {
                var service = new UpdateEventChecklistService(session.CurrentUserProfile);
                var serviceData = new UpdateEventChecklist();
                serviceData.QualityESigDetail = ESigCaptureUtil.CollectQualityESigDetail();
                serviceData.QualityObject = new NamedObjectRef() { CDOTypeName = "Event", Name = InstanceID.Data.ToString() };
                serviceData.ExecuteChecklist = (ExecuteChecklist) ChecklistControl.Data;
                var request = new UpdateEventChecklist_Request();
                var result = new UpdateEventChecklist_Result();

                ResultStatus resultStatus = service.ExecuteTransaction(serviceData, request, out result);

                if (resultStatus != null && resultStatus.IsSuccess)
                {
                    DisplayMessage(resultStatus);
                }
                else
                {
                    DisplayMessage(resultStatus);
                }
                ESigCaptureUtil.CleanQualityESigCaptureDM();
            }
        }

        public override void GetInputData(Service serviceData)
        {
            base.GetInputData(serviceData);

            ((QualityTxn)serviceData).QualityObject = new NamedObjectRef() { CDOTypeName = "Event", Name = InstanceID.Data.ToString() };
        }

        private void LoadDropdownList(ExecuteChecklist executeChecklist)
        {
            if (executeChecklist == null || executeChecklist.ServiceDetails == null)
                return;

            int num = 0;
            foreach (var executeChecklistDetail in executeChecklist.ServiceDetails)
            {              
                if (executeChecklistDetail.ResponseEntryControl == OM.UIControlTypeEnum.Picklist)
                {
                    string text = executeChecklistDetail.ResponseSet != null ? GetIdByEntryName(executeChecklistDetail.ResponseSet.Name) : string.Empty;
                    DropDownList dropDownList = ChecklistControl.FindControl(text + "_DropDownList" + num + "_") as DropDownList;
                    var recordSet = CreateRecordSet(executeChecklistDetail.UserResponses);
                    dropDownList.SetSelectionValues(recordSet);

                    foreach(ExecuteChklstResponseDtl executeChklstResponseDtl in executeChecklistDetail.UserResponses)
                    {
                        if (executeChklstResponseDtl.ResponseSelected.Value)
                        {
                            dropDownList.Data = executeChklstResponseDtl.ResponseValue.ToString();
                        }
                    }                  
                }
                num++;
            }
        }

        private static string GetIdByEntryName(string name)
        {
            return name.Replace(" ", string.Empty).Replace("-", string.Empty).Replace("$", string.Empty);
        }

        private OM.RecordSet CreateRecordSet(IEnumerable<OM.ExecuteChklstResponseDtl> responses)
        {
            OM.RecordSet recSet = null;
            if (responses != null)
            {
                recSet = new OM.RecordSet
                {
                    Headers = new[] { new OM.Header(), new OM.Header() }
                };
                recSet.Headers[0].Label = new OM.Label { DefaultValue = "Value", Value = "Value" };
                recSet.Headers[0].Name = "Value";
                recSet.Headers[0].TypeCode = TypeCode.String;

                recSet.Headers[1].Label = new OM.Label { Value = "Name", DefaultValue = "Name" };
                recSet.Headers[1].TypeCode = TypeCode.String;
                recSet.Headers[1].Name = "Name";

                var rows = new List<OM.Row>();
                foreach (OM.ExecuteChklstResponseDtl response in responses)
                {
                    string ctrlLabel = string.Empty;
                    string ctrlValue = string.Empty;
                    if (response.ResponseItemDisplay != null)
                    {
                        ctrlLabel = response.ResponseItemDisplay.ResponseLabel.Value;
                        if (response.ResponseItemDisplay.ResponseValue != null)
                            ctrlValue = response.ResponseItemDisplay.ResponseValue.Value.ToString(CultureInfo.InvariantCulture);
                    }

                    var r = new OM.Row { Values = new[] { ctrlValue, ctrlLabel } };
                    rows.Add(r);
                }
                recSet.Rows = rows.ToArray();
            }
            return recSet;
        }
    }

}
