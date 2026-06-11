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

using SWC = System.Web.UI.WebControls;

/// <summary>
/// Summary description for scsWIPMainActivity
/// </summary>
/// 
namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class scsWIPMainActivity:MatrixWebPart
    {
        protected CWC.PagePanel _ActivityTile00 { get { return Page.FindCamstarControl("ActivityTile00") as CWC.PagePanel; } }
        protected CWC.PagePanel _ActivityTile01 { get { return Page.FindCamstarControl("ActivityTile01") as CWC.PagePanel; } }
        protected CWC.PagePanel _ActivityTile02 { get { return Page.FindCamstarControl("ActivityTile02") as CWC.PagePanel; } }
        protected CWC.PagePanel _ActivityTile03 { get { return Page.FindCamstarControl("ActivityTile03") as CWC.PagePanel; } }
        protected CWC.PagePanel _ActivityTile04 { get { return Page.FindCamstarControl("ActivityTile04") as CWC.PagePanel; } }
        protected CWC.PagePanel _ActivityTile05 { get { return Page.FindCamstarControl("ActivityTile05") as CWC.PagePanel; } }
        protected CWC.PagePanel _ActivityTile06 { get { return Page.FindCamstarControl("ActivityTile06") as CWC.PagePanel; } }
        protected CWC.PagePanel _ActivityTile07 { get { return Page.FindCamstarControl("ActivityTile07") as CWC.PagePanel; } }
        protected CWC.PagePanel _ActivityTile08 { get { return Page.FindCamstarControl("ActivityTile08") as CWC.PagePanel; } }
        protected CWC.PagePanel _ActivityTile09 { get { return Page.FindCamstarControl("ActivityTile09") as CWC.PagePanel; } }
        protected CWC.PagePanel _ActivityTile010 { get { return Page.FindCamstarControl("ActivityTile010") as CWC.PagePanel; } }
        protected CWC.PagePanel _ActivityTile011 { get { return Page.FindCamstarControl("ActivityTile011") as CWC.PagePanel; } }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            InitializeActivityTiles();
        }


        public void InitializeActivityTiles()
        {
            string sActivity = "#ACTIVITY#";
            string sStatus = "#STATUS#";
            string sMessage = "#MESSAGE#";
            string sSubMessage = "#SUBMESSAGE#";

            int iTileHeight = 80;
            int iTileWidth = 350;

            RenderTile(_ActivityTile00, "ActivityTile00", iTileHeight, iTileWidth, sStatus, sMessage, sSubMessage, sActivity);
            RenderTile(_ActivityTile01, "ActivityTile01", iTileHeight, iTileWidth, sStatus, sMessage, sSubMessage, sActivity);
            RenderTile(_ActivityTile02, "ActivityTile02", iTileHeight, iTileWidth, sStatus, sMessage, sSubMessage, sActivity);
            RenderTile(_ActivityTile03, "ActivityTile03", iTileHeight, iTileWidth, sStatus, sMessage, sSubMessage, sActivity);
            RenderTile(_ActivityTile04, "ActivityTile04", iTileHeight, iTileWidth, sStatus, sMessage, sSubMessage, sActivity);
            RenderTile(_ActivityTile05, "ActivityTile05", iTileHeight, iTileWidth, sStatus, sMessage, sSubMessage, sActivity);
            RenderTile(_ActivityTile06, "ActivityTile06", iTileHeight, iTileWidth, sStatus, sMessage, sSubMessage, sActivity);
            RenderTile(_ActivityTile07, "ActivityTile07", iTileHeight, iTileWidth, sStatus, sMessage, sSubMessage, sActivity);
            RenderTile(_ActivityTile08, "ActivityTile08", iTileHeight, iTileWidth, sStatus, sMessage, sSubMessage, sActivity);
            RenderTile(_ActivityTile09, "ActivityTile09", iTileHeight, iTileWidth, sStatus, sMessage, sSubMessage, sActivity);
            RenderTile(_ActivityTile010, "ActivityTile010", iTileHeight, iTileWidth, sStatus, sMessage, sSubMessage, sActivity);
            RenderTile(_ActivityTile011, "ActivityTile011", iTileHeight, iTileWidth, sStatus, sMessage, sSubMessage, sActivity);
        }

        public void RenderTile(CWC.PagePanel Tile, string sTileID, int iTileHeight, int iTileWidth, string sStatus, string sMessage, string sSubMessage, string sActivity)
        {
            Tile.Controls.Clear();

            // main container panel            
            SWC.Panel oTileDiv = new SWC.Panel();
            oTileDiv.ID = sTileID;
            oTileDiv.CssClass = "WIPMain_Activity_MainDiv";
            oTileDiv.Width = iTileWidth;
            oTileDiv.Height = iTileHeight;
            oTileDiv.ScrollBars = SWC.ScrollBars.None;
            oTileDiv.Style["position"] = "relative";
            oTileDiv.Attributes.Add("activity", sActivity);

            // status div
            SWC.Panel oStatusDiv = new SWC.Panel();
            oStatusDiv.ID = sTileID + "_StatusDiv";
            oStatusDiv.Style["position"] = "absolute";
            oStatusDiv.Style["top"] = "2px";
            oStatusDiv.Style["left"] = "2px";
            oStatusDiv.CssClass = "WIPMain_Activity_StatusDiv_Optional";
            oStatusDiv.Height = (iTileHeight - 4);
            oStatusDiv.Width = 6;

            // icon div
            SWC.Panel oIconDiv = new SWC.Panel();
            oIconDiv.ID = sTileID + "_IconDiv";
            oIconDiv.Style["position"] = "absolute";
            oIconDiv.Style["top"] = "15px";
            oIconDiv.Style["left"] = "13px";
            oIconDiv.CssClass = "WIPMain_Activity_IconDiv";
            oIconDiv.Height = (iTileHeight - 30);
            oIconDiv.Width = 70;

            // icon
            SWC.Image oIcon = new SWC.Image();
            oIcon.ID = sTileID + "_Icon";
            oIcon.CssClass = "WIPMain_Activity_Icon";
            oIcon.ImageUrl = "assets/image/typeAction48.svg";
            oIcon.Style["vertical-align"] = "middle";
            oIcon.Style["horizontal-align"] = "center";
            oIcon.Height = 40;
            oIcon.Width = 40;

            // message div
            SWC.Panel oMessageDiv = new SWC.Panel();
            oMessageDiv.ID = sTileID + "_MessageDiv";
            oMessageDiv.Style["position"] = "absolute";
            oMessageDiv.Style["top"] = "5px";
            oMessageDiv.Style["left"] = "65px";
            oMessageDiv.CssClass = "WIPMain_Activity_MessageDiv";
            oMessageDiv.Height = 15;
            /*oMessageDiv.Width = 200;*/
            string sMessageHTML = "<span class='WIPMain_Activity_Message'>" + sMessage + "</span>";
            oMessageDiv.Controls.Add(new LiteralControl(sMessageHTML));

            // sub message            
            SWC.Panel oSubMsgDiv = new SWC.Panel();
            oSubMsgDiv.ID = sTileID + "_SubMessageDiv";
            oSubMsgDiv.Style["position"] = "absolute";
            oSubMsgDiv.Style["top"] = "17px";
            oSubMsgDiv.Style["left"] = "65px";
            oSubMsgDiv.CssClass = "WIPMain_Activity_SubMessageDiv";
            oSubMsgDiv.Height = 13;
            oSubMsgDiv.Width = 200;
            string sSubMessageHTML = "<span class='WIPMain_Activity_SubMessage'>" + sSubMessage + "</span>";
            oSubMsgDiv.Controls.Add(new LiteralControl(sSubMessageHTML));

            // status message div
            SWC.Panel oStatusMessageDiv = new SWC.Panel();
            oStatusMessageDiv.ID = sTileID + "_StatusMessageDiv";
            oStatusMessageDiv.Style["position"] = "absolute";
            oStatusMessageDiv.Style["top"] = "55px";
            oStatusMessageDiv.Style["left"] = "65px";
            oStatusMessageDiv.CssClass = "WIPMain_Activity_StatusMessageDiv";
            oStatusMessageDiv.Height = 8;
            oStatusMessageDiv.Width = 200;
            string sStatusHTML = "<span class='WIPMain_Activity_StatusMessage'>" + sStatus + "</span>";
            oStatusMessageDiv.Controls.Add(new LiteralControl(sStatusHTML));

            oIconDiv.Controls.Add(oIcon);
            oTileDiv.Controls.Add(oStatusDiv);
            oTileDiv.Controls.Add(oIconDiv);
            oTileDiv.Controls.Add(oMessageDiv);
            oTileDiv.Controls.Add(oSubMsgDiv);
            oTileDiv.Controls.Add(oStatusMessageDiv);

            Tile.Controls.Add(oTileDiv);
        }
    }
}