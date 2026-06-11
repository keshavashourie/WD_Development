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

/// <summary>
/// Summary description for SS_ResourceLayoutCanvas
/// </summary>
namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class SS_ResourceLayoutMaint: MatrixWebPart
    {
        protected CWC.PagePanel _pnlCanvasPanel { get { return Page.FindCamstarControl("CanvasPanel") as CWC.PagePanel; } }
        protected CWC.PagePanel _pnlControlPanel { get { return Page.FindCamstarControl("ControlPanel") as CWC.PagePanel; } }
        protected CWC.PagePanel _pnlDeletePanel { get { return Page.FindCamstarControl("DeletePanel") as CWC.PagePanel; } }   

        protected CWC.TextBox _txtLayoutHeight { get { return Page.FindCamstarControl("ObjectChanges_LayoutHeight") as CWC.TextBox; } }
        protected CWC.TextBox _txtLayoutWidth { get { return Page.FindCamstarControl("ObjectChanges_LayoutWidth") as CWC.TextBox; } }        
        protected CWC.TextBox _txtRenderCanvasState { get { return Page.FindCamstarControl("RenderCanvas_State") as CWC.TextBox; } }

        protected CWC.TextBox _txtTargetName { get { return Page.FindCamstarControl("TargetName") as CWC.TextBox; } }
        protected CWC.TextBox _txtTargetX { get { return Page.FindCamstarControl("TargetX") as CWC.TextBox; } }
        protected CWC.TextBox _txtTargetY { get { return Page.FindCamstarControl("TargetY") as CWC.TextBox; } }
        protected CWC.TextBox _txtAction { get { return Page.FindCamstarControl("Details_Action") as CWC.TextBox; } }        

        protected CWC.Button _btnRenderCanvas { get { return Page.FindCamstarControl("RenderCanvas") as CWC.Button; } }
        protected CWC.Button _btnUpdateDetail { get { return Page.FindCamstarControl("UpdateDetail") as CWC.Button; } }
        protected CWC.Button _btnDeleteDetail { get { return Page.FindCamstarControl("DeleteDetail") as CWC.Button; } }
        protected JQDataGrid _grdDetails { get { return Page.FindCamstarControl("ObjectChanges_Details") as JQDataGrid; } }

        protected CWC.FileBrowse _fileBackgroundFile { get { return Page.FindCamstarControl("ObjectChanges_BackgroundFilename") as CWC.FileBrowse; } }

        protected const string _kSpaceTag = "_0xSPACEx0_";
        //-----------------------------------------
        //
        //-----------------------------------------
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            
            _btnRenderCanvas.Hidden = true;
            _btnUpdateDetail.Hidden = true;
            _btnDeleteDetail.Hidden = true;
            _txtRenderCanvasState.Hidden = true;
            _txtAction.Hidden = true;
            _txtTargetName.Hidden = true;
            _txtTargetX.Hidden = true;
            _txtTargetY.Hidden = true;
            
            _btnRenderCanvas.Click += new EventHandler(_btnRenderCanvas_Click);
            _btnDeleteDetail.Click += new EventHandler(_btnDeleteDetail_Click);
            _btnUpdateDetail.Click += new EventHandler(_btnUpdateDetail_Click);

            ScriptManager.RegisterClientScriptBlock(_pnlCanvasPanel, this.GetType(), "CanvasScript", "ResourceLayoutMaintScript();", true);                        
        }

        //-----------------------------------------
        //
        //-----------------------------------------
        void _btnDeleteDetail_Click(object sender, EventArgs e)
        {
            SWC.Table oTable = Page.FindCamstarControl("ResourceListing") as SWC.Table;
            foreach (SWC.TableRow oTR in oTable.Rows)
            {
                string sResourceName = oTR.Cells[0].Text;
                string sResourceX = oTR.Cells[1].Text;
            }
        }
        
        //-----------------------------------------
        //
        //-----------------------------------------
        void _btnRenderCanvas_Click(object sender, EventArgs e)
        {
            DrawPanel();
        }

        //-----------------------------------------
        //
        //-----------------------------------------
        private void DrawPanel()
        {
            // need to clear all the panels as there is an iFrame inside by default.
            // the iFrame messes up all alignment
            _pnlCanvasPanel.Controls.Clear();
            _pnlControlPanel.Controls.Clear();
            _pnlDeletePanel.Controls.Clear();

            // --------------------------------------------------RESOURCE ICON TEMPLATE --------------------------------------------------
            SWC.Panel pnlResourceIcon = new SWC.Panel();
            pnlResourceIcon.ID = "resourceIcon";
            pnlResourceIcon.CssClass = "resourceIconClass";
            pnlResourceIcon.Width = 50;
            pnlResourceIcon.Height = 40;            
            pnlResourceIcon.Style["z-index"] = "20";

            // resource name div tag
            SWC.Panel pnlResourceName = new SWC.Panel();
            pnlResourceName.CssClass = "resourceIdClass"; //"resourceIdClass ui-selectee";
            pnlResourceName.Height = 15;
            pnlResourceName.Width = 50;

            // image icon div
            SWC.Panel pnlIconImage = new SWC.Panel();
            pnlIconImage.CssClass = "drawnMaintIconClass_Icon"; //"drawnMaintIconClass_Icon ui-selectee";
            pnlIconImage.Height = 35;
            pnlIconImage.Width = 50;            

            // create the image icon
            SWC.Image imgIcon = new SWC.Image();
            imgIcon.ID = "imgIcon";
            imgIcon.ImageUrl = "images/User/SS_ResourceLayoutIcon.png";
            imgIcon.Style["vertical-align"] = "middle";
            imgIcon.Height = 30;
            imgIcon.Width = 30;
            imgIcon.CssClass = "ui-selectee";

            // add a span element with specific css to help with centering the icon aligment
            string sIconAlignSpan = @"<span class='drawnMaintIconClass_IconAlignmentHelper'></span>"; //@"<span class='drawnMaintIconClass_IconAlignmentHelper ui-selectee'></span>";
            pnlIconImage.Controls.Add(new LiteralControl(sIconAlignSpan));
            pnlIconImage.Controls.Add(imgIcon);

            //-----------details panel ---------------------------------            
            SWC.Panel pnlDetails = new SWC.Panel();
            pnlDetails.CssClass = "resourceIconDetailsClass";//"resourceIconDetailsClass ui-selectee";
            pnlDetails.Style["position"] = "absolute";
            pnlDetails.Style["top"] = "";
            pnlDetails.Style["left"] = "";
            pnlDetails.Style["z-index"] = "999"; // set the z-index to a large value so that the popup will be ontop of other elements

            string sDetailsPanelLayout = "" +
            @"<div class='drawnMaintIconDetailsClass_Resource'>" + // @"<div class='drawnMaintIconDetailsClass_Resource ui-selectee'>" +
                @"<span class='dMIDC_ResourceValue'>#RESOURCE#</span>" + // @"<span class='dMIDC_ResourceValue ui-selectee'>#RESOURCE#</span>" +
            @"</div>";           

            pnlDetails.Controls.Add(new LiteralControl(sDetailsPanelLayout));            
  
            // add name, image and details to the resource Icon template
            pnlResourceIcon.Controls.Add(pnlIconImage);
            pnlResourceIcon.Controls.Add(pnlResourceName);
            pnlResourceIcon.Controls.Add(pnlDetails);

            //--------------------------------------------------DELETE ICON BUTTON--------------------------------------------------
            SWC.Panel pnlDelete = new SWC.Panel();
            pnlDelete.ID = "deleteIcon";
            pnlDelete.CssClass = "deleteIconClass";
            pnlDelete.Width = 40;
            pnlDelete.Height = 40;            

            // image icon div
            SWC.Panel pnlDeleteImage = new SWC.Panel();
            pnlDeleteImage.CssClass = "deleteIconClass_Icon";
            pnlDeleteImage.Height = 30;
            pnlDeleteImage.Width = 30; 

            // image for the delete 
            SWC.Image imgDelete = new SWC.Image();
            imgDelete.ID = "imgDelete";
            imgDelete.ImageUrl = "images/User/SS_Canvas_Delete.png";
            imgDelete.Style["vertical-align"] = "middle";
            imgDelete.Height = 25;
            imgDelete.Width = 25;

            pnlDeleteImage.Controls.Add(new LiteralControl(sIconAlignSpan));
            pnlDeleteImage.Controls.Add(imgDelete);

            // delete name div tag
            SWC.Panel pnlDeleteName = new SWC.Panel();
            pnlDeleteName.CssClass = "deleteIconClass_Label";
            pnlDeleteName.Height = 15;
            pnlDeleteName.Width = 50;
            pnlDeleteName.Controls.Add(new LiteralControl(""));

            pnlDelete.Controls.Add(pnlDeleteImage);
            //pnlDelete.Controls.Add(pnlDeleteName);

            //--------------------------------------------------CANVAS AREA--------------------------------------------------

            //------------create the drawing area container (to enable the scrollbar)---------------------
            SWC.Panel pnlDrawAreaFrame = new SWC.Panel();
            pnlDrawAreaFrame.ID = "canvasAreaFrame";
            pnlDrawAreaFrame.CssClass = "canvasAreaFrameClass";
            pnlDrawAreaFrame.Width = _pnlCanvasPanel.Width;
            pnlDrawAreaFrame.Height = int.Parse(_pnlCanvasPanel.Height.Value.ToString());// -int.Parse(pnlResourceIcon.Height.Value.ToString());
            pnlDrawAreaFrame.BorderStyle = System.Web.UI.WebControls.BorderStyle.Solid;
            pnlDrawAreaFrame.BorderWidth = 1;
            pnlDrawAreaFrame.ScrollBars = System.Web.UI.WebControls.ScrollBars.Auto;
            //pnlDrawAreaFrame.Style["position"] = "relative";
            

            //-----------------create the drawing area----------------------------------
            SWC.Panel pnlDrawArea = new SWC.Panel();
            pnlDrawArea.ID = "canvasArea";
            pnlDrawArea.CssClass = "canvasAreaClass";
            pnlDrawArea.Width = int.Parse(_txtLayoutWidth.Data.ToString());
            pnlDrawArea.Height = int.Parse(_txtLayoutHeight.Data.ToString());
            pnlDrawArea.BorderStyle = System.Web.UI.WebControls.BorderStyle.Solid;
            pnlDrawArea.BorderWidth = 1;
            pnlDrawArea.Style["position"] = "relative";
            pnlDrawArea.Style["z-index"] = "10"; // set the z-index of the drawing canvas to be less than that of the resourceIcon templace so the drop effect is ontop of the canvas

            // set the background image if available
            if (_fileBackgroundFile.Data != null)
            {
                if (!string.IsNullOrEmpty(_fileBackgroundFile.Data.ToString()))
                {
                    pnlDrawArea.Style["background-image"] = _fileBackgroundFile.Data.ToString();
                    pnlDrawArea.Style["background-repeat"] = "no-repeat";
                    pnlDrawArea.Style["background-position"] = "center center";
                }
            }

            ResourceLayoutDetailsChanges[] oDetails = (_grdDetails.GridContext as BoundContext).Data as ResourceLayoutDetailsChanges[];
            if (oDetails != null)
            {                
                foreach (ResourceLayoutDetailsChanges oDetail in oDetails)
                {
                    SWC.Panel oResource = new SWC.Panel();                    
                    oResource = InitResourceObject(oDetail.Resource.Name, int.Parse(oDetail.YLocation.ToString()), int.Parse(oDetail.XLocation.ToString()));
                    pnlDrawArea.Controls.Add(oResource);                    
                }
            }

            pnlDrawAreaFrame.Controls.Add(pnlDrawArea);

            //-------------------------------------------------- PANEL ASSIGNMENTS--------------------------------------------------
            _pnlCanvasPanel.Controls.Add(pnlDrawAreaFrame);          
            _pnlControlPanel.Controls.Add(pnlResourceIcon);
            _pnlDeletePanel.Controls.Add(pnlDelete);


            CamstarWebControl.SetRenderToClient(_pnlDeletePanel);
            CamstarWebControl.SetRenderToClient(_pnlControlPanel);
            CamstarWebControl.SetRenderToClient(_pnlCanvasPanel);           
        }

        //-----------------------------------------
        //
        //-----------------------------------------
        private SWC.Panel InitResourceObject(string sResourceID, int iTopPosition, int iLeftPosition)
        {   
            // replace any spaces in the ResourceID with the spacetag, since jQuery cannot handle having a space in any ID
            string sResourceName = sResourceID;
            string sResourceDisplayName = sResourceName;
            sResourceID = sResourceID.Replace(" ", _kSpaceTag);
          
            // div for the object
            SWC.Panel oResource = new SWC.Panel();
            oResource.Style["position"] = "absolute";       
            oResource.Style["top"] = (iTopPosition).ToString() + "px"; 

            oResource.Style["left"] = iLeftPosition.ToString() + "px";
            oResource.ID = sResourceID;
            oResource.CssClass = "drawnMaintIconClass";
            oResource.Width = 50;
            oResource.Height = 50;

            //---------image icon div---------
            SWC.Panel pnlIconImage = new SWC.Panel();
            pnlIconImage.CssClass = "drawnMaintIconClass_Icon";
            pnlIconImage.Height = 35;
            pnlIconImage.Width = 50;
            //pnlIconImage.Style["margin-left"] = "8px";

            SWC.Image imgIcon = new SWC.Image();
            imgIcon.ID = "imgIcon_" + sResourceID;
            imgIcon.ImageUrl = "images/User/SS_ResourceLayoutIcon.png";
            imgIcon.Style["vertical-align"] = "middle";

            imgIcon.Height = 30;
            imgIcon.Width = 30;

            // add a span element with specific css to help with centering the icon aligment
            string sIconAlignSpan = @"<span class='drawnMaintIconClass_IconAlignmentHelper'></span>";
            pnlIconImage.Controls.Add(new LiteralControl(sIconAlignSpan));
            pnlIconImage.Controls.Add(imgIcon);

            //-----------equipment name div------------
            SWC.Panel pnlResourceName = new SWC.Panel();
            pnlResourceName.CssClass = "resourceIdClass";
            pnlResourceName.Height = 15;
            pnlResourceName.Width = 50;
            if (sResourceName.Length > 8)
                sResourceDisplayName = sResourceName.Substring(0, 7) + "..";

            pnlResourceName.Controls.Add(new LiteralControl(sResourceDisplayName));


            //-----------DETAILS PANEL: BEGIN----------------------------------------------

            // main panel
            SWC.Panel pnlDetails = new SWC.Panel();
            pnlDetails.CssClass = "drawnMaintIconDetailsClass";
            pnlDetails.Style["position"] = "absolute";
            pnlDetails.Style["top"] = "40px";
            pnlDetails.Style["left"] = "40px";
            pnlDetails.Style["z-index"] = "999"; // set the z-index to a large value so that the popup will be ontop of other elements

            string sDetailsPanelLayout = "" +
            @"<div class='drawnMaintIconDetailsClass_Resource'>" +
                @"<span class='dMIDC_ResourceValue'>#RESOURCE#</span>" +            
            @"</div>";

            // replace placeholders with values
            sDetailsPanelLayout = sDetailsPanelLayout.Replace("#RESOURCE#", sResourceName);                       

            pnlDetails.Controls.Add(new LiteralControl(sDetailsPanelLayout));

            //-----------DETAILS PANEL: END------------------------------------------------

            oResource.Controls.Add(pnlIconImage);
            oResource.Controls.Add(pnlResourceName);
            oResource.Controls.Add(pnlDetails);

            //oResource.BorderStyle = System.Web.UI.WebControls.BorderStyle.Dotted;
            //oResource.BorderWidth = 1;

            return oResource;
        }

        //-----------------------------------------
        //
        //-----------------------------------------
        void _btnUpdateDetail_Click(object sender, EventArgs e)
        {
            string sAction = _txtAction.Data.ToString();
            string sResource = _txtTargetName.Data.ToString();
            string sResourceX = _txtTargetX.Data.ToString();
            string sResourceY = _txtTargetY.Data.ToString();            

            int iResourceXLocation = int.Parse(Math.Round(decimal.Parse(sResourceX)).ToString());
            int iResourceYLocation = int.Parse(Math.Round(decimal.Parse(sResourceY)).ToString());

            string sResourcePrefix = _txtTargetName.ClientID.ToString().Replace("TargetName", "");

            sResource = sResource.Replace(sResourcePrefix, "");
            sResource = sResource.Replace(_kSpaceTag, " ");

            switch (sAction)
            {
                case "ADD":
                    DetailsGrid_AddRow(sResource, iResourceXLocation, iResourceYLocation);
                    break;
                case "UPDATE":
                    DetailsGrid_UpdateRow(sResource, iResourceXLocation, iResourceYLocation);
                    break;
                case "DELETE":
                    DetailsGrid_DeleteRow(sResource);
                    break;
            }
        }

        //-----------------------------------------
        //
        //-----------------------------------------
        private void DetailsGrid_AddRow(string sResourceName, int iXLocation, int iYLocation)
        {                      
            ResourceLayoutDetailsChanges[] oDetails = (_grdDetails.GridContext as BoundContext).Data as ResourceLayoutDetailsChanges[];
            List<ResourceLayoutDetailsChanges> oNewDetails = new List<ResourceLayoutDetailsChanges>();
                        
            if (oDetails != null)
            {
                // clone the existing rows
                foreach (ResourceLayoutDetailsChanges oDetail in oDetails)
                {
                    ResourceLayoutDetailsChanges oClone = new ResourceLayoutDetailsChanges();
                    oClone.Resource = new NamedObjectRef(oDetail.Resource.Name);
                    oClone.XLocation = oDetail.XLocation;
                    oClone.YLocation = oDetail.YLocation;

                    oNewDetails.Add(oClone);
                }
            }

            // add the new row
            ResourceLayoutDetailsChanges oNew = new ResourceLayoutDetailsChanges();
            oNew.Resource = new NamedObjectRef(sResourceName);
            oNew.XLocation = iXLocation;
            oNew.YLocation = iYLocation;
            oNewDetails.Add(oNew);

            (_grdDetails.GridContext as BoundContext).Data = oNewDetails.ToArray();
            _grdDetails.DataBind();           
        }

        //-----------------------------------------
        //
        //-----------------------------------------
        private void DetailsGrid_UpdateRow(string sResourceName, int iXLocation, int iYLocation)
        {            
            ResourceLayoutDetailsChanges[] oDetails = (_grdDetails.GridContext as BoundContext).Data as ResourceLayoutDetailsChanges[];                                            
            
            if (oDetails != null)
            {                
                foreach (ResourceLayoutDetailsChanges oDetail in oDetails)
                {
                    if (oDetail.Resource.Name == sResourceName)
                    {
                        oDetail.XLocation = iXLocation;
                        oDetail.YLocation = iYLocation;
                    }                 
                }
            }

            (_grdDetails.GridContext as BoundContext).Data = oDetails;
            _grdDetails.DataBind();            
        }

        //-----------------------------------------
        //
        //-----------------------------------------
        private void DetailsGrid_DeleteRow(string sResourceName)
        {
            ResourceLayoutDetailsChanges[] oDetails = (_grdDetails.GridContext as BoundContext).Data as ResourceLayoutDetailsChanges[];
            List<ResourceLayoutDetailsChanges> oNewDetails = new List<ResourceLayoutDetailsChanges>();

            if (oDetails != null)
            {
                // clone the existing rows
                foreach (ResourceLayoutDetailsChanges oDetail in oDetails)
                {
                    if (oDetail.Resource.Name != sResourceName)
                    {
                        ResourceLayoutDetailsChanges oClone = new ResourceLayoutDetailsChanges();
                        oClone.Resource = new NamedObjectRef(oDetail.Resource.Name);
                        oClone.XLocation = oDetail.XLocation;
                        oClone.YLocation = oDetail.YLocation;

                        oNewDetails.Add(oClone);
                    }
                }
            }

            (_grdDetails.GridContext as BoundContext).Data = oNewDetails.ToArray();
            _grdDetails.DataBind();            
        }

        //--------------------------------------------
        //
        //--------------------------------------------
        public override void PostExecute(ResultStatus status, Service serviceData)
        {
            base.PostExecute(status, serviceData);
            DrawPanel();
        }
       
    }
}



