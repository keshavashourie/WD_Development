// Copyright Siemens 2023  
using System;
using System.Data;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using Camstar.WCF.Services;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using Camstar.WebPortal.Personalization;
using Camstar.WebPortal.PortalFramework;

using Camstar.WebPortal.Utilities;
using Camstar.WebPortal.FormsFramework.Utilities;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WCF.ObjectStack;

namespace Camstar.WebPortal.WebPortlets
{

    /// <summary>
    /// Handle doc attach functionality in the Container Status header web part
    /// </summary>
    public class ContainerStatus : MatrixWebPart
    {
	    #region Controls
        	   

        protected virtual ContainerListGrid ContainerField
		{
			get { return Page.FindCamstarControl("ContainerStatus_ContainerName") as ContainerListGrid; }
		}

        protected virtual CWC.Button DocAttachBtn
		{
			get { return Page.FindCamstarControl("DocAttachButton") as CWC.Button; }
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
			DocAttachBtn.Click += new EventHandler(DocAttachBtn_Click);
        }

        #endregion

        #region Public Functions

        #endregion

        #region Protected Functions
        /// <summary>
        /// Set data contracts when the Doc Attach button is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void DocAttachBtn_Click(object sender, EventArgs e)
		{		
			UIComponentDataMember CalledExternally = new UIComponentDataMember();
			CalledExternally.Name = "CalledExternally";
			CalledExternally.Value = true;
			if (Page.SessionDataContract.GetValueByName("CalledExternally") == null)
			{
				UIComponentDataMember[] NewDataMembers = new UIComponentDataMember[Page.SessionDataContract.DataMembers.Length + 1];
				int index = 0;
				foreach (UIComponentDataMember CDM in Page.SessionDataContract.DataMembers)
				{
					NewDataMembers.SetValue(CDM, index);
					index = index + 1;
				}
				NewDataMembers.SetValue(CalledExternally, index);
				Page.SessionDataContract.DataMembers = NewDataMembers;
			}
			else
				Page.SessionDataContract.SetValueByName("CalledExternally", CalledExternally);

			UIComponentDataMember IsContainer = new UIComponentDataMember();
			IsContainer.Name = "IsContainer";
			IsContainer.Value = true;
			if (Page.SessionDataContract.GetValueByName("IsContainer") == null)
			{
				UIComponentDataMember[] NewDataMembers = new UIComponentDataMember[Page.SessionDataContract.DataMembers.Length + 1];
				int index = 0;
				foreach (UIComponentDataMember CDM in Page.SessionDataContract.DataMembers)
				{
					NewDataMembers.SetValue(CDM, index);
					index = index + 1;
				}
				NewDataMembers.SetValue(IsContainer, index);
				Page.SessionDataContract.DataMembers = NewDataMembers;
			}
			else
				Page.SessionDataContract.SetValueByName("IsContainer", IsContainer);

			UIComponentDataMember ContainerName = new UIComponentDataMember();
			ContainerName.Name = "ContainerName";
			ContainerName.Value = ContainerField.Data;
			if (Page.SessionDataContract.GetValueByName("ContainerName") == null)
			{
				UIComponentDataMember[] NewDataMembers = new UIComponentDataMember[Page.SessionDataContract.DataMembers.Length + 1];
				int index = 0;
				foreach (UIComponentDataMember CDM in Page.SessionDataContract.DataMembers)
				{
					NewDataMembers.SetValue(CDM, index);
					index = index + 1;
				}
				NewDataMembers.SetValue(ContainerName, index);
				Page.SessionDataContract.DataMembers = NewDataMembers;
			}
			else
				Page.SessionDataContract.SetValueByName("ContainerName", ContainerName);
		}
        #endregion

        #region Constants

        #endregion

	    #region Private Member Variables

        #endregion

    }

}

