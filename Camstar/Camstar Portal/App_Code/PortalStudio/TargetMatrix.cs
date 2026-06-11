// Copyright Siemens 2023  
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using Camstar.WCF.ObjectStack;
using System.IO;
using Camstar.WebPortal.Personalization;
using System.ServiceModel;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

namespace WebClientPortal
{
    public partial class PortalStudioService
    {
        [OperationContract]
        public virtual ResultStatus LoadAllTargetMatrixItems(out string[] retVal)
        {
            bool status = false;
            string message = string.Empty;
            retVal = null;
            if (IsSessionValid())
            {
                if (IsSessionValid())
                {
                    try
                    {
                        if (File.Exists(HttpContext.Current.Server.MapPath(XmlPath)))
                        {
                            System.Xml.Serialization.XmlSerializer serializer = ItemsSerializer;
                            System.IO.Stream rd = File.OpenRead(HttpContext.Current.Server.MapPath(XmlPath));
                            var items = serializer.Deserialize(rd) as TargetMatrixItem[];
                            rd.Close();
                            if (items != null)
                            {
                                retVal = items.Select(item => item.Name).ToArray();
                            }
                        }
                        status = true;
                    }
                    catch (Exception e)
                    {
                        message = e.Message;
                    }
                }
            }
            return new ResultStatus(message, status);
        }

        [OperationContract]
        public virtual ResultStatus LoadTargetMatrixColumns(string name, out string[] retVal)
        {
            bool status = false;
            string message = string.Empty;
            retVal = null;
            if (IsSessionValid())
            {
                try
                {
                    if (File.Exists(HttpContext.Current.Server.MapPath(XmlPath)))
                    {
                        System.Xml.Serialization.XmlSerializer serializer = ItemsSerializer;
                        System.IO.Stream rd = File.OpenRead(HttpContext.Current.Server.MapPath(XmlPath));
                        var items = serializer.Deserialize(rd) as TargetMatrixItem[];
                        rd.Close();
                        if (items != null)
                        {
                            var matrixItem = items.FirstOrDefault(matrix => matrix.Name == name);
                            if (matrixItem != null && matrixItem.Columns != null)
                                retVal = matrixItem.Columns.Select(col => col.Name).ToArray();
                        }
                    }
                    status = true;
                }
                catch (Exception e)
                {
                    message = e.Message;
                }
            }
            return new ResultStatus(message, status);
        }

        [OperationContract]
        public virtual ResultStatus LoadTargetMatrixItems(out string xml)
        {
            bool status = false;
            string message = string.Empty;
            xml = null;

            if (IsSessionValid())
            {
                try
                {
                    if (File.Exists(HttpContext.Current.Server.MapPath(XmlPath)))
                        xml = File.ReadAllText(HttpContext.Current.Server.MapPath(XmlPath));
                    status = true;
                }
                catch (Exception e)
                {
                    message = e.Message;
                }
            }

            return new ResultStatus(message, status);
        }

