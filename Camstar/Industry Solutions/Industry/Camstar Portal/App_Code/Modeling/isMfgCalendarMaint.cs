// © 2023 Siemens Product Lifecycle Management Software Inc.
using Camstar.WCF.ObjectStack;
using Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Camstar.WebPortal.WebPortlets.Modeling
{
    public class isMfgCalendarMaint : MatrixWebPart
    {
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            //add atribute, which allows to perform file saving via js
            AddShiftButton.Attributes.Add("actionType", "SubmitAction");
            AddShiftButton.Click += AddShiftButton_Click;
            AddShiftButton.Enabled = false;
            RadioBtn_Overwrite.DataChanged += new EventHandler(RadioBtn_Overwrite_DataChanged);
            RadioBtn_Update.DataChanged += new EventHandler(RadioBtn_Update_DataChanged);
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            var originalShifts = GetOriginalCalendarShifts();
            // If calendar shifts exist in grid and system then show Overwrite and Update radio buttons & Add shift button disabled
            if (originalShifts.Length > 0 && !IsPostExecute)
            {
                if (!(RadioBtn_Overwrite.RadioControl.Checked || RadioBtn_Update.RadioControl.Checked))
                {
                    RadioBtn_Overwrite.Visible = true;
                    RadioBtn_Update.Visible = true;
                    AddShiftButton.Enabled = false;
                }
                else if (RadioBtn_Overwrite.RadioControl.Checked || RadioBtn_Update.RadioControl.Checked)
                {
                    AddShiftButton.Enabled = !string.IsNullOrEmpty(UploadField.Data?.ToString());
                }
            }
            // If no calendar shifts in grid OR if new calendar instance then show only the Add Shift button
            else
            {
                if (!IsPostExecute || GetCalendarShiftsInGrid().Length == 0)
                {
                    RadioBtn_Overwrite.Visible = false;
                    RadioBtn_Update.Visible = false;
                    AddShiftButton.Enabled = !string.IsNullOrEmpty(UploadField.Data?.ToString());
                }
                else
                    IsPostExecute = false;
            }
        }

        protected virtual void AddShiftButton_Click(object sender, EventArgs e)
        {
            var filePath = FileInput.UploadFilePath; //new FormsFramework.CallStack().Context.LocalSession["uploadedFile"] as string;
            if (string.IsNullOrEmpty(filePath) || !System.IO.File.Exists(filePath))
            {
                AddShiftButton.Enabled = false;
                RadioBtn_Overwrite.RadioControl.Checked = false;
                RadioBtn_Update.RadioControl.Checked = false;
                return;
            }
            SpreadsheetDocument doc = null;
            var parsedShifts = new CalendarShiftChanges[0];
            try
            {
                if (filePath.ToLower().EndsWith(".csv"))
                {
                    parsedShifts = ParseCSVFile(filePath);                   
                }
                else
                {
                    doc = SpreadsheetDocument.Open(filePath, false);
                    Sheet sheet = doc.WorkbookPart.Workbook.Descendants<Sheet>().FirstOrDefault(s => s.Name == "Calendar");
                    if (sheet == null)
                    {
                        sheet = doc.WorkbookPart.Workbook.Descendants<Sheet>().FirstOrDefault(s => s.Name == "RecordSet");
                        if (sheet == null)
                        {
                            if (doc.WorkbookPart.Workbook.Sheets.Count() == 1)
                            {
                                sheet = doc.WorkbookPart.Workbook.Descendants<Sheet>().FirstOrDefault();
                            }
                            else
                            {
                                Page.DisplayMessage(ExcellErrorLabel.Text, false);
                                return;
                            }
                        }
                    }
                    var part = doc.WorkbookPart.GetPartById(sheet.Id) as WorksheetPart;
                    var columns = part.Worksheet.Descendants<Column>();
                    var stringTable = doc.WorkbookPart.GetPartsOfType<SharedStringTablePart>().FirstOrDefault();
                    var rows = part.Worksheet.Descendants<DocumentFormat.OpenXml.Spreadsheet.Row>();
                    var rowsCount = rows.Count();
                    if (rowsCount < 2)
                        return;
                    

                    //If user selects Overwrite radio button or if the calendar shifts grid is empty
                    if (RadioBtn_Overwrite.RadioControl.Checked || GetCalendarShiftsInGrid().Length == 0 || (RadioBtn_Overwrite.Visible == false && RadioBtn_Update.Visible == false))
                    {
                        parsedShifts = ParseImportedShiftsForOverwrite(rowsCount, rows, stringTable);
                    }
                    //If user selects Update radio button
                    else if (RadioBtn_Update.RadioControl.Checked)
                    {
                        parsedShifts = ParseImportedShiftsForUpdate(rowsCount, rows, stringTable);
                    }
                }
                CalendarShifts.Data = parsedShifts;
            }
            catch (Exception ex)
            {
                Page.DisplayMessage(ExcellErrorLabel.Text, false);
            }
            finally
            {
                if (doc != null)
                    doc.Close();
                UploadField.ClearData();
                FileInput.DeleteUniqueFolder();
                AddShiftButton.Enabled = false;
                RadioBtn_Overwrite.RadioControl.Checked = false;
                RadioBtn_Update.RadioControl.Checked = false;
                RadioBtn_Overwrite.Visible = false;
            }
        }

        public override void PostExecute(ResultStatus status, Service serviceData)
        {
            base.PostExecute(status, serviceData);
            IsPostExecute = true;
            if (status.IsSuccess)
            {
                FileInput.DeleteUniqueFolder();
            }
            // After Save, if Calendar Shifts exist then show radio buttons & Add shift button disabled.
            if (GetCalendarShiftsInGrid().Length > 0)
            {
                RadioBtn_Overwrite.Visible = true;
                RadioBtn_Update.Visible = true;
                AddShiftButton.Enabled = false;
            }
            // After Save, if Calendar Shifts is empty then hide radio buttons & Add shift button enabled.
            else
            {
                RadioBtn_Overwrite.Visible = false;
                RadioBtn_Update.Visible = false;
                AddShiftButton.Enabled = true;
            }
        }

        private IEnumerable<Cell> GetResultCells(IEnumerable<Cell> cells)
        {
            var resultCells = new List<Cell>();
            var previousIndex = 64;
            int index = 0;
            foreach (var cell in cells)
            {
                if (cell.CellReference == null)
                {
                    resultCells.Add(cell);
                    index++;
                }
                else
                {
                    var cellIndex = Convert.ToInt32(cell.CellReference.ToString().ElementAt(0));
                    if ((cellIndex - previousIndex) == 1)
                    {
                        resultCells.Add(cell);
                        previousIndex = cellIndex;
                    }
                    else
                    {
                        for (var i = cellIndex - previousIndex; i > 1; i--)
                        {
                            resultCells.Add(new Cell()
                            {
                                CellValue = new CellValue("")
                            });
                        }
                        resultCells.Add(cell);
                        previousIndex = cellIndex;
                    }
                }
            }
            return resultCells;
        }

        const int COL_COUNT_EXPORT = 11;
        const int COL_COUNT_NORMAL = 13;

        protected virtual CalendarShiftChanges[] ParseImportedShiftsForOverwrite(int rowsCount, IEnumerable<DocumentFormat.OpenXml.Spreadsheet.Row> rows, SharedStringTablePart stringTable)
        {
            var items = new List<CalendarShiftChanges>();
            int? fiscalYear, fiscalQuarter, fiscalMonth, fiscalWeek;
            double isNonScheduledTime = 0;

            var headers = rows.ElementAt(0).Elements<Cell>();
            int headerCount = headers.Count();
            if (headerCount == COL_COUNT_EXPORT || headers.Count() >= COL_COUNT_NORMAL)
            {                
                for (int i = 1; i < rowsCount; i++)
                {
                    var cells = rows.ElementAt(i).Elements<Cell>();
                    var resultCells = GetResultCells(cells);
                    int cellCount = resultCells.Count();

                    if (cellCount < 7 || (resultCells.ElementAt(1).CellValue == null && resultCells.ElementAt(3).CellValue == null && resultCells.ElementAt(4).CellValue == null))//7 cells are required only
                    {
                        if (i == 1) //Only display error if the 1st row is malformed.
                        {
                            Page.DisplayMessage(ExcellErrorLabel.Text, false);
                        }
                        break;
                    }
                    DateTime calendarDate = GetCelDateTime(resultCells.ElementAt(1));
                    DateTime shiftStart = GetCelDateTime(resultCells.ElementAt(3));
                    DateTime shiftEnd = GetCelDateTime(resultCells.ElementAt(4));
                    if (calendarDate == DateTime.MinValue ||shiftStart == DateTime.MinValue || shiftEnd == DateTime.MinValue)
                        continue;
                    //if (!double.TryParse(resultCells.ElementAt(3).CellValue.Text, out shiftStart))//Shift Start
                    //    continue;
                    //if (!double.TryParse(resultCells.ElementAt(4).CellValue.Text, out shiftEnd))//Shift End
                    //    continue;
                    //Not required fields                    
                    int nonScheduledCell = headerCount == COL_COUNT_EXPORT ? 10 : headerCount == COL_COUNT_NORMAL+1 ? COL_COUNT_NORMAL : -1;
                    string nonScheduledTime = string.Empty;
                    try
                    {
                        nonScheduledTime = nonScheduledCell < 0 || cellCount < nonScheduledCell || resultCells.Count() <= nonScheduledCell || resultCells.ElementAt(nonScheduledCell).CellValue == null ? "" : resultCells.ElementAt(nonScheduledCell).CellValue.Text;
                    }
                    catch (Exception iex)
                    {
                        string msg = iex.ToString();
                    }
                    if (!string.IsNullOrEmpty(nonScheduledTime))
                        if (!double.TryParse(nonScheduledTime, out isNonScheduledTime))//isNonScheduledTime
                            continue;
                    int teamInt = (resultCells.ElementAt(5).DataType != null && resultCells.ElementAt(5).DataType == CellValues.SharedString) ? Convert.ToInt32(resultCells.ElementAt(5).CellValue.Text) : -1;
                    int shiftInt = (resultCells.ElementAt(2).DataType != null && resultCells.ElementAt(2).DataType == CellValues.SharedString) ? Convert.ToInt32(resultCells.ElementAt(2).CellValue.Text) : -1;
                    string team = GetCellStringValue(resultCells.ElementAt(5));
                    string shift = GetCellStringValue(resultCells.ElementAt(2));
                    fiscalYear = intParse(resultCells.ElementAt(6).CellValue);//FiscalYear
                    fiscalQuarter = intParse(resultCells.ElementAt(7).CellValue);//FiscalQuarter
                    fiscalMonth = intParse(resultCells.ElementAt(8).CellValue);//FiscalMonth
                    fiscalWeek = intParse(resultCells.ElementAt(9).CellValue);//FiscalWeek

                    AddNewShifts(ref items, calendarDate, resultCells, stringTable, shiftStart, shiftEnd, team, fiscalYear, fiscalQuarter, fiscalMonth, fiscalWeek, shift, teamInt, shiftInt);

                    //only add isNonScheduledTime if it is not null so that the field would be truly null instead of 0
                    if (!string.IsNullOrEmpty(nonScheduledTime))
                        items[items.Count - 1].isNonScheduledTime = isNonScheduledTime;
                }
            } else
                Page.DisplayMessage(ExcellErrorLabel.Text, false);
            return items.ToArray();
        }

        protected DateTime GetCelDateTime(Cell cell)
        {
            DateTime value = DateTime.MinValue;
            try
            {
                if (cell.DataType != null && cell.DataType == CellValues.String)
                    value = DateTime.Parse(cell.CellValue.Text);
                else
                    value = DateTime.FromOADate(double.Parse(cell.CellValue.Text));
            }
            catch (Exception ex) 
            {
                string msg = ex.ToString();
            }
            return value;
        }

        protected string GetCellStringValue(Cell cell)
        {
            string value = string.Empty;
            try
            {
                if (cell.CellValue != null && (cell.DataType == null || cell.DataType != CellValues.SharedString))
                    value = String.Copy(cell.CellValue.Text);
            }
            catch(Exception ex)
            {
                string err = ex.Message;
            }
            return value;
        }

        protected virtual CalendarShiftChanges[] ParseImportedShiftsForUpdate(int rowsCount, IEnumerable<DocumentFormat.OpenXml.Spreadsheet.Row> rows, SharedStringTablePart stringTable)
        {
            var items = new List<CalendarShiftChanges>();
            var presentCalendarShiftsInGrid = GetCalendarShiftsInGrid();
            int? fiscalYear, fiscalQuarter, fiscalMonth, fiscalWeek;
            double isNonScheduledTime = 0;

            var headers = rows.ElementAt(0).Elements<Cell>();
            int headerCount = headers.Count();
            if (headerCount == COL_COUNT_EXPORT || headers.Count() >= COL_COUNT_NORMAL)
            {
                for (int i = 1; i < rowsCount; i++)
                {
                    var cells = rows.ElementAt(i).Elements<Cell>();
                    var resultCells = GetResultCells(cells);
                    int cellCount = resultCells.Count();

                    if (cellCount < 7)//7 cells are required only
                    {
                        if (i == 1) //Only display error if the 1st row is malformed.
                        {
                            Page.DisplayMessage(ExcellErrorLabel.Text, false);
                        }
                        break;
                    }

                    string calendarShiftId = (resultCells.ElementAt(0).DataType != null && resultCells.ElementAt(0).DataType == CellValues.SharedString) ?
                        stringTable.SharedStringTable.ElementAt(Convert.ToInt32(resultCells.ElementAt(0).CellValue.Text)).InnerText.Trim() : "";//CalendarShiftId
                    DateTime calendarDate = GetCelDateTime(resultCells.ElementAt(1));
                    DateTime shiftStart = GetCelDateTime(resultCells.ElementAt(3));
                    DateTime shiftEnd = GetCelDateTime(resultCells.ElementAt(4));
                    if (calendarDate == DateTime.MinValue || shiftStart == DateTime.MinValue || shiftEnd == DateTime.MinValue)
                        continue;

                    /*if (!double.TryParse(resultCells.ElementAt(1).CellValue.Text, out calendarDate))//Calendar Date
                        continue;
                    if (!double.TryParse(resultCells.ElementAt(3).CellValue.Text, out shiftStart))//Shift Start
                        continue;
                    if (!double.TryParse(resultCells.ElementAt(4).CellValue.Text, out shiftEnd))//Shift End
                        continue;
                    */
                    //Not required fields
                    int nonScheduledCell = headerCount == COL_COUNT_EXPORT ? 10 : headerCount == COL_COUNT_NORMAL ? 13 : 14;
                    string nonScheduledTime = nonScheduledCell < 0 || cellCount < nonScheduledCell || resultCells.ElementAt(nonScheduledCell).CellValue == null ? "" : resultCells.ElementAt(nonScheduledCell).CellValue.Text;
                    if (!string.IsNullOrEmpty(nonScheduledTime))
                        if (!double.TryParse(nonScheduledTime, out isNonScheduledTime))//isNonScheduledTime
                            continue;
                    int teamInt = (resultCells.ElementAt(5).DataType != null && resultCells.ElementAt(5).DataType == CellValues.SharedString) ? Convert.ToInt32(resultCells.ElementAt(5).CellValue.Text) : -1;
                    int shiftInt = (resultCells.ElementAt(2).DataType != null && resultCells.ElementAt(2).DataType == CellValues.SharedString) ? Convert.ToInt32(resultCells.ElementAt(2).CellValue.Text) : -1;
                    string team = GetCellStringValue(resultCells.ElementAt(5));
                    string shift = GetCellStringValue(resultCells.ElementAt(2));
                    fiscalYear = intParse(resultCells.ElementAt(6).CellValue);//FiscalYear
                    fiscalQuarter = intParse(resultCells.ElementAt(7).CellValue);//FiscalQuarter
                    fiscalMonth = intParse(resultCells.ElementAt(8).CellValue);//FiscalMonth
                    fiscalWeek = intParse(resultCells.ElementAt(9).CellValue);//FiscalWeek

                    //Update existing calendar shifts in grid w.r.t. calendar shift Id imported via excel sheet
                    if (!string.IsNullOrEmpty(calendarShiftId))
                    {
                        presentCalendarShiftsInGrid.Where(csc => csc.ObjectToChange != null && csc.ObjectToChange.ID == calendarShiftId)
                            .All(c =>
                            {
                                c.CalendarDate = calendarDate;
                                c.Shift = shiftInt >= 0 ? new NamedObjectRef(stringTable.SharedStringTable.ElementAt(shiftInt).InnerText) : new NamedObjectRef(shift);
                                c.ShiftStart = shiftStart;
                                c.ShiftEnd = shiftEnd;
                                //Not required fields
                                c.Team = teamInt >= 0 ? new NamedObjectRef(stringTable.SharedStringTable.ElementAt(teamInt).InnerText) : new NamedObjectRef(team);
                                c.FiscalYear = fiscalYear;
                                c.FiscalQuarter = fiscalQuarter;
                                c.FiscalMonth = fiscalMonth;
                                c.FiscalWeek = fiscalWeek;
                                //only add isNonScheduledTime if it is not null so that the field would be truly null instead of 0
                                if (!string.IsNullOrEmpty(nonScheduledTime))
                                    c.isNonScheduledTime = isNonScheduledTime;
                                return true;
                            });
                    }
                    //Append calendar shifts(imported via excel sheet) in grid with empty calendar shift Id
                    else
                    {
                        AddNewShifts(ref items, calendarDate, resultCells, stringTable, shiftStart, shiftEnd, team, fiscalYear, fiscalQuarter, fiscalMonth, fiscalWeek, shift, teamInt, shiftInt);

                        //only add isNonScheduledTime if it is not null so that the field would be truly null instead of 0
                        if (!string.IsNullOrEmpty(nonScheduledTime))
                            items[items.Count - 1].isNonScheduledTime = isNonScheduledTime;
                    }
                }
            } else
                Page.DisplayMessage(ExcellErrorLabel.Text, false);
            return presentCalendarShiftsInGrid.Concat(items).ToArray();
        }
        
        protected virtual void AddNewShifts(ref List<CalendarShiftChanges> items, DateTime calendarDate, IEnumerable<Cell> resultCells,
            SharedStringTablePart stringTable, DateTime shiftStart, DateTime shiftEnd, string team, int? fiscalYear, int? fiscalQuarter, int? fiscalMonth, int? fiscalWeek, string shift, int teamInt, int shiftInt)
        {
            items.Add(new CalendarShiftChanges
            {
                CalendarDate = calendarDate,
                Shift = shiftInt >= 0 ? new NamedObjectRef(stringTable.SharedStringTable.ElementAt(shiftInt).InnerText) : new NamedObjectRef(shift),
                ShiftStart = shiftStart,
                ShiftEnd = shiftEnd,
                //Not required fields
                Team = teamInt >= 0 ? new NamedObjectRef(stringTable.SharedStringTable.ElementAt(teamInt).InnerText) :new NamedObjectRef(team),
                FiscalYear = fiscalYear,
                FiscalQuarter = fiscalQuarter,
                FiscalMonth = fiscalMonth,
                FiscalWeek = fiscalWeek
            });
        }

        protected virtual void RadioBtn_Overwrite_DataChanged(object sender, EventArgs e)
        {
            if (RadioBtn_Overwrite.RadioControl.Checked)
            {
                RadioBtn_Update.RadioControl.Checked = false;
                AddShiftButton.Enabled = !string.IsNullOrEmpty(UploadField.Data?.ToString());
            }
        }

        protected virtual void RadioBtn_Update_DataChanged(object sender, EventArgs e)
        {
            if (RadioBtn_Update.RadioControl.Checked)
            {
                RadioBtn_Overwrite.RadioControl.Checked = false;
                AddShiftButton.Enabled = !string.IsNullOrEmpty(UploadField.Data?.ToString());
            }
        }

        protected virtual CalendarShiftChanges[] GetCalendarShiftsInGrid()
        {
            return CalendarShifts.Data != null ? CalendarShifts.Data as CalendarShiftChanges[] : new CalendarShiftChanges[0];
        }

        protected virtual CalendarShiftChanges[] GetOriginalCalendarShifts()
        {
            return CalendarShifts.OriginalData != null ? CalendarShifts.OriginalData as CalendarShiftChanges[] : new CalendarShiftChanges[0];
        }

        protected virtual int? intParse(CellValue cellValue)
        {
            int? value = null;
            try
            {
                if (cellValue != null && !string.IsNullOrEmpty(cellValue.Text))
                {
                    value = int.Parse(cellValue.Text);
                }
            }
            catch { }
            return value;
        }

        protected virtual int GetInt(CellValue cellValue)
        {
            int value = 0;
            try
            {
                if (cellValue != null && !string.IsNullOrEmpty(cellValue.Text))
                {
                    value = int.Parse(cellValue.Text);
                }
            }
            catch { }
            return value;
        }

        #region Properties
        protected virtual Button AddShiftButton
        {
            get { return Page.FindCamstarControl("AddShiftButton") as Button; }
        }
        protected virtual FormsFramework.WebControls.Label ExcellErrorLabel
        {
            get { return Page.FindCamstarControl("ExcellErrorLabel") as FormsFramework.WebControls.Label; }
        }
        protected virtual JQDataGrid CalendarShifts
        {
            get { return Page.FindCamstarControl("CalendarShifts") as JQDataGrid; }
        }
        protected virtual FileInput UploadField
        {
            get { return Page.FindCamstarControl("FileInput") as FileInput; }
        } // UploadField 
        protected virtual RadioButton RadioBtn_Overwrite
        {
            get { return Page.FindCamstarControl("RadioBtn_MfgCalendar_Overwrite") as RadioButton; }
        } // RadioBtn_Overwrite

        protected virtual RadioButton RadioBtn_Update
        {
            get { return Page.FindCamstarControl("RadioBtn_MfgCalendar_Update") as RadioButton; }
        } // RadioBtn_Update
        #endregion

        #region Private Members
        private bool IsPostExecute { get; set; }
        #endregion

        #region CSV File Support
        const int COL_SHIFT_ID = 0;
        const int COL_DATE = 1;
        const int COL_SHIFT_NAME = 2;
        const int COL_SHIFT_START = 3;
        const int COL_SHIFT_END = 4;
        const int COL_TEAM_NAME = 5;
        const int COL_FY_YEAR = 6;
        const int COL_FY_QTR = 7;
        const int COL_FY_MONTH = 8;
        const int COL_FY_WEEK = 9;

        protected CalendarShiftChanges[] ParseCSVFile(string filePath)
        {
            var items = new List<CalendarShiftChanges>();
            try
            {
                CalendarShiftChanges[] presentCalendarShiftsInGrid = RadioBtn_Update.RadioControl.Checked ? GetCalendarShiftsInGrid() : null;
                int row = 0;
                string[] lines = System.IO.File.ReadAllLines(filePath);
                foreach (string line in lines)
                {
                    string[] data = line.Split(',');
                    if (row > 0 && data.Length > COL_COUNT_NORMAL)
                    {
                        int nonScheduledCell = data.Length == COL_COUNT_EXPORT ? 10 : data.Length == COL_COUNT_NORMAL + 1 ? COL_COUNT_NORMAL : -1;

                        string shiftID = data[COL_SHIFT_ID].Trim('\"');
                        DateTime calendarDate = GetDateTime(data[COL_DATE].Trim('\"'));
                        string shiftName = data[COL_SHIFT_NAME].Trim('\"');
                        DateTime shiftStart = GetDateTime(data[COL_SHIFT_START].Trim('\"'));
                        DateTime shiftEnd = GetDateTime(data[COL_SHIFT_END].Trim('\"'));
                        string teamName = data[COL_TEAM_NAME].Trim('\"');
                        int fyYear = GetInt(data[COL_FY_YEAR].Trim('\"'));
                        int fyQtr = GetInt(data[COL_FY_QTR].Trim('\"'));
                        int fyMonth = GetInt(data[COL_FY_MONTH].Trim('\"'));
                        int fyWeek = GetInt(data[COL_FY_WEEK].Trim('\"'));
                        double nonScheduledTIme = nonScheduledCell > 0 ? GetDouble(data[nonScheduledCell].Trim('\"')) : 0.0;

                        if (presentCalendarShiftsInGrid != null && !string.IsNullOrEmpty(shiftID))
                        {
                            presentCalendarShiftsInGrid.Where(csc => csc.ObjectToChange != null && csc.ObjectToChange.ID == shiftID)
                            .All(c =>
                            {
                                c.CalendarDate = calendarDate;
                                c.Shift = new NamedObjectRef(shiftName);
                                c.ShiftStart = shiftStart;
                                c.ShiftEnd = shiftEnd;
                                //Not required fields
                                c.Team = new NamedObjectRef(teamName);
                                c.FiscalYear = fyYear;
                                c.FiscalQuarter = fyQtr;
                                c.FiscalMonth = fyMonth;
                                c.FiscalWeek = fyMonth;
                                //only add isNonScheduledTime if it is not null so that the field would be truly null instead of 0
                                if (nonScheduledTIme > 0)
                                    c.isNonScheduledTime = nonScheduledTIme;
                                return true;
                            });

                        }
                        else
                        {
                            CalendarShiftChanges shift = new CalendarShiftChanges();
                            shift.CalendarDate = calendarDate;
                            shift.FiscalMonth = fyMonth;
                            shift.FiscalQuarter = fyQtr;
                            shift.FiscalWeek = fyWeek;
                            shift.FiscalYear = fyYear;
                            if (nonScheduledTIme > 0)
                                shift.isNonScheduledTime = nonScheduledTIme;
                            shift.Shift = new NamedObjectRef(shiftName);
                            shift.ShiftEnd = shiftEnd;
                            shift.ShiftStart = shiftStart;
                            shift.Team = new NamedObjectRef(teamName);

                            items.Add(shift);
                        }
                    }
                    row++;
                }
                if (presentCalendarShiftsInGrid != null)
                    return presentCalendarShiftsInGrid.Concat(items.ToArray()).ToArray();
            }
            catch (Exception ex)
            {
                string msg = ex.ToString();
                Page.DisplayMessage(ExcellErrorLabel.Text, false);
            }
            return items.ToArray();
        }

        protected double GetDouble(object doubleObject)
        {
            double value = 0;// double.MinValue;
            if (doubleObject != null)
            {
                Type type = doubleObject.GetType();
                if (type.Equals(typeof(string)))
                {
                    string val = doubleObject.ToString();
                    if (!string.IsNullOrEmpty(val))
                    {
                        try
                        {
                            if (val.Contains("+308"))
                                value = double.MaxValue;
                            else if (val.Contains("-308"))
                                value = double.MinValue;
                            else if (val != double.NaN.ToString() && val != double.NegativeInfinity.ToString() && val != double.PositiveInfinity.ToString())
                                value = Convert.ToDouble(val, System.Globalization.CultureInfo.InvariantCulture);
                        }
                        catch {  /*  Ignore exceptions as purpose is to provide method to safely return value  without exception  */  }
                    }
                }
                else if (!type.Equals(typeof(DBNull)))
                    value = Convert.ToDouble(doubleObject, System.Globalization.CultureInfo.InvariantCulture);
            }
            return value;
        }
        protected int GetInt(object intObject)
        {
            int retVal = 0;// int.MinValue;
            if (intObject != null)
            {
                Type type = intObject.GetType();
                // We do this if/else statement, because the type may be "System.DBNull".
                if (type == typeof(System.Int32) || type == typeof(int))
                {
                    retVal = (int)intObject;
                }
                else if (type == typeof(System.Int16) || type == typeof(System.Byte) || type == typeof(System.SByte))
                {
                    retVal = Convert.ToInt32(intObject);
                }
                else if (type == typeof(System.Int64) || type == typeof(long))
                {
                    retVal = Convert.ToInt32(intObject);
                }
                else if (type == typeof(System.Decimal))
                {
                    retVal = System.Decimal.ToInt32((System.Decimal)intObject);
                }
                else if (type == typeof(Double) || type == typeof(float))
                {
                    retVal = Convert.ToInt32(intObject);
                }
                else if (type == typeof(System.String))
                {
                    retVal = intObject.ToString().Length > 0 ? Convert.ToInt32(intObject.ToString()) : 0;
                }
                else if (!type.Equals(typeof(DBNull)))
                    retVal = Convert.ToInt32(intObject);
            }
            return retVal;
        }
        protected DateTime GetDateTime(object dateTimeObject)
        {
            DateTime retVal = new DateTime(0, DateTimeKind.Utc);
            if (dateTimeObject != null)
            {
                Type type = dateTimeObject.GetType();
                if (type.Equals(typeof(DateTime)))
                    retVal = (DateTime)dateTimeObject;
                else if (type.Equals(typeof(string)) && !string.IsNullOrEmpty(dateTimeObject.ToString()))
                {
                    try
                    {
                        retVal = DateTime.Parse(dateTimeObject.ToString(), System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.RoundtripKind);// DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);
                    }
                    catch //(System.Exception ex) 
                    {
                        retVal = DateTime.Parse(dateTimeObject.ToString(), System.Threading.Thread.CurrentThread.CurrentCulture, System.Globalization.DateTimeStyles.RoundtripKind);
                    }
                }
                else if (type.Equals(typeof(long)) || type.Equals(typeof(Int64)))
                    retVal = new DateTime((long)dateTimeObject);
            }
            return retVal;
        }
        #endregion CSV File Support

    }
}