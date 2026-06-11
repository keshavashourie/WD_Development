// Copyright Siemens 2023  
using System;
using System.Linq;
using System.Collections.Generic;
using System.Configuration;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;

using Camstar.WCF.ObjectStack;
using Camstar.WebPortal.Utilities;
using Camstar.WebPortal.Constants;
using Camstar.WebPortal.WCFUtilities;
using CWF = Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.PortalFramework;
using Camstar.WebPortal.WebPortlets;
using Camstar.WebPortal.FormsFramework.Utilities;
using CamstarWebControls = Camstar.WebPortal.FormsFramework.WebControls;
using CamstarPortal.WebControls;
using CGC = Camstar.WebPortal.FormsFramework.WebGridControls;
using Camstar.WCF.Services;

using Camstar.WebPortal.FormsFramework;

namespace Camstar.Portal
{
    ///// <summary>
    ///// Represents mode of a page
    ///// </summary>
    public enum EquivalencyPageMode { View, Edit }

    ///// <summary>
    ///// An instance of the class is stored inside Session to be used between redirections.
    ///// </summary>
    [Serializable()]
    public class RecordViewPageState : CallStackStateBase
    {
        public virtual int SelectedTabIndex { get; set; }
    }

    [Serializable]
    public class RecordViewContext : QualityObjectContext
    {
        public virtual QualityStatusEnum QualityObjectStatus { get; set; }
        public virtual ApprovalStatusEnum QualityObjectApprovalStatus { get; set; }
        public virtual EquivalencyPageMode PageMode { get; set; }
        public virtual string RegulatoryAgency { get; set; }
        public virtual int? SelectedTabIndex { get; set; }
        public virtual int AvailableActions { get; set; }
        public virtual bool ReadOnly { get; set; }
        public virtual bool SelectProcessModelTab { get; set; }

        public virtual bool this[QualityRecordActionsEnum action]
        {
            get { return (AvailableActions & (int)action) > 0; }
        }
    }
}
