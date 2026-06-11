/* Copyright 2022 Siemens */
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
/// Summary description for SS_LotUnTerminate
/// </summary>
namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
	public class SS_LotScheduleCancel : MatrixWebPart
	{
		#region Properties
		protected CWC.TextBox _txtSelectionId { get { return Page.FindCamstarControl("LotScheduleCancel_SelectionId") as CWC.TextBox; } }
		protected JQDataGrid _gridContainers { get { return Page.FindCamstarControl("LotScheduleCancel_Containers") as JQDataGrid; } }
		protected CWC.TextBox txtComputerName { get { return Page.FindCamstarControl("LotScheduleCancel_ComputerNameField") as CWC.TextBox; } }
		protected Camstar.WebPortal.FormsFramework.WebControls.ContainerList containerSelectionValues { get { return Page.FindCamstarControl("ContainerField") as Camstar.WebPortal.FormsFramework.WebControls.ContainerList; } }
		protected CWC.NamedSubentity _subScheduleData { get { return Page.FindCamstarControl("LotScheduleCancel_ScheduleData") as CWC.NamedSubentity; } }
		protected CWC.NamedObject _ShipToFactory { get { return Page.FindCamstarControl("LotScheduleCancel_ShipToFactory") as CWC.NamedObject; } }
		protected CWC.NamedObject _PackingType { get { return Page.FindCamstarControl("LotScheduleCancel_PackingType") as CWC.NamedObject; } }
		protected CWC.TextBox _txtExternalComments { get { return Page.FindCamstarControl("LotScheduleCancel_ExternalComments") as CWC.TextBox; } }
		protected CWC.TextBox _txtSalesOrderNumber { get { return Page.FindCamstarControl("LotScheduleCancel_SalesOrderNumber") as CWC.TextBox; } }


		#endregion

		#region Page Events

		/// <summary>
		/// Gets Schedule data to bind data to dropdown
		/// </summary>
		public void GetScheduleData()
		{
			try
			{			
				if (_txtSelectionId.Data != null)
				{
					FrameworkSession fs = FrameworkManagerUtil.GetFrameworkSession();
					LotScheduleCancelService svc = new LotScheduleCancelService(fs.CurrentUserProfile);
					LotScheduleCancel txn = new LotScheduleCancel();
					LotScheduleCancel_Info txnInfo = new LotScheduleCancel_Info();

					//request of data to be retrived.	
					txn.Container = new ContainerRef(_txtSelectionId.TextControl.Text);

					//What field would we like data
					txnInfo.ScheduleData = new OM.Info();
					txnInfo.ScheduleData.RequestSelectionValues = true;					

					//Request object
					LotScheduleCancel_Request req = new LotScheduleCancel_Request();
					//Result object
					LotScheduleCancel_Result res = new LotScheduleCancel_Result();

					req.Info = txnInfo;

					ResultStatus rs = svc.GetEnvironment(txn, req, out res);

					//if (rs.IsSuccess && res.Environment.ScheduleData.SelectionValues != null )
					if (rs.IsSuccess)
					{	
					    if (res.Environment.ScheduleData.SelectionValues != null)
						{
							_subScheduleData.DropDownControl.Items.Clear();
							if (res.Environment.ScheduleData.SelectionValues.Rows != null)
							{
								//int i = 0;
								foreach (Row data in res.Environment.ScheduleData.SelectionValues.Rows)
								{
									_subScheduleData.DropDownControl.Items.Add(data.Values[0].ToString());
									//i++;
								}
								ScheduleData_DataChanged();
							}
							else 
							{
								ResultStatus errorMessage = new ResultStatus(_txtSelectionId.TextControl.Text + ": is not a valid lot or has not been scheduled!", false);
								DisplayMessage(errorMessage);
							}
						}						
						else 
						{
							ResultStatus errorMessage1 = new ResultStatus(_txtSelectionId.TextControl.Text + ": is not a valid lot or has not been scheduled!", false);
							DisplayMessage(errorMessage1);
						}						
					}
					else 
					{
						DisplayMessage(rs);
					}
				}
			}
			catch (Exception Ex)
			{
				DisplayMessage(new OM.ResultStatus(Ex.TargetSite.Name + "(): " + Ex.Message, false));
			}
			finally
			{
				_txtSelectionId.Focus();
			}			
		}
		
		/// <summary>
		/// This is an event that is fire when data is change in the SelectionId field is changed.
		/// </summary>
		public void ScheduleData_DataChanged()
		{

			try
			{
				if (_txtSelectionId.Data != null && _subScheduleData.Data != null)
				{
					//clear the clear message
					Page.StatusBar.ClearMessage();

					// get the session and user profile
					var fs = FrameworkManagerUtil.GetFrameworkSession();

					// Get the service type of the page
					string sServiceType = Page.PrimaryServiceType;

					// Run proper constructor. We need to be dynamic with the primary service type
					var svcType = WCFObject.CreateObjectType(sServiceType + "Service");
					//create a request object
					var oRequest = WCFObject.CreateObject(sServiceType + "_Request");
					var svcConstructor = svcType.GetConstructor(new Type[] { typeof(UserProfile) });
					var oService = svcConstructor.Invoke(new object[] { fs.CurrentUserProfile });

					//retriving data dynamically for Flexibility of the page.
					var oServiceData = CreateServiceData(sServiceType);

					var info = CreateServiceInfo(sServiceType);
					var oServiceInfo = info as LotScheduleCancel_Info;
					(oRequest as Request).Info = oServiceInfo;

					//setting the schedulingData id for retrival of data.
					(oServiceData as LotScheduleCancel).ScheduleData = new SubentityRef();
					(oServiceData as LotScheduleCancel).ScheduleData.ID = _subScheduleData.DropDownControl.Items[_subScheduleData.DropDownControl.SelectedIndex].Text;

					//request of data to be retrived.
					oServiceInfo.Product = FieldInfoUtil.RequestValue();
					oServiceInfo.MfgOrder = FieldInfoUtil.RequestValue();
					oServiceInfo.ProductBOM = FieldInfoUtil.RequestValue();
					oServiceInfo.ProcessSpec = FieldInfoUtil.RequestValue();
					oServiceInfo.FirstWIPStep = FieldInfoUtil.RequestValue();
					oServiceInfo.Owner = FieldInfoUtil.RequestValue();
					oServiceInfo.ShipToFactory = FieldInfoUtil.RequestValue();
					oServiceInfo.ExpectedStartDate = FieldInfoUtil.RequestValue();
					oServiceInfo.ExpectedEndDate = FieldInfoUtil.RequestValue();
					oServiceInfo.CycleTime = FieldInfoUtil.RequestValue();
					oServiceInfo.Priority = FieldInfoUtil.RequestValue();
					oServiceInfo.PackingType = FieldInfoUtil.RequestValue();
					oServiceInfo.ExternalComments = FieldInfoUtil.RequestValue();
					oServiceInfo.SalesOrderNumber = FieldInfoUtil.RequestValue();
					oServiceInfo.NewLotId = FieldInfoUtil.RequestValue();
					oServiceInfo.AutoSetNewLotId = FieldInfoUtil.RequestValue();
					oServiceInfo.ToWorkflow = FieldInfoUtil.RequestValue();
					oServiceInfo.ToWorkflowStep = FieldInfoUtil.RequestValue();
					oServiceInfo.ChangeToPreScheduleProduct = FieldInfoUtil.RequestValue();
					oServiceInfo.Comments = FieldInfoUtil.RequestValue();
					oServiceInfo.Containers = FieldInfoUtil.RequestValue();


					// init the result object
					Result oResult = new Result();

					// execute to request the value
					ResultStatus resultStatus = (oService as IShopFloorBase).GetEnvironment(oServiceData, (oRequest as Request), out oResult);//objSvc.ResolveSelectionId(objSvcData, new LotScheduleCancel_Request { Info = objSvcInfo }, out objResult);

					if (resultStatus.IsSuccess)
					{
						_ShipToFactory.ClearData();
						_PackingType.ClearData();
						_txtExternalComments.ClearData();
						_txtSalesOrderNumber.ClearData();
						DisplayValues((oResult.Value as LotScheduleCancel));
					}
					else
						DisplayMessage(resultStatus);
				}
			}
			catch (Exception Ex)
			{
				DisplayMessage(new OM.ResultStatus(Ex.TargetSite.Name + "(): " + Ex.Message, false));
			}
			finally
			{
				//_txtSelectionId.TextControl.Text = "";
				_txtSelectionId.Focus();
			}
		}
		
		/// <summary>
		/// This event is fired when the submit action button is clicked.
		/// </summary>
		/// <param name="serviceData"></param>
		public override void GetInputData(Service serviceData)
		{
			base.GetInputData(serviceData);
			(serviceData as LotScheduleCancel).ScheduleData = new SubentityRef();
			(serviceData as LotScheduleCancel).ScheduleData.ID = _subScheduleData.DropDownControl.SelectedValue;
			//nullify Lot Level
			foreach (var container in (serviceData as LotScheduleCancel).Containers)
				container.Level = null;
		}

		/// <summary>
		/// On page load
		/// </summary>
		/// <param name="e"></param>
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
            if (txtComputerName.Data == null)
                txtComputerName.Data = SEMI.AppCode.UIUtility.GetComputerName(this);
            
		}

		/// <summary>
		/// Resets the page value to default
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public override void WebPartCustomAction(object sender, Personalization.CustomActionEventArgs e)
		{
			base.WebPartCustomAction(sender, e);
			var action = e.Action as Camstar.WebPortal.Personalization.CustomAction;

			if (action != null && action.Parameters == "Reset")
			{
				ClearData();
			}
		}

		/// <summary>
		/// Excutes after the submit button is clicked
		/// </summary>
		/// <param name="status"></param>
		/// <param name="serviceData"></param>
		public override void PostExecute(OM.ResultStatus status, OM.Service serviceData)
		{
			base.PostExecute(status, serviceData);
			if (status.IsSuccess)
			{
				ClearData();
			}
			OnLoad(null);
		}

		/// <summary>
		/// Clears data from the form
		/// </summary>
		private void ClearData()
		{
			Page.ClearValues();
			_gridContainers.ClearData();
			_txtSelectionId.Focus();
			containerSelectionValues.ClearData();
			_subScheduleData.DropDownControl.Items.Clear();
			OnLoad(null);
		}

		#endregion

	}
}



