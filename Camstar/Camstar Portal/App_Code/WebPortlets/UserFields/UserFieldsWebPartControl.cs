// Copyright Siemens 2023  
using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml.Linq;
using System.Collections.Generic;
using System.Reflection;

using Camstar.WCF.ObjectStack;
using Camstar.WebPortal.WCFUtilities;
using Camstar.WebPortal.Personalization;
using PERS = Camstar.WebPortal.Personalization;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using Camstar.WebPortal.FormsFramework.Utilities;

using WebClientPortal;
using RadioButton = Camstar.WebPortal.FormsFramework.WebControls.RadioButton;

using OM = Camstar.WCF.ObjectStack;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;

namespace Camstar.WebPortal.WebPortlets
{
    public class UserFieldsWebPart : MatrixWebPart
    {

        #region Public Properties

        public override string PrimaryServiceType
        {
            get
            {
                return string.IsNullOrEmpty(base.PrimaryServiceType) ? Page.PrimaryServiceType : base.PrimaryServiceType;
            }
            set
            {
                base.PrimaryServiceType = value;
            }
        } // PrimaryServiceType

        #endregion

        #region MatrixWebPart Methods

        protected override void AddFieldControls()
        {
            if(!string.IsNullOrEmpty(PrimaryServiceType))
            {
                var serviceDescription = GetFieldsDirectory();

                if (serviceDescription != null && serviceDescription.ChildItems != null &&
                    serviceDescription.ChildItems.Count > 0)
                {
                    var controlsList = GetUserFieldsListControls(serviceDescription, true);

                    int index = 0;
                    controlsList.ForEach(control => AddControl(control, ref index));
                }
            }
            base.AddFieldControls();
        } // AddFieldControls

        protected virtual void AddControl(Control control, ref int index)
        {
            if (control is FieldControl || control is JQDataGrid)
            {
                this[index/3, index%3] = control;
                index++;
            }
            if (IsResponsive)
            {
                var style = new PERS.Style();
                if (control is FieldControl)
                {
                    style.CSSClass = "col-4";
                    Items[index - 1].Style = style;
                }
                else if (control is JQDataGrid)
                {
                    style.CSSClass = "col-sm-12 col-md-4";
                    Items[index - 1].Style = style;
                }
            }

        } // AddControl

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            //set the max width so there is no overflow when using in the context of pagepanel
            this.CssClass = "userfields-wp";
        }

        protected override void OnPreLoad(object sender, EventArgs e)
        {
            base.OnPreLoad(sender, e);
            // Apply personlizations for grids            
            Items.ForEach(it => it.Controls.ForEach(c => {
                if (c is JQDataGrid)
                    (c as JQDataGrid).ApplyFieldPersonalization();
                else if (c is ContainerListGrid)
                    (c as ContainerListGrid).ApplyFieldPersonalization();
            }));
        }


        protected override void OnPreRender(System.EventArgs e)
        {
            base.OnPreRender(e);

            //if this is a float page, pass the service object back in a datacontract
            if (IsFloatPage)
                Page.DataContract.SetValueByName("UserFieldsDM", GetUserFields());
        }

        protected virtual OM.Service GetUserFields()
        {
            Type t = Type.GetType(string.Format(WCFClientAssemblyQualifiedPrefixName, _WCFNamespace, PrimaryServiceType));
                       
            Service retVal = (Service)Activator.CreateInstance(t);
                                  
            GetInputData(retVal);
            return retVal;
        }

        #endregion

        #region Build Controls List Methods

        protected virtual OMTypeDescriptor InitializeServiceDescriptor()
        {
            OMTypeDescriptor resultType = null;

            string serviceType = PrimaryServiceType;
            var serviceDataObject = CreateServiceData(serviceType);

            if (serviceDataObject != null)
            {
                resultType = new OMTypeDescriptor
                {
                    Name = serviceType,
                    TypeName = serviceDataObject.GetType().Name,
                    ItemType = OMType.Service
                };

                if (serviceDataObject is Maintenance &&
                    WCFObject.IsFieldExist(string.Format("{0}.{1}", serviceType, mkObjectChanges)))
                {
                    var metaData = WCFObject.GetFieldMetadata(string.Format("{0}.{1}", serviceType, mkObjectChanges));

                    resultType = new OMTypeDescriptor
                    {
                        Name = mkObjectChanges,
                        TypeName = metaData.CDOTypeName,
                        ItemType = OMType.Property,
                        Owner = resultType
                    };
                }
            }
            return resultType;
        } // InitializeServiceDescriptor

