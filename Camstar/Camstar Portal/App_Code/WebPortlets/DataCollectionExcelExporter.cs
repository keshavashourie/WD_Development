// Copyright Siemens 2023
using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using Camstar.WebPortal.WebPortlets;
using CamstarPortal.WebControls.DataCollections;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.ExtendedProperties;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.VariantTypes;
using ap = DocumentFormat.OpenXml.ExtendedProperties;
using vt = DocumentFormat.OpenXml.VariantTypes;
using a = DocumentFormat.OpenXml.Drawing;
using BottomBorder = DocumentFormat.OpenXml.Spreadsheet.BottomBorder;
using Fill = DocumentFormat.OpenXml.Spreadsheet.Fill;
using Fonts = DocumentFormat.OpenXml.Spreadsheet.Fonts;
using FontScheme = DocumentFormat.OpenXml.Spreadsheet.FontScheme;
using ForegroundColor = DocumentFormat.OpenXml.Spreadsheet.ForegroundColor;
using GradientFill = DocumentFormat.OpenXml.Drawing.GradientFill;
using GradientStop = DocumentFormat.OpenXml.Drawing.GradientStop;
using Hyperlink = DocumentFormat.OpenXml.Drawing.Hyperlink;
using LeftBorder = DocumentFormat.OpenXml.Spreadsheet.LeftBorder;
using Outline = DocumentFormat.OpenXml.Drawing.Outline;
using PatternFill = DocumentFormat.OpenXml.Spreadsheet.PatternFill;
using RightBorder = DocumentFormat.OpenXml.Spreadsheet.RightBorder;
using Run = DocumentFormat.OpenXml.Spreadsheet.Run;
using RunProperties = DocumentFormat.OpenXml.Spreadsheet.RunProperties;
using Text = DocumentFormat.OpenXml.Spreadsheet.Text;
using TopBorder = DocumentFormat.OpenXml.Spreadsheet.TopBorder;

using Camstar.WebPortal.Constants;


namespace Camstar.WebPortal.PortalFramework
{
    public class DataCollectionExcelExporter
    {
        public virtual void CreatePackageDataCollection(DataSet ds, string activityRole, bool isActivityCompletion,
                                                WebPartPageBase page, string fileName, string title)
        {
            var stream = new MemoryStream();
            _activityRole = activityRole;
            _isActivityCompletion = isActivityCompletion;
            _title = title;

            using (SpreadsheetDocument package = SpreadsheetDocument.Create(stream, SpreadsheetDocumentType.Workbook))
            {
                AddPartsDataCollection(package, ds);
            }

            stream.Close();
            page.Session["DownloadData"] = stream.ToArray();
            page.RedirectCall(
                string.Format("StartDownloadFile(\"{0}.xlsx\", \"{1}\", \"{2}\")", fileName, stream.GetHashCode(), 0),
                false);
        }

        public virtual void CreateCSVPackageDataCollection(DataSet ds, WebPartPageBase page, string fileName)
        {
            var stream = new MemoryStream();
            var sw = new StringWriter();

            for (int i = 0; i < ds.Tables.Count; i++)
            {
                if (ds.Tables[i].Columns.Count > 0)
                {
                    int iColCount = ds.Tables[i].Columns.Count;
                    DataTable dt = ds.Tables[i];

                    if (i > 0)
                    {
                        var excelDataTable = dt as ExcelDataTable;
                        sw.Write(string.Format("Name of Data Group: {0}", excelDataTable));
                        sw.Write(",");
                        sw.Write(sw.NewLine);

                        if (!String.IsNullOrEmpty(excelDataTable.LastEnteredBy))
                        {
                            sw.Write(string.Format("Last entered By: {0} on {1}", excelDataTable.LastEnteredBy,
                                                   excelDataTable.LastEnteredDateTime));
                            sw.Write(",");
                            sw.Write(sw.NewLine);
                        }

                        if (!String.IsNullOrEmpty(excelDataTable.Role))
                        {
                            sw.Write(string.Format("Role: {0}", excelDataTable.Role));
                            sw.Write(",");
                            sw.Write(sw.NewLine);
                        }
                    }

                    for (int j = 0; j < iColCount; j++)
                    {
                        string columnName = (dt.Columns[j].ExtendedProperties[ExtendedPropertyConstants.Caption] == null)
                                                ? string.Empty
                                                : dt.Columns[j].ExtendedProperties[ExtendedPropertyConstants.Caption].ToString();
                        sw.Write(string.Format("\"{0}\"", columnName));
                        if (j < iColCount - 1)
                        {
                            sw.Write(",");
                        }
                    }
                    sw.Write(sw.NewLine);

                    foreach (DataRow dr in dt.Rows)
                    {
                        for (int j = 0; j < iColCount; j++)
                        {
                            if (!Convert.IsDBNull(dr[j]))
                            {
                                string text = string.Empty;
                                if (!string.IsNullOrEmpty(dr[j].ToString()))
                                {
                                    text = dr[j].ToString().Split('~')[0];
                                }

                                sw.Write(string.Format("\"{0}\"", text));
                            }
                            if (j < iColCount - 1)
                            {
                                sw.Write(",");
                            }
                        }
                        sw.Write(sw.NewLine);
                    }
                }

                sw.Write(sw.NewLine);
            }

            sw.Close();

            var uniEncoding = new UnicodeEncoding();
            byte[] byteString = uniEncoding.GetBytes(sw.ToString());
            int count = 0;
            while (count < byteString.Length)
            {
                stream.WriteByte(byteString[count++]);
            }

            stream.Seek(0, SeekOrigin.Begin);

            stream.Close();
            page.Session["DownloadData"] = stream.ToArray();
            page.RedirectCall(string.Format("StartDownloadFile(\"{0}.csv\", \"{1}\", \"{2}\")",
                fileName, stream.GetHashCode(), 0), false);
        }

