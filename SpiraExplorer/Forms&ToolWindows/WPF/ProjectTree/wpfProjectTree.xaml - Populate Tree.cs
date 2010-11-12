using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Inflectra.SpiraTest.IDEIntegration.VisualStudio.Spira_ImportExport;
using System.Resources;
 
namespace Inflectra.SpiraTest.IDEIntegration.VisualStudio.WPF.Forms
{
	/// <summary>
	/// Interaction logic for wpfProjectTree.xaml
	/// </summary>
	public partial class wpfProjectTree : UserControl
	{
		private List<Inflectra.SpiraTest.IDEIntegration.VisualStudio.Connect.SpiraProject> _Projects = new List<Connect.SpiraProject>();
		private List<ImportExport> _Clients = new List<ImportExport>();
		//Default node numbers.
		private const int NODE_INCIDENTNUM = 0;
		private const int NODE_TASKNUM = 1;
		private const int NODE_REQUIREMENTNUM = 2;

		private ResourceManager _resources = null;

		private void loadProjects(string ProjList)
		{
			try
			{
				//Get our list of projects.
				this._Projects = new List<Connect.SpiraProject>();
				if (!string.IsNullOrEmpty(ProjList))
				{
					foreach (string strProj in ProjList.Split(Connect.SpiraProject.CHAR_RECORD))
					{
						this._Projects.Add(Connect.SpiraProject.GenerateFromString(strProj));
					}
				}

				//Refresh the treeview.
				this.refreshProjects();
			}
			catch (Exception ex)
			{
				Connect.logEventMessage("wpfProjectTree::loadProjects", ex, System.Diagnostics.EventLogEntryType.Error);
			}
		}

		private void refreshProjects()
		{
			try
			{
				//Turn on the trailing working bar.
				this.barLoading.Visibility = Visibility.Visible;

				//Erase what's in the treeview already.
				this.trvProject.Items.Clear();

				//Cancel and kill any existing clients.
				foreach (ImportExport client in this._Clients)
				{
					try
					{
						client.CancelAsync(null);
						client.Connection_Disconnect();
					}
					catch { };

					client.Dispose();
				}

				//Clear the list.
				this._Clients = new List<ImportExport>();

				//Create the clients.
				foreach (Connect.SpiraProject Project in this._Projects)
				{
					//Create the client.
					ImportExport client = new ImportExport();
					client.UnsafeAuthenticatedConnectionSharing = false;
					client.AllowAutoRedirect = true;
					client.PreAuthenticate = true;
					client.Timeout = 10000;
					client.Url = Project.ServerURL.AbsoluteUri + Connect.SpiraProject.URL_APIADD;
					client.CookieContainer = new System.Net.CookieContainer();

					//Attach events. (Real work done here.)
					client.Connection_Authenticate2Completed += new Connection_Authenticate2CompletedEventHandler(client_FinishConnecting);
					client.System_GetProductVersionCompleted += new System_GetProductVersionCompletedEventHandler(client_FinishConnecting);
					client.Connection_ConnectToProjectCompleted += new Connection_ConnectToProjectCompletedEventHandler(client_FinishConnecting);
					client.Incident_RetrieveCompleted += new Incident_RetrieveCompletedEventHandler(client_FinishIncident);
					client.Requirement_RetrieveCompleted += new Requirement_RetrieveCompletedEventHandler(client_FinishRequirement);
					client.Task_RetrieveCompleted += new Task_RetrieveCompletedEventHandler(client_FinishTask);

					//Create the userstate.
					ObjectState evtObj = new ObjectState();
					evtObj.curSearchIsMine = true;
					evtObj.Project = Project;

					//Add it to the list.
					this._Clients.Add(client);

					//Add a new project node to the tree.
					TreeViewItem projNode = createProjectNode(Project.ToString());
					evtObj.NodeNumber = this.trvProject.Items.Add(projNode);

					//Fire off the connection..
					client.Connection_Authenticate2Async(Project.UserName, Project.UserPass, this._resources.GetString("strAddinProgNamePretty"), evtObj);
				}
				//If no projects, hide the bar.
				if (this._Clients.Count == 0)
				{
					this.barLoading.Visibility = Visibility.Collapsed;
				}
			}
			catch (Exception ex)
			{
				Connect.logEventMessage("wpfProjectTree::refreshProjects", ex, System.Diagnostics.EventLogEntryType.Error);
			}
		}

