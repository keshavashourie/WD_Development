//Copyright Siemens 2023  
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Threading.Tasks;
using Camstar.WCF.ObjectStack;
using Camstar.XMLClient.API;
using System.Web;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.Utilities;

/// <summary>
/// The Impact Engine class is used to compare CDO instance data between a source system and a target system.
/// </summary>
namespace Camstar.WebPortal.WebPortlets
{
    public class ImpactEngine
    {

        #region Properties

        public virtual List<ImpactResultItem> CDODifferences { get; set; }
        public virtual CPModelingInstanceDtl[] Instances { get; set; }

        public virtual string SourceUsername { get; set; }
        public virtual string SourcePassword { get; set; }

        [System.ComponentModel.DefaultValue("443")]
        public virtual int SourcePort { get; set; }

        [System.ComponentModel.DefaultValue("localhost")]
        public virtual string SourceHost { get; set; }

        public virtual string TargetUsername { get; set; }
        public virtual string TargetPassword { get; set; }

        [System.ComponentModel.DefaultValue("443")]
        public virtual int TargetPort { get; set; }
        public virtual string TargetHost { get; set; }

        public virtual CDOCache CurrentCDOCache { get; set; }
        #endregion

        public ImpactEngine()
        {
            CDODifferences = new List<ImpactResultItem>();
            if(HttpContext.Current != null)
            {
                var _labelCache = FrameworkManagerUtil.GetLabelCache(HttpContext.Current.Session);

                if (_labelCache != null)
                {
                    _updateResult = _labelCache.GetLabelByName("UpdateButton").Value;
                    _newResult = _labelCache.GetLabelByName("NewAction").Value;
                    _errorResult = _labelCache.GetLabelByName("Lbl_Error").Value;
                    _incompatibleResult = _labelCache.GetLabelByName("ActivationImpact_Incompatible").Value;
                }
            }
        }

        /// <summary>
        /// This is the main entry point function for the ImpactEngine. The engine will loop through
        /// all Instances and for each one get the values back from the source and target for every instance
        /// and compare the differences. When all instances have been processed the values are returned in the 
        /// CDODifferences List
        /// </summary>
        public virtual void ProcessInstances()
        {
            //Validate there are instances and that all string properties have a value
            if (Instances == null || Instances.Count() == 0) { return; }

            var exception = this.GetType().GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public)
                        .Where(pi => pi.PropertyType == typeof(string))
                        .Select(pi => (string)pi.GetValue(this))
                        .Any(pi => String.IsNullOrEmpty(pi));

            if (exception)
            { throw new Camstar.Exceptions.CamstarException("Host and Target connection properties must be set to call this function."); }

            //Instantiate InSite XML Client sessions for the source and the target for use/reuse
            csiConnection sourceConnection = CreateXMLClientConnection(SourceHost, SourcePort);
            csiConnection targetConnection = CreateXMLClientConnection(TargetHost, TargetPort);

            var sourceGUID = System.Guid.NewGuid();
            var targetGUID = System.Guid.NewGuid();

            csiSession sourceSession = CreateXMLClientSession(sourceConnection, SourceUsername, SourcePassword, sourceGUID);
            csiSession targetSession = CreateXMLClientSession(targetConnection, TargetUsername, TargetPassword, targetGUID);

