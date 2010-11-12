using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media.Imaging;
using Inflectra.SpiraTest.IDEIntegration.VisualStudio.WinForms.Forms;
using System.Resources;
using System.Windows.Markup;
using System.Xml;
using System.IO;

namespace Inflectra.SpiraTest.IDEIntegration.VisualStudio.WPF.Forms 
{
	/// <summary>
	/// Interaction logic for wpfServerProject.xaml
	/// </summary>
	public partial class wpfAssignProject : Window
	{
		//Boolean whether or not data was saved.
		private bool hasChanged = false;
		//Resources
		private ResourceManager _resources = null;

		/// <summary>Creates a new instance of the form. Should call setSpiraProjects() and setSoltion() after calling this.</summary>
		internal wpfAssignProject()
		{
			try
			{
				InitializeComponent();

				//Get the resources.
				this._resources = Connect.getCultureResource;

				//Title
				this.Title = this._resources.GetString("strAssignProjectTitle");
				//Icon
				try
				{
					System.Drawing.Icon ico = (System.Drawing.Icon)this._resources.GetObject("icoLogo");
					MemoryStream icoStr = new MemoryStream();
					ico.Save(icoStr);
					icoStr.Seek(0, SeekOrigin.Begin);
					this.Icon = BitmapFrame.Create(icoStr);
				}
				catch (Exception ex)
				{
					Connect.logEventMessage("wpfNewSpiraProject::.ctor::MakeIcon", ex, System.Diagnostics.EventLogEntryType.Error);
				}

				//Load logos and images.
				this.imgLogo.Source = getImage("imgLogo", new Size()).Source;
				this.imgLogo.Height = imgLogo.Source.Height;
				this.imgLogo.Width = imgLogo.Source.Width;
				this.btnNew.Content = getImage("imgAdd", new Size(16, 16));
				this.btnEdit.Content = getImage("imgEdit", new Size(16, 16));
				this.btnDelete.Content = getImage("imgDelete", new Size(16, 16));

				//Set events.
				this.btnEdit.IsEnabledChanged += new DependencyPropertyChangedEventHandler(btn_IsEnabledChanged);
				this.btnDelete.IsEnabledChanged += new DependencyPropertyChangedEventHandler(btn_IsEnabledChanged);
				this.btnNew.Click += new RoutedEventHandler(btnNewEdit_Click);
				this.btnEdit.Click += new RoutedEventHandler(btnNewEdit_Click);
				this.btnAdd.Click += new RoutedEventHandler(btnAdd_Click);
				this.btnRemove.Click += new RoutedEventHandler(btnRemove_Click);
				this.btnSave.Click += new RoutedEventHandler(btnSave_Click);
				this.btnCancel.Click += new RoutedEventHandler(btnCancel_Click);
				this.btnDelete.Click += new RoutedEventHandler(btnDelete_Click);
				this.btn_IsEnabledChanged(this.btnEdit, new DependencyPropertyChangedEventArgs());
				this.btn_IsEnabledChanged(this.btnDelete, new DependencyPropertyChangedEventArgs());

				//Set the caption.
				this.setRTFCaption();
			}
			catch (Exception ex)
			{
				Connect.logEventMessage("wpfAssignProject::.ctor", ex, System.Diagnostics.EventLogEntryType.Error);
			}
		}

		/// <summary>Hit when the user decides they want to delete a project.</summary>
		/// <param name="sender">btnDelete</param>
		/// <param name="e">Event Args</param>
		void btnDelete_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				string sureMsg = "Are you sure you want to delete project:" + Environment.NewLine + ((Connect.SpiraProject)this.lstAvailProjects.SelectedItem).ToString();
				MessageBoxResult userSure = MessageBox.Show(sureMsg, "Remove Project?", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes);

