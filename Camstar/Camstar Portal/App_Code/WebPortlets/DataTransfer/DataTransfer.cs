// Copyright Siemens 2024  
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using Camstar.WCF.Services;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.Utilities;
using Newtonsoft.Json;
using OM = Camstar.WCF.ObjectStack;
using Siemens.OPCR.Diagnostics;
using Camstar.WebPortal.Constants;
using System.Threading.Tasks;

namespace Camstar.WebPortal.WebPortlets.DataTransfer
{
    public class DataTransfer
    {
        static protected LogClient _log = null;

        private Dictionary<string, SelectedInstanceItem> processedSelectedItems = new Dictionary<string, SelectedInstanceItem>();
        public virtual TransferType Type { get; private set; }
        public virtual TransferStatus Status { get { return _info.Status; } }
        public virtual ErrorItem[] Errors { get; set; }
        public virtual SummaryItem[] Summary { get { return _info.GetTransferSummary(_labelsForTransfer); } }
        public virtual string TransferStatusResponse { get; set; }

        public DataTransfer(IDataTransferInfo dataTransferInfo, IDataTransferRepository dataTransferRepository, IDictionary<string, string> labelsForTransfer)
        {
            _dataTransferRepository = dataTransferRepository;
            _info = dataTransferInfo;
            _labelsForTransfer = labelsForTransfer;
        }

        private void LogMessage(LogSeverity level, string method, string message)
        {
            if (_log == null)
            {
                _log = new LogClient("Camstar");
                _log.ClassName = "DataTransfer";
            }
            _log.LogMessage(level, method, message);
        }

        public virtual void SetTransferType(TransferType type)
        {
            Type = type;
        }

        public virtual OM.ResultStatus Transfer(string transferName, string transferSetName, SelectedInstanceItem[] instances, object[] options, bool isEncription = false)
        {
            _info.Init(transferName, Type, instances);
            _instances = instances;

            if (!instances.Any())
                return new OM.ResultStatus(_labelsForTransfer["Lbl_DT_SelectInstance"] ?? string.Empty, false);

            if (Type == TransferType.Export)
            {
                var resultStatus = _dataTransferRepository.Export(instances, transferName, GetUploadLocationURL(transferName), isEncription);
                return resultStatus;
            }
            if (Type == TransferType.Import)
            {
                Object trackableObject = null;

                var importIfExists = (bool)options[0];
                var importAction = (OM.ImportActionEnum?)options[1];
                if (options.Length > 2)
                {
                    trackableObject = options[2];
                    var resultStatus = _dataTransferRepository.ImportChangePackage(instances, transferName, transferSetName, importIfExists, importAction, trackableObject);
                    return resultStatus;
                }
                else
                {
                    var resultStatus = _dataTransferRepository.Import(instances, transferName, transferSetName, importIfExists, importAction);
                    return resultStatus;
                }
            }
            return null;
        }

        public virtual OM.ResultStatus GetTransferStatus(string transferName)
        {
            OM.RecordSet result;

            var transferType = "";
            if (Type == TransferType.Export)
                transferType = "1";
            if (Type == TransferType.Import)
                transferType = "2";

            var resultStatus = _dataTransferRepository.GetTransferStatus(transferName, transferType, out result);
            if (resultStatus.IsSuccess)
            {
                if (result != null)
                    UpdateTransferedStatus(result);
            }
            return resultStatus;
        }

        public virtual string[] GetExcludedObjects()
        {
            return _dataTransferRepository.GetExcludedObjectsByFactory();
        }
        private List<SelectedInstanceItem> _references;
        private string[] _excludedReferences;
        public virtual SelectedInstanceItem[] GetReferences(SelectedInstanceItem[] selectedInstances, string[] excludedReferences, bool filteredRefs = false)
        {
            _references = new List<SelectedInstanceItem>(selectedInstances);
            _excludedReferences = excludedReferences;
            while (selectedInstances != null && selectedInstances.Length > 0)
            {
                int i = 0;
                var objReferences = _dataTransferRepository.GetReferences(selectedInstances, filteredRefs);
                foreach (var reference in objReferences)
                {
                    var inspectedId = selectedInstances[i].InstanceID;
                    if (reference.ObjectFields != null)
                        foreach (var field in reference.ObjectFields)
                        {
                            EvaluateReference(field);
                        }
                    var refToClose = _references.FirstOrDefault(fx => fx.InstanceID == inspectedId);
                    if (refToClose != null)
                        refToClose.ShouldOpen = false;
                    i++;
                }
                selectedInstances =
                    (from r in _references
                     where r.ShouldOpen
                     select new SelectedInstanceItem { InstanceID = r.InstanceID, Name = r.Name }).ToArray();
            }
            var orderNumber = 1;
            Array.ForEach(_references.ToArray(), r => { r.Order = orderNumber; orderNumber++; });
            return _references.ToArray();
        }

