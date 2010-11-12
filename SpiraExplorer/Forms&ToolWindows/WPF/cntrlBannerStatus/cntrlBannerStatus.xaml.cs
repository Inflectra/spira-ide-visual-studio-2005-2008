using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;

namespace Inflectra.SpiraTest.IDEIntegration.VisualStudio.Forms_ToolWindows.WPF
{
	/// <summary>
	/// Interaction logic for cntrlBannerStatus.xaml
	/// </summary>
	public partial class cntrlBannerStatus : UserControl
	{

		private Queue<Message> _msgsError;
		private Queue<Message> _msgsWarning;
		private Queue<Message> _msgsInfo;

		//Thread for timing.
		private Thread _threadTiming;

	
		public cntrlBannerStatus()
		{
			InitializeComponent();

			this._msgsError = new Queue<Message>();
			this._msgsWarning = new Queue<Message>();
			this._msgsInfo = new Queue<Message>();

			//Create our thread.
			TimerThread timer = new TimerThread();
			timer.TimeUp += new EventHandler(timer_TimeUp);
			this._threadTiming = new Thread(new ThreadStart(timer.StartTimer));
			this._threadTiming.Name = "Background Display Timer Thread";
			this._threadTiming.Priority = ThreadPriority.Lowest;
			this._threadTiming.Start();
		}

		void timer_TimeUp(object sender, EventArgs e)
		{
			//At this point, we check to see if ther are any other messages to display.
			if (this._msgsError.Count != 0)
			{
				//Have an error message.
			}
			else
			{
				//No error.
				if (this._msgsWarning.Count != 0)
				{
					//Have a warning message.
				}
				else
				{
					//No Error & Warning.
					if (this._msgsInfo.Count != 0)
					{
						//Have an Info message.
					}
					else
					{
						//No messages in queue.
						this.panelError.Visibility = System.Windows.Visibility.Collapsed;
						this.panelWarning.Visibility = System.Windows.Visibility.Collapsed;
						this.panelInfo.Visibility = System.Windows.Visibility.Collapsed;
						this.panelNone.Visibility = System.Windows.Visibility.Collapsed;
					}
				}
			}
		}

		private class Message
		{
			public string Message;
			public MessageTypeEnum Type;

			public enum MessageTypeEnum
			{
				Info = 0,
				Warning = 1,
				Error = 2
			}
		}

		private class TimerThread
		{
			//Cancel flag.
			static bool _Abort = false;

			//Timer event.
			public event EventHandler TimeUp;

			public void StartTimer()
			{
				while (!_Abort)
				{
					Thread.Sleep(5000);

					if (this.TimeUp != null)
						this.TimeUp(this, new EventArgs());
				}
			}
		}
	}
}
