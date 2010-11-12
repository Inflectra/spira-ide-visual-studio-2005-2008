using System;
using System.Windows;
using System.Windows.Controls;

namespace Inflectra.SpiraTest.IDEIntegration.VisualStudio.WPF.Forms
{
	public partial class wpfDetailsRequirement : UserControl
	{
		/// <summary>The tag used to describe this item.</summary>
		private string _ItemTag;
		/// <summary>The project details.</summary>
		private Connect.SpiraProject _Project;
		/// <summary>The client used to talk to the server.</summary>
		private Spira_ImportExport.ImportExport _Client;
		/// <summary># of Async's still running</summary>
		private int _NumRunning = 0;
		/// <summary>Total number of Async calls.</summary> 
		private int _NumCount = 0;
		/// <summary>The task details.</summary>
		private Spira_ImportExport.RemoteRequirement _Requirement;
		private Spira_ImportExport.RemoteUser _UserCreated;
		private Spira_ImportExport.RemoteUser _UserOwner;
		private Spira_ImportExport.RemoteRelease _Release;


		/// <summary>Loads the specified task into the display form.</summary>
		/// <param name="TaskTag">Item tag, like "TK:1234".</param>
		/// <param name="Project">SpiraProject for details on this project.</param>
		internal void LoadRequirement(Inflectra.SpiraTest.IDEIntegration.VisualStudio.Connect.SpiraProject Project, string TaskTag)
		{
			try
			{
				//Set display
				this.panelForm.Visibility = Visibility.Collapsed;
				this.panelLoading.Visibility = Visibility.Visible;
				this.panelLoadingError.Visibility = Visibility.Collapsed;

				//Set vaiables.
				this._ItemTag = TaskTag;
				this.lblItemTag.Content = this._ItemTag;
				this._Project = Project;

				//Call loading function.
				this.LoadItem_Requirement();
			}
			catch (Exception ex)
			{
				Connect.logEventMessage("wpfDetailsRequirement::LoadRequirement", ex, System.Diagnostics.EventLogEntryType.Error);
			}
		}

		private void LoadItem_Requirement()
		{
			try
			{
				//Set up the client.
				this._Client = new Inflectra.SpiraTest.IDEIntegration.VisualStudio.Spira_ImportExport.ImportExport();
				this._Client.CookieContainer = new System.Net.CookieContainer();
				this._Client.Url = this._Project.ServerURL + Connect.SpiraProject.URL_APIADD;

				//Set new event handlers.
				this._Client.Connection_Authenticate2Completed += new Inflectra.SpiraTest.IDEIntegration.VisualStudio.Spira_ImportExport.Connection_Authenticate2CompletedEventHandler(LoadItem_Task1);
				this._Client.Connection_ConnectToProjectCompleted += new Inflectra.SpiraTest.IDEIntegration.VisualStudio.Spira_ImportExport.Connection_ConnectToProjectCompletedEventHandler(LoadItem_Task2);
				this._Client.Requirement_RetrieveByIdCompleted += new Inflectra.SpiraTest.IDEIntegration.VisualStudio.Spira_ImportExport.Requirement_RetrieveByIdCompletedEventHandler(LoadItem_Task3);
				this._Client.User_RetrieveByIdCompleted += new Inflectra.SpiraTest.IDEIntegration.VisualStudio.Spira_ImportExport.User_RetrieveByIdCompletedEventHandler(LoadItem_Task3);
				this._Client.Release_RetrieveByIdCompleted += new Inflectra.SpiraTest.IDEIntegration.VisualStudio.Spira_ImportExport.Release_RetrieveByIdCompletedEventHandler(LoadItem_Task3);

				string[] token = this._ItemTag.Split(':');
				if (token.Length == 2)
				{
					int artNum = -1;
					if (int.TryParse(token[1], out artNum))
					{
						this._Client.Connection_Authenticate2Async(this._Project.UserName, this._Project.UserPass, this._resources.GetString("strAddinProgNamePretty"), this._NumCount++);
					}
				}
			}
			catch (Exception ex)
			{
				Connect.logEventMessage("wpfDetailsRequirement::LoadItem_Requirement", ex, System.Diagnostics.EventLogEntryType.Error);
			}
		}

