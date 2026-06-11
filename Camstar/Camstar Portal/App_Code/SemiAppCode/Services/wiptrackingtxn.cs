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
/// Summary description for WIPTrackingTxn
/// </summary>

namespace SEMI.AppCode.Services
{
    public class WIPTrackingTxn
    {
        //-----------------------------------------
        //
        //-----------------------------------------
        public static void SetServiceObjects(string ServiceType, ref object Service, ref object ServiceData, ref object ServiceInfo, ref object ServiceRequest, ref object ServiceResult)
        {
            Service = new object();
            ServiceInfo = new object();
            ServiceData = new object();
            ServiceRequest = new object();
            ServiceResult = new object();
            
            var fs = FrameworkManagerUtil.GetFrameworkSession();

            Service = new WSDataCreator().CreateService(ServiceType, fs.CurrentUserProfile);
            ServiceInfo = WCFObject.CreateObject(ServiceType + "_Info");
            ServiceData = WCFObject.CreateObject(ServiceType);
            ServiceRequest = WCFObject.CreateObject(ServiceType + "_Request");
            ServiceResult = WCFObject.CreateObject(ServiceType + "_Result");
            
            ////object oService2 = WCFObject.CreateObject("AssemblyMotherLotWIPMainService");
            

           
            ////(Service as ServiceService).UserProfile = new UserProfile();
            ////(Service as ServiceService).UserProfile = fs.CurrentUserProfile;            
                     
        } // SetServiceObjects      
    }
}



