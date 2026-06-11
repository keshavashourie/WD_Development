using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using OM = Camstar.WCF.ObjectStack;
using Newtonsoft.Json;
using Camstar.WCF.Services;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.Utilities;
using cbArgs = Camstar.WebPortal.WebPortlets.CommandBarCallBackArgs;

namespace Camstar.WebPortal.Helpers
{
    /// <summary>
    /// Summary description for ContainerStatusInquiry
    /// </summary>
    public class CommandBarHelper
    {
        public CommandBarHelper()
        {
        }

        public virtual PanelNameValueData GetPanelData(cbArgs _callbackArgs)
        {
            if (_callbackArgs != null)
            {
                return new PanelNameValueData {
                    { "__clientType", _callbackArgs.clientType },
                    { "__serverType", _callbackArgs.serverType },
                    { "__fun", _callbackArgs.fun }
                };
            }
            return null;
        }

        //private LabelList getLabelList(List<contStatusField> flds)
        //{
        //    const string fexPref = "ContainerTxn.CurrentContainerStatus.";
        //    var ll = new LabelList();
        //    foreach (var f in flds)
        //    {
        //        var metadata = OM.Environment.GetMetadata(fexPref + f.name);
        //        if (metadata != null)
        //            f.labelId = metadata.LabelID;
        //    }
        //    var lbs =
        //        from f in flds
        //        where f.labelId != null
        //        select new OM.Label(f.labelId.Value);

        //    return new LabelList(lbs.ToArray());
        //}
        //private string __clientTypeName;
    }

    public class PanelNameValueData : Dictionary<string, object>
    {
    }
}