		#region Client Event Handlers
		/// <summary>Hit when the client is finished connecting and logging in.</summary>
		/// <param name="sender">The client that called it.</param>
		/// <param name="e">Event Args.</param>
		private void client_FinishConnecting(object sender, EventArgs e)
		{
			try
			{
				ImportExport client = (ImportExport)sender;
				string eType = e.GetType().ToString();
				eType = eType.Substring(eType.LastIndexOf('.') + 1);

				switch (eType)
				{
					case "Connection_Authenticate2CompletedEventArgs":
						{
							//Connect finished.
							Connection_Authenticate2CompletedEventArgs evt = (Connection_Authenticate2CompletedEventArgs)e;
							ObjectState evtObj = (ObjectState)evt.UserState;
							if (evt.Error == null)
							{
								client.System_GetProductVersionAsync(evtObj);
							}
							else
							{
								//Set error to node.
								TreeViewItem oldNode = (TreeViewItem)this.trvProject.Items[evtObj.NodeNumber];
								this.trvProject.Items.RemoveAt(evtObj.NodeNumber);
								this.trvProject.Items.Insert(evtObj.NodeNumber, this.changeNodeImage(oldNode, "imgError"));
								((TreeViewItem)this.trvProject.Items[evtObj.NodeNumber]).ToolTip = new TextBlock() { Text = evt.Error.Message };
								((TreeViewItem)this.trvProject.Items[evtObj.NodeNumber]).Items.Clear();
								//Error, clean up.
								removeClient(client);
							}
						}
						break;

					case "System_GetProductVersionCompletedEventArgs":
						{
							//Connect finished.
							System_GetProductVersionCompletedEventArgs evt = (System_GetProductVersionCompletedEventArgs)e;
							ObjectState evtObj = (ObjectState)evt.UserState;
							if (evt.Error == null)
							{
								evtObj.ClientVersion = evt.Result;
								client.Connection_ConnectToProjectAsync(evtObj.Project.ProjectID, evtObj);
							}
							else
							{
								//Set error to node.
								TreeViewItem oldNode = (TreeViewItem)this.trvProject.Items[evtObj.NodeNumber];
								this.trvProject.Items.RemoveAt(evtObj.NodeNumber);
								this.trvProject.Items.Insert(evtObj.NodeNumber, this.changeNodeImage(oldNode, "imgError"));
								((TreeViewItem)this.trvProject.Items[evtObj.NodeNumber]).ToolTip = new TextBlock() { Text = evt.Error.Message };
								((TreeViewItem)this.trvProject.Items[evtObj.NodeNumber]).Items.Clear();
								//Error, clean up.
								removeClient(client);
							}
						}
						break;

					case "Connection_ConnectToProjectCompletedEventArgs":
						{
							//Connect finished.
							Connection_ConnectToProjectCompletedEventArgs evt = (Connection_ConnectToProjectCompletedEventArgs)e;
							ObjectState evtObj = (ObjectState)evt.UserState;
							if (evt.Error == null)
							{
								evtObj.curSearchIsMine = true;
								RemoteFilter[] filters = GenerateFilter(evtObj.Project.UserID, this.btnShowClosed.IsChecked.Value, "IN");
								RemoteSort sort = GenerateSort();
								client.Incident_RetrieveAsync(filters, sort, 1, 9999999, evtObj);
							}
							else
							{
								//Set error to node.
								TreeViewItem oldNode = (TreeViewItem)this.trvProject.Items[evtObj.NodeNumber];
								this.trvProject.Items.RemoveAt(evtObj.NodeNumber);
								this.trvProject.Items.Insert(evtObj.NodeNumber, this.changeNodeImage(oldNode, "imgError"));
								((TreeViewItem)this.trvProject.Items[evtObj.NodeNumber]).ToolTip = new TextBlock() { Text = evt.Error.Message };
								((TreeViewItem)this.trvProject.Items[evtObj.NodeNumber]).Items.Clear();
								//Error, clean up.
								removeClient(client);
							}
						}
						break;
				}
			}
			catch (Exception ex)
			{
				Connect.logEventMessage("wpfProjectTree::client_FinishConnecting", ex, System.Diagnostics.EventLogEntryType.Error);
			}
		}

