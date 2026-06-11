/* Copyright 2019 Siemens */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Text;
using System.Collections;

using Camstar.WCF.ObjectStack;
using Camstar.WCF.Services;
using Camstar.WebPortal.Utilities;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.Constants;
using Camstar.WebPortal.WCFUtilities;
using Camstar.WebPortal.Personalization;

/// <summary>
/// Summary description for SchedulingTxn
/// </summary>

namespace SEMI.AppCode.Services
{
    public class SchedulingTxn
    {
        //-----------------------------------------
        //
        //-----------------------------------------
        public static void GetServiceTypes(string ObjectType, ref string LotScheduleServiceType, ref string LotScheduleModifyServiceType, ref string LotSchedulePreparationServiceType, ref string LotScheduleReleaseServiceType, ref string LotScheduleRequestType, ref string LotScheduleRequestCancelType, ref string LotScheduleCancelType)
        {
            try
            {
                switch (ObjectType)
                {
                    case "WAFER":
                        LotScheduleServiceType = "WaferLotSchedule";
                        LotScheduleModifyServiceType = "WaferLotScheModify";
                        LotSchedulePreparationServiceType = "WaferLotSchePrep";
                        LotScheduleCancelType = "WaferLotScheCancel";
                        LotScheduleReleaseServiceType = "WaferLotScheRelease";
                        LotScheduleRequestType = "WaferLotScheRequest";
                        LotScheduleRequestCancelType = "WaferLotScheRequestCancel";
                        break;
                    case "WAFERSORT":
                        LotScheduleServiceType = "WaferSortLotSchedule";
                        LotScheduleModifyServiceType = "WaferSortLotScheModify";
                        LotSchedulePreparationServiceType = "WaferSortLotSchePrep";
                        LotScheduleCancelType = "WaferSortLotScheCancel";
                        LotScheduleReleaseServiceType = "WaferSortLotScheRelease";
                        LotScheduleRequestType = "WaferSortLotScheRequest";
                        LotScheduleRequestCancelType = "WaferSortLotScheRequestCancel";
                        break;
                    case "BACKGRIND":
                        LotScheduleServiceType = "BackGrindLotSchedule";
                        LotScheduleModifyServiceType = "BackGrindLotScheModify";
                        LotSchedulePreparationServiceType = "BackGrindLotSchePrep";
                        LotScheduleCancelType = "BackGrindLotScheCancel";
                        LotScheduleReleaseServiceType = "BackGrindLotScheRelease";
                        break;
                    case "ASSEMBLY":
                        LotScheduleServiceType = "AssemblyLotSchedule";
                        LotScheduleModifyServiceType = "AssemblyLotScheModify";
                        LotSchedulePreparationServiceType = "AssemblyLotSchePrep";
                        LotScheduleCancelType = "AssemblyLotScheCancel";
                        LotScheduleReleaseServiceType = "AssemblyLotScheRelease";
                        break;
                    case "ASSEMBLYCARRIER":
                        LotScheduleServiceType = "AssemblyCarrierLotSchedule";
                        LotScheduleModifyServiceType = "AssemblyCarrierLotScheModify";
                        LotSchedulePreparationServiceType = "AssemblyCarrierLotSchePrep";
                        LotScheduleCancelType = "AssemblyCarrierLotScheCancel";
                        LotScheduleReleaseServiceType = "AssemblyCarrierLotScheRelease";
                        break;
                    case "ASSEMBLYMOTHERLOT":
                        LotScheduleServiceType = "AssemblyMotherLotSchedule";
                        LotScheduleModifyServiceType = "AssemblyMotherLotScheModify";
                        LotSchedulePreparationServiceType = "AssemblyMotherLotSchePrep";
                        LotScheduleCancelType = "AssemblyMotherLotScheCancel";
                        LotScheduleReleaseServiceType = "AssemblyMotherLotScheRelease";
                        break;
                    case "ASSEMBLYSUBLOT":
                        LotScheduleServiceType = "AssemblySubLotSchedule";
                        LotScheduleModifyServiceType = "AssemblySubLotScheModify";
                        LotSchedulePreparationServiceType = "AssemblySubLotSchePrep";
                        LotScheduleCancelType = "AssemblySubLotScheCancel";
                        LotScheduleReleaseServiceType = "AssemblySubLotScheRelease";
                        break;
                    case "TEST":
                        LotScheduleServiceType = "TestLotSchedule";
                        LotScheduleModifyServiceType = "TestLotScheModify";
                        LotSchedulePreparationServiceType = "TestLotSchePrep";
                        LotScheduleCancelType = "TestLotScheCancel";
                        LotScheduleReleaseServiceType = "TestLotScheRelease";
                        LotScheduleRequestType = "TestLotScheRequest";
                        LotScheduleRequestCancelType = "TestLotScheRequestCancel";
                        break;
                    case "FINALTEST":
                        LotScheduleServiceType = "FinalTestLotSchedule";
                        LotScheduleModifyServiceType = "FinalTestLotScheModify";
                        LotSchedulePreparationServiceType = "FinalTestLotSchePrep";
                        LotScheduleCancelType = "FinalTestLotScheCancel";
                        LotScheduleReleaseServiceType = "FinalTestLotScheRelease";
                        LotScheduleRequestType = "FinalTestLotScheRequest";
                        LotScheduleRequestCancelType = "FinalTestLotScheRequestCancel";
                        break;
                    default:
                        LotScheduleServiceType = "LotSchedule";
                        LotScheduleModifyServiceType = "LotScheduleModify";
                        LotSchedulePreparationServiceType = "LotSchedulePreparation";
                        LotScheduleCancelType = "LotScheduleCancel";
                        LotScheduleReleaseServiceType = "LotScheduleRelease";
                        LotScheduleRequestType = "LotScheduleRequest";
                        LotScheduleRequestCancelType = "LotScheduleRequestCancel";
                        break;
                }
            }
            catch (Exception Ex)
            {
                throw new Exception(Ex.TargetSite.Name + "(): " + Ex.Message);
            }
        } // GetServiceTypes
    }
}