		/// <summary>Hit after we have successfully logged on. Launches off connecting to the project.</summary>
		/// <param name="sender">ImportExport</param>
		/// <param name="e">Event Args</param>
		private void LoadItem_Task1(object sender, Spira_ImportExport.Connection_Authenticate2CompletedEventArgs e)
		{
			try
			{
				if (e.Error == null)
				{
					this._Client.Connection_ConnectToProjectAsync(this._Project.ProjectID, this._NumCount++);
				}
				else
				{
					this.panelLoading.Visibility = Visibility.Collapsed;
					this.panelLoadingError.Visibility = Visibility.Visible;
					this.msgLoadingErrorMsg.Text = getErrorMessage(e.Error);
					Connect.logEventMessage("wpfDetailsRequirement::LoadItem_Task1. Error communicating!", e.Error, System.Diagnostics.EventLogEntryType.Error);
				}
			}
			catch (Exception ex)
			{
				Connect.logEventMessage("wpfDetailsRequirement::LoadItem_Task1", ex, System.Diagnostics.EventLogEntryType.Error);
			}
		}

		/// <summary>Hit after we successfully connected to the project. Launches all all other data retrievals.</summary>
		/// <param name="sender">ImportExport</param>
		/// <param name="e">Event Args</param>
		private void LoadItem_Task2(object sender, Spira_ImportExport.Connection_ConnectToProjectCompletedEventArgs e)
		{
			try
			{
				if ((e.Error == null) && (e.Result != false))
				{
					//Fire off asyncs.
					this._NumRunning = 1;
					this._Client.Requirement_RetrieveByIdAsync(int.Parse(this._ItemTag.Split(':')[1]), this._NumCount++);
				}
				else
				{
					this.panelLoading.Visibility = Visibility.Collapsed;
					this.panelLoadingError.Visibility = Visibility.Visible;
					Exception ex = e.Error;
					string errMsg = "";
					if (e.Error == null)
						errMsg = "Could not connect to project #" + this._Project.ProjectID.ToString();
					else
					{
						errMsg = getErrorMessage(e.Error);
					}
					this.msgLoadingErrorMsg.Text = errMsg;
					Connect.logEventMessage("wpfDetailsRequirement::LoadItem_Task2. Error communicating!", e.Error, System.Diagnostics.EventLogEntryType.Error);
				}
			}
			catch (Exception ex)
			{
				Connect.logEventMessage("wpfDetailsRequirement::LoadItem_Task2", ex, System.Diagnostics.EventLogEntryType.Error);
			}
		}

		private void LoadItem_Task3(object sender, EventArgs e)
		{
			try
			{
				Exception exThrown = null;

				bool isErrorThrown = false;
				string strErrMsg = "";
				string EventType = e.GetType().ToString().Substring(e.GetType().ToString().LastIndexOf('.') + 1);

				switch (EventType)
				{
					case "Requirement_RetrieveByIdCompletedEventArgs":
						{
							Spira_ImportExport.Requirement_RetrieveByIdCompletedEventArgs evt = (Spira_ImportExport.Requirement_RetrieveByIdCompletedEventArgs)e;
							if (!evt.Cancelled)
							{
								if (evt.Error == null)
								{
									this._Requirement = evt.Result;
									this._NumRunning--;
									System.Diagnostics.Debug.WriteLine("» Retreive Requirement Complete. " + this._NumRunning.ToString() + " left.");

									//Fire off two calls to get users.
									this._NumRunning++;
									this._NumCount++;
									this._Client.User_RetrieveByIdAsync(this._Requirement.AuthorId, -1);
									if (this._Requirement.OwnerId.HasValue)
									{
										this._NumRunning++;
										this._NumCount++;
										this._Client.User_RetrieveByIdAsync(this._Requirement.OwnerId.Value, -2);
									}
									if (this._Requirement.ReleaseId.HasValue)
									{
										this._NumRunning++;
										this._Client.Release_RetrieveByIdAsync(this._Requirement.ReleaseId.Value, this._NumCount++);
									}
								}
								else
								{
									isErrorThrown = true;
									strErrMsg = this.getErrorMessage(evt.Error);
									exThrown = evt.Error;
								}
							}
						}
						break;

					case "User_RetrieveByIdCompletedEventArgs":
						{
							Spira_ImportExport.User_RetrieveByIdCompletedEventArgs evt = (Spira_ImportExport.User_RetrieveByIdCompletedEventArgs)e;
							if (!evt.Cancelled)
							{
								if (evt.Error == null)
								{
									int numRun = ((int)evt.UserState);
									if (numRun == -1)
									{
										this._UserCreated = evt.Result;
										this._NumRunning--;
									}
									else if (numRun == -2)
									{
										this._UserOwner = evt.Result;
										this._NumRunning--;
									}
									System.Diagnostics.Debug.WriteLine("» Retreive User (" + numRun.ToString() + ") Complete. " + this._NumRunning.ToString() + " left.");
								}
								else
								{
									isErrorThrown = true;
									strErrMsg = this.getErrorMessage(evt.Error);
									exThrown = evt.Error;
								}

							}
						}
						break;

					case "Release_RetrieveByIdCompletedEventArgs":
						{
							Spira_ImportExport.Release_RetrieveByIdCompletedEventArgs evt = (Spira_ImportExport.Release_RetrieveByIdCompletedEventArgs)e;
							if (!evt.Cancelled)
							{
								if (evt.Error == null)
								{
									this._Release = evt.Result;
									this._NumRunning--;
									System.Diagnostics.Debug.WriteLine("» Retreive Release Complete. " + this._NumRunning.ToString() + " left.");
								}
								else
								{
									isErrorThrown = true;
									strErrMsg = this.getErrorMessage(evt.Error);
									exThrown = evt.Error;
								}
							}
						}
						break;
				}

				if (isErrorThrown)
				{
					try
					{
						this._Client.CancelAsync(-1);
						this._Client.CancelAsync(-2);
					}
					catch { }
					for (int i = 0; i <= this._NumCount; i++)
					{
						try
						{
							this._Client.CancelAsync(i);
						}
						catch { }
					}
					//Display error information.
					this.panelLoading.Visibility = Visibility.Collapsed;
					this.panelLoadingError.Visibility = Visibility.Visible;
					this.panelForm.Visibility = Visibility.Collapsed;
					this.msgLoadingErrorMsg.Text = strErrMsg;
					Connect.logEventMessage("wpfDetailsRequirement::LoadItem_Task3. Error communicating!", exThrown, System.Diagnostics.EventLogEntryType.Error);
				}
				else
				{
					if (this._NumRunning == 0)
					{
						//Populate fields.
						this.LoadItem_PopulateFields();

						//Show loading complete.
						this.panelLoading.Visibility = Visibility.Collapsed;
						this.panelForm.Visibility = Visibility.Visible;
					}
				}
			}
			catch (Exception ex)
			{
				Connect.logEventMessage("wpfDetailsRequirement::LoadItem_Task3", ex, System.Diagnostics.EventLogEntryType.Error);
			}
		}

