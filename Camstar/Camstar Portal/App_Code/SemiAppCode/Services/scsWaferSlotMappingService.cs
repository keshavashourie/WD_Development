/* Copyright 2020 Siemens */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using System.Text.RegularExpressions;
using Camstar.WebPortal.FormsFramework;
using Camstar.WCF.ObjectStack;

public class scsWaferSlotMappingService
{
    private List<AssignementModel> WafersList, SlotMapsList, ExtendedList;
    private SlotMapDetails[] SlotMapDetails;
    private JQDataGrid Wafers, SlotMapsDetails;
    private bool IsLotAssignedSlotMap, WaferSlotExist = false;
    private int[] existedList;
    private string SlotRowID = String.Empty, inputLot = String.Empty;

    private ObjectValueHandler objectHandler;

    private Regex regex = new Regex(@"\(([^\}]+)\)");
    public scsWaferSlotMappingService() { /*EMPTY CONSTRUCTOR*/ }

    public scsWaferSlotMappingService(JQDataGrid _gridWafersField, JQDataGrid _gridSlotMapsDetailsField, bool _chkboxIsLotAssignedSlotMap)
    { this.Wafers = _gridWafersField; this.SlotMapsDetails = _gridSlotMapsDetailsField; this.IsLotAssignedSlotMap = _chkboxIsLotAssignedSlotMap; }


    public scsWaferSlotMappingService(List<AssignementModel> _wafersList, SlotMapDetails[] _slotMapDetails)
    {
        this.WafersList = _wafersList;
        this.SlotMapDetails = _slotMapDetails;
    }

    class SortingPriority : IComparer<AssignementModel>
    {
        public int Compare(AssignementModel WaferA, AssignementModel WaferB)
        {
            if (!WaferA.WaferScribeNumber.Equals(WaferB.WaferScribeNumber))
                return WaferA.WaferScribeNumber.CompareTo(WaferB.WaferScribeNumber);
            else if (!WaferA.Lot.Equals(WaferB.Lot))
                return WaferA.Lot.CompareTo(WaferB.Lot);
            else
                return WaferA.WaferNumber.CompareTo(WaferB.WaferNumber);
        }
    }

    public string AssignWafersToSlots(string AssignmentMethod)
    {
        try
        {
            if (!IsLotAssignedSlotMap)
            {
                if ((Wafers.GridContext.GetTotalRows() != 0) && (SlotMapsDetails.GridContext.GetTotalRows() != 0))
                {
                    int countSizeOfSlot = CountSlot(); // Counts the number of slots
                    WafersList = new List<AssignementModel>(); // Only accepts data from wafer grid
                    SlotMapsList = new List<AssignementModel>(); // Only accepts data from slot map grid
                    ExtendedList = new List<AssignementModel>(); // Combines both Wafers and SlotMap Lists

                    string Method = AssignmentMethod.Substring(3); // Extracts info from a single value

                    objectHandler = GetWafersFromGrid(countSizeOfSlot);
                    if (objectHandler.isNotAvailable) return objectHandler.msg;
                    else
                    {
                        WafersList = objectHandler.list; // Get wafer list from grid

                        // Ascending & Descending all Operation
                        if (Method.Equals("all"))
                        {
                            SlotMapsList = GetExisitingWaferFromSlot("all");  // Get Existing value from _gridSlotMapsDetailsField
                            ExtendedList = SlotMapsList.Concat(WafersList).ToList(); // Using concatination technique to combine both grid's extracted list

                            if (ExtendedList.Count > 0)
                            {
                                ExtendedList.Sort(new SortingPriority()); // Sort according WaferScribeID (1st priority)
                            }

                            clearGrid();
                            PopulateValuesIntoGrid(AssignmentMethod, ExtendedList);
                        }

                        if (Method.Equals("fill"))
                        {
                            WafersList.Sort(new SortingPriority()); // Sort according WaferScribeID (1st priority)                          
                            SlotMapsList = GetExisitingWaferFromSlot("fill");  // Get Existing value from _gridSlotMapsDetailsField                           
                            clearGrid();
                            PopulateValuesIntoGrid(AssignmentMethod, WafersList, SlotMapsList);
                        }
                    }
                }
            }
        }
        catch (Exception ex) { }

        return null;
    }

