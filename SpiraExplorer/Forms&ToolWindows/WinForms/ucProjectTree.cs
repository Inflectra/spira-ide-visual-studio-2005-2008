using System.Windows.Forms;
using Inflectra.SpiraTest.IDEIntegration.VisualStudio.WPF.Forms;

namespace Inflectra.SpiraTest.IDEIntegration.VisualStudio.WinForms.Forms
{
	public partial class ucProjectTree : UserControl
	{
		public ucProjectTree()
		{
			InitializeComponent();

			//Manually call the 'no Solution Loaded' fundtion.
			this.setSolution(null);
		}

		internal void setSettingsFile(Connect.SettingsClass.XMLFileReader settingsFile)
		{
			//Flow down.
			( (wpfProjectTree)this.elementHost1.Child ).setSettingsFile(settingsFile);
		}

		internal void setSolution(string SolutionName)
		{
			//Flow down.
			( (wpfProjectTree)this.elementHost1.Child ).setSolution(SolutionName);
		}
	}
}