        protected virtual void ParseTableDataCollection(WorksheetPart worksheetPart, SharedStringTablePart sharedStringTablePart,
                                              DataTable table)
        {
            var columns = worksheetPart.Worksheet.GetFirstChild<Columns>();
            var cls = new Column[table.Columns.Count];
            for (int i = 0; i < table.Columns.Count; i++)
            {
                DataColumn column = table.Columns[i];
                if (column.ExtendedProperties[ExtendedPropertyConstants.Visible] != null &&
                    (bool)column.ExtendedProperties[ExtendedPropertyConstants.Visible])
                {
                    int index = (int)column.ExtendedProperties[ExtendedPropertyConstants.Index];

                    double width = i == 0 ? 20.0 : 45.0;

                    cls[index] = new Column
                                     {
                                         Min = (uint)(index + 1),
                                         Max = (uint)(index + 1),
                                         Width = width,
                                         CustomWidth = true
                                     };
                }
            }
            columns.Append(cls.OfType<OpenXmlElement>());
            worksheetPart.Worksheet.Save();

            Cell cell;
            uint rowIndex = 2;
            string columnName = ((char)('A' + 0)).ToString();

            if (!table.TableName.Contains("DataCollectionDetails"))
            {
                var run1 = new Run();
                var runProperties1 = new RunProperties();
                var bold2 = new Bold();
                var fontSize4 = new FontSize { Val = 12D };
                var color28 = new Color { Theme = 1U };
                var runFont1 = new RunFont { Val = "Calibri" };
                var fontFamily1 = new FontFamily { Val = 2 };
                var fontScheme5 = new FontScheme { Val = FontSchemeValues.Minor };

                runProperties1.Append(bold2);
                runProperties1.Append(fontSize4);
                runProperties1.Append(color28);
                runProperties1.Append(runFont1);
                runProperties1.Append(fontFamily1);
                runProperties1.Append(fontScheme5);
                var text1 = new Text { Text = table.TableName };

                run1.Append(runProperties1);
                run1.Append(text1);

                var sharedStringItem1 = new SharedStringItem();
                sharedStringItem1.Append(run1);

                int index1 = InsertSharedStringItem(table.TableName, sharedStringTablePart, sharedStringItem1);
                cell = InsertCellInWorksheet(columnName, rowIndex, worksheetPart, false);
                cell.CellValue = new CellValue((index1).ToString());
                cell.StyleIndex = 7U;
                cell.DataType = new EnumValue<CellValues>(CellValues.SharedString);

                if (_isActivityCompletion)
                {
                    rowIndex++;
                    index1 = InsertSharedStringItem("Last Entered By", sharedStringTablePart, null);
                    cell = InsertCellInWorksheet(columnName, rowIndex, worksheetPart, false);
                    cell.CellValue = new CellValue((index1).ToString());
                    cell.DataType = new EnumValue<CellValues>(CellValues.SharedString);

                    rowIndex++;
                    index1 = InsertSharedStringItem("Role", sharedStringTablePart, null);
                    cell = InsertCellInWorksheet(columnName, rowIndex, worksheetPart, false);
                    cell.CellValue = new CellValue((index1).ToString());
                    cell.DataType = new EnumValue<CellValues>(CellValues.SharedString);

                    rowIndex--;
                    columnName = ((char)('A' + 1)).ToString();
                    index1 = InsertSharedStringItem(DateTime.Now.ToString(), sharedStringTablePart, null);
                    cell = InsertCellInWorksheet(columnName, rowIndex, worksheetPart, false);
                    cell.CellValue = new CellValue((index1).ToString());
                    cell.DataType = new EnumValue<CellValues>(CellValues.SharedString);

                    rowIndex++;
                    index1 = InsertSharedStringItem(_activityRole, sharedStringTablePart, null);
                    cell = InsertCellInWorksheet(columnName, rowIndex, worksheetPart, false);
                    cell.CellValue = new CellValue((index1).ToString());
                    cell.DataType = new EnumValue<CellValues>(CellValues.SharedString);
                }
            }
            else
            {
                int index1 = InsertSharedStringItem(_title, sharedStringTablePart, null);
                columnName = ((char)('A' + 1)).ToString();
                cell = InsertCellInWorksheet(columnName, rowIndex, worksheetPart, false);
                cell.CellValue = new CellValue((index1).ToString());
                cell.StyleIndex = 7U;
                cell.DataType = new EnumValue<CellValues>(CellValues.SharedString);
            }

            for (int i = 0; i < table.Columns.Count; i++)
            {
                DataColumn column = table.Columns[i];
                if (column.ExtendedProperties[ExtendedPropertyConstants.Visible] != null &&
                    (bool)column.ExtendedProperties[ExtendedPropertyConstants.Visible])
                {
                    string text = (column.ExtendedProperties[ExtendedPropertyConstants.Caption] == null)
                                      ? string.Empty
                                      : column.ExtendedProperties[ExtendedPropertyConstants.Caption].ToString();
                    SharedStringItem sharedStringItem1 = null;
                    if (!string.IsNullOrEmpty(text) && !table.TableName.Contains("DataCollectionDetails"))
                    {
                        var run1 = new Run();
                        var runProperties1 = new RunProperties();
                        //var bold2 = new Bold();
                        var fontSize4 = new FontSize { Val = 9D };
                        var color28 = new Color { Theme = 1U };
                        var runFont1 = new RunFont { Val = "Calibri" };
                        var fontFamily1 = new FontFamily { Val = 2 };
                        var fontScheme5 = new FontScheme { Val = FontSchemeValues.Major };

                        //runProperties1.Append(bold2);
                        runProperties1.Append(fontSize4);
                        runProperties1.Append(color28);
                        runProperties1.Append(runFont1);
                        runProperties1.Append(fontFamily1);
                        runProperties1.Append(fontScheme5);
                        var text1 = new Text { Space = SpaceProcessingModeValues.Preserve, Text = text.Split('~')[0] };

                        run1.Append(runProperties1);
                        run1.Append(text1);

                        sharedStringItem1 = new SharedStringItem();

                        var run2 = new Run();
                        if (text.Split('~').Length > 1)
                        {
                            var runProperties2 = new RunProperties();
                            var fontSize5 = new FontSize { Val = 9D };
                            var color29 = new Color { Theme = 1U };
                            var runFont2 = new RunFont { Val = "Calibri" };
                            var fontFamily2 = new FontFamily { Val = 2 };
                            var fontScheme6 = new FontScheme { Val = FontSchemeValues.Major };

                            runProperties2.Append(fontSize5);
                            runProperties2.Append(color29);
                            runProperties2.Append(runFont2);
                            runProperties2.Append(fontFamily2);
                            runProperties2.Append(fontScheme6);
                            var text2 = new Text { Text = text.Split('~')[1] };

                            run2.Append(runProperties2);
                            run2.Append(text2);
                        }

                        sharedStringItem1.Append(run1);
                        sharedStringItem1.Append(run2);
                    }
                    else if (table.TableName.Contains("DataCollectionDetails") && i == 0)
                    {
                        var run2 = new Run();
                        var runProperties = new RunProperties();
                        var bold = new Bold();
                        var fontSize = new FontSize { Val = 12D };
                        var color = new Color { Theme = 1U };
                        var runFont = new RunFont { Val = "Calibri" };
                        var fontFamily = new FontFamily { Val = 2 };
                        var fontScheme = new FontScheme { Val = FontSchemeValues.Major };

                        runProperties.Append(bold);
                        runProperties.Append(fontSize);
                        runProperties.Append(color);
                        runProperties.Append(runFont);
                        runProperties.Append(fontFamily);
                        runProperties.Append(fontScheme);
                        var value3 = new Text { Text = text };

                        run2.Append(runProperties);
                        run2.Append(value3);

                        sharedStringItem1 = new SharedStringItem();
                        sharedStringItem1.Append(run2);
                    }

                    int index = InsertSharedStringItem(text, sharedStringTablePart, sharedStringItem1);
                    cell = InsertCellInWorksheet(
                        ((char)('A' + (int)column.ExtendedProperties[ExtendedPropertyConstants.Index] + 1)).ToString(), 6,
                        worksheetPart, true);

                    cell.CellValue = new CellValue((index).ToString());
                    cell.StyleIndex = i == 0 ? 6U : (table.TableName == "DataCollectionDetails" ? 6U : 2U);
                    cell.DataType = new EnumValue<CellValues>(CellValues.SharedString);
                }
            }

            for (int i = 0; i < table.Columns.Count; i++)
            {
                DataColumn column = table.Columns[i];
                if (column.ExtendedProperties[ExtendedPropertyConstants.Visible] != null &&
                    (bool)column.ExtendedProperties[ExtendedPropertyConstants.Visible])
                {
                    int j;
                    for (j = 0; j < table.Rows.Count; j++)
                    {
                        string text = table.Rows[j][i].ToString();
                        SharedStringItem sharedStringItem1 = null;
                        if (table.TableName.Contains("DataCollectionDetails") && i == 0)
                        {
                            var run = new Run();
                            var runProperties = new RunProperties();
                            var bold = new Bold();
                            var fontSize = new FontSize { Val = 12D };
                            var color = new Color { Theme = 1U };
                            var runFont = new RunFont { Val = "Calibri" };
                            var fontFamily = new FontFamily { Val = 2 };
                            var fontScheme = new FontScheme { Val = FontSchemeValues.Major };

                            runProperties.Append(bold);
                            runProperties.Append(fontSize);
                            runProperties.Append(color);
                            runProperties.Append(runFont);
                            runProperties.Append(fontFamily);
                            runProperties.Append(fontScheme);
                            var value = new Text { Text = text };

                            run.Append(runProperties);
                            run.Append(value);

                            sharedStringItem1 = new SharedStringItem();
                            sharedStringItem1.Append(run);
                        }
                        int index = InsertSampleValues(text, sharedStringTablePart, sharedStringItem1);
                        cell = InsertCellInWorksheet(((char)('A' + (int)column.ExtendedProperties[ExtendedPropertyConstants.Index] + 1)).
                                    ToString(), (uint)(j + 2) + 5, worksheetPart, false);
                        cell.CellValue = new CellValue((index).ToString());
                        
                        if (i == 0 && table.TableName != "DataCollectionDetails")
                        {
                            cell.StyleIndex = 2U;
                        }
                        else
                        {
                            cell.StyleIndex = 6U;
                        }
                        cell.DataType = new EnumValue<CellValues>(CellValues.SharedString);
                    }
                }
            }
            sharedStringTablePart.SharedStringTable.Save();
            worksheetPart.Worksheet.Save();
        }