    private bool IsWaferAssigned(SlotMapDetails oSlotMapDetail)
    {
        foreach (AssignementModel wafer in WafersList)
        {
            if (wafer.WaferNumber.Equals(oSlotMapDetail.WaferNumber.Value.ToString()) &&
                wafer.WaferScribeNumber.Equals(oSlotMapDetail.WaferScribeNumber.ToString()))
                return true;
        }
        return false;
    }

    public void AssignWafersToSlots(string AssignmentMethod, out SlotMapDetails[] oSlotMapDetail)
    {
        oSlotMapDetail = SlotMapDetails;
        if (!IsLotAssignedSlotMap)
        {
            if ((WafersList.Count > 0) && (SlotMapDetails.Length > 0))
            {
                string Method = AssignmentMethod.Substring(3); // Extracts info from a single value

                // Ascending & Descending all Operation
                if (Method.Equals("all"))
                {
                    bool IsSlotMapEmpty = true;
                    int numberOfSlotAvailable = 0;
                    for (int i = 0; i < oSlotMapDetail.Length; i++)
                    {
                        if (oSlotMapDetail[i].Status != null && oSlotMapDetail[i].Status.ToString() == "DOWN") continue;
                        numberOfSlotAvailable++;
                        if ((oSlotMapDetail[i].WaferNumber != null) && (!oSlotMapDetail[i].WaferNumber.ToString().Equals("")))
                        {
                            IsSlotMapEmpty = false;
                            if (!IsWaferAssigned(oSlotMapDetail[i]))
                            {
                                WafersList.Add(new AssignementModel(oSlotMapDetail[i].WaferNumber.ToString(),
                                    oSlotMapDetail[i].WaferScribeNumber.ToString().ToString(),
                                    oSlotMapDetail[i].Lot.ToString()));
                            }
                        }
                    }

                    if (WafersList.Count > numberOfSlotAvailable)
                    {
                        throw new Exception("The slot map has not enough slot for all wafers, either none or partial wafers are assigned.");
                    }

                    if (WafersList.GroupBy(x => x.WaferScribeNumber).Any(g => g.Count() > 1))
                    {
                        throw new Exception("Cannot assign duplicate wafers into a same carrier.");
                    }

                    WafersList.Sort(new SortingPriority());

                    if (AssignmentMethod.Equals("dscall")) WafersList.Reverse();

                    if (!IsSlotMapEmpty)
                    {
                        for (int i = 0; i < oSlotMapDetail.Length; i++)
                        {
                            if (oSlotMapDetail[i].Status != null && oSlotMapDetail[i].Status.ToString() != "UP") continue;
                            oSlotMapDetail[i].WaferNumber = "";
                            oSlotMapDetail[i].WaferScribeNumber = "";
                            oSlotMapDetail[i].Lot = null;
                        }
                    }
                    int j = 0;
                    for (int i = 0; i < oSlotMapDetail.Length; i++)
                    {
                        if (j >= WafersList.Count) break;
                        if (oSlotMapDetail[i].Status != null && oSlotMapDetail[i].Status.ToString() != "UP") continue;
                        oSlotMapDetail[i].WaferNumber = WafersList[j].WaferNumber;
                        oSlotMapDetail[i].WaferScribeNumber = WafersList[j].WaferScribeNumber;
                        oSlotMapDetail[i].Lot = new ContainerRef(WafersList[j].Lot);
                        j++;
                    }
                }

                if (Method.Equals("fill"))
                {
                    int numberOfSlotAvailable = 0;
                    for (int i = 0; i < oSlotMapDetail.Length; i++)
                    {
                        if (oSlotMapDetail[i].Status != null && oSlotMapDetail[i].Status.ToString() == "DOWN") continue;
                        numberOfSlotAvailable++;
                        if ((oSlotMapDetail[i].WaferNumber != null) && (!oSlotMapDetail[i].WaferNumber.ToString().Equals("")))
                        {
                            if (IsWaferAssigned(oSlotMapDetail[i]))
                            {
                                //throw new Exception("Wafer is already assigned to a slot.");
                            }
                        }
                    }

                    int counter = 0;
                    for (int i = 0; i < oSlotMapDetail.Length; i++)
                    {
                        if (oSlotMapDetail[i].WaferNumber != null && !oSlotMapDetail[i].WaferNumber.ToString().Equals("")) counter++;
                    }

                    if ((WafersList.Count + counter) > numberOfSlotAvailable)
                    {
                        throw new Exception("The slot map has not enough slot for all wafers, either none or partial wafers are assigned.");
                    }

                    if (WafersList.GroupBy(x => x.WaferScribeNumber).Any(g => g.Count() > 1))
                    {
                        throw new Exception("Cannot assign duplicate wafers into a same carrier.");
                    }

                    WafersList.Sort(new SortingPriority());

                    if (AssignmentMethod.Equals("dscfill")) WafersList.Reverse();

                    int j = 0;
                    for (int i = 0; i < oSlotMapDetail.Length; i++)
                    {
                        if (j >= WafersList.Count) break;
                        if (oSlotMapDetail[i].Status != null && oSlotMapDetail[i].Status.ToString() != "UP" || oSlotMapDetail[i].WaferNumber != null && !oSlotMapDetail[i].WaferNumber.ToString().Equals("")) continue;
                        oSlotMapDetail[i].WaferNumber = WafersList[j].WaferNumber;
                        oSlotMapDetail[i].WaferScribeNumber = WafersList[j].WaferScribeNumber;
                        oSlotMapDetail[i].Lot = new ContainerRef(WafersList[j].Lot);
                        j++;
                    }
                }
            }
        }
    }

