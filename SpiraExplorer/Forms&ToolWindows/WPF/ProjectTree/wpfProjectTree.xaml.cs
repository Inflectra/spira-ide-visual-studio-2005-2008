using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Inflectra.SpiraTest.IDEIntegration.VisualStudio.Spira_ImportExport;
using Inflectra.SpiraTest.IDEIntegration.VisualStudio.WinForms.Forms;

namespace Inflectra.SpiraTest.IDEIntegration.VisualStudio.WPF.Forms 
{
	/// <summary>
	/// Interaction logic for wpfProjectTree.xaml
	/// </summary>
	public partial class wpfProjectTree : UserControl
	{
		// Other Settings
		Connect.SettingsClass.XMLFileReader _settingsFile = null;
		string _solutionName = null;

		//Click events.
		public event EventHandler OpenDetails;
		//public event EventHandler OpenIncident;
		//public event EventHandler OpenTask;

		public wpfProjectTree()
		{
			try
			{
				InitializeComponent();

				//Get resources.
				this._resources = Connect.getCultureResource;

				//Set button images and events.
				// - Config button
				Image btnConfigImage = getImage("imgSettings", new Size(16, 16));
				btnConfigImage.Stretch = Stretch.None;
				this.btnConfig.Content = btnConfigImage;
				this.btnConfig.Click += new RoutedEventHandler(btnConfig_Click);
				// - Show Completed button
				Image btnCompleteImage = getImage("imgShowCompleted", new Size(16, 16));
				btnCompleteImage.Stretch = Stretch.None;
				this.btnShowClosed.Content = btnCompleteImage;
				this.btnShowClosed.IsEnabledChanged += new DependencyPropertyChangedEventHandler(toolButton_IsEnabledChanged);
				this.btnShowClosed.Click += new RoutedEventHandler(btnRefresh_Click);
				// - Refresh Button
				Image btnRefreshImage = getImage("imgRefresh", new Size(16, 16));
				btnRefreshImage.Stretch = Stretch.None;
				this.btnRefresh.Content = btnRefreshImage;
				this.btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click);
				this.btnRefresh.IsEnabledChanged += new DependencyPropertyChangedEventHandler(toolButton_IsEnabledChanged);
				// - Set bar color.
				this.barLoading.Foreground = (Brush)new System.Windows.Media.BrushConverter().ConvertFrom(this._resources.GetString("barForeColor"));

				//Load nodes.
				this.CreateStandardNodes();
			}
			catch (Exception ex)
			{
				Connect.logEventMessage("wpfProjectTree::.ctor()", ex, System.Diagnostics.EventLogEntryType.Error);
			}
		}