        protected virtual void AddPartsDataCollection(SpreadsheetDocument spreadsheetDocument, DataSet ds)
        {
            WorkbookPart workbookpart = spreadsheetDocument.AddWorkbookPart();
            workbookpart.Workbook = new Workbook();

            var extendedFilePropertiesPart1 = spreadsheetDocument.AddNewPart<ExtendedFilePropertiesPart>("rId3");
            GenerateExtendedFilePropertiesPart1().Save(extendedFilePropertiesPart1);

            var workbookStylesPart1 = workbookpart.AddNewPart<WorkbookStylesPart>("rId3");
            GenerateWorkbookStylesPart1().Save(workbookStylesPart1);

            var themePart1 = workbookpart.AddNewPart<ThemePart>("rId2");
            GenerateThemePart1().Save(themePart1);

            var sharedStringTablePart1 = workbookpart.AddNewPart<SharedStringTablePart>("rId4");
            GenerateSharedStringTablePart1().Save(sharedStringTablePart1);

            // Add Sheets to the Workbook.
            Sheets sheets = spreadsheetDocument.WorkbookPart.Workbook.AppendChild(new Sheets());

            for (int i = 0; i < ds.Tables.Count; i++)
            {
                // Add a WorksheetPart to the WorkbookPart.
                var worksheetPart = workbookpart.AddNewPart<WorksheetPart>();
                GenerateWorksheetPart1().Save(worksheetPart);
                worksheetPart.Worksheet.Save();
                var sheet = new Sheet
                                  {
                                      Id = workbookpart.GetIdOfPart(worksheetPart),
                                      SheetId = (uint)(sheets.Count() + 1),
                                      Name = ds.Tables[i].ToString()
                                  };
                sheets.Append(sheet);
                ParseTableDataCollection(worksheetPart, sharedStringTablePart1, ds.Tables[i]);
            }

            workbookpart.Workbook.Save();
        }

