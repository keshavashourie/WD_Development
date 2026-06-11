// Copyright Siemens 2023
using Camstar.WCF.Services;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.FormsFramework.WebControls;
using System;
using System.Collections.Generic;
using OM = Camstar.WCF.ObjectStack;
using System.Data;
using Camstar.WebPortal.FormsFramework.WebControls.PickLists;

namespace Camstar.WebPortal.WebPortlets
{
    public class SPCTesterPage : MatrixWebPart
    {
        protected virtual ContainerList ContainerForChart { get { return Page.FindCamstarControl("ContainerForChart") as ContainerList; } }
        protected virtual NamedObject SPCChart { get { return Page.FindCamstarControl("SPCChart") as NamedObject; } }
        protected virtual NamedObject SPCChartGroup { get { return Page.FindCamstarControl("SPCChartGroup") as NamedObject; } }
        protected virtual FormsFramework.WebControls.RevisionedObject DataCollection { get { return Page.FindCamstarControl("DataCollection") as FormsFramework.WebControls.RevisionedObject; } }
        static List<OM.RevisionedObjectRef> DCList;
        static List<OM.NamedObjectRef> GroupsList;
        static List<OM.NamedObjectRef> ChartsList;
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            ContainerForChart.DataChanged += ContainerForChart_DataChanged;
            DataCollection.PickListPanelControl.PostProcessData += DCPickListPanelControl_PostProcessData;
            SPCChart.PickListPanelControl.PostProcessData += SPCChartPickListPanelControl_PostProcessData;
            SPCChartGroup.PickListPanelControl.PostProcessData += SPCChartGroupPickListPanelControl_PostProcessData;
        }

        private void SPCChartGroupPickListPanelControl_PostProcessData(object sender, DataRequestEventArgs e)
        {
            if (GroupsList != null)
            {
                var rs = e.Data as DataTable;
                List<DataRow> RowsToDelete = new List<DataRow>();
                foreach (var r in rs.Rows)
                {
                    var row = r as DataRow;
                    if (row != null && !GroupsList.Exists(m => m.Name == row["Name"].ToString()))
                    {
                        RowsToDelete.Add(row);
                    }
                }

                foreach (var dr in RowsToDelete)
                {
                    rs.Rows.Remove(dr);
                }
                e.Data = rs;
            }
        }

        private void SPCChartPickListPanelControl_PostProcessData(object sender, DataRequestEventArgs e)
        {
            if (ChartsList != null)
            {
                var rs = e.Data as DataTable;
                List<DataRow> RowsToDelete = new List<DataRow>();
                foreach (var r in rs.Rows)
                {
                    var row = r as DataRow;
                    if (row != null && !ChartsList.Exists(m => m.Name == row["Name"].ToString()))
                    {
                        RowsToDelete.Add(row);
                    }
                }

                foreach (var dr in RowsToDelete)
                {
                    rs.Rows.Remove(dr);
                }
                e.Data = rs;
            }
        }

        private void DCPickListPanelControl_PostProcessData(object sender, FormsFramework.WebControls.PickLists.DataRequestEventArgs e)
        {
            if (DCList != null)
            {
                var rs = e.Data as DataTable;
                List<DataRow> RowsToDelete = new List<DataRow>();
                foreach (var r in rs.Rows)
                {
                    var row = r as DataRow;
                    if (row != null && !DCList.Exists(m => m.Name == row["Name"].ToString()))
                    {
                        RowsToDelete.Add(row);
                    }
                }

                foreach (var dr in RowsToDelete)
                {
                    rs.Rows.Remove(dr);
                }
                e.Data = rs;
            }
        }

        private void ContainerForChart_DataChanged(object sender, EventArgs e)
        {
            DCList = GetDCList();
            ProcessChartGroupsAndEntries();
            DataCollection.ClearData();
            SPCChart.ClearData();
            SPCChartGroup.ClearData();
            DataCollection.RequestSelectionValues();
            SPCChart.RequestSelectionValues();
            SPCChartGroup.RequestSelectionValues();
        }

