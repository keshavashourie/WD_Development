using Newtonsoft.Json;
using Siemens.PLM.SDK.Core.Client;
using Siemens.PLM.SDK.Lifecycle.Design.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;

namespace Camstar.WebPortal.WebPortlets.XceleratorShare
{
    public class Utils
    {
        public static void GetMCADExtensions()
        {
            try
            {
                if (MCADExtensions.elements is null)
                {
                    string filePath = HttpContext.Current.Server.MapPath(@"App_Code\WebPortlets\XceleratorShare\MCADExt.json");
                    using (StreamReader r = new StreamReader(filePath))
                    {
                        var json = r.ReadToEnd();
                        MCADExt myDeserializedClass = JsonConvert.DeserializeObject<MCADExt>(json);
                        MCADExtensions.elements = myDeserializedClass.elements;
                    }
                }
            }
            catch (Exception exception)
            {
                HttpContext.Current.Trace.Write(MethodBase.GetCurrentMethod().Name + "  " + exception.ToString());

            }
        }

    }
}






