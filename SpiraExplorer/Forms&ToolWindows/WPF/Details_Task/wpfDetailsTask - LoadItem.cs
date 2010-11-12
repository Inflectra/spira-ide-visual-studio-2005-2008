using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Resources;

namespace Inflectra.SpiraTest.IDEIntegration.VisualStudio.WPF.Forms
{
	public partial class wpfDetailsTask : UserControl
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
		/// <summary>Determines whether the API has projectUsers available.</summary>
		private bool _hasProjUsers = false;
		/// <summary>The task details.</summary>
		private Spira_ImportExport.RemoteTask _Task;
		/// <summary>The concurrency task details.</summary>
		private Spira_ImportExport.RemoteTask _TaskConcurrent;
		/// <summary>Requirements for selecting.</summary>
		private Spira_ImportExport.RemoteRequirement[] _Requirements;
		/// <summary>Users for selecting.</summary>
		private Spira_ImportExport.RemoteProjectUser[] _ProjectUsers;
		/// <summary>Releases for selecting.</summary>
		private Spira_ImportExport.RemoteRelease[] _ProjectReleases;

		private ResourceManager _resources = null;

		/// <summary>Loads the specified task into the display form.</summary>
		/// <param name="TaskTag">Item tag, like "TK:1234".</param>
		/// <param name="Project">SpiraProject for details on this project.</param>
		internal void LoadTask(Inflectra.SpiraTest.IDEIntegration.VisualStudio.Connect.SpiraProject Project, string TaskTag)
		{
			try
			{
				this._isInLoadMode = true;
				//Set display
				this.panelForm.Visibility = Visibility.Collapsed;
				this.panelLoading.Visibility = Visibility.Visible;
				this.panelLoadingError.Visibility = Visibility.Collapsed;

				//Set vaiables.
				this._ItemTag = TaskTag;
				this.lblItemTag.Content = this._ItemTag;
				this._Project = Project;

				//Call loading function.
				this.LoadItem_Task();
			}
			catch (Exception ex)
			{
				Connect.logEventMessage("wpfDetailsTask::LoadItem", ex, System.Diagnostics.EventLogEntryType.Error);
			}
		}

