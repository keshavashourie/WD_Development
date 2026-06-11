// Copyright Siemens 2024  
using System;
using System.Collections.Generic;
using Camstar.WebPortal.FormsFramework.WebControls;
using OM = Camstar.WCF.ObjectStack;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using Camstar.WebPortal.Personalization;

/// <summary>
/// Used to maintain the Spec Maintenance page by adding additional functionality to the BOP Grid and Hours/Units fields on the page
/// </summary>
namespace Camstar.WebPortal.WebPortlets.Modeling
{
    public class SpecMaint : MatrixWebPart
    {
        #region Controls
        protected virtual FormsFramework.WebControls.RadioButton HoursPerUnitField
        {
            get { return Page.FindCamstarControl("HoursPerUnit") as FormsFramework.WebControls.RadioButton; }
        }
        protected virtual FormsFramework.WebControls.RadioButton UnitsPerHour
        {
            get { return Page.FindCamstarControl("UnitsPerHour") as FormsFramework.WebControls.RadioButton; }
        }

        protected virtual FormsFramework.WebControls.RadioButton TimePerBatch
        {
            get { return Page.FindCamstarControl("TimePerBatch") as FormsFramework.WebControls.RadioButton; }
        }        

        protected virtual FormsFramework.WebGridControls.JQDataGrid BOPGrid
        {
            get { return Page.FindCamstarControl("BOPGrid") as FormsFramework.WebGridControls.JQDataGrid; }
        }
        protected Duration HoursPerUnitControl
        {
            get { return Page.FindCamstarControl("SchedulingDetail_HoursPerUnit") as Duration; }
        }

        protected TextBox UnitsPerHourControl
        {
            get { return Page.FindCamstarControl("SchedulingDetail_UnitsPerHour") as TextBox; }
        }

        protected TextBox TimePerBatchControl
        {
            get { return Page.FindCamstarControl("SchedulingDetail_TimePerBatch") as TextBox; }
        }

		private Dictionary<string, string> portalHelpFile = new Dictionary<string, string> {
            { "SpecMaint", @"onlinehelpoutput/portalmodeling_Help/PortalModeling_CSH.htm#ModelingObjects/Defining_Specs_P.htm"},
        };
        #endregion

        #region Protected Functions
        protected override void OnPreRender(EventArgs e)
        {
			string helpUrl = string.Empty;

            if (portalHelpFile.ContainsKey(PrimaryServiceType)) 
                helpUrl = portalHelpFile[PrimaryServiceType];

            if (!string.IsNullOrEmpty(helpUrl))
            {
                Personalization.PageContent content = Page.Model.PublishedContent as PageContent;
                if (content != null)
                {
                    content.HelpFileURL = helpUrl;
                }
            }
			
            base.OnPreRender(e);

            OM.RecordSet result = null;
            var status = new OM.ResultStatus(string.Empty, true);

            status = (BOPGrid.GridContext as SelValGridContext).GetSelectionValuesData(out result, -1);
            if (status.IsSuccess)
                if (result.Rows != null && result.Rows.Length != 0)
                    BOPGrid.SetSelectionValues(result);
        }
        #endregion

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            HoursPerUnitControl.ReadOnly = !((bool)HoursPerUnitField.Data);
            UnitsPerHourControl.ReadOnly = !((bool)UnitsPerHour.Data);
            TimePerBatchControl.ReadOnly = !((bool)TimePerBatch.Data);
        }

        #region Public Functions
        public override void DisplayValues(OM.Service serviceData)
        {
            base.DisplayValues(serviceData);

            OM.SpecMaint data = serviceData as OM.SpecMaint;

            if (data == null || data.ObjectChanges == null || data.ObjectChanges.SchedulingDetail == null)
                return;

            HoursPerUnitField.Data = data.ObjectChanges.SchedulingDetail.RunRateOption == OM.RunRateEnum.HoursPerUnit;
            UnitsPerHour.Data = data.ObjectChanges.SchedulingDetail.RunRateOption == OM.RunRateEnum.UnitsPerHour;
            TimePerBatch.Data = data.ObjectChanges.SchedulingDetail.RunRateOption == OM.RunRateEnum.TimePerBatch;

            HoursPerUnitControl.ReadOnly = !(bool)HoursPerUnitField.Data;
            UnitsPerHourControl.ReadOnly = !(bool)UnitsPerHour.Data;
            TimePerBatchControl.ReadOnly = !(bool)TimePerBatch.Data;
        }

        public override void GetInputData(OM.Service serviceData)
        {
            base.GetInputData(serviceData);

            OM.SpecMaint data = serviceData as OM.SpecMaint;

            if (data != null && data.ObjectChanges != null)
            {
                if (data.ObjectChanges.SchedulingDetail == null)
                    data.ObjectChanges.SchedulingDetail = new OM.SpecSchedulingDetailChanges();

                if (!HoursPerUnitField.IsEmpty)
                    data.ObjectChanges.SchedulingDetail.RunRateOption = OM.RunRateEnum.HoursPerUnit;
                else if (!UnitsPerHour.IsEmpty)
                    data.ObjectChanges.SchedulingDetail.RunRateOption = OM.RunRateEnum.UnitsPerHour;
                else if (!TimePerBatch.IsEmpty)
                    data.ObjectChanges.SchedulingDetail.RunRateOption = OM.RunRateEnum.TimePerBatch;                    
            }
        }

        public override void RequestValues(OM.Info serviceInfo, OM.Service serviceData)
        {
            base.RequestValues(serviceInfo, serviceData);

            OM.SpecMaint_Info info = serviceInfo as OM.SpecMaint_Info;
            if (info.ObjectChanges != null && info.ObjectChanges.SchedulingDetail != null)
                info.ObjectChanges.SchedulingDetail.RunRateOption = new OM.Info(true);
        }
        #endregion

    }
}
