// Copyright Siemens 2023  
using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Web;
using System.Data;


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

/// <summary>
/// Summary description for AQLSamplingGrid
/// </summary>
/// 
namespace WebClientPortal
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
            catch (Exception)
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
                }
                CamstarWebControl.SetRenderToClient(TargetGrid);
            }
            catch (Exception)
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
            }
            catch (Exception)
            { }
        } // SelectionValuesGrid_SetColumns

        //-----------------------------------------
        //
        //-----------------------------------------
        public static void ItemListGrid_SetColumns(Camstar.WebPortal.WebPortlets.MatrixWebPart RefPage, DataTable GridData, string GridId, string[] TextBoxColumnNames = null, string TypeNameID = "RefTargetGrid", bool AllowColumnSort = false, string[] HiddenColumnsNames = null, bool ExecuteLoadPersonalisation = true)
        {
            try
            {
                Type _dynamicItemType = null;

                if (TypeNameID == "RefTargetGrid")
                    TypeNameID = "__" + GridId;

                //TODO: Need to test this post- PE conversion
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
                                if (TextBoxColumnNames == null && HiddenColumnsNames == null)
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
                                    if (HiddenColumnsNames != null)
                                    {
                                        foreach (string sColumnName in HiddenColumnsNames)
                                            if (!htHiddenColumns.ContainsKey(sColumnName.ToString()))
                                                htHiddenColumns.Add(sColumnName.ToString(), sColumnName.ToString());
                                    }

                                    List<JQFieldBase> oFieldBaseList = new List<JQFieldBase>();
                                    foreach (DataColumn d in GridData.Columns)
                                    {
                                        bool bHidden = false;
                                        bHidden = (htHiddenColumns.ContainsKey(d.ColumnName.ToString()));

                                        if (htTextBoxColumns.ContainsKey(d.ColumnName.ToString()))
                                        {
                                            // add as a textbox
                                            PERS.JQTextBox oText = new PERS.JQTextBox();
                                            oText.Name = d.ColumnName.ToString();
                                            oText.Editable = true;
                                            oText.LabelText = d.Caption.ToString();
                                            oText.Sortable = AllowColumnSort;
                                            oText.Hidden = bHidden;
                                            oFieldBaseList.Add(oText as PERS.JQFieldBase);
                                        }
                                        else
                                        {
                                            // add as regular field
                                            PERS.JQFieldData oField = new JQFieldData();
                                            oField.Name = d.ColumnName.ToString();
                                            oField.Editable = false;
                                            oField.LabelText = d.Caption.ToString();
                                            oField.Sortable = AllowColumnSort;
                                            oField.Hidden = bHidden;
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
                                //varContext.DataWindow = null;
                            }
                        }
                    }
                }


                if (ExecuteLoadPersonalisation)
                    RefPage.LoadPersonalization();
            }
            catch (Exception)
            { }
        } // ItemListGrid_SetColumns         

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
                _dynamicType = null;
                _dynamicType = CreateDynamicType(GridData, TypeNameID);
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
                RefPage.RenderToClient = true;
            }
            catch (Exception)
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
                    RefPage.RenderToClient = true;
                }
                else
                    GridUtility.ItemListGrid_BindDataTable(RefPage, GridData, ref TargetGrid, TypeNameID);
            }
            catch (Exception)
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
                //createdType = assem.GetType(typeName);
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

        public static Type resetDynamicType(string typeName)
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
                    createdType = null;
                    //AppDomain.Unload(domain);
                }
            }

            return null;
        }
    }

    public class GenericGridRow : Object
    { }
}
