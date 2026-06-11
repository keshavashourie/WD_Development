using System;
using System.Collections.Generic;
using System.Linq;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using Camstar.WebPortal.FormsFramework.WebControls;
using System.Data;
using Camstar.WCF.ObjectStack;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.WCFUtilities;
using Camstar.WCF.Services;
using Camstar.WebPortal.Utilities;
using System.Text.RegularExpressions;
/// <summary>
/// Summary description for MultiContainerRename
/// </summary>
namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class MultiContainerRename : MatrixWebPart
    {
        #region Properties
        protected virtual ContainerListGrid ContainerTextBox
        {
            get
            {
                return Page.FindCamstarControl("ContainerStatus_ContainerName") as ContainerListGrid;
            }
        }

        protected virtual TextBox NewContainerTextBox
        {
            get
            {
                return Page.FindCamstarControl("MultiContainer_NewContainer") as TextBox;
            }
        }

        protected virtual JQDataGrid ContainerGrid
        {
            get
            {
                return Page.FindCamstarControl("MultiContainer_Containers") as JQDataGrid;
            }
        }
        protected virtual JQDataGrid SubEntityGrid
        {
            get
            {
                return Page.FindCamstarControl("SubEntityGrid") as JQDataGrid;
            }
        }

        protected virtual Button AddToGridButton
        {
            get
            {
                return Page.FindCamstarControl("Add_item_to_grid") as Button;
            }
        }

        protected List<GridRow> AllContainers
        {
            get
            {
                var containers = Page.SessionVariables["AllContainers"] ?? new List<GridRow>();
                return containers as List<GridRow>;
            }
            set
            {
                if (!Page.SessionVariables.Contains("AllContainers"))
                    Page.SessionVariables.Add("AllContainers", value);
                else
                    Page.SessionVariables["AllContainers"] = value;
            }
        }

        protected bool IsRowDeleted
        {
            get
            {
                var containers = Page.SessionVariables["IsRowDeleted"] ?? false;
                return (bool)containers;
            }
            set
            {
                if (!Page.SessionVariables.Contains("IsRowDeleted"))
                    Page.SessionVariables.Add("IsRowDeleted", value);
                else
                    Page.SessionVariables["IsRowDeleted"] = value;
            }
        }
        #endregion

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            AddToGridButton.Enabled = false;
            AddToGridButton.Click += AddToGridButton_Click;
            ContainerGrid.GridContext.RowDeleting += GridContext_RowDeleting;
            ContainerGrid.GridContext.RowDeleted += GridContext_RowDeleted;
            ContainerTextBox.DataChanged += TextBoxes_TextChanged;
            NewContainerTextBox.TextChanged += TextBoxes_TextChanged;
        }

        private void TextBoxes_TextChanged(object sender, EventArgs e)
        {
            EnableDisableButton();
        }

        protected override void OnPreRender(EventArgs e)
        {
            if (IsRowDeleted)
            {
                ContainerGrid.GridContext.RenderToClient = true;
                IsRowDeleted = false;
            }

            SetAllContainersData();

            base.OnPreRender(e);
        }

        private void EnableDisableButton()
        {
            if (ContainerTextBox.Data == null|| NewContainerTextBox.Data == null)
                AddToGridButton.Enabled = false;
            else
                AddToGridButton.Enabled = true;
        }

        public override void GetInputData(Service serviceData)
        {
            base.GetInputData(serviceData);

            (serviceData as RenameContainers).RenameContainersInfo = AllContainers.Select(x => new RenameContainerInfo
            {
                Container = new ContainerRef { Name = x.Container.ToString() },
                ServiceDetail = new ContainerRenameDetail { NewName = x.NewName.ToString() }
            }).ToArray();
        }

        public override bool PreExecute(Info serviceInfo, Service serviceData)
        {
            if (AllContainers.Count == 0)
            {
                DisplayMessage(new ResultStatus(FrameworkManagerUtil.GetLabelValue("Lbl_RenameContainersListIsEmpty"), false));
                return false;
            }
            return base.PreExecute(serviceInfo, serviceData);
        }

        private ResponseData GridContext_RowDeleted(object sender, JQGridEventArgs args)
        {
            args.Cancel = true;
            ContainerGrid.Data = AllContainers.ToArray();
            IsRowDeleted = true;
            return args.Response;
        }

        private ResponseData GridContext_RowDeleting(object sender, JQGridEventArgs args)
        {
            AllContainers = AllContainers.Where(x => x != args.Context.SelectedItem).ToList();
            ContainerGrid.Data = null;
            return args.Response;
        }

        private void SetAllContainersData()
        {
            if (ContainerGrid.Data == null && AllContainers.Count > 0)
                AllContainers.Clear();

            else if (ContainerGrid.Data as List<GridRow> != null
                && (ContainerGrid.Data as List<GridRow>).Count != AllContainers.Count())
                AllContainers = ContainerGrid.Data as List<GridRow>;
        }

        private void AddToGridButton_Click(object sender, EventArgs e)
        {
            if (!ContainerTextBox.IsEmpty && !NewContainerTextBox.IsEmpty)
            {
                var obj = CollectControls();
                var (message, IsValidate) = ContainerValidate(obj);

                if (IsValidate)
                {
                    var allContainers = AllContainers;
                    allContainers.Add(obj);
                    ContainerGrid.Data = allContainers.ToArray();
                    AllContainers = allContainers;
                    ClearControlls();
                }
                else
                    DisplayMessage(new ResultStatus(message, IsValidate));
            }
        }

        private void ClearControlls()
        {
            ContainerTextBox.ClearData();
            NewContainerTextBox.ClearData();
        }

        private GridRow CollectControls() => new GridRow
        {
            Container = new Primitive<string>(ContainerTextBox.Data.ToString()),
            NewName = new Primitive<string>(NewContainerTextBox.Data.ToString())
        };

        private (string, bool) ContainerValidate(GridRow container)
        {
            var containerName = container.Container.Value;
            var newContainerName = container.NewName.Value;
            if (!IsContainerExist(containerName))
                return (FrameworkManagerUtil.GetLabelValue("Err_ComponentRemove_NonexistentContainer"), false);

            else if (IsContainerExist(newContainerName))
                return (FrameworkManagerUtil.GetLabelValue("DuplicateContainerRef").Replace("#ErrorMsg.Name2", FrameworkManagerUtil.GetLabelValue("CSICDOName_Container"))
                                                                                .Replace("#ErrorMsg.Name", newContainerName), false);

            else if (AllContainers.Any(n => n.NewName == newContainerName || n.Container == containerName))
                return (FrameworkManagerUtil.GetLabelValue("ListIsNotUnique").Replace("#ErrorMsg.FieldLabel", FrameworkManagerUtil.GetLabelValue("Lbl_ContainersToRename")), false);

            return ("Success", true);
        }

        protected bool IsContainerExist(string containerName)
        {
            var fs = FrameworkManagerUtil.GetFrameworkSession();
            var creator = new WSDataCreator();
            var service = creator.CreateService(nameof(ContainerRename), fs.CurrentUserProfile) as IShopFloorBase;

            var request = creator.CreateObject(nameof(ContainerRename) + "_Request") as Request;
            var result = creator.CreateObject(nameof(ContainerRename) + "_Result") as Result;
            var data = Page.CreateServiceData(nameof(ContainerRename));
            var info = Page.CreateServiceInfo(nameof(ContainerRename));
            var cdoObj = new WCFObject(data);
            var infoObj = new WCFObject(info);

            request.Info = info;

            cdoObj.SetValue(".Container", new ContainerRef(containerName));
            infoObj.SetValue(".Container", FieldInfoUtil.RequestValue());

            return service.Load(data, request, out result).IsSuccess;
        }
    }

    public class GridRow
    {
        public Primitive<string> Container { get; set; }
        public Primitive<string> NewName { get; set; }
    }
}