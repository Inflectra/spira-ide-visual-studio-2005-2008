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
		// Standard nodes.
		private TreeViewItem _nodeNoSolution = null;
		private TreeViewItem _nodeNoProjects = null;
		private TreeViewItem _nodeError = null;

		/// <summary>Creates the standard nodes. Run at class creation.</summary>
		private void CreateStandardNodes()
		{
			try
			{
				//Define our standard nodes here.
				// - No Projects
				this._nodeNoProjects = new TreeViewItem();
				this._nodeNoProjects.Header = createNodeHeader("imgNo", "No projects selected for this solution.");

				// - No Solution
				this._nodeNoSolution = new TreeViewItem();
				this._nodeNoSolution.Header = createNodeHeader("imgNo", "No solution loaded.");

				// - Error node.
				this._nodeError = new TreeViewItem();
				this._nodeError.Header = createNodeHeader("imgError", "Error contacting server.");
			}
			catch (Exception ex)
			{
				Connect.logEventMessage("wpfProjectTree::CreateStandardNodes", ex, System.Diagnostics.EventLogEntryType.Error);
			}
		}

		/// <summary>Will change the text label of a TreeViewItem. The TreeViewItem must have a TextBlock in the Header control.</summary>
		/// <param name="inNode">The node to change.</param>
		/// <param name="Label">The string to change it to.</param>
		/// <returns>New modified treenode.</returns>
		private TreeViewItem changeNodeText(TreeViewItem inNode, string Label)
		{
			try
			{
				//Loop through and find the first textblock.
				int ctrlIndex = -1;
				for (int I = 0; I < ((StackPanel)inNode.Header).Children.Count; I++)
				{
					if (((StackPanel)inNode.Header).Children[I].GetType() == typeof(TextBlock))
					{
						ctrlIndex = I;
					}
				}

				if (ctrlIndex > -1)
				{
					((TextBlock)((StackPanel)inNode.Header).Children[ctrlIndex]).Text = Label;
				}
			}
			catch (Exception ex)
			{
				Connect.logEventMessage("wpfProjectTree::changeNodeText", ex, System.Diagnostics.EventLogEntryType.Error);
			}
			return inNode;
		}

		/// <summary>Creates a node display header.</summary>
		/// <param name="imageKey">The image key to use. null skips image.</param>
		/// <param name="Label">The text to use.</param>
		/// <returns>A StackPanel that can be saved to the node's Header property.</returns>
		private StackPanel createNodeHeader(string imageKey, string Label)
		{
			try
			{
				StackPanel stckPnl = new StackPanel() { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 2, 2, 2) };
				if (!string.IsNullOrEmpty(imageKey))
				{
					stckPnl.Children.Add(getImage(imageKey, new Size(16, 16)));
				}
				stckPnl.Children.Add(new TextBlock() { Text = Label, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(3, 0, 0, 0) });

				return stckPnl;
			}
			catch (Exception ex)
			{
				Connect.logEventMessage("wpfProjectTree::createNodeHeader", ex, System.Diagnostics.EventLogEntryType.Error);
				return new StackPanel();
			}
		}

		/// <summary>Will change the image label of a TreeViewItem. The TreeViewItem must have a TextBlock in the Header control.</summary>
		/// <param name="inNode">The node to change.</param>
		/// <param name="Label">The image key to change to. null will remove the image.</param>
		/// <returns>New modified treenode.</returns>
		private TreeViewItem changeNodeImage(TreeViewItem inNode, string imageKey)
		{
			try
			{
				//Loop through and find the first textblock.
				int ctrlIndex = -1;
				for (int I = 0; I < ((StackPanel)inNode.Header).Children.Count; I++)
				{
					if (((StackPanel)inNode.Header).Children[I].GetType() == typeof(Image))
					{
						ctrlIndex = I;
					}
				}

				if (ctrlIndex > -1)
				{
					if (!string.IsNullOrEmpty(imageKey))
					{
						((StackPanel)inNode.Header).Children.RemoveAt(ctrlIndex);
						((StackPanel)inNode.Header).Children.Insert(ctrlIndex, getImage(imageKey, new Size(16, 16)));
					}
					else
					{
						((StackPanel)inNode.Header).Children.RemoveAt(ctrlIndex);
					}
				}
			}
			catch (Exception ex)
			{
				Connect.logEventMessage("wpfProjectTree::changeNodeImage", ex, System.Diagnostics.EventLogEntryType.Error);
			}
			return inNode;
		}

		/// <summary>Creates a project node.</summary>
		/// <param name="projectText">name to display on the node.</param>
		/// <returns>TreeViewitem with folders created.</returns>
		private TreeViewItem createProjectNode(string projectText)
		{
			try
			{
				TreeViewItem retNode = new TreeViewItem();
				//retNode.Resources.Add(SystemColors.HighlightBrushKey, Brushes.LightGreen);


				// - Standard Project Node.
				retNode.Header = createNodeHeader("imgFolder", projectText);

				TreeViewItem nodeIncident = new TreeViewItem();
				nodeIncident.Header = createNodeHeader("imgFolderIncident", "Incidents");


				TreeViewItem nodeTasks = new TreeViewItem();
				nodeTasks.Header = createNodeHeader("imgFolderTask", "Tasks");

				TreeViewItem nodeReqs = new TreeViewItem();
				nodeReqs.Header = createNodeHeader("imgFolderRequirement", "Requirements");

				retNode.Items.Add(nodeIncident);
				retNode.Items.Add(nodeTasks);
				retNode.Items.Add(nodeReqs);

				return retNode;
			}
			catch (Exception ex)
			{
				Connect.logEventMessage("wpfProjectTree::createProjectNode", ex, System.Diagnostics.EventLogEntryType.Error);
				return new TreeViewItem();
			}
		}
	}
}
