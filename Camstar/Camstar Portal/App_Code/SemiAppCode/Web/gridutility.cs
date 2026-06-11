/* Copyright 2022 Siemens */
using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Data;
using System.Text;
using System.Web;
using System.Web.UI;

using System.Reflection;
using System.Reflection.Emit;

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
using PERS = Camstar.WebPortal.Personalization;
using Camstar.WebPortal.PortalFramework;

using System.Dynamic;

/// <summary>
/// Summary description for GridUtility
/// </summary>

namespace SEMI.AppCode
{
    public class GridUtility
    {
        //-----------------------------------------
        //
        //-----------------------------------------
        public static void SelectionValuesGrid_ClearColumns(ref JQDataGrid TargetGrid)
        {
            try
            {
                // if the grid has headers from a previous query, need to clear the headers
                if (TargetGrid.BoundContext.Fields.Count > 0)
                {
                    JQFieldCollection objFieldClear = new JQFieldCollection();
                    TargetGrid.BoundContext.Fields = objFieldClear;
                }
            }
            catch (Exception ex)
            { }
        } // SelectionValuesGrid_ClearColumns

        //-----------------------------------------
        //
        //-----------------------------------------
        public static void SelectionValuesGrid_AddDataRow(ref JQDataGrid TargetGrid, RecordSet RecordSetData)
        {
            try
            {
                //Add the lot details to grid.
                DataTable dtGridTable = (TargetGrid.GridContext as BoundContext).Data as DataTable;
                if (dtGridTable == null)
                {
                    TargetGrid.SetSelectionValues(RecordSetData);
                }
                else
                {
                    DataRow drGridRow = dtGridTable.NewRow();

                    for (int x = 0; x <= RecordSetData.Headers.Length - 1; x++)
                    {
                        drGridRow.SetField(RecordSetData.Headers[x].Name, RecordSetData.Rows[0].Values[x]);
                    }
                    dtGridTable.Rows.Add(drGridRow);
                    TargetGrid.Data = dtGridTable;

                    //Hide the extra '__STYLE' column for Lot Grid query
                    if ((TargetGrid.GridContext as BoundContext).Fields["__STYLE"] != null)
                        (TargetGrid.GridContext as BoundContext).Fields["__STYLE"].Visible = false;
                }
                CamstarWebControl.SetRenderToClient(TargetGrid);
            }
            catch (Exception ex)
            { }
        } // SelectionValuesGrid_AddDataRow

        //-----------------------------------------
        //
        //-----------------------------------------
        public static void SelectionValuesGrid_SetColumns(ref JQDataGrid TargetGrid, Header[] HeaderValues)
        {
            try
            {
                // add the column headers of the results
                foreach (OM.Header objHeader in HeaderValues)
                {
                    DataColumn dc = new DataColumn(objHeader.Name);
                    TargetGrid.AddField(dc);
                }
                //Hide the extra '__STYLE' column for Lot Grid query
                if ((TargetGrid.GridContext as BoundContext).Fields["__STYLE"] != null)
                    (TargetGrid.GridContext as BoundContext).Fields["__STYLE"].Visible = false;
            }
            catch (Exception ex)
            { }
        } // SelectionValuesGrid_SetColumns               