    private ObjectValueHandler GetWafersFromGrid(int countSizeOfSlot)
    {
        List<AssignementModel> list = new List<AssignementModel>();
        int counter = 0, internalCounter = 0; bool LotIsNotAvailable = false;
        string msg = String.Empty;

        // Getting Wafer Details
        for (int i = 0; i < Wafers.GridContext.GetTotalRows(); i++)
        {
            SlotRowID = IdGenerator(i);
            inputLot = Wafers.GridContext.GetCell(SlotRowID, "Container").ToString();
            inputLot = regex.Replace(inputLot, "");

            if (Wafers.GridContext.GetCell(SlotRowID, "Grade").ToString() != "Add")
            {
                if (!isWaferExisted(Wafers.GridContext.GetCell(SlotRowID, "WaferScribeNumber").ToString()))
                {
                    list.Add(new AssignementModel(
                        Wafers.GridContext.GetCell(SlotRowID, "WaferNumber").ToString(),
                        Wafers.GridContext.GetCell(SlotRowID, "WaferScribeNumber").ToString(), inputLot));
                    Wafers.GridContext.SetCell(SlotRowID, "Grade", "Add");
                }
            }
        }

        if (LotIsNotAvailable) { msg = "No Lots are available on ToCarrier List"; }
        ObjectValueHandler obj = new ObjectValueHandler(list, msg, LotIsNotAvailable);

        return obj;
    }

    private Boolean isWaferExisted(string waferScribeNumber)
    {
        for (int j = 0; j < SlotMapsDetails.GridContext.GetTotalRows(); j++)
        {
            string sSlotRowID = IdGenerator(j);
            if (SlotMapsDetails.GridContext.GetCell(sSlotRowID, "WaferScribeNumber") != null && SlotMapsDetails.GridContext.GetCell(sSlotRowID, "WaferScribeNumber").ToString() == waferScribeNumber)
            {
                return true;
            }
        }
        return false;
    }

