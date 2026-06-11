// Copyright Siemens 2022
using Camstar.WCF.ObjectStack;
using Camstar.WCF.Services;
using Camstar.WebPortal.FormsFramework.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Camstar.WebPortal.WebPortlets.Modeling
{
    /// <summary>
    /// Code behind for the Factory Hierarchy Model (FHM) Import page. 
    /// TreeData and TreeDefnition will be passed from FH Page to this popup page through Data Contract
    /// </summary>
    public class isFactoryHierarchyImport : FactoryHierarchyImport
    {
 
        #region Methods

        protected List<string> GetAllInvLocationName()
        {
            var svcParams = new isInventoryLocationMaint();

            var request = new isInventoryLocationMaint_Request()
            {
                Info = new isInventoryLocationMaint_Info
                {
                    ObjectListInquiry = new Info(false, true)
                }
            };

            var result = new isInventoryLocationMaint_Result();
            FrameworkSession session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
            var service = new isInventoryLocationMaintService(session.CurrentUserProfile);
            ResultStatus resultStatus = service.GetEnvironment(svcParams, request, out result);

            if (resultStatus.IsSuccess && result.Environment.ObjectListInquiry.SelectionValues.Rows != null)
                return result.Environment.ObjectListInquiry.SelectionValues.Rows.Select(s => s.Values[0]).ToList();
            else
                return new List<string>();
        }

        /// <summary>
        /// Modify Tree Data by adding isCheck and checkboxDisable property
        /// [isCheck] - Flag to indicate the creation of checkbox in tree node and its status when created 
        ///             (TRUE-Checkbox checked, FALSE-Checkbox unchecked oncreate)
        /// [checkboxDisabled] - Flag to indicate whether the checkbox is enabled/disabled
        /// </summary>
        /// <returns>A string representation of the import data in JSON format</returns>
        protected override string ModifyImportData(string importData)
        {
            List<string> allResourceName = GetAllResourceName();
            List<string> allInventoryName = GetAllInvLocationName();
            dynamic importTreeData = JsonConvert.DeserializeObject<dynamic>(importData);

            // If import data already exist, checkbox is disabled
            dynamic selectedArea = importTreeData.Areas[0];
            if (!selectedArea.ContainsKey("Cells"))
                return importData;

            foreach (dynamic cell in selectedArea.Cells)
            {
                bool isExist = allResourceName.Contains(cell.Name.Value);

                cell.checkboxDisable = isExist;
                cell.isChecked = true;

                if (cell.ContainsKey("Equipment"))
                {
                    List<dynamic> removeEqpList = new List<dynamic>();
                    foreach (dynamic equipment in cell.Equipment)
                    {
                        isExist = allResourceName.Contains(equipment.Name.Value);
                        equipment.checkboxDisable = isExist;
                        equipment.isChecked = true;

                        // Change Equipment to Inventory Location if machine type = Storage
                        if (equipment.MachineType == "Storage")
                        {
                            if (!cell.ContainsKey("InventoryLocations"))
                                cell.InventoryLocations = new JArray();
                            equipment.checkboxDisable = allInventoryName.Contains(equipment.Name.Value);
                            cell.InventoryLocations.Add(equipment);
                            removeEqpList.Add(equipment);
                        }
                    }

                    // Remove equipment that have been added to inventory location
                    foreach (dynamic equipment in removeEqpList)
                        cell.Equipment.Remove(equipment);
                }
            }

            return JsonConvert.SerializeObject(importTreeData);
        }

        #endregion
    }

}