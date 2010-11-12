using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Inflectra.SpiraTest.IDEIntegration.VisualStudio.WPF.Controls;
using System.Windows.Media.Imaging;
 
namespace Inflectra.SpiraTest.IDEIntegration.VisualStudio.WPF.Forms
{
	/// <summary>
	/// Interaction logic for wpfDetailsTask.xaml
	/// </summary>
	public partial class wpfDetailsTask : UserControl
	{
		/// <summary>Determines whether or not changes have been made.</summary>
		private bool _isFieldChanged;
		/// <summary>Determines whether or not the HTML Description has changed.</summary>
		private bool _isDescChanged;
		/// <summary>Specifies whether or not we're loading data.</summary>
		private bool _isInLoadMode;
		/// <summary>Hold the handle to our window.</summary>
		private EnvDTE80.Window2 _DetailsWindow = null;
		/// <summary>The string containing out Tab Text</summary>
		private string _DetailsWindowTitle = null;
		/// <summary>Stores the pre-defined set of Priorities.</summary>
		private Dictionary<int, string> _TaskPriorities;
		/// <summary>Stores the pre-defined set of Statuses.</summary>
		private Dictionary<int, string> _TaskSastuses;


		public wpfDetailsTask()
		{
			try
			{
				InitializeComponent();

				//Get the resources.
				this._resources = Connect.getCultureResource;

				this._TaskPriorities = new Dictionary<int, string>();
				this._TaskSastuses = new Dictionary<int, string>();

				this._TaskPriorities.Add(1, "Critical");
				this._TaskPriorities.Add(2, "High");
				this._TaskPriorities.Add(3, "Medium");
				this._TaskPriorities.Add(4, "Low");
				this._TaskSastuses.Add(1, "Not Started");
				this._TaskSastuses.Add(2, "In Progress");
				this._TaskSastuses.Add(3, "Completed");
				this._TaskSastuses.Add(4, "Blocked");
				this._TaskSastuses.Add(5, "Deferred");

				//Load images.
				this._bnrImgInfo.Source = this.getImage("imgInfoWPF", new Size()).Source;
				this._bnrImgError.Source = this.getImage("imgErrorWPF", new Size()).Source;
				this._barImgInfo.Source = this.getImage("imgInfoWPF", new Size()).Source;
				this._barImgWarning.Source = this.getImage("imgWarningWPF", new Size()).Source;
				this._barImgError.Source = this.getImage("imgErrorWPF", new Size()).Source;
				this._barImgLogo.Source = this.getImage("imgLogoWPF", new Size()).Source;
			}
			catch (Exception ex)
			{
				Connect.logEventMessage("wpfDetailsTask::.ctor", ex, System.Diagnostics.EventLogEntryType.Error);
			}
		}

		private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
		{
			e.Handled = true;
			//They selected to reload.
			this.LoadTask(this._Project, this._ItemTag);
		}

		/// <summary>Generates a pretty Error message string.</summary>
		/// <param name="e">Exception.</param>
		/// <returns>String of the error messages.</returns>
		private string getErrorMessage(Exception e)
		{
			try
			{
				Exception ex = e;
				string errMsg = "» " + ex.Message;
				while (ex.InnerException != null)
				{
					errMsg += Environment.NewLine + "» " + ex.InnerException.Message;
					ex = ex.InnerException;
				}

				return errMsg;
			}
			catch (Exception ex)
			{
				Connect.logEventMessage("wpfDetailsTask::getErrorMessage", ex, System.Diagnostics.EventLogEntryType.Error);
			}
			return "";
		}

		/// <summary>Hit when a numeric field loses focus. Verifies the item entered is a number.</summary>
		/// <param name="sender">cntrlEstEffortH, cntrlEstEffortM, cntrlActEffortH, cntrlActEffortM, cntrlPerComplete</param>
		/// <param name="e">Event Args</param>
		private void _cntrl_LostFocus(object sender, RoutedEventArgs e)
		{
			e.Handled = true;

			try
			{
				TextBox control = (TextBox)sender;

				int tryNum;
				if (int.TryParse(control.Text, out tryNum))
				{
					if (tryNum < 0)
						control.Text = "0";
					if (control == this.cntrlPerComplete && tryNum > 100)
						control.Text = "100";
					this._isFieldChanged = true;
					if (!this._DetailsWindow.Caption.Contains("*"))
					{
						this._DetailsWindow.Caption = this._DetailsWindowTitle + " *";
					}
				}
				else
				{
					control.Text = "";
				}
			}
			catch (Exception ex)
			{
				Connect.logEventMessage("wpfDetailsTask::_cntrl_LostFocus", ex, System.Diagnostics.EventLogEntryType.Error);
			}
		}

		/// <summary>Hit when the user tries to enter data into a numeric field. Verifies it's numeric only.</summary>
		/// <param name="sender">cntrlEstEffortH, cntrlEstEffortM, cntrlActEffortH, cntrlActEffortM, cntrlPerComplete</param>
		/// <param name="e">Event Args</param>
		private void _cntrl_PreviewTextInput(object sender, TextCompositionEventArgs e)
		{
			e.Handled = true;
			try
			{
				int tryParse;
				int.TryParse(e.Text, out tryParse);
			}
			catch (Exception ex)
			{
				Connect.logEventMessage("wpfDetailsTask::_cntrl_PreviewTextInput", ex, System.Diagnostics.EventLogEntryType.Error);
			}
		}