		private void LoadItem_Task()
		{
			try
			{
				this._isInLoadMode = true;

				//Set up the client.
				this._Client = new Inflectra.SpiraTest.IDEIntegration.VisualStudio.Spira_ImportExport.ImportExport();
				this._Client.CookieContainer = new System.Net.CookieContainer();
				this._Client.Url = this._Project.ServerURL + Connect.SpiraProject.URL_APIADD;

				//Set new event handlers.
				this._Client.Connection_Authenticate2Completed += new Inflectra.SpiraTest.IDEIntegration.VisualStudio.Spira_ImportExport.Connection_Authenticate2CompletedEventHandler(LoadItem_Task1);
				this._Client.Connection_ConnectToProjectCompleted += new Inflectra.SpiraTest.IDEIntegration.VisualStudio.Spira_ImportExport.Connection_ConnectToProjectCompletedEventHandler(LoadItem_Task2);
				this._Client.Task_RetrieveByIdCompleted += new Inflectra.SpiraTest.IDEIntegration.VisualStudio.Spira_ImportExport.Task_RetrieveByIdCompletedEventHandler(LoadItem_Task3);
				this._Client.Release_RetrieveCompleted += new Inflectra.SpiraTest.IDEIntegration.VisualStudio.Spira_ImportExport.Release_RetrieveCompletedEventHandler(LoadItem_Task3);
				this._Client.Requirement_RetrieveCompleted += new Inflectra.SpiraTest.IDEIntegration.VisualStudio.Spira_ImportExport.Requirement_RetrieveCompletedEventHandler(LoadItem_Task3);
				this._Client.Project_RetrieveUserMembershipCompleted += new Inflectra.SpiraTest.IDEIntegration.VisualStudio.Spira_ImportExport.Project_RetrieveUserMembershipCompletedEventHandler(LoadItem_Task3);
				this._Client.Task_UpdateCompleted += new Inflectra.SpiraTest.IDEIntegration.VisualStudio.Spira_ImportExport.Task_UpdateCompletedEventHandler(_Client_Task_UpdateCompleted);

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
				Connect.logEventMessage("wpfDetailsTask::LoadItem_Task", ex, System.Diagnostics.EventLogEntryType.Error);
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
					int incidentID = (int)e.UserState;

					//Get server version.
					this.LoadItem_VerifyVersion(this._Client);

					this._Client.Connection_ConnectToProjectAsync(this._Project.ProjectID, this._NumCount++);
				}
				else
				{
					this.panelLoading.Visibility = Visibility.Collapsed;
					this.panelLoadingError.Visibility = Visibility.Visible;
					this.msgLoadingErrorMsg.Text = getErrorMessage(e.Error);
				}
			}
			catch (Exception ex)
			{
				Connect.logEventMessage("wpfDetailsTask::LoadItem_Task1", ex, System.Diagnostics.EventLogEntryType.Error);
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
					this._NumRunning = 3;
					this._Client.Requirement_RetrieveAsync(new Spira_ImportExport.RemoteFilter[] { }, 1, 9999999, this._NumCount++);
					this._Client.Task_RetrieveByIdAsync(int.Parse(this._ItemTag.Split(':')[1]), this._NumCount++);
					this._Client.Release_RetrieveAsync(true, this._NumCount++);
					if (this._hasProjUsers)
					{
						this._NumRunning++;
						this._Client.Project_RetrieveUserMembershipAsync(this._NumCount++);
					}
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
				}
			}
			catch (Exception ex)
			{
				Connect.logEventMessage("wpfDetailsTYask::LoadItem_Task2", ex, System.Diagnostics.EventLogEntryType.Error);
			}
		}

		private void LoadItem_Task3(object sender, EventArgs e)
		{
			try
			{
				bool isErrorThrown = false;
				string strErrMsg = "";
				string EventType = e.GetType().ToString().Substring(e.GetType().ToString().LastIndexOf('.') + 1);

				switch (EventType)
				{
					case "Task_RetrieveByIdCompletedEventArgs":
						{
							Spira_ImportExport.Task_RetrieveByIdCompletedEventArgs evt = (Spira_ImportExport.Task_RetrieveByIdCompletedEventArgs)e;
							if (!evt.Cancelled)
							{
								if (evt.Error == null)
								{
									if (this._isInConcurrency)
										this._TaskConcurrent = evt.Result;
									else
										this._Task = evt.Result;
									this._NumRunning--;
									System.Diagnostics.Debug.WriteLine("» Retreive Task Complete. " + this._NumRunning.ToString() + " left.");
								}
								else
								{
									isErrorThrown = true;
									strErrMsg = this.getErrorMessage(evt.Error);
								}
							}
						}
						break;

					case "Requirement_RetrieveCompletedEventArgs":
						{
							Spira_ImportExport.Requirement_RetrieveCompletedEventArgs evt = (Spira_ImportExport.Requirement_RetrieveCompletedEventArgs)e;
							if (!evt.Cancelled)
							{
								if (evt.Error == null)
								{
									this._Requirements = evt.Result;
									this._NumRunning--;
									System.Diagnostics.Debug.WriteLine("» Retreive Requirements Complete. " + this._NumRunning.ToString() + " left.");
								}
								else
								{
									isErrorThrown = true;
									strErrMsg = this.getErrorMessage(evt.Error);
								}
							}
						}
						break;
					case "Project_RetrieveUserMembershipCompletedEventArgs":
						{
							Spira_ImportExport.Project_RetrieveUserMembershipCompletedEventArgs evt = (Spira_ImportExport.Project_RetrieveUserMembershipCompletedEventArgs)e;
							if (!evt.Cancelled)
							{
								if (evt.Error == null)
								{
									this._ProjectUsers = evt.Result;
									this._NumRunning--;
									System.Diagnostics.Debug.WriteLine("» Retreive Project Users Complete. " + this._NumRunning.ToString() + " left.");
								}
								else
								{
									isErrorThrown = true;
									strErrMsg = this.getErrorMessage(evt.Error);
								}
							}
						}
						break;
					case "Release_RetrieveCompletedEventArgs":
						{
							Spira_ImportExport.Release_RetrieveCompletedEventArgs evt = (Spira_ImportExport.Release_RetrieveCompletedEventArgs)e;
							if (!evt.Cancelled)
							{
								if (evt.Error == null)
								{
									this._ProjectReleases = evt.Result;
									this._NumRunning--;
									System.Diagnostics.Debug.WriteLine("» Retreive Project Releases Complete. " + this._NumRunning.ToString() + " left.");
								}
								else
								{
									isErrorThrown = true;
									strErrMsg = this.getErrorMessage(evt.Error);
								}
							}
						}
						break;
				}

				if (isErrorThrown)
				{
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

					this._isInLoadMode = false;
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

						this._isInLoadMode = false;
						this._isInConcurrency = false;
					}
				}
			}

			catch (Exception ex)
			{
				Connect.logEventMessage("wpfDetailsTask::LoadItem_Task3", ex, System.Diagnostics.EventLogEntryType.Error);
			}
		}

		private void LoadItem_VerifyVersion(Spira_ImportExport.ImportExport client)
		{
			try
			{
				//Get the version number and disable any items necessary.
				Spira_ImportExport.RemoteVersion version = client.System_GetProductVersion();
				string[] mainVers = version.Version.Split('.');
				int verMain = int.Parse(mainVers[0]);
				int verRev = int.Parse(mainVers[1]);
				int verBuild = int.Parse(mainVers[2]);

				bool enableCustom = false;
				bool enableHistory = false;
				bool showWorkflowMessage = false;

				if (verMain > 2)
				{
					enableCustom = true;
				}
				else
				{
					if (verMain == 2)
					{
						if (verRev >= 3)
						{
							if (verBuild >= 1)
							{
								if (version.Patch.HasValue && version.Patch < 17)
								{
									showWorkflowMessage = true;
								}
							}
							else
							{
								showWorkflowMessage = true;
							}
						}
						else
						{
							showWorkflowMessage = true;
						}
					}
					else
					{
						showWorkflowMessage = true;
					}
				}

				if (showWorkflowMessage)
				{
					this.panelWarning.Visibility = Visibility.Visible;
					this.panelNone.Visibility = Visibility.Collapsed;
					this.msgWrnMessage.Text = "Application version is less than 2.3.1(17). Owner of the Task cannot be changed.";
					this._hasProjUsers = false;
				}
				else
				{
					this._hasProjUsers = true;
				}
			}
			catch (Exception ex)
			{
				Connect.logEventMessage("wpfDetailsTask::LoadItem_VerifyVersion", ex, System.Diagnostics.EventLogEntryType.Error);
			}
		}

		private void LoadItem_PopulateFields()
		{
			try
			{
				if (this._isInConcurrency)
				{
					//Flag fields that don't match.
					if (this._Task.Name != this._TaskConcurrent.Name)
						this.cntrlName.Tag = ((this._Task.Name != this.cntrlName.Text) ? "2" : "1");
					else
						this.cntrlName.Tag = null;

					if (this._Task.Description != this._TaskConcurrent.Description)
						this.grpDescription.Tag = ((this._isDescChanged) ? "2" : "1");
					else
						this.grpDescription.Tag = null;

					if (this._Task.TaskPriorityId != this._TaskConcurrent.TaskPriorityId)
						this.cntrlPriority.Tag = ((this._Task.TaskPriorityId != ((KeyValuePair<int, string>)this.cntrlPriority.SelectedItem).Key) ? "2" : "1");
					else
						this.cntrlPriority.Tag = null;

					if (this._Task.OwnerId != this._TaskConcurrent.OwnerId)
						this.cntrlOwnedBy.Tag = ((this._Task.OwnerId != ((Spira_ImportExport.RemoteUser)this.cntrlOwnedBy.SelectedItem).UserId) ? "2" : "1");
					else
						this.cntrlOwnedBy.Tag = null;

					if (this._Task.RequirementId != this._TaskConcurrent.RequirementId)
						this.cntrlRequirement.Tag = ((this._TaskConcurrent.RequirementId != ((Spira_ImportExport.RemoteRequirement)this.cntrlRequirement.SelectedItem).RequirementId) ? "2" : "1");
					else
						this.cntrlRequirement.Tag = null;

					if (this._Task.ReleaseId != this._TaskConcurrent.ReleaseId)
						this.cntrlRelease.Tag = ((this._Task.ReleaseId != ((Spira_ImportExport.RemoteRelease)this.cntrlRelease.SelectedItem).ReleaseId) ? "2" : "1");
					else
						this.cntrlRelease.Tag = null;

					if (this._Task.TaskStatusId != this._TaskConcurrent.TaskStatusId)
						this.cntrlStatus.Tag = ((this._Task.TaskStatusId != ((KeyValuePair<int, string>)this.cntrlStatus.SelectedItem).Key) ? "2" : "1");
					else
						this.cntrlStatus.Tag = null;

					if (this._Task.CompletionPercent != this._TaskConcurrent.CompletionPercent)
						this.cntrlPerComplete.Tag = ((this._Task.CompletionPercent.ToString() != this.cntrlPerComplete.Text) ? "2" : "1");
					else
						this.cntrlPerComplete.Tag = null;

					if (this._Task.EstimatedEffort != this._TaskConcurrent.EstimatedEffort)
					{
						//Need to do several comparisons.
						bool isSame = true;
						if (!this._Task.EstimatedEffort.HasValue && !string.IsNullOrEmpty(this.cntrlEstEffortH.Text) && !string.IsNullOrEmpty(this.cntrlEstEffortM.Text))
							isSame = false;
						if (this._Task.EstimatedEffort.HasValue &&
							((Math.Floor(((double)this._Task.EstimatedEffort / (double)60)).ToString() != this.cntrlEstEffortH.Text.Trim()) ||
							(((double)this._Task.EstimatedEffort % (double)60).ToString() != this.cntrlEstEffortM.Text.Trim())))
						{
							isSame = false;
						}

						this.cntrlEstEffortH.Tag = this.cntrlEstEffortM.Tag = ((isSame) ? "2" : "1");
					}

					if (this._Task.ActualEffort != this._TaskConcurrent.ActualEffort)
					{
						//Need to do several comparisons.
						bool isSame = true;
						if (!this._Task.ActualEffort.HasValue && !string.IsNullOrEmpty(this.cntrlActEffortH.Text) && !string.IsNullOrEmpty(this.cntrlActEffortM.Text))
							isSame = false;
						if (this._Task.ActualEffort.HasValue &&
							((Math.Floor(((double)this._Task.ActualEffort / (double)60)).ToString() != this.cntrlActEffortH.Text.Trim()) ||
							(((double)this._Task.ActualEffort % (double)60).ToString() != this.cntrlActEffortM.Text.Trim())))
						{
							isSame = false;
						}

						this.cntrlActEffortH.Tag = this.cntrlActEffortM.Tag = ((isSame) ? "2" : "1");
					}

					//Copy concurreny issue to real one, and update screen.
					this._Task = this._TaskConcurrent;
				}

				// Name & Description
				this.cntrlName.Text = this._Task.Name;
				this.cntrlDescription.HTMLText = this._Task.Description;
				// Priority
				this.cntrlPriority.Items.Clear();
				this.cntrlPriority.Items.Add(new ComboBoxItem() { Content = "-- None --" });
				this.cntrlPriority.SelectedIndex = 0;
				foreach (KeyValuePair<int, string> Pri in this._TaskPriorities)
				{
					int numAdded = this.cntrlPriority.Items.Add(Pri);
					if (this._Task.TaskPriorityId == Pri.Key)
						this.cntrlPriority.SelectedIndex = numAdded;
				}
				// Owner
				this.cntrlOwnedBy.Items.Clear();
				if (this._hasProjUsers)
				{
					this.cntrlOwnedBy.Items.Add(new ComboBoxItem() { Content = "-- None --" });
					this.cntrlOwnedBy.SelectedIndex = 0;
					foreach (Spira_ImportExport.RemoteProjectUser User in this._ProjectUsers)
					{
						int numAdded = this.cntrlOwnedBy.Items.Add(this._Client.User_RetrieveById(User.UserId));
						if (User.UserId == this._Task.OwnerId)
							this.cntrlOwnedBy.SelectedIndex = numAdded;
					}
				}
				else
				{
					this.cntrlOwnedBy.Items.Add(new ComboBoxItem() { Content = this._Task.OwnerName });
					this.cntrlOwnedBy.SelectedIndex = 0;
				}
				// Requirement
				this.cntrlRequirement.Items.Clear();
				this.cntrlRequirement.Items.Add(new ComboBoxItem() { Content = "-- None --" });
				this.cntrlRequirement.SelectedIndex = 0;
				foreach (Spira_ImportExport.RemoteRequirement Req in this._Requirements)
				{
					int numAdded = this.cntrlRequirement.Items.Add(Req);
					if (Req.RequirementId == this._Task.RequirementId)
						this.cntrlRequirement.SelectedIndex = numAdded;
				}
				// Releases
				this.cntrlRelease.Items.Clear();
				this.cntrlRelease.Items.Add(new ComboBoxItem() { Content = "-- None --" });
				this.cntrlRelease.SelectedIndex = 0;
				foreach (Spira_ImportExport.RemoteRelease Rel in this._ProjectReleases)
				{
					int numAdded = this.cntrlRelease.Items.Add(Rel);
					if (Rel.ReleaseId == this._Task.ReleaseId)
						this.cntrlRelease.SelectedIndex = numAdded;
				}
				// Schedule
				this.cntrlStatus.Items.Clear();
				foreach (KeyValuePair<int, string> Status in this._TaskSastuses)
				{
					int numAdded = this.cntrlStatus.Items.Add(Status);
					if (this._Task.TaskStatusId == Status.Key)
						this.cntrlStatus.SelectedIndex = numAdded;
				}
				this.cntrlStartDate.SelectedDate = this._Task.StartDate;
				this.cntrlEndDate.SelectedDate = this._Task.EndDate;
				this.cntrlPerComplete.Text = this._Task.CompletionPercent.ToString();
				this.cntrlEstEffortH.Text = ((this._Task.EstimatedEffort.HasValue) ? Math.Floor(((double)this._Task.EstimatedEffort / (double)60)).ToString() : "");
				this.cntrlEstEffortM.Text = ((this._Task.EstimatedEffort.HasValue) ? ((double)this._Task.EstimatedEffort % (double)60).ToString() : "");
				this.cntrlActEffortH.Text = ((this._Task.ActualEffort.HasValue) ? Math.Floor(((double)this._Task.ActualEffort / (double)60)).ToString() : "");
				this.cntrlActEffortM.Text = ((this._Task.ActualEffort.HasValue) ? ((double)this._Task.ActualEffort % (double)60).ToString() : "");
			}
			catch (Exception ex)
			{
				Connect.logEventMessage("wpfDetailsTask::LoadItem_PopulateFields", ex, System.Diagnostics.EventLogEntryType.Error);
			}

		}
	}
}
