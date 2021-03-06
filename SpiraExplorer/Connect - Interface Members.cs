using System;
using System.Diagnostics;
using System.Resources;
using EnvDTE;
using EnvDTE80;
using Extensibility;
using System.Windows.Forms;

namespace Inflectra.SpiraTest.IDEIntegration.VisualStudio
{
	/// <summary>The object for implementing an Add-in.</summary>
	/// <seealso class='IDTExtensibility2' />
	public partial class Connect : IDTExtensibility2, IDTCommandTarget
	{

		private ResourceManager _resources = null;
		private bool _debug = false;

		/// <summary>Implements the constructor for the Add-in object. Place your initialization code within this method.</summary>
		public Connect()
		{
			try
			{
				//Get resource library.
				this._resources = Connect.getCultureResource;

				//Create event logging source if needed.
				if (!EventLog.SourceExists((string)this._resources.GetObject("strAddinProgName")))
				{
					EventLog.CreateEventSource((string)this._resources.GetObject("strAddinProgName"), "Application");
				}
			}
			catch (Exception ex)
			{
				try
				{
					Connect.logEventMessage("Connect - Get Resources", ex, EventLogEntryType.Error);
				}
				catch (Exception ex2)
				{
					string msgEr = ex.Message;
					while (ex.InnerException != null)
					{
						msgEr += Environment.NewLine + ex.InnerException.Message;
						ex = ex.InnerException;
					}
					MessageBox.Show("Error loading SpiraTeam Explorer:" + Environment.NewLine + msgEr, "Error:", MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
				throw (ex);
			}

			this.logMessage("Enter: Connect()", EventLogEntryType.Information);
			this.logMessage("Exit: Connect", EventLogEntryType.Information);
		}

		#region IDTExtensibility2 Interface
		/// <summary>Implements the OnConnection method of the IDTExtensibility2 interface. Receives notification that the Add-in is being loaded.</summary>
		/// <param term='application'>Root object of the host application.</param>
		/// <param term='connectMode'>Describes how the Add-in is being loaded.</param>
		/// <param term='addInInst'>Object representing this Add-in.</param>
		/// <seealso class='IDTExtensibility2' />
		public void OnConnection(object application, ext_ConnectMode connectMode, object addInInst, ref Array custom)
		{
			//Debug
			string msg = "Enter: OnConnection(" + ((application == null) ? "null" : "object") + ", " + connectMode.ToString() + ", " + ((addInInst == null) ? "null" : "object") + ", " + ((custom == null) ? "null" : "object") + ")";
			this.logMessage(msg, EventLogEntryType.Information);

			this._applicationObject = (DTE2)application;
			this._addInInstance = (AddIn)addInInst;
			switch (connectMode)
			{
				#region Initial UI Load (ext_cm_UISetup)
				case ext_ConnectMode.ext_cm_UISetup:
					{
						this.logMessage("OnConnection - Create menu", EventLogEntryType.Information);
						try
						{
							this.createMenus();
						}
						catch (Exception ex)
						{
							Connect.logEventMessage("OnConnection - Create menu.", ex, EventLogEntryType.Error);
						}
					}
					break;
				#endregion

				#region IDE Load (ext_cm_AfterStartup, ext_cm_Startup)
				case ext_ConnectMode.ext_cm_AfterStartup:
				case ext_ConnectMode.ext_cm_Startup:
					{
						try
						{
							this.logMessage("OnConnection - Open Settings", EventLogEntryType.Information);
							// Create our Settings class.
							string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) +
								"\\" + (string)this._resources.GetObject("strCompany") +
								"\\" + (string)this._resources.GetObject("strAddinProgNamePretty") +
								"\\" + (string)this._resources.GetObject("strAddinProgName") + ".xni";
							this._Settings = new SettingsClass.XMLFileReader(appData);
							//Make default debug if not already set.
							if (!string.IsNullOrEmpty(this._Settings.GetValue(this._resources.GetString("strSettingSectionGeneral"), "Debug")))
							{
								this._debug = bool.Parse(this._Settings.GetValue(this._resources.GetString("strSettingSectionGeneral"), "Debug"));
							}
							else
							{
								this._Settings.SetValue(this._resources.GetString("strSettingSectionGeneral"), "Debug", this._debug.ToString());
								this._Settings.Save();
							}

							//Create our forms and read our settings.
							this.createControls();

							//Attach to events.
							this.logMessage("OnConnection - Attach Events", EventLogEntryType.Information);
							this.solEvents = this._applicationObject.Events.SolutionEvents;
							this.solEvents.BeforeClosing += new _dispSolutionEvents_BeforeClosingEventHandler(SolutionEvents_BeforeClosing);
							this.solEvents.Opened += new _dispSolutionEvents_OpenedEventHandler(SolutionEvents_Opened);
							this.solEvents.Renamed += new _dispSolutionEvents_RenamedEventHandler(SolutionEvents_Renamed);
						}
						catch (Exception ex)
						{
							Connect.logEventMessage("OnConnection - Open Settings / Attach Events.", ex, EventLogEntryType.Error);
						}
					}
					break;
				#endregion
			}
			this.logMessage("Exit: OnConnection", EventLogEntryType.Information);
		}

		/// <summary>Implements the OnDisconnection method of the IDTExtensibility2 interface. Receives notification that the Add-in is being unloaded.</summary>
		/// <param term='disconnectMode'>Describes how the Add-in is being unloaded.</param>
		/// <param term='custom'>Array of parameters that are host application specific.</param>
		/// <seealso class='IDTExtensibility2' />
		public void OnDisconnection(ext_DisconnectMode disconnectMode, ref Array custom)
		{
			this.logMessage("Enter: OnDisconnection(" + disconnectMode.ToString() + ", " + ((custom == null) ? "null" : "object") + ")", EventLogEntryType.Information);
			this.logMessage("Exit: OnDisconnection", EventLogEntryType.Information);
		}

		/// <summary>Implements the OnAddInsUpdate method of the IDTExtensibility2 interface. Receives notification when the collection of Add-ins has changed.</summary>
		/// <param term='custom'>Array of parameters that are host application specific.</param>
		/// <seealso class='IDTExtensibility2' />		
		public void OnAddInsUpdate(ref Array custom)
		{
			this.logMessage("Enter: OnAddInsUpdate(" + ((custom == null) ? "null" : "object") + ")", EventLogEntryType.Information);
			this.logMessage("Exit: OnAddInsUpdate", EventLogEntryType.Information);
		}

		/// <summary>Implements the OnStartupComplete method of the IDTExtensibility2 interface. Receives notification that the host application has completed loading.</summary>
		/// <param term='custom'>Array of parameters that are host application specific.</param>
		/// <seealso class='IDTExtensibility2' />
		public void OnStartupComplete(ref Array custom)
		{
			this.logMessage("Enter: OnStartupComplete(" + ((custom == null) ? "null" : "object") + ")", EventLogEntryType.Information);
			this.logMessage("Exit: OnStartupComplete", EventLogEntryType.Information);
		}

		/// <summary>Implements the OnBeginShutdown method of the IDTExtensibility2 interface. Receives notification that the host application is being unloaded.</summary>
		/// <param term='custom'>Array of parameters that are host application specific.</param>
		/// <seealso class='IDTExtensibility2' />
		public void OnBeginShutdown(ref Array custom)
		{
			this.logMessage("Enter: OnBeginShutdown(" + ((custom == null) ? "null" : "object") + ")", EventLogEntryType.Information);

			//Save our settings.
			this._Settings.SetValue(this._resources.GetString("strSettingSectionGeneral"), "windowVisible", this.win_ProjectTree.Visible.ToString());
			this._Settings.SetValue(this._resources.GetString("strSettingSectionGeneral"), "windowFloating", this.win_ProjectTree.IsFloating.ToString());
			this._Settings.SetValue(this._resources.GetString("strSettingSectionGeneral"), "windowAutoHides", this.win_ProjectTree.AutoHides.ToString());

			this._Settings.Save();

			this.logMessage("Exit: OnBeginShutdown", EventLogEntryType.Information);
		}

		#endregion

		#region IDTCommandTarget Interface
		/// <summary>Implements the QueryStatus method of the IDTCommandTarget interface. This is called when the command's availability is updated</summary>
		/// <param term='commandName'>The name of the command to determine state for.</param>
		/// <param term='neededText'>Text that is needed for the command.</param>
		/// <param term='status'>The state of the command in the user interface.</param>
		/// <param term='commandText'>Text requested by the neededText parameter.</param>
		/// <seealso class='Exec' />
		public void QueryStatus(string commandName, vsCommandStatusTextWanted neededText, ref vsCommandStatus status, ref object commandText)
		{
			//this.logMessage("Enter: QueryStatus(" + commandName.ToString() + ", " + neededText.ToString() + ", " + ((status == null) ? "null" : "object") + ", " + ((commandText == null) ? "null" : "object") + ")", EventLogEntryType.Information);
			if (neededText == vsCommandStatusTextWanted.vsCommandStatusTextWantedNone)
			{
				if ((commandName == "Inflectra.SpiraTest.IDEIntegration.VisualStudio.Connect.viewDetail") || (commandName == "Inflectra.SpiraTest.IDEIntegration.VisualStudio.Connect.viewTree"))
				{
					status = (vsCommandStatus)vsCommandStatus.vsCommandStatusSupported | vsCommandStatus.vsCommandStatusEnabled;
				}
			}
			//this.logMessage("Exit: QueryStatus", EventLogEntryType.Information);
		}

		/// <summary>Implements the Exec method of the IDTCommandTarget interface. This is called when the command is invoked.</summary>
		/// <param term='commandName'>The name of the command to execute.</param>
		/// <param term='executeOption'>Describes how the command should be run.</param>
		/// <param term='varIn'>Parameters passed from the caller to the command handler.</param>
		/// <param term='varOut'>Parameters passed from the command handler to the caller.</param>
		/// <param term='handled'>Informs the caller if the command was handled or not.</param>
		/// <seealso class='Exec' />
		public void Exec(string commandName, vsCommandExecOption executeOption, ref object varIn, ref object varOut, ref bool handled)
		{
			this.logMessage("Enter: Exec(" + commandName + ", " + executeOption.ToString() + ", " + ((varIn == null) ? "null" : "object") + ", " + ((varOut == null) ? "null" : "object") + ", " + handled.ToString() + ")", EventLogEntryType.Information);

			handled = false;
			if (executeOption == vsCommandExecOption.vsCommandExecOptionDoDefault)
			{
				switch (commandName)
				{
					case "Inflectra.SpiraTest.IDEIntegration.VisualStudio.Connect.viewTree":
						{
							this.logMessage("Exec - ViewTree", EventLogEntryType.Information);

							handled = true;
							if (this.win_ProjectTree == null)
							{
								this.logMessage("Exec - Create Form", EventLogEntryType.Information);
								this.createControls();
							}
							this.win_ProjectTree.Visible = true;
							this.win_ProjectTree.Activate();
						}
						break;
				}
			}

			this.logMessage("Exit: Exec", EventLogEntryType.Information);
		}

		#endregion
	}
}