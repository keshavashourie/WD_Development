// Copyright Siemens 2023  

using System;
using Camstar.WCF.ObjectStack;

namespace Camstar.WebPortal.WebPortlets.ChangeManagement
{
    public class MoveNonStdChangePkg : MatrixWebPart
    {
        public MoveNonStdChangePkg()
        { }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
        }

        public override void PostExecute(ResultStatus status, Service serviceData)
        {
            base.PostExecute(status, serviceData);
            if (status != null && status.IsSuccess)
        {
                Page.ClearValues(serviceData);
        }        
        }
    }
}
