/* Copyright 2019 Siemens */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Text;

using Camstar.WCF.ObjectStack;
using Camstar.WCF.Services;
using Camstar.WebPortal.Utilities;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.Constants;
using Camstar.WebPortal.WCFUtilities;
using Camstar.WebPortal.Personalization;

using SWC = System.Web.UI.WebControls;
using System.Collections;

/// <summary>
/// Summary description for SS_ResourceLayoutService
/// IMPORTANT NOTE: 
///     The web config needs to be modified for this service to be enabled/exposed
///     Ensure that the following tags are set in the web.config
///     1) within the <endpointBehaviors></endpointBehaviors> tag, add a new endpoint behavior like below
///         <behavior name="SS_ResourceLayoutServiceAjaxBehavior">
///             <webHttp />
///             <enableWebScript />
///         </behavior>
///     2) within the <services></services> tag, add the following service
///         <service name="SS_ResourceLayoutService">
///             <endpoint address="" behaviorConfiguration="SS_ResourceLayoutServiceAjaxBehavior" binding="webHttpBinding" contract="SS_ResourceLayoutService"/>
///         </service>
/// </summary>

[ServiceContract(Namespace = "")]
[AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
public class SS_ResourceLayoutService
{
    //--------------------------------------
    //
    //--------------------------------------
    [OperationContract]
    [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json)]
    public List<SS_ResourceLayoutItem> ResourceLayout_GetResourceLayoutItems(string ResourceLayoutName)
    {
        List<SS_ResourceLayoutItem> oResourceItems = new List<SS_ResourceLayoutItem>();
        RecordSet rsResources = new RecordSet();
        Hashtable htResources = new Hashtable();

        rsResources = RetrieveRuntimeLayout(ResourceLayoutName);
        htResources = InitResourceListing(rsResources);
        foreach (DictionaryEntry htItem in htResources)
        {
            SS_ResourceLayoutItem oItem = new SS_ResourceLayoutItem();
            oItem = htItem.Value as SS_ResourceLayoutItem;
            oResourceItems.Add(oItem);
        }       

        return oResourceItems;
    }

    //--------------------------------------
    //
    //--------------------------------------
    private RecordSet RetrieveRuntimeLayout(string ResourceLayout)
    {
        // get the session and user profile
        var fs = FrameworkManagerUtil.GetFrameworkSession();
        QueryService oService = new QueryService(fs.CurrentUserProfile);
        QueryOptions oOptions = new QueryOptions();
        RecordSet oData = new RecordSet();
        QueryParameters oParameters = new QueryParameters();
        QueryParameter[] oParameterList = new QueryParameter[1];
        oParameterList[0] = new QueryParameter();
        oParameterList[0].Name = "NameFilter";
        oParameterList[0].Value = ResourceLayout;

        oParameters.Parameters = oParameterList;

        ResultStatus oResult = oService.Execute("_ResourceLayoutForRuntime", oParameters, oOptions, out oData);
        if (oResult.IsSuccess)
        {
            string sTableName = "";
            string sCDOName = "";

            if (oData != null)
            {
                if (oData.Rows != null)
                {
                    if (oData.Rows.Length > 0)
                    {
                        sTableName = oData.Rows[0].Values[0].ToString();
                        sCDOName = oData.Rows[0].Values[1].ToString();
                    }
                }
            }

            return oData;
        }

        return null;
    } // RetrieveRuntimeLayout

    //-----------------------------------------
    //
    //-----------------------------------------
    private Hashtable InitResourceListing(RecordSet rsResources)
    {
        Hashtable htResources = new Hashtable();
        if (rsResources != null)
            if (rsResources.Rows != null)
                if (rsResources.Rows.Length > 0)
                {
                    foreach (Row oRow in rsResources.Rows)
                    {
                        /* Current logic assumes that the data returned has the columns in the following order:
                         *     ResourceLayout[0]; ResourceName[1]; Status[2]; Reason[3]; IsAvailable[4]; BackgroundFilename[5];
                         *     LayoutHeight[6] LayoutWidth[7]; XLocation[8]; YLocation[9]; IconFilename[10]; IconHeight[11]; IconWidth[12];
                         *     MinTime[13]; MinTimeAlertFrame[14]; MintTimeOverTimeFrame[15]; LotCount[16]; LotID[17]; TrackSequenceByEquipment[18]; STYLE[19];
                         *         
                         * If there is change to the query SQL, need to re-confirm the order of the columns
                         */

                        string[] sValues = oRow.Values;
                        string sResourceName = sValues[1];

                        if (!htResources.ContainsKey(sResourceName))
                        {
                            SS_ResourceLayoutItem oResourceItem = new SS_ResourceLayoutItem();
                            oResourceItem.ResourceLayout = sValues[0];
                            oResourceItem.ResourceName = sValues[1];
                            oResourceItem.Status = sValues[2];
                            oResourceItem.Reason = sValues[3];
                            if (sValues[4].ToString() == "0")
                                oResourceItem.IsAvailable = false;
                            else
                                oResourceItem.IsAvailable = true;
                            oResourceItem.BackgroundFilename = sValues[5];
                            oResourceItem.LayoutHeight = int.Parse(sValues[6]);
                            oResourceItem.LayoutWidth = int.Parse(sValues[7]);
                            oResourceItem.XLocation = sValues[8];
                            oResourceItem.YLocation = sValues[9];
                            oResourceItem.IconFilename = sValues[10];
                            oResourceItem.IconHeight = int.Parse(sValues[11]);
                            oResourceItem.IconWidth = int.Parse(sValues[12]);
                            oResourceItem.MinTime = sValues[13];
                            oResourceItem.MinTimeAlertFrame = sValues[14];
                            oResourceItem.MintTimeOverTimeFrame = sValues[15];
                            oResourceItem.LotCount = int.Parse(sValues[16]);
                            if (!string.IsNullOrEmpty(sValues[17]))
                                oResourceItem.AddLotID(sValues[17]);
                            oResourceItem.TrackSeqByEquipment = sValues[18];
                            oResourceItem.STYLE = sValues[19];

                            htResources.Add(sResourceName, oResourceItem);
                        }
                        else
                        {
                            // existing resource, just add the lotID its LotID list
                            SS_ResourceLayoutItem oResourceItem = new SS_ResourceLayoutItem();
                            oResourceItem = htResources[sResourceName] as SS_ResourceLayoutItem;
                            oResourceItem.AddLotID(sValues[17]);
                            // update the hashtable with the modified ResourceItem
                            htResources[sResourceName] = oResourceItem;
                        }
                    }
                }

        return htResources;
    }

    //--------------------------------------
    //
    //--------------------------------------
    [DataContract]
    public class SS_ResourceLayoutItem
    {
        private string sResourceLayout;
        private string sResourceName;
        private string sStatus;
        private string sReason;
        private bool bIsAvailable;
        private string sBackgroundFilename;
        private int iLayoutHeight;
        private int iLayoutWidth;
        private string sXLocation;
        private string sYLocation;
        private string sIconFilename;
        private int iIconHeight;
        private int iIconWidth;
        private string sMinTime;
        private string sMinTimeAlertFrame;
        private string sMintTimeOverTimeFrame;
        private int iLotCount;
        private List<string> lstLotID;
        private string sTrackSeqByEquipment;
        private string sSTYLE;
        private string sLotIDListHTML;

        public SS_ResourceLayoutItem()
        {
            // constructor
            iLayoutHeight = 600;
            iLayoutWidth = 800;
            bIsAvailable = false;

            iIconHeight = 28;
            iIconWidth = 28;

            iLotCount = 0;

            sXLocation = "0";
            sYLocation = "0";

            sLotIDListHTML = "";

            lstLotID = new List<string>();
        }

        [DataMember]
        public string ResourceName
        {
            get { return sResourceName; }
            set { sResourceName = value; }
        }
        
        public string ResourceLayout
        {
            get { return sResourceLayout; }
            set { sResourceLayout = value; }
        }

        [DataMember]
        public string Status
        {
            get { return sStatus; }
            set { sStatus = value; }
        }

        [DataMember]
        public string Reason
        {
            get { return sReason; }
            set { sReason = value; }
        }

        [DataMember]
        public bool IsAvailable
        {
            get { return bIsAvailable; }
            set { bIsAvailable = value; }
        }
        
        public string BackgroundFilename
        {
            get { return sBackgroundFilename; }
            set { sBackgroundFilename = value; }
        }

        public int LayoutHeight
        {
            get { return iLayoutHeight; }
            set { iLayoutHeight = value; }
        }

        public int LayoutWidth
        {
            get { return iLayoutWidth; }
            set { iLayoutWidth = value; }
        }

        public string XLocation
        {
            get { return sXLocation; }
            set { sXLocation = value; }
        }

        public string YLocation
        {
            get { return sYLocation; }
            set { sYLocation = value; }
        }

        public string IconFilename
        {
            get { return sIconFilename; }
            set { sIconFilename = value; }
        }

        public int IconHeight
        {
            get { return iIconHeight; }
            set { iIconHeight = value; }
        }

        public int IconWidth
        {
            get { return iIconWidth; }
            set { iIconWidth = value; }
        }

        [DataMember]
        public string MinTime
        {
            get { return sMinTime; }
            set { sMinTime = value; }
        }

        [DataMember]
        public string MinTimeAlertFrame
        {
            get { return sMinTimeAlertFrame; }
            set { sMinTimeAlertFrame = value; }
        }

        [DataMember]
        public string MintTimeOverTimeFrame
        {
            get { return sMintTimeOverTimeFrame; }
            set { sMintTimeOverTimeFrame = value; }
        }

        [DataMember]
        public int LotCount
        {
            get { return iLotCount; }
            set { iLotCount = value; }
        }

        [DataMember]
        public List<string> LotIDs
        {
            get { return lstLotID; }
        }

        [DataMember]
        public string TrackSeqByEquipment
        {
            get { return sTrackSeqByEquipment; }
            set { sTrackSeqByEquipment = value; }
        }

        [DataMember]
        public string STYLE
        {
            get { return sSTYLE; }
            set { sSTYLE = value; }
        }

        [DataMember]
        public string LotIDListHTML
        {
            get { return sLotIDListHTML; }
            set { sLotIDListHTML = value; }
        }

        public void AddLotID(string sLotID)
        {
            lstLotID.Add(sLotID);
            sLotIDListHTML = sLotIDListHTML + sLotID + @"<br>";
        }

    }


   
}