        //-----------------------------------------
        //
        //-----------------------------------------
        public static void ItemListGrid_SetColumns(Camstar.WebPortal.WebPortlets.MatrixWebPart RefPage, DataTable GridData, string GridId, 
            string[] TextBoxColumnNames = null, string TypeNameID = "RefTargetGrid", bool AllowColumnSort = false, 
            string[] HiddenColumnNames = null, bool ExecuteLoadPersonalisation = true, 
            string[] SpecificWidthColumnNames = null, Camstar.WCF.ObjectStack.Header[] HeaderData = null, string[] CheckBoxColumnNames = null)
        {
            try
            {
                Type _dynamicItemType = null;

                if (TypeNameID == "RefTargetGrid")
                    TypeNameID = "__" + GridId;

                //foreach (var varControl in (RefPage.Page as WebPartPageBase).Model.Personalizations.OfType<PERS.PageContent>().Where(c => c.DynamicWebParts != null))
                //{
                //    var varWebPart = varControl.DynamicWebParts.FirstOrDefault(w => w.Name == RefPage.ID);
                var varWebPart = (RefPage.Page.Model.PublishedContent as PageContent).DynamicWebParts.FirstOrDefault(w => w.Name == RefPage.ID);

                    if (varWebPart != null)
                    {
                        var varGridControl = varWebPart.Control.FirstOrDefault(ct => ct.Name == GridId);
                        if (varGridControl != null)
                        {
                            var varGridSettings = varGridControl.Items.FirstOrDefault(i => i.Value is PERS.GridDataSettingsItemList);                            
                            if (varGridSettings != null)
                            {
                                if (GridData != null)
                                {

                                    // translate the headerData to a hashtable for quick searching
                                    Hashtable htHeaderLabels = new Hashtable();
                                    if (HeaderData != null)
                                    {
                                        htHeaderLabels = new Hashtable();
                                        foreach (Header hd in HeaderData)
                                        {
                                            if (!htHeaderLabels.ContainsKey(hd.Name))
                                                htHeaderLabels.Add(hd.Name, hd.Label);
                                        }
                                    }

                                    _dynamicItemType = CreateDynamicType(GridData, TypeNameID);
                                    if (TextBoxColumnNames == null && HiddenColumnNames == null && CheckBoxColumnNames == null)
                                        (varGridSettings.Value as PERS.GridDataSettingsItemList).Columns =
                                            (from d in GridData.Columns.OfType<DataColumn>()
                                             select new PERS.JQFieldData() { Name = d.ColumnName, Editable = false, LabelText = GetLabel(ref htHeaderLabels, d.ColumnName.ToString()).Value, Resizable = true, Sortable = AllowColumnSort, LabelName = GetLabel(ref htHeaderLabels, d.ColumnName.ToString()).Name }).ToArray<PERS.JQFieldBase>();
                                    else
                                    {
                                        // translate the TextBoxColumnNames into a hashtable for quick searching
                                        Hashtable htTextBoxColumns = new Hashtable();
                                        if (TextBoxColumnNames != null)
                                        {
                                            foreach (string sColumnName in TextBoxColumnNames)
                                                if (!htTextBoxColumns.ContainsKey(sColumnName.ToString()))
                                                    htTextBoxColumns.Add(sColumnName.ToString(), sColumnName.ToString());
                                        }

                                        // translate the HiddenColumnName into a hashtable for quick searching
                                        Hashtable htHiddenColumns = new Hashtable();
                                        if (HiddenColumnNames != null)
                                        {
                                            foreach (string sColumnName in HiddenColumnNames)
                                                if (!htHiddenColumns.ContainsKey(sColumnName.ToString()))
                                                    htHiddenColumns.Add(sColumnName.ToString(), sColumnName.ToString());
                                        }

                                        // translate the SpecificWidthColumnNames into a hashtable
                                        Hashtable htSpecificWidthColumns = new Hashtable();
                                        if (SpecificWidthColumnNames != null)
                                        {
                                            foreach (string sColumn in SpecificWidthColumnNames)
                                            {
                                                string[] sItem = sColumn.Split('|');
                                                if (!htSpecificWidthColumns.ContainsKey(sItem[0].ToString()))
                                                    htSpecificWidthColumns.Add(sItem[0].ToString(), sItem[1].ToString());
                                            }
                                        }

                                        // translate the CheckBoxColumnNames into a hashtable
                                        Hashtable htCheckBoxColumns = new Hashtable();
                                        if (CheckBoxColumnNames != null)
                                        {
                                            foreach (string sColumnName in CheckBoxColumnNames)
                                                if (!htCheckBoxColumns.ContainsKey(sColumnName.ToString()))
                                                    htCheckBoxColumns.Add(sColumnName.ToString(), sColumnName.ToString());
                                        }

                                        List<JQFieldBase> oFieldBaseList = new List<JQFieldBase>();
                                        foreach (DataColumn d in GridData.Columns)
                                        {
                                            bool bHidden = false;
                                            bHidden = (htHiddenColumns.ContainsKey(d.ColumnName.ToString()));
                                            int iColumnWidth = 150;

                                            if (htSpecificWidthColumns.ContainsKey(d.ColumnName.ToString()))
                                                iColumnWidth = int.Parse(htSpecificWidthColumns[d.ColumnName.ToString()].ToString());

                                            if (htTextBoxColumns.ContainsKey(d.ColumnName.ToString()))
                                            {
                                                // add as a textbox
                                                PERS.JQTextBox oText = new PERS.JQTextBox();
                                                oText.Name = d.ColumnName.ToString();
                                                oText.BindPath = d.ColumnName.ToString();
                                                oText.TypeValidation = false;                                                
                                                oText.Editable = true;
                                                oText.LabelText = GetLabel(ref htHeaderLabels, d.ColumnName.ToString()).Value;
                                                oText.LabelName = GetLabel(ref htHeaderLabels, d.ColumnName.ToString()).Name;
                                                oText.Sortable = AllowColumnSort;
                                                oText.Hidden = bHidden;
                                                oText.Width = iColumnWidth;                                                
                                                oFieldBaseList.Add(oText as PERS.JQFieldBase);
                                            }
                                            else if (htCheckBoxColumns.ContainsKey(d.ColumnName.ToString()))
                                            {
                                                // add as a checkbox
                                                PERS.JQFieldCheckBox oText = new PERS.JQFieldCheckBox();
                                                oText.Name = d.ColumnName.ToString();
                                                oText.BindPath = d.ColumnName.ToString();
                                                oText.TypeValidation = false;
                                                oText.Editable = true;
                                                oText.LabelText = GetLabel(ref htHeaderLabels, d.ColumnName.ToString()).Value;
                                                oText.LabelName = GetLabel(ref htHeaderLabels, d.ColumnName.ToString()).Name;
                                                oText.Sortable = AllowColumnSort;
                                                oText.Hidden = bHidden;
                                                oText.Width = iColumnWidth;
                                                oFieldBaseList.Add(oText as PERS.JQFieldBase);
                                            }
                                            else
                                            {
                                                // add as regular field
                                                PERS.JQFieldData oField = new JQFieldData();
                                                oField.Name = d.ColumnName.ToString();
                                                oField.BindPath = d.ColumnName.ToString();
                                                oField.TypeValidation = false;
                                                oField.Editable = false;
                                                oField.LabelText = GetLabel(ref htHeaderLabels, d.ColumnName.ToString()).Value;
                                                oField.LabelName = GetLabel(ref htHeaderLabels, d.ColumnName.ToString()).Name;
                                                oField.Sortable = AllowColumnSort;
                                                oField.Hidden = bHidden;
                                                oField.Width = iColumnWidth;
                                                oFieldBaseList.Add(oField as JQFieldBase);
                                            if (d.ColumnName.ToString().Equals("Timer"))
                                            {
                                                oField.CustomScriptHandlers = new JQGridCustomScript[1] { new JQGridCustomScript() { Handler="function(p1){return 'jqgrid-duration-autoupdate';}", Name= "onAddCellAttributes" } };
                                            }
                                            }
                                        }

                                        (varGridSettings.Value as PERS.GridDataSettingsItemList).Columns = oFieldBaseList.ToArray();
                                    }
                                }
                                else
                                {
                                    (varGridSettings.Value as PERS.GridDataSettingsItemList).Columns = new PERS.JQFieldBase[0];
                                }
                            }

                            var varLocalSession = RefPage.Page.PortalContext.LocalSession;
                            if (varLocalSession != null)
                            {
                                var varContext = varLocalSession["WebPart_" + RefPage.ID + "~" + GridId.ToString()] as BoundContext;
                                if (varContext != null)
                                {
                                    varContext.Fields.Clear();
                                    varContext.ItemType = _dynamicItemType;
                                    //varContext.DataWindow = null;
                                }                              
                            }
                        }
                    }
                //} // foreach

                if (ExecuteLoadPersonalisation)
                    RefPage.LoadPersonalization();
            }
            catch (Exception ex)
            { }
        } // ItemListGrid_SetColumns         

