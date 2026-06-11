// Copyright Siemens 2019  
using System;
using System.Linq;
using System.Data;
using Camstar.WCF.ObjectStack;
using Camstar.WCF.Services;
using Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using Camstar.WebPortal.FormsFramework.Utilities;
using System.Collections.Generic;

namespace Camstar.WebPortal.WebPortlets.ChangeManagement
{
    public class scsPackageDetailWP : PackageDetailWP
    {
        Dictionary<int, string[]> matrixList = new Dictionary<int, string[]>();

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            RetrieveAllMatrixCDO();
        }

        protected override void IncludedInstances_SnapCompleted(DataTable table)
        {
            foreach (DataRow row in table.Rows)
            {
                row["IsImported"] = Convert.ToInt32(IsImportedPackageChk.IsChecked);
                row["IsModelingObjectExist"] = 0;
                if (new MaintCDOCache().MaintCdoData.Any(x => x.CDODefID == row["CDOTypeID"] as string))
                {

                    row["IsModelingObjectExist"] = 1;

                    var matrixList = Page.Session["MatrixCDOID"] as Dictionary<int, string[]>;

                    foreach (var MatrixCDO in matrixList)
                    {

                        if (MatrixCDO.Value[0].Equals(row["CDOTypeID"].ToString()))
                        {
                            row["IsModelingObjectExist"] = 0;
                        }
                    }
                }
            }
        }

        private void RetrieveAllMatrixCDO()
        {
            var fs = FrameworkManagerUtil.GetFrameworkSession();
            QueryService oService = new QueryService(fs.CurrentUserProfile);
            QueryOptions oOptions = new QueryOptions();
            RecordSet oData = new RecordSet();
            QueryParameters oParameters = new QueryParameters();

            ResultStatus oResult = oService.Execute("scsGetMatrixCDOName", oParameters, oOptions, out oData);
            if (oResult.IsSuccess)
            {
                if (oData.Rows != null)
                {
                    for (int i = 0; i < oData.Rows.Length; i++)
                    { matrixList.Add(i, oData.Rows[i].Values); }
                    Page.Session["MatrixCDOID"] = matrixList;
                }
            }
        }
    } 
}