    private void PopulateValuesIntoGrid(string assignmentMethod, List<AssignementModel> wafersList, List<AssignementModel> slotMapsList)
    {
        int counter = 0, internalCounter = 0;
        // Descending fill
        if (assignmentMethod.Equals("dscfill")) wafersList.Reverse();

        foreach (var v in slotMapsList)
        {
            inputLot = v.Lot;
            inputLot = regex.Replace(inputLot, "");

            SlotRowID = IdGenerator(v.SlotNumber);
            SlotMapsDetails.GridContext.SetCell(SlotRowID, "WaferNumber", v.WaferNumber);
            SlotMapsDetails.GridContext.SetCell(SlotRowID, "WaferScribeNumber", v.WaferScribeNumber);
            SlotMapsDetails.GridContext.SetCell(SlotRowID, "Lot", inputLot);
            CamstarWebControl.SetRenderToClient(Wafers);
            CamstarWebControl.SetRenderToClient(SlotMapsDetails);
        }

        while (counter < SlotMapsDetails.GridContext.GetTotalRows())
        {
            SlotRowID = IdGenerator(counter);
            if ((SlotMapsDetails.GridContext.GetCell(SlotRowID, "Status").ToString() == "UP") &&
                    ((SlotMapsDetails.GridContext.GetCell(SlotRowID, "WaferNumber") == null) ||
                    (SlotMapsDetails.GridContext.GetCell(SlotRowID, "WaferNumber").ToString().Equals(""))))
            {
                if (internalCounter < wafersList.Count())
                {
                    SlotMapsDetails.GridContext.SetCell(SlotRowID, "WaferNumber", wafersList[internalCounter].WaferNumber);
                    SlotMapsDetails.GridContext.SetCell(SlotRowID, "WaferScribeNumber", wafersList[internalCounter].WaferScribeNumber);
                    SlotMapsDetails.GridContext.SetCell(SlotRowID, "Lot", wafersList[internalCounter].Lot);
                    internalCounter++;
                }
                else { break; }

            }
            counter++;
            CamstarWebControl.SetRenderToClient(Wafers);
            CamstarWebControl.SetRenderToClient(SlotMapsDetails);
        }

    }

    private void PopulateValuesIntoGrid(string assignmentMethod, List<AssignementModel> extendedList)
    {
        // Descending all
        if (assignmentMethod.Equals("dscall")) extendedList.Reverse();

        int counter = 0;
        for (int i = 0; i < SlotMapsDetails.GridContext.GetTotalRows(); i++)
        {
            if (counter >= extendedList.Count) break;
            SlotRowID = IdGenerator(i);
            if (SlotMapsDetails.GridContext.GetCell(SlotRowID, "Status").ToString() == "DOWN") continue;
            inputLot = extendedList[counter].Lot;
            inputLot = regex.Replace(inputLot, "");
            SlotMapsDetails.GridContext.SetCell(SlotRowID, "WaferNumber", extendedList[counter].WaferNumber);
            SlotMapsDetails.GridContext.SetCell(SlotRowID, "WaferScribeNumber", extendedList[counter].WaferScribeNumber);
            SlotMapsDetails.GridContext.SetCell(SlotRowID, "Lot", inputLot);
            counter++;
        }
        CamstarWebControl.SetRenderToClient(SlotMapsDetails);
    }

