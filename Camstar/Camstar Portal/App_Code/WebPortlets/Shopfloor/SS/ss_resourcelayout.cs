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

using SWC = System.Web.UI.WebControls;
using System.Collections;

/// <summary>
/// Summary description for SS_ResourceLayout
/// </summary>
namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class SS_ResourceLayout:MatrixWebPart 
    {
        protected CWC.PagePanel _pnlChartPanel { get { return Page.FindCamstarControl("CanvasPanel") as CWC.PagePanel; } }
        protected CWC.NamedObject _ndoResourceLayout { get { return Page.FindCamstarControl("ResourceLayout") as CWC.NamedObject; } }
        protected CWC.NamedObject _ndoAvailableResources { get { return Page.FindCamstarControl("AvailableResources") as CWC.NamedObject; } }        
        protected CWC.TextBox _txtIntervalIDs { get { return Page.FindCamstarControl("IntervalIDs") as CWC.TextBox; } }        
        protected CWC.PagePanel _pnlRefreshSlider { get { return Page.FindCamstarControl("RefreshSliderPanel") as CWC.PagePanel; } }

        //-----------------------------------------
        //
        //-----------------------------------------
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            _ndoResourceLayout.DataChanged += new EventHandler(_ndoResourceLayout_DataChanged);            
            _txtIntervalIDs.Hidden = true;

            ScriptManager.RegisterClientScriptBlock(_pnlChartPanel, this.GetType(), "CanvasScript", "ResourceLayoutViewScript();", true);

            // re-render the layout view if there was a popup transaction
            if (SEMI.AppCode.UIUtility.IsPopupClose(this))
                _ndoResourceLayout_DataChanged(null, null);
            
        }

        //-----------------------------------------
        //
        //-----------------------------------------
        void _ndoResourceLayout_DataChanged(object sender, EventArgs e)
        {
            _ndoAvailableResources.Enabled = false;
            if (_ndoResourceLayout.Data != null)
            {
                Hashtable htResources = new Hashtable();
                RecordSet rsResources = new RecordSet();
                rsResources = RetrieveRuntimeLayout(_ndoResourceLayout.Data.ToString());
                htResources = InitResourceListing(rsResources);
                DrawCanvas(htResources);

                //add the resources to the AvailableResources ndo
                List<NamedObjectRef> lstResources = new List<NamedObjectRef>();
                foreach (DictionaryEntry oItem in htResources)
                { 
                    NamedObjectRef oResource = new NamedObjectRef(oItem.Key.ToString());
                    lstResources.Add(oResource);
                }

                CWC.NamedObject ndoAvailableResourcesTemp = Page.FindCamstarControl("AvailableResources") as CWC.NamedObject;
                _ndoAvailableResources.ClearSelectionValues();
                _ndoAvailableResources.ClearData();

                if (lstResources.Count > 0)
                {
                    SEMI.AppCode.ControlsUtility.NamedObjectControl_SetSelectionValues(ref ndoAvailableResourcesTemp, lstResources.ToArray());
                    _ndoAvailableResources.Enabled = true;
                }
            }
        }

        //-----------------------------------------
        //
        //-----------------------------------------
        private void DrawCanvas( Hashtable htResources)
        {            
            int iCanvasWidth = 800;
            int iCanvasHeight = 600;
            string sBackground = "";

            _pnlChartPanel.Controls.Clear();

            //set the panel height and width
            if (htResources.Count > 0)
            {
                foreach (DictionaryEntry htItem in htResources)
                {                   
                    SS_ResourceLayoutItem oResourceItem = new SS_ResourceLayoutItem();
                    oResourceItem = htItem.Value as SS_ResourceLayoutItem;
                    if (oResourceItem.LayoutHeight != 0)
                        iCanvasHeight = oResourceItem.LayoutHeight;

                    if (oResourceItem.LayoutWidth != 0)
                        iCanvasWidth = oResourceItem.LayoutWidth;

                    if (!string.IsNullOrEmpty(oResourceItem.BackgroundFilename))
                        sBackground = oResourceItem.BackgroundFilename;

                    break;
                }
            }   

            // create the drawing area container (to enable the scrollbar)
            System.Web.UI.WebControls.Panel pnlDrawAreaFrame = new System.Web.UI.WebControls.Panel();
            pnlDrawAreaFrame.ID = "canvasAreaFrame";
            pnlDrawAreaFrame.CssClass = "canvasAreaFrameClass";
            pnlDrawAreaFrame.Width = _pnlChartPanel.Width;
            pnlDrawAreaFrame.Height = int.Parse(_pnlChartPanel.Height.Value.ToString());
            pnlDrawAreaFrame.BorderStyle = System.Web.UI.WebControls.BorderStyle.Solid;
            pnlDrawAreaFrame.BorderWidth = 1;
            pnlDrawAreaFrame.ScrollBars = System.Web.UI.WebControls.ScrollBars.Auto;

            // create the drawing area
            SWC.Panel pnlDrawArea = new SWC.Panel();
            pnlDrawArea.ID = "canvasArea";
            pnlDrawArea.CssClass = "canvasAreaClass";
            pnlDrawArea.Width = iCanvasWidth;
            pnlDrawArea.Height = iCanvasHeight;
            pnlDrawArea.BorderStyle = System.Web.UI.WebControls.BorderStyle.Solid;
            pnlDrawArea.BorderWidth = 1;
            pnlDrawArea.Style["position"] = "relative";

            // set the background image if available            
            if (!string.IsNullOrEmpty(sBackground))
            {
                pnlDrawArea.Style["background-image"] = sBackground;
                pnlDrawArea.Style["background-repeat"] = "no-repeat";
                pnlDrawArea.Style["background-position"] = "center center";
            }
            
            if (htResources != null)
            {
                foreach (DictionaryEntry htItem in htResources)
                {
                    SWC.Panel oResource = new SWC.Panel();
                    SS_ResourceLayoutItem oResourceItem = new SS_ResourceLayoutItem();
                    oResourceItem = htItem.Value as SS_ResourceLayoutItem;
                    oResource = InitResourceObject(oResourceItem.ResourceName, int.Parse(oResourceItem.YLocation), int.Parse(oResourceItem.XLocation), oResourceItem);
                    pnlDrawArea.Controls.Add(oResource);
                }
            }

            pnlDrawAreaFrame.Controls.Add(pnlDrawArea);

            // add resourceIcon and drawArea to the canvas
            _pnlChartPanel.Controls.Add(pnlDrawAreaFrame);

            // add the div for the refresh slider, jqueryUI do the rest
            _pnlRefreshSlider.Controls.Clear();            
            System.Web.UI.WebControls.Panel pnlSlider = new System.Web.UI.WebControls.Panel();
            pnlSlider.ID = "refreshSlider";
            string sRefreshIntervalLabel = @"<label id='refreshIntervalLabel' class='refreshIntervalLabelClass'>Refresh Interval:</label> " +
            "<input type='text' id='refreshIntervalValue' class='refreshIntervalValueClass' readonly>";            

            _pnlRefreshSlider.Controls.Add(new LiteralControl(sRefreshIntervalLabel));
            _pnlRefreshSlider.Controls.Add(pnlSlider);

            CamstarWebControl.SetRenderToClient(_pnlChartPanel);
        }
        
        //-----------------------------------------
        //
        //-----------------------------------------
        private SWC.Panel InitResourceObject(string sResourceID, int iTopPosition, int iLeftPosition, SS_ResourceLayoutItem oLayoutItem)
        {
			string spaceTag = "_0xSPACEx0_";
			string sJQueryResourceID = sResourceID.Replace(" ", spaceTag);

            // div for the object
            SWC.Panel oResource = new SWC.Panel();
            oResource.Style["position"] = "absolute";
            oResource.Style["top"] = iTopPosition.ToString() + "px";
            oResource.Style["left"] = iLeftPosition.ToString() + "px";
			oResource.ID = sJQueryResourceID; //sResourceID;
			oResource.Attributes.Add("resname", sResourceID);
            oResource.CssClass = "drawnIconClass";
            oResource.Width = 50;
            oResource.Height = 50;                  

            //---------image icon div---------
            SWC.Panel pnlIconImage = new SWC.Panel();                                

            pnlIconImage.CssClass = "drawnIconClass_Icon";
            pnlIconImage.Height = 35;
            pnlIconImage.Width = 50;                        
          
            SWC.Image imgIcon = new SWC.Image();
            imgIcon.CssClass = "drawnIconClass_Icon_Image";
            imgIcon.Style["color"] = oLayoutItem.STYLE;

            // add the resourceLayoutIcon_DownState cssClass that triggers the glowing red  
            if (!oLayoutItem.IsAvailable) 
                imgIcon.CssClass = "drawnIconClass_Icon_Image resourceLayoutIcon_DownState";

            ////// if no items tracked in then make the box shadow white
            ////if (oLayoutItem.LotCount <= 0)
            ////    imgIcon.Style["color"] = "#fff";

			imgIcon.ID = "imgIcon_" + sJQueryResourceID; // sResourceID;
            if (string.IsNullOrEmpty(oLayoutItem.IconFilename))
                imgIcon.ImageUrl = "images/User/SS_ResourceLayoutIcon.png";
            else
                imgIcon.ImageUrl = oLayoutItem.IconFilename;
            imgIcon.Style["vertical-align"] = "middle";

            // fix the image size for now
            imgIcon.Height = oLayoutItem.IconHeight;
            imgIcon.Width = oLayoutItem.IconWidth;

            // add a span element with specific css to help with centering the icon aligment
            string sIconAlignSpan = @"<span class='drawnIconClass_IconAlignmentHelper'></span>";
            pnlIconImage.Controls.Add(new LiteralControl(sIconAlignSpan));
            pnlIconImage.Controls.Add(imgIcon);

            //---------resource name div---------
            SWC.Panel pnlResource_Name = new SWC.Panel();
            pnlResource_Name.CssClass = "drawnIconClass_Resource";
            pnlResource_Name.Height = 15;
            pnlResource_Name.Width = 50;
            pnlResource_Name.Style["color"] = oLayoutItem.STYLE;
            string sHTML = "<span class='drawnIconClass_Resource'>#RESOURCE#</span>";
            string sLotCountValue = "";
            if (oLayoutItem.LotCount > 0)
                sLotCountValue = "[" + oLayoutItem.LotCount.ToString() + "]";

            string sResourceName = sResourceID;
            if (sResourceID.Length > 8)
                sResourceName = sResourceName.Substring(0, 7) + "..";

            // <span class='drawnIconClass_LotCount'>#LOTCOUNT#</span>sHTML = sHTML.Replace("#LOTCOUNT#", sLotCountValue);
            sHTML = sHTML.Replace("#RESOURCE#", sResourceName);

            pnlResource_Name.Controls.Add(new LiteralControl(sHTML));

            //oResource.BorderStyle = System.Web.UI.WebControls.BorderStyle.Dotted;
            //oResource.BorderWidth = 1;            
            //pnlResource_Name.BorderStyle = System.Web.UI.WebControls.BorderStyle.Solid;
            //pnlResource_Name.BorderWidth = 1;
            //pnlStatus.BorderStyle = System.Web.UI.WebControls.BorderStyle.Dotted;
            //pnlStatus.BorderWidth = 1;

            //-----------DETAILS PANEL: BEGIN----------------------------------------------
           
            // main panel
            SWC.Panel pnlDetails = new SWC.Panel();
            pnlDetails.CssClass = "drawnIconDetailsClass";
            pnlDetails.Style["position"] = "absolute";
            pnlDetails.Style["top"] = "40px";
            pnlDetails.Style["left"] = "40px";
            pnlDetails.Style["z-index"] = "999"; // set the z-index to a large value so that the popup will be ontop of other elements

            string sDetailsPanelLayout = "" +
            @"<div class='drawnIconDetailsClass_Resource'>" +
                @"<span class='dIDC_ResourceValue'>#RESOURCE#</span>" +
            @"</div>" +            
            @"<div class='drawnIconDetailsClass_Status'>" +
                @"<span class='dIDC_Label'>Status&nbsp;:&nbsp;</span><span class='dIDC_StatusValue'>#STATUS#</span>" +
                @"<br>" +
                @"<span class='dIDC_Label'>Reason&nbsp;:&nbsp;</span><span class='dIDC_ReasonValue'>#REASON#</span>" +
            @"</div>" +
            @"<div class='drawnIconDetailsClass_LotInfo'>" +
                @"<span class='dIDC_Label'>Lot Count&nbsp;:&nbsp;</span><span class='dIDC_LotCountValue'>#LOTCOUNT#</span>" +
                @"<div class='dIDC_Lot'>#LOTIDS#</div>" +
            @"</div>";

            // replace placeholders with values
            sDetailsPanelLayout = sDetailsPanelLayout.Replace("#RESOURCE#", oLayoutItem.ResourceName);
            sDetailsPanelLayout = sDetailsPanelLayout.Replace("#STATUS#", oLayoutItem.Status);
            sDetailsPanelLayout = sDetailsPanelLayout.Replace("#REASON#", oLayoutItem.Reason);
            sDetailsPanelLayout = sDetailsPanelLayout.Replace("#LOTCOUNT#", oLayoutItem.LotCount.ToString());

            string sLotIDList = "";
            if (oLayoutItem.LotCount > 0)
            {                
                foreach (string sLotID in oLayoutItem.LotIDs)
                {
                    sLotIDList = sLotIDList + sLotID + "<br>"; 
                }
            }
            sDetailsPanelLayout = sDetailsPanelLayout.Replace("#LOTIDS#", sLotIDList);

            pnlDetails.Controls.Add(new LiteralControl(sDetailsPanelLayout));

            //-----------DETAILS PANEL: END------------------------------------------------

            //oResource.Controls.Add(pnlStatus);            
            oResource.Controls.Add(pnlIconImage);           
            oResource.Controls.Add(pnlResource_Name);
            oResource.Controls.Add(pnlDetails);

            return oResource;
        }

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
                                 oResourceItem.LayoutHeight = ParseIntegerEx(sValues[6], oResourceItem.LayoutHeight);//int.Parse(sValues[6]);
                                 oResourceItem.LayoutWidth = ParseIntegerEx(sValues[7], oResourceItem.LayoutWidth);//int.Parse(sValues[7]);
                                 oResourceItem.XLocation = sValues[8];
                                 oResourceItem.YLocation = sValues[9];
                                 oResourceItem.IconFilename = sValues[10];
                                 oResourceItem.IconHeight = ParseIntegerEx(sValues[11], oResourceItem.IconHeight);//int.Parse(sValues[11]);
                                 oResourceItem.IconWidth = ParseIntegerEx(sValues[12], oResourceItem.IconHeight);//int.Parse(sValues[12]);
                                 oResourceItem.MinTime = sValues[13];
                                 oResourceItem.MinTimeAlertFrame = sValues[14];
                                 oResourceItem.MintTimeOverTimeFrame = sValues[15];
                                 oResourceItem.LotCount = ParseIntegerEx(sValues[16], oResourceItem.LotCount);//int.Parse(sValues[16]);
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

        //--------------------------------------
        //
        //--------------------------------------
        private int ParseIntegerEx(string sInput, int iDefault = 0)
        {
            int iResult = iDefault;

            try 
            {
                iResult = int.Parse(sInput);
            }
            catch
            {
                iResult = iDefault;
            }

            return iResult;
        }

        //--------------------------------------
        //
        //--------------------------------------
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

            public SS_ResourceLayoutItem()
            {
                // constructor
                iLayoutHeight = 600;
                iLayoutWidth = 800;
                bIsAvailable = false;

                iIconHeight = 30;
                iIconWidth = 30;

                iLotCount = 0;

                sXLocation = "0";
                sYLocation = "0";

                lstLotID = new List<string>();
            }

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

            public string Status
            {
                get { return sStatus; }
                set { sStatus = value; }
            }

            public string Reason
            {
                get { return sReason; }
                set { sReason = value; }
            }

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

            public string MinTime
            {
                get { return sMinTime; }
                set { sMinTime = value; }
            }

            public string MinTimeAlertFrame
            {
                get { return sMinTimeAlertFrame; }
                set { sMinTimeAlertFrame = value; }
            }

            public string MintTimeOverTimeFrame
            {
                get { return sMintTimeOverTimeFrame; }
                set { sMintTimeOverTimeFrame = value; }
            }

            public int LotCount
            {
                get { return iLotCount; }
                set { iLotCount = value; }
            }

            public List<string> LotIDs
            {
                get { return lstLotID; }
            }

            public string TrackSeqByEquipment
            {
                get { return sTrackSeqByEquipment; }
                set { sTrackSeqByEquipment = value; }
            }

            public string STYLE
            {
                get { return sSTYLE; }
                set { sSTYLE = value; }
            }

            public void AddLotID(string sLotID)
            {
                lstLotID.Add(sLotID);
            }
        }        
    }       
}



