// Copyright Siemens 2023  
using System;
using Camstar.WebPortal.FormsFramework.WebControls;


namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class UpdateSamplingLot: MatrixWebPart
    {
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (Qty != null)
                Qty.DataChanged += DataChanged;
            if (SampleRate != null)
                SampleRate.DataChanged += DataChanged;
        }

        protected virtual void DataChanged(object sender, EventArgs e)
        {
            if(SamplingLot == null || Qty == null || SampleRate == null)
                return;
            var submitAction = Page.ActionDispatcher.GetActionByName("Submit");
            if(submitAction == null)
                return;
            submitAction.IsDisabled = SamplingLot.Data == null || (Qty.Data == null && SampleRate.Data == null);

        }
        protected virtual NamedObject SamplingLot
        {
            get { return Page.FindCamstarControl("UpdateSamplingLot_SamplingLot") as NamedObject; }
        }
        protected virtual TextBox Qty
        {
            get { return Page.FindCamstarControl("UpdateSamplingLot_Qty") as TextBox; }
        }
        protected virtual TextBox SampleRate
        {
            get { return Page.FindCamstarControl("UpdateSamplingLot_SampleRate") as TextBox; }
        }
    }
}
