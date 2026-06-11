// Copyright Siemens 2023  
using System;
using System.Collections.Generic;
using System.Text;

namespace Camstar.WebPortal.WebPortlets.Concierge
{
    public class ConciergeConstants
    {
        public const string Title = "Concierge";
        public const string InternationalFormat = "d MMM yyyy";
        public const int GreenMinRange = 3;
        public const int YellowMinRange = 0;
        public const int YellowMaxRange = 2;
        public const int RedMinRange = -1;
    }

   
    public class QualityObjectType
    {
        public const string Event = "Event";
    }

    public class Images
    {
        public const string RedSquare = "images/wpzonechrome/status-redsquare.png";
        public const string YellowTriange = "images/wpzonechrome/status-yellowtriangle.png";
        public const string GreenSquare = "images/wpzonechrome/status-greencircle.png";
        public const string ConciergeIconPath = "images/PortalIcons/";
        public const string ConciergeIconPhysicalPath = "\\Images\\PortalIcons\\";
        public const string DefaultIconImagePath = "images/PortalIcons/mailbox_fav_32.gif";

        public const string FirstImage = "images/picklist/page-first.png";
        public const string FirstImageDisabled = "images/picklist/page-first-d.png";
        public const string LastImage = "images/picklist/page-last.png";
        public const string LastImageDisabled = "images/picklist/page-last-d.png";
        public const string NextImage = "images/picklist/page-fwd.png";
        public const string NextImageDisabled = "images/picklist/page-fwd-d.png";
        public const string PreviousImage = "images/picklist/page-back.png";
        public const string PreviousImageDisabled = "images/picklist/page-back-d.png";
        public const string ConciergeTriangleRightPath = "Themes/Horizon/Images/Icons/icon-expand-16x16.svg";

    }

    public class CSSConstants
    {
        public const string ConciergeSectionHeader = "divSectionHeader";
        public const string ConciergeSectionContainer = "divSectionContainer";
        public const string TaskItemHidden = "trHidden";
        public const string ConciergeSectionExpand = "tdExpand";
    }

    public class AttributeKeys
    {
        public const string ID = "id";
        public const string Class = "class";
        public const string OnClick = "onclick";
    }

    public class AttributeValues
    {
        public const string TableClientPagerIDTemplate = "{0}_tblClientPage";
        public const string ImageFirstIDTemplate = "{0}_imgFirst";
        public const string ImageLastIDTemplate = "{0}_imgLast";
        public const string ImagePreviousIDTemplate = "{0}_imgPrevious";
        public const string ImageNextIDTemplate = "imgNext";
        public const string ClientSidePageStartUpScriptTemplate = "<script type=\"text/javascript\"> var clientPage = new Camstar.Controls.ClientPager(\"{0}\");</script>";
    }

    public class ToDoListItemTypeIDs
    {
        public const int ProcessModel = 7697;
        public const int Phase = 7695;
        public const int Plan = 7696;
        public const int Activity = 7694;
        public const int Event = 7489;
        public const int CAPA = 7647;
    }

    public class ToDoListItemTypeDescriptions
    {
        public const string ProcessModel = "ProcessModel";
        public const string Phase = "Phase";
        public const string Plan = "Plan";
        public const string Activity = "Activity";
        public const string Event = "Event";
        public const string CAPA = "CAPA";
    }

    public class DisplayMessages
    {
        public const string ProcessModel = "Process Model has been assigned to you";
        public const string Phase = "Phase has been assigned to you";
        public const string Plan = "Plan has been assigned to you";
        public const string Activity = "Activity has been assigned to you";
        public const string Event = "has been assigned to you";
        public const string CAPA = "has been assigned to you";
    }
}