        protected virtual OMTypeDescriptor GetFieldsDirectory()
        {
            return GetFieldsDirectory(InitializeServiceDescriptor());
        } // GetFieldsDirectory

        protected virtual OMTypeDescriptor GetFieldsDirectory(OMTypeDescriptor typeDescriptor)
        {
            OMTypeDescriptor resultType = null;
            var portalStudioService = new PortalStudioService();

            if (typeDescriptor != null && portalStudioService.GetFieldsDirectory(ref typeDescriptor).IsSuccess)
                resultType = typeDescriptor;

            return resultType;
        }

        // GetFieldsDirectory

        protected virtual string GenerateControlName(string fieldExpression)
        {
            string[] path = fieldExpression.Split('.');
            return string.Format(mkNamePattern, PrimaryServiceType, path[path.Length - 1]);
        } // GenerateControlName

        protected virtual List<Control> GetUserFieldsListControls(OMTypeDescriptor typeDescription, bool isUserDefinedField)
        {
            var controlsList = new List<Control>();

            var userFieldsList = isUserDefinedField
                                             ? typeDescription.ChildItems.Where(fd => fd.Metadata != null && fd.Metadata.IsUserDefinedField).ToList()
                                             : typeDescription.ChildItems;

            List<FieldsDirectoryMapItem> userFieldsMapItemList = GetFieldsDirectoryMapItemList(userFieldsList);

            if (userFieldsMapItemList != null && userFieldsMapItemList.Count > 0)
            {
                userFieldsMapItemList.ForEach(fdm =>
                {
                    Control control = GetControl(fdm);

                    if (control != null)
                        controlsList.Add(control);
                }
                                                );
            }

            return controlsList;
        } // GetUserFieldsListControls

        protected virtual Type GetControlType(string controlTypeName)
        {
            return Type.GetType(controlTypeName, false) ??
                   Type.GetType(controlTypeName + ", App_Code", false) ??
                   Type.GetType(controlTypeName + ", Camstar.WebPortal.FormsFramework.WebControls", false) ??
                   Type.GetType(controlTypeName + ", Camstar.WebPortal.FormsFramework.WebGridControls", false) ??
                   Type.GetType(controlTypeName + ", Camstar.WebPortal.PortalFramework", false) ??
                   Type.GetType(controlTypeName + ", Camstar.WebPortal.FormsFramework", false) ??
                   Type.GetType(controlTypeName + ", CamstarPortal.WebControls", false);
        } // GetControlType

        protected virtual Control GetControl(FieldsDirectoryMapItem fieldsDirectoryMapItem)
        {
            Type controlType = GetControlType(fieldsDirectoryMapItem.ControlType);

            Control ctrl = null;
            if (controlType != null)
            {
                ctrl = Activator.CreateInstance(controlType) as Control;

                if (ctrl != null)
                {
                    ctrl.ID = GenerateControlName(fieldsDirectoryMapItem.TypeDescriptor.FieldExpression);
                    if (ctrl is ContainerListGrid)
                    {
                        var jgrid = ctrl as ContainerListGrid;
                        jgrid.FieldExpressions = fieldsDirectoryMapItem.TypeDescriptor.FieldExpression;
                        jgrid.RequestValueExpressions = fieldsDirectoryMapItem.TypeDescriptor.FieldExpression;
                    }
                    else if (ctrl is FieldControl)
                    {
                        var fc = (ctrl as FieldControl);
                        fc.FieldExpressions = fieldsDirectoryMapItem.TypeDescriptor.FieldExpression;
                        fc.LabelId = fieldsDirectoryMapItem.TypeDescriptor.Metadata.LabelID.ToString();
                        fc.ReadOnly = fieldsDirectoryMapItem.TypeDescriptor.Metadata.IsReadOnly;
                        fc.Required = fieldsDirectoryMapItem.TypeDescriptor.Metadata.IsRequired;
                        fc.CDOTypeName = fieldsDirectoryMapItem.TypeDescriptor.Metadata.CDOTypeName;
                    }
                    else if (ctrl is JQDataGrid)
                    {
                        var jgrid = ctrl as JQDataGrid;

                        var settings = new PERS.GridDataSettingsItemList();
                        jgrid.Settings = settings;
                        jgrid.GridDataMode = GridDataModes.ItemList;
                        (jgrid as ILabel).LabelId = fieldsDirectoryMapItem.TypeDescriptor.Metadata.LabelID.ToString();

                        settings.FieldExpressions = fieldsDirectoryMapItem.TypeDescriptor.FieldExpression;
                        settings.IsRequiredOnSubmit = fieldsDirectoryMapItem.TypeDescriptor.Metadata.IsRequired;
                        settings.EditorSettings = new PERS.JQGridEditorSettings() { EditingMode = JQEditingModes.Inline };

                        // Build columns
                        if (fieldsDirectoryMapItem.TypeDescriptor.Metadata.IsList)
                        {
                            var fx = CreateJQGridColumn(fieldsDirectoryMapItem.TypeDescriptor.Metadata) as JQFieldData;
                            fx.Name = fieldsDirectoryMapItem.TypeDescriptor.Name + "_Value";
                            fx.Editable = !fieldsDirectoryMapItem.TypeDescriptor.Metadata.IsReadOnly;
                            fx.Width = 200;
                            fx.Required = fieldsDirectoryMapItem.TypeDescriptor.Metadata.IsRequired;
                            if (fx is PERS.JQNamedObject)
                                (fx as PERS.JQNamedObject).EditorProperties = new EditorPropertiesNDO() { FieldExpressions = fieldsDirectoryMapItem.TypeDescriptor.FieldExpression };
                            settings.Columns = new JQFieldBase[] { fx };
                        }
                        if (IsResponsive)
                        {
                            if (jgrid.Settings.Automation == null)
                                jgrid.Settings.Automation = new GridAutomation();
                            jgrid.Settings.Automation.ResponsiveWidth = true;
                            jgrid.Settings.Automation.ShrinkColumnWidthToFit = true;
                        }
                    }

                    if (ctrl is RadioButton)
                        (ctrl as RadioButton).RadioControl.CheckedChanged += RadioButton_Click;

                    if (ctrl is Camstar.WebPortal.FormsFramework.WebControls.DateChooser)
                        (ctrl as Camstar.WebPortal.FormsFramework.WebControls.DateChooser).MaskingEnabled = true;
                }
            }
            return ctrl;
        } // GetControl

