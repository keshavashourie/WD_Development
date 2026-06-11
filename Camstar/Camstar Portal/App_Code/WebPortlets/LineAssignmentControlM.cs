// Copyright Siemens 2023  

using System.Web.UI;
using Camstar.WebPortal.Constants;
using Camstar.WebPortal.PortalFramework;
using Helpers;

namespace Camstar.WebPortal.WebPortlets
{
    public class LineAssignmentControlM : LineAssignmentControl
    {
        protected override IMatrixBuilder MatrixBuilder
        {
            get { return _matrixBuilder; }
        }
        private IMatrixBuilder _matrixBuilder = new DivLayoutBuilder();
    }
}