        // Given text and a SharedStringTablePart, creates a SharedStringItem with the specified text 
        // and inserts it into the SharedStringTablePart. If the item already exists, returns its index.
        public static int InsertSharedStringItem(string text, SharedStringTablePart shareStringPart,
                                                 SharedStringItem stringItem)
        {
            // If the part does not contain a SharedStringTable, create one.
            if (shareStringPart.SharedStringTable == null)
            {
                shareStringPart.SharedStringTable = new SharedStringTable();
            }

            int i = 0;

            // Iterate through all the items in the SharedStringTable. If the text already exists, return its index.
            
            foreach (SharedStringItem item in shareStringPart.SharedStringTable.Elements<SharedStringItem>())
            {
                if (item.InnerText == text)
                {
                    return i;
                }

                i++;
            }

            shareStringPart.SharedStringTable.AppendChild(stringItem ?? new SharedStringItem(new Text(text)));
            shareStringPart.SharedStringTable.Save();

            return i;
        }

        // Given text and a SharedStringTablePart, creates a SharedStringItem with the specified text 
        // and inserts it into the SharedStringTablePart. If the item already exists, returns its index.
        public static int InsertSampleValues(string text, SharedStringTablePart shareStringPart,
                                                 SharedStringItem stringItem)
        {
            // If the part does not contain a SharedStringTable, create one.
            if (shareStringPart.SharedStringTable == null)
            {
                shareStringPart.SharedStringTable = new SharedStringTable();
            }

            // Iterate through all the items in the SharedStringTable. If the text already exists, return its index.

            shareStringPart.SharedStringTable.AppendChild(stringItem ?? new SharedStringItem(new Text(text)));
            shareStringPart.SharedStringTable.Save();

            return shareStringPart.SharedStringTable.Elements<SharedStringItem>().Count();
        }

        // Given a column name, a row index, and a WorksheetPart, inserts a cell into the worksheet. 
        // If the cell already exists, returns it. 
        public static Cell InsertCellInWorksheet(string columnName, uint rowIndex, WorksheetPart worksheetPart,
                                                 bool isColumn)
        {
            Worksheet worksheet = worksheetPart.Worksheet;
            var sheetData = worksheet.GetFirstChild<SheetData>();
            string cellReference = columnName + rowIndex;

            // If the worksheet does not contain a row with the specified row index, insert one.
            Row row;

            if (sheetData.Elements<Row>().Where(r => r.RowIndex == rowIndex).Count() != 0)
            {
                row = sheetData.Elements<Row>().Where(r => r.RowIndex == rowIndex).First();
            }
            else
            {
                row = new Row { RowIndex = rowIndex };
                sheetData.Append(row);
            }

            row.CustomHeight = true;
            row.Height = isColumn ? 50.0 : 30.0;

            // If there is not a cell with the specified column name, insert one.  
            if (row.Elements<Cell>().Where(c => c.CellReference.Value == columnName + rowIndex).Count() > 0)
            {
                return row.Elements<Cell>().Where(c => c.CellReference.Value == cellReference).First();
            }

            // Cells must be in sequential order according to CellReference. Determine where to insert the new cell.
            Cell refCell = null;
            foreach (Cell cell in row.Elements<Cell>())
            {
                if (string.Compare(cell.CellReference.Value, cellReference, true) > 0)
                {
                    refCell = cell;
                    break;
                }
            }

            var newCell = new Cell { CellReference = cellReference };

            row.InsertBefore(newCell, refCell);

            return newCell;
        }

