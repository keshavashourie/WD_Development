// Copyright Siemens 2019
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Web.UI;
using Camstar.WCF.ObjectStack;
using Camstar.WCF.Services;

using Camstar.WebPortal.PortalFramework;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WebPortal.Utilities;
using Camstar.WebPortal.WCFUtilities;
using Camstar.WebPortal.WebPortlets;
using Camstar.WebPortal.Personalization;

using CamstarPortal.WebControls;


namespace Camstar.WebPortal.WebPortlets.Modeling
{
    public class ObjectDataPointDetail : MatrixWebPart
    {
        protected virtual JQDataGrid Object_DPQueryParameters
        { get { return Page.FindCamstarControl("Object_DPQueryParameters") as JQDataGrid; } }

        protected virtual DropDownList Object_SelValType
        { get { return Page.FindCamstarControl("Object_SelValType") as DropDownList; } }

        protected virtual DropDownList Object_ObjectType
        { get { return Page.FindCamstarControl("Object_ObjectType") as DropDownList; } }

        protected virtual DropDownList Object_ObjectTypeName
        { get { return Page.FindCamstarControl("Object_ObjectTypeName") as DropDownList; } }

        protected virtual NamedObject Object_ObjectGroup
        { get { return Page.FindCamstarControl("Object_ObjectGroup") as NamedObject; } }

        protected virtual TextBox Object_ListFieldExpression
        { get { return Page.FindCamstarControl("Object_ListFieldExpression") as TextBox; } }

        protected virtual DropDownList Object_QueryType
        { get { return Page.FindCamstarControl("Object_QueryType") as DropDownList; } }

        protected virtual DropDownList Object_QueryName
        { get { return Page.FindCamstarControl("Object_QueryName") as DropDownList; } }

        const string SessionResourceCDOs = "SessionResourceCDOs";
        public List<int> ResourceCDOs
        {
            get { return HttpContext.Current.Session[SessionResourceCDOs] as List<int>; }
            set { HttpContext.Current.Session[SessionResourceCDOs] = value; }
        }
        const string SessionResourceGroupCDOs = "SessionResourceGroupCDOs";
        public List<int> ResourceGroupCDOs
        {
            get { return HttpContext.Current.Session[SessionResourceGroupCDOs] as List<int>; }
            set { HttpContext.Current.Session[SessionResourceGroupCDOs] = value; }
        }

        protected virtual CheckBox MapToUserAttribute
        { get { return Page.FindCamstarControl("ObjectDataPointChanges_MapToUserAttribute") as CheckBox; } }

        protected virtual TextBox AttributeName
        { get { return Page.FindCamstarControl("ObjectDataPointChanges_AttributeName") as TextBox; } }

        protected virtual CheckBox ValidateResource
        { get { return Page.FindCamstarControl("ObjectDataPointChanges_ValidateResource") as CheckBox; } }

        protected virtual TextBox DataPointName
        { get { return Page.FindCamstarControl("DataPointName") as TextBox; } }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            AttributeName.Enabled = (bool)MapToUserAttribute.Data;
            MapToUserAttribute.DataChanged += MapToUserAttribute_DataChanged;

            if (!Page.IsPostBack)
            {
                FrameworkSession session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
                QueryUtil qryUtil = new QueryUtil(session.CurrentUserProfile);
                if (ResourceCDOs == null)
                {
                    DataTable dt = qryUtil.Execute("SelectResourceCDOs");
                    if (dt != null)
                    {
                        List<int> list = new List<int>();
                        foreach (DataRow row in dt.Rows)
                        {
                            list.Add(int.Parse(row[0].ToString()));
                        }
                        ResourceCDOs = list;
                    }
                }
                if (ResourceGroupCDOs == null)
                {
                    DataTable dt = qryUtil.Execute("SelectResourceGroupCDOs");
                    if (dt != null)
                    {
                        List<int> list = new List<int>();
                        foreach (DataRow row in dt.Rows)
                        {
                            list.Add(int.Parse(row[0].ToString()));
                        }
                        ResourceGroupCDOs = list;
                    }
                }
            }
            Object_QueryName.DataChanged += Object_QueryName_DataChanged;

            Object_SelValType.DataChanged += SelValType_and_ObjectType_DataChanged;
            Object_ObjectType.DataChanged += SelValType_and_ObjectType_DataChanged;