        //-----------------------------------------
        //
        //-----------------------------------------
        private static OM.Label GetLabel(ref Hashtable HeaderData, string ID)
        {
            OM.Label lblReturn = new OM.Label();
            
            lblReturn.Value = ID;
            lblReturn.Name = null;        

            if (HeaderData != null)
                if (HeaderData.ContainsKey(ID))
                    lblReturn = HeaderData[ID] as OM.Label;

            if (lblReturn == null)
            {
                lblReturn = new OM.Label();                
                lblReturn.Value = ID;
                lblReturn.Name = null;
            }

            return lblReturn;
        }

        //-----------------------------------------
        //
        //-----------------------------------------
        public static void ItemListGrid_BindDataTable(Camstar.WebPortal.WebPortlets.MatrixWebPart RefPage, DataTable GridData, ref JQDataGrid TargetGrid, string TypeNameID = "RefTargetGrid")
        {
            try
            {
                if (TypeNameID == "RefTargetGrid")
                    TypeNameID = "__" + TargetGrid.ID;

                var varItemDataContext = (TargetGrid.GridContext as ItemDataContext);
                Type _dynamicType = CreateDynamicType(GridData, TypeNameID);
                Array arData = new WSDataCreator().CreateArrayObject(_dynamicType, GridData.Rows.Count) as Array;

                var varRow = new GenericGridRow[GridData.Rows.Count];
                int intRowIndex = 0;
                foreach (DataRow r in GridData.Rows)
                {
                    var ob = arData.GetValue(intRowIndex);
                    //var wcfObj = new WCFObject(ob);
                    var properties = _dynamicType.GetProperties();

                    varRow[intRowIndex] = new GenericGridRow();
                    foreach (var c in GridData.Columns.OfType<DataColumn>())
                    {
                        var pp = properties.FirstOrDefault(p => p.Name == c.ColumnName);
                        if (pp != null)
                        {
                            
                            var v = GridData.Rows[intRowIndex][c];
                            
                            if (v is System.DBNull)
                                pp.SetValue(ob, string.Empty, null);
                            else if (v is System.Int32)
                                pp.SetValue(ob, v.ToString(), null);
                            else if (v is System.DateTime)
                                pp.SetValue(ob, v.ToString(), null);
                            else if (v is System.Double)
                                pp.SetValue(ob, v.ToString(), null);
                            else if (v is bool)
                                pp.SetValue(ob, v.ToString(), null);
                            else
                                pp.SetValue(ob, v, null);
                            
                        }
                    }
                    intRowIndex++;
                }
                TargetGrid.Data = arData;

                //Hide the extra '__STYLE' column for Lot Grid query
                if ((TargetGrid.GridContext as BoundContext).Fields["__STYLE"] != null)
                    (TargetGrid.GridContext as BoundContext).Fields["__STYLE"].Visible = false; 

                RefPage.RenderToClient = true;               
            }
            catch (Exception ex)
            { }
        } // ItemListGrid_BindDataTable

