// Copyright Siemens 2022  
using System;
using System.Linq;
using System.Web;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using Camstar.WebPortal.Personalization;
using PERS = Camstar.WebPortal.Personalization;

namespace Camstar.WebPortal.WebPortlets.Modeling
{
    public class isUserDataCollection: UserDataCollection
    {
        #region Methods
        protected override void OnLoad(EventArgs e)
        {
            if (!(DataPointGrid.Settings as GridDataSettingsItemList).EditorSettings.SubClassMap[1].PopupPageAction.DataContractMap.Links.Any(x => x.SourceMember == "DefectReasonGroupDC"))
            {
                var ContractLinksList = (DataPointGrid.Settings as GridDataSettingsItemList).EditorSettings.SubClassMap[1].PopupPageAction.DataContractMap.Links.ToList();
                var DefectReasonGroupLink = new UIComponentDataContractLink() { SourceMember = "DefectReasonGroupDC", TargetMember = "DefectReasonGroupDC" };
                ContractLinksList.Add(DefectReasonGroupLink);
                var ContractLinksArray = ContractLinksList.ToArray();
                (DataPointGrid.Settings as GridDataSettingsItemList).EditorSettings.SubClassMap[1].PopupPageAction.DataContractMap.Links = ContractLinksArray;
            }

            base.OnLoad(e);
        }
        #endregion
    }
}