        protected static DocumentFormat.OpenXml.ExtendedProperties.Properties GenerateExtendedFilePropertiesPart1()
        {
            return new DocumentFormat.OpenXml.ExtendedProperties.Properties(
                    new Application("Microsoft Excel"),
                    new DocumentSecurity("0"),
                    new ScaleCrop("false"),
                    new HeadingPairs(new VTVector(new Variant(new VTLPSTR("Worksheets")),
                            new Variant(new VTInt32("1"))) { BaseType = VectorBaseValues.Variant, Size = (UInt32Value)2U }),
                    new TitlesOfParts(new VTVector(new VTLPSTR("Results")) { BaseType = VectorBaseValues.Lpstr, Size = (UInt32Value)1U }),
                    new LinksUpToDate("false"),
                    new SharedDocument("false"),
                    new HyperlinksChanged("false"),
                    new ApplicationVersion("12.0000"));
        }

        protected static Stylesheet GenerateWorkbookStylesPart1()
        {
            return new Stylesheet(new Fonts(new Font(
                            new FontSize { Val = 9D },
                            new Color { Theme = (UInt32Value)1U },
                            new FontName { Val = "Calibri" },
                            new FontFamily { Val = 2 }),
                        new Font(
                            new FontSize { Val = 11D },
                            new Color { Theme = (UInt32Value)1U },
                            new FontName { Val = "Calibri" },
                            new FontFamily { Val = 2 }),
                        new Font(
                            new FontSize { Val = 15D },
                            new Color { Theme = (UInt32Value)1U },
                            new FontName { Val = "Calibri" },
                            new FontFamily { Val = 2 })
                        ) { Count = (UInt32Value)3U },
                    new Fills(new Fill(new PatternFill { PatternType = PatternValues.None }),
                        new Fill(new PatternFill { PatternType = PatternValues.Gray125 }),
                        new Fill(new PatternFill
                                     {
                                         PatternType = PatternValues.Solid,
                                         ForegroundColor = new ForegroundColor { Rgb = "C5D9F1" }
                                     }),
                        new Fill(new PatternFill
                                     {
                                         PatternType = PatternValues.Solid,
                                         ForegroundColor = new ForegroundColor { Rgb = "FFFF00" }
                                     }),
                        new Fill(new PatternFill
                                     {
                                         PatternType = PatternValues.Solid,
                                         ForegroundColor = new ForegroundColor { Rgb = "FF0000" }
                                     }),
                        new Fill(new PatternFill
                                     {
                                         PatternType = PatternValues.Solid,
                                         ForegroundColor = new ForegroundColor { Rgb = "D4842C" }
                                     }),
                        new Fill(new PatternFill { PatternType = PatternValues.None })
                        ) { Count = (UInt32Value)6U },
                    new Borders(
                        new Border(
                            new LeftBorder { Style = BorderStyleValues.None },
                            new RightBorder { Style = BorderStyleValues.None },
                            new TopBorder { Style = BorderStyleValues.None },
                            new BottomBorder { Style = BorderStyleValues.None },
                            new DiagonalBorder()),
                        new Border(
                            new LeftBorder { Style = BorderStyleValues.Thin, Color = new Color { Rgb = "000000" } },
                            new RightBorder { Style = BorderStyleValues.Thin, Color = new Color { Rgb = "000000" } },
                            new TopBorder { Style = BorderStyleValues.Thin, Color = new Color { Rgb = "000000" } },
                            new BottomBorder { Style = BorderStyleValues.Thin, Color = new Color { Rgb = "000000" } },
                            new DiagonalBorder())
                        ) { Count = (UInt32Value)2U },
                    new CellStyleFormats(
                        new CellFormat
                            {
                                NumberFormatId = (UInt32Value)0U,
                                FontId = (UInt32Value)0U,
                                FillId = (UInt32Value)0U,
                                BorderId = (UInt32Value)0U
                            },
                        new CellFormat
                            {
                                NumberFormatId = (UInt32Value)0U,
                                FontId = (UInt32Value)0U,
                                FillId = (UInt32Value)0U,
                                BorderId = (UInt32Value)1U
                            }
                        ) { Count = (UInt32Value)1U },
                    new CellFormats(
                        new CellFormat
                            {
                                NumberFormatId = (UInt32Value)0U,
                                FontId = (UInt32Value)0U,
                                FillId = (UInt32Value)0U,
                                BorderId = (UInt32Value)0U,
                                FormatId = (UInt32Value)0U
                            },
                        new CellFormat
                            {
                                NumberFormatId = (UInt32Value)0U,
                                FontId = (UInt32Value)1U,
                                FillId = (UInt32Value)1U,
                                BorderId = (UInt32Value)1U,
                                FormatId = (UInt32Value)1U
                            },
                        new CellFormat
                            {
                                NumberFormatId = (UInt32Value)0U,
                                FontId = (UInt32Value)0U,
                                FillId = (UInt32Value)2U,
                                BorderId = (UInt32Value)1U,
                                FormatId = (UInt32Value)1U
                            },
                        new CellFormat
                            {
                                NumberFormatId = (UInt32Value)0U,
                                FontId = (UInt32Value)1U,
                                FillId = (UInt32Value)3U,
                                BorderId = (UInt32Value)1U,
                                FormatId = (UInt32Value)1U
                            },
                        new CellFormat
                            {
                                NumberFormatId = (UInt32Value)0U,
                                FontId = (UInt32Value)1U,
                                FillId = (UInt32Value)4U,
                                BorderId = (UInt32Value)1U,
                                FormatId = (UInt32Value)1U
                            },
                        new CellFormat
                            {
                                NumberFormatId = (UInt32Value)0U,
                                FontId = (UInt32Value)1U,
                                FillId = (UInt32Value)5U,
                                BorderId = (UInt32Value)1U,
                                FormatId = (UInt32Value)1U
                            },
                        new CellFormat
                            {
                                NumberFormatId = (UInt32Value)0U,
                                FontId = (UInt32Value)1U,
                                FillId = (UInt32Value)6U,
                                BorderId = (UInt32Value)1U,
                                FormatId = (UInt32Value)1U
                            },
                        new CellFormat
                            {
                                NumberFormatId = (UInt32Value)0U,
                                FontId = (UInt32Value)2U,
                                FillId = (UInt32Value)6U,
                                BorderId = (UInt32Value)1U,
                                FormatId = (UInt32Value)1U
                            }
                        ) { Count = (UInt32Value)8U },
                    new CellStyles(
                        new CellStyle { Name = "Normal", FormatId = (UInt32Value)0U, BuiltinId = (UInt32Value)0U }
                        ) { Count = (UInt32Value)1U },
                    new DifferentialFormats { Count = (UInt32Value)0U },
                    new TableStyles
                        {
                            Count = (UInt32Value)0U,
                            DefaultTableStyle = "TableStyleMedium9",
                            DefaultPivotStyle = "PivotStyleLight16"
                        });
        }

