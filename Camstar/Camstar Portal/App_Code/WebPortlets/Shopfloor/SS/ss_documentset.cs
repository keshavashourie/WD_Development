/* Copyright 2023 Siemens */
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
using SEMI.AppCode;

/// <summary>
/// Summary description for ES_DocumentSetPopupVP
/// </summary>
namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class SS_DocumentSet : MatrixWebPart
    {
        protected CWC.TextBox _txtDocumentSet { get { return Page.FindCamstarControl("DocSet_DocumentSet") as CWC.TextBox; } }
        protected CWC.TextBox _txtContainer { get { return Page.FindCamstarControl("DocSet_Container") as CWC.TextBox; } }
        protected CWC.NamedObject _ndoEquipment { get { return Page.FindCamstarControl("DocSet_Equipment") as CWC.NamedObject; } }
        protected CWC.ViewDocumentsControl _DocumentSetViewer { get { return Page.FindCamstarControl("DocSet_Viewer") as CWC.ViewDocumentsControl; } }
        protected JQDataGrid _gridDocuments { get { return Page.FindCamstarControl("GUIUtility_Documents") as JQDataGrid; } }

        //-----------------------------------------
        //
        //-----------------------------------------
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // add the data contract member        
            if (Page.DataContract.GetValueByName("DocumentSetDM") != null)
                _txtDocumentSet.Data = Page.DataContract.GetValueByName("DocumentSetDM").ToString();

            if (Page.DataContract.GetValueByName("DocSetEquipmentDM") != null)
                _ndoEquipment.Data = Page.DataContract.GetValueByName("DocSetEquipmentDM");

            if (Page.DataContract.GetValueByName("DocSetContainerDM") != null)
                _txtContainer.Data = Page.DataContract.GetValueByName("DocSetContainerDM").ToString();


            if (_txtDocumentSet.Data != null)
            {
                _txtDocumentSet.Hidden = false;
                _txtContainer.Hidden = true;
                _ndoEquipment.Hidden = true;

                _txtDocumentSet.Visible = true;
                _txtContainer.Visible = false;
                _ndoEquipment.Visible = false;
            }
            else 
            {
                _txtDocumentSet.Hidden = true;
                _txtContainer.Hidden = false;
                _ndoEquipment.Hidden = false;

                _txtDocumentSet.Visible = false;
                _txtContainer.Visible = true;
                _ndoEquipment.Visible = true;
            }

            if (_txtDocumentSet.Data != null || _txtContainer.Data != null)
                FetchData();
        }

        public void ManualLoad()
        {
            FetchData();
        }

        //-----------------------------------------
        //
        //-----------------------------------------
        private void FetchData()
        {
            try
            {
                var fs = FrameworkManagerUtil.GetFrameworkSession();
                GUIUtilityService oService = new GUIUtilityService(fs.CurrentUserProfile);
                GUIUtility oServiceData = new GUIUtility();
                GUIUtility_Info oServiceInfo = new GUIUtility_Info();
                GUIUtility_Request oRequest = new GUIUtility_Request();
                GUIUtility_Result oResult = new GUIUtility_Result();
                ResultStatus oResultStatus = new ResultStatus();    
                            
                if ((_txtDocumentSet.Data == null))
                {
                    //  Prepare the request
                    oServiceData.Container = new ContainerRef(_txtContainer.Data.ToString());
                    if (_ndoEquipment.Data != null)
                        oServiceData.Equipment = new NamedObjectRef(_ndoEquipment.Data.ToString());
                        
                    oServiceInfo.scsDocumentSets = new DocumentSetDetails_Info();
                    oServiceInfo.scsDocumentSets.Document = FieldInfoUtil.RequestValue();
                    oServiceInfo.scsDocumentSets.DocumentSet = FieldInfoUtil.RequestValue();
                    oServiceInfo.scsDocumentSets.SourceObjectType = FieldInfoUtil.RequestValue();
                    oServiceInfo.scsDocumentSets.SourceObjectName = FieldInfoUtil.RequestValue();
                    oServiceInfo.scsDocumentSets.SourceObjectRevision = FieldInfoUtil.RequestValue();
                    oServiceInfo.scsDocumentSets.Identifier = FieldInfoUtil.RequestValue();
                    oServiceInfo.scsDocumentSets.DocumentBrowseMode = FieldInfoUtil.RequestValue();


                    oRequest.Info = oServiceInfo;

                    oResultStatus = oService.GetEnvironment(oServiceData, oRequest, out oResult);

                    if (oResultStatus.IsSuccess)
                    {
                        if (oResult.Value.scsDocumentSets != null)
                        {
                            DocumentEntry[] objDocuments = new DocumentEntry[oResult.Value.scsDocumentSets.Length];
                            int i = 0;
                            foreach (DocumentSetDetails oDoc in oResult.Value.scsDocumentSets)
                            {
                                objDocuments[i] = new DocumentEntry();
                                objDocuments[i].Document = oDoc.Document;
                                objDocuments[i].DocumentIdentifier = oDoc.Identifier;
                                objDocuments[i].Name = oDoc.Document.Name;
                                objDocuments[i].DisplayName = oDoc.Document.Name;
                                objDocuments[i].DocumentBrowseMode = (int)((BrowseModeEnum)oDoc.DocumentBrowseMode);
                                i++;
                            }
                            DocumentSet objDocSet = new DocumentSet();
                            objDocSet.DocumentEntries = objDocuments;
                            _DocumentSetViewer.Data = objDocSet;

                            //bind to the regular grid
                            _gridDocuments.Data = oResult.Value.scsDocumentSets;
                            CamstarWebControl.SetRenderToClient(_gridDocuments);

                            ////oRow.Cells.FromKey("Document").Value = oDoc.Document.__name;
                            ////oRow.Cells.FromKey("DocumentSet").Value = oDoc.DocumentSet.__name;
                            ////oRow.Cells.FromKey("SourceObjectType").Value = oDoc.SourceObjectType;
                            ////oRow.Cells.FromKey("SourceObjectName").Value = oDoc.SourceObjectName;
                            ////oRow.Cells.FromKey("SourceObjectRevision").Value = oDoc.SourceObjectRevision;
                            ////oRow.Cells.FromKey("Identifier").Value = oDoc.Identifier;
                            ////oRow.Cells.FromKey("Identifier").TargetURL = ("@" + oDoc.Identifier);  
                        }
                    }
                    else
                    {
                        DisplayMessage(oResultStatus);
                    }                              
                }
                else
                {
                    if (_txtDocumentSet.Data != null)
                        LoadDocumentSet();
                    //////  Set some controls
                    ////DocumentSetField.TextEditControl.Text = oWebData.DocumentSet;
                    ////ContainerField.Visible = false;
                    ////EquipmentField.Visible = false;
                    ////DocumentSetsField.Columns.FromKey("SourceObjectType").Hidden = true;
                    ////DocumentSetsField.Columns.FromKey("SourceObjectName").Hidden = true;
                    ////DocumentSetsField.Columns.FromKey("SourceObjectRevision").Hidden = true;
                    ////string sSQL;
                    ////QueryResult oQueryResult = new QueryResult();
                    ////QueryOption oQueryOption = new QueryOption();
                    ////oQueryOption.StartRow = 1;
                    ////oQueryOption.RowSetSize = 100;
                    ////sSQL = ("SELECT " + ("DB.DocumentName " + (", CASE WHEN DE.DocumentId = D.DocumentId THEN 1 ELSE 0 END ISROR " + (", D.Identifier " + ("FROM " + ("DocumentSet DS " + ("INNER JOIN DocumentEntry DE ON DS.DocumentSetId = DE.DocumentSetId " + ("INNER JOIN Document D ON (DE.DocumentId = D.DocumentId OR DE.DocumentBaseId = D.DocumentBaseId) " + ("INNER JOIN DocumentBase DB ON D.DocumentBaseId = DB.DocumentBaseId " + ("WHERE " + ("DS.DocumentSetName = \'"
                    ////            + (oWebData.DocumentSet + "\'"))))))))))));
                    ////if (Query.WSQuery.ExecuteAdHocQuery(sSQL, oQueryOption, oQueryResult, oResultStatus))
                    ////{
                    ////    if (!(oQueryResult.Data == null))
                    ////    {
                    ////        if ((oQueryResult.Data.Tables.Count > 0))
                    ////        {
                    ////            foreach (DataRow oDataRow in oQueryResult.Data.Tables[0].Rows)
                    ////            {
                    ////                DocumentSetsField.Rows.Add();
                    ////                oRow = DocumentSetsField.Rows[(DocumentSetsField.Rows.Count - 1)];
                    ////                if ((oDataRow.Item[1] != "0"))
                    ////                {
                    ////                    oRow.Cells.FromKey("Document").Value = (oDataRow.Item[0] + (":" + oDataRow.Item[1]));
                    ////                }
                    ////                else
                    ////                {
                    ////                    oRow.Cells.FromKey("Document").Value = oDataRow.Item[0];
                    ////                }
                    ////                oRow.Cells.FromKey("DocumentSet").Value = oWebData.DocumentSet;
                    ////                oRow.Cells.FromKey("Identifier").Value = oDataRow.Item[2];
                    ////                oRow.Cells.FromKey("Identifier").TargetURL = ("@" + oDataRow.Item[2]);
                    ////                DocumentSetsField.GridRows.Add(oRow);
                    ////            }
                    ////        }
                    ////    }
                    ////}
                    ////else
                    ////{
                    ////    throw new Exception(oResultStatus.Message);
                    ////}
                }
            }
            catch (Exception Ex)
            {
                DisplayMessage(new ResultStatus((Ex.TargetSite.Name + ("(): " + Ex.Message)), false));
            }
        }

        //-----------------------------------------
        //
        //-----------------------------------------
        private void LoadDocumentSet()
        {
            UserProfile profile = HttpContext.Current.Session[Camstar.WebPortal.Constants.SessionConstants.UserProfile] as UserProfile;
            DocumentSetMaintService objService = new DocumentSetMaintService(profile);

            DocumentSetMaint objServiceData = new DocumentSetMaint();
            DocumentSetMaint_Info objServiceInfo = new DocumentSetMaint_Info();
            DocumentSetChanges_Info objChangesInfo = new DocumentSetChanges_Info();
            DocumentSetMaint_Result result = null;

            OM.ResultStatus resultStatus = null;

            objServiceData.ObjectToChange = new NamedObjectRef(_txtDocumentSet.Data.ToString());
            objChangesInfo.DocumentEntries = new DocumentEntryChanges_Info
            {
                Document = FieldInfoUtil.RequestValue()
            };

            objServiceInfo.ObjectChanges = objChangesInfo;
            resultStatus = objService.Load(objServiceData, new DocumentSetMaint_Request { Info = objServiceInfo }, out result);

            if (resultStatus.IsSuccess)
            {
                DocumentSet objDocSet = new DocumentSet();
                int intDocCount = 0;
                intDocCount = result.Value.ObjectChanges.DocumentEntries.Length;
                DocumentEntry[] objDocuments = new DocumentEntry[intDocCount];

                for (int i = 0; i < intDocCount; i++)
                {
                    DocumentMaintService objDocService = new DocumentMaintService(profile);
                    DocumentMaint objDocServiceData = new DocumentMaint();
                    DocumentMaint_Info objDocServiceInfo = new DocumentMaint_Info();
                    DocumentChanges_Info objDocChangesInfo = new DocumentChanges_Info();
                    DocumentMaint_Result resultDoc = null;

                    objDocServiceData.ObjectToChange = result.Value.ObjectChanges.DocumentEntries[i].Document;
                    objDocChangesInfo = new DocumentChanges_Info
                    {
                        Name = FieldInfoUtil.RequestValue(),
                        Identifier = FieldInfoUtil.RequestValue(),
                        BrowseMode = FieldInfoUtil.RequestValue(),
                    };

                    objDocServiceInfo.ObjectChanges = objDocChangesInfo;
                    resultStatus = objDocService.Load(objDocServiceData, new DocumentMaint_Request { Info = objDocServiceInfo }, out resultDoc);

                    if (resultStatus.IsSuccess)
                    {
                        objDocuments[i] = new DocumentEntry();
                        objDocuments[i].Document = result.Value.ObjectChanges.DocumentEntries[i].Document;
                        objDocuments[i].DocumentIdentifier = resultDoc.Value.ObjectChanges.Identifier;
                        objDocuments[i].DocumentBrowseMode = (int)((BrowseModeEnum)resultDoc.Value.ObjectChanges.BrowseMode);
                        objDocuments[i].Name = resultDoc.Value.ObjectChanges.Name;
                        objDocuments[i].DisplayName = resultDoc.Value.ObjectChanges.Name;
                    }
                }

                objDocSet.DocumentEntries = objDocuments;
                _DocumentSetViewer.Data = objDocSet;
            }
            else
            {
                _DocumentSetViewer.Data = null;
            }

        }
    }
}