        private List<OM.RevisionedObjectRef> GetDCList()
        {
            List<OM.RevisionedObjectRef> dCList = new List<OM.RevisionedObjectRef>();
            if (ContainerForChart.Data != null)
            {
                var selectedContainer = ContainerForChart.Data.ToString();
                var prof = FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile;
                var svc = new WCF.Services.ContainerTxnService(prof);
                var data = new OM.ContainerTxn();

                data.Container = new OM.ContainerRef(selectedContainer);
                var req = new WCF.Services.ContainerTxn_Request
                {
                    Info = new OM.ContainerTxn_Info
                    {
                        CurrentContainerStatus = new OM.CurrentContainerStatus_Info
                        {
                            SpecName = new OM.Info(true),
                            SpecRevision = new OM.Info()
                        }
                    }
                };
                WCF.Services.ContainerTxn_Result res;
                var state = svc.Load(data, req, out res);
                if (state.IsSuccess)
                {
                    var qServ = new SpecMaintService(prof);
                    var request = new SpecMaint_Request
                    {
                        Info = new OM.SpecMaint_Info
                        {
                            UDCList = new OM.Info(true)
                        }
                    };
                    OM.SpecMaint dt;
                    if (res.Value.CurrentContainerStatus.SpecRevision != null)
                        dt = new OM.SpecMaint { ObjectToChange = new OM.RevisionedObjectRef(res.Value.CurrentContainerStatus.SpecName.Value, res.Value.CurrentContainerStatus.SpecRevision.Value) };
                    else
                        dt = new OM.SpecMaint { ObjectToChange = new OM.RevisionedObjectRef(res.Value.CurrentContainerStatus.SpecName.Value) };
                    var status = qServ.Load(dt, request, out var result);
                    if (status.IsSuccess && result.Value.UDCList != null)
                    {
                        foreach (var udc in result.Value.UDCList)
                        {
                            dCList.Add(udc);
                        }
                    }
                }
            }
            else
                return null;
            return dCList;
        }

        private void ProcessChartGroupsAndEntries()
        {
            if(DCList != null)
            {
                GroupsList = new List<OM.NamedObjectRef>();
                ChartsList = new List<OM.NamedObjectRef>();
                foreach (var dc in DCList)
                {
                    var service = new DataCollectionDefMaintService(FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile);
                    var cdo = new OM.DataCollectionDefMaint { ObjectToChange = new OM.RevisionedObjectRef(dc.Name.ToString()) };
                    var request = new DataCollectionDefMaint_Request
                    {
                        Info = new OM.DataCollectionDefMaint_Info
                        {
                            ObjectChanges = new OM.DataCollectionDefChanges_Info
                            {
                                SPCChartDefGroup = new OM.Info(true),
                                SPCChartDefEntries = new OM.Info(true)
                            }
                        }
                    };

                    OM.ResultStatus oRS = service.Load(cdo, request, out var oResult);
                    if (oRS.IsSuccess)
                    {
                        if (oResult.Value.ObjectChanges.SPCChartDefGroup != null)
                        {
                            GroupsList.Add(oResult.Value.ObjectChanges.SPCChartDefGroup);
                            var _srv = new SPCChartDefGroupMaintService(FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile);
                            var _cdo = new OM.SPCChartDefGroupMaint { ObjectToChange = new OM.NamedObjectRef(oResult.Value.ObjectChanges.SPCChartDefGroup.Name.ToString()) };
                            var _request = new SPCChartDefGroupMaint_Request
                            {
                                Info = new OM.SPCChartDefGroupMaint_Info
                                {
                                    ObjectChanges = new OM.SPCChartDefGroupChanges_Info
                                    {
                                        Groups = new OM.Info(true)
                                    }
                                }
                            };

                            OM.ResultStatus _oRS = _srv.Load(_cdo, _request, out var _oResult);
                            if (_oRS.IsSuccess)
                            {
                                if (_oResult.Value.ObjectChanges.Groups != null && _oResult.Value.ObjectChanges.Groups.Length > 0)
                                    GroupsList.AddRange(_oResult.Value.ObjectChanges.Groups);
                            }
                        }
                        if (oResult.Value.ObjectChanges.SPCChartDefEntries != null)
                            ChartsList.AddRange(oResult.Value.ObjectChanges.SPCChartDefEntries);
                    }
                }
            }
            else
            {
                GroupsList = null;
                ChartsList = null;
            }
        }






    }

}
