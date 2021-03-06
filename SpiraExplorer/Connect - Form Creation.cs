using System;
using System.Collections.Generic;
using System.Reflection;
using EnvDTE;
using EnvDTE80;
using Extensibility;
using Inflectra.SpiraTest.IDEIntegration.VisualStudio.WinForms.Forms;
using Inflectra.SpiraTest.IDEIntegration.VisualStudio.WPF.Forms;
using System.Resources;
using System.Globalization;
using Inflectra.SpiraTest.IDEIntegration.VisualStudio.Resources;
using Microsoft.VisualStudio.CommandBars;

namespace Inflectra.SpiraTest.IDEIntegration.VisualStudio
{
	/// <summary>The object for implementing an Add-in.</summary>
	/// <seealso class='IDTExtensibility2' />
	public partial class Connect : IDTExtensibility2, IDTCommandTarget
	{
		//Windows
		private Window2 win_ProjectTree = null;
		private Dictionary<string, Window2> win_Details = new Dictionary<string, Window2>();

		//GUIDs for windows.
		private const string GUID_PROJECTTREE = "{B4B2629B-FD11-4C4E-990A-1B90FD672D93}";

		/// <summary>Create or view the ProjectTree form.</summary>
		private void createControls()
		{
			this.logMessage("Connect::createControls()", System.Diagnostics.EventLogEntryType.Information);
			//Create the Project Tree.
			if (this.win_ProjectTree == null)
			{
				try
				{
					this.logMessage("Connect::createControls  Create Form", System.Diagnostics.EventLogEntryType.Information);
					object refObj = new object();
					Windows2 windows = ((Windows2)this._applicationObject.Windows);
					string asm = Assembly.GetExecutingAssembly().Location;
					this.win_ProjectTree = (Window2)windows.CreateToolWindow2(this._addInInstance,
						asm,
						"Inflectra.SpiraTest.IDEIntegration.VisualStudio.WinForms.Forms.ucProjectTree",
						this._resources.GetString("strProjectTreeName"),
						GUID_PROJECTTREE,
						ref refObj);

					//Set the icon.
					try
					{
						this.win_ProjectTree.SetTabPicture(((System.Drawing.Bitmap)this._resources.GetObject("2")).GetHbitmap().ToInt32());
					}
					catch (Exception ex)
					{
						try
						{
							this.win_ProjectTree.SetTabPicture(((System.Drawing.Bitmap)this._resources.GetObject("2")).GetHbitmap());
						}
						catch (Exception ex2)
						{
							Connect.logEventMessage("Connect:createControls  Could not load icon image.", ex2, System.Diagnostics.EventLogEntryType.Error);
							Connect.logEventMessage("Connect:createControls  Could not load icon image. Inner Exception", ex, System.Diagnostics.EventLogEntryType.Error);
						}
					}
					//Was it visible last?
					bool isVisible = false;
					string isVisSetting = this._Settings.GetValue(this._resources.GetString("strSettingSectionGeneral"), "windowVisible");
					if (isVisSetting != null)
						bool.TryParse(isVisSetting, out isVisible);
					this.win_ProjectTree.Visible = isVisible;
					if (isVisible)
						this.win_ProjectTree.Activate();

					//Tie to events.
					ucProjectTree Ctrl = (ucProjectTree)this.win_ProjectTree.Object;
					wpfProjectTree wpfCtrl = (wpfProjectTree)Ctrl.elementHost1.Child;
					wpfCtrl.OpenDetails += new EventHandler(wpfCtrl_OpenDetails);

					//Give the settings file.
					Ctrl.setSettingsFile(this._Settings);
					//Give the solution name, if loaded.
					if (this._applicationObject.Solution.IsOpen)
					{
						string solName = ((string)this._applicationObject.Solution.Properties.Item("Name").Value).Replace(' ', '_');
						Ctrl.setSolution(solName);
					}
				}
				catch (Exception ex)
				{
					Connect.logEventMessage("Connect::createControls  Create Window", ex, System.Diagnostics.EventLogEntryType.Error);
				}
			}
			else
			{
				this.logMessage("Connect::createControls  No Work Needed", System.Diagnostics.EventLogEntryType.Information);
			}

			this.logMessage("Connect::createControls Exit", System.Diagnostics.EventLogEntryType.Information);
		}

