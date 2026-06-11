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
/// Summary description for scsWIPMainTxnRibbon
/// </summary>
/// 
namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class scsShopfloorTxnPanel : MatrixWebPart
    {
        // WIP Main txn tile
        protected CWC.PagePanel _MoveInTile { get { return Page.FindCamstarControl("MoveInTile") as CWC.PagePanel; } }
        protected CWC.PagePanel _TrackInTile { get { return Page.FindCamstarControl("TrackInTile") as CWC.PagePanel; } }
        protected CWC.PagePanel _TrackOutTile { get { return Page.FindCamstarControl("TrackOutTile") as CWC.PagePanel; } }
        protected CWC.PagePanel _MoveOutTile { get { return Page.FindCamstarControl("MoveOutTile") as CWC.PagePanel; } }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            InitializeActivityTiles();
        }
           

        public void InitializeActivityTiles()
        {

            int iTileHeight = 32;
            int iTileWidth = 250;

            RenderTile(_MoveInTile, "MoveInTile", iTileHeight, iTileWidth, "Move In", "MoveIn");
            RenderTile(_TrackInTile, "TrackInTile", iTileHeight, iTileWidth, "Track In", "TrackIn");
            RenderTile(_TrackOutTile, "TrackOutTile", iTileHeight, iTileWidth, "Track Out", "TrackOut");
            RenderTile(_MoveOutTile, "MoveOutTile", iTileHeight, iTileWidth, "Move Out", "MoveOut");
        }

        public void RenderTile(CWC.PagePanel Tile, string sTileID, int iTileHeight, int iTileWidth, string sActivity, string sCssActivity) {
            Tile.Controls.Clear();

            SWC.Panel oTileDiv = new SWC.Panel();
            oTileDiv.ID = sTileID;
            //oTileDiv.CssClass = "WIPMain_Txn_";
            oTileDiv.Width = iTileWidth;
            oTileDiv.Height = iTileHeight;
            oTileDiv.ScrollBars = SWC.ScrollBars.None;
            oTileDiv.Style["position"] = "relative";
            string sStatusHTML = "<span class='WIPMain_Message'>" + sActivity + "</span>";
            oTileDiv.Controls.Add(new LiteralControl(sStatusHTML));
      
            Tile.Controls.Add(oTileDiv);     

        }
    }
}