				if (userSure == MessageBoxResult.Yes)
				{
					this.hasChanged = true;
					this.lstAvailProjects.Items.RemoveAt(this.lstAvailProjects.SelectedIndex);
				}
			}
			catch (Exception ex)
			{
				Connect.logEventMessage("wpfAssignProject::btnDelete_Click", ex, System.Diagnostics.EventLogEntryType.Error);
			}
		}

		/// <summary>Hit when the Cencel button on the form is clicked.</summary>
		/// <param name="sender">btnCancel</param>
		/// <param name="e">Event Args</param>
		void btnCancel_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				if (this.hasChanged)
				{
					MessageBoxResult OKtoClose = MessageBox.Show("Lose changes made to settings?", "Close?", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Cancel);
					if (OKtoClose == MessageBoxResult.Yes)
					{
						this.DialogResult = false;
					}
				}
				else
				{
					this.DialogResult = false;
				}
			}
			catch (Exception ex)
			{
				Connect.logEventMessage("wpfAssignProject::btnCancel_Click", ex, System.Diagnostics.EventLogEntryType.Error);
			}
		}

		/// <summary>Hit when the Save button is clicked.</summary>
		/// <param name="sender">btnSave</param>
		/// <param name="e">Event Args</param>
		void btnSave_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				//The user wanted to save. Save our settings and raise the closeform event.
				string selProjects = "";
				string availProjects = "";

				foreach (Connect.SpiraProject proj in this.lstAvailProjects.Items)
				{
					availProjects += Connect.SpiraProject.GenerateToString(proj) + Connect.SpiraProject.CHAR_RECORD;
				}
				foreach (Connect.SpiraProject proj in this.lstSelectProjects.Items)
				{
					string projstr = Connect.SpiraProject.GenerateToString(proj) + Connect.SpiraProject.CHAR_RECORD;
					availProjects += projstr;
					selProjects += projstr;
				}
				availProjects = availProjects.Trim(Connect.SpiraProject.CHAR_RECORD);
				selProjects = selProjects.Trim(Connect.SpiraProject.CHAR_RECORD);

				this._Settings.SetValue("General", "Projects", availProjects);
				this._Settings.SetValue(this._solName, "Projects", selProjects);
				this._Settings.Save();

				this.DialogResult = true;
			}
			catch (Exception ex)
			{
				Connect.logEventMessage("wpfAssignProject::btnSave_Click", ex, System.Diagnostics.EventLogEntryType.Error);
			}
		}

		/// <summary>Remove the selected projects from the 'Selected' list.</summary>
		/// <param name="sender">The btnRemove</param>
		/// <param name="e">Event Args</param>
		void btnRemove_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				//Like removing duplicates, but in reverse.
				List<Connect.SpiraProject> SelProjs = new List<Connect.SpiraProject>();
				foreach (Connect.SpiraProject proj in this.lstSelectProjects.SelectedItems)
				{
					this.lstAvailProjects.Items.Add(proj);
					SelProjs.Add(proj);
				}

				foreach (Connect.SpiraProject proj in SelProjs)
				{
					for (int i = 0; i < this.lstSelectProjects.Items.Count; )
					{
						if (proj.IsEqualTo((Connect.SpiraProject)this.lstSelectProjects.Items[i]))
						{
							this.lstSelectProjects.Items.RemoveAt(i);
						}
						else
						{
							i++;
						}
					}
				}

				this.hasChanged = true;
			}
			catch (Exception ex)
			{
				Connect.logEventMessage("wpfAssignProject::btnRemove_Click", ex, System.Diagnostics.EventLogEntryType.Error);
			}
		}

		/// <summary>Hit when the user wants to assign a serverproject.</summary>
		/// <param name="sender">btnAdd</param>
		/// <param name="e">Event Args</param>
		void btnAdd_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				//Add selected items to the solution project.
				foreach (object selItem in this.lstAvailProjects.SelectedItems)
				{
					//Add it to the selected items panel.
					this.lstSelectProjects.Items.Add(selItem);
				}

				//Remove dplicates.
				this.removeDuplicates();

				this.hasChanged = true;
			}
			catch (Exception ex)
			{
				Connect.logEventMessage("wpfAssignProject::btnAdd_Click", ex, System.Diagnostics.EventLogEntryType.Error);
			}
		}

		/// <summary>Hit when the user wants to add/edit a serverproject.</summary>
		/// <param name="sender">btnNew / btnEdit</param>
		/// <param name="e">Event Args</param>
		void btnNewEdit_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				Button button = (Button)sender;

				//Create the form.
				wpfNewSpiraProject frmAddProject = new wpfNewSpiraProject();

				if (button.Name == "btnEdit")
				{
					//Get the item selected.
					Connect.SpiraProject proj = (Connect.SpiraProject)this.lstAvailProjects.SelectedItem;
					frmAddProject.txbServer.Text = proj.ServerURL.AbsoluteUri;
					frmAddProject.txbUserID.Text = proj.UserName;
					frmAddProject.txbUserPass.Password = proj.UserPass;
					int projnum = frmAddProject.cmbProjectList.Items.Add(proj);
					frmAddProject.cmbProjectList.SelectedIndex = projnum;
				}

				if (frmAddProject.ShowDialog().Value)
				{
					if (frmAddProject.cmbProjectList.SelectedItem != null)
					{
						Connect.SpiraProject selProject = (Connect.SpiraProject)frmAddProject.cmbProjectList.SelectedItem;

						//Add it to the available list if there's no existing ones.
						bool AddToSelected = false;
						for (int i = 0; i < this.lstAvailProjects.Items.Count; )
						{
							if (((Connect.SpiraProject)this.lstAvailProjects.Items[i]).IsEqualTo(selProject))
							{
								this.lstAvailProjects.Items.RemoveAt(i);
							}
							else
							{
								i++;
							}
						}
						for (int i = 0; i < this.lstSelectProjects.Items.Count; )
						{
							if (((Connect.SpiraProject)this.lstSelectProjects.Items[i]).IsEqualTo(selProject))
							{
								this.lstSelectProjects.Items.RemoveAt(i);
								AddToSelected = true;
							}
							else
							{
								i++;
							}
						}

						if (AddToSelected)
						{
							this.lstSelectProjects.Items.Add(selProject);
						}
						else
						{
							this.lstAvailProjects.Items.Add(selProject);
						}
					}
					this.hasChanged = true;
				}
			}
			catch (Exception ex)
			{
				Connect.logEventMessage("wpfAssignProject::btnNewEdit_Click", ex, System.Diagnostics.EventLogEntryType.Error);
			}
		}

		/// <summary>Hit when a button IsEnabled is changed.</summary>
		/// <param name="sender">btnEdit / btnDelete</param>
		/// <param name="e">Event Args</param>
		void btn_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			try
			{
				Button button = (Button)sender;
				((Image)button.Content).Opacity = ((button.IsEnabled) ? 1 : .5);
			}
			catch (Exception ex)
			{
				Connect.logEventMessage("wpfAssignProject::btn_IsEnabledChanged", ex, System.Diagnostics.EventLogEntryType.Error);
			}
		}

		/// <summary>Hit when either listbox's selection changes. Sets required button states.</summary>
		/// <param name="sender">Control that sent the event.</param>
		/// <param name="e">SelectionChangedEventArgs</param>
		private void listbox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			try
			{
				ListBox userInteract = (ListBox)sender;

				switch (userInteract.Name)
				{
					case "lstAvailProjects":
						{
							if (userInteract.SelectedItems.Count > 0)
							{
								this.btnAdd.IsEnabled = this.lstSelectProjects.IsEnabled;
								this.btnEdit.IsEnabled = (userInteract.SelectedItems.Count == 1);
								this.btnDelete.IsEnabled = (userInteract.SelectedItems.Count == 1);
							}
							else
							{
								this.btnAdd.IsEnabled = false;
								this.btnEdit.IsEnabled = false;
								this.btnDelete.IsEnabled = false;
							}
						}
						break;

					case "lstSelectProjects":
						{
							if (userInteract.SelectedItems.Count > 0)
							{
								this.btnRemove.IsEnabled = this.lstSelectProjects.IsEnabled;
							}
							else
							{
								this.btnRemove.IsEnabled = false;
							}
						}
						break;
				}
			}
			catch (Exception ex)
			{
				Connect.logEventMessage("wpfAssignProject::listbox_SelectionChanged", ex, System.Diagnostics.EventLogEntryType.Error);
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
				if (!size.IsEmpty && (size.Height != 0 && size.Width != 0))
				{
					retImage.Height = size.Height;
					retImage.Width = size.Width;
				}

				BitmapSource image = null;
				try
				{
					image = getBMSource((System.Drawing.Bitmap)this._resources.GetObject(key));
				}
				catch
				{
				}

				retImage.Source = image;

				return retImage;
			}
			catch (Exception ex)
			{
				Connect.logEventMessage("wpfAssignProject::getImage", ex, System.Diagnostics.EventLogEntryType.Error);
				return new Image();
			}
		}

		/// <summary>Converts a resource to a WPF image. Needed for application resources.</summary>
		/// <param name="image">Bitmap of the image to convert.</param>
		/// <returns>BitmapSource suitable for an Image control.</returns>
		private BitmapSource getBMSource(Object image)
		{
			try
			{
				if (image != null && image is System.Drawing.Bitmap)
				{
					System.Drawing.Bitmap imageCast = (System.Drawing.Bitmap)image;
					IntPtr bmStream = imageCast.GetHbitmap();
					System.Windows.Int32Rect rect = new Int32Rect(0, 0, imageCast.Width, imageCast.Height);

					return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(bmStream, IntPtr.Zero, rect, BitmapSizeOptions.FromEmptyOptions());
				}
			}
			catch (Exception ex)
			{
				Connect.logEventMessage("wpfAssignProject::getBMSource", ex, System.Diagnostics.EventLogEntryType.Error);
			}
			return null;
		}

		/// <summary>Removes any entries in the Available list that are also in the Selected list.</summary>
		private void removeDuplicates()
		{
			try
			{
				foreach (Connect.SpiraProject proj in this.lstSelectProjects.Items)
				{
					//Loop through the ones available..
					for (int i = 0; i < this.lstAvailProjects.Items.Count; )
					{
						if (((Connect.SpiraProject)this.lstAvailProjects.Items[i]).IsEqualTo(proj))
						{
							this.lstAvailProjects.Items.RemoveAt(i);
						}
						else
						{
							i++;
						}
					}
				}
			}
			catch (Exception ex)
			{
				Connect.logEventMessage("wpfAssignProject::removeDuplicates", ex, System.Diagnostics.EventLogEntryType.Error);
			}
		}

		/// <summary>Converts a string into a stream for the RTFTextBox</summary>
		private void setRTFCaption()
		{
			try
			{
				string rtfText = "";
				if (string.IsNullOrEmpty(this._solName))
				{
					rtfText = this._resources.GetString("flowSelectNoSolution");
				}
				else
				{
					rtfText = this._resources.GetString("flowSelectSolution");
					rtfText = rtfText.Replace("%solution%", this._solName);
				}

				this.headerCaption.Document = (FlowDocument)XamlReader.Load(new XmlTextReader(new StringReader(rtfText)));
			}
			catch (Exception ex)
			{
				Connect.logEventMessage("wpfAssignProject::setRTFCaption", ex, System.Diagnostics.EventLogEntryType.Error);
			}
		}
	}
}
