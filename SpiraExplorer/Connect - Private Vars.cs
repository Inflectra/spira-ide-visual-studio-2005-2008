using System;
using System.Windows;
using System.Windows.Data;
using EnvDTE;
using EnvDTE80;
using Extensibility;
using System.IO;
using System.Diagnostics;

namespace Inflectra.SpiraTest.IDEIntegration.VisualStudio
{
	/// <summary>The object for implementing an Add-in.</summary>
	/// <seealso class='IDTExtensibility2' />
	public partial class Connect : IDTExtensibility2, IDTCommandTarget
	{
		// Environment and Application.
		private DTE2 _applicationObject;
		private SolutionEvents solEvents;
		private AddIn _addInInstance;

		//Settings file.
		private SettingsClass.XMLFileReader _Settings;

#if DEBUG
		private bool _debug_build = true;
#else
		private bool _debug_build = false;
#endif

		private void logMessage(string Message, EventLogEntryType Severity)
		{
			try
			{
				if ((Severity == EventLogEntryType.Error) || (this._debug) || (this._debug_build))
				{
					Connect.logEventMessage(Message, Severity);
				}
			}
			catch
			{
			}
		}
	}

	[ValueConversion(typeof(string), typeof(Thickness))]
	internal class PaddingConverter : IValueConverter
	{
		#region IValueConverter Members

		object IValueConverter.Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if (((string)parameter) != "IndentLevel")
			{
				Thickness ret = new Thickness();
				ret.Bottom = 0;
				ret.Right = 0;
				ret.Top = 0;
				ret.Left = (((string)parameter).Length / 3) * 5;

				return ret;
			}
			else return new Thickness(0, 0, 0, 0);
		}

		object IValueConverter.ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			return null;
		}

		#endregion
	}
}