using System;
using System.Collections.Generic;
using System.Web.Services.Protocols;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Inflectra.SpiraTest.IDEIntegration.VisualStudio.Spira_ImportExport;
using Inflectra.SpiraTest.IDEIntegration.VisualStudio.WPF.Controls;
using System.Windows.Media.Imaging; 

namespace Inflectra.SpiraTest.IDEIntegration.VisualStudio.WPF.Forms
{
	/// <summary>
	/// Interaction logic for wpfDetailsIncident.xaml
	/// </summary>
	public partial class wpfDetailsIncident : UserControl
	{
		//Private vars.
		// - Determines whether or not the user changed the description.
		private bool _isDescChanged = false;
		private bool _isResChanged = false;
		private bool _isFieldChanged = false;
		private EnvDTE80.Window2 _DetailsWindow = null;
		private string _DetailsWindowTitle = null;

		/// <summary>Creates a new instance of our IncidentDetailsForm.</summary>
		public wpfDetailsIncident()
		{
			try
			{
				InitializeComponent();

				//Get resources
				this._resources = Connect.getCultureResource;

				//Lock numeric fields
				this.cntrlPerComplete.PreviewTextInput += new TextCompositionEventHandler(_cntrl_PreviewTextInput);
				this.cntrlEstEffortH.PreviewTextInput += new TextCompositionEventHandler(_cntrl_PreviewTextInput);
				this.cntrlEstEffortM.PreviewTextInput += new TextCompositionEventHandler(_cntrl_PreviewTextInput);
				this.cntrlActEffortH.PreviewTextInput += new TextCompositionEventHandler(_cntrl_PreviewTextInput);
				this.cntrlActEffortM.PreviewTextInput += new TextCompositionEventHandler(_cntrl_PreviewTextInput);
				this.cntrlPerComplete.LostFocus += new RoutedEventHandler(_cntrl_LostFocus);
				this.cntrlEstEffortH.LostFocus += new RoutedEventHandler(_cntrl_LostFocus);
				this.cntrlEstEffortM.LostFocus += new RoutedEventHandler(_cntrl_LostFocus);
				this.cntrlActEffortH.LostFocus += new RoutedEventHandler(_cntrl_LostFocus);
				this.cntrlActEffortM.LostFocus += new RoutedEventHandler(_cntrl_LostFocus);

				//Load images.
				this._bnrImgInfo.Source = this.getImage("imgInfoWPF", new Size()).Source;
				this._bnrImgError.Source = this.getImage("imgErrorWPF", new Size()).Source;
				this._barImgInfo.Source = this.getImage("imgInfoWPF", new Size()).Source;
				this._barImgWarning.Source = this.getImage("imgWarningWPF", new Size()).Source;
				this._barImgError.Source = this.getImage("imgErrorWPF", new Size()).Source;
				this._barImgLogo.Source = this.getImage("imgLogoWPF", new Size()).Source;
			}
			catch (Exception ex)
			{
				Connect.logEventMessage("wpfDetailsIncident::.ctor", ex, System.Diagnostics.EventLogEntryType.Error);
			}
		}

		/// <summary>Set the window handle for this form.</summary>
		internal EnvDTE80.Window2 setDetailWindow
		{
			set
			{
				this._DetailsWindow = value;
				this._DetailsWindowTitle = this._DetailsWindow.Caption;
			}
		}

		/// <summary>Hit when a numeric field loses focus. Verifies the item entered is a number.</summary>
		/// <param name="sender">cntrlEstEffortH, cntrlEstEffortM, cntrlActEffortH, cntrlActEffortM, cntrlPerComplete</param>
		/// <param name="e">Event Args</param>
		void _cntrl_LostFocus(object sender, RoutedEventArgs e)
		{
			try
			{
				TextBox control = (TextBox)sender;

				int tryNum;
				if (int.TryParse(control.Text, out tryNum))
				{
					if (tryNum < 0)
						control.Text = "0";
					if (control == this.cntrlPerComplete && tryNum > 100)
						control.Text = "100";
					this._isFieldChanged = true;
					this._btnSave.IsEnabled = true;
					if (!this._DetailsWindow.Caption.Contains("*"))
					{
						this._DetailsWindow.Caption = this._DetailsWindowTitle + " *";
					}
				}
				else
				{
					control.Text = "";
				}
			}
			catch (Exception ex)
			{
				Connect.logEventMessage("wpfDetailsIncident::_cntrl_LostFocus", ex, System.Diagnostics.EventLogEntryType.Error);
			}
		}