    private List<AssignementModel> GetExisitingWaferFromSlot(string type)
    {
        List<AssignementModel> list = new List<AssignementModel>();

        // Slot maps for ascending & descending all
        if (type.Equals("all"))
        {
            for (int i = 0; i < SlotMapsDetails.GridContext.GetTotalRows(); i++)
            {
                SlotRowID = IdGenerator(i);
                if ((SlotMapsDetails.GridContext.GetCell(SlotRowID, "Status").ToString() == "UP") &&
                    (SlotMapsDetails.GridContext.GetCell(SlotRowID, "WaferNumber") != null) &&
                    (!SlotMapsDetails.GridContext.GetCell(SlotRowID, "WaferNumber").ToString().Equals("")))
                    list.Add(new AssignementModel(SlotMapsDetails.GridContext.GetCell(SlotRowID, "WaferNumber").ToString(),
                        SlotMapsDetails.GridContext.GetCell(SlotRowID, "WaferScribeNumber").ToString(),
                        SlotMapsDetails.GridContext.GetCell(SlotRowID, "Lot").ToString()));
            }
        }

        // Slot maps for ascending & descending fill
        if (type.Equals("fill"))
        {
            for (int i = 0; i < SlotMapsDetails.GridContext.GetTotalRows(); i++)
            {
                SlotRowID = IdGenerator(i);
                if ((SlotMapsDetails.GridContext.GetCell(SlotRowID, "WaferNumber") != null) && (SlotMapsDetails.GridContext.GetCell(SlotRowID, "WaferNumber").ToString().Length != 0))
                    list.Add(new AssignementModel(SlotMapsDetails.GridContext.GetCell(SlotRowID, "WaferNumber").ToString(),
                        SlotMapsDetails.GridContext.GetCell(SlotRowID, "WaferScribeNumber").ToString(),
                        SlotMapsDetails.GridContext.GetCell(SlotRowID, "Lot").ToString(), i));
            }
        }

        return list;
    }

    private string IdGenerator(int num) { return num.ToString().PadLeft(6, '0'); }

    private int CountSlot()
    {
        int index = 0;
        for (int i = 0; i < SlotMapsDetails.GridContext.GetTotalRows(); i++)
        {
            SlotRowID = IdGenerator(i);
            if (SlotMapsDetails.GridContext.GetCell(SlotRowID, "Status").ToString() == "UP" &&
                (SlotMapsDetails.GridContext.GetCell(SlotRowID, "WaferNumber") == null || SlotMapsDetails.GridContext.GetCell(SlotRowID, "WaferNumber").ToString() == ""))
                index++;
        }

        return index;
    }

    // Grid value cleaning method
    private void clearGrid()
    {
        for (int i = 0; i < SlotMapsDetails.GridContext.GetTotalRows(); i++)
        {
            SlotRowID = IdGenerator(i);
            SlotMapsDetails.GridContext.SetCell(SlotRowID, "WaferNumber", "");
            SlotMapsDetails.GridContext.SetCell(SlotRowID, "WaferScribeNumber", "");
            SlotMapsDetails.GridContext.SetCell(SlotRowID, "Lot", "");
        }
    }

    public class AssignementModel
    {
        public string WaferNumber { set; get; }
        public string WaferScribeNumber { set; get; }
        public string Lot { set; get; }
        public int SlotNumber { set; get; }

        public AssignementModel(string _waferNumber, string _waferScribeNumber, string _lot)
        {
            this.WaferNumber = _waferNumber;
            this.WaferScribeNumber = _waferScribeNumber;
            this.Lot = _lot;
        }

        public AssignementModel(string _waferNumber, string _waferScribeNumber, string _lot, int _slotNumber)
        {
            this.WaferNumber = _waferNumber;
            this.WaferScribeNumber = _waferScribeNumber;
            this.Lot = _lot;
            this.SlotNumber = _slotNumber;
        }
    }

    class ObjectValueHandler
    {
        public List<AssignementModel> list { set; get; }
        public string msg { set; get; }
        public bool isNotAvailable { set; get; }

        public ObjectValueHandler(List<AssignementModel> _list, string _msg, bool _isNotAvailable)
        { this.list = _list; this.msg = _msg; this.isNotAvailable = _isNotAvailable; }
    }
}