            //Loop through every instance, get the xml response and compare
            Parallel.ForEach(Instances, currentInstance =>
            {
                //foreach (var currentInstance in Instances)
                //{
                //TODO: Need to revisit threading, lock for now
                lock (CDODifferences)
                {
                    //Make calls to Source and Target to get the XML Response
                    string sourceXML = callInstanceInquiry(sourceSession, currentInstance.ObjectName, currentInstance.Revision, currentInstance.ObjectTypeName);
                    string targetXML = callInstanceInquiry(targetSession, currentInstance.ObjectName, currentInstance.Revision, currentInstance.ObjectTypeName);

                    if (sourceXML.Contains(Error) || targetXML.Contains(Error))
                    {
                        ImpactResultItem resultItem = new ImpactResultItem
                        {
                            ObjectType = currentInstance.ObjectTypeName.Value,
                            Result = _errorResult,
                            Description = currentInstance.Description != null ? currentInstance.Description.Value : string.Empty,
                            InstanceName = currentInstance.ObjectName.Value,                            
                            Revision = currentInstance.Revision != null ? currentInstance.Revision.Value : string.Empty,
                            SourceValue = sourceXML.Contains(Error) ? sourceXML.Split('~').Last() : string.Empty,
                            TargetValue = targetXML.Contains(Error) ? targetXML.Split('~').Last() : string.Empty,
                        };
                        CDODifferences.Add(resultItem);
                    }
                    else
                    {
                        //If the object doesn't exist on the target, extract the needed values from the source
                        if (targetXML == NewInstance)
                        {
                            ImpactResultItem resultItem = new ImpactResultItem
                            {
                                ObjectType = currentInstance.ObjectTypeName.Value,
                                Result = _newResult,
                                Description = currentInstance.Description != null ? currentInstance.Description.Value : string.Empty,
                                InstanceName = currentInstance.ObjectName.Value,                                
                                Revision = currentInstance.Revision != null ? currentInstance.Revision.Value : string.Empty
                            };
                            CDODifferences.Add(resultItem);
                        }
                        else
                        {
                            //Items are different - run Comparison of results
                            XElement sourceElement = XElement.Parse(sourceXML);
                            XElement targetElement = XElement.Parse(targetXML);

                            string rev = currentInstance.Revision != null ? currentInstance.Revision.Value : string.Empty;
                            Compare(sourceElement, targetElement, currentInstance.ObjectTypeName.Value, currentInstance.ObjectName.Value, rev);
                        }
                    }
                }

                //}

            });

            //Close the XML Sessions when complete
            sourceConnection.removeSession(sourceGUID.ToString());
            targetConnection.removeSession(targetGUID.ToString());
        }

        protected virtual csiConnection CreateXMLClientConnection(string host, int port)
        {
            csiClient client = new csiClient();
            csiConnection connection = client.createConnection(host, port);

            return connection;
        }

        protected virtual csiSession CreateXMLClientSession(csiConnection xmlConnection, string userName, string password, Guid sessionGUID)
        {
            return xmlConnection.createSession(userName, password, sessionGUID.ToString());
        }

        /// <summary>
        /// This function makes the call to retrieve the data for the instance
        /// </summary>
        /// <param name="xmlSession"></param>
        /// <param name="objectName"></param>
        /// <param name="revision"></param>
        /// <param name="objectType"></param>
        /// <returns></returns>
        protected virtual string callInstanceInquiry(csiSession xmlSession, Primitive<string> objectName, Primitive<string> revision, Primitive<string> objectTypeName)
        {
            try
            {
                csiDocument responseDoc;
                csiDocument targetDocument;

                var docGuid = System.Guid.NewGuid().ToString();
                targetDocument = xmlSession.createDocument(docGuid);

                csiService targetService = targetDocument.createService("CDOInstanceInfoInquiry");

                //Set up input data fields
                csiObject inputData;
                inputData = targetService.inputData();
                inputData.dataField("InstanceName").setValue(objectName.Value);
                if (revision != null)
                {
                    inputData.dataField("Revision").setValue(revision.Value);
                }
                inputData.dataField("CDOTypeName").setValue(objectTypeName.Value);


                targetService.perform("Load");

                targetService.setExecute();

                //Set up request data fields
                csiRequestData requestData;
                requestData = targetService.requestData();
                requestData.requestField("CDOInstanceInfo");
                requestData.requestField("CompletionMsg");

                responseDoc = targetDocument.submit();

                if (responseDoc.checkErrors() == true)
                {
                    csiExceptionData exceptionData;
                    exceptionData = responseDoc.exceptionData();

                    var exDescr = exceptionData.getDescription();
                    if (exDescr.Contains(NotFound))
                    {
                        //If the instance is not found on the target then it is a new object
                        return NewInstance;
                    }
                    else
                    {
                        throw new Exception(exceptionData.getDescription());
                    }
                }
                else
                {
                    return responseDoc.asXML();
                }
            }
            catch (Exception ex)
            {
                return string.Concat(Error, ex.Message);
            }
        }