		/// <summary>Hit when the user tries to enter data into a numeric field. Verifies it's numeric only.</summary>
		/// <param name="sender">cntrlEstEffortH, cntrlEstEffortM, cntrlActEffortH, cntrlActEffortM, cntrlPerComplete</param>
		/// <param name="e">Event Args</param>
		void _cntrl_PreviewTextInput(object sender, TextCompositionEventArgs e)
		{
			try
			{
				int tryParse;
				if (!int.TryParse(e.Text, out tryParse))
				{
					e.Handled = true;
				}
			}
			catch (Exception ex)
			{
				e.Handled = false;
				Connect.logEventMessage("wpfDetailsIncident::_cntrl_PreviewiTextInput", ex, System.Diagnostics.EventLogEntryType.Error);
			}
		}

		/// <summary>The item code for the loaded artifact.</summary>
		/// <returns>String in the format of "IN:xxxx"</returns>
		internal string getItemCode()
		{
			return this._itemCode;
		}

		/// <summary>Hit when the user clicks a message bar.</summary>
		/// <param name="sender">messageWarning / messageError / messageInfo</param>
		/// <param name="e">MouseButton Event Args</param>
		private void messageWarning_MouseDown(object sender, MouseButtonEventArgs e)
		{
			try
			{
				Grid panel = (Grid)sender;

				if (panel.Visibility == Visibility.Visible)
				{
					panel.Visibility = Visibility.Collapsed;
					this.panelNone.Visibility = Visibility.Visible;
				}
			}
			catch (Exception ex)
			{
				Connect.logEventMessage("wpfDetailsIncident::messageWarning_MouseDown", ex, System.Diagnostics.EventLogEntryType.Error);
			}
		}