        protected virtual PERS.JQFieldBase CreateJQGridColumn(OMMetadata m)
        {
            PERS.JQFieldData f;
            switch (m.FieldTypeCode)
            {
                case WCF.ObjectStack.FieldTypeCode.TimeStamp:
                    f = new PERS.JQDateChooser();
                    break;
                case WCF.ObjectStack.FieldTypeCode.Boolean:
                    f = new PERS.JQFieldCheckBox();
                    break;
                case WCF.ObjectStack.FieldTypeCode.Reference:
                    f = new PERS.JQNamedObject();
                    break;
                default:
                    f = new PERS.JQTextBox();
                    break;
            }
            return f;
        }


        #endregion

        #region Fields Mapping Methods

        protected virtual ResultStatus LoadFieldsDirectoryMapping(out List<FieldsDirectoryMapItem> mapItemList)
        {
            bool status = false;
            string message = string.Empty;
            mapItemList = new List<FieldsDirectoryMapItem>();

            try
                {
                    if (File.Exists(HttpContext.Current.Server.MapPath(mkMappingFileName)))
                    {
                        XDocument xdoc = XDocument.Load(HttpContext.Current.Server.MapPath(mkMappingFileName));

                        mapItemList =
                            (from map in xdoc.Descendants(MappingFileStructure.FieldMapEntry)
                             let control = map.Element(MappingFileStructure.ControlLibrary)
                             let item = new FieldsDirectoryMapItem
                                            {
                                                TypeNames = map.Attribute(MappingFileStructure.TypeNames) == null
                                                        ? new string[0]
                                                        : map.Attribute(MappingFileStructure.TypeNames).Value.Split(','),
                                                FieldNames = map.Attribute(MappingFileStructure.FieldNames) == null
                                                        ? new string[0]
                                                        : map.Attribute(MappingFileStructure.FieldNames).Value.Split(','),
                                                IsEnum = map.Attribute(MappingFileStructure.IsEnum) == null
                                                        ? false
                                                        : bool.Parse(map.Attribute(MappingFileStructure.IsEnum).Value),
                                                IsList = map.Attribute(MappingFileStructure.IsList) == null
                                                        ? false
                                                        : bool.Parse(map.Attribute(MappingFileStructure.IsList).Value),
                                                IsReadOnly = map.Attribute(MappingFileStructure.IsReadOnly) == null
                                                        ? false
                                                        : bool.Parse(map.Attribute(MappingFileStructure.IsReadOnly).Value),
                                                ControlType = control.Attribute(MappingFileStructure.ControlType) == null
                                                        ? string.Empty
                                                        : control.Attribute(MappingFileStructure.ControlType).Value
                                                              
                                            }
                             select item).ToList();
                    }
                    status = true;
                }
                catch (Exception e)
                {
                    message = e.Message;
                }

            return new ResultStatus(message, status);
        } // LoadFieldsDirectoryMapping