		/// <summary>Hit when the client is finished getting a list of Incidents.</summary>
		/// <param name="sender">The client that caused the event.</param>
		/// <param name="evt">Event Args.</param>
		private void client_FinishIncident(object sender, Incident_RetrieveCompletedEventArgs evt)
		{
			try
			{
				ImportExport client = (ImportExport)sender;
				ObjectState evtObj = (ObjectState)evt.UserState;

				//Make the current node and the existing parent node.
				TreeViewItem incItemNode = new TreeViewItem();
				TreeViewItem incNode = (TreeViewItem)((TreeViewItem)this.trvProject.Items[evtObj.NodeNumber]).Items[NODE_INCIDENTNUM];

				//Get the label.
				string nodeLabel = ((evtObj.curSearchIsMine) ? "My" : "Unassigned") + " " + this._resources.GetString("strIncidents");

				if (evt.Error == null)
				{
					//Set the image & change text.
					if (evt.Result.Length > 0)
					{
						incItemNode.Header = createNodeHeader("imgFolderIncident", nodeLabel + " (" + evt.Result.Length + ")");
					}
					else
					{
						incItemNode.Header = createNodeHeader("imgFolderIncident", nodeLabel);
					}

					//Add child nodes.
					foreach (RemoteIncident Inc in evt.Result)
					{
						TreeViewItem incidentNode = new TreeViewItem();
						incidentNode.Header = createNodeHeader("imgIncident", Inc.Name);
						incidentNode.MouseDoubleClick += new MouseButtonEventHandler(tree_NodeDoubleClick);
						incidentNode.Tag = Connect.SpiraProject.GenerateToString(evtObj.Project) + Connect.SpiraProject.CHAR_RECORD + "IN:" + Inc.IncidentId.ToString();

						incItemNode.Items.Add(incidentNode);
					}

					incNode.Items.Add(incItemNode);

					//Fire the next step.
					if (evtObj.curSearchIsMine)
					{
						evtObj.curSearchIsMine = false;
						RemoteFilter[] filters = this.GenerateFilter(-999, this.btnShowClosed.IsChecked.Value, "IN");
						RemoteSort sort = this.GenerateSort();
						client.Incident_RetrieveAsync(filters, sort, 1, 9999999, evtObj);
					}
					else
					{
						evtObj.curSearchIsMine = true;
						RemoteFilter[] filters = this.GenerateFilter(evtObj.Project.UserID, this.btnShowClosed.IsChecked.Value, "TK");
						RemoteSort sort = this.GenerateSort();
						client.Task_RetrieveAsync(filters, sort, 1, 9999999, evtObj);
					}
				}
				else
				{
					//Set error flag on node.
					incItemNode = changeNodeImage(incItemNode, "imgError");
					incItemNode.Items.Clear();
					incItemNode.ToolTip = new TextBlock() { Text = evt.Error.Message };

					//Add it to our projectnode.
					incNode.Items.Add(incItemNode);

					//Error, clean up.
					removeClient(client);
				}
			}
			catch (Exception ex)
			{
				Connect.logEventMessage("wpfProjectTree::client_FinishIncident", ex, System.Diagnostics.EventLogEntryType.Error);
			}
		}

