// Copyright Siemens 2023  
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Camstar.WCF.ObjectStack;
using Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml;

namespace Camstar.WebPortal.WebPortlets.Modeling
{
    public class MfgCalendarMaint : MatrixWebPart
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
            if (originalShifts.Length > 0)
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
                RadioBtn_Overwrite.Visible = false;
                RadioBtn_Update.Visible = false;
                AddShiftButton.Enabled = !string.IsNullOrEmpty(UploadField.Data?.ToString());
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
            try
            {
                doc = SpreadsheetDocument.Open(filePath, false);
                Sheet sheet = doc.WorkbookPart.Workbook.Descendants<Sheet>().FirstOrDefault(s => s.Name == "Calendar");
                if (sheet == null)
                {
                    sheet = doc.WorkbookPart.Workbook.Descendants<Sheet>().FirstOrDefault(s => s.Name == "RecordSet");
                    if (sheet == null)
                    {
                        Page.DisplayMessage(ExcellErrorLabel.Text, false);
                        return;
                    }
                }
                var part = doc.WorkbookPart.GetPartById(sheet.Id) as WorksheetPart;
                var columns = part.Worksheet.Descendants<Column>();
                var stringTable = doc.WorkbookPart.GetPartsOfType<SharedStringTablePart>().FirstOrDefault();
                var rows = part.Worksheet.Descendants<DocumentFormat.OpenXml.Spreadsheet.Row>();
                var rowsCount = rows.Count();
                if (rowsCount < 2)
                    return;
                var parsedShifts = new CalendarShiftChanges[0];

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
                
                CalendarShifts.Data = parsedShifts;
            }
            catch (Exception)
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

