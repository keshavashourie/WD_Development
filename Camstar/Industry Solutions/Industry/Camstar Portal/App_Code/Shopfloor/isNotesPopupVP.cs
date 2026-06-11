// © 2022 Siemens Product Lifecycle Management Software Inc.
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WebPortal.FormsFramework.Utilities;
using OM = Camstar.WCF.ObjectStack;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.Utilities;
using PERS = Camstar.WebPortal.Personalization;

namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class isNotesPopupVP : MatrixWebPart
    {
        #region Properties
        protected virtual CWC.TextBox InspectNote
        {
            get
            {
                return Page.FindCamstarControl("InspectNoteTextBox") as CWC.TextBox;
            }
        }
        protected virtual CWC.TextBox RepairNote
        {
            get
            {
                return Page.FindCamstarControl("RepairNoteTextBox") as CWC.TextBox;
            }
        }
        protected virtual CWC.TextBox DefectDataJson
        {
            get { return Page.FindCamstarControl("DefectDataJson") as CWC.TextBox; }
        }

        #endregion Properties

        #region Page Lifecycle Methods

        protected override void OnLoad(System.EventArgs e)
        {
            base.OnLoad(e);
            string json = DefectDataJson.TextControl.Text;

            if (!string.IsNullOrEmpty(json))
            {
                CurrentDefect[] existingDefects = CurrentDefect.DeserializeDefects(json);

                if (existingDefects.Length > 0)
                {
                    InspectNote.TextControl.Text = existingDefects[0].isInspectNote;
                    RepairNote.TextControl.Text = existingDefects[0].isRepairNotes;
                }
            }
        }

        #endregion Page Lifecycle Methods
    }
}