		/// <summary>Hit when the user clicks to save the incident.</summary>
		/// <param name="sender">The save button.</param>
		/// <param name="e">Event Args</param>
		private void _cntrlSave_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				if (this._isFieldChanged)
				{
					string badFields = "";

					if (this.workflow_CheckFieldValues(out badFields))
					{
						this.isInSaveMode = true;
						// Update the form to show we're saving.
						this.panelNone.Visibility = Visibility.Collapsed;
						this.panelWarning.Visibility = Visibility.Visible;
						this.msgWrnMessage.Text = "Saving incident...";
						this.panelContents.IsEnabled = false;

						RemoteIncident newIncident = new RemoteIncident();

						//Copy over our base fields..
						newIncident.Name = this.cntrlIncidentName.Text;
						newIncident.IncidentTypeId = ((RemoteIncidentType)this.cntrlType.SelectedItem).IncidentTypeId.Value;
						if (this.cntrlStatus.SelectedItem.GetType() == typeof(RemoteWorkflowIncidentTransition))
							newIncident.IncidentStatusId = ((RemoteWorkflowIncidentTransition)this.cntrlStatus.SelectedItem).IncidentStatusID_Output;
						else if (this.cntrlStatus.SelectedItem.GetType() == typeof(RemoteIncidentStatus))
							newIncident.IncidentStatusId = ((RemoteIncidentStatus)this.cntrlStatus.SelectedItem).IncidentStatusId.Value;
						newIncident.OpenerId = ((this.cntrlDetectedBy.SelectedItem.GetType() == typeof(RemoteUser)) ? ((RemoteUser)this.cntrlDetectedBy.SelectedItem).UserId.Value : -1);
						newIncident.OwnerId = ((this.cntrlOwnedBy.SelectedItem.GetType() == typeof(RemoteUser)) ? ((RemoteUser)this.cntrlOwnedBy.SelectedItem).UserId.Value : new int?());
						newIncident.PriorityId = ((this.cntrlPriority.SelectedItem.GetType() == typeof(RemoteIncidentPriority)) ? ((RemoteIncidentPriority)this.cntrlPriority.SelectedItem).PriorityId : new int?());
						newIncident.SeverityId = ((this.cntrlSeverity.SelectedItem.GetType() == typeof(RemoteIncidentSeverity)) ? ((RemoteIncidentSeverity)this.cntrlSeverity.SelectedItem).SeverityId : new int?());
						newIncident.DetectedReleaseId = ((this.cntrlDetectedIn.SelectedItem.GetType() == typeof(RemoteRelease)) ? ((RemoteRelease)this.cntrlDetectedIn.SelectedItem).ReleaseId : new int?());
						newIncident.ResolvedReleaseId = ((this.cntrlResolvedIn.SelectedItem.GetType() == typeof(RemoteRelease)) ? ((RemoteRelease)this.cntrlResolvedIn.SelectedItem).ReleaseId : new int?());
						newIncident.VerifiedReleaseId = ((this.cntrlVerifiedIn.SelectedItem.GetType() == typeof(RemoteRelease)) ? ((RemoteRelease)this.cntrlVerifiedIn.SelectedItem).ReleaseId : new int?());
						if (this._isDescChanged)
							newIncident.Description = this.cntrlDescription.HTMLText;
						else
							newIncident.Description = this._Incident.Description;
						//Now custom properties.
						//TODO: Custom Property fields. (Needs API update.)
						newIncident.List01 = this._Incident.List01;
						newIncident.List02 = this._Incident.List02;
						newIncident.List03 = this._Incident.List03;
						newIncident.List04 = this._Incident.List04;
						newIncident.List05 = this._Incident.List05;
						newIncident.List06 = this._Incident.List06;
						newIncident.List07 = this._Incident.List07;
						newIncident.List08 = this._Incident.List08;
						newIncident.List09 = this._Incident.List09;
						newIncident.List10 = this._Incident.List10;
						newIncident.Text01 = this._Incident.Text01;
						newIncident.Text02 = this._Incident.Text02;
						newIncident.Text03 = this._Incident.Text03;
						newIncident.Text04 = this._Incident.Text04;
						newIncident.Text05 = this._Incident.Text05;
						newIncident.Text06 = this._Incident.Text06;
						newIncident.Text07 = this._Incident.Text07;
						newIncident.Text08 = this._Incident.Text08;
						newIncident.Text09 = this._Incident.Text09;
						newIncident.Text10 = this._Incident.Text10;
						//Schedule fields.
						newIncident.StartDate = this.cntrlStartDate.SelectedDate;
						newIncident.ClosedDate = this.cntrlEndDate.SelectedDate;
						newIncident.CompletionPercent = ((string.IsNullOrEmpty(this.cntrlPerComplete.Text)) ? 0 : int.Parse(this.cntrlPerComplete.Text));
						int? EstH = ((string.IsNullOrEmpty(this.cntrlEstEffortH.Text)) ? new int?() : int.Parse(this.cntrlEstEffortH.Text));
						int? EstM = ((string.IsNullOrEmpty(this.cntrlEstEffortM.Text)) ? new int?() : int.Parse(this.cntrlEstEffortM.Text));
						newIncident.EstimatedEffort = ((!EstH.HasValue && !EstM.HasValue) ? new int?() : (((!EstH.HasValue) ? 0 : EstH.Value * 60) + ((!EstM.HasValue) ? 0 : EstM.Value)));
						int? ActH = ((string.IsNullOrEmpty(this.cntrlActEffortH.Text)) ? new int?() : int.Parse(this.cntrlActEffortH.Text));
						int? ActM = ((string.IsNullOrEmpty(this.cntrlActEffortM.Text)) ? new int?() : int.Parse(this.cntrlActEffortM.Text));
						newIncident.ActualEffort = ((!ActH.HasValue && !ActM.HasValue) ? new int?() : (((!ActH.HasValue) ? 0 : ActH.Value * 60) + ((!ActM.HasValue) ? 0 : ActM.Value)));
						//Now the set fields.
						newIncident.IncidentId = this._Incident.IncidentId;
						newIncident.LastUpdateDate = this._Incident.LastUpdateDate;
						newIncident.CreationDate = this._Incident.CreationDate;
						newIncident.ProjectId = this._Incident.ProjectId;

						//Add a resoution.
						RemoteIncidentResolution newRes = new RemoteIncidentResolution();
						if (this._isResChanged)
						{
							newRes.CreationDate = DateTime.Now;
							newRes.CreatorId = this._Project.UserID;
							newRes.IncidentId = newIncident.IncidentId.Value;
							newRes.Resolution = this.cntrlResolution.HTMLText;
						}
						this._NumRunning++;
						this._Client.Incident_UpdateAsync(newIncident, this._NumAsync++);
						if (this._isResChanged)
						{
							this._NumRunning++;
							this._Client.Incident_AddResolutionsAsync(new RemoteIncidentResolution[] { newRes }, this._NumAsync++);
						}
					}
					else
					{
						string errMsg = "";
						if (badFields.Split(';').Length > 3)
							errMsg = "You must fill out all required fields before saving.";
						else
						{
							errMsg = "The ";
							foreach (string fieldName in badFields.Split(';'))
							{
								errMsg += fieldName + ", ";
							}
							errMsg = errMsg.Trim(' ').Trim(',');
							errMsg += " field" + ((badFields.Split(';').Length > 1) ? "s" : "");
							errMsg += " " + ((badFields.Split(';').Length > 1) ? "are" : "is");
							errMsg += " required before saving.";
						}
						this.msgErrMessage.Text = errMsg;
						this.panelError.Visibility = Visibility.Visible;
						this.panelWarning.Visibility = Visibility.Collapsed;
						this.panelInfo.Visibility = Visibility.Collapsed;
						this.panelNone.Visibility = Visibility.Collapsed;
					}

				}
			}
			catch (Exception ex)
			{
				Connect.logEventMessage("wpfDetailsIncident::_cntrlSave_Click", ex, System.Diagnostics.EventLogEntryType.Error);
			}
		}

		/// <summary>Hit when the client has completed the initial update of the incident.</summary>
		/// <param name="sender">Client</param>
		/// <param name="e">Event Args</param>
		private void _Client_Incident_UpdateCompleted(object sender, EventArgs e)
		{
			try
			{
				string EventType = e.GetType().ToString().Substring(e.GetType().ToString().LastIndexOf('.') + 1);

				//Message string.
				string errMsg = "";
				string errTool = "";

				switch (EventType)
				{
					case "Incident_AddResolutionsCompletedEventArgs":
						{
							this._NumRunning--;
							Incident_AddResolutionsCompletedEventArgs evt = (Incident_AddResolutionsCompletedEventArgs)e;
							if (evt.Error == null)
							{
								//Successful inserting of new Resolution. Reload info.
								this.loadItem_PopulateDiscussion(evt.Result);
							}
							else
							{
								errMsg += "Could not insert new discussion.";
								errTool += Environment.NewLine + this.getErrorMessage(evt.Error);
								errTool = errTool.Trim();
							}
						}
						break;
					case "AsyncCompletedEventArgs":
						{
							this._NumRunning--;

							System.ComponentModel.AsyncCompletedEventArgs evt = (System.ComponentModel.AsyncCompletedEventArgs)e;
							if (evt.Error == null)
							{
								this.loadItem_Incident();
								//Inform user item was saved.
								this.panelWarning.Visibility = Visibility.Collapsed;
								this.panelInfo.Visibility = Visibility.Visible;
								this.msgInfMessage.Text = "Incident saved.";
							}
							else
							{
								if (evt.Error.GetType() == typeof(SoapException) && (((SoapException)evt.Error).Detail.FirstChild.Name == "DataAccessConcurrencyException"))
								{
									//Fire off the data concurrency pull.
									this.isInConcMode = true;
									this._NumRunning++;
									this._Client.Incident_RetrieveByIdAsync(int.Parse(this._itemCode.Split(':')[1]), this._NumAsync++);
								}
								else
								{
									//Show error.
									this.panelWarning.Visibility = Visibility.Collapsed;
									this.panelError.Visibility = Visibility.Visible;
									this.msgErrMessage.Text = getErrorMessage(evt.Error);
								}
							}
						}
						break;
				}
				if (this._NumRunning == 0 && !this.isInConcMode)
				{
					this.isInSaveMode = false;
				}
			}
			catch (Exception ex)
			{
				Connect.logEventMessage("wpfDetailsIncident::_Client_Incident_UpdateCompleted", ex, System.Diagnostics.EventLogEntryType.Error);
			}
		}

		/// <summary>Hit when the Type or Status dropdown is changed. Have to reload workflow and update fields.</summary>
		/// <param name="sender">cntrlType / cntrlStatus</param>
		/// <param name="e">Event Args</param>
		private void _cntrlStatusType_Changed(object sender, SelectionChangedEventArgs e)
		{
			try
			{
				if (!this.isInLoadMode)
				{
					if (!this._DetailsWindow.Caption.Contains("*"))
					{
						this._DetailsWindow.Caption = this._DetailsWindowTitle + " *";
					}

					if ((this.cntrlType.SelectedItem != null) && (this.cntrlStatus.SelectedItem != null))
					{

						if (this.cntrlStatus.SelectedItem.GetType() == typeof(RemoteIncidentStatus))
						{
							//It's the original status. Jump back.
							this.loadItem_SetEnabledFields(this._IncWkfFields_Current);
						}
						else if (this.cntrlStatus.SelectedItem.GetType() == typeof(RemoteWorkflowIncidentTransition))
						{
							//Create the client.
							Spira_ImportExport.ImportExport client = new Inflectra.SpiraTest.IDEIntegration.VisualStudio.Spira_ImportExport.ImportExport();
							client.Url = this._Project.ServerURL + Connect.SpiraProject.URL_APIADD;
							client.CookieContainer = new System.Net.CookieContainer();

							client.Connection_Authenticate2(this._Project.UserName, this._Project.UserPass, this._resources.GetString("strAddinProgNamePretty"));
							client.Connection_ConnectToProject(this._Project.ProjectID);

							//Get the updated workflow and set the fields!
							this._IncWkfFields_Updated = scanWorkFlowFields(client.Incident_RetrieveWorkflowFields(((RemoteIncidentType)this.cntrlType.SelectedItem).IncidentTypeId.Value, ((RemoteWorkflowIncidentTransition)this.cntrlStatus.SelectedItem).IncidentStatusID_Output));
							this.loadItem_SetEnabledFields(this._IncWkfFields_Updated);

							//Set the flag letting us know item has changed.
							this._isFieldChanged = true;
							this._btnSave.IsEnabled = true;
						}

					}
				}
			}
			catch (Exception ex)
			{
				Connect.logEventMessage("wpfDetailsIncident::_cntrlStatusType_Changed", ex, System.Diagnostics.EventLogEntryType.Error);
			}
		}

		/// <summary>Hit when the user wants to reload the incident after an error.</summary>
		/// <param name="sender">TextBlock</param>
		/// <param name="e">Event Args</param>
		private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
		{
			int inNumber = int.Parse(this.getItemCode().Split(':')[1]);
			this.loadItem(this._Project, this._itemCode);
		}

		/// <summary>Hit when a textbox or dropdown list changes.</summary>
		/// <param name="sender">cntrlIncidentName, cntrlDetectedBy, cntrlOwnedBy, cntrlPriority, cntrlSeverity, cntrlDetectedIn, cntrlResolvedIn, cntrlVerifiedIn, cntrlDescription</param>
		/// <param name="e"></param>
		private void _cntrl_TextChanged(object sender, EventArgs e)
		{
			try
			{
				if (!this.isInLoadMode)
				{
					if (!this._DetailsWindow.Caption.Contains("*"))
					{
						this._DetailsWindow.Caption = this._DetailsWindowTitle + " *";
					}

					this._isFieldChanged = true;
					this._btnSave.IsEnabled = true;
					if (sender.GetType() == typeof(wpfRichHTMLText))
					{
						if (((wpfRichHTMLText)sender).Name == "cntrlDescription")
						{
							this._isDescChanged = true;
						}
						else if (((wpfRichHTMLText)sender).Name == "cntrlResolution")
						{
							this._isResChanged = true;
						}
					}
				}
			}
			catch (Exception ex)
			{
				Connect.logEventMessage("wpfDetailsIncident::TextBlock_MouseDown", ex, System.Diagnostics.EventLogEntryType.Error);
			}
		}

		/// <summary>Generates a pretty Error message string.</summary>
		/// <param name="e">Exception.</param>
		/// <returns>String of the error messages.</returns>
		private string getErrorMessage(Exception e)
		{
			Exception ex = e;
			string errMsg = "» " + ex.Message;
			while (ex.InnerException != null)
			{
				errMsg += Environment.NewLine + "» " + ex.InnerException.Message;
				ex = ex.InnerException;
			}

			return errMsg;
		}

		/// <summary>Scans the result from a RetrieveWorkflowField call and add the fields into a useable dictionary.</summary>
		/// <param name="workFields">RemoteWorkflowField[]</param>
		/// <returns>Dictionary of Field and Status</returns>
		private Dictionary<int, int> scanWorkFlowFields(Spira_ImportExport.RemoteWorkflowIncidentFields[] workFields)
		{
			try
			{
				Dictionary<int, int> retList = new Dictionary<int, int>();
				foreach (Spira_ImportExport.RemoteWorkflowIncidentFields Field in workFields)
				{
					if (retList.ContainsKey(Field.FieldID))
					{
						if (Field.FieldStatus > retList[Field.FieldID])
						{
							retList[Field.FieldID] = Field.FieldStatus;
						}
					}
					else
					{
						retList.Add(Field.FieldID, Field.FieldStatus);
					}
				}

				return retList;
			}
			catch (Exception ex)
			{
				Connect.logEventMessage("wpfDetailsIncident::scanWorkFlowFields", ex, System.Diagnostics.EventLogEntryType.Error);
				return new Dictionary<int, int>();
			}
		}

		/// <summary>Converts a resource to a WPF image. Needed for application resources.</summary>
		/// <param name="image">Bitmap of the image to convert.</param>
		/// <returns>BitmapSource suitable for an Image control.</returns>
		private BitmapSource getBMSource(System.Drawing.Bitmap image)
		{
			try
			{
				if (image != null)
				{
					IntPtr bmStream = image.GetHbitmap();
					return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(bmStream, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromWidthAndHeight(image.Width, image.Height));
				}
			}
			catch (Exception ex)
			{
				Connect.logEventMessage("wpfDetailsIncident::getBMSource", ex, System.Diagnostics.EventLogEntryType.Error);
			}
			return null;
		}

		/// <summary>Creates an Image control for a specified resource.</summary>
		/// <param name="Key">The key name of the resource to use. Will search and use Product-dependent resources first.</param>
		/// <param name="Size">Size of the desired image, or null.</param>
		/// <param name="Stretch">Desired stretch setting of image, or null.</param>
		/// <returns>Resulting image, or null if key is not found.</returns>
		private Image getImage(string key, Size size)
		{
			try
			{
				Image retImage = new Image();
				if (size != null)
				{
					retImage.Height = size.Height;
					retImage.Width = size.Width;
				}
				//if (fill != null)
				//{
				//    retImage.Stretch = fill;
				//}
				BitmapSource image = null;
				try
				{
					image = getBMSource((System.Drawing.Bitmap)this._resources.GetObject(key));
				}
				catch
				{
				}

				retImage.Source = image;

				return retImage;
			}
			catch (Exception ex)
			{
				Connect.logEventMessage("wpfDetailsIncident::getImage", ex, System.Diagnostics.EventLogEntryType.Error);
				return new Image();
			}
		}
	}
}