            Object_ObjectGroup.DataChanged += Object_ObjectGroup_DataChanged;

            Object_ListFieldExpression.Hidden = true;
            Object_ObjectGroup.Hidden = true;
            Object_QueryType.Hidden = true;
            Object_QueryName.Hidden = true;
            Object_DPQueryParameters.Hidden = true;
			
            if (Object_SelValType.Data != null)
            {
                ClearControls((ObjectSelValTypeEnum) Object_SelValType.Data);
                EnableControls((ObjectSelValTypeEnum) Object_SelValType.Data);
            } else
				CheckValidateResource();

            if (Object_SelValType.Data != null && Object_ObjectTypeName.Data != null && Object_ObjectGroup.Data != null && (ObjectSelValTypeEnum)Object_SelValType.Data == ObjectSelValTypeEnum.ObjectGroup)
                (Object_ObjectGroup as NamedObject).CDOType = Object_ObjectTypeName.Text;
        }

        protected virtual void MapToUserAttribute_DataChanged(object sender, EventArgs e)
        {
            AttributeName.Enabled = (bool)MapToUserAttribute.Data;

            if (!AttributeName.Enabled)
                AttributeName.ClearData();

            if (AttributeName.Enabled && AttributeName.Data == null)
                AttributeName.Data = DataPointName.Data;
        }

        protected virtual void ClearControls(ObjectSelValTypeEnum selValType)
        {
            switch (selValType)
            {
                case ObjectSelValTypeEnum.ObjectGroup:
                    {
                        Object_QueryType.ClearData();
                        Object_QueryName.ClearData();
                        Object_DPQueryParameters.ClearData();

                        Object_ListFieldExpression.ClearData();
                        break;
                    }
                case ObjectSelValTypeEnum.Query:
                    {
                        Object_ObjectGroup.Data = null;
                        Object_ObjectGroup.ClearData();
                        Object_ListFieldExpression.ClearData();
                        break;
                    }
                case ObjectSelValTypeEnum.ListField:
                    {
                        Object_ObjectGroup.Data = null;
                        Object_ObjectGroup.ClearData();
                        Object_QueryType.ClearData();
                        Object_QueryName.ClearData();
                        Object_DPQueryParameters.ClearData();
                        break;
                    }
                case ObjectSelValTypeEnum.None:
                    {
                        Object_ObjectGroup.Data = null;
                        Object_ObjectGroup.ClearData();
                        Object_QueryType.ClearData();
                        Object_QueryName.ClearData();
                        Object_DPQueryParameters.ClearData();

                        Object_ListFieldExpression.ClearData();
                        break;
                    }
                case ObjectSelValTypeEnum.Static:
                    {
                        Object_ObjectGroup.Data = null;
                        Object_ObjectGroup.ClearData();
                        Object_QueryType.ClearData();
                        Object_QueryName.ClearData();
                        Object_DPQueryParameters.ClearData();

                        Object_ListFieldExpression.ClearData();
                        break;
                    }
            }
        }

        protected virtual void EnableControls(ObjectSelValTypeEnum selValType)
        {
            switch (selValType)
            {
                case ObjectSelValTypeEnum.ObjectGroup :
                    {
                        Object_ObjectGroup.Hidden = false;
                        Object_ListFieldExpression.Hidden = true;
                        break;
                    }
                case ObjectSelValTypeEnum.Query:
                    {
                        Object_QueryType.Hidden = false;
                        Object_QueryName.Hidden = false;
                        Object_DPQueryParameters.Hidden = false;
                        break;
                    }
                case ObjectSelValTypeEnum.ListField:
                    {
                        Object_ListFieldExpression.Hidden = false;
                        break;
                    }
                case ObjectSelValTypeEnum.Static:
                    {
                        Object_ListFieldExpression.Hidden = true;
                        break;
                    }
                case ObjectSelValTypeEnum.None:
                    {
                        Object_ListFieldExpression.Hidden = true;
                        break;
                    }

            }
			CheckValidateResource();
        }

