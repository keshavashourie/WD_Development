/* Copyright 2024 Siemens */
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

/// <summary>
/// Summary description for UIUtility
/// </summary>

namespace SEMI.AppCode
{
    public class UIUtility
    {
        //-----------------------------------------
        //
        //-----------------------------------------
        public static bool IsPopupClose(Camstar.WebPortal.WebPortlets.MatrixWebPart RefPage)
        {
            if (RefPage.Page.EventArgument == "FloatingFrameSubmitParentPostBackArgument")
                return true;
            else
                return false;
        } // IsPopupClose      

            //-----------------------------------------
        //
        //-----------------------------------------
        public static void SimpleWIPMainActivityPage_OnPostExecute(ResultEventArgs e, Camstar.WebPortal.WebPortlets.MatrixWebPart RefPage)
        {
            if (e.Status.IsSuccess)
            {
                var callStackKeyDM = RefPage.Page.PortalContext.DataContract.GetValueByName<string>("WIPMainCallStackKey");
                if (callStackKeyDM != null)
                    ScriptManager.RegisterStartupScript(RefPage.Page.Form, RefPage.Page.GetType(), "SimpleWIPMain_onActivityPageSubmitted", string.Format("SimpleWIPMain_onActivityPageSubmitted('{0}');", callStackKeyDM), true);
            }
        }


        //-----------------------------------------
        //
        //-----------------------------------------
        public static void LandingPageRefresh(Camstar.WebPortal.WebPortlets.MatrixWebPart RefPage)
        {
            ScriptManager.RegisterStartupScript(RefPage.Page.Form, RefPage.Page.GetType(), "LandingPage_onPageReload", "LandingPage_onPageReload();", true);
        }

        //-----------------------------------------
        //
        //-----------------------------------------
        public static void ClosePopup(Camstar.WebPortal.WebPortlets.MatrixWebPart RefPage, bool NotifyParent = false)
        {
            if (NotifyParent)
                ScriptManager.RegisterStartupScript(RefPage.Page.Form, RefPage.Page.GetType(), "OKButton", "window.parent.__page._isTabContainerPage=null;window.parent.CloseFloatingFrame(true);", true);
            else
                ScriptManager.RegisterStartupScript(RefPage.Page.Form, RefPage.Page.GetType(), "OKButton", "window.parent.__page._isTabContainerPage=null;window.parent.CloseFloatingFrame(false);", true);
        }

        //-----------------------------------------
        //
        //-----------------------------------------
        public static void MaximizePopUp(Camstar.WebPortal.WebPortlets.MatrixWebPart RefPage)
        {
            if (RefPage.Page.IsAJAXFloatingFrame)
                ScriptManager.RegisterStartupScript(RefPage.Page.Form, RefPage.Page.GetType(), "PopUpMaximize", "pop.GetCallerPage().pop.maximize();", true);
        }

        //-----------------------------------------
        //
        //-----------------------------------------
        public static string GetComputerName(Camstar.WebPortal.WebPortlets.MatrixWebPart RefPage)
        {
            string remoteAddrStorageKey = "remote_addr_storage";
            string ip; //= RefPage.Page.Request.ServerVariables["remote_addr"];
			
			ip = RefPage.Page.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
			if (ip == null || ip == "")
			{	
				ip = RefPage.Page.Request.ServerVariables["remote_addr"];
			}

            var compNameStorage = RefPage.Page.Session[remoteAddrStorageKey] as Dictionary<string, string>;
            if (compNameStorage == null)
            {
                compNameStorage = new Dictionary<string, string>();
                RefPage.Page.Session[remoteAddrStorageKey] = compNameStorage;
            }

            string compName = string.Empty;
            try
            {
                if (!compNameStorage.ContainsKey(ip)) // there is no computer name is storage.
                {
                    compName = System.Net.Dns.GetHostEntry(ip).HostName.Split('.')[0];
                    compNameStorage.Add(ip, compName); // add computer name to storage.
                }
                else
                    compName = compNameStorage[ip];
            }
            catch
            {
                compNameStorage.Add(ip, string.Empty); // unable to retrieve computer name - save empty name to storage.
            }

            return compName;
        } // GetComputerName   

