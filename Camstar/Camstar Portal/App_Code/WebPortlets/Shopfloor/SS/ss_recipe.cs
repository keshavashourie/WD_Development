/* Copyright 2019 Siemens */
using System;
using System.Collections.Generic;
using System.Linq;
using Camstar.WebPortal.WebPortlets;
using Camstar.WCF.ObjectStack;
using Camstar.WebPortal.Utilities;
using Camstar.WCF.Services;
using System.Web;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using Camstar.WebPortal.FormsFramework.WebControls;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using System.Web.UI.WebControls;

/// <summary>
/// Summary description for SS_Recipe
/// </summary>
namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class SS_Recipe : MatrixWebPart
    {
        #region Properties

        // TextBoxs
        CWC.TextBox _txtSelectionIdField { get { return Page.FindCamstarControl("SelectionId") as CWC.TextBox; } }      
        CWC.TextBox _txtRecipeDescriptionField { get { return Page.FindCamstarControl("RecipeDescription") as CWC.TextBox; } }
        // NamedObject
        CWC.NamedObject _ndoEquipmentField { get { return Page.FindCamstarControl("Equipment") as CWC.NamedObject; } }
        // RevisionedObject
        CWC.RevisionedObject _rdoRequiredRecipeField { get { return Page.FindCamstarControl("RequiredRecipe") as CWC.RevisionedObject; } }
        // ContainerList
        CWC.ContainerList _ContainerField { get { return Page.FindCamstarControl("Container") as CWC.ContainerList; } }
        // JQDataGrids
        JQDataGrid _gridRecipeParametersField { get { return Page.FindCamstarControl("RecipeParameters") as JQDataGrid; } }
        JQDataGrid _gridSubRecipesField { get { return Page.FindCamstarControl("SubRecipes") as JQDataGrid; } }
        CWC.NamedObject _ndoProcessTypeField { get { return Page.FindCamstarControl("ProcessType") as CWC.NamedObject; } }        
    

        #endregion
        
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            FetchRequiredRecipe();
            FetchGridData();
        }

        private void FetchRequiredRecipe()
        {
            try
            {
                // get the session and user profile
                var fs = FrameworkManagerUtil.GetFrameworkSession();

                WIPMainService oService = new WIPMainService(fs.CurrentUserProfile);
                WIPMain oServiceData = new WIPMain();
                WIPMain_Info oServiceInfo = new WIPMain_Info();
                ResultStatus oResultStatus = new ResultStatus();

                if (_txtSelectionIdField.TextControl.Text != null)
                {
                    if (_ContainerField.Data != null)
                    {
                        oServiceData.Container = new ContainerRef();
                        oServiceData.Container.Name = _ContainerField.Data.ToString();
                    }
                    else
                    {
                        oServiceData.SelectionId = _txtSelectionIdField.Data == null ? null : _txtSelectionIdField.Data.ToString(); 
                    }
                }

                if (_ndoEquipmentField.TextEditControl.Text != null)
                {
                    oServiceData.Equipment = new NamedObjectRef();
                    oServiceData.Equipment.Name = _ndoEquipmentField.TextEditControl.Text;
                }

                if (_ndoProcessTypeField.TextEditControl.Text != null)
                {
                    oServiceData.ProcessType = new NamedObjectRef();
                    oServiceData.ProcessType.Name = _ndoProcessTypeField.TextEditControl.Text;
                }
				
                oServiceInfo.RequiredRecipe = FieldInfoUtil.RequestValue();

                //Set the request
                WIPMain_Request oServiceRequest = new WIPMain_Request();
                oServiceRequest.Info = oServiceInfo;

                // Set the status
                WIPMain_Result oServiceResult = new WIPMain_Result();

                // execute the request selection values
                ResultStatus objRS = oService.ResolveSelectionId(oServiceData, oServiceRequest, out oServiceResult);
                if (objRS.IsSuccess)
                {
                    if (oServiceInfo.RequiredRecipe != null)
                    {
                        _rdoRequiredRecipeField.Data = oServiceResult.Value.RequiredRecipe;
                    }
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        private void FetchGridData()
        {
            try
            {
                // get the session and user profile
                var fs = FrameworkManagerUtil.GetFrameworkSession();

                RecipeMaintService oService = new RecipeMaintService(fs.CurrentUserProfile);
                RecipeMaint oServiceData = new RecipeMaint();
                RecipeMaint_Info oServiceInfo = new RecipeMaint_Info();
                ResultStatus oResultStatus = new ResultStatus();

                RevisionedObjectRef r = new RevisionedObjectRef();
                r.Name = _rdoRequiredRecipeField.TextEditControl.Text;
                if (_rdoRequiredRecipeField.RevisionControl.Text != "")
                {
                    r.Revision = _rdoRequiredRecipeField.RevisionControl.Text;
                }
                else
                {
                    r.RevisionOfRecord = true;
                }
                oServiceData.ObjectToChange = r;

                //Field to Request
                RecipeChanges_Info rInfo = new RecipeChanges_Info();
                rInfo.Name = FieldInfoUtil.RequestValue();
                rInfo.Revision = FieldInfoUtil.RequestValue();
                rInfo.IsRevOfRcd = FieldInfoUtil.RequestValue();
                rInfo.Description = FieldInfoUtil.RequestValue();

                // Recipe Parameters Request

                rInfo.DocumentParameters = new DocumentParametersChanges_Info();
                rInfo.DocumentParameters.ParamName = FieldInfoUtil.RequestValue();
                rInfo.DocumentParameters.ParamValue = FieldInfoUtil.RequestValue();
                // CR 131
                rInfo.DocumentParameters.ss_FieldType = FieldInfoUtil.RequestValue();
                rInfo.DocumentParameters.ss_MinDataValue = FieldInfoUtil.RequestValue();
                rInfo.DocumentParameters.ss_MaxDataValue = FieldInfoUtil.RequestValue();
                rInfo.DocumentParameters.ss_ValidValues = FieldInfoUtil.RequestValue();

                // Sub Recipes Request

                rInfo.SubRecipes = new SubRecipesChanges_Info();
                rInfo.SubRecipes.SubRecipe = FieldInfoUtil.RequestValue();
                rInfo.SubRecipes.SubEquipmentLogicalID = FieldInfoUtil.RequestValue();
                rInfo.SubRecipes.RecipeSequence = FieldInfoUtil.RequestValue();

                oServiceInfo.ObjectChanges = rInfo;

                //Set the request
                RecipeMaint_Request oServiceRequest = new RecipeMaint_Request();
                oServiceRequest.Info = oServiceInfo;

                // Set the status
                RecipeMaint_Result oServiceResult = new RecipeMaint_Result();

                // execute the request selection values
                ResultStatus objRS = oService.Load(oServiceData, oServiceRequest, out oServiceResult);

                _gridRecipeParametersField.ClearData();
                _gridSubRecipesField.ClearData();

                if (objRS.IsSuccess)
                {
                    if (oServiceResult.Value.ObjectChanges.Description != null)
                    {
                        _txtRecipeDescriptionField.TextControl.Text = oServiceResult.Value.ObjectChanges.Description.ToString();
                    }

                    if (oServiceResult.Value.ObjectChanges.DocumentParameters != null)
                    {
                        foreach (DocumentParametersChanges _recipeParameters in oServiceResult.Value.ObjectChanges.DocumentParameters)
                        {
                            // int iNewRowCount = _gridRecipeParametersField.BoundContext.GetTotalRows();
                            // (_gridRecipeParametersField.GridContext as ItemDataContext).MakeAutoRowId(iNewRowCount);
                            // string id = (_gridRecipeParametersField.GridContext as ItemDataContext).AddNewRow(iNewRowCount.ToString());
                            // object recipeParametersDetails = ((_gridRecipeParametersField.GridContext as ItemDataContext).Data as Array).GetValue(iNewRowCount);

                            // (recipeParametersDetails as DocumentParametersChanges).ParamName = _recipeParameters.ParamName;
                            // (recipeParametersDetails as DocumentParametersChanges).ParamValue = _recipeParameters.ParamValue;

                            // _gridRecipeParametersField.GridContext.AdjustCurrentPage(id);
                            // CamstarWebControl.SetRenderToClient(_gridRecipeParametersField);

                            RecipeParameters_AddNewRow((string)_recipeParameters.ParamName, (string)_recipeParameters.ParamValue, (string)_recipeParameters.ss_FieldType, (string)_recipeParameters.ss_MinDataValue, (string)_recipeParameters.ss_MaxDataValue, (string)_recipeParameters.ss_ValidValues);
                        }
                    }

                    if (oServiceResult.Value.ObjectChanges.SubRecipes != null)
                    {
                        foreach (SubRecipesChanges _subRecipes in oServiceResult.Value.ObjectChanges.SubRecipes)
                        {
                            //int iNewRowCount = _gridSubRecipesField.BoundContext.GetTotalRows();
                            //(_gridSubRecipesField.GridContext as ItemDataContext).MakeAutoRowId(iNewRowCount);
                            //string id = (_gridSubRecipesField.GridContext as ItemDataContext).AddNewRow(iNewRowCount.ToString());
                            //object subRecipesDetails = ((_gridSubRecipesField.GridContext as ItemDataContext).Data as Array).GetValue(iNewRowCount);

                            //(subRecipesDetails as SubRecipesChanges).SubRecipe = _subRecipes.SubRecipe;
                            //(subRecipesDetails as SubRecipesChanges).SubEquipmentLogicalID = _subRecipes.SubEquipmentLogicalID;
                            //(subRecipesDetails as SubRecipesChanges).RecipeSequence = _subRecipes.RecipeSequence;

                            //_gridSubRecipesField.GridContext.AdjustCurrentPage(id);
                            //CamstarWebControl.SetRenderToClient(_gridSubRecipesField);

                            SubRecipes_AddNewRow(_subRecipes.SubRecipe, (string) _subRecipes.SubEquipmentLogicalID, (int) _subRecipes.RecipeSequence);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        public void RecipeParameters_AddNewRow(string ParamName, string ParamValue, string ss_FieldType, string ss_MinDataValue, string ss_MaxDataValue, string ss_ValidValues)
        {
            try
            {
                JQDataGrid _gridDetails = _gridRecipeParametersField;
                DocumentParametersChanges[] oNewDetail = new DocumentParametersChanges[1];
                oNewDetail[0] = new DocumentParametersChanges();
                oNewDetail[0].ParamName = ParamName;
                oNewDetail[0].ParamValue = ParamValue;
                oNewDetail[0].ss_FieldType = ss_FieldType;
                oNewDetail[0].ss_MinDataValue = ss_MinDataValue;
                oNewDetail[0].ss_MaxDataValue = ss_MaxDataValue;
                oNewDetail[0].ss_ValidValues = ss_ValidValues;
                DocumentParametersChanges[] oExisting = (_gridDetails.GridContext as BoundContext).Data as DocumentParametersChanges[];
                if (oExisting != null)
                {
                    bool isUnique = true;
                    for (int i = 0; i < oExisting.Length; i++)
                    {
                        if (oExisting[i].ParamName.Equals(oNewDetail[0].ParamName))
                        {
                            isUnique = false;
                        }
                    }
                    if (isUnique)
                    {
                        DocumentParametersChanges[] oMerged = new DocumentParametersChanges[oExisting.Length + 1];
                        Array.Copy(oExisting, oMerged, oExisting.Length);
                        Array.Copy(oNewDetail, 0, oMerged, oExisting.Length, 1);
                        (_gridDetails.GridContext as BoundContext).Data = oMerged.ToArray();
                    }
                }
                else
                {
                    (_gridDetails.GridContext as BoundContext).Data = oNewDetail.ToArray();
                }
                _gridDetails.BoundContext.LoadData();
                CamstarWebControl.SetRenderToClient(_gridDetails);
            }
            catch (Exception ex)
            { }
        }

        public void SubRecipes_AddNewRow(RevisionedObjectRef SubRecipe, string SubEquipmentLogicalID, int RecipeSequence)
        {
            try
            {
                JQDataGrid _gridDetails = _gridSubRecipesField;
                SubRecipesChanges[] oNewDetail = new SubRecipesChanges[1];
                oNewDetail[0] = new SubRecipesChanges();
                oNewDetail[0].SubRecipe = SubRecipe;
                oNewDetail[0].SubEquipmentLogicalID = SubEquipmentLogicalID;
                oNewDetail[0].RecipeSequence = RecipeSequence;
                SubRecipesChanges[] oExisting = (_gridDetails.GridContext as BoundContext).Data as SubRecipesChanges[];
                if (oExisting != null)
                {
                    bool isUnique = true;
                    for (int i = 0; i < oExisting.Length; i++)
                    {
                        if (oExisting[i].SubRecipe.Equals(oNewDetail[0].SubRecipe))
                        {
                            isUnique = false;
                        }
                    }
                    if (isUnique)
                    {
                        SubRecipesChanges[] oMerged = new SubRecipesChanges[oExisting.Length + 1];
                        Array.Copy(oExisting, oMerged, oExisting.Length);
                        Array.Copy(oNewDetail, 0, oMerged, oExisting.Length, 1);
                        (_gridDetails.GridContext as BoundContext).Data = oMerged.ToArray();
                    }
                }
                else
                {
                    (_gridDetails.GridContext as BoundContext).Data = oNewDetail.ToArray();
                }
                _gridDetails.BoundContext.LoadData();
                CamstarWebControl.SetRenderToClient(_gridDetails);
            }
            catch (Exception ex)
            { }
        }
    }
}