        protected virtual FieldsDirectoryMapItem FindFieldDirectoryMapItem(OMTypeDescriptor fieldDescriptor, List<FieldsDirectoryMapItem> mapItemsList)
        {
            var mapItem = mapItemsList
                .Select(i => i)
                .Where(m => 
                    {
                        // First check field attributes
                        var res =
                            (m.IsEnum == fieldDescriptor.Metadata.IsEnum) &&
                            (m.IsList == fieldDescriptor.Metadata.IsList) &&
                            (m.IsReadOnly == fieldDescriptor.Metadata.IsReadOnly);
                        // Second check field names
                        if (m.TypeNames.Length > 0)
                            res = m.TypeNames.Contains(fieldDescriptor.TypeName);
                        // Third check field types
                        if (m.FieldNames.Length > 0)
                            res = m.FieldNames.Contains(fieldDescriptor.Name);
                        if(res)
                            m.TypeDescriptor = fieldDescriptor;

                        return res;
                    })
                .FirstOrDefault();

            if (mapItem != null)
                return new FieldsDirectoryMapItem
                           {
                               ControlType = mapItem.ControlType,
                               FieldNames = mapItem.FieldNames,
                               IsEnum = mapItem.IsEnum,
                               IsList = mapItem.IsList,
                               IsReadOnly = mapItem.IsReadOnly,
                               TypeDescriptor = mapItem.TypeDescriptor,
                               TypeNames = mapItem.TypeNames
                           };
            else
                return null;
                
        } // FindFieldDirectoryMapItem

        protected virtual List<FieldsDirectoryMapItem> GetFieldsDirectoryMapItemList(List<OMTypeDescriptor> fieldDescriptorsList)
        {
            List<FieldsDirectoryMapItem> result = null;
            List<FieldsDirectoryMapItem> mapItemsList;

            if (LoadFieldsDirectoryMapping(out mapItemsList).IsSuccess)
            {
                result = new List<FieldsDirectoryMapItem>();
                fieldDescriptorsList.ForEach(typeDescriptor =>
                                            {
                                                var mapItem = FindFieldDirectoryMapItem(typeDescriptor, mapItemsList);

                                                if (mapItem != null)
                                                    result.Add(mapItem);
                                            });
            }
            return result;
        } // GetFieldsDirectoryMapItemList

        #endregion

        #region Constants

        protected const string  mkObjectChanges = "ObjectChanges";
        protected const string mkNamePattern = "UserFields_{0}_{1}";
        protected const string mkGridFieldExpressionPattern = "{0}:{1}";
        protected const string mkMappingFileName = "FieldsDirectory.xml";
        private const string WCFClientAssemblyQualifiedPrefixName = "{0}.{1}, Camstar.WCFClient";
        private const string WCFClientBaseQualifiedPrefixName = "{0}.{1}, Camstar.WCFClientBase";
        public const string _WCFNamespace = "Camstar.WCF.ObjectStack";

        #endregion

    } // UserFieldsWebPart

    #region Fields Mapping Classes

    internal struct MappingFileStructure
    {
        public const string FieldMapEntry = "FieldMapEntry";
        public const string ControlLibrary = "ControlLibrary";
        public const string TypeNames = "TypeNames";
        public const string FieldNames = "FieldNames";
        public const string IsEnum = "IsEnum";
        public const string IsList = "IsList";
        public const string IsReadOnly = "IsReadOnly";
        public const string ControlType = "ControlType";
    } // MappingFileStructure

    public class FieldsDirectoryMapItem
    {
        public virtual bool IsEnum { get; set; }
        public virtual bool IsList { get; set; }
        public virtual bool IsReadOnly { get; set; }
        public virtual string[] TypeNames { get; set; }
        public virtual string[] FieldNames { get; set; }
        public virtual OMTypeDescriptor TypeDescriptor { get; set; }
        public virtual string ControlType { get; set; }

        public const string FieldMapEntryConst = "FieldMapEntry";
        public const string ControlLibraryConst = "ControlLibrary";
        public const string TypeNamesConst = "TypeNames";
        public const string FieldNamesConst = "FieldNames";
        public const string IsEnumConst = "IsEnum";
        public const string IsListConst = "IsList";
        public const string IsReadOnlyConst = "IsReadOnly";
        public const string ControlTypeConst = "ControlType";
    } // FieldsDirectoryMapItem

    #endregion
}