		/// <summary>Hit when the user clicks a message bar.</summary>
		/// <param name="sender">messageWarning / messageError / messageInfo</param>
		/// <param name="e">MouseButton Event Args</param>
		private void messageWarning_MouseDown(object sender, MouseButtonEventArgs e)
		{
			e.Handled = true;
			try
			{
				this.panelNone.Visibility = Visibility.Visible;
				this.panelError.Visibility = Visibility.Collapsed;
				this.panelWarning.Visibility = Visibility.Collapsed;
				this.panelInfo.Visibility = Visibility.Collapsed;
			}
			catch (Exception ex)
			{
				Connect.logEventMessage("wpfDetailsTask::messageWarning_MouseDown", ex, System.Diagnostics.EventLogEntryType.Error);
			}
		}

		/// <summary>Hit when the user wants to save the Task.</summary>
		/// <param name="sender">SaveButton</param>
		/// <param name="e">EventArgs</param>
		private void _cntrlSave_Click(object sender, RoutedEventArgs e)
		{
			e.Handled = true;

			this.btnSave.IsEnabled = false;

			this.UpdateItem();
		}

		/// <summary>Hit when a dropdown list changes.</summary>
		/// <param name="sender">cntrlOwnedBy, cntrlPriority, cntrlRequirement, cntrlRequirement, cntrlnStatus</param>
		/// <param name="e"></param>
		private void cntrl_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			e.Handled = true;
			try
			{
				if (!this._isInLoadMode)
				{
					if (!this._DetailsWindow.Caption.Contains("*"))
					{
						this._DetailsWindow.Caption = this._DetailsWindowTitle + " *";
					}
					this._isFieldChanged = true;
					this.btnSave.IsEnabled = true;
				}
			}
			catch (Exception ex)
			{
				Connect.logEventMessage("wpfDetailsTask::cntrl_SelectionChanged", ex, System.Diagnostics.EventLogEntryType.Error);
			}
		}

		/// <summary>Hit when a textbox or dropdown list changes.</summary>
		/// <param name="sender">cntrlName, cntrlDescription, cntrlPerComplete, cntrlEstEffortH, cntrlEstEffortM, cntrlActEffortH, cntrlActEffortM</param>
		/// <param name="e"></param>
		private void _cntrl_TextChanged(object sender, EventArgs e)
		{
			if (e.GetType() == typeof(TextChangedEventArgs))
				((TextChangedEventArgs)e).Handled = true;

			try
			{
				if (!this._isInLoadMode)
				{
					if (!this._DetailsWindow.Caption.Contains("*"))
					{
						this._DetailsWindow.Caption = this._DetailsWindowTitle + " *";
					}

					this._isFieldChanged = true;
					this.btnSave.IsEnabled = true;

					if (sender.GetType() == typeof(wpfRichHTMLText))
					{
						if (((wpfRichHTMLText)sender).Name == "cntrlDescription")
						{
							this._isDescChanged = true;
						}
					}
				}
			}
			catch (Exception ex)
			{
				Connect.logEventMessage("wpfDetailsTask::_cntrl_TextChanged", ex, System.Diagnostics.EventLogEntryType.Error);
			}
		}

		/// <summary>Set the window handle for this form.</summary>
		internal EnvDTE80.Window2 setDetailWindow
		{
			set
			{
				this._DetailsWindow = value;
				this._DetailsWindowTitle = this._DetailsWindow.Caption;
			}
		}

		/// <summary>The tag of the item this form displays.</summary>
		public string itemTag
		{
			get
			{
				return this._ItemTag;
			}
		}

		/// <summary>Validates the dates are in order.</summary>
		/// <returns>True if dates are good; false otherwise.</returns>
		private bool validate_Dates()
		{
			try
			{
				if ((this.cntrlEndDate.SelectedDate.HasValue) && (this.cntrlStartDate.SelectedDate.HasValue))
				{
					if (!(this.cntrlEndDate.SelectedDate >= this.cntrlStartDate.SelectedDate))
					{
						return false;
					}
				}
				return true;
			}
			catch (Exception ex)
			{
				Connect.logEventMessage("wpfDetailsTask::validate_Dates", ex, System.Diagnostics.EventLogEntryType.Error);
			}
			return false;
		}


		/// <summary>Hit when the selected date changes.</summary>
		/// <param name="sender">cntrlStartDate, cntrlEndDate</param>
		/// <param name="e">SelectionChangedEventArgs</param>
		private void cntrlDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
		{
			e.Handled = true;
			try
			{
				if (!this._isInLoadMode)
				{
					this._isFieldChanged = true;
					this.btnSave.IsEnabled = true;
					if (!this._DetailsWindow.Caption.Contains("*"))
					{
						this._DetailsWindow.Caption = this._DetailsWindowTitle + " *";
					}

					//Does some simple verification on the dates.
					if (sender == null)
					{
					}
					else
					{
						if (!this.validate_Dates())
						{
							this.panelNone.Visibility = Visibility.Collapsed;
							this.panelError.Visibility = Visibility.Visible;
							this.msgErrMessage.Text = "The start date must be before the end date.";
						}
					}
				}
			}
			catch (Exception ex)
			{
				Connect.logEventMessage("wpfDetailsTask::cntrlDate_SelectedDateChanged", ex, System.Diagnostics.EventLogEntryType.Error);
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
				Connect.logEventMessage("wpfDetailsTask::getBMSource", ex, System.Diagnostics.EventLogEntryType.Error);
			}
			return null;
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
				//if (fill != null)
				//{
				//    retImage.Stretch = fill;
				//}
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
				Connect.logEventMessage("wpfDetailsTask::getImage", ex, System.Diagnostics.EventLogEntryType.Error);
				return new Image();
			}
		}
	}
}
