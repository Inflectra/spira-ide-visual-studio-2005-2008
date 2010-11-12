using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Input;
using System.Resources;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Inflectra.SpiraTest.IDEIntegration.VisualStudio.WPF.Forms 
{
	/// <summary>
	/// Interaction logic for wpfDetailsTask.xaml
	/// </summary>
	public partial class wpfDetailsRequirement : UserControl
	{
		/// <summary>Hold the handle to our window.</summary>
		private EnvDTE80.Window2 _DetailsWindow = null;
		/// <summary>The string containing out Tab Text</summary>
		private string _DetailsWindowTitle = null;
		/// <summary>Stores the pre-defined set of Priorities.</summary>
		private Dictionary<int, string> _ReqPriorities;
		/// <summary>Stores the pre-defined set of Statuses.</summary>
		private Dictionary<int, string> _ReqSastuses;

		private ResourceManager _resources = null;

		public wpfDetailsRequirement()
		{
			try
			{
				InitializeComponent();

				//Get resources.
				this._resources = Connect.getCultureResource;

				this._ReqPriorities = new Dictionary<int, string>();
				this._ReqSastuses = new Dictionary<int, string>();

				this._ReqPriorities.Add(1, "Critical");
				this._ReqPriorities.Add(2, "High");
				this._ReqPriorities.Add(3, "Medium");
				this._ReqPriorities.Add(4, "Low");
				this._ReqSastuses.Add(1, "Requested");
				this._ReqSastuses.Add(2, "Planned");
				this._ReqSastuses.Add(3, "In Progress");
				this._ReqSastuses.Add(4, "Completed");
				this._ReqSastuses.Add(5, "Accepted");
				this._ReqSastuses.Add(6, "Rejected");
				this._ReqSastuses.Add(7, "Evaluated");

				//Load images.
				this._bnrImgInfo.Source = this.getImage("imgInfoWPF", new Size()).Source;
				this._bnrImgError.Source = this.getImage("imgErrorWPF", new Size()).Source;
				this._barImgLogo.Source = this.getImage("imgLogoWPF", new Size()).Source;
			}
			catch (Exception ex)
			{
				Connect.logEventMessage("wpfDetailsRequirement::.ctor", ex, System.Diagnostics.EventLogEntryType.Error);
			}
		}

		private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
		{
			//They selected to reload.
			this.LoadRequirement(this._Project, this._ItemTag);
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
				Connect.logEventMessage("wpfDetailsRequirement::getErrorMessage", ex, System.Diagnostics.EventLogEntryType.Error);
			}
			return "";
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

		/// <summary>The active item for this window.</summary>
		public string getItemCode
		{
			get
			{
				return this._ItemTag;
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
			}
			catch (Exception ex)
			{
				Connect.logEventMessage("wpfDetailsRequirement::getBMSource", ex, System.Diagnostics.EventLogEntryType.Error);
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
				Connect.logEventMessage("wpfDetailsRequirement::getImage", ex, System.Diagnostics.EventLogEntryType.Error);
			}
			return new Image();
		}


	}
}
