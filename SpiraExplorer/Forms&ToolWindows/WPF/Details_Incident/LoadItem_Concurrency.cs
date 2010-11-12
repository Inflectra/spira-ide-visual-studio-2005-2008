using System;
using System.Windows.Controls;

namespace Inflectra.SpiraTest.IDEIntegration.VisualStudio.WPF.Forms
{
	public partial class wpfDetailsIncident : UserControl
	{
		/// <summary>Uses the specified concurrency incident to determine the status of a field, and highlight that field for the user to take action on.</summary>
		/// <param name="ConcurrencyIncident">The reloaded incident that has concurrency differences.</param>
		private void concurrency_HighlightFields(Spira_ImportExport.RemoteIncident ConcurrencyIncident)
		{
			try
			{
				#region Standard Fields
				// - Name
				if (this._Incident.Name != ConcurrencyIncident.Name)
				{
					if (this._Incident.Name != this.cntrlIncidentName.Text)
					{
						this.cntrlIncidentName.Tag = "2";
					}
					else
					{
						this.cntrlIncidentName.Tag = "1";
					}
				}
				// - Type
				if (this._Incident.IncidentTypeId != ConcurrencyIncident.IncidentTypeId)
				{
					if (this._Incident.IncidentTypeId != ((Spira_ImportExport.RemoteIncidentType)this.cntrlType.SelectedItem).IncidentTypeId)
					{
						this.cntrlType.Tag = "2";
					}
					else
					{
						this.cntrlType.Tag = "1";
					}
				}
				// - Status
				if (this._Incident.IncidentStatusId != ConcurrencyIncident.IncidentStatusId)
				{
					this.cntrlStatus.Tag = this.cntrlStatus.Background.Clone();

					int numSelected = -1;
					if (this.cntrlStatus.SelectedItem.GetType() == typeof(Spira_ImportExport.RemoteWorkflowIncidentTransition))
						numSelected = ((Spira_ImportExport.RemoteWorkflowIncidentTransition)this.cntrlStatus.SelectedItem).IncidentStatusID_Output;
					else if (this.cntrlStatus.SelectedItem.GetType() == typeof(Spira_ImportExport.RemoteIncidentStatus))
						numSelected = ((Spira_ImportExport.RemoteIncidentStatus)this.cntrlStatus.SelectedItem).IncidentStatusId.Value;

					if (this._Incident.IncidentStatusId != numSelected)
					{
						this.cntrlStatus.Tag = "2";
					}
					else
					{
						this.cntrlStatus.Tag = "1";
					}
				}
				// - Detected By
				if (this._Incident.OpenerId != ConcurrencyIncident.OpenerId)
				{
					if (this._Incident.OpenerId != ((Spira_ImportExport.RemoteUser)this.cntrlDetectedBy.SelectedItem).UserId)
					{
						this.cntrlDetectedBy.Tag = "2";
					}
					else
					{
						this.cntrlDetectedBy.Tag = "1";
					}
				}
				// - Assigned To
				if (this._Incident.OwnerId != ConcurrencyIncident.OwnerId)
				{
					if (this._Incident.OwnerId != ((Spira_ImportExport.RemoteUser)this.cntrlOwnedBy.SelectedItem).UserId)
					{
						this.cntrlOwnedBy.Tag = "2";
					}
					else
					{
						this.cntrlOwnedBy.Tag = "1";
					}
				}
				// - Priority
				if (this._Incident.PriorityId != ConcurrencyIncident.PriorityId)
				{
					if (this._Incident.PriorityId != ((Spira_ImportExport.RemoteIncidentPriority)this.cntrlPriority.SelectedItem).PriorityId)
					{
						this.cntrlPriority.Tag = "2";
					}
					else
					{
						this.cntrlPriority.Tag = "1";
					}
				}
				// - Severity
				if (this._Incident.SeverityId != ConcurrencyIncident.SeverityId)
				{
					if (this._Incident.SeverityId != ((Spira_ImportExport.RemoteIncidentSeverity)this.cntrlSeverity.SelectedItem).SeverityId)
					{
						this.cntrlSeverity.Tag = "2";
					}
					else
					{
						this.cntrlSeverity.Tag = "1";
					}
				}
				// - Detected In
				if (this._Incident.DetectedReleaseId != ConcurrencyIncident.DetectedReleaseId)
				{
					if (this._Incident.DetectedReleaseId != ((Spira_ImportExport.RemoteRelease)this.cntrlDetectedIn.SelectedItem).ReleaseId)
					{
						this.cntrlDetectedIn.Tag = "2";
					}
					else
					{
						this.cntrlDetectedIn.Tag = "1";
					}
				}
				// - Resolved In
				if (this._Incident.ResolvedReleaseId != ConcurrencyIncident.ResolvedReleaseId)
				{
					if (this._Incident.ResolvedReleaseId != ((Spira_ImportExport.RemoteRelease)this.cntrlResolvedIn.SelectedItem).ReleaseId)
					{
						this.cntrlResolvedIn.Tag = "2";
					}
					else
					{
						this.cntrlResolvedIn.Tag = "1";
					}
				}
				// - Verified In
				if (this._Incident.VerifiedReleaseId != ConcurrencyIncident.VerifiedReleaseId)
				{
					if (this._Incident.VerifiedReleaseId != ((Spira_ImportExport.RemoteRelease)this.cntrlVerifiedIn.SelectedItem).ReleaseId)
					{
						this.cntrlVerifiedIn.Tag = "2";
					}
					else
					{
						this.cntrlVerifiedIn.Tag = "1";
					}
				}
				// - Description
				if (this._Incident.Description.Trim() != ConcurrencyIncident.Description.Trim())
				{
					if (this._isDescChanged)
					{
						this.grpDescription.Tag = "2";
					}
					else
					{
						this.grpDescription.Tag = "1";
					}
				}
				#endregion
				#region Schedule Fields
				// - Start Date
				int schHead = 0;
				if (this._Incident.StartDate != ConcurrencyIncident.StartDate)
				{
					if (this._Incident.StartDate != this.cntrlStartDate.SelectedDate)
					{
						this.cntrlStartDate.Tag = "2";
						schHead = 2;
					}
					else
					{
						this.cntrlStartDate.Tag = "1";
						if (schHead < 1) schHead = 1;
					}
				}
				// - End Date
				if (this._Incident.ClosedDate != ConcurrencyIncident.ClosedDate)
				{
					if (this._Incident.ClosedDate != this.cntrlEndDate.SelectedDate)
					{
						this.cntrlEndDate.Tag = "2";
						schHead = 2;
					}
					else
					{
						this.cntrlEndDate.Tag = "1";
						if (schHead < 1) schHead = 1;
					}
				}
				// - Percent Complete
				if (this._Incident.CompletionPercent != ConcurrencyIncident.CompletionPercent)
				{
					if (this._Incident.CompletionPercent.ToString() != this.cntrlPerComplete.Text)
					{
						this.cntrlPerComplete.Tag = "2";
						schHead = 2;
					}
					else
					{
						this.cntrlPerComplete.Tag = "1";
						if (schHead < 1) schHead = 1;
					}
				}
				// - Estimated Effort
				if (this._Incident.EstimatedEffort != ConcurrencyIncident.EstimatedEffort)
				{
					//Need to do several comparisons.
					bool isSame = true;
					if (!this._Incident.EstimatedEffort.HasValue && !string.IsNullOrEmpty(this.cntrlEstEffortH.Text) && !string.IsNullOrEmpty(this.cntrlEstEffortM.Text))
						isSame = false;
					if (this._Incident.EstimatedEffort.HasValue &&
						((Math.Floor(((double)this._Incident.EstimatedEffort / (double)60)).ToString() != this.cntrlEstEffortH.Text.Trim()) ||
						(((double)this._Incident.EstimatedEffort % (double)60).ToString() != this.cntrlEstEffortM.Text.Trim())))
					{
						isSame = false;
					}

					if (!isSame)
					{
						this.cntrlEstEffortH.Tag = this.cntrlEstEffortM.Tag = "2";
						schHead = 2;
					}
					else
					{
						this.cntrlEstEffortH.Tag = this.cntrlEstEffortM.Tag = "1";
						if (schHead < 1) schHead = 1;
					}
				}
				// - Actual Effort
				if (this._Incident.ActualEffort != ConcurrencyIncident.ActualEffort)
				{
					//Need to do several comparisons.
					bool isSame = true;
					if (!this._Incident.ActualEffort.HasValue && !string.IsNullOrEmpty(this.cntrlActEffortH.Text) && !string.IsNullOrEmpty(this.cntrlActEffortM.Text))
						isSame = false;
					if (this._Incident.ActualEffort.HasValue &&
						((Math.Floor(((double)this._Incident.ActualEffort / (double)60)).ToString() != this.cntrlActEffortH.Text.Trim()) ||
						(((double)this._Incident.ActualEffort % (double)60).ToString() != this.cntrlActEffortM.Text.Trim())))
					{
						isSame = false;
					}

					if (!isSame)
					{
						this.cntrlActEffortH.Tag = this.cntrlActEffortM.Tag = "2";
						schHead = 2;
					}
					else
					{
						this.cntrlActEffortH.Tag = this.cntrlActEffortM.Tag = "1";
						if (schHead < 1) schHead = 1;
					}
				}
				if (schHead > 0)
				{
					this.cntrlTabSchedule.Tag = schHead.ToString();
				}
				#endregion
			}
			catch (Exception ex) 
			{
				Connect.logEventMessage("wpfDetailsIncident::concurrency_HighlightFields", ex, System.Diagnostics.EventLogEntryType.Error);
			}
		}

