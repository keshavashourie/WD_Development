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
/// Summary description for SS_MaintenanceScheduleInquiry
/// </summary>
namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class SS_MaintenanceScheduleInquiry : MatrixWebPart
    {
        //variables declaration
        protected CWC.NamedObject _ndoResourceGroup { get { return Page.FindCamstarControl("MaintenanceScheduleInquiry_ResourceGroup") as CWC.NamedObject; } }
        protected CWC.NamedObject _ndoResource { get { return Page.FindCamstarControl("MaintenanceScheduleInquiry_Resource") as CWC.NamedObject; } }
        protected CWC.DateChooser _dateStartDate { get { return Page.FindCamstarControl("MaintenanceScheduleInquiry_ScheduleStartDate") as CWC.DateChooser; } }
        protected CWC.DateChooser _dateEndDate { get { return Page.FindCamstarControl("MaintenanceScheduleInquiry_ScheduleEndDate") as CWC.DateChooser; } }
        protected CWC.DropDownList _ddlTimeScale { get { return Page.FindCamstarControl("MaintenanceScheduleInquiry_TimeScale") as CWC.DropDownList; } }
        protected CWC.CheckBox _chkShowAll { get { return Page.FindCamstarControl("MaintenanceScheduleInquiry_ShowAll") as CWC.CheckBox; } }
        protected CWC.TextBox _txtCurrentPage { get { return Page.FindCamstarControl("CurrentPageTxt") as CWC.TextBox; } }
        protected CWC.TextBox _txtTotalPage { get { return Page.FindCamstarControl("TotalPageTxt") as CWC.TextBox; } }
        protected JQDataGrid _gridResourceSchedules { get { return Page.FindCamstarControl("MaintenanceScheduleInquiry_ResourceSchedules") as JQDataGrid; } }
        protected CWC.PagePanel _pnlCanvasPanel { get { return Page.FindCamstarControl("CanvasPanel") as CWC.PagePanel; } }
        protected CWC.Button _btnFetch { get { return Page.FindCamstarControl("FetchBtn") as CWC.Button; } }
        protected CWC.Button _btnPrevious { get { return Page.FindCamstarControl("previousBtn") as CWC.Button; } }
        protected CWC.Button _btnNext { get { return Page.FindCamstarControl("nextBtn") as CWC.Button; } }
        protected CWC.Label _lblPageNo { get { return Page.FindCamstarControl("PageNo") as CWC.Label; } }

        int rowsPerPage = 10; //default rowsPerPage is 10

        //----------------------------------
        // OnLoad Event
        //----------------------------------
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            ScriptManager.RegisterClientScriptBlock(_pnlCanvasPanel, this.GetType(), "CanvasScript", "MaintenanceScheduleInquiryScript();", true); //register javascript
        }

        //----------------------------------
        // Fetch Schedule Overview
        //----------------------------------
        private ResultStatus FetchScheduleOverview()
        {
            //pagination calculation
            int currentPage = Convert.ToInt32(_txtCurrentPage.Data.ToString());
            int startRow = 1;
            if (currentPage > 1)
                startRow = ( rowsPerPage * (currentPage - 1)) + 1;

            int stopRow = rowsPerPage * currentPage;

            _pnlCanvasPanel.Controls.Clear();

            //Initialize Service & Objects
            UserProfile profile = FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile;
            MaintenanceScheduleInquiryService Svc = new MaintenanceScheduleInquiryService(profile);
            MaintenanceScheduleInquiry SvcData = new MaintenanceScheduleInquiry();
            MaintenanceScheduleInquiry_Info SvcInfo = new MaintenanceScheduleInquiry_Info();
            MaintenanceScheduleInquiry_Request ReqData = new MaintenanceScheduleInquiry_Request();
            MaintenanceScheduleInquiry_Result ResData = new MaintenanceScheduleInquiry_Result();

            //Set Input Data
            if (_ndoResourceGroup.Data != null)
            {
                SvcData.ResourceGroup = new NamedObjectRef();
                SvcData.ResourceGroup.Name = _ndoResourceGroup.Data.ToString();
            }

            if (_ndoResource.Data != null)
            {
                SvcData.Resource = new NamedObjectRef();
                SvcData.Resource.Name = _ndoResource.Data.ToString();
            }

            if (_dateStartDate.Data != null)
            {
                SvcData.ScheduleStartDate = Convert.ToDateTime(_dateStartDate.Data.ToString());
            }

            if (_dateEndDate.Data != null)
            {
                SvcData.ScheduleEndDate = Convert.ToDateTime(_dateEndDate.Data.ToString());
            }

            if (_chkShowAll.Data != null)
            {
                SvcData.ShowAll = Convert.ToBoolean(_chkShowAll.Data.ToString());
            }

            if (_ddlTimeScale.Data != null)
            {
                SvcData.TimeScale = new Enumeration<TimeScaleEnum, int>();
                SvcData.TimeScale.Value = Convert.ToInt32(_ddlTimeScale.Data.ToString());
            }

            //set start row num
            SvcData.STARTROWNUM = new Primitive<int>();
            SvcData.STARTROWNUM.Value = startRow;

            //set stop row num
            SvcData.STOPROWNUM = new Primitive<int>();
            SvcData.STOPROWNUM.Value = stopRow;

            //Set Request Value
            SvcInfo.ResourceSchedules = new ResourceSchedules_Info();
            SvcInfo.ResourceSchedules.RequestValue = true;
            ReqData.Info = SvcInfo;

            //Execute Validate User
            ResultStatus Validations = new ResultStatus();
            Validations = Svc.Validate_User(SvcData, ReqData, out ResData);
            if (Validations.IsSuccess)
            {
                //Execute Request
                ResultStatus Results = new ResultStatus();
                if (_ndoResourceGroup.Data != null) //2 sets of similar queries with ResourceGroup the only difference 
                    Results = Svc.GetOverview1(SvcData, ReqData, out ResData);
                else
                    Results = Svc.GetOverview2(SvcData, ReqData, out ResData);

                if (Results.IsSuccess)
                {
                    if (ResData.Value.ResourceSchedules != null)
                    {
                        _txtTotalPage.Data = ((ResData.Value.ResourceSchedules[0].TotalRows.Value - 1) / rowsPerPage) + 1; //calculate total page based on query result
                        foreach (ResourceSchedules Schedule in ResData.Value.ResourceSchedules)
                        {
                            if (Schedule.MaintenanceReqType.ToString().Equals("RecurringDateReq")) //if it is RecurringDateReq, get the Occurences using GetRecurringDateReqDetails event
                            {
                                //Initialize Service & Objects
                                MaintenanceScheduleInquiryService Svc2 = new MaintenanceScheduleInquiryService(profile);
                                MaintenanceScheduleInquiry SvcData2 = new MaintenanceScheduleInquiry();
                                MaintenanceScheduleInquiry_Info SvcInfo2 = new MaintenanceScheduleInquiry_Info();
                                MaintenanceScheduleInquiry_Request ReqData2 = new MaintenanceScheduleInquiry_Request();
                                MaintenanceScheduleInquiry_Result ResData2 = new MaintenanceScheduleInquiry_Result();

                                //Set Input Data
                                if (Schedule.Resource != null)
                                {
                                    SvcData2.Resource = new NamedObjectRef();
                                    SvcData2.Resource = Schedule.Resource;
                                }

                                if (Schedule.MaintenanceReq != null)
                                {
                                    SvcData2.TempMaintenanceReq = new RevisionedObjectRef();
                                    SvcData2.TempMaintenanceReq = Schedule.MaintenanceReq;
                                }

                                if (_dateStartDate.Data != null)
                                {
                                    SvcData2.ScheduleStartDate = Convert.ToDateTime(_dateStartDate.Data.ToString());
                                }

                                if (_dateEndDate.Data != null)
                                {
                                    SvcData2.ScheduleEndDate = Convert.ToDateTime(_dateEndDate.Data.ToString());
                                }

                                //Set Request Value
                                SvcInfo2.TempDateOccurences = new DateOccurences_Info();
                                SvcInfo2.TempDateOccurences.RequestValue = true;
                                ReqData2.Info = SvcInfo2;

                                //Execute Request
                                ResultStatus Results2 = Svc2.GetRecurringDateReqDetails(SvcData2, ReqData2, out ResData2);
                                if (Results2.IsSuccess)
                                {
                                    if (ResData2.Value.TempDateOccurences != null)
                                    {
                                        Schedule.DateOccurences = ResData2.Value.TempDateOccurences; //transfer the occurences to the Schedule object
                                    }
                                }
                            }
                        }

                        DrawCanvas(ResData.Value.ResourceSchedules); //Draw Tables in the Panel
                    }
                    else //if schedules is empty
                    {
                        _txtCurrentPage.Data = 1;
                        _txtTotalPage.Data = 1;
                        _pnlCanvasPanel.Controls.Clear();
                        CamstarWebControl.SetRenderToClient(_pnlCanvasPanel); //clear the panel when no data is found
                    }
                }
            }

            return Validations;
        }

        //-----------------------------------------
        // Draw Canvas
        //-----------------------------------------
        private void DrawCanvas(ResourceSchedules[] Schedules)
        {
            _pnlCanvasPanel.Controls.Clear();
            int maxWidth = 500;
            int detailPanelWidth = 225;
            string sDetailsPanelLayout = "<br>";
            DateTime startDate = Convert.ToDateTime(_dateStartDate.Data.ToString());
            DateTime endDate = Convert.ToDateTime(_dateEndDate.Data.ToString());
            TimeSpan duration = endDate - startDate;

            // create the drawing area container (to enable the scrollbar)
            SWC.Panel pnlDrawAreaFrame = new SWC.Panel();
            pnlDrawAreaFrame.ID = "canvasAreaFrame";
            pnlDrawAreaFrame.CssClass = "canvasAreaFrameClass";
            pnlDrawAreaFrame.Style["position"] = "relative";
            pnlDrawAreaFrame.Width = _pnlCanvasPanel.Width;
            pnlDrawAreaFrame.Height = int.Parse(_pnlCanvasPanel.Height.Value.ToString());
            pnlDrawAreaFrame.BorderStyle = System.Web.UI.WebControls.BorderStyle.None;
            pnlDrawAreaFrame.BorderWidth = 1;
            pnlDrawAreaFrame.ScrollBars = System.Web.UI.WebControls.ScrollBars.Auto;

            SWC.Panel pnlDetails = new SWC.Panel(); // create another panel for tables
            foreach (ResourceSchedules Schedule in Schedules)
            {
                SWC.Panel pnlScheduleTable = new SWC.Panel();
                //pnlScheduleTable.Style["position"] = "relative";

                if (Schedule.MaintenanceReqType == "DateReq" || Schedule.MaintenanceReqType == "RecurringDateReq")
                {
                    sDetailsPanelLayout += "" +
                    @"<table style='padding:0px 0px 0px 0px;  border:1px solid'>" +
                        @"<tr>" +
                            @"<td>" +
                                @"<table><tr><td style='border:0px solid; font-size:small; font-family:Tahoma; width:100px;overflow:hidden;display:inline-block;white-space:nowrap'> " + Schedule.Resource.Name + " </td></tr></table>" +
                            @"</td>" +
                            @"<td>
                        <table>
                             <tr style='font-size:small; font-family:Tahoma; background-color:white'>
                                <td style='width:250px;border:1px solid;overflow:hidden;display:inline-block;white-space:nowrap'>&nbsp;PM / Date</td>";

                    pnlScheduleTable.Controls.Add(new LiteralControl(sDetailsPanelLayout));
                    sDetailsPanelLayout = "";

                    //Check if time scale is Day, Week, or Month
                    if (_ddlTimeScale.Data.ToString() == "1") //TimeScale is Day
                    {
                        int totalDays = Convert.ToInt32(duration.TotalDays);
                        int startDay = startDate.Day;
                        int singleWidth = maxWidth / totalDays;
                        DateTime TempStartDate1 = startDate;

                        for (int i = startDay; i <= totalDays + startDay; i++)
                        {
                            sDetailsPanelLayout += "<td style='width:" + singleWidth + "px; border:1px solid'>" + TempStartDate1.Month.ToString("00") + "/" + TempStartDate1.Day.ToString("00") + "</td>";
                            TempStartDate1 = TempStartDate1.AddDays(1);
                        }
                        sDetailsPanelLayout += @"</tr>
                            <tr style='background-color:lightgreen;'>
                                <td style='border:0px solid; font-size:small; font-family:Tahoma; background-color:white'>&nbsp;" + Schedule.MaintenanceReq.Name + ":" + Schedule.MaintenanceReq.Revision + "</td>";

                        pnlScheduleTable.Controls.Add(new LiteralControl(sDetailsPanelLayout));
                        sDetailsPanelLayout = "";

                        DateTime TempStartDate2 = startDate;
                        for (int i = startDay; i <= totalDays + startDay; i++)
                        {
                            if (Schedule.MaintenanceReqType == "RecurringDateReq")
                            {
                                if (Schedule.DateOccurences != null)
                                {
                                    var value = Schedule.DateOccurences.FirstOrDefault(item => item.DueDateDay == TempStartDate2.Day && item.DueDateMonth == TempStartDate2.Month && item.DueDateYear == TempStartDate2.Year);
                                    var filteredOccurences = Schedule.DateOccurences.Where(item => item.DueDateDay == TempStartDate2.Day && item.DueDateMonth == TempStartDate2.Month && item.DueDateYear == TempStartDate2.Year);
                                    if (value != null)
                                    {
                                        SWC.Panel pnlScheduleDue = new SWC.Panel();
                                        pnlScheduleDue.Style["position"] = "relative";
                                        pnlScheduleDue.CssClass = "ScheduleDue";
                                        sDetailsPanelLayout += " <td class='ScheduleDue' style='background-color:red'><div style='position: relative;' class='ScheduleDuePosition'> ";
                                        pnlScheduleDue.Controls.Add(new LiteralControl(sDetailsPanelLayout));
                                        sDetailsPanelLayout = "";

                                        //---------details panel---------
                                        SWC.Panel pnlScheduleDetails = new SWC.Panel();
                                        pnlScheduleDetails.CssClass = "ScheduleDetails";
                                        pnlScheduleDetails.Style["position"] = "absolute";
                                        pnlScheduleDetails.Width = detailPanelWidth;
                                        //if (singleWidth > 30)
                                        //    pnlScheduleDetails.Style["left"] = singleWidth + "px";
                                        //else
                                        //    pnlScheduleDetails.Style["left"] = "30px";
                                        pnlScheduleDetails.Style["z-index"] = "999"; // set the z-index to a large value so that the popup will be ontop of other elements

                                        foreach (DateOccurences occurence in filteredOccurences)
                                        {
                                            string sHTML = "<span class='ScheduleDetails_FormatedDate'>#DATE#</span>";
                                            sHTML = sHTML.Replace("#DATE#", Convert.ToDateTime(occurence.DueDate.ToString()).ToString("dddd, dd MMMM yyyy HH:mm"));
                                            pnlScheduleDetails.Controls.Add(new LiteralControl(sHTML));
                                        }

                                        pnlScheduleDue.Controls.Add(pnlScheduleDetails);
                                        sDetailsPanelLayout = "</div></td> ";
                                        pnlScheduleDue.Controls.Add(new LiteralControl(sDetailsPanelLayout));
                                        sDetailsPanelLayout = "";
                                        pnlScheduleTable.Controls.Add(pnlScheduleDue);
                                    }
                                    else
                                    {
                                        sDetailsPanelLayout += " <td></td> ";
                                        pnlScheduleTable.Controls.Add(new LiteralControl(sDetailsPanelLayout));
                                        sDetailsPanelLayout = "";
                                    }
                                }
                                else
                                {
                                    sDetailsPanelLayout += " <td></td> ";
                                    pnlScheduleTable.Controls.Add(new LiteralControl(sDetailsPanelLayout));
                                    sDetailsPanelLayout = "";
                                }
                            }
                            else if (Schedule.MaintenanceReqType == "DateReq")
                            {
                                if (Schedule.ScheduleDateDay != null)
                                {
                                    if (Schedule.ScheduleDateDay == TempStartDate2.Day && Schedule.ScheduleDateMonth == TempStartDate2.Month && Schedule.ScheduleDateYear == TempStartDate2.Year)
                                    {
                                        SWC.Panel pnlScheduleDue = new SWC.Panel();
                                        pnlScheduleDue.CssClass = "ScheduleDue";
                                        pnlScheduleDue.Style["position"] = "relative";
                                        sDetailsPanelLayout += " <td class='ScheduleDue' style='background-color:red'><div style='position: relative;' class='ScheduleDuePosition'> ";
                                        pnlScheduleDue.Controls.Add(new LiteralControl(sDetailsPanelLayout));
                                        sDetailsPanelLayout = "";

                                        //---------details panel---------
                                        SWC.Panel pnlScheduleDetails = new SWC.Panel();
                                        pnlScheduleDetails.CssClass = "ScheduleDetails";
                                        pnlScheduleDetails.Style["position"] = "absolute";
                                        pnlScheduleDetails.Width = detailPanelWidth;
                                        //if (singleWidth > 30)
                                        //    pnlScheduleDetails.Style["left"] = singleWidth + "px";
                                        //else
                                        //    pnlScheduleDetails.Style["left"] = "30px";
                                        pnlScheduleDetails.Style["z-index"] = "999"; // set the z-index to a large value so that the popup will be ontop of other elements
                                        string sHTML = "<span class='ScheduleDetails_FormatedDate'>#DATE#</span>";
                                        sHTML = sHTML.Replace("#DATE#", Convert.ToDateTime(Schedule.ScheduleDate.ToString()).ToString("dddd, dd MMMM yyyy HH:mm"));
                                        pnlScheduleDetails.Controls.Add(new LiteralControl(sHTML));

                                        pnlScheduleDue.Controls.Add(pnlScheduleDetails);
                                        sDetailsPanelLayout = "</div></td> ";
                                        pnlScheduleDue.Controls.Add(new LiteralControl(sDetailsPanelLayout));
                                        sDetailsPanelLayout = "";
                                        pnlScheduleTable.Controls.Add(pnlScheduleDue);
                                    }
                                    else
                                    {
                                        sDetailsPanelLayout += " <td></td> ";
                                        pnlScheduleTable.Controls.Add(new LiteralControl(sDetailsPanelLayout));
                                        sDetailsPanelLayout = "";
                                    }
                                }
                                else
                                {
                                    sDetailsPanelLayout += " <td></td> ";
                                    pnlScheduleTable.Controls.Add(new LiteralControl(sDetailsPanelLayout));
                                    sDetailsPanelLayout = "";
                                }
                            }
                            TempStartDate2 = TempStartDate2.AddDays(1);
                        }
                    }
                    else if (_ddlTimeScale.Data.ToString() == "2") //TimeScale is Week
                    {
                        DateTime lastdayofYear = new DateTime(startDate.Year, 12, 31);
                        int lastweekofYear = GetWeekNumber(lastdayofYear);
                        //int totalWeeks = Convert.ToInt32(duration.TotalDays/7);
                        int startWeek = GetWeekNumber(startDate);
                        int endWeek = GetWeekNumber(endDate);
                        if (endWeek < startWeek)
                        {
                            endWeek = endWeek + lastweekofYear;
                        }
                        else if (endWeek == startWeek && startDate.Year < endDate.Year)
                        {
                            endWeek = endWeek + lastweekofYear - 1;
                        }
                        int totalWeeks = endWeek - startWeek;
                        int singleWidth = 500;
                        if (totalWeeks > 0)
                            singleWidth = maxWidth / totalWeeks;
                        DateTime TempStartDate1 = startDate;

                        for (int i = startWeek; i <= totalWeeks + startWeek; i++)
                        {
                            sDetailsPanelLayout += "<td style='width:" + singleWidth + "px; border:1px solid'>W" + GetWeekNumber(TempStartDate1).ToString("00") + "</td>";
                            TempStartDate1 = TempStartDate1.AddDays(7);
                        }
                        sDetailsPanelLayout += @"</tr>
                            <tr style='background-color:lightgreen;'>
                                <td style='border:0px solid; font-size:small; font-family:Tahoma; background-color:white'>&nbsp;" + Schedule.MaintenanceReq.Name + ":" + Schedule.MaintenanceReq.Revision + "</td>";

                        pnlScheduleTable.Controls.Add(new LiteralControl(sDetailsPanelLayout));
                        sDetailsPanelLayout = "";

                        DateTime TempStartDate2 = startDate;
                        for (int i = startWeek; i <= totalWeeks + startWeek; i++)
                        {
                            if (Schedule.MaintenanceReqType == "RecurringDateReq")
                            {
                                if (Schedule.DateOccurences != null)
                                {
                                    var value = Schedule.DateOccurences.FirstOrDefault(item => item.DueDateWeek == GetWeekNumber(TempStartDate2));
                                    var filteredOccurences = Schedule.DateOccurences.Where(item => item.DueDateWeek == GetWeekNumber(TempStartDate2));
                                    if (value != null)
                                    {
                                        SWC.Panel pnlScheduleDue = new SWC.Panel();
                                        pnlScheduleDue.CssClass = "ScheduleDue";
                                        pnlScheduleDue.Style["position"] = "relative";
                                        sDetailsPanelLayout += " <td class='ScheduleDue' style='background-color:red'><div style='position: relative;' class='ScheduleDuePosition'> ";
                                        pnlScheduleDue.Controls.Add(new LiteralControl(sDetailsPanelLayout));
                                        sDetailsPanelLayout = "";

                                        //---------details panel---------
                                        SWC.Panel pnlScheduleDetails = new SWC.Panel();
                                        pnlScheduleDetails.CssClass = "ScheduleDetails";
                                        pnlScheduleDetails.Style["position"] = "absolute";
                                        pnlScheduleDetails.Width = detailPanelWidth;
                                        //if (singleWidth > 30)
                                        //    pnlScheduleDetails.Style["left"] = singleWidth + "px";
                                        //else
                                        //    pnlScheduleDetails.Style["left"] = "30px";
                                        pnlScheduleDetails.Style["z-index"] = "999"; // set the z-index to a large value so that the popup will be ontop of other elements
                                        foreach (DateOccurences occurence in filteredOccurences)
                                        {
                                            string sHTML = "<span class='ScheduleDetails_FormatedDate'>#DATE#</span>";
                                            sHTML = sHTML.Replace("#DATE#", Convert.ToDateTime(occurence.DueDate.ToString()).ToString("dddd, dd MMMM yyyy HH:mm"));
                                            pnlScheduleDetails.Controls.Add(new LiteralControl(sHTML));
                                        }

                                        pnlScheduleDue.Controls.Add(pnlScheduleDetails);
                                        sDetailsPanelLayout = "</div></td> ";
                                        pnlScheduleDue.Controls.Add(new LiteralControl(sDetailsPanelLayout));
                                        sDetailsPanelLayout = "";
                                        pnlScheduleTable.Controls.Add(pnlScheduleDue);
                                    }
                                    else
                                    {
                                        sDetailsPanelLayout += " <td></td> ";
                                        pnlScheduleTable.Controls.Add(new LiteralControl(sDetailsPanelLayout));
                                        sDetailsPanelLayout = "";
                                    }
                                }
                                else
                                {
                                    sDetailsPanelLayout += " <td></td> ";
                                    pnlScheduleTable.Controls.Add(new LiteralControl(sDetailsPanelLayout));
                                    sDetailsPanelLayout = "";
                                }
                            }
                            else if (Schedule.MaintenanceReqType == "DateReq")
                            {
                                if (Schedule.ScheduleDateDay != null)
                                {
                                    if (Schedule.ScheduleDateWeek == GetWeekNumber(TempStartDate2) && Schedule.ScheduleDate.Value.Date >= startDate.Date && Schedule.ScheduleDate.Value.Date <= endDate.Date)
                                    {
                                        SWC.Panel pnlScheduleDue = new SWC.Panel();
                                        pnlScheduleDue.CssClass = "ScheduleDue";
                                        pnlScheduleDue.Style["position"] = "relative";
                                        sDetailsPanelLayout += " <td class='ScheduleDue' style='background-color:red'><div style='position: relative;' class='ScheduleDuePosition'> ";
                                        pnlScheduleDue.Controls.Add(new LiteralControl(sDetailsPanelLayout));
                                        sDetailsPanelLayout = "";

                                        //---------details panel---------
                                        SWC.Panel pnlScheduleDetails = new SWC.Panel();
                                        pnlScheduleDetails.CssClass = "ScheduleDetails";
                                        pnlScheduleDetails.Style["position"] = "absolute";
                                        pnlScheduleDetails.Width = detailPanelWidth;
                                        //if (singleWidth > 30)
                                        //    pnlScheduleDetails.Style["left"] = singleWidth + "px";
                                        //else
                                        //    pnlScheduleDetails.Style["left"] = "30px";
                                        pnlScheduleDetails.Style["z-index"] = "999"; // set the z-index to a large value so that the popup will be ontop of other elements
                                        string sHTML = "<span class='ScheduleDetails_FormatedDate'>#DATE#</span>";
                                        sHTML = sHTML.Replace("#DATE#", Convert.ToDateTime(Schedule.ScheduleDate.ToString()).ToString("dddd, dd MMMM yyyy HH:mm"));
                                        pnlScheduleDetails.Controls.Add(new LiteralControl(sHTML));

                                        pnlScheduleDue.Controls.Add(pnlScheduleDetails);
                                        sDetailsPanelLayout = "</div></td> ";
                                        pnlScheduleDue.Controls.Add(new LiteralControl(sDetailsPanelLayout));
                                        sDetailsPanelLayout = "";
                                        pnlScheduleTable.Controls.Add(pnlScheduleDue);
                                    }
                                    else
                                    {
                                        sDetailsPanelLayout += " <td></td> ";
                                        pnlScheduleTable.Controls.Add(new LiteralControl(sDetailsPanelLayout));
                                        sDetailsPanelLayout = "";
                                    }
                                }
                                else
                                {
                                    sDetailsPanelLayout += " <td></td> ";
                                    pnlScheduleTable.Controls.Add(new LiteralControl(sDetailsPanelLayout));
                                    sDetailsPanelLayout = "";
                                }
                            }
                            TempStartDate2 = TempStartDate2.AddDays(7);
                        }
                    }
                    else if (_ddlTimeScale.Data.ToString() == "3") //TimeScale is Month
                    {
                        int maxMonth = 0;
                        int countMonths = 0;

                        if (endDate.Year == startDate.Year && endDate.Month == startDate.Month)
                            maxMonth = endDate.Month + 1;
                        else if (endDate.Month == startDate.Month)
                            maxMonth = endDate.Month + 13;
                        else if (endDate.Month > startDate.Month)
                            maxMonth = endDate.Month + 1;
                        else
                            maxMonth = endDate.Month + 13;

                        countMonths = maxMonth - startDate.Month;
                        int singleWidth = maxWidth / countMonths;

                        DateTime TempStartDate = startDate;
                        for (int i = startDate.Month; i < maxMonth; i++)
                        {
                            sDetailsPanelLayout += "<td style='width:" + singleWidth + "px; border:1px solid'>" + TempStartDate.ToString("MMM").ToUpper() + "</td>";
                            TempStartDate = TempStartDate.AddMonths(1);
                        }
                        sDetailsPanelLayout += @"</tr>
                            <tr style='background-color:lightgreen;'>
                                <td style='border:0px solid; font-size:small; font-family:Tahoma; background-color:white'>&nbsp;" + Schedule.MaintenanceReq.Name + ":" + Schedule.MaintenanceReq.Revision + "</td>";

                        pnlScheduleTable.Controls.Add(new LiteralControl(sDetailsPanelLayout));
                        sDetailsPanelLayout = "";

                        for (int i = startDate.Month; i < maxMonth; i++)
                        {
                            if (Schedule.MaintenanceReqType == "RecurringDateReq")
                            {
                                if (Schedule.DateOccurences != null)
                                {
                                    DateOccurences emptyOccurences = new DateOccurences();
                                    var value = emptyOccurences;
                                    DateOccurences[] filteredOccurences = new DateOccurences[0];
                                    if (i > 12)
                                    {
                                        value = Schedule.DateOccurences.FirstOrDefault(item => item.DueDateMonth == i - 12 && item.DueDateYear == endDate.Year);
                                        filteredOccurences = Schedule.DateOccurences.Where(item => item.DueDateMonth == i - 12 && item.DueDateYear == endDate.Year).ToArray();
                                    }
                                    else
                                    {
                                        value = Schedule.DateOccurences.FirstOrDefault(item => item.DueDateMonth == i && item.DueDateYear == startDate.Year);
                                        filteredOccurences = Schedule.DateOccurences.Where(item => item.DueDateMonth == i && item.DueDateYear == startDate.Year).ToArray();
                                    }
                                    if (value != null)
                                    {
                                        SWC.Panel pnlScheduleDue = new SWC.Panel();
                                        pnlScheduleDue.CssClass = "ScheduleDue";
                                        pnlScheduleDue.Style["position"] = "relative";
                                        sDetailsPanelLayout += " <td class='ScheduleDue' style='background-color:red'><div style='position: relative;' class='ScheduleDuePosition'> ";
                                        pnlScheduleDue.Controls.Add(new LiteralControl(sDetailsPanelLayout));
                                        sDetailsPanelLayout = "";
                                        //---------details panel---------
                                        SWC.Panel pnlScheduleDetails = new SWC.Panel();
                                        pnlScheduleDetails.CssClass = "ScheduleDetails";
                                        pnlScheduleDetails.Style["position"] = "absolute";
                                        pnlScheduleDetails.Width = detailPanelWidth;
                                        //if (singleWidth > 30)
                                        //    pnlScheduleDetails.Style["left"] = singleWidth + "px";
                                        //else
                                        //    pnlScheduleDetails.Style["left"] = "30px";
                                        pnlScheduleDetails.Style["z-index"] = "999"; // set the z-index to a large value so that the popup will be ontop of other elements

                                        foreach (DateOccurences occurence in filteredOccurences)
                                        {
                                            string sHTML = "<span class='ScheduleDetails_FormatedDate'>#DATE#</span>";
                                            sHTML = sHTML.Replace("#DATE#", Convert.ToDateTime(occurence.DueDate.ToString()).ToString("dddd, dd MMMM yyyy HH:mm"));
                                            pnlScheduleDetails.Controls.Add(new LiteralControl(sHTML));
                                        }

                                        pnlScheduleDue.Controls.Add(pnlScheduleDetails);
                                        sDetailsPanelLayout = "</div></td> ";
                                        pnlScheduleDue.Controls.Add(new LiteralControl(sDetailsPanelLayout));
                                        sDetailsPanelLayout = "";
                                        pnlScheduleTable.Controls.Add(pnlScheduleDue);
                                    }
                                    else
                                    {
                                        sDetailsPanelLayout += " <td></td> ";
                                        pnlScheduleTable.Controls.Add(new LiteralControl(sDetailsPanelLayout));
                                        sDetailsPanelLayout = "";
                                    }
                                }
                                else
                                {
                                    sDetailsPanelLayout += " <td></td> ";
                                    pnlScheduleTable.Controls.Add(new LiteralControl(sDetailsPanelLayout));
                                    sDetailsPanelLayout = "";
                                }
                            }
                            else if (Schedule.MaintenanceReqType == "DateReq")
                            {
                                if (Schedule.ScheduleDateMonth != null && Schedule.ScheduleDate.Value.Date >= startDate.Date && Schedule.ScheduleDate.Value.Date <= endDate.Date)
                                {
                                    if (i > 12)
                                    {
                                        if (i - 12 == Schedule.ScheduleDateMonth)
                                        {
                                            SWC.Panel pnlScheduleDue = new SWC.Panel();
                                            pnlScheduleDue.CssClass = "ScheduleDue";
                                            pnlScheduleDue.Style["position"] = "relative";
                                            sDetailsPanelLayout += " <td class='ScheduleDue' style='background-color:red'><div style='position: relative;' class='ScheduleDuePosition'> ";
                                            pnlScheduleDue.Controls.Add(new LiteralControl(sDetailsPanelLayout));
                                            sDetailsPanelLayout = "";

                                            //---------details panel---------
                                            SWC.Panel pnlScheduleDetails = new SWC.Panel();
                                            pnlScheduleDetails.CssClass = "ScheduleDetails";
                                            pnlScheduleDetails.Style["position"] = "absolute";
                                            pnlScheduleDetails.Width = detailPanelWidth;
                                            //if (singleWidth > 30)
                                            //    pnlScheduleDetails.Style["left"] = singleWidth + "px";
                                            //else
                                            //    pnlScheduleDetails.Style["left"] = "30px";
                                            pnlScheduleDetails.Style["z-index"] = "999"; // set the z-index to a large value so that the popup will be ontop of other elements
                                            string sHTML = "<span class='ScheduleDetails_FormatedDate'>#DATE#</span>";
                                            sHTML = sHTML.Replace("#DATE#", Convert.ToDateTime(Schedule.ScheduleDate.ToString()).ToString("dddd, dd MMMM yyyy HH:mm"));
                                            pnlScheduleDetails.Controls.Add(new LiteralControl(sHTML));

                                            pnlScheduleDue.Controls.Add(pnlScheduleDetails);
                                            sDetailsPanelLayout = "</div></td> ";
                                            pnlScheduleDue.Controls.Add(new LiteralControl(sDetailsPanelLayout));
                                            sDetailsPanelLayout = "";
                                            pnlScheduleTable.Controls.Add(pnlScheduleDue);
                                        }
                                        else
                                        {
                                            sDetailsPanelLayout += " <td></td> ";
                                            pnlScheduleTable.Controls.Add(new LiteralControl(sDetailsPanelLayout));
                                            sDetailsPanelLayout = "";
                                        }
                                    }
                                    else
                                    {
                                        if (i == Schedule.ScheduleDateMonth)
                                        {
                                            SWC.Panel pnlScheduleDue = new SWC.Panel();
                                            pnlScheduleDue.CssClass = "ScheduleDue";
                                            pnlScheduleDue.Style["position"] = "relative";
                                            sDetailsPanelLayout += " <td class='ScheduleDue' style='background-color:red'><div style='position: relative;' class='ScheduleDuePosition'> ";
                                            pnlScheduleDue.Controls.Add(new LiteralControl(sDetailsPanelLayout));
                                            sDetailsPanelLayout = "";

                                            //---------details panel---------
                                            SWC.Panel pnlScheduleDetails = new SWC.Panel();
                                            pnlScheduleDetails.CssClass = "ScheduleDetails";
                                            pnlScheduleDetails.Style["position"] = "absolute";
                                            pnlScheduleDetails.Width = detailPanelWidth;
                                            //if (singleWidth > 30)
                                            //    pnlScheduleDetails.Style["left"] = singleWidth + "px";
                                            //else
                                            //    pnlScheduleDetails.Style["left"] = "30px";
                                            pnlScheduleDetails.Style["z-index"] = "999"; // set the z-index to a large value so that the popup will be ontop of other elements
                                            string sHTML = "<span class='ScheduleDetails_FormatedDate'>#DATE#</span>";
                                            sHTML = sHTML.Replace("#DATE#", Convert.ToDateTime(Schedule.ScheduleDate.ToString()).ToString("dddd, dd MMMM yyyy HH:mm"));
                                            pnlScheduleDetails.Controls.Add(new LiteralControl(sHTML));

                                            pnlScheduleDue.Controls.Add(pnlScheduleDetails);
                                            sDetailsPanelLayout = "</div></td> ";
                                            pnlScheduleDue.Controls.Add(new LiteralControl(sDetailsPanelLayout));
                                            sDetailsPanelLayout = "";
                                            pnlScheduleTable.Controls.Add(pnlScheduleDue);
                                        }
                                        else
                                        {
                                            sDetailsPanelLayout += " <td></td> ";
                                            pnlScheduleTable.Controls.Add(new LiteralControl(sDetailsPanelLayout));
                                            sDetailsPanelLayout = "";
                                        }
                                    }
                                }
                                else
                                {
                                    sDetailsPanelLayout += " <td></td> ";
                                    pnlScheduleTable.Controls.Add(new LiteralControl(sDetailsPanelLayout));
                                    sDetailsPanelLayout = "";
                                }
                            }
                        }
                    }
                    sDetailsPanelLayout += @"</tr>
                        </table>
                        </td>
                        </tr>
                    </table>
                    <br>";

                    pnlScheduleTable.Controls.Add(new LiteralControl(sDetailsPanelLayout));
                    sDetailsPanelLayout = "";
                }
                else if (Schedule.MaintenanceReqType == "ThruputReq")
                {
                    sDetailsPanelLayout += "" +
                    @"<table style='padding:0px 0px 0px 0px;  border:1px solid'>" +
                        @"<tr>" +
                            @"<td>" +
                                @"<table><tr><td style='border:0px solid; font-size:small; font-family:Tahoma; width:100px'> " + Schedule.Resource.Name + " </td></tr></table>" +
                            @"</td>" +
                            @"<td>
                        <table>
                            <tr style='font-size:small; font-family:Tahoma; background-color:white;'>
                                <td style='width:250px;border:1px solid'>&nbsp;PM / % processed</td>
                                <td style='width:50px; border:1px solid'>10%</td><td style='width:50px; border:1px solid'>20%</td><td style='width:50px; border:1px solid'>30%</td><td style='width:50px; border:1px solid'>40%</td>
                                <td style='width:50px; border:1px solid'>50%</td><td style='width:50px; border:1px solid'>60%</td><td style='width:50px; border:1px solid'>70%</td><td style='width:50px; border:1px solid'>80%</td>
                                <td style='width:50px; border:1px solid'>90%</td><td style='width:50px; border:1px solid'>100%</td>                                
                            </tr>
                            <tr style='background-color:yellow'> 
                               <td style='font-size:small; font-family:Tahoma; background-color:white'>&nbsp;" + Schedule.MaintenanceReq.Name + ":" + Schedule.MaintenanceReq.Revision + " ( " + Schedule.ThruputQtyPercent.Value.ToString() + "% ) </td>";

                    pnlScheduleTable.Controls.Add(new LiteralControl(sDetailsPanelLayout));
                    sDetailsPanelLayout = "";

                    for (double i = 10; i <= 100; i += 10)
                    {
                        if (Schedule.ThruputQtyPercent != null)
                        {
                            if (i <= Schedule.ThruputQtyPercent.Value)
                            {
                                sDetailsPanelLayout += " <td style='background-color:red'></td> ";
                            }
                            else
                            {
                                sDetailsPanelLayout += " <td></td> ";
                            }
                        }
                        else
                        {
                            sDetailsPanelLayout += " <td></td> ";
                        }
                    }
                    sDetailsPanelLayout += @"</tr>
                        </table>
                        </td>
                        </tr>
                    </table>
                    <br>";

                    pnlScheduleTable.Controls.Add(new LiteralControl(sDetailsPanelLayout));
                    sDetailsPanelLayout = "";
                }
                else if (Schedule.MaintenanceReqType == "ss_UsageReq")
                {
                    sDetailsPanelLayout += "" +
                    @"<table style='padding:0px 0px 0px 0px;  border:1px solid'>" +
                        @"<tr>" +
                            @"<td>" +
                                @"<table><tr><td style='border:0px solid; font-size:small; font-family:Tahoma; width:100px'> " + Schedule.Resource.Name + " </td></tr></table>" +
                            @"</td>" +
                            @"<td>
                        <table>
                            <tr style='font-size:small; font-family:Tahoma; background-color:white;'>
                                <td style='width:250px;border:1px solid'>&nbsp;PM / % processed</td>
                                <td style='width:50px; border:1px solid'>10%</td><td style='width:50px; border:1px solid'>20%</td><td style='width:50px; border:1px solid'>30%</td><td style='width:50px; border:1px solid'>40%</td>
                                <td style='width:50px; border:1px solid'>50%</td><td style='width:50px; border:1px solid'>60%</td><td style='width:50px; border:1px solid'>70%</td><td style='width:50px; border:1px solid'>80%</td>
                                <td style='width:50px; border:1px solid'>90%</td><td style='width:50px; border:1px solid'>100%</td>                                
                            </tr>
                            <tr style='background-color:yellow'> 
                               <td style='font-size:small; font-family:Tahoma; background-color:white'>&nbsp;" + Schedule.MaintenanceReq.Name + ":" + Schedule.MaintenanceReq.Revision + " ( " + Schedule.UsageCountPercent.Value.ToString() + "% ) </td>";

                    pnlScheduleTable.Controls.Add(new LiteralControl(sDetailsPanelLayout));
                    sDetailsPanelLayout = "";

                    for (double i = 10; i <= 100; i += 10)
                    {
                        if (Schedule.UsageCountPercent != null)
                        {
                            if (i <= Schedule.UsageCountPercent.Value)
                            {
                                sDetailsPanelLayout += " <td style='background-color:red'></td> ";
                            }
                            else
                            {
                                sDetailsPanelLayout += " <td></td> ";
                            }
                        }
                        else
                        {
                            sDetailsPanelLayout += " <td></td> ";
                        }
                    }
                    sDetailsPanelLayout += @"</tr>
                        </table>
                        </td>
                        </tr>
                    </table>
                    <br>";

                    pnlScheduleTable.Controls.Add(new LiteralControl(sDetailsPanelLayout));
                    sDetailsPanelLayout = "";
                }
                pnlDetails.Controls.Add(pnlScheduleTable);
            }

            pnlDrawAreaFrame.Controls.Add(pnlDetails);

            // add resourceIcon and drawArea to the canvas
            _pnlCanvasPanel.Controls.Add(pnlDrawAreaFrame);

            CamstarWebControl.SetRenderToClient(_pnlCanvasPanel);
        }

        //----------------------------
        // Update Pagination function
        //----------------------------
        public void UpdatePagination()
        {
            int curPage = Convert.ToInt32(_txtCurrentPage.Data.ToString());
            int totPage = Convert.ToInt32(_txtTotalPage.Data.ToString());
            
            string pageTxt = curPage + "/" + totPage;
            _lblPageNo.Text = pageTxt;
            _lblPageNo.LabelText = pageTxt;

            if (curPage > 1)
                _btnPrevious.Enabled = true;
            else
                _btnPrevious.Enabled = false;

            if (curPage < totPage)
                _btnNext.Enabled = true;
            else
                _btnNext.Enabled = false;
        }

        //----------------------------
        // Web Part Custom Action
        //----------------------------
        public override void WebPartCustomAction(object sender, Personalization.CustomActionEventArgs e)
        {
            base.WebPartCustomAction(sender, e);
            var action = e.Action as CustomAction;
            if (action != null)
            {
                int curPage = Convert.ToInt32(_txtCurrentPage.Data.ToString());
                ResultStatus oResultStatus = new ResultStatus();
                if (action.Parameters == "FetchScheduleOverview")
                {
                    _txtCurrentPage.Data = 1;
                    oResultStatus = FetchScheduleOverview();
                    if (oResultStatus.IsSuccess)
                    {
                        FetchScheduleOverview();
                        UpdatePagination();
                    }
                }
                else if (action.Parameters == "PreviousPage")
                {
                    int newCurPage = curPage - 1;
                    _txtCurrentPage.Data = newCurPage;
                    oResultStatus = FetchScheduleOverview();
                    if (oResultStatus.IsSuccess)
                    {
                        FetchScheduleOverview();
                        UpdatePagination();
                    }
                }
                else if (action.Parameters == "NextPage")
                {
                    int newCurPage = curPage + 1;
                    _txtCurrentPage.Data = newCurPage;
                    oResultStatus = FetchScheduleOverview();
                    if (oResultStatus.IsSuccess)
                    {
                        FetchScheduleOverview();
                        UpdatePagination();
                    }
                }
                e.Result = oResultStatus;
            }
        }

        //-----------------------------------------
        // Get Week Number from a DateTime variable
        //-----------------------------------------
        public static int GetWeekNumber(DateTime dtPassed)
        {
            System.Globalization.CultureInfo ciCurr = System.Globalization.CultureInfo.CurrentCulture;

            DayOfWeek day = ciCurr.Calendar.GetDayOfWeek(dtPassed);
            if (day >= DayOfWeek.Monday && day <= DayOfWeek.Wednesday)
            {
                dtPassed = dtPassed.AddDays(3);
            }

            int weekNum = ciCurr.Calendar.GetWeekOfYear(dtPassed, System.Globalization.CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
            return weekNum;
        }
    }
}



