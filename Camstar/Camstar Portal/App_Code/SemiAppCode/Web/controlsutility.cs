/* Copyright 2019 Siemens */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Collections;

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
/// Summary description for ControlUtility
/// </summary>
namespace SEMI.AppCode
{
    public class ControlsUtility
    {
        //-----------------------------------------
        //
        //-----------------------------------------
        public static void NamedObjectControl_SetSelectionValues(ref CWC.NamedObject NamedObjectControl, NamedObjectRef[] NamedObjectRefList)
        {
            try            
            {
                RecordSet rsNamedObject = new RecordSet();
                Header[] rsHeaders = new Header[1];
                Row[] rsRows = new Row[NamedObjectRefList.Length];

                rsHeaders[0] = new Header();
                rsHeaders[0].TypeCode = TypeCode.String;
                rsHeaders[0].Name = "Name";

                rsNamedObject.Headers = rsHeaders;

                for (int x = 0; x <= NamedObjectRefList.Length - 1; x++)
                {
                    rsRows[x] = new Row();
                    string[] strRowValues = new string[1];
                    strRowValues[0] = NamedObjectRefList[x].Name;
                    rsRows[x].Values = strRowValues;
                }

                rsNamedObject.Rows = rsRows;
                NamedObjectControl.SetSelectionValues(rsNamedObject);
            }
            catch
            {}
        } // NamedObjectControl_SetSelectionValues

        //-----------------------------------------
        //
        //-----------------------------------------
        public static void NamedSubentityControl_SetSelectionValues(ref CWC.NamedSubentity NamedSubentityControl, NamedSubentityRef[] NamedSubentityRefList)
        {
            try
            {
                RecordSet rsNamedObject = new RecordSet();
                Header[] rsHeaders = new Header[2];
                Row[] rsRows = new Row[NamedSubentityRefList.Length];

                rsHeaders[0] = new Header();
                rsHeaders[0].TypeCode = TypeCode.String;
                rsHeaders[0].Name = "Name";

                rsHeaders[1] = new Header();
                rsHeaders[1].TypeCode = TypeCode.String;
                rsHeaders[1].Name = "ID";

                rsNamedObject.Headers = rsHeaders;

                for (int x = 0; x <= NamedSubentityRefList.Length - 1; x++)
                {
                    rsRows[x] = new Row();
                    string[] strRowValues = new string[2];
                    strRowValues[0] = NamedSubentityRefList[x].Name;
                    strRowValues[1] = NamedSubentityRefList[x].ID;
                    rsRows[x].Values = strRowValues;
                }

                rsNamedObject.Rows = rsRows;
                NamedSubentityControl.SetSelectionValues(rsNamedObject);
            }
            catch
            { }
        } // NamedSubentityControl_SetSelectionValues

        //-----------------------------------------
        //
        //-----------------------------------------
        public static void DropDownListControl_SetSelectionValues(ref CWC.DropDownList DropDownListControl, NamedObjectRef[] DropDownRefList)
        {
            try
            {
                RecordSet rsNamedObject = new RecordSet();
                Header[] rsHeaders = new Header[2];
                Row[] rsRows = new Row[DropDownRefList.Length];

                rsHeaders[0] = new Header();
                rsHeaders[0].TypeCode = TypeCode.String;
                rsHeaders[0].Name = "Name";

                rsHeaders[1] = new Header();
                rsHeaders[1].TypeCode = TypeCode.String;
                rsHeaders[1].Name = "ID";

                rsNamedObject.Headers = rsHeaders;

                for (int x = 0; x <= DropDownRefList.Length - 1; x++)
                {
                    rsRows[x] = new Row();
                    string[] strRowValues = new string[2];
                    strRowValues[0] = DropDownRefList[x].Name;
                    strRowValues[1] = DropDownRefList[x].ID;
                    rsRows[x].Values = strRowValues;
                }

                rsNamedObject.Rows = rsRows;
                DropDownListControl.SetSelectionValues(rsNamedObject);
            }
            catch
            { }
        } // DropDownListControl_SetSelectionValues

