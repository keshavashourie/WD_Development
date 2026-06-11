//Copyright Siemens 2022
using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WCF.ObjectStack;
using Camstar.WebPortal.FormsFramework.Utilities;
using Newtonsoft.Json;
using Camstar.WebPortal.Constants;
using Camstar.WCF.Services;
using System.Web;
using System.Threading.Tasks;
using System.Net.Http;
using Camstar.WebPortal.Personalization;

namespace Camstar.WebPortal.WebPortlets.Modeling
{

    /// <summary>
    /// TODO: Add a Summary description for this Camstar Web Part
    /// </summary>
    public class scsFactoryMaint : FactoryMaint
    {
        #region Controls

        CWC.TextBox _stepsToProcessField { get { return Page.FindCamstarControl("ObjectChanges_scsMaxScheduleStepsToProcess") as CWC.TextBox; } }
        CWC.CheckBox _isPreactorEnabled { get { return Page.FindCamstarControl("ObjectChanges_isPreactorEnabled") as CWC.CheckBox; } }
        #endregion

        #region Protected Functions
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            _stepsToProcessField.DataChanged += new EventHandler(StepsToProcessField_Change);
 
        }

        private void StepsToProcessField_Change(object sender, EventArgs e)
        {
            var labelCache = FrameworkManagerUtil.GetLabelCache(System.Web.HttpContext.Current.Session);


            if (_stepsToProcessField.Data != null)
            {
                var _stepsToProcessValues = _stepsToProcessField.Data.ToString();
                int _MaxProcessStepsCount = int.Parse(_stepsToProcessValues);
				if(_isPreactorEnabled != null && _isPreactorEnabled.IsChecked == true)
				{
					if ( _MaxProcessStepsCount > 40)
						{
							var WarningMaxScheduleSteps = String.Format(labelCache.GetLabelByName("scsCompletionMsgMaxScheduleSteps").Value);
							Page.DisplayWarning(WarningMaxScheduleSteps);
						}
						
						else
						{
							if(_MaxProcessStepsCount <= 0 || _stepsToProcessValues == null)
							{
							var WariningEmptyMaxScheduleSteps = String.Format(labelCache.GetLabelByName("scsEmptyFieldMsgMaxScheduleSteps").Value);
                            Page.DisplayWarning(WariningEmptyMaxScheduleSteps);	
							}
						}
					
				}
               
				
            }
           


        }
        #endregion
    }

}