		protected void CheckValidateResource()
		{
            bool isResource = false;
			if (Object_ObjectType.Data != null)
			{
				if (Object_ObjectType.Data != null && ResourceCDOs != null) 
					isResource = ResourceCDOs.Find(s => s == (int)Object_ObjectType.Data) > 0;
				if (Object_ObjectType.Data != null && ResourceGroupCDOs != null && !isResource)
					isResource = ResourceGroupCDOs.Find(s => s == (int)Object_ObjectType.Data) > 0;
			}
			ValidateResource.Hidden = !isResource;
		}

        protected virtual void SelValType_and_ObjectType_DataChanged(object sender, EventArgs e)
        {
            if (Object_SelValType.Data != null)
            {
                ClearControls((ObjectSelValTypeEnum)Object_SelValType.Data);
                EnableControls((ObjectSelValTypeEnum)Object_SelValType.Data);
            }
        }
        protected virtual void Object_ObjectGroup_DataChanged(object sender, EventArgs e)
        {
            if (!Page.IsPostBack) return;

            var dataContracts = Page.PortalContext.DataContract.DataMembers;
            if (dataContracts != null)
            {

                var dcPopup = dataContracts.FirstOrDefault(d => d.Value is PopupData);
                if (dcPopup != null)
                {
                    ((dcPopup.Value as PopupData).OutgoingData as ObjectDataPointChanges).ObjectGroup = (NamedObjectRef)Object_ObjectGroup.Data;
                }
            }
        }

        protected virtual void Object_QueryName_DataChanged(object sender, EventArgs e)
        {
            if (!Page.IsPostBack) return;
            if (Object_QueryType.Data == null || Object_QueryName.Data == null) 
            {
                Object_DPQueryParameters.ClearData();
                return;
            }

            FrameworkSession session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
            var service = new Camstar.WCF.Services.UserDataCollectionDefMaintService(session.CurrentUserProfile);

            var serviceData = new UserDataCollectionDefMaint();
            serviceData.ObjectChanges = new UserDataCollectionDefChanges();

            List<DataPointQueryParamChanges> _qp = new List<DataPointQueryParamChanges>();
            _qp.Add(new DataPointQueryParamChanges { ListItemAction = ListItemAction.Add });

            List<ObjectDataPointChanges> _dp = new List<ObjectDataPointChanges>();
            _dp.Add(new ObjectDataPointChanges()
            {
                ListItemAction = ListItemAction.Add,
                DataPointQueryParams = _qp.ToArray(),
                QueryName = (String)Object_QueryName.Data,
                QueryType = (QueryTypeEnum)Object_QueryType.Data
            });

            serviceData.ObjectChanges.DataPoints = _dp.ToArray();

            var request = new Camstar.WCF.Services.UserDataCollectionDefMaint_Request();
            var result = new Camstar.WCF.Services.UserDataCollectionDefMaint_Result();
            var resultStatus = new ResultStatus();

            request.Info = new UserDataCollectionDefMaint_Info
            {

                ObjectChanges = new UserDataCollectionDefChanges_Info
                {
                    DataPoints = new ObjectDataPointChanges_Info
                        {
                            DataPointQueryParams = new DataPointQueryParamChanges_Info
                                {
                                    ParameterName = new Info { RequestSelectionValues = true }
                                }
                        }
                }
            };

            resultStatus = service.GetEnvironment(serviceData, request, out result);

            if (!resultStatus.IsSuccess)
                throw new ApplicationException(resultStatus.ExceptionData.Description);

            Object_DPQueryParameters.ClearData();

            if (
                ((result.Environment as UserDataCollectionDefMaint_Environment).ObjectChanges.DataPoints as
                 ObjectDataPointChanges_Environment).DataPointQueryParams.ParameterName.SelectionValues.Rows != null)
            {
                RecordSet _params =
                    ((result.Environment as UserDataCollectionDefMaint_Environment).ObjectChanges.DataPoints as
                     ObjectDataPointChanges_Environment).DataPointQueryParams.ParameterName.SelectionValues;

                List<DataPointQueryParamChanges> queryParams = new List<DataPointQueryParamChanges>();

                foreach (var _row in _params.Rows)
                {
                    DataPointQueryParamChanges _queryParam = new DataPointQueryParamChanges()
                        {
                            ParameterName = _row.Values[0],
                            ParameterExpression = null
                        };
                    queryParams.Add(_queryParam);
                }
                Object_DPQueryParameters.Data = queryParams.ToArray();
            }
        }
    }
}