        //-----------------------------------------
        //
        //-----------------------------------------
        public static void ItemListGrid_AddDataRow(Camstar.WebPortal.WebPortlets.MatrixWebPart RefPage, DataTable GridData, ref JQDataGrid TargetGrid, string TypeNameID = "RefTargetGrid")
        {
            try
            {

                if (TypeNameID == "RefTargetGrid")
                    TypeNameID = "__" + TargetGrid.ID;

                var varItemDataContext = (TargetGrid.GridContext as ItemDataContext);                
                if (varItemDataContext.Data != null)
                {
                    Type _dynamicItemType = CreateDynamicType(GridData, TypeNameID);                    
                    Array arData = new WSDataCreator().CreateArrayObject(_dynamicItemType, GridData.Rows.Count + varItemDataContext.GetTotalRows()) as Array;                                        
                    Array arExistingData = TargetGrid.Data as Array;
                    var properties = _dynamicItemType.GetProperties();                      
                    var varRow = new GenericGridRow[GridData.Rows.Count + varItemDataContext.GetTotalRows()];
      
                    System.Array.Copy(arExistingData, arData, varItemDataContext.GetTotalRows());

                    int intArrayRowIndex = varItemDataContext.GetTotalRows();
                    int intRowIndex = 0;
                    foreach (DataRow r in GridData.Rows)
                    {
                        varRow[intArrayRowIndex] = new GenericGridRow();
                        var ob = arData.GetValue(intArrayRowIndex);
                        var abc = GridData.Columns.OfType<DataColumn>();

                        foreach (var c in GridData.Columns.OfType<DataColumn>())
                        {
                            var pp = properties.FirstOrDefault(p => p.Name == c.ColumnName);
                            if (pp != null)
                            {
                                var v = GridData.Rows[intRowIndex][c];
                                if (v is System.DBNull)
                                    pp.SetValue(ob, string.Empty, null);
                                else if (v is System.Int32)
                                    pp.SetValue(ob, v.ToString(), null);
                                else if (v is System.DateTime)
                                    pp.SetValue(ob, v.ToString(), null);
                                else if (v is System.Double)
                                    pp.SetValue(ob, v.ToString(), null);
                                else if (v is bool)
                                    pp.SetValue(ob, v.ToString(), null);
                                else
                                    pp.SetValue(ob, v.ToString(), null);
                            }
                        }
                        intRowIndex++;
                        intArrayRowIndex++;
                    }

                    TargetGrid.Data = arData;

                    //Hide the extra '__STYLE' column for Lot Grid query
                    if ((TargetGrid.GridContext as BoundContext).Fields["__STYLE"] != null)
                        (TargetGrid.GridContext as BoundContext).Fields["__STYLE"].Visible = false;

                    RefPage.RenderToClient = true; 
                }
                else
                    GridUtility.ItemListGrid_BindDataTable(RefPage, GridData, ref TargetGrid, TypeNameID);
            }
            catch (Exception ex)
            { }
        } // ItemListGrid_AddDataRow             

