using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Inflectra.SpiraTest.IDEIntegration.VisualStudio.WPF.Forms
{
	public partial class wpfDetailsTask : UserControl
	{
		private bool _isInConcurrency = false;

		/// <summary>Hit when the updating is finished.</summary>
		/// <param name="sender">Client</param>
		/// <param name="e">Event Args</param>
		private void _Client_Task_UpdateCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
		{
			try
			{
				if (e.Error == null)
				{ 
					this.LoadItem_Task();
					this.panelError.Visibility = Visibility.Collapsed;
					this.panelInfo.Visibility = Visibility.Visible;
					this.msgInfMessage.Text = "Task details saved.";
					this.panelWarning.Visibility = Visibility.Collapsed;
					this.panelNone.Visibility = Visibility.Collapsed;

					this._NumRunning--;

					//Reload the item.
					this.LoadItem_Task();

					this._DetailsWindow.Caption = this._DetailsWindow.Caption.Replace(" *", "");
				}
				else
				{
					Type eType = e.Error.GetType();

					if (eType == typeof(System.Web.Services.Protocols.SoapException))
					{
						System.Web.Services.Protocols.SoapException ex = (System.Web.Services.Protocols.SoapException)e.Error;

						if (ex.Detail.FirstChild.Name == "DataValidationException")
						{
							this.panelError.Visibility = Visibility.Visible;
							this.msgErrMessage.Text = ex.Detail.InnerText;
							this.panelInfo.Visibility = Visibility.Collapsed;
							this.panelWarning.Visibility = Visibility.Collapsed;
							this.panelNone.Visibility = Visibility.Collapsed;
						}
						else if (ex.Detail.FirstChild.Name == "DataAccessConcurrencyException")
						{
							this.panelError.Visibility = Visibility.Visible;
							this.panelInfo.Visibility = Visibility.Collapsed;
							this.msgErrMessage.Text = "This incident was modified by another user. New data has been loaded." + Environment.NewLine + "Yellow fields were modified by the other user. Red fields were both modified by you and the other user.";
							this._isInConcurrency = true;
							this.LoadItem_Task();
						}
					}
					else
					{
						this.panelError.Visibility = Visibility.Visible;
						this.msgErrMessage.Text = "Error saving: " + e.Error.Message;
						this.panelInfo.Visibility = Visibility.Collapsed;
						this.panelWarning.Visibility = Visibility.Collapsed;
						this.panelNone.Visibility = Visibility.Collapsed;
					}
				}
				this.panelForm.IsEnabled = true;
			}
			catch (Exception ex)
			{
				Connect.logEventMessage("wpfDetailsTask::_Client_Task_UpdateCompleted", ex, System.Diagnostics.EventLogEntryType.Error);
			}
		}

		/// <summary>Called when the user clicks the 'Save' button.</summary>
		private void UpdateItem()
		{
			try
			{
				bool datesOK = this.validate_Dates();
				//They want to save their changes.
				if ((this._isFieldChanged || this._isDescChanged) && datesOK)
				{
					//Reset banners.
					this.panelWarning.Visibility = Visibility.Visible;
					this.panelError.Visibility = Visibility.Collapsed;
					this.panelInfo.Visibility = Visibility.Collapsed;
					this.panelNone.Visibility = Visibility.Collapsed;
					this.msgWrnMessage.Text = "Saving Task...";

					//Main fields.
					Spira_ImportExport.RemoteTask newTask = new Inflectra.SpiraTest.IDEIntegration.VisualStudio.Spira_ImportExport.RemoteTask();
					newTask.Name = this.cntrlName.Text;
					newTask.Description = ((this._isDescChanged) ? this.cntrlDescription.HTMLText : this._Task.Description);
					newTask.TaskPriorityId = ((this.cntrlPriority.SelectedItem.GetType() == typeof(KeyValuePair<int, string>)) ? ((KeyValuePair<int, string>)this.cntrlPriority.SelectedItem).Key : new int?());
					newTask.OwnerId = ((this.cntrlOwnedBy.SelectedItem.GetType() == typeof(Spira_ImportExport.RemoteUser)) ? ((Spira_ImportExport.RemoteUser)cntrlOwnedBy.SelectedItem).UserId : this._Task.OwnerId);
					newTask.RequirementId = ((this.cntrlRequirement.SelectedItem.GetType() == typeof(Spira_ImportExport.RemoteRequirement)) ? ((Spira_ImportExport.RemoteRequirement)this.cntrlRequirement.SelectedItem).RequirementId : new int?());
					newTask.ReleaseId = ((this.cntrlRelease.SelectedItem.GetType() == typeof(Spira_ImportExport.RemoteRelease)) ? ((Spira_ImportExport.RemoteRelease)this.cntrlRelease.SelectedItem).ReleaseId : new int?());
					newTask.TaskStatusId = ((KeyValuePair<int, string>)this.cntrlStatus.SelectedItem).Key;
					newTask.CompletionPercent = int.Parse(this.cntrlPerComplete.Text);
					newTask.StartDate = this.cntrlStartDate.SelectedDate;
					newTask.EndDate = this.cntrlEndDate.SelectedDate;
					int? EstH = ((string.IsNullOrEmpty(this.cntrlEstEffortH.Text)) ? new int?() : int.Parse(this.cntrlEstEffortH.Text));
					int? EstM = ((string.IsNullOrEmpty(this.cntrlEstEffortM.Text)) ? new int?() : int.Parse(this.cntrlEstEffortM.Text));
					newTask.EstimatedEffort = ((!EstH.HasValue && !EstM.HasValue) ? new int?() : (((!EstH.HasValue) ? 0 : EstH.Value * 60) + ((!EstM.HasValue) ? 0 : EstM.Value)));
					int? ActH = ((string.IsNullOrEmpty(this.cntrlActEffortH.Text)) ? new int?() : int.Parse(this.cntrlActEffortH.Text));
					int? ActM = ((string.IsNullOrEmpty(this.cntrlActEffortM.Text)) ? new int?() : int.Parse(this.cntrlActEffortM.Text));
					newTask.ActualEffort = ((!ActH.HasValue && !ActM.HasValue) ? new int?() : (((!ActH.HasValue) ? 0 : ActH.Value * 60) + ((!ActM.HasValue) ? 0 : ActM.Value)));
					//Custom fields.
					newTask.List01 = this._Task.List01;
					newTask.List02 = this._Task.List02;
					newTask.List03 = this._Task.List03;
					newTask.List04 = this._Task.List04;
					newTask.List05 = this._Task.List05;
					newTask.List06 = this._Task.List06;
					newTask.List07 = this._Task.List07;
					newTask.List08 = this._Task.List08;
					newTask.List09 = this._Task.List09;
					newTask.List10 = this._Task.List10;
					newTask.Text01 = this._Task.Text01;
					newTask.Text02 = this._Task.Text02;
					newTask.Text03 = this._Task.Text03;
					newTask.Text04 = this._Task.Text04;
					newTask.Text05 = this._Task.Text05;
					newTask.Text06 = this._Task.Text06;
					newTask.Text07 = this._Task.Text07;
					newTask.Text08 = this._Task.Text08;
					newTask.Text09 = this._Task.Text09;
					newTask.Text10 = this._Task.Text10;
					//Fixed fields.
					newTask.CreationDate = this._Task.CreationDate;
					newTask.LastUpdateDate = this._Task.LastUpdateDate;
					newTask.ProjectId = this._Task.ProjectId;
					newTask.TaskId = this._Task.TaskId;

					//Let's try to save it.
					this._NumRunning++;
					this._Client.Task_UpdateAsync(newTask, this._NumCount++);

				}
				else if (!datesOK)
				{
					this.panelNone.Visibility = Visibility.Collapsed;
					this.panelError.Visibility = Visibility.Visible;
					this.msgErrMessage.Text = "The start date must be before the end date.";
				}
			}
			catch (Exception ex)
			{
				Connect.logEventMessage("wpfDetailsTask::UpdateItem", ex, System.Diagnostics.EventLogEntryType.Error);
			}
		}
	}
}
