using System;
using EnvDTE;
using EnvDTE80;
using Extensibility;
using Inflectra.SpiraTest.IDEIntegration.VisualStudio.Resources;
using System.Diagnostics;
using System.Resources;
using System.Reflection;

namespace Inflectra.SpiraTest.IDEIntegration.VisualStudio
{
	/// <summary>The object for implementing an Add-in.</summary>
	/// <seealso class='IDTExtensibility2' />
	public partial class Connect : IDTExtensibility2, IDTCommandTarget
	{

		public static ResourceManager getCultureResource
		{
			get
			{
				Assembly addinAssembly = Assembly.GetExecutingAssembly();
				Assembly satelliteAssembly = addinAssembly.GetSatelliteAssembly(System.Globalization.CultureInfo.GetCultureInfo("en-US"));
				string resAssName = "";
				foreach (string resName in satelliteAssembly.GetManifestResourceNames())
				{
					if (resName == "SpiraExplorer.resources.Properties.Resources.resources")
						resAssName = resName.Substring(0, resName.LastIndexOf("."));
				}

				try
				{
					return new ResourceManager(resAssName, satelliteAssembly);
				}
				catch
				{
					return null;
				}
			}
		}

		public static void logEventMessage(string Message, Exception ex, EventLogEntryType Severity)
		{
			string msg = Message;
			if (ex != null)
			{
				msg += Environment.NewLine + Environment.NewLine;
				msg += "Error Messages:" + Environment.NewLine;
				msg += ex.Message;
				string stack = ex.StackTrace;
				while (ex.InnerException != null)
				{
					msg += Environment.NewLine + ex.InnerException.Message;
					stack += Environment.NewLine + "-----" + Environment.NewLine + ex.InnerException.StackTrace;
					//Advance to next exception.
					ex = ex.InnerException;
				}
				msg += Environment.NewLine + Environment.NewLine;
				msg += "Stack Trace:" + Environment.NewLine + stack;
			}

			Connect.logEventMessage(msg, Severity);

		}

		public static void logEventMessage(string Message, EventLogEntryType Severity)
		{
			try
			{
				//Get resources


				//Create event source if needed.
				if (!EventLog.SourceExists(Connect.getCultureResource.GetString("strAddinProgName")))
					EventLog.CreateEventSource(Connect.getCultureResource.GetString("strAddinProgName"), "Application");

				Message = Connect.getCultureResource.GetString("strAddinProgName") + ": " + Message;

				EventLog.WriteEntry(Connect.getCultureResource.GetString("strAddinProgName"), Message, Severity);
			}
			catch
			{
			}
		}

	}
}