// Copyright Siemens 2023  
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using Camstar.WCF.ObjectStack;
using OM = Camstar.WCF.ObjectStack;
using Camstar.WCF.Services;
using Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WebPortal.FormsFramework.WebControls.PickLists;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.Utilities;
using Camstar.WebPortal.WCFUtilities;
using Camstar.WebPortal.FormsFramework.HtmlControls;
using Camstar.WebPortal.FormsFramework;


/// <summary>
/// Summary description for NumberingRuleMaint
/// </summary>

namespace Camstar.WebPortal.WebPortlets.Modeling
{
    /// <summary>
    /// TODO: Add a Summary description for this Camstar Web Part
    /// </summary>
    public class NumberingRuleMaint: MatrixWebPart
    {
        #region Controls

        protected virtual CWC.CheckBox ResetLastAssignedSeqField
        {
            get { return Page.FindCamstarControl("ResetLastAssignedSeqField") as CWC.CheckBox; }
        }

        protected virtual CWC.TextBox MaximumValue
        {
            get { return Page.FindCamstarControl("ObjectChanges_MaximumValue") as CWC.TextBox; }
        }

        protected virtual CWC.TextBox MaximumValueHex
        {
            get { return Page.FindCamstarControl("ObjectChanges_MaximumValueHex") as CWC.TextBox; }
        }

        protected virtual CWC.TextBox MaximumValueAlphaNumeric
        {
            get { return Page.FindCamstarControl("ObjectChanges_MaximumValueAlphaNumeric") as CWC.TextBox; }
        }

        protected virtual CWC.TextBox LastAssignedSequence
        {
            get { return Page.FindCamstarControl("ObjectChanges_LastAssignedSequence") as CWC.TextBox; }
        }

        protected virtual CWC.TextBox LastAssignedSequenceHex
        {
            get { return Page.FindCamstarControl("ObjectChanges_LastAssignedSequenceHex") as CWC.TextBox; }
        }

        protected virtual CWC.TextBox LastAssignedSequenceAlphaNumeric
        {
            get { return Page.FindCamstarControl("ObjectChanges_LastAssignedSequenceAlphaNumeric") as CWC.TextBox; }
        }

        protected virtual CWC.CheckBox UseHexadecimalValue
        {
            get { return Page.FindCamstarControl("ObjectChanges_UseHexadecimalValue") as CWC.CheckBox; }
        }

        protected virtual CWC.CheckBox UseAlphaNumbericValue
        {
            get { return Page.FindCamstarControl("ObjectChanges_UseAlphaNumbericValue") as CWC.CheckBox; }
        }

        protected virtual CWC.CheckBox UsePrefixBased
        {
            get { return Page.FindCamstarControl("ObjectChanges_UsePrefixBased") as CWC.CheckBox; }
        }

        protected virtual CWC.DropDownList OverflowOptionsDropDown
        {
            get { return Page.FindCamstarControl("OverflowOptions") as CWC.DropDownList; }
        }

        protected virtual JQDataGrid ExcludeValueGrid
        {
            get
            {
                return Page.FindCamstarControl("ExcludedValuesGrid") as JQDataGrid;
            }
        }

        #endregion             

        #region Public Functions

        public override void GetInputData(Service serviceData)
        {
            (serviceData as Camstar.WCF.ObjectStack.NumberingRuleMaint).ResetLastAssignedSequence = (LastAssignedSequence.IsChanged || LastAssignedSequenceHex.IsChanged || LastAssignedSequenceAlphaNumeric.IsChanged);
            base.GetInputData(serviceData);
        }

        #endregion

        #region Protected methods

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            UseHexadecimalValue.DataChanged += UseHexadecimalValue_DataChanged;
            UseAlphaNumbericValue.DataChanged += UseAlphaNumbericValue_DataChanged;
        }

        private void UseAlphaNumbericValue_DataChanged(object sender, EventArgs e)
        {
            if (UseAlphaNumbericValue.Data.ToString() == "True")
            {
                if (MaximumValue.TextControl.Text != null && MaximumValueAlphaNumeric.Data == null)
                {
                    MaximumValueAlphaNumeric.TextControl.Text = MaximumValue.TextControl.Text;
                    MaximumValue.ClearData();
                }

                if (LastAssignedSequence.TextControl.Text != null && LastAssignedSequenceAlphaNumeric.Data != null && LastAssignedSequenceAlphaNumeric.Data.ToString() == "0")
                {
                    LastAssignedSequenceAlphaNumeric.TextControl.Text = LastAssignedSequence.TextControl.Text;
                    LastAssignedSequence.Data = 0;
                }
            }
            else
            {
                if (MaximumValueAlphaNumeric.TextControl.Text != null && MaximumValue.Data == null)
                {
                    MaximumValue.TextControl.Text = MaximumValueAlphaNumeric.TextControl.Text;
                    MaximumValueAlphaNumeric.ClearData();
                }

                if (LastAssignedSequenceAlphaNumeric.TextControl.Text != null && LastAssignedSequence.Data != null && LastAssignedSequence.Data.ToString() == "0")
                {
                    LastAssignedSequence.TextControl.Text = LastAssignedSequenceAlphaNumeric.TextControl.Text;
                    LastAssignedSequenceAlphaNumeric.Data = 0;
                }
            }
        }

        protected virtual void UseHexadecimalValue_DataChanged(object sender, EventArgs e)
        {
            if (UseHexadecimalValue.Data.ToString() == "True")
            {
                if (MaximumValue.TextControl.Text != null && MaximumValueHex.Data == null)
                {
                    MaximumValueHex.TextControl.Text = MaximumValue.TextControl.Text;
                    MaximumValue.ClearData();
                }

                if (LastAssignedSequence.TextControl.Text != null && LastAssignedSequenceHex.Data != null && LastAssignedSequenceHex.Data.ToString() == "0")
                {
                    LastAssignedSequenceHex.TextControl.Text = LastAssignedSequence.TextControl.Text;
                    LastAssignedSequence.Data = 0;
                }
            }
            else
            {
                if (MaximumValueHex.TextControl.Text != null && MaximumValue.Data == null)
                {
                    MaximumValue.TextControl.Text = MaximumValueHex.TextControl.Text;
                    MaximumValueHex.ClearData();
                }

                if (LastAssignedSequenceHex.TextControl.Text != null && LastAssignedSequence.Data != null && LastAssignedSequence.Data.ToString() == "0")
                {
                    LastAssignedSequence.TextControl.Text = LastAssignedSequenceHex.TextControl.Text;
                    LastAssignedSequenceHex.Data = 0;
                }
            }
        }

        #endregion
    }
}