		/// <summary>Hit when the user decides to open the details of an item.</summary>
		/// <param name="sender">String containing the tag to open.</param>
		/// <param name="e">EventArgs</param>
		void wpfCtrl_OpenDetails(object sender, EventArgs e)
		{
			try
			{

				this.logMessage("Connect::wpfCtrl_OpenDetails(" + ((sender != null) ? ((sender.GetType() == typeof(string)) ? sender : "object") : "null") + ", " + ((e == null) ? "null" : "object") + ")", System.Diagnostics.EventLogEntryType.Information);

				//Open the Details of an item.
				SpiraProject itemProj = SpiraProject.GenerateFromString(((string)sender).Split(SpiraProject.CHAR_RECORD)[0]);
				string itemCode = ((string)sender).Split(SpiraProject.CHAR_RECORD)[1].ToUpperInvariant();
				string itemType = itemCode.Split(':')[0];
				int itemNum = int.Parse(itemCode.Split(':')[1]);

				if (this.win_Details.ContainsKey(itemCode))
				{
					this.logMessage("Connect::wpfCtrl_OpenDetails  Activate Detail Window", System.Diagnostics.EventLogEntryType.Information);
					//Activate existing window.
					this.win_Details[itemCode].Visible = true;
					this.win_Details[itemCode].Activate();
				}
				else
				{
					this.logMessage("Connect::wpfCtrl_OpenDetails  Create Detail Window", System.Diagnostics.EventLogEntryType.Information);
					//Create new window.
					string tabTitle = "";
					string winClass = "";
					if (itemType == "TK")
					{
						winClass = "Inflectra.SpiraTest.IDEIntegration.VisualStudio.WinForms.Forms.ucDetailsTask";
						tabTitle = "Task #" + itemNum.ToString();
					}
					else if (itemType == "IN")
					{
						winClass = "Inflectra.SpiraTest.IDEIntegration.VisualStudio.WinForms.Forms.ucDetailsIncident";
						tabTitle = "Incident #" + itemNum.ToString();
					}
					else if (itemType == "RQ")
					{
						winClass = "Inflectra.SpiraTest.IDEIntegration.VisualStudio.WinForms.Forms.ucDetailsRequirement";
						tabTitle = "Requirement #" + itemNum.ToString();
					}

					//Create the window here.
					object refObj = new object();
					Windows2 windows = ((Windows2)this._applicationObject.Windows);
					string asm = Assembly.GetExecutingAssembly().Location;

					string newGuid = "{" + Guid.NewGuid().ToString() + "}";

					try
					{
						Window2 win_Detail = (Window2)windows.CreateToolWindow2(this._addInInstance,
								asm,
								winClass,
								this._resources.GetString("strProjectTreeName"),
								newGuid,
								ref refObj);

						try
						{
							win_Detail.SetTabPicture(((System.Drawing.Bitmap)this._resources.GetObject("2")).GetHbitmap().ToInt32());
						}
						catch (Exception ex)
						{
							try
							{
								win_Detail.SetTabPicture(((System.Drawing.Bitmap)this._resources.GetObject("2")).GetHbitmap());
							}
							catch (Exception ex2)
							{
								Connect.logEventMessage("Connect:wpfCtrl_OpenDetails  Could not load icon image.", ex2, System.Diagnostics.EventLogEntryType.Error);
								Connect.logEventMessage("Connect:wpfCtrl_OpenDetails  Could not load icon image. Inner Exception", ex, System.Diagnostics.EventLogEntryType.Error);
							}
						}
						win_Detail.IsFloating = false;
						try
						{
							win_Detail.WindowState = vsWindowState.vsWindowStateMaximize;
						}
						catch { }
						win_Detail.AutoHides = false;
						win_Detail.Linkable = false;
						win_Detail.Visible = true;
						win_Detail.Caption = tabTitle;

						if (itemType == "TK")
						{
							ucDetailsTask Ctrl = (ucDetailsTask)win_Detail.Object;
							wpfDetailsTask WpfCtrl = (wpfDetailsTask)Ctrl.elementHost1.Child;

							WpfCtrl.setDetailWindow = win_Detail;
							WpfCtrl.LoadTask(itemProj, itemCode);
						}
						else if (itemType == "IN")
						{
							ucDetailsIncident Ctrl = (ucDetailsIncident)win_Detail.Object;
							wpfDetailsIncident WpfCtrl = (wpfDetailsIncident)Ctrl.elementHost1.Child;

							WpfCtrl.setDetailWindow = win_Detail;
							WpfCtrl.loadItem(itemProj, itemCode);
						}
						else if (itemType == "RQ")
						{
							ucDetailsRequirement Ctrl = (ucDetailsRequirement)win_Detail.Object;
							wpfDetailsRequirement WpfCtrl = (wpfDetailsRequirement)Ctrl.elementHost1.Child;

							WpfCtrl.setDetailWindow = win_Detail;
							WpfCtrl.LoadRequirement(itemProj, itemCode);
						}

						this.win_Details.Add(itemCode, win_Detail);
					}
					catch (Exception ex)
					{
						Connect.logEventMessage("Connect::wpfCtrl_OpenDetails  Create Detail Window", ex, System.Diagnostics.EventLogEntryType.Error);

					}
				}
				this.logMessage("Connect::wpfCtrl_OpenDetails  Exit", System.Diagnostics.EventLogEntryType.Information);
			}
			catch (Exception ex)
			{
				Connect.logEventMessage("Connect::wpfCtrl_OpenDetails", ex, System.Diagnostics.EventLogEntryType.Error);
			}
		}

