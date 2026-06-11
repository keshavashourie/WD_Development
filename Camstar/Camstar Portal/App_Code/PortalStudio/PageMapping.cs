// Copyright Siemens 2023  
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Camstar.WCF.ObjectStack;
using System.IO;
using Camstar.WebPortal.Personalization;
using System.ServiceModel;

namespace WebClientPortal
{
    /// <summary>
    /// Summary description for PageMapping
    /// </summary>
    public partial class PortalStudioService
    {
        static object SyncObject = new object();
        
        [OperationContract]
        public virtual ResultStatus LoadPageMapping(out string xml)
        {
            bool status = false;
            string message = string.Empty;
            xml = null;

            if (IsSessionValid())
            {
                try
                {
                    if (PageMappingFile != null)
                        xml = File.ReadAllText(PageMappingFile);
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
        public virtual ResultStatus LoadPageItem(string name, out string xml)
        {
            bool status = false;
            string message = string.Empty;
            xml = null;

            if (IsSessionValid())
            {
                try
                {
                    if (PageMappingFile != null)
                    {
                        System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(typeof(PageMappingItem[]));
                        Stream rd = File.OpenRead(PageMappingFile);
                        PageMappingItem[] items = serializer.Deserialize(rd) as PageMappingItem[];
                        rd.Close();
                        if (items != null)
                        {
                            PageMappingItem item = items.SingleOrDefault(i => i.Name == name);
                            if (item != null)
                            {
                                serializer = new System.Xml.Serialization.XmlSerializer(typeof(PageMappingItem));
                                System.Text.StringBuilder builder = new System.Text.StringBuilder();
                                TextWriter wr = new StringWriter(builder);
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
        public virtual ResultStatus LoadPageMapItem(string name, out PageMappingItem pageMap)
        {
            bool status = false;
            string message = string.Empty;
            pageMap = null;

            if (IsSessionValid())
            {
                System.IO.Stream rd = null;
                try
                {
                    if (PageMappingFile != null)
                    {
                        System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(typeof(PageMappingItem[]));
                        rd = File.OpenRead(PageMappingFile);
                        var items = serializer.Deserialize(rd) as PageMappingItem[];
                        rd.Close();
                        if (items != null)
                        {
                            pageMap = items.SingleOrDefault(i => i.Name == name);
                        }
                    }

                    status = true;
                }
                catch (Exception e)
                {
                    if (rd != null)
                        rd.Close();
                    message = e.Message;
                }
            }
            return new ResultStatus(message, status);
        }

        [OperationContract]
        public virtual ResultStatus DeletePageItem(string name)
        {
            bool status = false;
            string message = string.Empty;

            if (IsSessionValid())
            {
                Stream rd = null;
                try
                {
                    if (PageMappingFile != null)
                    {
                        lock (SyncObject)
                        {
                            System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(typeof(PageMappingItem[]));
                            rd = File.OpenRead(PageMappingFile);
                            PageMappingItem[] items = serializer.Deserialize(rd) as PageMappingItem[];
                            rd.Close();
                            items = items.Where(i => i.Name != name).ToArray();
                            Stream wr = File.Create(PageMappingFile);
                            serializer.Serialize(wr, items);
                            wr.Close();
                        }
                    }
                    message = string.Format("Map Item \"{0}\" is removed successfully.", name);
                    status = true;
                }
                catch (Exception e)
                {
                    message = e.Message;
                    if( rd != null)
                        rd.Close();
                }
            }

            return new ResultStatus(message, status);
        }

        [OperationContract]
        public virtual ResultStatus SavePageMapping(string name, string xml)
        {
            bool status = false;
            string message = string.Empty;

            if (IsSessionValid())
            {
                Stream rd = null;
                try
                {
                    PageMappingItem[] items = null;
                    System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(typeof(PageMappingItem));
                    TextReader trd = new StringReader(xml);
                    PageMappingItem item = serializer.Deserialize(trd) as PageMappingItem;
                    trd.Close();

                    lock (SyncObject)
                    {
                        if (PageMappingFile != null)
                        {
                            serializer = new System.Xml.Serialization.XmlSerializer(typeof(PageMappingItem[]));
                            rd = File.OpenRead(PageMappingFile);
                            items = serializer.Deserialize(rd) as PageMappingItem[];
                            rd.Close();
                        }
                        if((items ?? new PageMappingItem[0]).SingleOrDefault(i => i.Name == item.Name) != null && string.IsNullOrEmpty(name))
                            throw new Exception(string.Format("Map Item \"{0}\" already exists.", item.Name));
                        items = new[] { item }.Union((items ?? new PageMappingItem[0]).Where(i => i.Name != name)).ToArray();
                        serializer = new System.Xml.Serialization.XmlSerializer(typeof(PageMappingItem[]));
                        Stream wr = File.Create(PageMappingFile);
                        serializer.Serialize(wr, items);
                        wr.Close();
                    }

                    HttpContext.Current.Application.Lock();
                    HttpContext.Current.Application[Camstar.WebPortal.Constants.PageFlowContants.ApplicationVariableLookupKeys.PageMappingCache] = null;
                    HttpContext.Current.Application.UnLock();
                    message = "Page Mapping has been saved successfully.";
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
        public virtual ResultStatus SavePageMap(string name, PageMappingItem pageMap)
        {
            bool status = false;
            string message = string.Empty;

            if (IsSessionValid())
            {
                try
                {
                    PageMappingItem[] items = null;
                    var serializer = new System.Xml.Serialization.XmlSerializer(typeof(PageMappingItem));

                    lock (SyncObject)
                    {
                        if (PageMappingFile != null)
                        {
                            serializer = new System.Xml.Serialization.XmlSerializer(typeof(PageMappingItem[]));
                            var rd = File.OpenRead(PageMappingFile);
                            items = serializer.Deserialize(rd) as PageMappingItem[];
                            rd.Close();
                        }
                        if ((items ?? new PageMappingItem[0]).SingleOrDefault(i => i.Name == pageMap.Name) != null && string.IsNullOrEmpty(name))
                            throw new Exception(string.Format("Map Item \"{0}\" already exists.", pageMap.Name));
                        items = new[] { pageMap }.Union((items ?? new PageMappingItem[0]).Where(i => i.Name != name)).ToArray();
                        serializer = new System.Xml.Serialization.XmlSerializer(typeof(PageMappingItem[]));
                        Stream wr = File.Create(PageMappingFile);
                        serializer.Serialize(wr, items);
                        wr.Close();
                    }

                    HttpContext.Current.Application.Lock();
                    HttpContext.Current.Application[Camstar.WebPortal.Constants.PageFlowContants.ApplicationVariableLookupKeys.PageMappingCache] = null;
                    HttpContext.Current.Application.UnLock();
                    message = "Page Mapping has been saved successfully.";
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
        public virtual ResultStatus GetSuggestedMappingNames(out string[] names)
        {
            bool status = false;
            string message = string.Empty;
            names = null;

            if (IsSessionValid())
            {
                try
                {
                    names = Enum.GetNames(typeof(ConciergePageMappingEnum));
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
        public virtual ResultStatus GetSuggestedMappingParameters(out string[] parameters)
        {
            bool status = false;
            string message = string.Empty;
            parameters = null;

            if (IsSessionValid())
            {
                try
                {
                    parameters = typeof(ToDoListDetail_Info).GetProperties(System.Reflection.BindingFlags.DeclaredOnly
                        | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public).Select(p => p.Name).ToArray();
                    status = true;
                }
                catch (Exception e)
                {
                    message = e.Message;
                }
            }

            return new ResultStatus(message, status);
        }

        private string PageMappingFile
        {
            get
            {
                var f = HttpContext.Current.Server.MapPath("PageMapping.xml");
                return File.Exists(f) ? f : null;
            }
        }
    }
}