        protected DateTime GetCellDateTime(Cell cell)
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
            catch (Exception ex)
            {
                string err = ex.Message;
            }
            return value;
        }

        const int COL_COUNT_EXPORT = 11;
        const int COL_COUNT_NORMAL = 12;

        protected virtual CalendarShiftChanges[] ParseImportedShiftsForOverwrite(int rowsCount, IEnumerable<DocumentFormat.OpenXml.Spreadsheet.Row> rows, SharedStringTablePart stringTable)
        {
            var items = new List<CalendarShiftChanges>();
            int? fiscalYear, fiscalQuarter, fiscalMonth, fiscalWeek;

            var headers = rows.ElementAt(0).Elements<Cell>();
            int headerCount = headers.Count();
            if (headerCount == COL_COUNT_EXPORT || headers.Count() >= COL_COUNT_NORMAL)
            {
                for (int i = 1; i < rowsCount; i++)
                {
                    var cells = rows.ElementAt(i).Elements<Cell>();
                    var resultCells = GetResultCells(cells);

                    if (cells.Count() < 7 || (resultCells.ElementAt(1).CellValue == null && resultCells.ElementAt(3).CellValue == null && resultCells.ElementAt(4).CellValue == null))//7 cells are required only
                    {
                        if (i == 1) //Only display error if the 1st row is malformed.
                        {
                            Page.DisplayMessage(ExcellErrorLabel.Text, false);
                        }
                        break;
                    }

                    DateTime calendarDate = GetCellDateTime(resultCells.ElementAt(1));
                    DateTime shiftStart = GetCellDateTime(resultCells.ElementAt(3));
                    DateTime shiftEnd = GetCellDateTime(resultCells.ElementAt(4));
                    if (calendarDate == DateTime.MinValue || shiftStart == DateTime.MinValue || shiftEnd == DateTime.MinValue)
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
                }
            } else
                Page.DisplayMessage(ExcellErrorLabel.Text, false);
            return items.ToArray();
        }

        protected virtual CalendarShiftChanges[] ParseImportedShiftsForUpdate(int rowsCount, IEnumerable<DocumentFormat.OpenXml.Spreadsheet.Row> rows, SharedStringTablePart stringTable)
        {
            var items = new List<CalendarShiftChanges>();
            var presentCalendarShiftsInGrid = GetCalendarShiftsInGrid();
            int? fiscalYear, fiscalQuarter, fiscalMonth, fiscalWeek;

            for (int i = 1; i < rowsCount; i++)
            {
                var headers = rows.ElementAt(0).Elements<Cell>();
                if (headers.Count() < 12)
                {
                    Page.DisplayMessage(ExcellErrorLabel.Text, false);
                    break;

                }
                var cells = rows.ElementAt(i).Elements<Cell>();
                var resultCells = GetResultCells(cells);

                if (cells.Count() < 7)//7 cells are required only
                {
                    if (i == 1) //Only display error if the 1st row is malformed.
                    {
                        Page.DisplayMessage(ExcellErrorLabel.Text, false);
                    }
                    break;
                }

                string calendarShiftId = (resultCells.ElementAt(0).DataType != null && resultCells.ElementAt(0).DataType == CellValues.SharedString) ?
                    stringTable.SharedStringTable.ElementAt(Convert.ToInt32(resultCells.ElementAt(0).CellValue.Text)).InnerText.Trim() : "";//CalendarShiftId
                DateTime calendarDate = GetCellDateTime(resultCells.ElementAt(1));
                DateTime shiftStart = GetCellDateTime(resultCells.ElementAt(3));
                DateTime shiftEnd = GetCellDateTime(resultCells.ElementAt(4));
                if (calendarDate == DateTime.MinValue || shiftStart == DateTime.MinValue || shiftEnd == DateTime.MinValue)
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
                            c.Shift = (resultCells.ElementAt(2).DataType != null && resultCells.ElementAt(2).DataType == CellValues.SharedString) ?
                            new NamedObjectRef(stringTable.SharedStringTable.ElementAt(Convert.ToInt32(resultCells.ElementAt(2).CellValue.Text)).InnerText) :
                            new NamedObjectRef(shift);
                            c.ShiftStart = shiftStart;
                            c.ShiftEnd = shiftEnd;
                            //Not required fields
                            c.Team = (resultCells.ElementAt(5).DataType != null && resultCells.ElementAt(5).DataType == CellValues.SharedString) ?
                            new NamedObjectRef(stringTable.SharedStringTable.ElementAt(Convert.ToInt32(team)).InnerText) :
                            new NamedObjectRef(team);
                            c.FiscalYear = fiscalYear;
                            c.FiscalQuarter = fiscalQuarter;
                            c.FiscalMonth = fiscalMonth;
                            c.FiscalWeek = fiscalWeek;
                            return true;
                        });
                }
                //Append calendar shifts(imported via excel sheet) in grid with empty calendar shift Id
                else
                {
                    AddNewShifts(ref items, calendarDate, resultCells, stringTable, shiftStart, shiftEnd, team, fiscalYear, fiscalQuarter, fiscalMonth, fiscalWeek, shift, teamInt, shiftInt);
                }
            }
            return presentCalendarShiftsInGrid.Concat(items).ToArray();
        }

        protected virtual void AddNewShifts(ref List<CalendarShiftChanges> items, DateTime calendarDate, IEnumerable<Cell> resultCells,
            SharedStringTablePart stringTable, DateTime shiftStart, DateTime shiftEnd, string team, int? fiscalYear, int? fiscalQuarter, int? fiscalMonth, int? fiscalWeek, string shift, int teamInt, int shiftInt)
        {
            items.Add(new CalendarShiftChanges
            {
                CalendarDate =calendarDate,
                Shift = shiftInt >= 0 ? new NamedObjectRef(stringTable.SharedStringTable.ElementAt(shiftInt).InnerText) : new NamedObjectRef(shift),
                ShiftStart = shiftStart,
                ShiftEnd = shiftEnd,
                //Not required fields
                Team = teamInt >= 0 ? new NamedObjectRef(stringTable.SharedStringTable.ElementAt(teamInt).InnerText) : new NamedObjectRef(team),
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
            if (cellValue != null && !string.IsNullOrEmpty(cellValue.Text))
            {
                value = int.Parse(cellValue.Text);
            }
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
    }
}
