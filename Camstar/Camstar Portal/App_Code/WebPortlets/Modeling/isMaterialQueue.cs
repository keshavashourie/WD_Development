using System.Data;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using System;
using Camstar.WebPortal.FormsFramework.WebControls.PickLists;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using Camstar.WCF.ObjectStack;
using Camstar.WebPortal.FormsFramework.WebControls;

/// <summary>
/// Summary description for isMaterialQueue
/// </summary>
namespace Camstar.WebPortal.WebPortlets.Modeling
{
    public class isMaterialQueue : MatrixWebPart
    {
        #region Property
        protected virtual ContainerListGrid KitContainer
        {
            get
            {
                return Page.FindCamstarControl("ObjectChanges_isContainerObject") as ContainerListGrid;
            }
        }

        protected virtual ContainerListGrid KitContainerChangeHidden
        {
            get
            {
                return Page.FindCamstarControl("ObjectChanges_isContainerObject0") as ContainerListGrid;
            }
        }        

        protected virtual NamedObject KitMfgOrder
        {
            get
            {
                return Page.FindCamstarControl("ObjectChanges_isMfgOrder") as NamedObject;
            }
        }

        protected virtual NamedObject KitOperation
        {
            get
            {
                return Page.FindCamstarControl("ObjectChanges_isOperation") as NamedObject;
            }
        }

        protected virtual NamedObject KitResource
        {
            get
            {
                return Page.FindCamstarControl("ObjectChanges_isResource") as NamedObject;
            }
        }

        protected virtual CheckBox isIsKitCheckBox
        {
            get
            {
                return Page.FindCamstarControl("ObjectChanges_isIsKit") as CheckBox;
            }
        }

        protected bool isIsKit
        {
            get
            {
                return KitContainer.Data != null || KitMfgOrder.Data != null || KitOperation.Data != null || false;
            }
        }
        #endregion

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            KitMfgOrder.DataChanged += KitMfgOrderOnDataChanged;
            KitContainer.DataChanged += KitContainerOnDataChanged;
            KitOperation.DataChanged += KitOperationOnDataChanged;
            KitResource.DataChanged += KitResourceOnDataChanged;
            KitContainerChangeHidden.DataChanged += KitContainerChangeHiddenOnDataChanged;
        }

         /// <summary>
         /// Event handlers for kitting controls
         /// </summary>
         /// <param name="sender"></param>
         /// <param name="e"></param>
        private void KitContainerOnDataChanged(object sender, EventArgs e)
        {
            HandlerOfDataChangedEventOnControls(sender, e);
            KitContainerChangeHidden.Data = KitContainer.Data;
        }

        private void KitContainerChangeHiddenOnDataChanged(object sender, EventArgs e)
        {
            HandlerOfDataChangedEventOnControls(sender, e);
            KitContainer.Data = KitContainerChangeHidden.Data;
        }

        private void KitMfgOrderOnDataChanged(object sender, EventArgs e)
        {
            HandlerOfDataChangedEventOnControls(sender, e);
        }

        private void KitOperationOnDataChanged(object sender, EventArgs e)
        {
            HandlerOfDataChangedEventOnControls(sender, e);
        }

        private void KitResourceOnDataChanged(object sender, EventArgs e)
        {
            HandlerOfDataChangedEventOnControls(sender, e);
        }
        //event handlers block for kitting controls

        private void HandlerOfDataChangedEventOnControls(object sender, EventArgs e)
        {
            isIsKitCheckBox.Data = isIsKit;            
        }
    }
}