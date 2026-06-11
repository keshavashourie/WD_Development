// Copyright Siemens 2023  

using Camstar.WebPortal.FormsFramework.WebGridControls;
using Camstar.WebPortal.Personalization;
using Camstar.WebPortal.PortalFramework;
using Camstar.WebPortal.FormsFramework;
using Helpers;

namespace Camstar.WebPortal.WebPortlets
{
    public class UserFieldsWebPartM : UserFieldsWebPart
    {
        protected override IMatrixBuilder MatrixBuilder
        {
            get { return _matrixBuilder; }
        }

        IMatrixBuilder _matrixBuilder = new DivLayoutBuilder();
    }
}