		private void LoadItem_PopulateFields()
		{
			try
			{
				// Name & Description
				this.cntrlName.Text = this._Requirement.Name;
				this.cntrlDescription.HTMLText = this._Requirement.Description;
				// Importance
				this.cntrlImportance.Items.Clear();
				if (this._Requirement.ImportanceId.HasValue)
				{
					this.cntrlImportance.Items.Add(new ComboBoxItem() { Content = this._Requirement.ImportanceId.Value.ToString() + " - " + this._ReqPriorities[this._Requirement.ImportanceId.Value] });
				}
				else
				{
					this.cntrlImportance.Items.Add(new ComboBoxItem() { Content = "-- None --" });
				}
				this.cntrlImportance.SelectedIndex = 0;
				//Status
				this.cntrlStatus.Items.Clear();
				this.cntrlStatus.Items.Add(new ComboBoxItem() { Content = this._ReqSastuses[this._Requirement.StatusId] });
				this.cntrlStatus.SelectedIndex = 0;
				//Creator.
				this.cntrlCreatedBy.Items.Clear();
				this.cntrlCreatedBy.Items.Add(new ComboBoxItem() { Content = this._UserCreated.FirstName + " " + this._UserCreated.LastName });
				this.cntrlCreatedBy.SelectedIndex = 0;
				// Owner
				this.cntrlOwnedBy.Items.Clear();
				if (this._UserOwner != null)
				{
					this.cntrlOwnedBy.Items.Add(new ComboBoxItem() { Content = this._UserOwner.FirstName + " " + this._UserOwner.LastName });
					this.cntrlOwnedBy.SelectedIndex = 0;
				}
				// Releases
				this.cntrlRelease.Items.Clear();
				if (this._Release != null)
				{
					this.cntrlRelease.Items.Add(new ComboBoxItem() { Content = this._Release.Name });
				}
				else
				{
					this.cntrlRelease.Items.Add(new ComboBoxItem() { Content = "-- None --" });
				}
				this.cntrlRelease.SelectedIndex = 0;
				// Schedule
				this.cntrlEstEffortH.Text = ((this._Requirement.PlannedEffort.HasValue) ? Math.Floor(((double)this._Requirement.PlannedEffort / (double)60)).ToString() : "");
				this.cntrlEstEffortM.Text = ((this._Requirement.PlannedEffort.HasValue) ? ((double)this._Requirement.PlannedEffort % (double)60).ToString() : "");
			}
			catch (Exception ex)
			{
				Connect.logEventMessage("wpfDetailsRequirement::LoadItem_PopulateFields", ex, System.Diagnostics.EventLogEntryType.Error);
			}
		}
	}
}