        //-----------------------------------------
        //
        //-----------------------------------------
        public static bool AlertMessagesAvailable(string Message, out string[] AlertMessages, out string CompletionMessage, string Delimiter = "@*@")
        {
            if (Message != null)
            {
                string[] Messages = Message.Replace(Delimiter, "|").Split('|');

                int intAlertMessagesCount = Messages.Length - 1;
                AlertMessages = new string[intAlertMessagesCount];

                if (intAlertMessagesCount > 0)
                    for (int i = 0; i < intAlertMessagesCount; i++)
                        AlertMessages[i] = Messages[i + 1];

                CompletionMessage = Messages[0];

                if (intAlertMessagesCount > 0)
                    return true;
                else
                {
                    return false;
                }
            }
            else
            {
                AlertMessages = null;
                CompletionMessage = "";
                return false;
            }
        } // GetAlertMessages

        //-----------------------------------------
        // use to set framelocation 0 in horizon mode for alert popup
        //-----------------------------------------
        public static void SetHorizonAlertPopupFrameLocation(Camstar.WebPortal.WebPortlets.MatrixWebPart RefPage, FloatPageOpenAction objAction)
        {

            var theme = RefPage.Page.Session["CurrentTheme"] != null ? RefPage.Page.Session["CurrentTheme"].ToString() : string.Empty;
            if (theme.ToLower() == "horizon" && objAction != null && (objAction.FrameLocation.Width != 0 || objAction.FrameLocation.Height != 0))
            {
                objAction.FrameLocation.Width = 0;
                objAction.FrameLocation.Height = 0;
            }

            RefPage.Page.ActionDispatcher.ExecuteAction(objAction);
        }