        public virtual IEnumerable<SelectedInstanceItem> OrderSelectedInstances(SelectedInstanceItem[] selectedItems, bool includeRefs = false)
        {
            IEnumerable<SelectedInstanceItem> orderedItems = null;
            try
            {
                if (selectedItems != null && selectedItems.Count() > 0)
                {
                    LogMessage(LogSeverity.Information, "OrderSelectedInstances", $"Start processing {selectedItems.Length} selected items, IncludeRefs = {includeRefs}");
                    selectedItems = selectedItems.Select(x => { x.Order = 0; x.Dependencies = null; return x; }).ToArray();
                    _references = new List<SelectedInstanceItem>(selectedItems);
                    _excludedReferences = GetExcludedObjects();
                    // Remove excluded objects
                    _references.RemoveAll(x => !string.IsNullOrEmpty(x.CDOTypeID) && _excludedReferences.Any(y => y == x.CDOTypeID.ToString()));
                    GetReferencesForInstance();

                    LogMessage(LogSeverity.Information, "OrderSelectedInstances", $"Processing {_references.Count} references");
                    foreach (var inMemoryItem in _references)
                    {
                        // Check for ror's since if instance is revisioned object and NOT RoR, 
                        // it has a dependency on the RoR
                        if (!string.IsNullOrEmpty(inMemoryItem.Revision))
                        {
                            var inMemoryRoR = _references.FirstOrDefault(x => x.Name == inMemoryItem.Name && x.CDOTypeName == inMemoryItem.CDOTypeName && x.IsROR);
                            if (inMemoryRoR != null)
                            {
                                if (inMemoryItem.InstanceID != inMemoryRoR.InstanceID)
                                {
                                    if (inMemoryItem.Revision != inMemoryRoR.Revision)
                                    {
                                        if (inMemoryItem.Dependencies == null)
                                        {
                                            inMemoryItem.Dependencies = new string[] { inMemoryRoR.InstanceID };
                                        }
                                        else
                                        {
                                            if (!inMemoryItem.Dependencies.Any(x => x == inMemoryRoR.InstanceID))
                                            {
                                                var newList = inMemoryItem.Dependencies.ToList();
                                                newList.Add(inMemoryRoR.InstanceID);
                                                inMemoryItem.Dependencies = newList.ToArray();
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (inMemoryRoR.Dependencies == null)
                                        {
                                            inMemoryRoR.Dependencies = new string[] { inMemoryItem.InstanceID };
                                        }
                                        else
                                        {
                                            if (!inMemoryRoR.Dependencies.Any(x => x == inMemoryItem.InstanceID))
                                            {
                                                var newList = inMemoryRoR.Dependencies.ToList();
                                                newList.Add(inMemoryItem.InstanceID);
                                                inMemoryRoR.Dependencies = newList.ToArray();
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        // Get order of all dependencies                    
                        var order = inMemoryItem.Order > 0 ? inMemoryItem.Order : 1;
                        RecursivelyBuildInstanceDependenices(inMemoryItem, order);
                        processedSelectedItems.Clear();
                    }

                    // Remove excluded objects
                    _references.RemoveAll(x => !string.IsNullOrEmpty(x.CDOTypeID) && _excludedReferences.Any(y => y == x.CDOTypeID.ToString()));

                    if (!includeRefs)
                    {
                        if (_references.Any(x => selectedItems.Any(y => y.InstanceID == x.InstanceID)))
                        {
                            orderedItems = _references.Where(x => selectedItems.Any(y => y.InstanceID == x.InstanceID)).OrderByDescending(x => x.Order);
                        }
                        else
                        {
                            orderedItems = null;
                        }
                    }
                    else
                    {
                        orderedItems = _references.OrderByDescending(x => x.Order);
                    }
                    var index = 1;
                    foreach (var item in orderedItems)
                    {
                        item.Order = index;
                        index++;
                    }
                    orderedItems = orderedItems.OrderBy(x => x.Order);
                    LogMessage(LogSeverity.Information, "OrderSelectedInstances", $"Completed processing and returniung {orderedItems.Count()}");
                }
            }
            catch (Exception ex)
            {
                LogMessage(LogSeverity.Error, "OrderSelectedInstances", ex.ToString());
            }
            return orderedItems;
        }

        protected virtual void RecursivelyBuildInstanceDependenices(SelectedInstanceItem inst, int order)
        {
            if (processedSelectedItems.ContainsKey(inst.InstanceID))
            {
                return;
            }
            processedSelectedItems.Add(inst.InstanceID, inst);

            if (inst.Dependencies != null)
            {
                foreach (var d in inst.Dependencies)
                {
                    var inMemoryDep = _references.FirstOrDefault(x => x.InstanceID == d);
                    if (inMemoryDep != null)
                    {
                        inMemoryDep.ReferenceParent = inst.InstanceID;
                        order = _references.Where(x => inst.Dependencies.Any(y => y == x.InstanceID)).Max(x => x.Order);
                        inMemoryDep.Order = order;
                        RecursivelyBuildInstanceDependenices(inMemoryDep, order);
                    }
                }
                inst.Order = _references.Where(x => inst.Dependencies.Any(y => y == x.InstanceID)).Min(x => x.Order) - 1;
                inst.ShouldOpen = false;
            }
            else
            {
                if (inst.ShouldOpen)
                {
                    inst.Order = _references.Max(x => x.Order);
                }
            }
        }
        protected virtual void GetReferencesForInstance()
        {
            var objRefs = _dataTransferRepository.GetReferences(_references.ToArray());
            if (objRefs != null && objRefs.Count() > 0)
            {
                foreach (var objRef in objRefs)
                {
                    SelectedInstanceItem item = _references.FirstOrDefault(x => x.InstanceID == objRef.ObjectInstanceId.Value);
                    if (item == null)
                    {
                        item = new SelectedInstanceItem(objRef);
                        if (item.ShouldOpen)
                            _references.Add(item);
                    }
                    if (item.ShouldOpen)
                    {
                        item.IsROR = objRef.IsROR != null ? objRef.IsROR.Value : false;

                        if (objRef.ObjectFields != null)
                        {
                            foreach (var fld in objRef.ObjectFields)
                            {
                                AddInstanceDependency(item, fld);
                            }
                        }
                    }
                }
            }
        }

        protected virtual void AddInstanceDependency(SelectedInstanceItem parent, OM.ObjectField fld)
        {
            // Check if reference field
            if (fld is OM.ReferenceField)
            {
                var refFld = (OM.ReferenceField)fld;
                if (refFld.References != null)
                {
                    foreach (var rf in refFld.References)
                    {
                        //  Fix for CPR 336275 - IR # 10688948 - export fails for Resource that has Resource Setup data
                        if (rf.ObjectName != null && rf.ObjectInstanceId != null)
                        {
                            var child = _references.FirstOrDefault(x => x.InstanceID == rf.ObjectInstanceId.Value);
                            if (child == null)
                            {
                                child = new SelectedInstanceItem(rf);
                                if (child.ShouldOpen)
                                    _references.Add(child);
                            }

                            if (parent.Dependencies != null && child.Dependencies != null)
                            {
                                if (parent.Dependencies.Any(circRef => circRef == child.InstanceID))
                                {
                                    if (_circularRefInstances == null)
                                    {
                                        _circularRefInstances = new Dictionary<string, string>();
                                    }
                                    if (!_circularRefInstances.ContainsKey(child.Name))
                                    {
                                        _circularRefInstances.Add(child.Name, child.CDOTypeName);
                                    }
                                }
                            }

                            if (parent.Dependencies == null)
                            {
                                parent.Dependencies = new string[] { child.InstanceID };
                            }
                            else
                            {
                                if (!parent.Dependencies.Any(x => x == child.InstanceID))
                                {
                                    var newList = parent.Dependencies.ToList();
                                    newList.Add(child.InstanceID);
                                    parent.Dependencies = newList.ToArray();
                                }
                            }

                            if (rf.ObjectFields != null)
                            {
                                foreach (var rf2 in rf.ObjectFields)
                                {
                                    AddInstanceDependency(parent, rf2);
                                }
                            }
                        }
                    }
                }
            }
            if (fld is OM.SubentityField)
            {
                var subEntFld = (OM.SubentityField)fld;
                if (subEntFld.Instances != null)
                {

                    foreach (var si in subEntFld.Instances)
                    {
                        if (si.ObjectFields != null)
                        {
                            foreach (var siObjFld in si.ObjectFields)
                            {
                                AddInstanceDependency(parent, siObjFld);
                            }
                        }
                    }
                }
            }
        }

        protected virtual void AddFieldItemToReferences(OM.ObjectField r, int order, string parent)
        {
            if (r is OM.SubentityField)
            {
                var subentity = r as OM.SubentityField;
                if (subentity.Instances != null)
                {
                    Array.ForEach(subentity.Instances, item =>
                    {
                        AddSubentityInstance(item, order, parent);
                    });
                }
            }
            else if (r is OM.ReferenceField)
            {
                var reference = r as OM.ReferenceField;
                if (reference.References != null)
                    Array.ForEach(reference.References, rf =>
                    {
                        if (rf.ObjectFields != null)
                            Array.ForEach(rf.ObjectFields, item => AddFieldItemToReferences(item, order, (string)item.FieldName));
                        AddToReferences(new SelectedInstanceItem(rf) { ShouldOpen = true, Order = order, ReferenceParent = parent });
                    });
            }
        }

        protected virtual void RecursivelyIncrementOrder(SelectedInstanceItem item, int order, int depthCount)
        {
            depthCount++;
            var circRefInst = _references.FirstOrDefault(x => x.InstanceID == item.ReferenceParent);
            if (depthCount > _maxDepthCount)
            {
                if (_circularRefInstances == null)
                {
                    _circularRefInstances = new Dictionary<string, string>();
                }
                if (circRefInst != null)
                {
                    if (!_circularRefInstances.ContainsKey(circRefInst.Name) && !_circularRefInstances.ContainsKey(item.Name))
                    {
                        _circularRefInstances.Add(item.Name, item.CDOTypeName);
                    }
                }
            }
            item.Order = Math.Max(order, item.Order);
            var childItems = _references.Where(x => x.ReferenceParent == item.InstanceID);
            foreach (var child in childItems)
            {
                if (_circularRefInstances != null)
                {
                    if (circRefInst != null)
                    {
                        if (_circularRefInstances.ContainsKey(circRefInst.Name))
                        {
                            break;
                        }
                    }
                }
                RecursivelyIncrementOrder(child, item.Order + 1, depthCount);
            }
        }

        protected virtual void AddToReferences(SelectedInstanceItem it, int depthCount = 0)
        {
            if (_excludedReferences == null || !_excludedReferences.Contains(it.CDOTypeID.ToString()))
            {
                var existingItem = _references.FirstOrDefault(x => x.InstanceID == it.InstanceID);
                if (existingItem != null)
                {
                    existingItem.ReferenceParent = it.ReferenceParent;
                    RecursivelyIncrementOrder(existingItem, it.Order++, depthCount);
                }
                else
                    _references.Insert(0, it);
            }
        }

        protected virtual void AddSubentityInstance(OM.SubentityInstance sr, int order, string parent)
        {
            if (sr.ObjectFields != null)
            {
                Array.ForEach(sr.ObjectFields, item =>
                {
                    AddFieldItemToReferences(item, order, parent);
                });
            }
        }

        protected virtual void EvaluateReference(OM.ObjectField field)
        {
            var subentityField = field as OM.SubentityField;
            if (subentityField != null)
            {
                if (subentityField.Instances != null)
                    foreach (var subentityInstance in subentityField.Instances)
                    {
                        if (subentityInstance.ObjectFields != null)
                            foreach (var subInstanceField in subentityInstance.ObjectFields)
                            {
                                EvaluateReference(subInstanceField);
                            }
                    }
            }
            var referenceField = field as OM.ReferenceField;
            if (referenceField != null)
            {
                if (referenceField.References != null)
                    foreach (var reference in (field as OM.ReferenceField).References)
                    {
                        if (reference.ObjectFields != null)
                            foreach (var refObjField in reference.ObjectFields)
                            {
                                EvaluateReference(refObjField);
                            }
                        AddToReference(reference);
                    }
            }
        }
        protected virtual void AddToReference(OM.ObjectReferencesInfo reference)
        {
            var referenceItem = new SelectedInstanceItem(reference);// { ShouldOpen = true };
            if (referenceItem.ShouldOpen)
            {
                if (_references.All(x => x.InstanceID != referenceItem.InstanceID))
                {
                    if (_excludedReferences == null || !_excludedReferences.Contains(referenceItem.CDOTypeName))
                    {
                        _references.Insert(0, referenceItem);
                    }
                }
            }
        }

        public virtual OM.ResultStatus GetExportFile(string transferName)
        {
            ExportImportController_Result result;
            var resultStatus = _dataTransferRepository.GetExportFile(transferName, out result);

            if (result != null && result.Value != null)
                TransferStatusResponse = result.Value.ExportImportFileURL.ToString();

            return resultStatus;
        }

        protected virtual void UpdateTransferedStatus(OM.RecordSet result)
        {
            const int statusCol = 5;
            const string errorStatus = "2";

            if (result.Rows != null && result.Rows.Any())
            {
                _info.TransferedCount = result.Rows.Count();
            }

            if (result.Rows != null)
            {
                var errorRows = result.Rows.Where(r => r.Values[statusCol] == errorStatus);
                if (errorRows.Any())
                {
                    _info.Error();
                    Errors = errorRows.Select(error => new ErrorItem(error.Values[2], error.Values[3], error.Values[7])).ToArray();
                }
            }

            if (_info.TransferedCount >= _instances.Count())
            {
                _info.Complete();
            }
            TransferStatusResponse = GenerateTransferStatusResponse(result);
        }

        protected virtual string GenerateTransferStatusResponse(OM.RecordSet result)
        {
            var dt = result.GetAsExplicitlyDataTable();
            var logrecs = new List<ConsoleLogRecord>();

            if (dt != null && dt.Rows != null)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    var logrec = new ConsoleLogRecord(dt.Rows[i]) { RecordNumber = _info.TransferedCount + i };
                    logrecs.Add(logrec);
                }
            }

            if (_info.Status == TransferStatus.Complete)
            {
                var completeLogRec = new ConsoleLogRecord
                {
                    RecordNumber = 0,
                    RecordType = "complete",
                    InstanceType = "",
                    LogDetailID = 0,
                    Message = "",
                    ObjectName = "",
                    ProcTime = "",
                    Status = "",
                    TotalNumber = 0
                };
                logrecs.Add(completeLogRec);
            }

            return JsonConvert.SerializeObject(logrecs.OrderBy(r => r.ProcTime));
        }

        protected virtual string GetUploadLocationURL(string fileName)
        {
            if (fileName == null) return null;

            var cultureSettings = CamstarPortalSection.Settings;
            return cultureSettings.DefaultSettings.UploadDirectory +
                   (cultureSettings.DefaultSettings.UploadDirectory.EndsWith("\\") ? "" : "\\") +
                   fileName;
        }

        private IDataTransferInfo _info;
        private IDataTransferRepository _dataTransferRepository;
        private SelectedInstanceItem[] _instances;
        private IDictionary<string, string> _labelsForTransfer;
        private const int _maxDepthCount = 5;
        private string _circRefError = HttpContext.Current != null ? FrameworkManagerUtil.GetLabelValue("CircularRefErrorLbl") ?? string.Empty : string.Empty;
        private Dictionary<string, string> _circularRefInstances;
    }




    public class SummaryItem
    {
        public virtual string Paragraph { get; set; }
        public virtual string Category { get; set; }
        public virtual string DataText { get; set; }
        public virtual int? CntSuccess { get; set; }
        public virtual int? CntError { get; set; }

        public SummaryItem()
        {
            CntSuccess = null;
            CntError = null;
        }

        public SummaryItem(string paragraph, string category, string dataText)
            : this(paragraph, category, dataText, null, null)
        {
        }

        public SummaryItem(string paragraph, string category, string dataText, int? counter_success, int? counter_error)
        {
            Paragraph = paragraph;
            Category = category;
            DataText = dataText;
            CntSuccess = counter_success;
            CntError = counter_error;
        }
    }

    public class ErrorItem
    {
        public virtual string ObjectType { get; set; }
        public virtual string InstanceName { get; set; }
        public virtual string ErrorDescription { get; set; }

        public ErrorItem(string objectType, string instanceName, string errorDesc)
        {
            ObjectType = objectType;
            InstanceName = instanceName;
            ErrorDescription = errorDesc;
        }
    }


    public interface IDataTransferInfo
    {
        void Init(string transferName, TransferType type, SelectedInstanceItem[] instances);
        void Error();
        void Complete();
        SummaryItem[] GetTransferSummary(IDictionary<string, string> labelsForTransfer);

        TransferType Type { get; }
        TransferStatus Status { get; }
        int TransferedCount { get; set; }
    }

    public class DataTransferInfo : IDataTransferInfo
    {
        public virtual TransferType Type { get; private set; }
        public virtual TransferStatus Status { get; private set; }
        public virtual int TransferedCount { get; set; }
        protected virtual int TransferedObjs { get; set; }
        protected virtual string SelectedCount { get; set; }
        protected virtual string SelectedObjs { get; set; }
        protected virtual int ErroredCount { get; set; }
        protected virtual int ErroredObjs { get; set; }
        protected virtual string FileName { get; set; }
        protected virtual string EmployeeName { get; set; }
        protected virtual DateTime StartTime { get; set; }
        protected virtual DateTime EndTime { get; set; }

        public DataTransferInfo(string employeeName)
        {
            EmployeeName = employeeName;
            Status = TransferStatus.NotStarted;
        }

        public virtual void Init(string transferName, TransferType type, SelectedInstanceItem[] instances)
        {
            Type = type;
            Status = TransferStatus.Transfering;
            FileName = transferName;
            StartTime = DateTime.Now;
            TransferedCount = 0;
            SelectedObjs = instances.GroupBy(e => e.CDOTypeName).Count().ToString();
            SelectedCount = instances.Count().ToString();
        }

        public virtual void Error()
        {
            //If error occurs the whole transaction is rolled back, so reset
            Status = TransferStatus.Error;
            EndTime = DateTime.Now;
            TransferedCount = 0;
            TransferedObjs = 0;
            ErroredObjs = int.Parse(SelectedObjs);
            ErroredCount = int.Parse(SelectedCount);
        }

        public virtual void Complete()
        {
            Status = TransferStatus.Complete;
            EndTime = DateTime.Now;
            TransferedObjs = int.Parse(SelectedObjs);
        }

        public virtual SummaryItem[] GetTransferSummary(IDictionary<string, string> labelsForTransfer)
        {
            SummaryItem[] summaryItems;

            string statusStr = labelsForTransfer["Lbl_DT_Status"] ?? string.Empty;
            string employeeStr = labelsForTransfer["Lbl_DT_Employee"] ?? string.Empty;
            string startTimestampStr = labelsForTransfer["Lbl_DT_StartTimestamp"] ?? string.Empty;
            string executionTimeStr = labelsForTransfer["Lbl_DT_ExecutionTime"] ?? string.Empty;
            if (Status != TransferStatus.NotStarted)
            {
                var currentStatus = string.Empty;
                switch (Status)
                {
                    case TransferStatus.Complete:
                        currentStatus = labelsForTransfer["Lbl_DT_Complete"] ?? string.Empty;
                        break;
                    case TransferStatus.Error:
                        currentStatus = labelsForTransfer["Lbl_DT_Error"] ?? string.Empty;
                        break;
                    case TransferStatus.Transfering:
                        currentStatus = labelsForTransfer["Lbl_DT_Transferring"] ?? string.Empty;
                        break;
                    default:
                        break;
                }
                var executionTime = EndTime.Subtract(StartTime);
                string sourceStr = labelsForTransfer["Lbl_DT_Source"] ?? string.Empty;
                summaryItems = new SummaryItem[]
                                   {
                                       new SummaryItem(statusStr, statusStr, currentStatus),
                                       new SummaryItem(statusStr, employeeStr, EmployeeName),
                                       new SummaryItem(statusStr, startTimestampStr, StartTime.ToShortTimeString()),
                                       new SummaryItem(statusStr, executionTimeStr,
                                                       string.Format(labelsForTransfer["Lbl_DT_MinsSecs"] ?? string.Empty, executionTime.Minutes,
                                                                     executionTime.Seconds)),
                                       new SummaryItem(sourceStr, (labelsForTransfer["Lbl_DT_ObjectTypes"] ?? string.Empty) + Type.ToString() + "ed", SelectedObjs,
                                                       TransferedObjs, ErroredObjs),
                                       new SummaryItem(sourceStr, (labelsForTransfer["Lbl_DT_Instances"] ?? string.Empty) + Type.ToString() + "ed", SelectedCount,
                                                       TransferedCount, ErroredCount),
                                       new SummaryItem(sourceStr, Type.ToString() + labelsForTransfer["Lbl_DT_FileName"] ?? string.Empty, FileName)
                                   };
            }
            else
            {
                summaryItems = new SummaryItem[]
                                   {
                                       new SummaryItem(statusStr, statusStr, labelsForTransfer["Lbl_DT_NotStarted"] ?? string.Empty),
                                       new SummaryItem(statusStr, employeeStr, EmployeeName),
                                       new SummaryItem(statusStr, startTimestampStr, string.Empty),
                                       new SummaryItem(statusStr, executionTimeStr, string.Empty)
                                   };
            }

            return summaryItems;
        }
    }

    public enum TransferStatus
    {
        NotStarted = 0,
        Transfering,
        Complete,
        Error
    }

    public enum TransferType
    {
        NotSet,
        Import,
        Export,
        EndtoEnd
    }
    internal class ConsoleLogRecord
    {
        public virtual string RecordType { get; set; }
        public virtual string Status { get; set; }
        public virtual string InstanceType { get; set; }
        public virtual string ProcTime { get; set; }
        public virtual int RecordNumber { get; set; }
        public virtual int TotalNumber { get; set; }
        public virtual string ObjectName { get; set; }
        public virtual int LogDetailID { get; set; }
        public virtual string Message { get; set; }


        public ConsoleLogRecord(int total) { TotalNumber = total; }

        public ConsoleLogRecord()
        {
        }

        public ConsoleLogRecord(OM.ExportImportStatus details)
        {
            Status = details.Status.ToString();
            if (details.Status == OM.ExportImportStatusEnum.NotStarted)
            {
                RecordType = "idle";
            }
            else
            {
                RecordType = "proloque";
                ProcTime = details.ProcessStartTime.Value.ToString();
                RecordNumber = details.TotalObjectCount.Value;
            }
        }

        public ConsoleLogRecord(OM.ExportImportLogDetail details)
        {
            RecordType = "log";
            InstanceType = details.ObjectTypeName.Value;
            Status = details.Status.ToString();
            ProcTime = (details.LastUpdateTime != null ? details.LastUpdateTime.Value.ToString("T") : string.Empty);
            ObjectName = details.ObjectName.Value;
        }

        public ConsoleLogRecord(DataRow r)
        {
            RecordType = "log";
            InstanceType = r["ObjectType"] as string;
            Status = r["Status"].ToString();
            ProcTime = r["LastUpdateTime"].ToString();
            ObjectName = r["Name"] as string;
            LogDetailID = (int)r["DetailId"];
            Message = r["CompletionMessage"] as string;
        }

    }

    public interface IDataTransferRepository
    {
        OM.ResultStatus Export(SelectedInstanceItem[] selectedInstances, string exportName, string exportLocationUrl, bool encryptFile = false);
        OM.ResultStatus Import(SelectedInstanceItem[] selectedInstances, string importName, string importSetName, bool importIfExists, OM.ImportActionEnum? actionIfModified);
        OM.ResultStatus ImportChangePackage(SelectedInstanceItem[] selectedInstances, string importName, string importSetName, bool importIfExists, OM.ImportActionEnum? actionIfModified, object trackableObject);
        OM.ResultStatus GetTransferStatus(string transferName, string transferType, out OM.RecordSet result);
        OM.ResultStatus GetExportFile(string fileName, out ExportImportController_Result result);
        OM.ObjectReferencesInfo[] GetReferences(SelectedInstanceItem[] selectedInstances, bool selectedInstancesOnly = false);
        string[] GetExcludedObjectsByFactory();
    }

    public class DataTransferRepository : IDataTransferRepository
    {
        static protected LogClient _log = null;

        public DataTransferRepository(OM.UserProfile userProflie)
        {
            _userProfile = userProflie;            
        }

        private void LogMessage(LogSeverity level, string method, string message)
        {
            if (_log == null)
            {
                _log = new LogClient("Camstar");
                _log.ClassName = "DataTransferRepository";
            }
            _log.LogMessage(level, method, message);
        }

        public virtual OM.ResultStatus Export
            (SelectedInstanceItem[] selectedInstances, string exportName, string exportLocationURL, bool encryptFile = false)
        {
            var exportItems = ConvertSelectedToExportItems(selectedInstances);
            var serv = new ExportService(_userProfile);
            var data = new OM.Export { Details = exportItems, ExportImportName = exportName };

            data.ExportLocationURL = exportLocationURL;
            data.GenerateExportFile = true;
            data.EncryptFile = encryptFile;

            var res = serv.ExecuteTransaction(data);
            return res;
        }

        public virtual OM.ResultStatus Import(SelectedInstanceItem[] selectedInstances, string importName, string importSetName,
                                      bool importIfExists, OM.ImportActionEnum? actionIfModified)
        {
            var serv = new ImportService(_userProfile);
            var data = new OM.Import
            {
                Details = GenerateImportDetails(selectedInstances),
                ExportImportName = importName,
                ImportSetName = importSetName
            };

            data.ImportIfExists = importIfExists;
            data.ImportActionIfModified = actionIfModified;

            return serv.ExecuteTransaction(data);
        }

        public virtual OM.ResultStatus ImportChangePackage(SelectedInstanceItem[] selectedInstances, string importName, string importSetName,
                                        bool importIfExists, OM.ImportActionEnum? actionIfModified, Object trackableObject)
        {
            var serv = new ImportService(_userProfile);
            var changePackage = (OM.NamedObjectRef)trackableObject;
            var data = new OM.Import
            {
                Details = GenerateImportDetails(selectedInstances),
                ExportImportName = importName,
                ImportSetName = importSetName,
                TrackableObjectName = changePackage.Name,
            };
            data.ImportIfExists = importIfExists;
            data.ImportActionIfModified = actionIfModified;
            return serv.ExecuteTransaction(data);
        }

        protected virtual OM.ExportImportItem[] GenerateImportDetails(SelectedInstanceItem[] selectedItems)
        {
            var data = selectedItems;
            if (data != null)
            {
                var ds = data.OrderBy(dx => dx.Order);

                ArrayList importItems = new ArrayList();

                foreach (var item in ds)
                {
                    var importItem = new OM.ExportImportItem()
                    {
                        ListItemAction = OM.ListItemAction.Add,
                        ObjectName = item.Name,
                        ObjectTypeName =
                            new OM.Enumeration<OM.ExportableObjectEnum, string>(item.CDOTypeName)
                    };
                    if (!string.IsNullOrEmpty(item.Revision))
                    {
                        importItem.Revision = item.Revision;
                        importItem.IsROR = item.IsROR;
                    }
                    importItems.Add(importItem);
                }
                return importItems.ToArray(typeof(OM.ExportImportItem)) as OM.ExportImportItem[];
            }
            return null;
        }

        public virtual OM.ResultStatus GetExportFile(string fileName, out ExportImportController_Result result)
        {
            var serv = new ExportImportControllerService(_userProfile);
            var data = new OM.ExportImportController
            {
                ExportImportName = fileName,
                ExportImportType = OM.ExportImportTypeEnum.Export
            };
            var request = new ExportImportController_Request
            {
                Info = new OM.ExportImportController_Info { ExportImportFileURL = new OM.Info(true) }
            };
            var res = serv.GenerateExportFile(data, request, out result);
            return res;
        }

        public virtual OM.ResultStatus GetTransferStatus(string transferName, string transferType,
                                                 out OM.RecordSet result)
        {
            var serv = new QueryService(_userProfile);
            var qparam = new OM.QueryParameters()
            {
                Parameters = new OM.QueryParameter[]
                                                  {
                                                      new OM.QueryParameter("LastDetailId", "0"),
                                                      new OM.QueryParameter("ExportImportType", transferType),
                                                      new OM.QueryParameter("ExportImportName", transferName)
                                                  }
            };

            var resultStatus = serv.Execute("CompleteLogDetails", qparam,
                                            new OM.QueryOptions() { QueryType = OM.QueryType.System }, out result);
            return resultStatus;
        }

        public virtual string[] GetExcludedObjectsByFactory()
        {
            var service = new FactoryMaintService(_userProfile);
            var session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
            var factory = session.SessionValues.Factory;

            // only execute if factory is assigned to current session.
            if (!string.IsNullOrEmpty(factory))
            {
                var serviceData = new OM.FactoryMaint()
                {
                    ObjectToChange = new OM.NamedObjectRef(factory)
                };
                var request = new FactoryMaint_Request();
                request.Info = new OM.FactoryMaint_Info
                {
                    RequestValue = true,
                    ObjectChanges = new OM.FactoryChanges_Info
                    {
                        Name = new OM.Info(true),
                        ModelingObjsToExclude = new OM.ModelingObjsToExcludeChanges_Info
                        {
                            ModelingCDOTypeId = new OM.Info(true),
                            DisplayName = new OM.Info(true)
                        }
                    }
                };
                var result = new FactoryMaint_Result();
                var resultStatus = service.Load(serviceData, request, out result);
                if (IsErrorCheck(resultStatus) && result.Value.ObjectChanges.ModelingObjsToExclude != null)
                {
                    return result.Value.ObjectChanges.ModelingObjsToExclude
                        .Where(objExclude => objExclude.ModelingCDOTypeId != null)
                        .Select(objExclude => objExclude.ModelingCDOTypeId.ToString())
                        .ToArray();
                }
                else
                {
                    return new string[0];
                }
            }
            else
            {
                return new string[0];
            }
        }

        public virtual OM.ObjectReferencesInfo[] GetReferences(SelectedInstanceItem[] selectedInstances, bool selectedInstancesOnly = false)
        {
            var service = new WCF.Services.CDOInquiryService(_userProfile);
            var inquiry = new OM.CDOInquiry
            {
                SelectedInstances = (from s in selectedInstances select new OM.BaseObjectRef(s.InstanceID)).ToArray(),
                Recursive = false,
                FilterReferences = selectedInstancesOnly
            };
            if (inquiry.SelectedInstances == null || inquiry.SelectedInstances.Length == 0)
            {
                LogMessage(LogSeverity.Error, "GetReferences", $"SelectedInstances is NULL");
                return null;
            }
            var info = new OM.CDOInquiry_Info()
            {
                ObjectReferencesList = new OM.ObjectReferencesInfo_Info { RequestValue = true }
            };
            var request = new WCF.Services.CDOInquiry_Request { Info = info };
            CDOInquiry_Result result;
            LogMessage(LogSeverity.Error, "GetReferences", $"Start GetReferences for {inquiry.SelectedInstances.Length} items...");
            var resultStatus = service.GetReferences(inquiry, request, out result);
            LogMessage(LogSeverity.Error, "GetReferences", $"End   GetReferences for {inquiry.SelectedInstances.Length} items...");
            if (IsErrorCheck(resultStatus) && result.Value != null && result.Value.ObjectReferencesList != null)
            {
                return result.Value.ObjectReferencesList;
            }
            return null;
        }


        protected virtual OM.ExportImportItem[] ConvertSelectedToExportItems(SelectedInstanceItem[] selectedItems)
        {
            if (selectedItems != null)
            {
                var ds = selectedItems.OrderBy(dx => dx.Order);
                return
                    (from p in ds
                     select new OM.ExportImportItem
                     {
                         ListItemAction = OM.ListItemAction.Add,
                         ObjectType = new OM.Enumeration<OM.ExportableObjectEnum, string>(p.CDOTypeID),
                         ObjectInstanceId = p.InstanceID
                     }).ToArray();
            }
            return null;
        }

        protected virtual bool IsErrorCheck(OM.ResultStatus resultStatus)
        {
            if (resultStatus == null)
                throw new Exception("ResultStatus is NULL!");
            else if (!resultStatus.IsSuccess)
            {
                if (resultStatus.ExceptionData != null)
                {
                    LogMessage(LogSeverity.Error, "IsErrorCheck", $"ExceptionData: {resultStatus.ExceptionData.Description}");
                    throw new Exception(resultStatus.ExceptionData.Description);
                }
                else if (!string.IsNullOrEmpty(resultStatus.Message))
                {
                    LogMessage(LogSeverity.Error, "IsErrorCheck", $"Message: {resultStatus.Message}");
                    throw new Exception(resultStatus.Message);
                }
                else
                    LogMessage(LogSeverity.Error, "IsErrorCheck", $"Error: {resultStatus.ToString()}");
            }
            return true;
        }

        private OM.UserProfile _userProfile;
    }

    public class ImportFile
    {
        private string _filePath;
        private string _noImportExists = FrameworkManagerUtil.GetLabelValue("ImportSetNameDNELbl") ?? "Import Set Name does not exist";
        public string SetName;
        public WCF.ObjectStack.Import Import;

        public ImportFile(string filePath)
        {
            _filePath = filePath;

            SetImportFile();
        }

        protected virtual void SetImportFile()
        {
            var importSetName = System.IO.Path.GetFileNameWithoutExtension(_filePath);

            // Check if import set has been previously imported
            SetName = importSetName;
            var res = CheckImportSetStatus();
            if (res.IsSuccess)  /* Result is successful if import has been imported before */
            {
                // Append set name to create unique name
                var dateStr = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss").Replace(":", "-");
                importSetName += "_" + dateStr;
            }

            var controlIn = new WCF.ObjectStack.ExportImportController()
            {
                ExportImportFileURL = _filePath,
                ExportImportType = WCF.ObjectStack.ExportImportTypeEnum.ImportSet,
                ImportSetName = importSetName
            };
            var controlRequest = new ExportImportController_Request() { Info = new WCF.ObjectStack.ExportImportController_Info() { RequestValue = true } };
            var controlResult = new ExportImportController_Result();

            var session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
            var contServ = new ExportImportControllerService(session.CurrentUserProfile);
            contServ.BeginTransaction();
            contServ.SetImportFile(controlIn, controlRequest, out controlResult);
            contServ.ExecuteTransaction();
            contServ.CommitTransaction();
            SetName = importSetName;
        }

        public virtual WCF.ObjectStack.Import GetImportData()
        {
            var importStatus = CheckImportSetStatus();

            if (importStatus.IsSuccess)
            {
                var session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
                var data = new WCF.ObjectStack.Import() { ImportSetName = SetName, ImportIfExists = false };
                var importServ = new ImportService(session.CurrentUserProfile);
                var importRequest = new Import_Request()
                {
                    Info = new WCF.ObjectStack.Import_Info()
                    {
                        RequestValue = true,
                        ImportContents = new WCF.ObjectStack.ImportContent_Info()
                        {
                            RequestValue = true
                        }
                    }
                };
                var importResult = new Import_Result();
                var results = importServ.GetImportContents(data, importRequest, out importResult);
                if (IsErrorCheck(results))
                {
                    Import = importResult.Value;
                    return importResult.Value;
                }
            }
            return null;
        }

        protected virtual OM.ResultStatus CheckImportSetStatus()
        {
            var session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
            var inquiry = new WCF.ObjectStack.ImportStatusInquiry() { ImportSetName = SetName };
            var service = new ImportStatusInquiryService(session.CurrentUserProfile);
            var request = new ImportStatusInquiry_Request
            {
                Info = new WCF.ObjectStack.ImportStatusInquiry_Info()
                {
                    RequestValue = true,
                    ImportSetStatus = FieldInfoUtil.RequestValue()
                }
            };

            var result = new ImportStatusInquiry_Result();
            var resultStatus = service.GetImportSetStatus(inquiry, request, out result);

            if (IsErrorCheck(resultStatus) && result != null && result.Value != null)
            {
                var importSetStatus = ((WCF.ObjectStack.ImportStatusInquiry)result.Value).ImportSetStatus;

                if (importSetStatus == null)
                {
                    return new OM.ResultStatus(_noImportExists, false);
                }

                switch (importSetStatus.Value)
                {
                    case 3: //Failure
                        throw new Exception(resultStatus.Message);
                    case 2: //Completed
                        return resultStatus;
                    default: //In Progress
                        System.Threading.Thread.Sleep(3000);
                        return CheckImportSetStatus();
                }
            }
            return resultStatus;
        }

        protected virtual bool IsErrorCheck(OM.ResultStatus resultStatus)
        {
            if (!resultStatus.IsSuccess)
                throw new Exception(resultStatus.ExceptionData.Description);
            return true;
        }

    }
}
