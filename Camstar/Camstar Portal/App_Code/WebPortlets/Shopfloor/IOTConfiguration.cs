// Copyright Siemens 2023  
using Camstar.WebPortal.Helpers;
using System;
using System.Collections.Generic;

namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class IOTConfiguration : SwacPageBase
    {
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

        }

        protected override List<string> GetSwacComponentInitializationArgs(string webPartName, string buttonName)
        {
            string accessPermission = SrcIntegrationHelper.GetUserAccessPermission();
            string bearerToken = SrcIntegrationHelper.BearerToken;

            List<string> args = new List<string>
            {
                accessPermission,
                bearerToken
            };

            return args;
        }
    }
}
