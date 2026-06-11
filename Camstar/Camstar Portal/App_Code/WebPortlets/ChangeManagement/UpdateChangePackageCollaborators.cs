// Copyright Siemens 2023  
using Camstar.WCF.ObjectStack;
using Camstar.WCF.Services;
using Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using System;
using Camstar.WebPortal.FormsFramework.Utilities;


namespace Camstar.WebPortal.WebPortlets.ChangeManagement
{
    public class UpdateChangePackageCollaborators : MatrixWebPart
    {
        protected virtual JQDataGrid CollaboratorsGrid { get { return Page.FindCamstarControl("CollaboratorEntriesGrid") as JQDataGrid; } }
        protected virtual TextBox InstructionsForCollaborators { get { return Page.FindCamstarControl("InstructionsForCollaborators") as TextBox; } }
        protected virtual CheckBox NotifyCollaborators { get { return Page.FindCamstarControl("NotifyCollaborators") as CheckBox; } }

        protected virtual NamedObject MessageToCollaborator { get { return Page.FindCamstarControl("MessageToCollaborator") as NamedObject; } }

        protected virtual NamedObject MessageToOwner { get { return Page.FindCamstarControl("MessageToOwner") as NamedObject; } }

        protected override void OnPreRender(EventArgs e)
        {
            if (Page.IsPostBack)
                Page.SessionVariables["NotifyData"] = NotifyCollaborators.IsChecked;
            base.OnPreRender(e);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            NotifyCollaborators.CheckControl.CheckedChanged += NotifyCollaboratorsOnDataChanged;
        }

        protected virtual void NotifyCollaboratorsOnDataChanged(object sender, EventArgs eventArgs)
        {
            if (NotifyCollaborators.IsChecked && (MessageToCollaborator.Data == null || MessageToOwner.Data==null))
            {
                var serviceData = new UpdateChangePkg();
                Page.GetInputData(serviceData);
                var service = new UpdateChangePkgService(FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile);
                var request = new UpdateChangePkg_Request
                {
                    Info = new UpdateChangePkg_Info()
                    {
                        CollaboratorDetails = new CollaboratorDetails_Info()
                        {
                            EMailMessageToCollaborator = new Info(true),
                            EMailMessageToOwner = new Info(true)
                        },
                    }
                };
                UpdateChangePkg_Result result;
                service.LoadDefaultMessages(serviceData, request, out result);
                if (result != null && result.Value != null && result.Value.CollaboratorDetails != null)
                {
                    if(MessageToCollaborator.Data==null)
                        MessageToCollaborator.Data = result.Value.CollaboratorDetails.EMailMessageToCollaborator;
                    if (MessageToOwner.Data == null)
                        MessageToOwner.Data = result.Value.CollaboratorDetails.EMailMessageToOwner;
                }
            }


        }

        public virtual void LoadTemplate()
        {
            var serviceData = new UpdateChangePkg();
            Page.GetInputData(serviceData);
            var service = new UpdateChangePkgService(FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile);
            var request = new UpdateChangePkg_Request
            {
                Info = new UpdateChangePkg_Info()
                {
                    CollaboratorDetails = new CollaboratorDetails_Info()
                    {
                        CollaboratorEntries = new CollaboratorEntryDetails_Info()
                        {
                            SheetLevel = new Info(true),
                            Role = new Info(true),
                            Collaborator = new Info(true),
                            SubstituteOption = new Info(true),
                            Duration = new Info(true),
                            DurationUOM = new Info(true),
                            SpecialInstructions = new Info(true)
                        },
                        GeneralInstructions = new Info(true)
                    },
                }
            };
            UpdateChangePkg_Result result;
            service.LoadCollaboratorTemplate(serviceData, request, out result);
            if (result != null && result.Value != null && result.Value.CollaboratorDetails != null)
            {
                CollaboratorsGrid.Data = result.Value.CollaboratorDetails.CollaboratorEntries;
                InstructionsForCollaborators.Data = result.Value.CollaboratorDetails.GeneralInstructions;
            }
                
        }
    }
}
