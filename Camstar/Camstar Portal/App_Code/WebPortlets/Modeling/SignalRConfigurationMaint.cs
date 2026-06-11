// Copyright Siemens 2025
using System;
using System.Linq;
using OM = Camstar.WCF.ObjectStack;
using Camstar.WCF.ObjectStack;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using Microsoft.AspNet.SignalR;
using System.Reflection;
using System.Collections.Generic;

namespace Camstar.WebPortal.WebPortlets.Modeling
{
    public class SignalRConfigurationMaint : MatrixWebPart
    {
        #region Controls
        protected virtual CWC.TextBox txtSignalRHubName
        {
            get { return Page.FindCamstarControl("ObjectChanges_HubName") as CWC.TextBox; }
        }

        protected virtual CWC.DropDownList ddlSignalRHubName
        {
            get { return Page.FindCamstarControl("SignalRHubNamesList") as CWC.DropDownList; }
        }

        #endregion

        #region Methods
        protected override void OnPreRender(EventArgs e)
        {
            if (!Page.IsPostBack)
                PopulateSignalRHubNames();

            if (txtSignalRHubName.Data != null)
                ddlSignalRHubName.Data = CapitalizeFirstCharacter(txtSignalRHubName.Data.ToString());

            base.OnPreRender(e);
        }

        static string CapitalizeFirstCharacter(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            return char.ToUpper(input[0]) + input.Substring(1);
        }

        protected override void OnLoad(EventArgs e)
        {
            if (!Page.IsPostBack)
                txtSignalRHubName.Visible = false;

            ddlSignalRHubName.DataChanged += ddlSignalRHubName_DataChanged;

            base.OnLoad(e);
        }
        
        protected virtual void ddlSignalRHubName_DataChanged(object sender, EventArgs e)
        {
            txtSignalRHubName.Data = ddlSignalRHubName.Data;
        }

        private void PopulateSignalRHubNames()
        {
            // Fetch SignalR hub names
            var hubNames = GetSignalRHubs();

            OM.RecordSet recSet = new OM.RecordSet
            {
                Headers = new[] { new OM.Header(), new OM.Header() }
            };
            recSet.Headers[0].Label = new OM.Label { DefaultValue = "Value", Value = "Value" };
            recSet.Headers[0].Name = "Value";
            recSet.Headers[0].TypeCode = TypeCode.String;

            recSet.Headers[1].Label = new OM.Label { Value = "Name", DefaultValue = "Name" };
            recSet.Headers[1].TypeCode = TypeCode.String;
            recSet.Headers[1].Name = "Name";

            if (hubNames != null && hubNames.Any())
            {
                var rows = new List<OM.Row>();
                foreach (var hub in hubNames)
                {
                    var r = new OM.Row { Values = new[] { hub, hub } };
                    rows.Add(r);
                }
                recSet.Rows = rows.ToArray();
            }

            ddlSignalRHubName.SetSelectionValues(recSet);
        }

        private static string[] GetSignalRHubs()
        {
            // Get all types in the current assembly or specified assemblies
            var hubs = Assembly.GetExecutingAssembly()
                               .GetTypes()
                               .Where(type => typeof(Hub).IsAssignableFrom(type) && !type.IsAbstract)
                               .Select(type => type.Name) // Extract the name of each type
                               .ToArray();
            return hubs;
        }

        #endregion
    }
}
