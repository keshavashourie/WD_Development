// Copyright Siemens 2023  
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using Camstar.WCF.ObjectStack;
using Camstar.WCF.Services;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.FormsFramework.WebControls.PickLists;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using Camstar.WebPortal.Personalization;
using Camstar.WebPortal.Utilities;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using DT = Camstar.WebPortal.WebPortlets.DataTransfer;

namespace Camstar.WebPortal.WebPortlets
{

    /// <summary>
    /// Handle doc attach functionality in the Container Status header web part
    /// </summary>
    public class HierarchicalView  : MatrixWebPart
    {
        #region Controls


        protected virtual CWC.DropDownList ContentHierarchy
        {
            get { return Page.FindCamstarControl("ContentHierarchy") as CWC.DropDownList; }
        }
        protected virtual CWC.Button ToggleBtn
        {
            get { return Page.FindCamstarControl("ToggleBtn") as CWC.Button; }
        }
        protected virtual CWC.Label ExpandAllLbl
        {
            get { return Page.FindCamstarControl("ExpandAllLbl") as CWC.Label; }
        }
        protected virtual CWC.Label CollapseAllLbl
        {
            get { return Page.FindCamstarControl("CollapseAllLbl") as CWC.Label; }
        }
        #endregion

        #region Protected Functions

        /// <summary>
        /// Set the event handler on the doc attach button
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            var dc = Page.DataContract.GetValueByName<IEnumerable<ObjectTypeItem>>("HierarchyInstances");           
            if (dc != null && dc.Count() == 0)
            {
                ToggleBtn.Visible = false;
                ContentHierarchy.Visible = false;
                var errorMsg = FrameworkManagerUtil.GetLabelValue("AtLeastOneInstanceWarningLbl") ?? string.Empty;
                DisplayMessage(new ResultStatus(errorMsg, false));
            }
            else
            {
                if (Page.IsPostBack)
                {
                    return;
                }
                if (dc == null && Page.DataContract.GetValueByName<string>("InstanceId") != null)
                {
                    dc = new[] { new ObjectTypeItem { Instances = new[] { new SelectedInstanceItem { InstanceID = Page.DataContract.GetValueByName<string>("InstanceId") } } } };

                    // Get References when opened from modeling page
                    DT.DataTransfer dt = new DataTransfer.DataTransfer(new DT.DataTransferInfo(FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile.Name), new DT.DataTransferRepository(FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile), new Dictionary<string, string>());
                    var refs = dt.GetReferences(dc.FirstOrDefault().Instances.ToArray(), new string[0]);
                    if (refs != null && refs.Length > 0)
                    {
                        foreach (var inst in refs)
                        {
                            instances.Add(inst);
                        }
                    }
                }               
                var label = FrameworkManagerUtil.GetLabelCache().GetLabelByName("Lbl_ContentHierarchy");
                if (label != null)
                {
                    Page.Title = label.Value;
                }
                if (dc == null)
                {
                    return;
                }
                var service = new CDOInquiryService(FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile);

                foreach (var objectTypeItem in dc)
                {
                    // Only add instances if instance does not already exist in list
                    instances.AddRange(objectTypeItem.Instances.Where(x => instances.FirstOrDefault(y => y.InstanceID == x.InstanceID) == null));
                }

                var data = new CDOInquiry
                {
                    Recursive = true,
                    FilterReferences = true                    
                };                                
                data.SelectedInstances = instances.Select(
                    i => new BaseObjectRef
                    {
                        ID = i.InstanceID,
                        ListItemAction = ListItemAction.Add
                    }
                ).ToArray();
                var request = new CDOInquiry_Request
                {
                    Info = new CDOInquiry_Info
                    {
                        ObjectReferencesList = new ObjectReferencesInfo_Info
                        {
                            RequestValue = true
                        }
                    }
                };
                var result = new CDOInquiry_Result();
                ResultStatus rs = service.GetReferences(data, request, out result);

                if (!rs.IsSuccess || result.Value == null || result.Value.ObjectReferencesList == null)
                {
                    ContentHierarchy.Visible = ToggleBtn.Visible = false;
                    FormProcessor.DisplayMessage(rs);
                    return;
                }
                if (dc != null && dc.Count() > 0)
                {
                    foreach (var ot in dc)
                    {
                        instances.AddRange(ot.Instances);
                    }
                }
                ContentHierarchy.HideFilter = true;

                ContentHierarchy.DisplayingData += (sender, args) =>
                {
                    var list = new List<tree_row>();
                    foreach (var objRef in result.Value.ObjectReferencesList)
                    {
                        var node = new tree_row(((string)objRef.ObjectTypeValue).ToUpper() + ": " + (string.IsNullOrEmpty((string)objRef.Revision) ? (string)objRef.ObjectName : (string)objRef.ObjectName + CamstarPortalSection.GetRevisionDelimiter() + (string)objRef.Revision), (string)objRef.ObjectInstanceId);
                        var dcInstance = instances.FirstOrDefault(i => i.InstanceID.Equals((string)objRef.ObjectInstanceId));
                        if (dcInstance != null && !dcInstance.IsRef)
                        {
                            node.li_attr.@class = manuallyAddedObjectsStyle;
                        }
                        if (objRef.ObjectFields != null)
                        {
                            foreach (var objectField in objRef.ObjectFields)
                            {
                                ProcessReferences(list, node, objectField);
                            }
                        }
                        list.Add(node);
                    }

                    args.Data = list;
                    args.ViewMode = "tree";
                };
                ContentHierarchy.SetSelectionValues(new RecordSet());
                ScriptManager.RegisterStartupScript(ContentHierarchy, typeof(string), "expand_all_onload", "$(function() { var panel = $(\"#" + ContentHierarchy.PickListPanelControl.ClientID + " .viewer\"); " +
                                                                                                          "panel.bind(\"ready.jstree\", function() { var opened = true; $(\"#" + ToggleBtn.ClientID + "\")" +
                                                                                                          ".click(function() { if(opened) { panel.jstree(\"close_all\"); this.value = \"" + ExpandAllLbl.Text + "\"; } " +
                                                                                                          "else { panel.jstree(\"open_all\"); this.value = \"" + CollapseAllLbl.Text + "\"; } opened = !opened; return false; }); panel.jstree(\"open_all\"); }); });", true);
                ToggleBtn.LabelText = CollapseAllLbl.Text;
            }
        }