        //-----------------------------------------
        //
        //-----------------------------------------
        private static Type CreateDynamicType(DataTable TypeData, string typeName)
        {
            Type createdType = null;
            // create a dynamic assembly and module 
            var assemblyName = new AssemblyName();
            assemblyName.Name = "CamstarDynamicAssembly_" + typeName;
            var domain = System.Threading.Thread.GetDomain();
            ModuleBuilder module = null;
            AssemblyBuilder assemblyBuilder = null;

            var assem = domain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == assemblyName.Name);
            if (assem == null)
            {
                assemblyBuilder = domain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
                module = assemblyBuilder.DefineDynamicModule("tmpModule");
            }
            else
            {
                createdType = assem.GetType(typeName);
                if (createdType != null)
                {
                    return createdType;
                }
                assemblyBuilder = domain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
                module = assemblyBuilder.DefineDynamicModule("tmpModule");
            }

            // create a new type builder
            TypeBuilder typeBuilder = module.DefineType(typeName, TypeAttributes.Public | TypeAttributes.Class);

            // Loop over the attributes that will be used as the properties names in out new type
            foreach (var col in TypeData.Columns.OfType<DataColumn>())
            {
                string propertyName = col.ColumnName;

                // Generate a private field
                var field = typeBuilder.DefineField("_" + propertyName, typeof(string), FieldAttributes.Private);
                // Generate a public property
                PropertyBuilder property =
                    typeBuilder.DefineProperty(propertyName, System.Reflection.PropertyAttributes.None, typeof(string), new Type[] { typeof(string) });

                // The property set and property get methods require a special set of attributes:
                MethodAttributes GetSetAttr = MethodAttributes.Public | MethodAttributes.HideBySig;

                // Define the "get" accessor method for current private field.
                var currGetPropMthdBldr = typeBuilder.DefineMethod("get_value", GetSetAttr, typeof(string), Type.EmptyTypes);

                // Intermediate Language stuff...
                ILGenerator currGetIL = currGetPropMthdBldr.GetILGenerator();
                currGetIL.Emit(OpCodes.Ldarg_0);
                currGetIL.Emit(OpCodes.Ldfld, field);
                currGetIL.Emit(OpCodes.Ret);

                // Define the "set" accessor method for current private field.
                MethodBuilder currSetPropMthdBldr = typeBuilder.DefineMethod("set_value", GetSetAttr, null, new Type[] { typeof(string) });

                // Again some Intermediate Language stuff...
                ILGenerator currSetIL = currSetPropMthdBldr.GetILGenerator();
                currSetIL.Emit(OpCodes.Ldarg_0);
                currSetIL.Emit(OpCodes.Ldarg_1);
                currSetIL.Emit(OpCodes.Stfld, field);
                currSetIL.Emit(OpCodes.Ret);

                // Last, we must map the two methods created above to our PropertyBuilder to 
                // their corresponding behaviors, "get" and "set" respectively. 
                property.SetGetMethod(currGetPropMthdBldr);
                property.SetSetMethod(currSetPropMthdBldr);
            }

            createdType = typeBuilder.CreateType();

            // Generate our type
            return createdType;
        }

        //-----------------------------------------
        //
        //-----------------------------------------
        public static Type RetrieveDynamicType(string typeName)
        {
            Type createdType = null;
            // create a dynamic assembly and module 
            var assemblyName = new AssemblyName();
            assemblyName.Name = "CamstarDynamicAssembly_" + typeName;
            var domain = System.Threading.Thread.GetDomain();      

            var assem = domain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == assemblyName.Name);
            if (assem == null)
            {
                return null;
            }
            else
            {
                createdType = assem.GetType(typeName);
                if (createdType != null)
                {
                    return createdType;
                }               
            }