        /// <summary>
        /// Extracts the XML Response values into a List
        /// </summary>
        /// <param name="version"></param>
        /// <returns></returns>
        protected virtual List<CDOInstance> ExtractXML(XElement version)
        {
            return version.DescendantsAndSelf("CDOInstanceInfo").Elements("Fields").Elements("__listItem")
                  .Select(x => new CDOInstance
                  {
                      FieldName = x.Element("FieldName") != null ? x.Element("FieldName").Value : string.Empty,
                      FieldValue = x.Element("FieldValue") != null ? x.Element("FieldValue").Value : string.Empty,
                      Revision = x.Element("Revision") != null ? x.Element("Revision").Value : string.Empty,
                      IsSubentity = x.Element("IsSubentity") != null ? Convert.ToBoolean(x.Element("IsSubentity").Value) : false,
                      IsList = x.Element("IsList") != null ? Convert.ToBoolean(x.Element("IsList").Value) : false,
                      FieldCount = (x.Element("FieldCount") != null && !String.IsNullOrEmpty(x.Element("FieldCount").Value)) ? Convert.ToInt16(x.Element("FieldCount").Value) : 0,
                      SubentityItems = x.Elements().Where(d => d.Name.LocalName.StartsWith("AField") && d.HasElements).Select(a => new CDOInstance
                      {
                          FieldName = a.Element("FieldName") != null ? a.Element("FieldName").Value : string.Empty,
                          FieldValue = a.Element("FieldValue") != null ? a.Element("FieldValue").Value : string.Empty
                      }).ToList(),
                      ListItems = x.Element("IsList") != null ? (Convert.ToBoolean(x.Element("IsList").Value) == true && x.Element("Fields") != null) ? x.Element("Fields").Elements("__listItem").Where(d => d.HasElements).Select(a => new CDOInstance
                      {
                          FieldName = a.Element("FieldName") != null ? a.Element("FieldName").Value : string.Empty,
                          FieldValue = a.Element("FieldValue") != null ? a.Element("FieldValue").Value : string.Empty,
                          Revision = a.Element("Revision") != null ? a.Element("Revision").Value : string.Empty
                      }).ToList() : new List<CDOInstance>() : new List<CDOInstance>()

                  })
                  .ToList();
        }