        [OperationContract]
        public virtual ResultStatus LoadTargetMatrixItem(string name, out string xml)
        {
            bool status = false;
            string message = string.Empty;
            xml = null;

            if (IsSessionValid())
            {
                try
                {
                    if (File.Exists(HttpContext.Current.Server.MapPath(XmlPath)))
                    {
                        System.Xml.Serialization.XmlSerializer serializer = ItemsSerializer;
                        System.IO.Stream rd = File.OpenRead(HttpContext.Current.Server.MapPath(XmlPath));
                        var items = serializer.Deserialize(rd) as TargetMatrixItem[];
                        rd.Close();
                        if (items != null)
                        {
                            var item = items.SingleOrDefault(i => i.Name == name);
                            if (item != null)
                            {
                                serializer = ItemSerializer;
                                var builder = new System.Text.StringBuilder();
                                System.IO.TextWriter wr = new System.IO.StringWriter(builder);
                                serializer.Serialize(wr, item);
                                rd.Close();
                                xml = builder.ToString();
                            }
                        }
                    }
                    status = true;
                }
                catch (Exception e)
                {
                    message = e.Message;
                }
            }

            return new ResultStatus(message, status);
        }
        [OperationContract]
        public virtual ResultStatus DeleteTargetMatrixItem(string name)
        {
            bool status = false;
            string message = string.Empty;

            if (IsSessionValid())
            {
                try
                {
                    if (File.Exists(HttpContext.Current.Server.MapPath(XmlPath)))
                    {
                        lock (SyncObject)
                        {
                            var serializer = ItemsSerializer;
                            System.IO.Stream rd = File.OpenRead(HttpContext.Current.Server.MapPath(XmlPath));
                            var items = serializer.Deserialize(rd) as TargetMatrixItem[];
                            rd.Close();
                            if (items != null)
                            {
                                items = items.Where(i => i.Name != name).ToArray();
                                Stream wr = File.Create(HttpContext.Current.Server.MapPath(XmlPath));
                                serializer.Serialize(wr, items);
                                wr.Close();    
                            }
                        }
                    }
                    message = string.Format("Target Matrix \"{0}\" is removed successfully.", name);
                    status = true;
                }
                catch (Exception e)
                {
                    message = e.Message;
                }
            }

            return new ResultStatus(message, status);
        }

        [OperationContract]
        public virtual ResultStatus SaveTargetMatrix(string name, string xml)
        {
            bool status = false;
            string message = string.Empty;

            if (IsSessionValid())
            {
                try
                {
                    TargetMatrixItem[] items = null;
                    var serializer = ItemSerializer;
                    System.IO.TextReader trd = new System.IO.StringReader(xml);
                    var item = serializer.Deserialize(trd) as TargetMatrixItem;
                    trd.Close();

                    if (item != null)
                    {
                        lock (SyncObject)
                        {
                            if (File.Exists(HttpContext.Current.Server.MapPath(XmlPath)))
                            {
                                serializer = ItemsSerializer;
                                System.IO.Stream rd = File.OpenRead(HttpContext.Current.Server.MapPath(XmlPath));
                                items = serializer.Deserialize(rd) as TargetMatrixItem[];
                                rd.Close();
                            }
                            if ((items ?? new TargetMatrixItem[0]).SingleOrDefault(i => i.Name == item.Name) != null && string.IsNullOrEmpty(name))
                                throw new Exception(string.Format("Target Matrix \"{0}\" already exists.", item.Name));
                            items = new[] { item }.Union((items ?? new TargetMatrixItem[0]).Where(i => i.Name != name)).ToArray();
                            serializer = ItemsSerializer;
                            Stream wr = File.Create(HttpContext.Current.Server.MapPath(XmlPath));
                            serializer.Serialize(wr, items);
                            wr.Close();
                        }
                        message = "Target Matrix has been saved successfully.";
                        status = true;    
                    }
                    
                }
                catch (Exception e)
                {
                    message = e.Message;
                }
            }

            return new ResultStatus(message, status);
        }

