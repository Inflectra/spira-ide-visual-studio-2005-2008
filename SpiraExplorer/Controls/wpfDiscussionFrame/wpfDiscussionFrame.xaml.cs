using System.Windows;
using System.Windows.Controls;

namespace Inflectra.SpiraTest.IDEIntegration.VisualStudio.WPF.Controls
{
	/// <summary>
	/// Interaction logic for wpfDiscussionFrame.xaml
	/// </summary>
	public partial class wpfDiscussionFrame : UserControl
	{
		public wpfDiscussionFrame(string HeaderLine, string MessageBody)
		{
			InitializeComponent();

			this.txtHeader.Text = HeaderLine;
			this.txtMessage.BorderThickness = new Thickness(0);

			if (string.IsNullOrEmpty(MessageBody))
			{
				this.txtMessage.Visibility = Visibility.Collapsed;
			}
			else
			{
				this.txtMessage.HTMLText = MessageBody;
			}

		}
	}
}