        /// <summary>
        /// Extracts the xml from the source and target into lists of CDOInstances. Then use 
        /// linq to find the differences and add the results to the CDODifferences list
        /// </summary>
        /// <param name="sourceXML"></param>
        /// <param name="targetXML"></param>
        /// <param name="objectType"></param>
        public virtual void Compare(XElement sourceXML, XElement targetXML, string objectType, string objectName, string instanceRevision)
        {
            //Get the List of objects from the source and the target
            List<CDOInstance> sourceFields = ExtractXML(sourceXML);
            List<CDOInstance> targetFields = ExtractXML(targetXML);

            IEnumerable<CDOInstance> differences = sourceFields.Except(targetFields, new CDOInstanceComparer());

            foreach (var item in differences)
            {
                // Handle subentities:
                if (item.IsSubentity)
                {
                    if (item.FieldCount > 0)
                    {
                        //Process subentity or subentity list
                        //Determine if this is a named subentity - if it has the FieldName of "Name"
                        var sourceKey = item.SubentityItems.Find(x => x.FieldName == "Name") != null ? item.SubentityItems.Find(x => x.FieldName == "Name").FieldValue : string.Empty;
                        bool isNamed = !string.IsNullOrEmpty(sourceKey);

                        //Find the export / import key on the source and target
                        if (!isNamed)
                        {
                            sourceKey = item.SubentityItems.Find(x => x.FieldName == "ExportImportKey").FieldValue;
                        }
                        string targetKey = "";
                        var targetList = targetFields.FindAll(x => x.FieldName == item.FieldName).ToList();
                        bool matchFound = false;
                        foreach (var target in targetList)
                        {
                            if (isNamed)
                            {
                                targetKey = target.SubentityItems.Find(x => x.FieldName == "Name").FieldValue;
                            }
                            else
                            {
                                targetKey = target.SubentityItems.Find(x => x.FieldName == "ExportImportKey").FieldValue;
                            }

                            if (!string.IsNullOrEmpty(targetKey))
                            {
                                if (sourceKey == targetKey)
                                {
                                    matchFound = true;
                                    foreach (var subentityItem in item.SubentityItems)
                                    {
                                        //only add the items in the list that are changing
                                        if (subentityItem.FieldValue != target.SubentityItems.Find(x => x.FieldName == subentityItem.FieldName).FieldValue)
                                        {
                                            ImpactResultItem resultItem = new ImpactResultItem
                                            {
                                                FieldName = item.FieldName + '-' + sourceKey + "-" + subentityItem.FieldName,
                                                SourceValue = subentityItem.FieldValue,
                                                TargetValue = target.SubentityItems.Find(x => x.FieldName == subentityItem.FieldName).FieldValue,
                                                Result = _updateResult,
                                                Description = sourceFields.Find(x => x.FieldName == Description).FieldValue ?? string.Empty,
                                                ObjectType = objectType,
                                                InstanceName = objectName,
                                                Revision = instanceRevision
                                            };

                                            CDODifferences.Add(resultItem);
                                        }
                                    }
                                    break;
                                }
                            }
                        }

                        if (!matchFound)
                        {
                            var subentityCompare = item.SubentityItems.Find(x => x.FieldName == "Name") != null ? item.SubentityItems.Find(x => x.FieldName == "Name").FieldValue : item.SubentityItems.Find(x => x.FieldName == "ExportImportKey").FieldValue;

                            ImpactResultItem resultItem = null;
                            if (item.FieldName != "TxnMap")
                            {
                                resultItem = new ImpactResultItem
                                {
                                    FieldName = item.FieldName,
                                    Result = _updateResult,
                                    Description = sourceFields.Find(x => x.FieldName == Description).FieldValue ?? string.Empty,
                                    ObjectType = objectType,
                                    InstanceName = objectName,
                                    SourceValue = subentityCompare,
                                    Revision = instanceRevision
                                };
                            }
                            else
                            {                                            
                                var txnMap = item.SubentityItems.First(x => x.FieldName == "TxnToUse");
                                var txnName = CurrentCDOCache != null ? CurrentCDOCache.GetCDONameByCDOID(int.Parse(txnMap.FieldValue)) : string.Empty;                                
                                resultItem = new ImpactResultItem
                                {
                                    FieldName = txnName + " Txn Map",
                                    Result = _updateResult,
                                    Description = sourceFields.Find(x => x.FieldName == Description).FieldValue ?? string.Empty,
                                    ObjectType = objectType,
                                    InstanceName = objectName,
                                    SourceValue = subentityCompare,
                                    Revision = instanceRevision
                                };                                
                            }
                            CDODifferences.Add(resultItem);

                            var subentityItems = item.SubentityItems;
                            foreach (var subentityItem in subentityItems)
                            {
                                if (sourceFields.Find(x => x.FieldName == subentityItem.FieldName) != null && targetFields.Find(x => x.FieldName == subentityItem.FieldName) == null)
                                {
                                    ImpactResultItem resultItemSubEntity = new ImpactResultItem
                                    {
                                        FieldName = subentityItem.FieldName,
                                        SourceValue = subentityItem.FieldValue,
                                        Result = _updateResult,
                                        ObjectType = objectType,
                                        InstanceName = objectName,
                                        Revision = instanceRevision
                                    };
                                    CDODifferences.Add(resultItemSubEntity);
                                }
                            }
                        }
                        else
                        { matchFound = false; }
                    }
                }
                else if (item.IsList)
                {
                    //Process ListItems (non-subentity)
                    var targetList = targetFields.Find(x => x.FieldName == item.FieldName);
                    CDOInstance targetItem = new CDOInstance();
                    foreach (var listItem in item.ListItems)
                    {
                        if (!targetList.ListItems.Contains(listItem))
                        {
                            ImpactResultItem resultItem = new ImpactResultItem
                            {
                                FieldName = item.FieldName,
                                Result = _newResult,
                                Description = sourceFields.Find(x => x.FieldName == Description).FieldValue ?? string.Empty,
                                ObjectType = objectType,
                                InstanceName = objectName,
                                SourceValue = listItem.FieldValue + (!string.IsNullOrEmpty(listItem.Revision) ? " (" + listItem.Revision + ")" : string.Empty),
                                Revision = instanceRevision
                            };

                            CDODifferences.Add(resultItem);
                        }
                    }
                }
                else
                {
                    //non-subentity:

                    //Check for incompatibility - ie the Field itself does not exist on the target:
                    if (targetFields.Find(x => x.FieldName == item.FieldName) == null)
                    {
                        var cdoExists = CDODifferences.Find(x => x.FieldName == item.FieldName);
                        if (cdoExists == null)
                        {
                            ImpactResultItem resultItem = new ImpactResultItem
                            {
                                FieldName = item.FieldName,
                                SourceValue = item.FieldValue,
                                Result = _incompatibleResult,
                                ObjectType = objectType,
                                InstanceName = objectName,
                                Revision = instanceRevision
                            };
                            CDODifferences.Add(resultItem);
                        }
                    }
                    else
                    {
                        CDOInstance targetField = targetFields.Find(x => x.FieldName == item.FieldName);

                        ImpactResultItem resultItem = new ImpactResultItem
                        {
                            FieldName = item.FieldName,
                            SourceValue = item.FieldValue + (!string.IsNullOrEmpty(item.Revision) ? " (" + item.Revision + ")" : string.Empty),
                            TargetValue = targetField != null ? targetField.FieldValue + (!string.IsNullOrEmpty(targetField.Revision) ? " (" + targetField.Revision + ")" : string.Empty) : string.Empty,
                            Result = _updateResult,
                            Description = sourceFields.Find(x => x.FieldName == Description).FieldValue ?? string.Empty,
                            ObjectType = objectType,
                            InstanceName = objectName,
                            Revision = instanceRevision
                        };

                        CDODifferences.Add(resultItem);

                    }
                }
            }
        }