        protected static Theme GenerateThemePart1()
        {
            return new Theme(new ThemeElements(new ColorScheme(new Dark1Color(
                                new SystemColor { Val = SystemColorValues.WindowText, LastColor = "000000" }),
                            new Light1Color(
                                new SystemColor { Val = SystemColorValues.Window, LastColor = "FFFFFF" }),
                            new Dark2Color(
                                new RgbColorModelHex { Val = "1F497D" }),
                            new Light2Color(
                                new RgbColorModelHex { Val = "EEECE1" }),
                            new Accent1Color(
                                new RgbColorModelHex { Val = "4F81BD" }),
                            new Accent2Color(
                                new RgbColorModelHex { Val = "C0504D" }),
                            new Accent3Color(
                                new RgbColorModelHex { Val = "9BBB59" }),
                            new Accent4Color(
                                new RgbColorModelHex { Val = "8064A2" }),
                            new Accent5Color(
                                new RgbColorModelHex { Val = "4BACC6" }),
                            new Accent6Color(
                                new RgbColorModelHex { Val = "F79646" }),
                            new Hyperlink(
                                new RgbColorModelHex { Val = "0000FF" }),
                            new FollowedHyperlinkColor(
                                new RgbColorModelHex { Val = "800080" })
                            ) { Name = "Office" },
                        new DocumentFormat.OpenXml.Drawing.FontScheme(
                            new MajorFont(
                                new LatinFont { Typeface = "Cambria" },
                                new EastAsianFont { Typeface = "" },
                                new ComplexScriptFont { Typeface = "" },
                                new SupplementalFont { Script = "Jpan", Typeface = "??­??? ??°?‚??‚·????‚?" },
                                new SupplementalFont { Script = "Hang", Typeface = "?§‘??? ?? ?”•" },
                                new SupplementalFont { Script = "Hans", Typeface = "?®‹??“" },
                                new SupplementalFont { Script = "Hant", Typeface = "?–°??°????«”" },
                                new SupplementalFont { Script = "Arab", Typeface = "Times New Roman" },
                                new SupplementalFont { Script = "Hebr", Typeface = "Times New Roman" },
                                new SupplementalFont { Script = "Thai", Typeface = "Tahoma" },
                                new SupplementalFont { Script = "Ethi", Typeface = "Nyala" },
                                new SupplementalFont { Script = "Beng", Typeface = "Vrinda" },
                                new SupplementalFont { Script = "Gujr", Typeface = "Shruti" },
                                new SupplementalFont { Script = "Khmr", Typeface = "MoolBoran" },
                                new SupplementalFont { Script = "Knda", Typeface = "Tunga" },
                                new SupplementalFont { Script = "Guru", Typeface = "Raavi" },
                                new SupplementalFont { Script = "Cans", Typeface = "Euphemia" },
                                new SupplementalFont { Script = "Cher", Typeface = "Plantagenet Cherokee" },
                                new SupplementalFont { Script = "Yiii", Typeface = "Microsoft Yi Baiti" },
                                new SupplementalFont { Script = "Tibt", Typeface = "Microsoft Himalaya" },
                                new SupplementalFont { Script = "Thaa", Typeface = "MV Boli" },
                                new SupplementalFont { Script = "Deva", Typeface = "Mangal" },
                                new SupplementalFont { Script = "Telu", Typeface = "Gautami" },
                                new SupplementalFont { Script = "Taml", Typeface = "Latha" },
                                new SupplementalFont { Script = "Syrc", Typeface = "Estrangelo Edessa" },
                                new SupplementalFont { Script = "Orya", Typeface = "Kalinga" },
                                new SupplementalFont { Script = "Mlym", Typeface = "Kartika" },
                                new SupplementalFont { Script = "Laoo", Typeface = "DokChampa" },
                                new SupplementalFont { Script = "Sinh", Typeface = "Iskoola Pota" },
                                new SupplementalFont { Script = "Mong", Typeface = "Mongolian Baiti" },
                                new SupplementalFont { Script = "Viet", Typeface = "Times New Roman" },
                                new SupplementalFont { Script = "Uigh", Typeface = "Microsoft Uighur" }),
                            new MinorFont(
                                new LatinFont { Typeface = "Calibri" },
                                new EastAsianFont { Typeface = "" },
                                new ComplexScriptFont { Typeface = "" },
                                new SupplementalFont { Script = "Jpan", Typeface = "??­??? ??°?‚??‚·????‚?" },
                                new SupplementalFont { Script = "Hang", Typeface = "?§‘??? ?? ?”•" },
                                new SupplementalFont { Script = "Hans", Typeface = "?®‹??“" },
                                new SupplementalFont { Script = "Hant", Typeface = "?–°??°????«”" },
                                new SupplementalFont { Script = "Arab", Typeface = "Arial" },
                                new SupplementalFont { Script = "Hebr", Typeface = "Arial" },
                                new SupplementalFont { Script = "Thai", Typeface = "Tahoma" },
                                new SupplementalFont { Script = "Ethi", Typeface = "Nyala" },
                                new SupplementalFont { Script = "Beng", Typeface = "Vrinda" },
                                new SupplementalFont { Script = "Gujr", Typeface = "Shruti" },
                                new SupplementalFont { Script = "Khmr", Typeface = "DaunPenh" },
                                new SupplementalFont { Script = "Knda", Typeface = "Tunga" },
                                new SupplementalFont { Script = "Guru", Typeface = "Raavi" },
                                new SupplementalFont { Script = "Cans", Typeface = "Euphemia" },
                                new SupplementalFont { Script = "Cher", Typeface = "Plantagenet Cherokee" },
                                new SupplementalFont { Script = "Yiii", Typeface = "Microsoft Yi Baiti" },
                                new SupplementalFont { Script = "Tibt", Typeface = "Microsoft Himalaya" },
                                new SupplementalFont { Script = "Thaa", Typeface = "MV Boli" },
                                new SupplementalFont { Script = "Deva", Typeface = "Mangal" },
                                new SupplementalFont { Script = "Telu", Typeface = "Gautami" },
                                new SupplementalFont { Script = "Taml", Typeface = "Latha" },
                                new SupplementalFont { Script = "Syrc", Typeface = "Estrangelo Edessa" },
                                new SupplementalFont { Script = "Orya", Typeface = "Kalinga" },
                                new SupplementalFont { Script = "Mlym", Typeface = "Kartika" },
                                new SupplementalFont { Script = "Laoo", Typeface = "DokChampa" },
                                new SupplementalFont { Script = "Sinh", Typeface = "Iskoola Pota" },
                                new SupplementalFont { Script = "Mong", Typeface = "Mongolian Baiti" },
                                new SupplementalFont { Script = "Viet", Typeface = "Arial" },
                                new SupplementalFont { Script = "Uigh", Typeface = "Microsoft Uighur" })
                            ) { Name = "Office" },
                        new FormatScheme(
                            new FillStyleList(
                                new SolidFill(
                                    new SchemeColor { Val = SchemeColorValues.PhColor }),
                                new GradientFill(
                                    new GradientStopList(
                                        new GradientStop(
                                            new SchemeColor(
                                                new Tint { Val = 50000 },
                                                new SaturationModulation { Val = 300000 }
                                                ) { Val = SchemeColorValues.PhColor }
                                            ) { Position = 0 },
                                        new GradientStop(
                                            new SchemeColor(
                                                new Tint { Val = 37000 },
                                                new SaturationModulation { Val = 300000 }
                                                ) { Val = SchemeColorValues.PhColor }
                                            ) { Position = 35000 },
                                        new GradientStop(
                                            new SchemeColor(
                                                new Tint { Val = 15000 },
                                                new SaturationModulation { Val = 350000 }
                                                ) { Val = SchemeColorValues.PhColor }
                                            ) { Position = 100000 }),
                                    new LinearGradientFill { Angle = 16200000, Scaled = true }
                                    ) { RotateWithShape = true },
                                new GradientFill(
                                    new GradientStopList(
                                        new GradientStop(
                                            new SchemeColor(
                                                new Shade { Val = 51000 },
                                                new SaturationModulation { Val = 130000 }
                                                ) { Val = SchemeColorValues.PhColor }
                                            ) { Position = 0 },
                                        new GradientStop(
                                            new SchemeColor(
                                                new Shade { Val = 93000 },
                                                new SaturationModulation { Val = 130000 }
                                                ) { Val = SchemeColorValues.PhColor }
                                            ) { Position = 80000 },
                                        new GradientStop(
                                            new SchemeColor(
                                                new Shade { Val = 94000 },
                                                new SaturationModulation { Val = 135000 }
                                                ) { Val = SchemeColorValues.PhColor }
                                            ) { Position = 100000 }),
                                    new LinearGradientFill { Angle = 16200000, Scaled = false }
                                    ) { RotateWithShape = true }),
                            new LineStyleList(
                                new Outline(
                                    new SolidFill(
                                        new SchemeColor(
                                            new Shade { Val = 95000 },
                                            new SaturationModulation { Val = 105000 }
                                            ) { Val = SchemeColorValues.PhColor }),
                                    new PresetDash { Val = PresetLineDashValues.Solid }
                                    )
                                    {
                                        Width = 9525,
                                        CapType = LineCapValues.Flat,
                                        CompoundLineType = CompoundLineValues.Single,
                                        Alignment = PenAlignmentValues.Center
                                    },
                                new Outline(
                                    new SolidFill(
                                        new SchemeColor { Val = SchemeColorValues.PhColor }),
                                    new PresetDash { Val = PresetLineDashValues.Solid }
                                    )
                                    {
                                        Width = 25400,
                                        CapType = LineCapValues.Flat,
                                        CompoundLineType = CompoundLineValues.Single,
                                        Alignment = PenAlignmentValues.Center
                                    },
                                new Outline(
                                    new SolidFill(
                                        new SchemeColor { Val = SchemeColorValues.PhColor }),
                                    new PresetDash { Val = PresetLineDashValues.Solid }
                                    )
                                    {
                                        Width = 38100,
                                        CapType = LineCapValues.Flat,
                                        CompoundLineType = CompoundLineValues.Single,
                                        Alignment = PenAlignmentValues.Center
                                    }),
                            new EffectStyleList(
                                new EffectStyle(
                                    new EffectList(
                                        new OuterShadow(
                                            new RgbColorModelHex(
                                                new Alpha { Val = 38000 }
                                                ) { Val = "000000" }
                                            )
                                            {
                                                BlurRadius = 40000L,
                                                Distance = 20000L,
                                                Direction = 5400000,
                                                RotateWithShape = false
                                            })),
                                new EffectStyle(
                                    new EffectList(
                                        new OuterShadow(
                                            new RgbColorModelHex(
                                                new Alpha { Val = 35000 }
                                                ) { Val = "000000" }
                                            )
                                            {
                                                BlurRadius = 40000L,
                                                Distance = 23000L,
                                                Direction = 5400000,
                                                RotateWithShape = false
                                            })),
                                new EffectStyle(
                                    new EffectList(
                                        new OuterShadow(
                                            new RgbColorModelHex(
                                                new Alpha { Val = 35000 }
                                                ) { Val = "000000" }
                                            )
                                            {
                                                BlurRadius = 40000L,
                                                Distance = 23000L,
                                                Direction = 5400000,
                                                RotateWithShape = false
                                            }),
                                    new Scene3DType(
                                        new Camera(
                                            new Rotation { Latitude = 0, Longitude = 0, Revolution = 0 }
                                            ) { Preset = PresetCameraValues.OrthographicFront },
                                        new LightRig(
                                            new Rotation { Latitude = 0, Longitude = 0, Revolution = 1200000 }
                                            ) { Rig = LightRigValues.ThreePoints, Direction = LightRigDirectionValues.Top }),
                                    new Shape3DType(
                                        new BevelTop { Width = 63500L, Height = 25400L }))),
                            new BackgroundFillStyleList(
                                new SolidFill(
                                    new SchemeColor { Val = SchemeColorValues.PhColor }),
                                new GradientFill(
                                    new GradientStopList(
                                        new GradientStop(
                                            new SchemeColor(
                                                new Tint { Val = 40000 },
                                                new SaturationModulation { Val = 350000 }
                                                ) { Val = SchemeColorValues.PhColor }
                                            ) { Position = 0 },
                                        new GradientStop(
                                            new SchemeColor(
                                                new Tint { Val = 45000 },
                                                new Shade { Val = 99000 },
                                                new SaturationModulation { Val = 350000 }
                                                ) { Val = SchemeColorValues.PhColor }
                                            ) { Position = 40000 },
                                        new GradientStop(
                                            new SchemeColor(
                                                new Shade { Val = 20000 },
                                                new SaturationModulation { Val = 255000 }
                                                ) { Val = SchemeColorValues.PhColor }
                                            ) { Position = 100000 }),
                                    new PathGradientFill(
                                        new FillToRectangle { Left = 50000, Top = -80000, Right = 50000, Bottom = 180000 }
                                        ) { Path = PathShadeValues.Circle }
                                    ) { RotateWithShape = true },
                                new GradientFill(
                                    new GradientStopList(
                                        new GradientStop(
                                            new SchemeColor(
                                                new Tint { Val = 80000 },
                                                new SaturationModulation { Val = 300000 }
                                                ) { Val = SchemeColorValues.PhColor }
                                            ) { Position = 0 },
                                        new GradientStop(
                                            new SchemeColor(
                                                new Shade { Val = 30000 },
                                                new SaturationModulation { Val = 200000 }
                                                ) { Val = SchemeColorValues.PhColor }
                                            ) { Position = 100000 }),
                                    new PathGradientFill(
                                        new FillToRectangle { Left = 50000, Top = 50000, Right = 50000, Bottom = 50000 }
                                        ) { Path = PathShadeValues.Circle }
                                    ) { RotateWithShape = true })
                            ) { Name = "Office" }),
                    new ObjectDefaults(),
                    new ExtraColorSchemeList()) { Name = "Office Theme" };
        }

        protected static Worksheet GenerateWorksheetPart1()
        {
            return new Worksheet(
                    new SheetDimension { Reference = "B6" },
                    new SheetViews(
                        new SheetView { TabSelected = true, WorkbookViewId = (UInt32Value)0U }),
                    new SheetFormatProperties { DefaultRowHeight = 15D, DefaultColumnWidth = 45D },
                    new Columns(),
                    new SheetData(),
                    new PageMargins { Left = 0.7D, Right = 0.7D, Top = 0.75D, Bottom = 0.75D, Header = 0.3D, Footer = 0.3D });
        }

        protected static SharedStringTable GenerateSharedStringTablePart1()
        {
            return new SharedStringTable();
        }

        private string _activityRole = string.Empty;
        private bool _isActivityCompletion;
        private string _title = string.Empty;

    }
}