        //-----------------------------------------
        //
        //-----------------------------------------
        public static RecordSet GetLotQuerySelection(Camstar.WebPortal.WebPortlets.MatrixWebPart RefPage, string ServiceType, string LotId, bool SingleLotOnly, 
            ref JQDataGrid TargetGrid, string TargetGridID = "", bool AllowColumnSort = false, string[] HiddenColumnNames = null, string[] CboxColumnNames = null)
        {
            try
            {
                if (TargetGridID == "")
                    TargetGridID = TargetGrid.ID;

                // get the session and user profile
                var fs = FrameworkManagerUtil.GetFrameworkSession();

                // init service objects
                GUIUtilityService oService = new GUIUtilityService(fs.CurrentUserProfile);
                GUIUtility oServiceData = new GUIUtility();
                GUIUtility_Info oServiceInfo = new GUIUtility_Info();
                GUIUtility_Result oServiceResult = new GUIUtility_Result();

                string sTypeNameID = "";

                // input data
                if (ServiceType == "")
                    ServiceType = RefPage.PrimaryServiceType;

                oServiceData.QueryName = "_" + ServiceType + "_Lot";
                oServiceData.IgnoreLotVerification = true;
                oServiceData.LotId = LotId;

                // set the TypeNameID
                sTypeNameID = oServiceData.QueryName.ToString();

                // request data
                oServiceInfo.LotId = FieldInfoUtil.RequestSelectionValue();

                // init request
                GUIUtility_Request oServiceRequest = new GUIUtility_Request();
                oServiceRequest.Info = oServiceInfo;
                // execute!
                ResultStatus oResultStatus = oService.GetEnvironment(oServiceData, oServiceRequest, out oServiceResult);

                if (oResultStatus.IsSuccess)
                {
                    RecordSet rsLots = oServiceResult.Environment.LotId.SelectionValues;
                    GridUtility.ItemListGrid_SetColumns(RefPage, rsLots.GetAsExplicitlyDataTable(), TargetGridID, null, sTypeNameID, AllowColumnSort, HiddenColumnNames, true, null, rsLots.Headers, CboxColumnNames);

                    if (SingleLotOnly)
                        GridUtility.ItemListGrid_BindDataTable(RefPage, rsLots.GetAsExplicitlyDataTable(), ref TargetGrid, sTypeNameID);
                    else
                        GridUtility.ItemListGrid_AddDataRow(RefPage, rsLots.GetAsExplicitlyDataTable(), ref TargetGrid, sTypeNameID);

                    return rsLots;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        } // GetLotQuerySelection

        //-----------------------------------------
        //
        //-----------------------------------------
        public static RecordSet GetLotQuerySelection(Camstar.WebPortal.WebPortlets.MatrixWebPart RefPage, string ServiceType, string LotId)
        {
            try
            {
                // get the session and user profile
                var fs = FrameworkManagerUtil.GetFrameworkSession();

                // init service objects
                GUIUtilityService oService = new GUIUtilityService(fs.CurrentUserProfile);
                GUIUtility oServiceData = new GUIUtility();
                GUIUtility_Info oServiceInfo = new GUIUtility_Info();
                GUIUtility_Result oServiceResult = new GUIUtility_Result();

                string sTypeNameID = "";

                // input data
                if (ServiceType == "")
                    ServiceType = RefPage.PrimaryServiceType;

                oServiceData.QueryName = "_" + ServiceType + "_Lot";
                oServiceData.IgnoreLotVerification = true;
                oServiceData.LotId = LotId;

                // set the TypeNameID
                sTypeNameID = oServiceData.QueryName.ToString();

                // request data
                oServiceInfo.LotId = FieldInfoUtil.RequestSelectionValue();

                // init request
                GUIUtility_Request oServiceRequest = new GUIUtility_Request();
                oServiceRequest.Info = oServiceInfo;
                // execute!
                ResultStatus oResultStatus = oService.GetEnvironment(oServiceData, oServiceRequest, out oServiceResult);

                if (oResultStatus.IsSuccess)
                {
                    RecordSet rsLots = oServiceResult.Environment.LotId.SelectionValues;
                    return rsLots;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        } // GetLotQuerySelection

        //===============================================================
        // SetCapital
        //===============================================================
        // This function set the value in the Textbox to Uppercase
        // Parameters:
        //       Control        
        //       - Control of the Textbox
        //       UpperCase   
        //       - Boolean to determine if value in Control is to be set to Uppercase
        //===============================================================
        public static void SetCapital(Camstar.WebPortal.FormsFramework.WebControls.TextBox Control, Boolean UpperCase)
        {
            try
            {
                if (UpperCase && Control.TextControl.Text != "")
                    Control.TextControl.Text = Control.TextControl.Text.ToUpper();
            }
            catch (Exception Ex)
            {
                throw new Exception(Ex.TargetSite.Name + "(): " + Ex.Message);
            }
        } // SetCapital

        public static void DisableMatrixFields(Camstar.WebPortal.WebPortlets.MatrixWebPart RefPage, string viewDM, CamstarControlsCollection matrixFields)
        {
            if (viewDM != null && viewDM == "View")
            {
                for (int x = 0; x < matrixFields.Count; x++)
                {
                    string sControlID = matrixFields[x].Control.ID;
                    string sControlType = matrixFields[x].Control.GetType().Name;

                    switch (sControlType)
                    {
                        case "NamedObject":
                            CWC.NamedObject ndoControl = RefPage.Page.FindCamstarControl(sControlID) as CWC.NamedObject;
                            ndoControl.Enabled = false;
                            break;
                        case "RevisionedObject":
                            CWC.RevisionedObject rdoControl = RefPage.Page.FindCamstarControl(sControlID) as CWC.RevisionedObject;
                            rdoControl.Enabled = false;
                            break;
                        case "TextBox":
                            CWC.TextBox txtControl = RefPage.Page.FindCamstarControl(sControlID) as CWC.TextBox;
                            txtControl.Enabled = false;
                            break;
                        case "CheckBox":
                            CWC.CheckBox chkControl = RefPage.Page.FindCamstarControl(sControlID) as CWC.CheckBox;
                            chkControl.Enabled = false;
                            break;
                        case "DropDownList":
                            CWC.DropDownList ddlControl = RefPage.Page.FindCamstarControl(sControlID) as CWC.DropDownList;
                            ddlControl.Enabled = false;
                            break;
                        case "DateChooser":
                            CWC.DateChooser dateControl = RefPage.Page.FindCamstarControl(sControlID) as CWC.DateChooser;
                            dateControl.Enabled = false;
                            break;
                        case "WorkflowNavigator":
                            CWC.WorkflowNavigator WfNavigatorControl = RefPage.Page.FindCamstarControl(sControlID) as CWC.WorkflowNavigator;
                            var stackControl = RefPage.Page.FindCamstarControl(WfNavigatorControl.ClientID + "_Stack") as FieldControl;
                            WfNavigatorControl.Enabled = false;
                            break;
                    }
                }
            }
        }
    }
}