		/// <summary>Resets all warning/error backgrounds from controls.</summary>
		private void concurrency_ResetHighlightFields()
		{
			try
			{
				//Simply go through all fields and reset the background brush.
				// - Main fields.
				this.cntrlIncidentName.Tag = null;
				this.cntrlType.Tag = null;
				this.cntrlStatus.Tag = null;
				this.cntrlDetectedBy.Tag = null;
				this.cntrlOwnedBy.Tag = null;
				this.cntrlPriority.Tag = null;
				this.cntrlSeverity.Tag = null;
				this.cntrlDetectedIn.Tag = null;
				this.cntrlResolvedIn.Tag = null;
				this.cntrlVerifiedIn.Tag = null;
				this.grpDescription.Tag = null;
				// - Schedule fields.
				this.cntrlTabSchedule.Tag = null;
				this.cntrlStartDate.Tag = null;
				this.cntrlEndDate.Tag = null;
				this.cntrlPerComplete.Tag = null;
				this.cntrlEstEffortH.Tag = this.cntrlEstEffortM.Tag = null;
				this.cntrlActEffortH.Tag = this.cntrlActEffortM.Tag = null;
				// - Discussion field.
				this.cntrlTabDiscussion.Tag = null;
				this.grpResolution.Tag = null;
			}
			catch (Exception ex)
			{
				Connect.logEventMessage("wpfDetailsIncident::concurrency_ResetHighlightFields", ex, System.Diagnostics.EventLogEntryType.Error);
			}
		}
	}
}
