// Copyright Siemens 2023  
using System;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Routing;
using System.ServiceModel.Configuration;
using System.IO;
using System.Xml;
using System.ServiceModel.Channels;
using System.Configuration;
using System.Xml.XPath;
using Camstar.WCF.ObjectStack;
using Camstar.WebPortal.FormsFramework.Utilities;
using System.Xml.Serialization;
using System.ComponentModel;


namespace WebClientPortal
{
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class SilverlightRoutingService : IRequestReplyRouter
    {
        [XmlIgnore]
        public virtual TimeSpan OffSet
        {
            get; 
            set; 
        }

        // XmlSerializer does not support TimeSpan, so use this property for 
        // serialization instead.
        [Browsable(false)]
        [XmlElement(DataType = "duration", ElementName = "OffSet")]
        public virtual string OffSetString
        {
            get
            {
                return XmlConvert.ToString(OffSet);
            }
            set
            {
                OffSet = string.IsNullOrEmpty(value) ?
                    TimeSpan.Zero : XmlConvert.ToTimeSpan(value);
            }
        }

        public virtual IAsyncResult BeginProcessRequest(Message message, AsyncCallback callback, object state)
        {
            FrameworkSession session = FrameworkManagerUtil.GetFrameworkSession();
            var profile = session != null ? session.CurrentUserProfile : null;
            if (profile == null)
            {
                // Session is expired 
                return null;
            }
            ChannelEndpointElement endpoint = GetEndpoint(message.Headers.Action);
            ChannelFactory<IRequestReplyRouter> factory = new ChannelFactory<IRequestReplyRouter>(new BasicHttpBinding(endpoint.BindingConfiguration), new EndpointAddress(endpoint.Address));
            channel = factory.CreateChannel();
            MemoryStream ms = new MemoryStream();
            XmlWriter writer = XmlWriter.Create(ms);
            message.WriteMessage(writer);
            writer.Flush();
            ms.Position = 0;
            XmlDocument doc = new XmlDocument();
            doc.Load(ms);

            XPathNavigator xpath = doc.CreateNavigator();
            XPathNavigator sn = xpath.SelectSingleNode("/*[local-name()='Envelope' and namespace-uri()='http://schemas.xmlsoap.org/soap/envelope/']/*[local-name()='Body' and namespace-uri()='http://schemas.xmlsoap.org/soap/envelope/'][1]/*[namespace-uri()='http://tempuri.org/'][1]/*[local-name()='userProfile' and namespace-uri()='http://tempuri.org/'][1]/*[local-name()='Name' and namespace-uri()='Camstar.WCF.ObjectStack'][1]");
            sn.SetValue(profile.Name);
            if (profile.SessionID != null)
            {
                sn = xpath.SelectSingleNode("/*[local-name()='Envelope' and namespace-uri()='http://schemas.xmlsoap.org/soap/envelope/']/*[local-name()='Body' and namespace-uri()='http://schemas.xmlsoap.org/soap/envelope/'][1]/*[namespace-uri()='http://tempuri.org/'][1]/*[local-name()='userProfile' and namespace-uri()='http://tempuri.org/'][1]/*[local-name()='SessionID' and namespace-uri()='Camstar.WCF.ObjectStack'][1]/*[local-name()='Value' and namespace-uri()='Camstar.WCF.ObjectStack'][1]");
                sn.SetValue(profile.SessionID.Value);
                if (profile.SessionID.IsEncrypted)
                    sn.InsertElementBefore(sn.Prefix, "IsEncrypted", null, profile.SessionID.IsEncrypted.ToString().ToLower());
            }
            else
            {
                sn = xpath.SelectSingleNode("/*[local-name()='Envelope' and namespace-uri()='http://schemas.xmlsoap.org/soap/envelope/']/*[local-name()='Body' and namespace-uri()='http://schemas.xmlsoap.org/soap/envelope/'][1]/*[namespace-uri()='http://tempuri.org/'][1]/*[local-name()='userProfile' and namespace-uri()='http://tempuri.org/'][1]/*[local-name()='Password' and namespace-uri()='Camstar.WCF.ObjectStack'][1]/*[local-name()='Value' and namespace-uri()='Camstar.WCF.ObjectStack'][1]");
                sn.SetValue(profile.Password.Value);
            }

            //pass the UTC Offset
            sn = xpath.SelectSingleNode("/*[local-name()='Envelope' and namespace-uri()='http://schemas.xmlsoap.org/soap/envelope/']/*[local-name()='Body' and namespace-uri()='http://schemas.xmlsoap.org/soap/envelope/'][1]/*[namespace-uri()='http://tempuri.org/'][1]/*[local-name()='userProfile' and namespace-uri()='http://tempuri.org/'][1]/*[local-name()='UTCOffset' and namespace-uri()='Camstar.WCF.ObjectStack'][1]");
            OffSet = profile.UTCOffset;
            if (sn != null)
                sn.SetValue(OffSetString);

            ms.SetLength(0);
            writer = XmlWriter.Create(ms);
            doc.WriteTo(writer);
            writer.Flush();
            ms.Position = 0;
            XmlReader reader = XmlReader.Create(ms);
            Message originalMessage = message;
            message = Message.CreateMessage(reader, int.MaxValue, message.Version);
            var resp = channel.BeginProcessRequest(message, callback, state);
            return resp;
        }

        protected virtual ChannelEndpointElement GetEndpoint(string action)
        {
            action = action.Remove(action.LastIndexOf('/'));
            action = action.Substring(action.LastIndexOf('/') + 2);
            ClientSection clientSection = ConfigurationManager.GetSection("system.serviceModel/client") as ClientSection;
            return clientSection.Endpoints[string.Format("contractType:Camstar.WCF.Services.I{0};name:Camstar.WCF.Services.{0}", action)];
        }

        public virtual Message EndProcessRequest(IAsyncResult result)
        {
            return channel.EndProcessRequest(result);
        }
        
        IRequestReplyRouter channel;
    }
}