            return null;
        }
                
        //-----------------------------------------
        //
        //-----------------------------------------
        public static void WIPData_ItemListGrid_SetColumns(Camstar.WebPortal.WebPortlets.MatrixWebPart RefPage, DataTable GridData, string GridId,
            string[] TextBoxColumnNames = null, string TypeNameID = "RefTargetGrid", bool AllowColumnSort = false,
            string[] HiddenColumnNames = null, bool ExecuteLoadPersonalisation = true,
            string[] SpecificWidthColumnNames = null, string[] RequiredColumnNames = null, bool SpecificFrameLocation = true)
        {
            try
            {
                Type _dynamicItemType = null;

                if (TypeNameID == "RefTargetGrid")
                    TypeNameID = "__" + GridId;

                 //foreach (var varControl in (RefPage.Page as WebPartPageBase).Model.Personalizations.OfType<PERS.PageContent>().Where(c => c.DynamicWebParts != null))
                //{
                //    var varWebPart = varControl.DynamicWebParts.FirstOrDefault(w => w.Name == RefPage.ID);
                var varWebPart = (RefPage.Page.Model.PublishedContent as PageContent).DynamicWebParts.FirstOrDefault(w => w.Name == RefPage.ID);
                    if (varWebPart != null)
                    {
                        var varGridControl = varWebPart.Control.FirstOrDefault(ct => ct.Name == GridId);
                        if (varGridControl != null)
                        {
                            var varGridSettings = varGridControl.Items.FirstOrDefault(i => i.Value is PERS.GridDataSettingsItemList);
                            if (varGridSettings != null)
                            {
                                if (GridData != null)
                                {
                                    _dynamicItemType = CreateDynamicType(GridData, TypeNameID);
                                    if (TextBoxColumnNames == null && HiddenColumnNames == null)
                                        (varGridSettings.Value as PERS.GridDataSettingsItemList).Columns =
                                            (from d in GridData.Columns.OfType<DataColumn>()
                                             select new PERS.JQFieldData() { Name = d.ColumnName, Editable = false, LabelText = d.Caption, Resizable = true, Sortable = AllowColumnSort }).ToArray<PERS.JQFieldBase>();
                                    else
                                    {
                                        // translate the TextBoxColumnNames into a hashtable for quick searching
                                        Hashtable htTextBoxColumns = new Hashtable();
                                        if (TextBoxColumnNames != null)
                                        {
                                            foreach (string sColumnName in TextBoxColumnNames)
                                                if (!htTextBoxColumns.ContainsKey(sColumnName.ToString()))
                                                    htTextBoxColumns.Add(sColumnName.ToString(), sColumnName.ToString());
                                        }

                                        // translate the HiddenColumnName into a hashtable for quick searching
                                        Hashtable htHiddenColumns = new Hashtable();
                                        if (HiddenColumnNames != null)
                                        {
                                            foreach (string sColumnName in HiddenColumnNames)
                                                if (!htHiddenColumns.ContainsKey(sColumnName.ToString()))
                                                    htHiddenColumns.Add(sColumnName.ToString(), sColumnName.ToString());
                                        }

                                        // translate the SpecificWidthColumnNames into a hashtable
                                        Hashtable htSpecificWidthColumns = new Hashtable();
                                        if (SpecificWidthColumnNames != null)
                                        {
                                            foreach (string sColumn in SpecificWidthColumnNames)
                                            {
                                                string[] sItem = sColumn.Split('|');
                                                if (!htSpecificWidthColumns.ContainsKey(sItem[0].ToString()))
                                                    htSpecificWidthColumns.Add(sItem[0].ToString(), sItem[1].ToString());
                                            }
                                        }


                                    // translate the RequiredColumnNames into a hashtable
                                    Hashtable htRequiredColumns = new Hashtable();
                                    if (RequiredColumnNames != null)
                                    {
                                        foreach (string sColumnName in RequiredColumnNames)
                                            if (!htRequiredColumns.ContainsKey(sColumnName.ToString()))
                                                htRequiredColumns.Add(sColumnName.ToString(), sColumnName.ToString());
                                    }

                                    int FrameLocationHeight = 0;
                                    int FrameLocationWidth = 0;
                                    if (SpecificFrameLocation)
                                    {
                                        FrameLocationHeight = 600;
                                        FrameLocationWidth = 510;
                                    }

                                    List<JQFieldBase> oFieldBaseList = new List<JQFieldBase>();
                                    foreach (DataColumn d in GridData.Columns)
                                    {
                                        bool bHidden = false;
                                        bHidden = (htHiddenColumns.ContainsKey(d.ColumnName.ToString()));
                                        int iColumnWidth = 150;

                                        if (htSpecificWidthColumns.ContainsKey(d.ColumnName.ToString()))
                                            iColumnWidth = int.Parse(htSpecificWidthColumns[d.ColumnName.ToString()].ToString());

                                        bool bRequired = false;
                                        bRequired = (htRequiredColumns.ContainsKey(d.ColumnName.ToString()));

                                        if (htTextBoxColumns.ContainsKey(d.ColumnName.ToString()))
                                        {
                                            // add as a textbox
                                            PERS.JQTextBox oText = new PERS.JQTextBox();
                                            oText.Name = d.ColumnName.ToString();
                                            oText.BindPath = d.ColumnName.ToString();
                                            oText.TypeValidation = false;
                                            oText.Editable = true;
                                            oText.LabelText = d.Caption.ToString();
                                            oText.Sortable = AllowColumnSort;
                                            oText.Hidden = bHidden;
                                            oText.Width = iColumnWidth;
                                            oFieldBaseList.Add(oText as PERS.JQFieldBase);
                                            oText.Required = bRequired;
                                        }
                                        else
                                        {
                                            // add as regular field
                                            PERS.JQFieldData oField = new JQFieldData();
                                            oField.Name = d.ColumnName.ToString();
                                            oField.BindPath = d.ColumnName.ToString();
                                            oField.TypeValidation = false;
                                            oField.Editable = false;
                                            oField.LabelText = d.Caption.ToString();
                                            oField.Sortable = AllowColumnSort;
                                            oField.Hidden = bHidden;
                                            oField.Width = iColumnWidth;
                                            oField.Required = bRequired;

                                            if (d.ColumnName.ToString().Substring(0, 4) == "_btn")
                                            {
                                                d.Caption = "&nbsp;";
                                                PERS.RowDataMapItem[] RowDataMap = new PERS.RowDataMapItem[]
                                                    {
                                                            new PERS.RowDataMapItem(){ ColumnName = d.ColumnName.Substring(4), DataMember = "WIPData_WIPDataValueDM" },
                                                            new PERS.RowDataMapItem(){ ColumnName = "_id_column", DataMember = "WIPData_GridRowIdDM" },
                                                    };

                                                PERS.UIComponentDataContractMap DataContractMap = new PERS.UIComponentDataContractMap()
                                                {
                                                    Links = new PERS.UIComponentDataContractLink[] 
                                                        {
                                                            new PERS.UIComponentDataContractLink(){ SourceMember = "WIPData_ContainerDM", TargetMember = "WIPDataValidValues_LotIdDM" },
                                                            new PERS.UIComponentDataContractLink(){ SourceMember = "WIPData_ProcessTypeDM", TargetMember = "WIPDataValidValues_ProcessTypeDM" },
                                                            new PERS.UIComponentDataContractLink(){ SourceMember = "WIPData_ServiceNameDM", TargetMember = "WIPDataValidValues_ServiceNameDM" },
                                                            new PERS.UIComponentDataContractLink(){ SourceMember = "WIPData_StaticKeyByWaferDM", TargetMember = "WIPDataValidValues_KeyDM" },
                                                            new PERS.UIComponentDataContractLink(){ SourceMember = "WIPData_GridRowIdDM", TargetMember = "WIPDataValidValues_GridRowIdDM" },
                                                            new PERS.UIComponentDataContractLink(){ SourceMember = "WIPData_WIPDataNameDM", TargetMember = "WIPDataValidValues_WIPDataNameDM" },
                                                            new PERS.UIComponentDataContractLink(){ SourceMember = "WIPData_WIPDataValueDM", TargetMember = "WIPDataValidValues_WIPDataValueDM" },
                                                            new PERS.UIComponentDataContractLink(){ SourceMember = "WIPData_EquipmentDM", TargetMember = "WIPDataValidValues_EquipmentDM" }
                                                        }
                                                };

                                                PERS.UIComponentDataContractReturnMap DataContractReturnMap = new PERS.UIComponentDataContractReturnMap()
                                                {
                                                    ReturnLinks = new PERS.UIComponentDataContractReturnLink[]
                                                        {
                                                            new PERS.UIComponentDataContractReturnLink(){ SourceMember = "WIPDataValidValues_KeyDM", TargetMember = "WIPData_KeyDM" },
                                                            new PERS.UIComponentDataContractReturnLink(){ SourceMember = "WIPDataValidValues_GridRowIdDM", TargetMember = "WIPData_GridRowIdDM" },
                                                            new PERS.UIComponentDataContractReturnLink(){ SourceMember = "WIPDataValidValues_WIPDataNameDM", TargetMember = "WIPData_WIPDataNameDM" },
                                                            new PERS.UIComponentDataContractReturnLink(){ SourceMember = "WIPDataValidValues_WIPDataValueDM", TargetMember = "WIPData_WIPDataValueDM" }
                                                        }
                                                };

                                                PERS.JQGridCellActionButton[] CellAction = new PERS.JQGridCellActionButton[] 
                                                    { 
                                                        new PERS.JQGridCellActionButton() 
                                                        { 
                                                            ID = d.ColumnName + "CellAction", 
                                                            DefaultAction = new PERS.FloatPageOpenAction()
                                                            {
                                                                Name = d.ColumnName + "DefaultAction",
                                                                PageName = "SS_WIPDataValidValuesPopupVP",
                                                                ESignatureRequired = PERS.BooleanTriState.No,
                                                                SPCEnabled = PERS.BooleanTriState.No,
                                                                WIPMessagesRequired = PERS.BooleanTriState.No,
                                                                ButtonPosition = PERS.ButtonPositionType.LeftPane,
                                                                Location = PERS.ActionLocation.Button,
                                                                EndResponse = true,
                                                                DataContractMap = DataContractMap,
                                                                DataContractReturnMap = DataContractReturnMap,
                                                                FrameLocation = new PERS.UIFloatingPageLocation() { Height = FrameLocationHeight, Width = FrameLocationWidth }
                                                            },
                                                            Image = @"Images\User\SS_WIPDataValidValues.png",
                                                            RowDataContractLinks=RowDataMap
                                                            , Position = PERS.HorizontalAlignment.NotSet
                                                        } 
                                                    };
                                                oField.CellActions = CellAction;
                                                oField.Width = 32;
                                                oField.CellStyle = "ui-jqgrid-column-image";
                                            }

                                            oFieldBaseList.Add(oField as JQFieldBase);                                            
                                        }
                                    }

                                    (varGridSettings.Value as PERS.GridDataSettingsItemList).Columns = oFieldBaseList.ToArray();
                                    }
                                }
                                else
                                {
                                    (varGridSettings.Value as PERS.GridDataSettingsItemList).Columns = new PERS.JQFieldBase[0];
                                }
                            }

                            var varLocalSession = RefPage.Page.PortalContext.LocalSession;
                            if (varLocalSession != null)
                            {
                                var varContext = varLocalSession["WebPart_" + RefPage.ID + "~" + GridId.ToString()] as BoundContext;
                                if (varContext != null)
                                {
                                    varContext.Fields.Clear();
                                    varContext.ItemType = _dynamicItemType;                                    
                                }
                            }
                        }
                    }
               // } // foreach

                if (ExecuteLoadPersonalisation)
                    RefPage.LoadPersonalization();
            }
            catch (Exception ex)
            { }
        } // WIPData_ItemListGrid_SetColumns     


        //-----------------------------------------
        // this logic is solely to detect if the Lot column's ID is fully upper case or mixed case.
        // when running against an Oracle DB, there are occurances that the return column is automaticall set to fully upper case 
        //-----------------------------------------
        public static string GetCasedColumnHeaderName(JQFieldCollection GridFields, string ColumnName)
        {
            string sReturnColumn = ColumnName;                                    
            foreach (JQField oField in GridFields)
            {
                if (oField.ID.ToUpper() == ColumnName.ToUpper())
                {
                    sReturnColumn = oField.ID;
                    break;
                }
            }
            return sReturnColumn;
        } // GetCasedColumnHeaderName
    }

    public class GenericGridRow : Object
    { }
        

}