        [OperationContract]
        public virtual ResultStatus LoadFromExcel(byte[] excel, out string xml)
        {
            bool status = false;
            string message = string.Empty;
            xml = null;
            if (IsSessionValid())
            {
                var str = new MemoryStream(excel);
                var matrixItem = new TargetMatrixItem();
                using (var document = SpreadsheetDocument.Open(str, false))
                {
                    var workbookPart = document.WorkbookPart;
                    var workbook = workbookPart.Workbook;
                    var sheets = workbook.Descendants<Sheet>();
                    foreach (var sheet in sheets)
                    {
                        var worksheetPart = (WorksheetPart)workbookPart.GetPartById(sheet.Id);
                        var sharedStringPart = workbookPart.SharedStringTablePart;
                        var values = sharedStringPart.SharedStringTable.Elements<SharedStringItem>().ToArray();
                        var cells = worksheetPart.Worksheet.Descendants<Cell>().ToList();
                        if (cells.Count > 0)
                        {
                            int headerRow = GetCellRow(cells[0].CellReference);
                            var matrixRows = new List<TargetMatrixRow>();
                            var matrixColumnsSet = new Dictionary<string, string>();
                            int prevRow = 0;
                            double target = 0;
                            string targetColumnName = string.Empty;
                            var matrixCells = new List<TargetMatrixCell>();
                            foreach (var cell in cells)
                            {
                                int row = GetCellRow(cell.CellReference);
                                string column = GetCellColumn(cell.CellReference);
                                bool rowChanged = row != prevRow;
                                string value = GetCellValue(cell, values);
                                if (value == null)
                                    continue;
                                if (row == headerRow)
                                {
                                    if (!matrixColumnsSet.ContainsKey(value))
                                    {
                                        if (string.Equals(value, "Target", StringComparison.CurrentCultureIgnoreCase))
                                            targetColumnName = column;
                                        matrixColumnsSet.Add(column, value);
                                    }
                                    prevRow = row;
                                }
                                else
                                {
                                    if (matrixColumnsSet.ContainsKey(column))
                                    {
                                        if (rowChanged)
                                        {
                                            if (matrixCells.Count > 0)
                                            {
                                                var newRow = new TargetMatrixRow { Cells = matrixCells.ToArray(), TargetValue = target };
                                                matrixRows.Add(newRow);
                                                matrixCells.Clear();
                                            }
                                            prevRow = row;
                                        }
                                        if (column == targetColumnName)
                                            double.TryParse(value, out target);
                                        else
                                            matrixCells.Add(new TargetMatrixCell { Name = matrixColumnsSet[column], Value = value });
                                    }
                                }
                            }
                            if (matrixCells.Count > 0)
                            {
                                var newRow = new TargetMatrixRow { Cells = matrixCells.ToArray(), TargetValue = target };
                                matrixRows.Add(newRow);
                                matrixCells.Clear();
                            }
                            matrixItem.Columns = matrixColumnsSet.Keys.Where(key => key != targetColumnName).Select(key => new TargetMatrixColumn { Caption = matrixColumnsSet[key], Name = matrixColumnsSet[key] }).ToArray();
                            matrixItem.Rows = matrixRows.ToArray();
                        }
                    }
                }
                if (matrixItem.Columns != null && matrixItem.Columns.Length > 0)
                {
                    var serializer = ItemSerializer;
                    var builder = new System.Text.StringBuilder();
                    using (var wr = new StringWriter(builder))
                    {
                        serializer.Serialize(wr, matrixItem);
                        xml = builder.ToString();    
                    }
                    status = true;
                }
            }
            return new ResultStatus(message, status);
        }

        protected static string GetCellValue(CellType cell, SharedStringItem[] values)
        {
            if (cell.DataType != null && cell.DataType.Value == CellValues.SharedString)
            {
                var index = int.Parse(cell.CellValue.Text);
                var value = values[index].InnerText;
                return value;
            }
            return cell.CellValue != null ? cell.CellValue.Text : null;
        }

        protected static int GetCellRow(string cellReference)
        {
            return int.Parse(cellReference.Replace(GetCellColumn(cellReference), string.Empty));
        }

        protected static string GetCellColumn(string cellReference)
        {
            return Regex.Replace(cellReference, @"\d", "");
        }
        
        public static System.Xml.Serialization.XmlSerializer ItemsSerializer
        {
            get
            {
                return _itemsSerializer ?? (_itemsSerializer = new System.Xml.Serialization.XmlSerializer(typeof(TargetMatrixItem[])));
            }
        }
        public static System.Xml.Serialization.XmlSerializer ItemSerializer
        {
            get
            {
                return _itemSerializer ?? (_itemSerializer = new System.Xml.Serialization.XmlSerializer(typeof(TargetMatrixItem)));
            }
        }

        private static System.Xml.Serialization.XmlSerializer _itemsSerializer;
        private static System.Xml.Serialization.XmlSerializer _itemSerializer;
        private const string XmlPath = "TargetMatrix.xml";
    }    
}