		/// <summary>Hit when the client is finishe getting a list of Requirements.</summary>
		/// <param name="sender">The client that caused the event.</param>
		/// <param name="evt">Event Args.</param>
		private void client_FinishRequirement(object sender, Requirement_RetrieveCompletedEventArgs evt)
		{
			try
			{
				ImportExport client = (ImportExport)sender;
				ObjectState evtObj = (ObjectState)evt.UserState;

				//Make the current node and the existing parent node.
				TreeViewItem reqItemNode = new TreeViewItem();
				TreeViewItem reqNode = (TreeViewItem)((TreeViewItem)this.trvProject.Items[evtObj.NodeNumber]).Items[NODE_REQUIREMENTNUM];
				reqItemNode = reqNode;

				if (evt.Error == null)
				{
					//Add child nodes.
					int numCounted = 0;
					foreach (RemoteRequirement Req in evt.Result)
					{
						if (!Req.Summary)
						{
							numCounted++;
							TreeViewItem reqDetNode = new TreeViewItem();
							reqDetNode.Header = createNodeHeader("imgRequirement", Req.Name);
							reqDetNode.MouseDoubleClick += new MouseButtonEventHandler(tree_NodeDoubleClick);
							reqDetNode.Tag = Connect.SpiraProject.GenerateToString(evtObj.Project) + Connect.SpiraProject.CHAR_RECORD + "RQ:" + Req.RequirementId.ToString();

							reqItemNode.Items.Add(reqDetNode);
						}
					}

					//Set the image & change text.
					if (numCounted > 0)
					{
						reqItemNode = this.changeNodeText(reqItemNode, this._resources.GetString("strRequirements") + " (" + numCounted.ToString() + ")");
					}

					//Clean up.
					removeClient(client);
				}
				else
				{
					//Set error flag on node.
					reqItemNode = changeNodeImage(reqItemNode, "imgError");
					reqItemNode.Items.Clear();
					reqItemNode.ToolTip = new TextBlock() { Text = evt.Error.Message };

					//Error, clean up.
					removeClient(client);
				}
			}
			catch (Exception ex)
			{
				Connect.logEventMessage("wpfProjectTree::client_FinishRequirement", ex, System.Diagnostics.EventLogEntryType.Error);
			}
		}

