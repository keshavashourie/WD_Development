// Copyright Siemens 2024 
using System;
using System.Activities.Expressions;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Web;
using System.Xml.Linq;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

using Camstar.WCF.ObjectStack;
using Camstar.WCF.Services;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using Camstar.WebPortal.Personalization;
using Camstar.WebPortal.PortalFramework;
using Camstar.WebPortal.Utilities;
using Camstar.WebPortal.WCFUtilities;
using Camstar.WebPortal.WebPortlets;
using Camstar.WebPortal.Tools.ASPXConverter;
using System.Text;
using PS = PortalStudioApi;
using System.Xml;


namespace WebClientPortal
{
    [ServiceContract(Namespace = "")]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public partial class PortalStudioService
    {
        [OperationContract]
        public virtual ResultStatus VerifyLogon(out UserProfile profile)
        {
            ResultStatus res = new ResultStatus("Logon Failed!", false);
            profile = null;
         if (IsSessionValid())
            {
                res = new ResultStatus("Logon OK", true);
                FrameworkSession session = FrameworkManagerUtil.GetFrameworkSession();
                profile = new UserProfile
                {
                    Name = session.CurrentUserProfile.Name,
                    UTCOffset = session.CurrentUserProfile.UTCOffset,
                    TimeZone = session.CurrentUserProfile.TimeZone,
                    SessionID = session.CurrentUserProfile.SessionID
                };
            }
            return res;
        }      

        [OperationContract]
        public virtual ResultStatus Logon(ref UserProfile profile, string domain)
        {
            string status = "Logon Failed!";
            var res = false;
            System.Web.Security.FormsAuthentication.SignOut();
            HttpContext.Current.Session.RemoveAll();
            var fs = new FrameworkSession();
            if( fs != null)
            {
                res = fs.Login(domain, profile.Name, profile.Password.Value, "CamstarPortal", HttpContext.Current.Application, HttpContext.Current.Session,
                TimeSpan.FromMinutes(60), "", "", ref status);
                if (res)
                {
                    //Authorization check - make sure the user has the role for Portal Studio Access
                    if (FrameworkManagerUtil.PortalStudioAccess(HttpContext.Current.Session))
                    {                   
                        System.Web.Security.FormsAuthentication.SetAuthCookie(profile.Name, true);
                        Camstar.WebPortal.Constants.URIConstantsBase.InitializeForAbsoluteURI(HttpContext.Current.Request);
                        profile.Name = fs.CurrentUserProfile.Name;
                        profile.SessionID = fs.CurrentUserProfile.SessionID;
                        profile.UTCOffset = fs.CurrentUserProfile.UTCOffset;
                        profile.TimeZone = fs.CurrentUserProfile.TimeZone;
                        if (profile.SessionID != null && !profile.SessionID.IsEmpty)
                            profile.Password = null;
                    }
                    else
                    {
                        status = Camstar.WebPortal.Constants.LabelConstantsAux.PortalStudioAccessDenied;
                        res = false;
                    }
                }
            }
            return new ResultStatus(status, res);
        }

        [OperationContract]
        public virtual ResultStatus LogonViaIDP(ref UserProfile profile, string email)
        {
            string status = "Logon Failed!";
            var res = false;
            System.Web.Security.FormsAuthentication.SignOut();
            HttpContext.Current.Session.RemoveAll();
            var fs = new FrameworkSession();
            if (fs != null)
            {
                res = fs.LoginViaIDP(email, "CamstarPortal", HttpContext.Current.Application, HttpContext.Current.Session, TimeSpan.FromMinutes(60),"", ref status);
                if (res)
                {
                    //Authorization check - make sure the user has the role for Portal Studio Access
                    if (FrameworkManagerUtil.PortalStudioAccess(HttpContext.Current.Session))
                    {
                        System.Web.Security.FormsAuthentication.SetAuthCookie(profile.Name, true);
                        Camstar.WebPortal.Constants.URIConstantsBase.InitializeForAbsoluteURI(HttpContext.Current.Request);
                        profile.Name = fs.CurrentUserProfile.Name;
                        profile.SessionID = fs.CurrentUserProfile.SessionID;
                        profile.UTCOffset = fs.CurrentUserProfile.UTCOffset;
                        profile.TimeZone = fs.CurrentUserProfile.TimeZone;
                        if (profile.SessionID != null && !profile.SessionID.IsEmpty)
                            profile.Password = null;
                    }
                    else
                    {
                        status = Camstar.WebPortal.Constants.LabelConstantsAux.PortalStudioAccessDenied;
                        res = false;
                    }
                }
            }
            return new ResultStatus(status, res);
        }

        [OperationContract]
        public virtual ResultStatus LogonAFX(ref UserProfile profile, string domain)
        {
            string status = "Logon Failed!";
            var res = false;
            System.Web.Security.FormsAuthentication.SignOut();
            HttpContext.Current.Session.RemoveAll();
            var fs = new FrameworkSession();
            if (fs != null)
            {
                res = fs.Login(domain, profile.Name, profile.Password.Value, "CamstarPortal", HttpContext.Current.Application, HttpContext.Current.Session,
                    profile.UTCOffset, profile.TimeZone, profile.Dictionary, ref status);
                if (res)
                {
                    //Authorization check - make sure the user has the role for Portal Studio Access
                    if (FrameworkManagerUtil.PortalStudioAccess(HttpContext.Current.Session))
                    {
                        System.Web.Security.FormsAuthentication.SetAuthCookie(profile.Name, true);
                        Camstar.WebPortal.Constants.URIConstantsBase.InitializeForAbsoluteURI(HttpContext.Current.Request);
                        profile.Name = fs.CurrentUserProfile.Name;
                        profile.SessionID = fs.CurrentUserProfile.SessionID;
                        profile.UTCOffset = fs.CurrentUserProfile.UTCOffset;
                        profile.TimeZone = fs.CurrentUserProfile.TimeZone;
                        if (profile.SessionID != null && !profile.SessionID.IsEmpty)
                            profile.Password = null;
                    }
                    else
                    {
                        status = Camstar.WebPortal.Constants.LabelConstantsAux.PortalStudioAccessDenied;
                        res = false;
                    }
                }
            }
            return new ResultStatus(status, res);
        }

        [OperationContract]
        public virtual ResultStatus Logout()
        {
            FrameworkSession session = FrameworkManagerUtil.GetFrameworkSession();
            session.Logout();
            System.Web.Security.FormsAuthentication.SignOut();
            HttpContext.Current.Session.Abandon();
            HttpContext.Current.Response.Cookies.Add(new HttpCookie("ASP.NET_SessionId", ""));

            return new ResultStatus("", true);
        }
        
        [OperationContract]
        public virtual ResultStatus SessionReset()
        {            
            HttpContext.Current.Session["SessionRefreshTime"] = DateTime.Now;    
            return new ResultStatus("", true);
        }

        [OperationContract]
        public virtual ResultStatus ResolveObjectColumns(string objectExpression, out Header[] headers)
        {
            bool status = false;
            string message = string.Empty;

            headers = null;

            if (IsSessionValid())
            {
                try
                {
                    if (WCFObject.IsFieldExist(objectExpression))
                    {
                        MetadataAttribute metadata = WCFObject.GetFieldMetadata(objectExpression);

                        if (metadata.FieldTypeCode == FieldTypeCode.Object)
                        {
                            var om = new OMTypeDescriptor
                            {
                                ItemType = OMType.Property,
                                Name = ControlUtil.ExtractFieldName(objectExpression),
                                TypeName = metadata.CDOTypeName
                            };

                            this.GetFieldsDirectory(ref om);

                            if (om.ChildItems != null)
                            {
                                headers = om.ChildItems.Where(item => item.ItemType == OMType.Property && !item.Metadata.IsList).Select(item =>
                                    new Header()
                                    {
                                        Name = string.Format("${0},{1},{2},{3}",
                                                    item.Metadata.IsReadOnly.ToString(),
                                                    item.Metadata.IsRequired.ToString(),
                                                    item.Metadata.IsEnum ? "Enum" : item.TypeName, item.Name),
                                        Label = new Camstar.WCF.ObjectStack.Label() { ID = item.Metadata.LabelID }
                                    }
                                ).ToArray();
                            }
                        }
                        else //if (metadata.FieldTypeCode == FieldTypeCode.Reference)
                        {
                            // Simple grid
                            headers = new Header[] { 
                                new Header 
                                { 
                                    Name = 
                                    string.Format("${0},{1},{2},{3}",
                                                    metadata.IsReadOnly,
                                                    metadata.IsRequired,
                                                    metadata.IsEnum ? "Enum" : metadata.TypeName, ControlUtil.ExtractFieldName(objectExpression)),                                 
                                    Label = new Camstar.WCF.ObjectStack.Label{ID=metadata.LabelID}
                                } 
                            };
                        }

                        // exclude some fields
                        var excluded = new string[] { "IsFrozen", "ListItemToChange", "ObjectToChange", "Parent" };
                        headers = headers.Where(h => !excluded.Contains(h.Name.Split(',').Last())).ToArray();
                        var labelCache = FrameworkManagerUtil.GetLabelCache();
                        if (labelCache != null)
                        {
                            var list = new LabelList(headers.Select(h => h.Label).ToArray());
                            labelCache.GetLabels(list);
                            headers.ToList().ForEach(h => h.Label = list.FirstOrDefault(l => l.ID == h.Label.ID));
                        }
                    }
                    else
                    {
                        throw new Exception(string.Format("Field expression '{0}' is not valid.", objectExpression));
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
        public virtual ResultStatus ResolveSelectionValuesColumns(string selvalExpression, out Header[] headers)
        {
            bool status = false;
            string message = string.Empty;

            headers = null;

            if (IsSessionValid())
            {
                try
                {
                    if (WCFObject.IsFieldExist(selvalExpression))
                    {
                        string serviceName = ControlUtil.ExtractServiceType(selvalExpression);
                        WSDataCreator creator = new WSDataCreator();

                        Info info = creator.CreateServiceInfo(serviceName);
                        Service data = creator.CreateServiceData(serviceName);

                        MetadataAttribute metadata = WCFObject.GetFieldMetadata(selvalExpression);
                        Info i = null;
                        if (metadata.FieldTypeCode == FieldTypeCode.Object)
                            i = creator.CreateServiceInfo(metadata.CDOTypeName);
                        else
                            i = new Info();
                        i.RequestSelectionValues = true;
                        i.RequestSelectionValuesInfo = new SelectionValuesInfo();
                        i.RequestSelectionValuesInfo.Parameters = new QueryParameter[]
                            {
                                new QueryParameter {Name="NameFilter", Value=Guid.NewGuid().ToString() },
                                new QueryParameter {Name="SpecificTypeOnly", Value="0" },
                            };

                        WCFObject wcfo = new WCFObject(info);
                        wcfo.SetValue(selvalExpression, i);

                        var service = new Camstar.WebPortal.FormsFramework.WCFService();
                        Camstar.WCF.ObjectStack.Environment env = null;
                        ResultStatus rs = service.GetEnvironment(data, info, null, null, ref env);

                        if (rs.IsSuccess)
                        {
                            wcfo = new WCFObject(env);
                            var obj = wcfo.GetValue(selvalExpression) as Camstar.WCF.ObjectStack.Environment;
                            if (obj != null && obj.SelectionValues != null)
                                headers = obj.SelectionValues.Headers;
                        }
                        else
                        {
                            throw new Exception(rs.ToString());
                        }
                    }
                    else
                    {
                        throw new Exception(string.Format("Field expression '{0}' is not valid.", selvalExpression));
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
        public virtual ResultStatus GetFieldsDirectory(ref OMTypeDescriptor type)
        {
            bool status = false;
            string message = string.Empty;

            if (IsSessionValid())
            {
                try
                {
                    if (type == null)
                        throw new Exception("parent type cannot be null");

                    ObjectsStackReflector omr = ObjectsStackReflector.GetInstance(type.TypeName);
                    message = omr.ReflectTypes(ref type);
                    status = string.IsNullOrEmpty(message);
                }
                catch (Exception e)
                {
                    message = e.Message;
                }
            }

            return new ResultStatus(message, status);
        }

        [OperationContract]
        public virtual ResultStatus GetExposedProperties(ref ControlDescriptor[] controls)
        {
            bool status = false;
            string message = string.Empty;
            if (IsSessionValid())
            {
                try
                {
                    foreach (ControlDescriptor control in controls)
                    {
                        Type controlType = Type.GetType(control.TypeName, false);
                        if (controlType != null)
                        {
                            IPersonalizableField field = Activator.CreateInstance(controlType) as IPersonalizableField;
                            if (field != null)
                            {
                                control.Properties =
                                       FieldControl.GetProperties(field.GetType()).OfType<System.ComponentModel.PropertyDescriptor>().
                                       Select(prop => new ControlPropertyDescriptor { Name = prop.Name, TypeName = prop.PropertyType.Name }).
                                       ToArray();
                            }

                            status = true;
                        }
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
        public virtual ResultStatus GetWritableProperties(string typeName, out string[] properties)
        {
            bool status = false;
            string message = string.Empty;
            properties = null;
            if (IsSessionValid())
            {
                try
                {
                    // TODO: Revise to avoid assembly names running over
                    Type target = Type.GetType(typeName, false);
                    if (target == null)
                        target = Type.GetType(typeName + ", Camstar.WebPortal.FormsFramework.WebControls", false);
                    if (target == null)
                        target = Type.GetType(typeName + ", Camstar.WebPortal.FormsFramework.WebGridControls", false);
                    if (target == null)
                        target = Type.GetType(typeName + ", Camstar.WebPortal.PortalFramework", false);
                    if (target == null)
                        target = Type.GetType(typeName + ", Camstar.WebPortal.FormsFramework", false);
                    if (target == null)
                        target = Type.GetType(typeName + ", CamstarPortal.WebControls", false);
                    if (target != null)
                    {
                        properties = target.GetProperties().Where(p => p.CanWrite && (p.PropertyType.IsPrimitive || p.PropertyType == typeof(object) || p.PropertyType == typeof(string))).Select(p => p.Name).ToArray();
                        var condAttrs = target.GetCustomAttributes(typeof(WritablePropertiesAttribute), true);
                        if (condAttrs != null)
                        {
                            foreach (var attr in condAttrs.OfType<WritablePropertiesAttribute>())
                            {
                                foreach (var a in attr.PropertyList)
                                {
                                    var moreProperties = GetMoreProperties(target, a);
                                    properties = properties.Union(moreProperties).ToArray();
                                }
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
        public virtual ResultStatus GetTraceLog(string pageName, out TraceItem[] items)
        {
            bool status = false;
            string message = string.Empty;
            items = null;
            if (IsSessionValid())
            {
                try
                {
                    if (!string.IsNullOrEmpty(pageName))
                        items = TraceManager.Current.Records.Where(item => item.Name.StartsWith(pageName + " (id-")).ToArray();
                    else
                        items = TraceManager.Current.Records.ToArray();

                    foreach (var it in items)
                    {
                        it.PageElapsedTime = it.DisplayElapsed;
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

        protected virtual string[] GetMoreProperties(Type target, string propertyName)
        {
            List<string> res = new List<string>();
            string prefix = string.Empty;
            foreach (var ps in propertyName.Split('.'))
            {
                var prop = target.GetProperty(ps);
                if (prop != null)
                {
                    if (prop.PropertyType.IsClass)
                    {
                        target = prop.PropertyType;
                        prefix += (prop.Name + ".");
                    }
                    else
                    {
                        res.Add(prefix + prop.Name);
                    }
                }
                else
                {
                    break;
                }
            }
            return res.ToArray();
        }

        [OperationContract]
        public virtual ResultStatus GetTypeDescriptor(string typeName, out TypeDescriptor type)
        {
            bool status = false;
            string message = string.Empty;
            type = new TypeDescriptor();
            if (IsSessionValid())
            {
                try
                {
                    // TODO: Revise to avoid assembly names running over
                    Type target = Type.GetType(typeName, false);
                    if (target == null)
                        target = Type.GetType(typeName + ", Camstar.WebPortal.FormsFramework.WebControls", false);
                    if (target == null)
                        target = Type.GetType(typeName + ", Camstar.WebPortal.FormsFramework.WebGridControls", false);
                    if (target == null)
                        target = Type.GetType(typeName + ", Camstar.WebPortal.PortalFramework", false);
                    if (target == null)
                        target = Type.GetType(typeName + ", Camstar.WebPortal.FormsFramework", false);
                    if (target == null)
                        target = Type.GetType(typeName + ", CamstarPortal.WebControls", false);
                    if (target != null)
                    {
                        object[] attributes = target.GetCustomAttributes(typeof(ClientSideDescriptorAttribute), true);
                        ClientSideDescriptorAttribute attribute = null;
                        if (attributes != null && attributes.Length > 0)
                            attribute = attributes[0] as ClientSideDescriptorAttribute;

                        type.TypeName = target.FullName;
                        type.Properties = target.GetProperties().Select(p => new ControlPropertyDescriptor() { Name = p.Name, TypeName = p.PropertyType.FullName, Writable = p.CanWrite }).OrderBy(p => p.Name).ToArray();
                        type.Events = target.GetEvents().Select(e => e.Name).OrderBy(e => e).ToArray();
                        if (target.Name.Contains("JQDataGrid"))
                        {
                            // Add GridContext events
                            var contextEvents = typeof (GridContext).GetEvents().Select(e => "GridContext." + e.Name).OrderBy(se => se);
                            type.Events = type.Events.Union(contextEvents).ToArray();
                        }
                        if (attribute != null)
                        {
                            type.ClientEvents = attribute.Events.OrderBy(o => o).ToArray();
                            type.ClientProperties = attribute.Properties.OrderBy(o => o).ToArray();
                            type.ClientMethods = attribute.Methods.OrderBy(o => o).ToArray();
                        }

                        var handlers = target.GetMethods(BindingFlags.Instance | BindingFlags.Public).Where(m =>
                        {
                            bool special =
                                m.Name.StartsWith("add_", false, null) ||
                                m.Name.StartsWith("remove_", false, null) ||
                                m.Name.StartsWith("get_", false, null) ||
                                m.Name.StartsWith("set_", false, null);
                            return !special;
                        }).Where(mi => !mi.IsGenericMethod).Select(mi =>
                            {
                                return new EventHandlerDescriptor
                                {
                                    Name = mi.Name,
                                    Parameters = mi.GetParameters().Select(pi =>
                                        new ParameterDescriptor
                                        {
                                            Name = pi.Name,
                                            Type = pi.ParameterType.AssemblyQualifiedName
                                        }).ToArray()
                                };
                            });
                        type.Handlers = (from h in handlers
                                         orderby h.ToString()
                                         group h by h.ToString() into grp
                                         select grp.FirstOrDefault()).ToArray();
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
        public virtual ResultStatus GetCodeBehindClassesForPages(out CodeBehindDescriptor[] codeBehindes)
        {
            codeBehindes = null;

            bool status = false;
            string message = string.Empty;

            if (IsSessionValid())
            {
                try
                {
                    codeBehindes = GetClassesByType(typeof(WebPartBase)).ToArray();
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
        public virtual ResultStatus GetCodeBehindClassesForWebParts(out CodeBehindDescriptor[] codeBehindes)
        {
            codeBehindes = null;

            bool status = false;
            string message = string.Empty;

            if (IsSessionValid())
            {
                try
                {
                    codeBehindes = GetClassesByType(typeof(WebPartBase)).ToArray();
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
        public virtual ResultStatus GetWebPartsList(out List<UIComponentReference> webParts)
        {
            webParts = new List<UIComponentReference>();
            bool status = false;
            string message = string.Empty;
            var ignoredAsms = new string[] { "CrystalDecisions", "WebGrease", "BusinessObjects" };

            if (IsSessionValid())
            {
                try
                {
                    var types =
                        from assembly in AppDomain.CurrentDomain.GetAssemblies()
                        where !ignoredAsms.Any(a=> assembly.FullName.StartsWith(a))
                        from t in assembly.GetTypes()
                        where
                            t.IsSubclassOf(typeof(WebPartBase)) && !t.IsSubclassOf(typeof(MatrixWebPartBase)) &&
                            !t.IsAbstract
                        select t;

                    foreach (var tp in types)
                    {
                        string[] categories = null;
                        var isDisplayed = true;
                        var title = tp.Name;
                        var attr = tp.GetCustomAttributes(typeof (PortalStudioAttribute), false).FirstOrDefault() as PortalStudioAttribute;

                        if (attr != null)
                        {
                            isDisplayed = attr.IsDisplayed;                         
                            if (isDisplayed && !string.IsNullOrEmpty(attr.Categories))
                                categories = attr.Categories.Split(',');
                            if (!string.IsNullOrEmpty(attr.Title))
                                title = attr.Title;
                        }

                        if (isDisplayed)
                        {
                            webParts.Add(
                                new UIComponentReference
                                {
                                    FullTypeName = tp.FullName,
                                    Title = title,
                                    Name = tp.FullName.Split('.').Last(),
                                    Categories = categories,
                                    IsStatic = true
                                } );
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

        protected virtual IEnumerable<CodeBehindDescriptor> GetClassesByType(Type classType)
        {
            if (FileLocation != null)
            {
                DirectoryInfo partDirectory = new DirectoryInfo(FileLocation);
                if (partDirectory.Exists)
                {
                    // Loop through the assemblies in the directory
                    foreach (Assembly privateAssembly in AppDomain.CurrentDomain.GetAssemblies())
                    {
                        Type[] assemblyTypes;
                        try
                        {
                            assemblyTypes = privateAssembly.GetTypes();
                        }
                        catch (Exception)
                        {
                            continue;
                        }
                        // Loop through the types in the assembly to see if contains any WebParts
                        // (or derivees thereof)
                        foreach (Type privateType in assemblyTypes)
                        {
                            // Process only web parts that are derived from the WebPartBase class
                            if (!privateType.IsSubclassOf(classType) || privateType.IsAbstract)
                                continue;
                            yield return new CodeBehindDescriptor
                            {
                                ClassName = privateType.Name,
                                Namespace = privateType.Namespace,
                                Description = privateType.FullName
                            };
                        }
                    }
                }
            }
        }

        protected virtual bool IsSessionValid()
        {
            FrameworkSession session = FrameworkManagerUtil.GetFrameworkSession();
            return session != null && session.CurrentUserProfile != null;
        }

        protected virtual string FileLocation
        {
            get
            {
                if (HttpContext.Current.Session != null)
                    return string.IsNullOrEmpty(_fileLocation) ?
                        HttpContext.Current.Server.MapPath(mkDefaultFileLocationValue) : _fileLocation;
                else
                    return null;
            }
            set
            {
                _fileLocation = value;
            }
        } // FileLocation

        [OperationContract]
        public virtual ResultStatus GetCustomActionHandlers(CodeBehindDescriptor codeBehind, out string[] handlers)
        {
            return GetActionHandler(codeBehind, typeof(Camstar.WebPortal.Personalization.CustomActionEventArgs), out handlers);
        }

        [OperationContract]
        public virtual ResultStatus GetConditionActionHandlers(CodeBehindDescriptor codeBehind, out string[] handlers)
        {
            return GetActionHandler(codeBehind, typeof(Camstar.WebPortal.Personalization.ConditionActionEventArgs), out handlers);
        }

        [OperationContract]
        public virtual ResultStatus GetInitContractActionHandlers(CodeBehindDescriptor codeBehind, out string[] handlers)
        {
            return GetActionHandler(codeBehind, typeof(Camstar.WebPortal.Personalization.InitContractActionEventArgs), out handlers);
        }

        protected virtual ResultStatus GetActionHandler(CodeBehindDescriptor codeBehind, Type argsType, out string[] handlers)
        {
            handlers = null;
            if (IsSessionValid())
            {
                Type handlersContainerType = Type.GetType(codeBehind.Description, false);
                if (handlersContainerType == null)
                    handlersContainerType = Type.GetType(codeBehind.Description + ", Camstar.WebPortal.PortalFramework", false);
                if (handlersContainerType != null)
                {
                    MethodInfo[] methods = handlersContainerType.GetMethods(BindingFlags.Instance | BindingFlags.Public);
                    if (methods != null)
                    {
                        handlers = methods.Where(m =>
                        {
                            bool condition = false;
                            ParameterInfo[] parameters = m.GetParameters();
                            if (parameters != null && parameters.Length == 2 &&
                                parameters[0].ParameterType == typeof(object) && parameters[1].ParameterType == argsType)
                                condition = true;
                            return condition;
                        }).OrderBy(m => m.Name).Select(m => m.Name).ToArray();
                    }
                }
            }

            return new ResultStatus("", true);
        }

        [OperationContract]
        public virtual ResultStatus GetWebPartLayoutStructure(ref WebPartDefinition webPartDefinition)
        {
            if (IsSessionValid())
            {
                try
                {
                    Type webPartType = Type.GetType(webPartDefinition.TypeName, false);
                    if (webPartType != null)
                    {
                        var atrs = webPartType.GetCustomAttributes(typeof(DynamicLayoutPersonalizationAttribute), true) as DynamicLayoutPersonalizationAttribute[];
                        if (atrs != null && atrs.Length > 0 && atrs.Single().IsDynamicLayout)
                        {
                            WebPartBase webPart = Activator.CreateInstance(webPartType) as WebPartBase;
                            webPart.GenerateLayoutStructure(webPartDefinition);
                        }
                        else
                        {
                            MethodInfo method = webPartType.GetMethod("GenerateLayoutStructure", BindingFlags.Static | BindingFlags.Public);
                            if (method != null)
                            {
                                ContentLayout layout = method.Invoke(null, null) as ContentLayout;
                                webPartDefinition.Layout = layout;
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    return new ResultStatus(e.ToString(), false);
                }
            }

            return new ResultStatus("", true);
        }

        [OperationContract]
        public virtual ResultStatus GetPageLayoutStructure(PageModel pageModel, out ContentLayout layout)
        {
            layout = null;
            if (IsSessionValid())
            {
                try
                {
                    string name = pageModel.TemplateName.Remove(0, "~//Template//".Length - 2).Replace(".ascx", "");
                    MethodInfo method = typeof(WebPartTemplate).GetMethod(name + "LayoutStructure", BindingFlags.Static | BindingFlags.Public);
                    if (method != null)
                    {
                        layout = method.Invoke(null, null) as ContentLayout;
                        if (layout != null)
                        {
                            foreach (GridLayoutCell cell in (layout as GridLayout).Cells)
                            {
                                Zone zone = (pageModel.PublishedContent as PageContent).Zones.SingleOrDefault(z => z.Name == cell.Items[0].Name);
                                if (zone != null)
                                    cell.Items = zone.WebPart.Select(wp => new ItemReference() { Name = wp.Name, Index = wp.Index }).ToArray();
                                else
                                    cell.Items = new ItemReference[0];
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    return new ResultStatus(e.ToString(), false);
                }
            }

            return new ResultStatus("", true);
        }

        [OperationContract]
        public virtual void UpdateCachedComponent(string name, CachedComponentType type)
        {
            if (IsSessionValid())
            {
                switch (type)
                {
                    case CachedComponentType.Page:
                        new PageMapping().RefreshComponent(name);
                        break;
                    case CachedComponentType.WebPart:
                        // Could be smart algo of refreshing relative pages only.
                        new PageMapping().RefreshComponents();
                        break;
                    case CachedComponentType.Tab:
                        TabMapping.Instance.RefreshComponent(name);
                        break;
                    case CachedComponentType.Menu:
                        CamstarPortal.WebControls.NavigationMenu.ResetMenu();
                        break;
                    case CachedComponentType.TargetMatrix:
                        TargetMatrixUtil.Instance.RefreshComponent(name);
                        break;
                    case CachedComponentType.Action:
                        new PageMapping().RefreshActions();
                        break;
                    case CachedComponentType.Settings:
                        // Not actions now
                        break;
                }
            }
        }

        [OperationContract]
        public virtual ResultStatus GetAvailableServices(out CodeBehindDescriptor[] services)
        {
            services = null;
            try
            {
                services = typeof(Camstar.WCF.ObjectStack.Service).Assembly.GetTypes()
                    .Where(type => type.Namespace == "Camstar.WCF.Services" && type.Name.EndsWith("Service") && !type.IsInterface)
                    .Select(item => new CodeBehindDescriptor()
                    {
                        ClassName = item.Name.Remove(item.Name.Length - "Service".Length), Description = item.BaseType.Name.Remove(item.BaseType.Name.Length - "Service".Length)
                    })
                    .OrderBy(i => i.ClassName).ToArray();
            }
            catch (Exception e)
            {
                return new ResultStatus(e.ToString(), false);
            }

            return new ResultStatus("", true);
        }

        [OperationContract]
        public virtual ResultStatus GetAvailableMaintServices(out CodeBehindDescriptor[] services)
        {
            services = null;
            try
            {
                services = typeof(Camstar.WCF.ObjectStack.Service).Assembly.GetTypes().Where(type => type.Namespace == "Camstar.WCF.Services" && type.Name.EndsWith("Service") && !type.IsInterface && type.IsSubclassOf(typeof(Camstar.WCF.Services.MaintenanceBase))).Select(item => new CodeBehindDescriptor() { ClassName = item.Name.Remove(item.Name.Length - "Service".Length), Description = item.BaseType.Name.Remove(item.BaseType.Name.Length - "Service".Length) }).OrderBy(i => i.ClassName).ToArray();
            }
            catch (Exception e)
            {
                return new ResultStatus(e.ToString(), false);
            }

            return new ResultStatus("", true);
        }

        [OperationContract]
        public virtual ResultStatus GetExportableMaintServices(out string[] services)
        {
            services = null;
            try
            {
                var session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
                var export = new Camstar.WCF.Services.ExportService(session.CurrentUserProfile);

                var data = new Export {Details = new ExportImportItem[1]};
                data.Details[0] = new ExportImportItem();

                var request = new Camstar.WCF.Services.Export_Request
                    {
                        Info = new Export_Info {Details = new ExportImportItem_Info {ObjectTypeName = new Info(false, true)}}
                    };

                Camstar.WCF.Services.Export_Result response;
                var result = export.GetEnvironment(data, request, out response);
                if (result.IsSuccess && response != null && response.Environment != null && response.Environment.Details != null)
                {
                    var details = response.Environment.Details;
                    if (details.ObjectTypeName != null && details.ObjectTypeName.SelectionValues != null)
                    {
                        var cdoList = details.ObjectTypeName.SelectionValues;
                        if (cdoList != null)
                        {
                            var exportList = new List<string>(cdoList.Rows.Length);
                            foreach (var row in cdoList.Rows)
                            {
                                var values = row.Values;
                                bool isAbstract = bool.Parse(values[3]);
                                if (!isAbstract)
                                {
                                    exportList.Add(values[1]);
                                }
                            }
                            services = exportList.ToArray();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                return new ResultStatus(e.ToString(), false);
            }

            return new ResultStatus("", true);
        }

        [OperationContract]
        public virtual ResultStatus GetSessionDataContract(out UIComponentDataContract contract)
        {
            contract = null;
            try
            {
                contract = HttpContext.Current.Session[Camstar.WebPortal.Constants.SessionConstants.SessionDataContract] as UIComponentDataContract;
            }
            catch (Exception e)
            {
                return new ResultStatus(e.ToString(), false);
            }

            return new ResultStatus("", true);
        }

        [OperationContract]
        public virtual ResultStatus GetAvailableWebPartsConnections(string fromWebPartTypeName, string toWebPartTypeName, out string[] connections)
        {
            connections = null;
            try
            {
                Type wpFromType = Type.GetType(fromWebPartTypeName, false);
                Type wpToType = Type.GetType(toWebPartTypeName, false);
                if (wpFromType != null && wpToType != null)
                {
                    var providers = wpFromType.GetMethods(BindingFlags.Instance | BindingFlags.Public).Where(m => CheckAttribute(m, typeof(CamstarConnectionProviderAttribute)));
                    var consumers = wpToType.GetMethods(BindingFlags.Instance | BindingFlags.Public).Where(m => CheckAttribute(m, typeof(CamstarConnectionConsumerAttribute)));

                    var fromConnections = providers.SelectMany(p => p.GetCustomAttributes(true)).Cast<CamstarConnectionProviderAttribute>().Select(a => a.ID);
                    var toConnections = consumers.SelectMany(p => p.GetCustomAttributes(true)).Cast<CamstarConnectionConsumerAttribute>().Select(a => a.ID);

                    connections = fromConnections.Intersect(toConnections).ToArray();
                }
            }
            catch (Exception e)
            {
                return new ResultStatus(e.ToString(), false);
            }

            return new ResultStatus("", true);
        }

        protected virtual bool CheckAttribute(MethodInfo method, Type attrType)
        {
            bool special =
                method.Name.StartsWith("add_", false, null) ||
                method.Name.StartsWith("remove_", false, null) ||
                method.Name.StartsWith("get_", false, null) ||
                method.Name.StartsWith("set_", false, null);
            bool qualified = !special;
            if (qualified)
            {
                var attrs = method.GetCustomAttributes(true).Where(a => attrType.IsAssignableFrom(a.GetType()));
                qualified = (attrs.Any());
            }
            return qualified;
        }

        [OperationContract]
        public virtual TimeSpan KeepAlive()
        {
            return (System.Web.Configuration.WebConfigurationManager.GetSection("system.web/sessionState") as System.Web.Configuration.SessionStateSection).Timeout;
        }

        [OperationContract]
        public virtual ResultStatus GetUserQueryParameters(string queryName, out CRParameter[] parameters)
        {
            ResultStatus res = new ResultStatus();
            parameters = null;
            if (IsSessionValid())
                res = PIMetric.GetParameters(queryName, out parameters);
            return res;
        }

        [OperationContract]
        public virtual ResultStatus GetUserQueryColumns(string queryName, CRParameter[] parameters, out string[] columns)
        {
            var res = new ResultStatus();
            columns = null;
            if (IsSessionValid())
                res = PIMetric.GetQueryColumns(queryName, parameters, out columns);
            return res;
        }

        [OperationContract]
        public virtual ResultStatus GetWebiReportParameters(ConnectionProfileData profile, string reportName, out CRParameter[] parameters)
        {
            bool status = false;
            string message = string.Empty;
            parameters = null;
            if (IsSessionValid())
            {
                try
                {
                    var currentUserProfile = FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile;
                    if (profile != null)
                    {
                        profile.UserName = currentUserProfile.Name;
                        var currentUserPassword = HttpContext.Current.Session[Camstar.WebPortal.Constants.SessionConstants.UserPassword] as EncryptedField;
                        if (currentUserPassword != null)
                            profile.Password = EncryptedField.GetPlainValue(currentUserPassword);
                    }
                    
                    var webi = new CamstarPortal.WebControls.WebiReportSession(profile);
                    parameters = webi.GetReportParameters(reportName);
                    webi.Dispose();
                    status = true;
                }
                catch (Exception e)
                {
                    message = "BO Server: " + e.Message;
                }
            }

            return new ResultStatus(message, status);
        }

        [OperationContract]
        public virtual ResultStatus GetWebiReportsList(ConnectionProfileData profile, out string[] reports)
        {
            bool status = false;
            string message = string.Empty;
            reports = null;
            if (IsSessionValid())
            {
                try
                {
                    var currentUserProfile = FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile;
                    if (profile != null)
                    {
                        profile.UserName = currentUserProfile.Name;
                        var currentUserPassword = HttpContext.Current.Session[Camstar.WebPortal.Constants.SessionConstants.UserPassword] as EncryptedField;
                        if (currentUserPassword != null)
                            profile.Password = EncryptedField.GetPlainValue(currentUserPassword);
                    }
                    var webi = new CamstarPortal.WebControls.WebiReportSession(profile);
                    reports = webi.GetReportsList();
                    webi.Dispose();
                    status = true;
                }
                catch (Exception e)
                {
                    message = "BO Server: " + e.Message;
                }
            }

            return new ResultStatus(message, status);
        }

        [OperationContract]
        public virtual ResultStatus GetControlsLibrary(out string controls)
        {
            controls = string.Empty;
            try
            {
                string path = HttpContext.Current.Server.MapPath("~/App_Data/ControlsLibrary.xml");
                controls = File.ReadAllText(path);
            }
            catch (Exception e)
            {
                return new ResultStatus(e.ToString(), false);
            }

            return new ResultStatus("", true);
        }

        [OperationContract]
        public virtual ControlModel[] GetControls()
        {
            string path = HttpContext.Current.Server.MapPath("~/App_Data/ControlsLibrary.xml");
            ControlModel[] controls = null;

            var xdoc = XDocument.Load(path);
            controls =
                (from ctl in xdoc.Descendants("Control")
                select new ControlModel(ctl)
                ).OrderBy(control => control.name).ToArray();
            return controls;
        }

        [OperationContract]
        public virtual ResultStatus GetFieldsDirectoryLinks(out FieldsDirectoryLink[] links)
        {
            bool status = false;
            string message = string.Empty;
            links = null;

            if (IsSessionValid())
            {
                try
                {
                    var osr = ObjectsStackReflector.GetInstance(null as Type);
                    links = osr.GetFieldsDirectoryLinks();

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
        public virtual ResultStatus UpdateFieldsDirectoryLinks(FieldsDirectoryLink[] links)
        {
            bool status = false;
            string message = string.Empty;

            if (IsSessionValid())
            {
                try
                {
                    var osr = ObjectsStackReflector.GetInstance(null as Type);
                    osr.UpdateFieldsDirectoryLinks(links);

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
        public virtual ResultStatus GetFieldMetadata(string fieldExpression, out OMMetadata metadata)
        {
            bool status = false;
            string message = string.Empty;
            metadata = null;

            if (IsSessionValid())
            {
                try
                {
                    if (WCFObject.IsFieldExist(fieldExpression))
                    {
                        metadata = ObjectsStackReflector.CreateMetadata(fieldExpression, false);
                    }
                    else
                    {
                        throw new Exception(string.Format("Field expression '{0}' is not valid.", fieldExpression));
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
        public virtual ResultStatus ConvertASPXtoPersonalization(string fileName, string aspxMarkup, string aspxCode, out string pageName)
        {
            bool status = false;
            string message = string.Empty;
            pageName = string.Empty;

            if (IsSessionValid())
            {
                try
                {
                    var converter = new ASPXConverter();
                    pageName = converter.Convert(fileName, aspxMarkup, aspxCode);

                    status = true;
                    message = string.Format("ASPX page {0} converted successfully", fileName);
                }
                catch (Exception e)
                {
                    message = e.Message;
                }
            }

            return new ResultStatus(message, status);
        }

        [OperationContract]
        public virtual ResultStatus GetActiveWorkspaces(out WorkspaceItem[] workspaces)
        {
            bool status = false;
            string message = string.Empty;
            workspaces = null;

            if (IsSessionValid())
            {
                try
                {
                    workspaces = WorkspacesUtil.Workspaces;
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
        public virtual ResultStatus CreatePagePermissions(string pageName, string defaultPageRole)
        {
            var createPermission = CamstarPortalSection.Settings.DefaultSettings.AutoPagePermission;
            if (!createPermission)
            {
                return new ResultStatus(string.Empty, true);
            }

            var session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);

            // Find page object id
            UIVirtualPageMaint_Result res;
            var pageMaintStatus = new UIVirtualPageMaintService(session.CurrentUserProfile).Load(
                new UIVirtualPageMaint { ObjectToChange = new NamedObjectRef(pageName) },
                new UIVirtualPageMaint_Request
                {
                    Info = new UIVirtualPageMaint_Info
                    {
                        ObjectToChange = new Info(true),
                        ObjectChanges = new UIVirtualPageChanges_Info
                        {
                            WorkspacePersonalizations = new UIPersonalizationChanges_Info { WorkspaceId = new Info(true) },
                            DeveloperPersonalization = new UIPersonalizationChanges_Info { Content = new Info(true) }
                        }
                    }
                },
                out res);

            if (pageMaintStatus.IsSuccess && res.Value != null && res.Value.ObjectToChange != null && res.Value.ObjectChanges != null)
            {
                var ch = res.Value.ObjectChanges;
                if (ch.WorkspacePersonalizations != null && (ch.DeveloperPersonalization == null || (ch.DeveloperPersonalization != null && ch.DeveloperPersonalization.Content == null)) && 
                    ch.WorkspacePersonalizations.Length > 2)
                {
                    // This is not the first publishing  -- skip
                    return new ResultStatus(string.Empty, true);
                }

                var serv = new RoleMaintService(session.CurrentUserProfile);
                RoleMaint_Result roleResult;
                var state = serv.Load(new RoleMaint {ObjectToChange = new NamedObjectRef(defaultPageRole)},
                    new RoleMaint_Request
                    {
                        Info =
                            new RoleMaint_Info
                            {
                                ObjectChanges =
                                    new RoleChanges_Info
                                    {
                                        Permissions = new RolePermissionChanges_Info {Name = new Info(true)}
                                    }
                            }
                    },
                    out roleResult);

                if (state.IsSuccess && roleResult.Value != null)
                {
                    // Find whether the page already has a permission
                    if (roleResult.Value.ObjectChanges != null && roleResult.Value.ObjectChanges.Permissions != null)
                    {
                        if(roleResult.Value.ObjectChanges.Permissions.Any(p => p.Name == pageName))
                            return new ResultStatus(string.Empty, true);
                    }
                    var idString = res.Value.ObjectToChange.ID;

                    serv.BeginTransaction();
                    serv.Load(new RoleMaint {ObjectToChange = new NamedObjectRef(defaultPageRole)});
                    serv.ExecuteTransaction(new RoleMaint
                    {
                        ObjectChanges = new RoleChanges
                        {
                            Permissions = new[]
                            {
                                new RolePermissionChanges
                                {
                                    Modes = new[] {new Primitive<int>(1)},
                                    Name = new Primitive<string>(pageName),
                                    ObjectInstanceIdString = idString,
                                    PermissionType = new Enumeration<PermissionTypeEnum, int>(200)
                                }
                            }
                        }
                    });
                    return serv.CommitTransaction();
                }
                else
                {
                    return state;
                }
            }
            else
            {
                return pageMaintStatus;
            }
        }

        [OperationContract]
        public Camstar.WebPortal.PortalConfiguration.PortalSettings GetSettings()
        {
            return CamstarPortalSection.GetSettings();
        }

        [OperationContract]
        public bool VerifyRBACPermission()
        {
            var session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
            var authManager = session.GetAuthorizationManager();            
            bool isRBAC = authManager.IsAuthorized(Camstar.WebPortal.WCFUtilities.Authorization.PermissionTypeEnum.PortalConfigurationSecurity, "Portal Studio RBAC", PermissionModeEnum.Browse);
            return isRBAC;
        }



        [OperationContract]
        public string[] GetRenderOverrideRoles()
        {
            
            var data = new RoleMaint() { ObjectChanges = new RoleChanges() { PermissionType = new Enumeration<PermissionTypeEnum, int>() } };
            var info = new RoleMaint_Info() { ObjectToChange = new Info(false, true) };
            var request = new RoleMaint_Request() { Info = info };
            var session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
            var service = new RoleMaintService(session.CurrentUserProfile);
            RoleMaint_Result result;
            var rs = service.GetEnvironment(data, request, out result);

            if(rs.IsSuccess)
            {
                return result.Environment.ObjectToChange.SelectionValues.Rows.Select(s => s.Values[0]).OrderBy(obj => obj).ToArray();
            }
            else
                return new string[0];
        }

        [OperationContract]
        public QueryData[] GetLabelCategories()
        {
            if (IsSessionValid())
            {
                var session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
                if (session != null)
                {
                    var service = new DictionaryServiceService(session.CurrentUserProfile);
                    var serviceData = new DictionaryService();

                    var request = new DictionaryService_Request()
                    {
                        Info = new DictionaryService_Info()
                        {
                            LabelCategoryDetails = new LabelCategoryDetails_Info()
                            {
                                RequestSelectionValues = true
                            }
                        }
                    };

                    var result = new DictionaryService_Result();
                    var status = service.GetEnvironment(serviceData, request, out result);

                    var details = new List<QueryData>();
                    if (result != null && status.IsSuccess)
                    {
                        if (result.Environment.LabelCategoryDetails != null && result.Environment.LabelCategoryDetails.SelectionValues != null)
                        {
                            Header[] headers = result.Environment.LabelCategoryDetails.SelectionValues.Headers;
                            Row[] rows = result.Environment.LabelCategoryDetails.SelectionValues.Rows;

                            if (rows != null && headers != null)
                            {
                                var CategoryIdIndex = Array.FindIndex(headers, h => h.Name.Equals("CategoryId"));
                                var CategoryNameIndex = Array.FindIndex(headers, h => h.Name.Equals("CategoryName"));
                                var CategoryDescIndex = Array.FindIndex(headers, h => h.Name.Equals("CategoryDescription"));

                                foreach (Row row in rows)
                                {
                                    details.Add(new QueryData()
                                    {
                                        ID = row.Values[CategoryIdIndex],
                                        Text = row.Values[CategoryNameIndex],
                                        Description = row.Values[CategoryDescIndex]
                                    });
                                }
                            }
                            return details.OrderBy(d => d.Text).ToArray();
                        }
                    }
                }
            }
            return null;
        }


        [OperationContract]
        public DictionaryLabel[] GetLabels(string categoryIds, string searchLabelName, string searchLabelText)
        {
            if (IsSessionValid())
            {
                var session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
                if (session != null)
                {
                    string EmployeeName = session.CurrentUserProfile.Name;

                    int filterCategoryId = int.TryParse(categoryIds, out int categoryId) ? categoryId : 0;
                    string filterCategory = filterCategoryId == 0 ? "" : $"and C.CategoryID = '{filterCategoryId}'";
                    string labelText = searchLabelText?.Replace("'", "''");
                    string labelName = searchLabelName?.Replace("'", "''");

                    string queryText = "select T.LabelID, T.Name, T.LabelValue, T.Value, T.Category, DL.LabelValue as DictionaryValue " +
                        "from ( select L.LabelID, L.Name as Name, L.LabelValue, L.LabelValue as Value, C.Name as Category  " +
                        "from Labels L join LabelCategory C on C.CategoryID = L.CategoryID " +
                        $"where L.LabelValue like '%{labelText}%' and L.Name like '%{labelName}%' {filterCategory}" +
                        $"union select U.LabelID, U.UserLabelName as Name, U.LabelValue, U.LabelValue as Value, 'UserLabel' as Category " +
                        $"from UserLabel U where U.LabelValue like '%{labelText}%' and U.UserLabelName like '%{labelName}%')" +
                        $"T join Employee E on E.EmployeeName = '{EmployeeName}' " +
                        "left join DictionaryLabel DL on DL.LabelID = T.LabelID " +
                        "and DL.DictionaryId = E.LanguageDictionaryId order by Category, Name offset 0 rows fetch next 201 rows only";

                    var recordSet = new RecordSet();

                    var service = new QueryService(session.CurrentUserProfile);
                    var options = new QueryOptions();
                    var resultStatus = service.ExecuteAdHoc(queryText, options, out recordSet);

                    if (resultStatus.IsSuccess)
                    {
                        var headers = recordSet.Headers;
                        var selectionValues = recordSet.Rows;
                        int LabelIdIndex = 0;
                        int LabelNameIndex = 1;
                        int LabelValueIndex = 2;
                        int LabelCategoryIndex = 4;
                        int DefaultValueIndex = 3;

                        var labelList = new List<DictionaryLabel>();
                        if (selectionValues != null)
                        {
                            labelList.AddRange(
                                selectionValues.Select(
                                    v => new DictionaryLabel()
                                    {
                                        ID = Int32.Parse(v.Values[LabelIdIndex]),
                                        Name = v.Values[LabelNameIndex],
                                        Value = v.Values[LabelValueIndex],
                                        Category = v.Values[LabelCategoryIndex],
                                        DefaultValue = v.Values[DefaultValueIndex]
                                    }

                                )
                            );
                        }
                        return labelList.ToArray();
                    }
                }
            }
            return new DictionaryLabel[0];
        }

        [OperationContract]
        public ResultStatus GetQueryList(string query, out QueryData[] list)
        {
            list = new QueryData[0] { };
            var queryText = GetQueryText(query, null);
            var recordSet = new RecordSet();

            var session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
            var service = new QueryService(session.CurrentUserProfile);
            var options = new QueryOptions();
            var resultStatus = service.ExecuteAdHoc(queryText, options, out recordSet);
            if (resultStatus.IsSuccess)
                if (recordSet.Rows != null)
                {
                    var qlist = new List<QueryData>();
                    foreach (var row in recordSet.Rows.OrderBy(r=>r.Values[1]))
                    {
                        qlist.Add(new QueryData()
                        {
                            ID = row.Values[0],
                            Text = row.Values[1],
                            Description = row.Values[2],
                            ParamCount = Int32.Parse(row.Values[3])
                        });
                    }
                    list = qlist.ToArray();
                }
            return resultStatus;
        }

        [OperationContract]
        public ResultStatus GetParameterList(string query, string[] parameters, out QueryData[] list)
        {
            list = null;
            var queryText = this.GetQueryText(query, parameters);
            var recordSet = new RecordSet();

            var session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
            var service = new QueryService(session.CurrentUserProfile);
            var options = new QueryOptions();
            var resultStatus = service.ExecuteAdHoc(queryText, options, out recordSet);
            if (resultStatus.IsSuccess)
            {
                var qlist = new List<QueryData>();
                foreach (var row in recordSet.Rows)
                    qlist.Add(new QueryData()
                    {
                        Text = row.Values[0],
                        Description = row.Values[1]
                    });
                list = qlist.ToArray();
            }
            return resultStatus;
        }

        [OperationContract]
        public ResultStatus GetAdHocQueryColumnsList(string query, QueryOptions options, out ColumnData[] list)
        {
            list = null;
            var recordSet = new RecordSet();        
            var session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
            var service = new QueryService(session.CurrentUserProfile);

            var resultStatus = service.ExecuteAdHoc(query, options, out recordSet);
            if (resultStatus.IsSuccess)
            {
                var clist = new List<ColumnData>();
                foreach (var h in recordSet.Headers.OrderBy(h => h.Name))
                    clist.Add(new ColumnData()
                    {
                        Name = h.Name,
                        LabelName = h.Label.Name,
                        TypeCode = h.TypeCode.ToString()
                    });
                list = clist.ToArray();
            }
            return resultStatus;
        }

        [OperationContract]
        public ResultStatus GetQueryColumnsList(string query, QueryOptions options, QueryParameter[] parameters, out ColumnData[] list)
        {
            list = null;
            if (options.QueryType == null)
            {
                options = new QueryOptions { QueryType = Camstar.WCF.ObjectStack.QueryType.System, ChangeCount = 0, StartRow = 0, RowSetSize = -1, RequestHeadersOnly = true };
                var resultStatus = GetQueryColumns(query, options, parameters, out list);
                if (!resultStatus.IsSuccess)
                {
                    options = new QueryOptions { QueryType = Camstar.WCF.ObjectStack.QueryType.User, ChangeCount = 0, StartRow = 0, RowSetSize = -1, RequestHeadersOnly = true };
                    resultStatus = GetQueryColumns(query, options, parameters, out list);
                }
                return resultStatus;
            }
            else
                return GetQueryColumns(query, options, parameters, out list);
        }

        private ResultStatus GetQueryColumns(string query, QueryOptions options, QueryParameter[] parameters, out ColumnData[] list)
        {
            list = null;
            var recordSet = new RecordSet();
            var session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
            var service = new QueryService(session.CurrentUserProfile);
            var qParameters = new QueryParameters();
            qParameters.Parameters = parameters;

            var resultStatus = service.Execute(query, qParameters, options, out recordSet);
            if (resultStatus.IsSuccess)
            {
                var clist = new List<ColumnData>();
                foreach (var h in recordSet.Headers.OrderBy(h => h.Name))
                    clist.Add(new ColumnData()
                    {
                        Name = h.Name,
                        LabelName = h.Label.Name,
                        TypeCode = h.TypeCode.ToString()
                    });
                list = clist.ToArray();
            }
            return resultStatus;
        }

        private string GetQueryText(string queryName, string[] parameters)
        {
            var queryText = "";
            switch(queryName)
            {
                case "QueryList_System":
                    queryText = "SELECT QueryDef.QueryDefID, QueryDef.Name, QueryDef.Description, " +
                            "(SELECT COUNT(*) FROM QueryParms QP WHERE QP.QueryDefID = QueryDef.QueryDefID) ParametersCount " +
                            "FROM QueryDef QueryDef Where QueryDef.QueryTypeID=4";
                    break;     
                case "QueryList_User":
                    queryText = "Select UserQueryId, UserQueryName, UserQuery.Description, " +
                    "(SELECT COUNT(*) FROM UserQueryParameter QP WHERE QP.UserQueryId= UserQuery.UserQueryId) ParametersCount From UserQuery";
                    break;
                case "ParamList_System":
                    queryText = @"Select QueryParms.Name, CPPDataTypes.Name as DataType From QueryDef join QueryParms on QueryDef.QueryDefID = QueryParms.QueryDefID
                            left join CPPDataTypes on QueryParms.CPPDataTypeID=CPPDataTypes.DataTypeID
                            Where QueryDef.Name ='{0}'";
                    break;
                case "ParamList_User":
                    queryText =  @"Select UserQueryParameter.UserQueryParameterName, CPPDataTypes.Name as DataType
                            From UserQuery join UserQueryParameter on UserQuery.UserQueryId = UserQueryParameter.UserQueryId
                            left join CPPDataTypes on UserQueryParameter.DataType=CPPDataTypes.DataTypeID
                            Where UserQuery.UserQueryName ='{0}'";
                    break;
            }
            //interate and insert parameters
            if(queryText.Length > 0 && parameters!=null)
            {
                for(var i =0; i< parameters.Count(); i++)                    
                    queryText = queryText.Replace('{' + i.ToString() + '}', parameters[i]);
            }
            return queryText;
        }

        [OperationContract]
        public ResultStatus SaveSettings(Camstar.WebPortal.PortalConfiguration.PortalSettings settings)
        {

            // encrypt the password
            if (settings.IntelligenceSettings.DataSources != null)
                settings.IntelligenceSettings.DataSources.ToList().ForEach(ds =>
                {
                    if (!string.IsNullOrEmpty(ds.LoginPasswordDecrypted))
                    {
                        ds.LoginPassword = Camstar.Util.CryptUtil.Encrypt(ds.LoginPasswordDecrypted);
                        ds.LoginPasswordDecrypted = null;
                    }
                });

            CamstarPortalSection.Settings = settings;

            CamstarPortalSection.SaveSettings();

            return new ResultStatus("", true);
        }

        [OperationContract]
        public ResultStatus GetComponent(UserProfile userProfile, RequestComponent request, out string model )
        {
            model = null;

            if (string.IsNullOrEmpty(request.componentName))
            {
                return new ResultStatus("name is empty", false);
            }
            if (string.IsNullOrEmpty(request.componentType))
            {
                return new ResultStatus("type is empty", false);
            }
            if (userProfile == null)
            {
                return new ResultStatus("profile is empty", false);
            }
            var eng = new ComponentEngine(userProfile);

            ResultStatus res;
            UIComponentModel uiModel = null;
            if (request.componentType == "page")
            {
                uiModel = eng.LoadPage(request.componentName, false, out res, true);
            }
            else if (request.componentType == "webpart")
            {
                WebPartModel wpModel;
                res = eng.LoadWebPart(request.componentName, out wpModel);
                if (res.IsSuccess)
                {
                    uiModel = wpModel;
                }
            }
            else
            {
                res = new ResultStatus("Unknown component type", false);
            }
            if (uiModel != null)
            {
                using (var stringWriter = new StringWriter())
                {
                    using (var writer = new JsonTextWriter(stringWriter) { QuoteName = true})
                    {
                        var ser = new JsonSerializer();
                        ser.ContractResolver = OMContractResolver.Instance;
                        ser.Serialize(writer, uiModel);
                    }
                    model = stringWriter.ToString();
                }
            }
            return res;
        }

        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped, 
            UriTemplate = "/component/{op}")]     
        public ApiResult ComponentOperation(string op, string name, string type, string workspace = "", object content = null)
        {
            var result = new ApiResult();
            if(!IsSessionValid())
            {
                result.status = new ResultStatus("Session is expired or not valid.", false);
                result.content = null;
                return result;
            }

            var rep = GetRepository(type, workspace);
            var component = new PS.Models.Component(rep);
            object obj = null;

            switch (op)
            {
                case "get":
                    obj = component.Get(name);
                    result.status = rep.GetLastError();
                    break;
                case "delete":
                    result.status = component.Delete(name);
                    break;
                case "publish":
                    result.status = component.Publish(content as string);
                    break;
                case "revert":
                    result.status = component.Revert(name, content as string);
                    break;
                case "copy":
                    obj = component.Copy(name, content as string);
                    result.status = rep.GetLastError();
                    break;
                case "save":
                    result.status = component.Save(content as string);
                    break;
                case "export":
                    obj = component.Get(name);
                    result.status = rep.GetLastError();              
                    if (obj != null)
                    {
                        string xml;
                        component.ExportComponent(name, workspace, false, out xml);
                        result.content = xml;
                        return result;
                    }
                    break;
                case "toxml":
                    {
                        bool isNew = (content as string) == "true";
                        result.status = component.ConvertToXml(name, workspace, isNew, out string xml);
                        obj = xml;

                    }
                    break;
                case "applyxml":
                    obj = component.ApplyXml(name, content as string);
                    result.status = new ResultStatus("xml applied", true);
                    break;
                case "isused":
                    obj = component.IsWebPartUsed(name, content as string);
                    result.status = rep.GetLastError();
                    if (obj == null)
                    {
                        obj = "-";
                    }
                    break;
            }
            if ( obj != null)
            result.content = PS.Models.ComponentSerializer.Serialize(obj);
            
            if (result.status == null)
                result.status = new ResultStatus("", true);
            return result;
        }

        [OperationContract]
        public ApiResult ImportComponent(string type, string name, bool isDev, byte[] xmlData)
        {
            var result = new ApiResult();
            var rep = GetRepository(type, "");
            var component = new PS.Models.Component(rep);
            string xml = null;

            if (xmlData[5] == 0 && xmlData[7] == 0 && xmlData[9] == 0 && xmlData[11] == 0 && xmlData[13] == 0)
            {
                xml = Encoding.Unicode.GetString(xmlData);
            }
            else
            {
                xml = Encoding.UTF8.GetString(xmlData);
            }

            if (xml.StartsWith("<?xml"))
            {
                var iLast = xml.IndexOf("?>");
                if (iLast != -1)
                {
                    xml = xml.Substring(iLast + 2);
                }
            }

            bool isWebPart = xml.IndexOf("<WebPartModel", 1, 250) != -1;
            bool isPage = xml.IndexOf("<PageModel", 1, 250) != -1;
            object viewModel = null;
            ResultStatus errStatus = null;

            if (isWebPart && type == "Webpart")
            {
                viewModel = component.Import(name, xml, isDev);
                errStatus = rep.GetLastError();
            }
            else if (isPage && type == "Page")
            {
                viewModel = component.Import(name, xml, isDev);
                errStatus = rep.GetLastError();
            }
            else
            {
                errStatus = new ResultStatus("Incorrect type of the import file", false);
            }

            result.content = PS.Models.ComponentSerializer.Serialize(viewModel);

            if (errStatus != null && !errStatus.IsSuccess)
                result.status = errStatus;

            return result;
        }

        [OperationContract]
        public _uiComponentRef_[] GetInstanceReferences(string cdoName)
        {
            var session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
            var service = new CDOInquiryService(session.CurrentUserProfile);

            var cdo = new CDOInquiry { CDODefName = new Enumeration<MaintainableObjectEnum, string>(cdoName) };
            var request = new CDOInquiry_Request { Info = new CDOInquiry_Info { CDOInstances = new Info(false, true) } };
            CDOInquiry_Result result;
            var status = service.GetEnvironment(cdo, request, out result);
            if (status.IsSuccess && result.Environment != null && result.Environment.CDOInstances != null && result.Environment.CDOInstances.SelectionValues != null && result.Environment.CDOInstances.SelectionValues.Rows != null)
            {
                var resultlist = result.Environment.CDOInstances.SelectionValues;

                var uiComponentReferences =
                    from r in resultlist.Rows
                    let v = r.Values
                    select new _uiComponentRef_
                    {
                        name = v[0],
                        type = cdoName
                    };

                return uiComponentReferences.ToArray();
            }

            return new _uiComponentRef_[] { };
        }


        [OperationContract]
        public byte[] BatchExport(PS.Helpers.BatchExportImport.ImportExportSettings settings)
        {
            byte[] res = null;
            byte[] respContentArray = null;

            var result = new ResultStatus("Batch export was finished correctly.", true);
            try
            {
                var session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
                settings.UserProfile = session.CurrentUserProfile;
                var batchExport = new PS.Helpers.BatchExportImport.BatchExportProcessor(settings);
                respContentArray = batchExport.Run(out result);
                if (result.IsSuccess)
                    result.Message = "Export completed";
                //result.Message = $"Export completed on {DateTime.Now} by {settings.UserProfile.Name}";
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.Message = "Batch export failed. " + ex.Message; //   $"Batch export failed. {ex.Message}";
            }

            byte[] resMessageBytes = System.Text.Encoding.UTF8.GetBytes(result.Message);
            var preamble = (result.IsSuccess ? "1" : "0") +
                    result.Message.Length.ToString("x4") +
                    (respContentArray != null ? respContentArray.LongLength.ToString("x8") : "00000000") +
                    result.Message;
            using (var outByteStream = new MemoryStream())
            {
                var preambleBytes = ASCIIEncoding.ASCII.GetBytes(preamble);
                outByteStream.Write(preambleBytes, 0, preambleBytes.Length);
                // Write data
                if( respContentArray != null)
                    outByteStream.Write(respContentArray, 0, respContentArray.Length);

                res = outByteStream.ToArray();
            }

            return res;
        }

        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, 
            RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
        public ResultStatus BatchImport(Stream stream)
        {
            var result = new ResultStatus("Batch export was finished correctly.", true);

            var settings = new PS.Helpers.BatchExportImport.ImportExportSettings()
            {
                UserProfile = GetProfile()
            };

            var fileName = "";
            var fileLen = 0;

            var headers = GetHeadersString(stream);
            headers.ForEach(s => 
            {
                if (s.StartsWith("Content-Disposition:"))
                {
                    var ii = s.IndexOf("filename=");
                    if (ii != -1)
                    {
                        var opts = s.Substring(ii + "filename=".Length + 1).TrimEnd('"');
                        var optsParts = opts.Split(',');
                        fileName = optsParts[0];
                        settings.SelectedWorkspaces = optsParts.Skip(2).ToArray();
                        settings.OverrideIfExists = optsParts[1] == "true";
                        fileLen = int.Parse(optsParts[optsParts.Length - 1]);
                    }
                }

                else if (s.StartsWith("Content-Type:"))
                {
                    // Can be:
                    //  application/x-zip-compressed
                    //  text/xml
                    var contentType = s.Substring("Content-Type:".Length + 1);
                    settings.ImportFrom = contentType == "text/xml" 
                            ? PS.Helpers.BatchExportImport.ImportExportSettings.ExportImportType.File
                            : PS.Helpers.BatchExportImport.ImportExportSettings.ExportImportType.MultipleFiles;
                }
            });

            // input bytes are written into the buffer with file length (otherwise there are some garbage at the end)
            var buf = new byte[fileLen];
            stream.Read(buf, 0, fileLen);
            var memStream = new MemoryStream(buf);

            memStream.Seek(0, SeekOrigin.Begin);
            settings.PostedFile = memStream;

            var importProcessor = new PS.Helpers.BatchExportImport.BatchImportProcessor(settings);
            try
            {
                 result = importProcessor.Run();
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.Message =  "Batch import failed. " + ex.Message;
            }
            if (result.IsSuccess)
                //result.Message = $"Import completed on {DateTime.Now} by {settings.UserProfile.Name}";
                result.Message = "Import completed";

            return result;
        }


        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped,
            UriTemplate = "/metadata/{op}")]
        public ApiResult MetadataOperation(string op, string[] inpParams = null)
        {
            var result = new ApiResult();
            JsonSerializerSettings settings = null;
            object outputObj = null;
            switch (op)
            {
                case "get":
                    PS.Models.ViewModelRules.Load(HttpContext.Current.Server.MapPath("~/PortalStudio/Content/metadata/ViewModelRules.json"));
                    outputObj = PS.Models.PersonalizationMetadata.GetViewModelMetadata();
                    settings = new JsonSerializerSettings
                    {
                        ContractResolver = new DefaultContractResolver() { NamingStrategy = LowerCaseKeyNamingStrategy.Instance }
                    };
                    break;

                case "types":
                    settings = new JsonSerializerSettings
                    {
                        ContractResolver = new DefaultContractResolver() { NamingStrategy = LowerCaseKeyNamingStrategy.Instance }
                    };
                    outputObj = PS.Models.PersonalizationMetadata.GetMetadataTypes(inpParams[0], inpParams.Skip(1).ToArray());
                    if( inpParams[1] == "PortalSettings")
                    {
                        var isRBAC_Hidden =
                            (CamstarPortalSection.Settings.DefaultSettings.RoleBasedAccess) ? !IsInRole() : false; 

                        var stypes = outputObj as Dictionary<string, PS.Models.ViewModelTypeDefBase>;
                        var defSection = (stypes.ContainsKey("DefaultSettings") ? stypes["DefaultSettings"] : null) as PS.Models.ViewModelObjectTypeDef;
                        var rba = defSection?.Properties?.FirstOrDefault(p => p.Name == "RoleBasedAccess");
                        if(rba != null)
                            rba.Hidden = isRBAC_Hidden;
                    }
                    break;

                case "workspaces":
                    outputObj = WorkspacesUtil.Workspaces;
                    break;

                case "descriptions":
                    var xdoc = XDocument.Load(HttpContext.Current.Server.MapPath("~/App_Data/Descriptions.xml"));
                    outputObj = new PropertyDescriptions(xdoc.Descendants("PropertyDescriptions").First());
                    settings = new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() };
                    break;
            }

            if (outputObj != null)
                result.content = JsonConvert.SerializeObject(outputObj, settings);

            return result;
        }

        private bool IsInRole()
        {
            var isInRole = false;
            var session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
            string queryString = String.Format("SELECT count(*) Result FROM EmployeeRole er join Employee e on e.EmployeeId = er.EmployeeId join RolePermission rp on rp.RoleId = er.RoleId where e.EmployeeName = '{0}' and rp.PermissionType = '210' and rp.ObjectMetaId = '3'", session.CurrentUserProfile.Name);
            var qs = new QueryService(session.CurrentUserProfile);
            RecordSet rs;
            var res = qs.ExecuteAdHoc(queryString, new QueryOptions { StartRow = 1, RowSetSize = 1, QueryType = Camstar.WCF.ObjectStack.QueryType.User }, out rs);
            if (res.IsSuccess)
            {
                isInRole = Convert.ToInt32(rs.Rows[0].Values[0]) > 0;
            }
            return isInRole;
        }

        private PS.Repositories.IComponentRepository GetRepository(string type, string workspace = "")
        { 
            var session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
            return PS.Repositories.ComponentRepository.GetRepositoryByType(type, session.CurrentUserProfile, workspace);
        }

        private UserProfile GetProfile()
        {
            var session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
            return session.CurrentUserProfile;
        }

        private List<string> GetHeadersString(Stream stream)
        {
            byte[] hb = new byte[1];
            char c;
            char cPrev = ' ';
            string s = "";
            var res = new List<string>();
            while (true)
            {
                stream.Read(hb, 0, 1);
                c = UTF8Encoding.UTF8.GetChars(hb, 0, 1)[0];
                if (c == '\n' && cPrev == '\r')
                {
                    if (s.Length == 0)
                        break;
                    res.Add(s);
                    s = "";
                }
                else if (c != '\r')
                {
                    s += c;
                }
                cPrev = c;
            }
            return res;
        }

        public class _uiComponentRef_
        {
            public string name { get; set; }
            public string type { get; set; }
        }

        private string _fileLocation = string.Empty;
        private string mkDefaultFileLocationValue = "~/bin";
        private JsonSerializerSettings jsonSettings = new JsonSerializerSettings() { ContractResolver = OMContractResolver.Instance };

        public class OMContractResolver : DefaultContractResolver
        {
            public static readonly OMContractResolver Instance = new OMContractResolver();

            protected override List<MemberInfo> GetSerializableMembers(Type objectType)
            {
                return base.GetSerializableMembers(objectType).Where(
                    m =>
                        {
                            var ignoreAttr = m.GetCustomAttribute<JsonIgnoreAttribute>();
                            //if(ignoreAttr != null )
                            //    System.Diagnostics.Debug.WriteLine(m.Name + " ignore");
                            return ignoreAttr == null;
                        }
                ).ToList();
            }

            protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
            {
                memberSerialization = MemberSerialization.OptOut;
                return base.CreateProperties(type, memberSerialization);
            }
        }

        public class LowerCaseKeyNamingStrategy : CamelCaseNamingStrategy
        {
            public static readonly LowerCaseKeyNamingStrategy Instance = new LowerCaseKeyNamingStrategy();
            public override string GetDictionaryKey(string key)
            {
                return base.GetDictionaryKey(key).ToLower();
            }
        }

        public class RequestData
        {

        }

        public class RequestComponent
        {
            public string componentName { get; set; }
            public string componentType { get; set; }

        }

        public class JsonRequest
        {
            public UserProfile userProfile { get; set; }
            public RequestComponent request {get; set;}
        }

        public class ControlModel
        {
            public ControlModel() { }

            public ControlModel(XElement xctl)
            {
                name = xctl.Element("Name").Value;
                description = xctl.Element("Description").Value;
                toolboxImage = xctl.Element("ToolboxImage").Value;
                designImage = xctl.Element("DesignImage").Value;
                type = xctl.Element("Type").Value;
                assembly = xctl.Element("Assembly").Value;
                designType = xctl.Element("DesignType").Value;
            }

            public string name { get; set; }
            public string description { get; set; }
            public string toolboxImage { get; set; }
            public string designImage { get; set; }

            public string designType { get; set; }
            public string type { get; set; }
            public string assembly { get; set; }

            public string typeName
            {
                get { return string.Format("{0}.{1}", assembly, type); }
            }
        }

        internal class PropertyDescription : Dictionary<string, string>
        {
            internal PropertyDescription(XElement el) : base()
            {
                foreach (var c in el.Descendants("Property"))
                {
                    var name = (c as XElement).Attribute("Name").Value;
                    var text = (c as XElement).Value;
                    this[name] = text;
                }
            }
        }

        internal class PropertyDescriptions
        {
            internal PropertyDescriptions(XElement element)
            {
                GeneralControl = new PropertyDescription(element.Element("GeneralControl"));
                PageContent = new PropertyDescription(element.Element("PageContent"));
                WebPartDefinition = new PropertyDescription(element.Element("WebPartDefinition"));
                PageflowContent = new PropertyDescription(element.Element("PageflowContent"));
            }
            public PropertyDescription GeneralControl { get; set; }
            public PropertyDescription PageContent { get; set; }
            public PropertyDescription WebPartDefinition { get; set; }
            public PropertyDescription PageflowContent { get; set; }
        }

        public class ApiResult
        {
            public ResultStatus status { get; set; }
            public string content { get; set; }

            public ApiResult()
            {
                status = new ResultStatus("", true);
                content = null;
            }
        }

    }

}
