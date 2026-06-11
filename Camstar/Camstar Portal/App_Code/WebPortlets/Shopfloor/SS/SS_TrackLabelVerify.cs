/* Copyright 2019 Siemens */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Text;
using System.Web;
using System.Web.UI;

using Camstar.WCF.ObjectStack;
using Camstar.WCF.Services;
using Camstar.WebPortal.Utilities;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using OM = Camstar.WCF.ObjectStack;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.Constants;
using Camstar.WebPortal.WCFUtilities;
using Camstar.WebPortal.Personalization;
using SEMI.AppCode;

/// <summary>
/// Summary description for SS_TrackLabelVerify
/// </summary>

namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class SS_TrackLabelVerify : MatrixWebPart
    {
        protected CWC.TextBox _txtComputerName { get { return Page.FindCamstarControl("ss_TrackLabelVerify_ComputerName") as CWC.TextBox; } }
        protected CWC.TextBox _txtChildLabel { get { return Page.FindCamstarControl("ss_TrackLabelVerify_ScanChildLabel") as CWC.TextBox; } }
        protected JQDataGrid _gridChildLabels { get { return Page.FindCamstarControl("ss_TrackLabelVerify_ChildLabelID") as JQDataGrid; } }

        //---------------------------------------------------
        // On Load Event
        //---------------------------------------------------
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            _txtChildLabel.DataChanged += new EventHandler(_txtChildLabel_DataChanged);
            _txtComputerName.TextControl.Text = SEMI.AppCode.UIUtility.GetComputerName(this);
        }

        //---------------------------------------------------
        // Lot ID Data Changed
        //---------------------------------------------------
        void _txtChildLabel_DataChanged(object sender, EventArgs e)
        {
            try
            {
                if (_txtChildLabel.Data != null)
                    AddChildLabelToGrid(_txtChildLabel.Data.ToString());
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        //---------------------------------------------------
        // Add Child Label to the Grid
        //---------------------------------------------------
        private void AddChildLabelToGrid(string sChildLabel)
        {
            try
            {
                // set grid details to list
                Primitive<string>[] objChildLabels = (_gridChildLabels.GridContext as BoundContext).Data as Primitive<string>[];
                List<Primitive<string>> objChildLabel = new List<Primitive<string>>();

                int selectedIndex = 0;
                if (objChildLabels != null)
                {
                    objChildLabel = objChildLabels.OfType<Primitive<string>>().ToList();

                    // exit if entry already exist
                    foreach (Primitive<string> oTool in objChildLabel)
                    {
                        if (oTool.Value == sChildLabel)
                        {
                            _txtChildLabel.DataChanged -= _txtChildLabel_DataChanged;
                            _txtChildLabel.Data = "";
                            return;
                        }
                        selectedIndex += 1;
                    }
                }

                // add new tool to the Tool grid
                Primitive<string> objNewRow = new Primitive<string>();
                objNewRow.Value = _txtChildLabel.Data.ToString();
                objChildLabel.Insert(selectedIndex, objNewRow);
                (_gridChildLabels.GridContext as BoundContext).Data = objChildLabel.ToArray();
                _gridChildLabels.BoundContext.LoadData();
                CamstarWebControl.SetRenderToClient(_gridChildLabels);

                // set scan tools field to empty
                _txtChildLabel.DataChanged -= _txtChildLabel_DataChanged;
                _txtChildLabel.Data = "";
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
    }
}



