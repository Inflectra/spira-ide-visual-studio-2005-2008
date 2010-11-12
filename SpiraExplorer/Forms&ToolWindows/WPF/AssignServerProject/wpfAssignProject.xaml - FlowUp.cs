using System.Collections.Generic;
using System.Windows.Controls;
using System;
using System.Windows;

namespace Inflectra.SpiraTest.IDEIntegration.VisualStudio.WPF.Forms
{
	/// <summary>
	/// Interaction logic for wpfServerProject.xaml
	/// </summary>
	public partial class wpfAssignProject : Window
	{
		private Connect.SettingsClass.XMLFileReader _Settings;
		private List<Connect.SpiraProject> _Projects;
		private string _solName = null;

		internal void setSettings(Connect.SettingsClass.XMLFileReader Settings) 
		{
			try
			{
				this._Settings = Settings;

				//We have the file, load up all avaiable projects already defined.
				this._Projects = new List<Connect.SpiraProject>();
				string strProjs = this._Settings.GetValue("General", "Projects");
				if (!string.IsNullOrEmpty(strProjs))
				{
					foreach (string strProj in strProjs.Split(Connect.SpiraProject.CHAR_RECORD))
					{
						Connect.SpiraProject Project = Connect.SpiraProject.GenerateFromString(strProj);
						this._Projects.Add(Project);
						this.lstAvailProjects.Items.Add(Project);
					}
				}
			}
			catch (Exception ex)
			{
				Connect.logEventMessage("wpfAssignProject::setSettings", ex, System.Diagnostics.EventLogEntryType.Error);
			}
		}

		internal void setSolution(string solName)
		{
			try
			{
				//We have the solution name, load up the projects associated, and remove them from the available.
				this._solName = solName;
				if (!string.IsNullOrEmpty(this._solName))
				{
					this._solName = this._solName.Replace(' ', '_');

					this.lstSelectProjects.Items.Clear();
					string strProjs = this._Settings.GetValue(this._solName, "Projects");
					if (!string.IsNullOrEmpty(strProjs))
					{
						foreach (string strProj in strProjs.Split(Connect.SpiraProject.CHAR_RECORD))
						{
							Connect.SpiraProject Project = Connect.SpiraProject.GenerateFromString(strProj);
							this.lstSelectProjects.Items.Add(Project);
						}
						//remove dupliates.
						this.removeDuplicates();
					}
					this.lstSelectProjects.IsEnabled = true;
				}
				this.setRTFCaption();
			}
			catch (Exception ex)
			{
				Connect.logEventMessage("wpfAssignProject::setSolution", ex, System.Diagnostics.EventLogEntryType.Error);
			}
		}
	}
}