		/// <summary>Hit when the client is finished getting a list of Tasks for the user.</summary>
		/// <param name="sender">The client that caused the event.</param>
		/// <param name="evt">Event Args.</param>
		private void client_FinishTask(object sender, Task_RetrieveCompletedEventArgs evt)
		{
			try
			{
				ImportExport client = (ImportExport)sender;
				ObjectState evtObj = (ObjectState)evt.UserState;

				//Make the current node and the existing parent node.
				TreeViewItem tskItemNode = new TreeViewItem();
				TreeViewItem tskNode = (TreeViewItem)((TreeViewItem)this.trvProject.Items[evtObj.NodeNumber]).Items[NODE_TASKNUM];

				//Get the label.
				string nodeLabel = ((evtObj.curSearchIsMine) ? "My" : "Unassigned") + " " + this._resources.GetString("strTasks");

				if (evt.Error == null)
				{
					//Set the image & change text.
					if (evt.Result.Length > 0)
					{
						tskItemNode.Header = createNodeHeader("imgFolderTask", nodeLabel + " (" + evt.Result.Length + ")");
					}
					else
					{
						tskItemNode.Header = createNodeHeader("imgFolderTask", nodeLabel);
					}

					//Add child nodes.
					foreach (RemoteTask Tsk in evt.Result)
					{
						TreeViewItem taskNode = new TreeViewItem();
						taskNode.Header = createNodeHeader("imgTask", Tsk.Name);
						taskNode.MouseDoubleClick += new MouseButtonEventHandler(tree_NodeDoubleClick);
						taskNode.Tag = Connect.SpiraProject.GenerateToString(evtObj.Project) + Connect.SpiraProject.CHAR_RECORD + "TK:" + Tsk.TaskId.ToString();

						tskItemNode.Items.Add(taskNode);
					}

					tskNode.Items.Add(tskItemNode);

					//Fire the next step.
					if (evtObj.curSearchIsMine)
					{
						evtObj.curSearchIsMine = false;
						RemoteFilter[] filters = this.GenerateFilter(-999, this.btnShowClosed.IsChecked.Value, "TK");
						RemoteSort sort = this.GenerateSort();
						client.Task_RetrieveAsync(filters, sort, 1, 9999999, evtObj);
					}
					else
					{
						evtObj.curSearchIsMine = true;
						RemoteFilter[] filters = this.GenerateFilter(evtObj.Project.UserID, this.btnShowClosed.IsChecked.Value, "RQ");
						RemoteSort sort = this.GenerateSort();
						client.Requirement_RetrieveAsync(filters, 1, 9999999, evtObj);
					}
				}
				else
				{
					//Set error flag on node.
					tskItemNode = changeNodeImage(tskItemNode, "imgError");
					tskItemNode.Items.Clear();
					tskItemNode.ToolTip = new TextBlock() { Text = evt.Error.Message };

					//Add it to our projectnode.
					tskNode.Items.Add(tskItemNode);

					//Error, clean up.
					removeClient(client);
				}
			}
			catch (Exception ex)
			{
				Connect.logEventMessage("wpfProjectTree::client_FinishTask", ex, System.Diagnostics.EventLogEntryType.Error);
			}

		}
		#endregion

		/// <summary>Will erase all nodes in the treeview and display the 'No Solution Loaded' message.</summary>
		private void noSolutionLoaded()
		{
			try
			{
				this.trvProject.Items.Clear();
				this.trvProject.Items.Add(this._nodeNoSolution);
				this.btnShowClosed.IsEnabled = false;
				this.btnRefresh.IsEnabled = false;
			}
			catch (Exception ex)
			{
				Connect.logEventMessage("wpfProjectTree::noSolutionLoaded", ex, System.Diagnostics.EventLogEntryType.Error);
			}
		}

		/// <summary>Will erase all nodes in the treeview and display the 'No Projects Loaded' message.</summary>
		private void noProjectsLoaded()
		{
			try
			{
				this.trvProject.Items.Clear();
				this.trvProject.Items.Add(this._nodeNoProjects);
				this.btnShowClosed.IsEnabled = false;
				this.btnRefresh.IsEnabled = false;
			}
			catch (Exception ex)
			{
				Connect.logEventMessage("wpfProjectTree::noProjectsLoaded", ex, System.Diagnostics.EventLogEntryType.Error);
			}
		}

		/// <summary>Removes the client from the list and possible enables form.</summary>
		/// <param name="client">The client to kill and remove.</param>
		private void removeClient(ImportExport client)
		{
			try
			{

				try
				{
					client.CancelAsync(null);
					//client.Connection_Disconnect();
				}
				catch
				{
				}
				client.Dispose();

				//Remove it from the list.
				this._Clients.Remove(client);

				//Null it out.
				client = null;


				if (this._Clients.Count == 0)
				{
					//Enable form.
					this.btnRefresh.IsEnabled = true;
					this.btnShowClosed.IsEnabled = true;
					this.barLoading.Visibility = Visibility.Collapsed;
				}
			}
			catch (Exception ex)
			{
				Connect.logEventMessage("wpfProjectTree::removeClient", ex, System.Diagnostics.EventLogEntryType.Error);
			}
		}

		private bool checkClientVersion(RemoteVersion version)
		{
			//Return 'true' if version is 2.3.1 p15 or greater. False if not.
			string[] verNums = version.Version.Split('.');

			return false;
		}
	}
}
