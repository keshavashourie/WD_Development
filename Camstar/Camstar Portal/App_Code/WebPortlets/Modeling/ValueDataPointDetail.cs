// Copyright Siemens 2023  
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Web.UI;
using Camstar.WCF.ObjectStack;
using Camstar.WCF.Services;

using Camstar.WebPortal.PortalFramework;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WebPortal.Utilities;
using Camstar.WebPortal.WCFUtilities;
using Camstar.WebPortal.WebPortlets;
using Camstar.WebPortal.Personalization;

using CamstarPortal.WebControls;
using System.Globalization;

namespace Camstar.WebPortal.WebPortlets.Modeling
{
    public class ValueDataPointDetail : MatrixWebPart
    {
        protected string BooleanTrueState
        {
            get { return Page.SessionVariables["BooleanTrueState"] as string; }
            set { Page.SessionVariables["BooleanTrueState"] = value; }
        }

        protected string BooleanFalseState
        {
            get { return Page.SessionVariables["BooleanFalseState"] as string; }
            set { Page.SessionVariables["BooleanFalseState"] = value; }
        }

        protected virtual CheckBox MapToUserAttribute
        { get { return Page.FindCamstarControl("MapToUserAttribute") as CheckBox; } }
        
        protected virtual TextBox AttributeName
        { get { return Page.FindCamstarControl("AttributeName") as TextBox; } }

        protected virtual TextBox DataPointName
        { get { return Page.FindCamstarControl("DataPointName") as TextBox; } }

        protected virtual DropDownList Value_DataType
        { get { return Page.FindCamstarControl("Value_DataType") as DropDownList; } }

        protected virtual TextBox BooleanTrue
        { get { return Page.FindCamstarControl("Boolean_True") as TextBox; } }

        protected virtual TextBox BooleanFalse
        { get { return Page.FindCamstarControl("Boolean_False") as TextBox; } }

        protected virtual TextBox UpperLimit
        { get { return Page.FindCamstarControl("UpperLimit") as TextBox; } }

        protected virtual TextBox LowerLimit
        { get { return Page.FindCamstarControl("LowerLimit") as TextBox; } }

        protected virtual TextBox DecimalScale
        { get { return Page.FindCamstarControl("DecimalScale") as TextBox; } }

        protected virtual DropDownList RoundingRule
        { get { return Page.FindCamstarControl("DataPoints_RoundingRule") as DropDownList; } }


        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            AttributeName.Enabled = (bool)MapToUserAttribute.Data;
            MapToUserAttribute.DataChanged += MapToUserAttribute_DataChanged;

            if (BooleanTrue != null && BooleanFalse != null)
            {
                if (BooleanTrue.Data != null)
                    BooleanTrueState = BooleanTrue.Data.ToString();
                if (BooleanFalse.Data != null)
                    BooleanFalseState = BooleanFalse.Data.ToString();
            }

            Value_DataType.DataChanged += Value_DataType_DataChanged;

        }

        private ResultStatus CheckScale(string limitArea, string controlName)
        {
            decimal num;
            if (!DecimalScale.Visible)
            {
                DecimalScale.ClearData();
                RoundingRule.ClearData();
                return new ResultStatus("", true);
            }

            limitArea = limitArea?.Replace(',', '.');

            bool isNum = Decimal.TryParse(limitArea, NumberStyles.Number, CultureInfo.CurrentUICulture, out num);

            if (DecimalScale.Data == null)
                return new ResultStatus("", true);

            if ((int)DecimalScale.Data < 0)
                return new ResultStatus(FrameworkManagerUtil.GetLabelCache(HttpContext.Current.Session).GetLabelByName("Lbl_ScaleNegativeValidation")?.Value, false);

            
            if (!isNum)
                return new ResultStatus("", true);
            
            var type = (DataTypeEnum)Enum.Parse(typeof(DataTypeEnum), Value_DataType.Data.ToString());
            if (type != DataTypeEnum.Decimal && type != DataTypeEnum.Fixed && type != DataTypeEnum.Float || limitArea == null)
                return new ResultStatus("", true);
            var found = limitArea.IndexOf('.');
            limitArea = found == -1 ? "" : limitArea.Substring(found + 1);
            if (limitArea.Length != (int)DecimalScale.Data)
            {
                
                return new ResultStatus(String.Format(FrameworkManagerUtil.GetLabelCache(HttpContext.Current.Session).GetLabelByName("Lbl_ScaleLimitValidation")?.Value, controlName), false);
            }                
            else
                return new ResultStatus("", true);
        }

        protected virtual void MapToUserAttribute_DataChanged(object sender, EventArgs e)
        {

            AttributeName.Enabled = (bool)MapToUserAttribute.Data;

            if (!AttributeName.Enabled)
                AttributeName.ClearData();

            if (AttributeName.Enabled && AttributeName.Data == null)
                AttributeName.Data = DataPointName.Data;
        }

        protected virtual void Value_DataType_DataChanged(object sender, EventArgs e)
        {
            var type = (DataTypeEnum)Enum.Parse(typeof(DataTypeEnum), Value_DataType.Data.ToString());
            if (Page.DataContract.GetValueByName("DecimalScaleDCDM") != null && (type == DataTypeEnum.Decimal || type == DataTypeEnum.Fixed || type == DataTypeEnum.Float))
            {
                bool visible = (bool)Page.DataContract.GetValueByName("DecimalScaleDCDM");
                DecimalScale.Visible = visible;
                RoundingRule.Visible = visible;
            }
            if (type == DataTypeEnum.Boolean || type == DataTypeEnum.Integer || type == DataTypeEnum.Object || type == DataTypeEnum.String || type == DataTypeEnum.Timestamp)
            {
                DecimalScale.Visible = false;
                RoundingRule.Visible = false;
            }
            if (!DecimalScale.Visible)
            {
                DecimalScale.ClearData();
                RoundingRule.ClearData();
            }
            if (BooleanTrue !=null && BooleanFalse != null)
            { 
                if (Value_DataType.GetEnumText(Value_DataType.SelectionData) == DataTypeEnum.Boolean.ToString())
                {
                    BooleanTrue.Visible = true;
                    BooleanFalse.Visible = true;
                    BooleanTrue.Data = BooleanTrueState;
                    BooleanFalse.Data = BooleanFalseState;
                }
                else
                {
                    BooleanTrue.Visible = false;
                    BooleanFalse.Visible = false;
                    BooleanTrue.Data = null;
                    BooleanFalse.Data = null;
                }
            }
        }

        public override void WebPartCustomAction(object sender, CustomActionEventArgs e)
        {
            var upperStatus = CheckScale(UpperLimit.Data as string, "Upper");
            if (!upperStatus.IsSuccess)
            {
                e.Result = upperStatus;
                return;
            }
            var lowerStatus = CheckScale(LowerLimit.Data as string, "Lower");
            if (!lowerStatus.IsSuccess)
            {
                e.Result = lowerStatus;
                return;
            }
            Page.CloseFloatingFrame(sender, e);
        }
    }
}
