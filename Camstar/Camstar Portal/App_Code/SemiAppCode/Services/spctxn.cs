/* Copyright 2019 Siemens */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Text;
using System.Web;
using System.Web.UI;

using Camstar.WCF.ObjectStack;
using Camstar.WCF.Services;
using Camstar.WebPortal.Utilities;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using OM = Camstar.WCF.ObjectStack;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.Constants;
using Camstar.WebPortal.WCFUtilities;
using Camstar.WebPortal.Personalization;

namespace SEMI.AppCode.Services
{
    public class SPCTxn
    {
        //-----------------------------------------
        //
        //-----------------------------------------
        public class SPCTxnDataPoint
        {
            private string __SPCTxnDataName;
            private string __DataPointID;
            private string __DataPointName;

            public string SPCTxnDataName
            {
                get { return __SPCTxnDataName; }
                set { __SPCTxnDataName = value; }
            }

            public string DataPointName
            {
                get { return __DataPointName; }
                set { __DataPointName = value; }
            }

            public string DataPointID
            {
                get { return __DataPointID; }
                set { __DataPointID = value; }
            }
        }

        //-----------------------------------------
        //
        //-----------------------------------------
        public class CustomSPCTxnData
        {
            private string __SPCTxnDataName;
            private string __SPCSetupName;
            private string __SPCResult;
            private List<SPCTxnDataPoint> __SPCTxnDataPointList = new List<SPCTxnDataPoint>();

            public string SPCTxnDataName
            {
                get { return __SPCTxnDataName; }
                set { __SPCTxnDataName = value; }
            }

            public string SPCSetupName
            {
                get { return __SPCSetupName; }
                set { __SPCSetupName = value; }
            }

            public List<SPCTxnDataPoint> SPCTxnDataPointList
            {
                get { return __SPCTxnDataPointList; }
            }

            public string SPCResult
            {
                get { return __SPCResult; }
                set { __SPCResult = value; }
            }

            //-----------------------------------------
            //
            //-----------------------------------------
            public bool AddTxnDataPointListItem(SPCTxnDataPoint DataPoint)
            {
                if (__SPCTxnDataPointList != null)
                    this.__SPCTxnDataPointList.Add(DataPoint);
                return true;
            }

            //-----------------------------------------
            //
            //-----------------------------------------
            public bool AddTxnDataPointListItem(string DataPointName, string DataPointID, string SPCTxnDataName)
            {
                SPCTxnDataPoint _dp = new SPCTxnDataPoint();
                _dp.DataPointID = DataPointID;
                _dp.DataPointName = DataPointName;
                _dp.SPCTxnDataName = SPCTxnDataName;

                if (__SPCTxnDataPointList != null)
                    this.__SPCTxnDataPointList.Add(_dp);
                return true;
            }
        } // CustomSPCTxnData

        //-----------------------------------------
        //
        //-----------------------------------------
        public class SPCTxnDataSet
        {
            private List<SPCTxnData> __DataList = new List<SPCTxnData>();

            public List<SPCTxnData> DataList
            {
                get { return __DataList; }
            }
            public int Count
            {
                get { return __DataList.Count; }
            }

            //-----------------------------------------
            //
            //-----------------------------------------
            public bool AddDataListItem(SPCTxnData inputTxnData)
            {
                if (__DataList != null)
                    this.__DataList.Add(inputTxnData);
                return true;
            }

            //-----------------------------------------
            //
            //-----------------------------------------
            public bool AddDataListItem(string SPCTxnDataName, string SPCSetup, string SPCResult, string SPCResultFilename, int ChartHeight, int ChartWidth, string FailureDocSet)
            {
                SPCTxnData newTxnData = new SPCTxnData();
                newTxnData.Name = SPCTxnDataName;
                newTxnData.SPCSetup = new NamedObjectRef(SPCSetup);
                newTxnData.SPCResult = SPCSetup;
                newTxnData.SPCResultFilename = SPCResultFilename;
                newTxnData.ChartHeight = ChartHeight;
                newTxnData.ChartWidth = ChartWidth;
                newTxnData.FailureDocumentSet = new NamedObjectRef(FailureDocSet);

                if (__DataList != null)
                    this.__DataList.Add(newTxnData);

                return true;
            }
        } // SPCTxnDataSet

        //-----------------------------------------
        //
        //-----------------------------------------
        public static bool LoadSPCTxnDataDetails(string strSPCTxnDataName, ref SPCTxnData oSPCTxnData)
        {
            SPCTxnDataMaint inputData = new SPCTxnDataMaint();
            inputData.ObjectToChange = new NamedObjectRef();
            inputData.ObjectToChange.Name = strSPCTxnDataName;

            SPCTxnDataMaint_Info info = new SPCTxnDataMaint_Info
            {
                ObjectChanges = new SPCTxnDataChanges_Info
                {
                    Name = FieldInfoUtil.RequestValue(),
                    SPCSetup = FieldInfoUtil.RequestValue(),
                    FailureDocumentSet = FieldInfoUtil.RequestValue(),
                    SPCResultFilename = FieldInfoUtil.RequestValue(),
                    SPCResult = FieldInfoUtil.RequestValue(),
                    ChartHeight = FieldInfoUtil.RequestValue(),
                    ChartWidth = FieldInfoUtil.RequestValue()
                }
            };

            UserProfile profile = HttpContext.Current.Session[Camstar.WebPortal.Constants.SessionConstants.UserProfile] as UserProfile;
            SPCTxnDataMaintService svc = new SPCTxnDataMaintService(profile);
            SPCTxnDataMaint_Result result = null;
            ResultStatus resultStatus = svc.Load(inputData, new SPCTxnDataMaint_Request { Info = info }, out result);

            if (resultStatus.IsSuccess)
            {
                SPCTxnDataChanges objectChanges = result.Value.ObjectChanges as SPCTxnDataChanges;
                oSPCTxnData.Name = objectChanges.Name;
                oSPCTxnData.SPCResult = objectChanges.SPCResult;
                oSPCTxnData.SPCSetup = objectChanges.SPCSetup;
                oSPCTxnData.SPCResultFilename = objectChanges.SPCResultFilename;
                oSPCTxnData.ChartHeight = objectChanges.ChartHeight;
                oSPCTxnData.ChartWidth = objectChanges.ChartWidth;
            }

            return true;
        }

        //-----------------------------------------
        //
        //-----------------------------------------
        public static bool PopupSPCChart(Camstar.WebPortal.WebPortlets.MatrixWebPart RefPage, UIComponentDataContractLink[] Links, UIComponentDataContractReturnLink[] ReturnLinks, int PoupWidth = 750, int PopupHeight = 580)
        {
            Camstar.WebPortal.Personalization.FloatPageOpenAction objAction = new FloatPageOpenAction();
            objAction.PageName = "SS_SPCChartsPopupVP";

            objAction.DataContractMap = new UIComponentDataContractMap();
            objAction.DataContractMap.Links = Links;            

            objAction.DataContractReturnMap = new UIComponentDataContractReturnMap();
            objAction.DataContractReturnMap.ReturnLinks = ReturnLinks;

            objAction.FrameLocation = new UIFloatingPageLocation();
            objAction.FrameLocation.Width = PoupWidth;
            objAction.FrameLocation.Height = PopupHeight;

            objAction.EndResponse = false;

            RefPage.Page.ActionDispatcher.ExecuteAction(objAction);
            return true;
        } // PopupSPCChart
    }
}