		/// <summary>Create our menu items.</summary>
		private void createMenus()
		{
			try
			{
				this.logMessage("Connect::createMenus()", System.Diagnostics.EventLogEntryType.Information);

				object[] contextGUIDS = new object[] { };
				Commands2 commands = (Commands2)_applicationObject.Commands;
				string toolsMenuName = "";

				// Get the localized name for the 'View' menu.
				try
				{
					this.logMessage("Connect::createMenus  Get Menuname", System.Diagnostics.EventLogEntryType.Information);
					string resourceName;
					ResourceManager resourceManager = new ResourceManager("Inflectra.SpiraTest.IDEIntegration.VisualStudio.Resources.CommandBar", Assembly.GetExecutingAssembly());
					CultureInfo cultureInfo = new CultureInfo(_applicationObject.LocaleID);

					if (cultureInfo.TwoLetterISOLanguageName == "zh")
					{
						System.Globalization.CultureInfo parentCultureInfo = cultureInfo.Parent;
						resourceName = this._resources.GetString("strMenuName");
					}
					else
					{
						resourceName = String.Concat(cultureInfo.TwoLetterISOLanguageName, this._resources.GetString("strMenuName"));
					}
					toolsMenuName = resourceManager.GetString(resourceName);
				}
				catch (Exception ex)
				{
					Connect.logEventMessage("Connect::createMenus  Get Menuname", ex, System.Diagnostics.EventLogEntryType.Error);
				}

				this.logMessage("Connect::createMenu  toolsMenuName: \"" + toolsMenuName + "\"", System.Diagnostics.EventLogEntryType.Information);

				//Get the main menubar & View submenu.
				Microsoft.VisualStudio.CommandBars.CommandBar menuBarCommandBar = ((Microsoft.VisualStudio.CommandBars.CommandBars)_applicationObject.CommandBars)["MenuBar"];
				CommandBarControl toolsControl = menuBarCommandBar.Controls[toolsMenuName];
				CommandBarPopup toolsPopup = (CommandBarPopup)toolsControl;

				this.logMessage("Connect::createMenu  Add named Command", System.Diagnostics.EventLogEntryType.Information);

				//Create the command.
				Command viewTree = commands.AddNamedCommand2(
					_addInInstance,
					"viewTree",
					this._resources.GetString("mnuViewTreeBtn"),
					this._resources.GetString("mnuViewTreeTip"),
					false,
					2,
					ref contextGUIDS,
					(int)vsCommandStatus.vsCommandStatusSupported + (int)vsCommandStatus.vsCommandStatusEnabled,
					(int)vsCommandStyle.vsCommandStylePictAndText,
					vsCommandControlType.vsCommandControlTypeButton
					);
				//Add the commands to the menu bar.
				this.logMessage("Connect::createMenu  Add to Toolbar", System.Diagnostics.EventLogEntryType.Information);
				try
				{
					if (toolsPopup.CommandBar != null)
					{
						if (viewTree != null) viewTree.AddControl(toolsPopup.CommandBar, 1);
					}
				}
				catch (Exception ex)
				{
					Connect.logEventMessage("Connect::createMenu  Add to Toolbar", ex, System.Diagnostics.EventLogEntryType.Error);
				}
			}
			catch (Exception ex)
			{
				Connect.logEventMessage("Connect::createMenu", ex, System.Diagnostics.EventLogEntryType.Error);
			}
			#region For A Submenu, not used.
			////Find the 'Spirateam Integration' command bar, or create a new one.
			//Microsoft.VisualStudio.CommandBars.CommandBar spiraCommandBar = null;
			//try
			//{
			//    spiraCommandBar = ( (Microsoft.VisualStudio.CommandBars.CommandBars)_applicationObject.CommandBars )[Branding.strMenuView];
			//}
			//catch
			//{
			//    //If this threw an error, we need to create a new one.
			//    spiraCommandBar = (Microsoft.VisualStudio.CommandBars.CommandBar)commands.AddCommandBar(
			//        Branding.strMenuView,
			//        vsCommandBarType.vsCommandBarTypeMenu,
			//        toolsPopup.CommandBar,
			//        1);
			//}

			////Create the two commands.
			//Command viewTree = commands.AddNamedCommand2(
			//    _addInInstance,
			//    "viewTree",
			//    Generic.mnuViewTreeBtn,
			//    Generic.mnuViewTreeTip,
			//    true,
			//    10,
			//    ref contextGUIDS,
			//    (int)vsCommandStatus.vsCommandStatusSupported + (int)vsCommandStatus.vsCommandStatusEnabled,
			//    (int)vsCommandStyle.vsCommandStylePictAndText,
			//    vsCommandControlType.vsCommandControlTypeButton
			//    );
			//Command viewDetail = commands.AddNamedCommand2(
			//    _addInInstance,
			//    "viewDetail",
			//    Generic.mnuViewDetailBtn,
			//    Generic.mnuViewDetailTip,
			//    true,
			//    13,
			//    ref contextGUIDS,
			//    (int)vsCommandStatus.vsCommandStatusSupported + (int)vsCommandStatus.vsCommandStatusEnabled,
			//    (int)vsCommandStyle.vsCommandStylePictAndText,
			//    vsCommandControlType.vsCommandControlTypeButton
			//    );

			////Add the commands to the menu bar.
			//try
			//{
			//    if (spiraCommandBar != null)
			//    {
			//        if (viewTree != null) viewTree.AddControl(spiraCommandBar, 1);
			//        if (viewDetail != null) viewDetail.AddControl(spiraCommandBar, 2);
			//    }

			//}
			//catch (System.ArgumentException)
			//{
			//    //If we are here, then the exception is probably because a command with that name
			//    //  already exists. If so there is no need to recreate the command and we can 
			//    //  safely ignore the exception.
			//}

			#endregion

		}
	}
}