        //-----------------------------------------
        //
        //-----------------------------------------
        public static void RevisionedObjectControl_SetSelectionValues(ref CWC.RevisionedObject RevisionedObjectControl, RevisionedObjectRef[] RevObjectRefList)
        {
            try
            {
                RecordSet rsRevisionedObject = new RecordSet();                                               
                Header[] rsHeaders = new Header[7];
                Row[] rsRows = new Row[RevObjectRefList.Length];               
                                
                rsHeaders[0] = new Header();
                rsHeaders[0].TypeCode = TypeCode.String;
                rsHeaders[0].Name = "Name";

                rsHeaders[1] = new Header();
                rsHeaders[1].TypeCode = TypeCode.String;
                rsHeaders[1].Name = "Revision";

                rsHeaders[2] = new Header();
                rsHeaders[2].TypeCode = TypeCode.String;
                rsHeaders[2].Name = "RevOfRcd";

                rsHeaders[3] = new Header();
                rsHeaders[3].TypeCode = TypeCode.Boolean;
                rsHeaders[3].Name = "IsFrozen";

                rsHeaders[4] = new Header();
                rsHeaders[4].TypeCode = TypeCode.String;
                rsHeaders[4].Name = "InstanceId";

                rsHeaders[5] = new Header();
                rsHeaders[5].TypeCode = TypeCode.String;
                rsHeaders[5].Name = "Description";

                rsHeaders[6] = new Header();
                rsHeaders[6].TypeCode = TypeCode.Int16;
                rsHeaders[6].Name = "Status";

                rsRevisionedObject.Headers = rsHeaders;
                
                // sort the revisioned objects
                Hashtable htNameRev = new Hashtable();            
                Hashtable htNameROR = new Hashtable();
                foreach (RevisionedObjectRef rdoRef in RevObjectRefList)
                {
                    if (!htNameRev.ContainsKey(rdoRef.Name))
                    {
                        List<string> RevArray = new List<string>();
                        RevArray.Add(rdoRef.Revision);
                        htNameRev.Add(rdoRef.Name, RevArray);
                    }
                    else
                    {
                        List<string> RevArray = htNameRev[rdoRef.Name] as List<string>;
                        RevArray.Add(rdoRef.Revision);
                        htNameRev[rdoRef.Name] = RevArray;
                    }

                    // set the ror object
                    if (rdoRef.RevisionOfRecord != null? rdoRef.RevisionOfRecord.Value : false)
                    {
                        if (!htNameROR.ContainsKey(rdoRef.Name))                    
                            htNameROR.Add(rdoRef.Name, rdoRef.Revision);                    
                    }
                }

                int intIndex = 0;
                foreach (DictionaryEntry objItem in htNameRev)
                {
                    List<string> RevArray = objItem.Value as List<string>;
                    foreach (string strRevValue in RevArray)
                    {
                        rsRows[intIndex] = new Row();
                        string[] strRowValue = new string[7];
                        strRowValue[0] = objItem.Key.ToString(); // name
                        strRowValue[1] = strRevValue; // rev
                        
                        string strRevOfRecInstanceId = objItem.Key.ToString() + "_" + strRevValue;
                        if (htNameROR.ContainsKey(objItem.Key.ToString()))
                            strRevOfRecInstanceId = objItem.Key.ToString() + "_" + htNameROR[objItem.Key.ToString()].ToString();

                        strRowValue[2] = strRevOfRecInstanceId; // revOfRcdInstanceId
                        strRowValue[3] = "false"; // IsFrozen
                        strRowValue[4] = objItem.Key.ToString() + "_" + strRevValue; // InstanceId
                        strRowValue[5] = ""; // Description
                        strRowValue[6] = "1"; // Status

                        rsRows[intIndex].Values = strRowValue;

                        intIndex++;
                    }                    
                }

                rsRevisionedObject.Rows = rsRows;
                RevisionedObjectControl.SetSelectionValues(rsRevisionedObject);
            }
            catch
            { }
        } // RevisionedObjectControl_SetSelectionValues


    }
}



