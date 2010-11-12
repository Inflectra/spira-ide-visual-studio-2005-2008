using System.Windows;
using System.Windows.Controls;
using System;

namespace Inflectra.SpiraTest.IDEIntegration.VisualStudio.WPF.Forms
{
	/// <summary>
	/// Interaction logic for wpfProjectTree.xaml
	/// </summary>
	public partial class wpfProjectTree : UserControl 
	{
		internal void setSettingsFile(Connect.SettingsClass.XMLFileReader settingsFile)
		{
			this._settingsFile = settingsFile;
		}

		internal void setSolution(string solName)
		{
			try
			{
				//Enable the progress bar as we load data.
				this.barLoading.Visibility = Visibility.Visible;
				this._solutionName = solName;

				if (string.IsNullOrEmpty(solName))
				{
					this.noSolutionLoaded();
					this.barLoading.Visibility = Visibility.Collapsed;
				}
				else
				{
					//Get projects associated with this solution.
					string AssProjects = this._settingsFile.GetValue(solName, "Projects");
					if (string.IsNullOrEmpty(AssProjects))
					{
						this.noProjectsLoaded();
						this.barLoading.Visibility = Visibility.Collapsed;
					}
					else
					{
						this.loadProjects(AssProjects);
					}
				}
			}
			catch (Exception ex)
			{
				Connect.logEventMessage("wpfProjectTree::setSolution", ex, System.Diagnostics.EventLogEntryType.Error);
			}
		}
	}
}