        private const string NewInstance = "~new";
        private const string Error = "~error~";
        private const string NotFound = "not found";
        private const string Description = "Description";
                
        private string _updateResult = "Update";
        private string _newResult = "New";
        private string _errorResult = "Error";
        private string _incompatibleResult = "Incompatible";
    }

    public class ImpactResultItem
    {
        public virtual string ObjectType { get; set; }

        public virtual string InstanceName { get; set; }

        public virtual string Revision { get; set; }

        public virtual string DisplayedName
        {
            get
            {
                string rev = !string.IsNullOrEmpty(Revision) ? CamstarPortalSection.GetRevisionDelimiter() + Revision : string.Empty;
                return InstanceName + rev;
            }          
        }

        public virtual string FieldName { get; set; }

        public virtual string Description { get; set; }

        public virtual string SourceValue { get; set; }

        public virtual string TargetValue { get; set; }

        public virtual string Result { get; set; }
    }

    public class CDOInstance : IEquatable<CDOInstance>
    {
        public virtual string FieldName { get; set; }
        public virtual string FieldValue { get; set; }
        public virtual string Revision { get; set; }
        public virtual bool IsSubentity { get; set; }
        public virtual bool IsList { get; set; }
        public virtual List<CDOInstance> SubentityItems { get; set; }
        public virtual List<CDOInstance> ListItems { get; set; }
        public virtual int FieldCount { get; set; }

        public virtual bool Equals(CDOInstance other)
        {
            bool equal = false;

            equal = this.FieldName == other.FieldName &&
                this.FieldValue == other.FieldValue &&
                this.Revision == other.Revision &&
                this.IsSubentity == other.IsSubentity &&
                this.IsList == other.IsList &&
                this.FieldCount == other.FieldCount;

            if (equal)
            {
                //Compare subentity and listitems Lists
                if ((other.SubentityItems != null && this.SubentityItems == null) || (this.SubentityItems != null && other.SubentityItems == null))
                { equal = false; }
                else if (other.SubentityItems != null && this.SubentityItems != null)
                {
                    equal = this.SubentityItems.SequenceEqual(other.SubentityItems);
                }

                if (equal)
                {
                    if ((other.ListItems != null && this.ListItems == null) || (this.ListItems != null && other.ListItems == null))
                    { equal = false; }
                    else if (other.ListItems != null && this.ListItems != null)
                    {
                        equal = this.ListItems.SequenceEqual(other.ListItems);
                    }
                }
            }


            return equal;
        }
    }

    internal class CDOInstanceComparer : IEqualityComparer<CDOInstance>
    {

        public virtual bool Equals(CDOInstance x, CDOInstance y)
        {
            //Check whether the objects are the same object.  
            if (Object.ReferenceEquals(x, y)) return true;

            //Check whether the objects name and value properties are equal.              
            bool isEqual = false;
            isEqual = x != null && y != null && x.FieldValue.Equals(y.FieldValue) && x.FieldName.Equals(y.FieldName) && x.Revision.Equals(y.Revision);

            if ((x.SubentityItems != null && y.SubentityItems != null) && x.SubentityItems.Count() > 0)
            {
                IEnumerable<CDOInstance> differences = x.SubentityItems.Except(y.SubentityItems, new SubentityFieldComparer());
                if (differences.Count() > 0) isEqual = false;
            }
            if ((x.ListItems != null && x.ListItems.Count() > 0) || (y.ListItems != null) && y.ListItems.Count() > 0)
            {                
                IEnumerable<CDOInstance> differences = x.ListItems.Except(y.ListItems, new SubentityFieldComparer());
                if (differences.Count() > 0)
                {
                    isEqual = false;
                }
                else
                {
                    differences = y.ListItems.Except(x.ListItems, new SubentityFieldComparer());
                    if (differences.Count() > 0) isEqual = false;
                }
            }
            return isEqual;
        }

        public virtual int GetHashCode(CDOInstance obj)
        {
            //Get hash code for the FieldName field if it is not null.  
            int hashCDOName = obj.FieldName == null ? 0 : obj.FieldName.GetHashCode();

            //Get hash code for the FieldValue field.  
            int hashCDOValue = obj.FieldValue == null ? 0 : obj.FieldValue.GetHashCode();

            return hashCDOName ^ hashCDOValue;
        }
    }


    internal class SubentityFieldComparer : IEqualityComparer<CDOInstance>
    {
        public virtual bool Equals(CDOInstance x, CDOInstance y)
        {
            //Check whether the objects are the same object.  
            if (Object.ReferenceEquals(x, y)) return true;

            //Check whether the objects name and value properties are equal.  
            return x != null && y != null && x.FieldValue.Equals(y.FieldValue) && x.FieldName.Equals(y.FieldName);
        }

        public virtual int GetHashCode(CDOInstance obj)
        {
            //Get hash code for the FieldName field if it is not null.  
            int hashCDOName = obj.FieldName == null ? 0 : obj.FieldName.GetHashCode();

            //Get hash code for the FieldValue field.  
            int hashCDOValue = obj.FieldValue == null ? 0 : obj.FieldValue.GetHashCode();

            return hashCDOName ^ hashCDOValue;
        }
    }
}