		/// <summary>Hit when the user wants to refresh the list.</summary>
		/// <param name="sender">btnRefresh, btnShowClosed</param>
		/// <param name="e">Event Args</param>
		void btnRefresh_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				this.setSolution(this._solutionName);
			}
			catch (Exception ex)
			{
				Connect.logEventMessage("wpfProjectTree::btnRefresh_Click", ex, System.Diagnostics.EventLogEntryType.Error);
			}
		}

		private void toolButton_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			try
			{
				if (sender.GetType() == typeof(Button))
				{
					Button btn = (Button)sender;

					btn.Opacity = ((btn.IsEnabled) ? 1 : .5);
				}
				else if (sender.GetType() == typeof(System.Windows.Controls.Primitives.ToggleButton))
				{
					System.Windows.Controls.Primitives.ToggleButton btn = (System.Windows.Controls.Primitives.ToggleButton)sender;

					btn.Opacity = ((btn.IsEnabled) ? 1 : .5);
				}
			}
			catch (Exception ex)
			{
				Connect.logEventMessage("wpfProjectTree::toolButton_IsEnabledChanged", ex, System.Diagnostics.EventLogEntryType.Error);
			}
		}

		/// <summary>Converts a resource to a WPF image. Needed for application resources.</summary>
		/// <param name="image">Bitmap of the image to convert.</param>
		/// <returns>BitmapSource suitable for an Image control.</returns>
		private BitmapSource getBMSource(System.Drawing.Bitmap image)
		{
			try
			{
				if (image != null)
				{
					IntPtr bmStream = image.GetHbitmap();
					return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(bmStream, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromWidthAndHeight(image.Width, image.Height));
				}
				return null;
			}
			catch (Exception ex)
			{
				Connect.logEventMessage("wpfProjectTree::getBMSource", ex, System.Diagnostics.EventLogEntryType.Error);
				return null;
			}
		}

		/// <summary>Creates an Image control for a specified resource.</summary>
		/// <param name="Key">The key name of the resource to use. Will search and use Product-dependent resources first.</param>
		/// <param name="Size">Size of the desired image, or null.</param>
		/// <param name="Stretch">Desired stretch setting of image, or null.</param>
		/// <returns>Resulting image, or null if key is not found.</returns>
		private Image getImage(string key, Size size)
		{
			try
			{
				Image retImage = new Image();
				if (size != null)
				{
					retImage.Height = size.Height;
					retImage.Width = size.Width;
				}

				BitmapSource image = null;
				image = getBMSource((System.Drawing.Bitmap)this._resources.GetObject(key));

				retImage.Source = image;

				return retImage;
			}
			catch (Exception ex)
			{
				Connect.logEventMessage("wpfProjectTree::getImage", ex, System.Diagnostics.EventLogEntryType.Error);
				return null;
			}
		}

		private void btnConfig_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				wpfAssignProject configWPF = new wpfAssignProject();

				//Set the settings and solution name.
				configWPF.setSettings(this._settingsFile);
				configWPF.setSolution(this._solutionName);

				if (configWPF.ShowDialog().Value)
				{
					//Reload projects.
					this.setSolution(this._solutionName);
				}
			}
			catch (Exception ex)
			{
				Connect.logEventMessage("wpfProjectTree::btnConfig_Click", ex, System.Diagnostics.EventLogEntryType.Error);
			}
		}

		private UIElement getNodeHeader(string HeaderText, ItemTypeEnumeration LeadImage)
		{
			try
			{
				StackPanel retHead = new StackPanel();
				retHead.Orientation = Orientation.Horizontal;

				//Get the image.
				Image headImage = new Image();
				switch (LeadImage)
				{
					case ItemTypeEnumeration.Incident:
						headImage = this.getImage("icoIncident", new Size(16, 16));
						break;
					case ItemTypeEnumeration.Requirement:
						headImage = this.getImage("icoRequirement", new Size(16, 16));
						break;
					case ItemTypeEnumeration.Task:
						headImage = this.getImage("icoTask", new Size(16, 16));
						break;
					default:
						headImage = this.getImage("icoError", new Size(16, 16));
						break;
				}
				TextBlock headText = new TextBlock();
				headText.Text = HeaderText;

				if (LeadImage != ItemTypeEnumeration.None)
					retHead.Children.Add(headImage);
				retHead.Children.Add(headText);

				return retHead;
			}
			catch (Exception ex)
			{
				Connect.logEventMessage("wpfProjectTree::getNodeHeader", ex, System.Diagnostics.EventLogEntryType.Error);
				return null;
			}
		}

		private class ObjectState
		{
			public bool curSearchIsMine = true;
			public int NodeNumber;
			public Connect.SpiraProject Project;
			public Spira_ImportExport.RemoteVersion ClientVersion;
		}

		private enum ItemTypeEnumeration
		{
			None = 0,
			Task = 1,
			Incident = 2,
			Requirement = 3,
			Folder = 4,
			HasItems = 10,
			HasNoItems = 11,
			Error = -999
		}

		/// <summary>Generates a filter for pulling information from the Spira server.</summary>
		/// <param name="UserNum">The user number of the data to pull for.</param>
		/// <param name="IncludeComplete">Whether or not to include completed/closed items.</param>
		/// <param name="IncTypeCode">The string type code of the artifact. "TK", "IN", "RQ"</param>
		/// <returns>A RemoteFilter set.</returns>
		private RemoteFilter[] GenerateFilter(int UserNum, bool IncludeComplete, string IncTypeCode)
		{
			try
			{
				RemoteFilter userFilter = new RemoteFilter() { PropertyName = "OwnerId", IntValue = UserNum };
				RemoteFilter statusFilter = new RemoteFilter();
				if (!IncludeComplete)
				{
					switch (IncTypeCode.ToUpperInvariant())
					{
						case "IN":
							{
								statusFilter = new RemoteFilter() { PropertyName = "IncidentStatusId", IntValue = -2 };
							}
							break;

						case "TK":
							{
								MultiValueFilter multiValue = new MultiValueFilter();
								multiValue.Values = new int[] { 1, 2, 4, 5 };
								statusFilter = new RemoteFilter() { PropertyName = "TaskStatusId", MultiValue = multiValue };
							}
							break;

						case "RQ":
							{
								MultiValueFilter multiValue = new MultiValueFilter();
								multiValue.Values = new int[] { 1, 2, 3, 5, 7 };
								statusFilter = new RemoteFilter() { PropertyName = "ScopeLevelId", MultiValue = multiValue };
							}
							break;
					}
				}

				return new RemoteFilter[] { userFilter, statusFilter };
			}
			catch (Exception ex)
			{
				Connect.logEventMessage("wpfProjectTree::GenerateFilter", ex, System.Diagnostics.EventLogEntryType.Error);
				return null;
			}
		}

		private RemoteSort GenerateSort()
		{
			RemoteSort sort = new RemoteSort();
			sort.PropertyName = "UserId";
			sort.SortAscending = true;

			return sort;
		}

		private void tree_NodeDoubleClick(object sender, EventArgs evt)
		{
			try
			{
				string itemTag = (string)((TreeViewItem)sender).Tag;

				this.OpenDetails(itemTag, new EventArgs());
			}
			catch (Exception ex)
			{
				Connect.logEventMessage("wpfProjectTree::tree_NodeDoubleClick", ex, System.Diagnostics.EventLogEntryType.Error);
			}
		}
	}
}
