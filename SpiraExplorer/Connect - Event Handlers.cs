using EnvDTE;
using Extensibility;
using Inflectra.SpiraTest.IDEIntegration.VisualStudio.WinForms.Forms;
using System;

namespace Inflectra.SpiraTest.IDEIntegration.VisualStudio
{
	/// <summary>The object for implementing an Add-in.</summary>
	/// <seealso class='IDTExtensibility2' />
	public partial class Connect : IDTExtensibility2, IDTCommandTarget
	{
		/// <summary>Handles when an open (or closed?) solution is renamed.</summary>
		/// <param name="OldName">The old name of the solution.</param>
		void SolutionEvents_Renamed(string OldName)
		{
			try
			{
				string solName = ((string)this._applicationObject.Solution.Properties.Item("Name").Value).Replace(' ', '_');

				//Get the string for the old solution.
				string solProjs = this._Settings.GetValue(OldName.Replace(' ', '_'), "Projects");
				//Save it to the new solution name.
				this._Settings.SetValue(solName, "Projects", solProjs);
				//Remove the old value.
				this._Settings.SetValue(OldName.Replace(' ', '_'), "Projects", null);

				//Save it to the file.
				this._Settings.Save();

				//Reload the tree.
				this.SolutionEvents_BeforeClosing();
				this.SolutionEvents_Opened();
			}
			catch (Exception ex)
			{
				Connect.logEventMessage("Connect::SolutionEvents_Renamed", ex, System.Diagnostics.EventLogEntryType.Error);
			}
		}

		/// <summary>Handles when a new solution was opened.</summary>
		void SolutionEvents_Opened()
		{
			try
			{
				string solName = ((string)this._applicationObject.Solution.Properties.Item("Name").Value).Replace(' ', '_');
				((ucProjectTree)this.win_ProjectTree.Object).setSolution(solName);
			}
			catch (Exception ex)
			{
				Connect.logEventMessage("Connect::SolutionEvents_Opened", ex, System.Diagnostics.EventLogEntryType.Error);
			}
		}

		/// <summary>Handles when a solution is closed.</summary>
		void SolutionEvents_BeforeClosing()
		{
			try
			{
				((ucProjectTree)this.win_ProjectTree.Object).setSolution(null);
			}
			catch (Exception ex)
			{
				Connect.logEventMessage("Connect::SolutionEvents_BeforeClosing", ex, System.Diagnostics.EventLogEntryType.Error);
			}
		}


	}
}