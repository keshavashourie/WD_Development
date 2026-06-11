/* Copyright 2019 Siemens */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for DataPacket
/// </summary>
/// 

namespace SEMI.AppCode
{
    public class DataPacket
    {
        private SEMI.AppCode.Services.SPCTxn.SPCTxnDataSet __SPCTxnDataSet = new SEMI.AppCode.Services.SPCTxn.SPCTxnDataSet();
        private string __ResultStatusMessage = "";
        private bool __IsErrorResultStatusMessage = false;
        private string[] __AlertMessages;
        private bool __IsAlertMessageAvailable = false;

        //-----------------------------------------
        //
        //-----------------------------------------
        public SEMI.AppCode.Services.SPCTxn.SPCTxnDataSet SPCTxnDataSet
        {
            get { return __SPCTxnDataSet; }
            set { __SPCTxnDataSet = value; }
        }

        //-----------------------------------------
        //
        //-----------------------------------------
        public string ResultStatusMessage
        {
            get { return __ResultStatusMessage; }
            set { __ResultStatusMessage = value; }
        }

        //-----------------------------------------
        //
        //-----------------------------------------
        public string[] AlertMessages
        {
            get { return __AlertMessages; }
            set
            {
                __AlertMessages = value;
                __IsAlertMessageAvailable = true;
            }
        }

        //-----------------------------------------
        //
        //-----------------------------------------
        public bool IsErrorResultStatusMessage
        {
            get { return __IsErrorResultStatusMessage; }
            set { __IsErrorResultStatusMessage = value; }
        }

        //-----------------------------------------
        //
        //-----------------------------------------
        public bool IsAlertMessageAvailable
        {
            get { return __IsAlertMessageAvailable; }
            set { __IsAlertMessageAvailable = value; }
        }
    }
}