        protected virtual void ProcessReferences(IList<tree_row> tree, tree_row parent, ObjectField objField)
        {
            if (objField is ReferenceField)
            {
                var field = (ReferenceField)objField;
                if (field.References != null)
                {
                    var firstRef = field.References.FirstOrDefault();
                    var objTypeName = string.Empty;
                    if (firstRef != null)
                    {
                        objTypeName = (string)firstRef.ObjectTypeValue;
                    }
                    foreach (var reference in field.References)
                    {
                        var dcInstance = instances.FirstOrDefault(i => i.InstanceID.Equals((string)reference.ObjectInstanceId));
                        //filter out response references for Export & Assign Content
                        if (dcInstance == null)
                        {
                            continue;
                        }
                        var refNode = new tree_row(String.Format("{0}: {1}", (objTypeName).ToUpper(), string.IsNullOrEmpty((string)reference.Revision) ? (string)reference.ObjectName : (string)reference.ObjectName + CamstarPortalSection.GetRevisionDelimiter() + reference.Revision), (string)reference.ObjectInstanceId);
                        //filter out "root duplicates"
                        for (var i = 0; i < tree.Count; i++)
                            if (tree[i].li_attr.key.Equals(refNode.li_attr.key) && tree[i].Children.Count == 0)
                            {
                                tree.RemoveAt(i);
                                break;
                            }
                        if (dcInstance != null && !dcInstance.IsRef)
                            refNode.li_attr.@class = manuallyAddedObjectsStyle;
                        if (reference.ObjectFields != null)
                            foreach (var objectField in reference.ObjectFields)
                                ProcessReferences(tree, refNode, objectField);
                        parent.AddChild(refNode);
                    }
                }
            }
            else if (objField is SubentityField)
            {
                var field = (SubentityField)objField;
                if (field.Instances != null)
                {
                    var firstInst = field.Instances.FirstOrDefault();
                    var objTypeName = string.Empty;
                    if (firstInst != null)
                    {
                        objTypeName = (string)firstInst.ObjectTypeName;
                    }

                    foreach (var instance in field.Instances)
                    {                        
                        var instanceNode = new tree_row(string.Format("{0}: {1}", objTypeName.ToUpper(), (string)instance.ObjectDisplayName, instance.Self.ID));
                        if (instance.ObjectFields != null)
                            foreach (var objectField in instance.ObjectFields)
                                ProcessReferences(tree, instanceNode, objectField);
                        parent.AddChild(instanceNode);
                    }
                }
            }
        }

        #endregion

        #region Public Functions

        #endregion

        #region Protected Functions

        #endregion

        #region Constants

        #endregion

        #region Private Member Variables        

        private List<SelectedInstanceItem> instances = new List<SelectedInstanceItem>(); 
        private const string manuallyAddedObjectsStyle = "hview-manually-added-object";        
        #endregion

    }

}

