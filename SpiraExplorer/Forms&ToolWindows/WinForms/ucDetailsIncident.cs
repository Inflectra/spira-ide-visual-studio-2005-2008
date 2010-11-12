using System.Windows.Forms;

namespace Inflectra.SpiraTest.IDEIntegration.VisualStudio.WinForms.Forms
{
	public partial class ucDetailsIncident : UserControl
	{
		public ucDetailsIncident()
		{
			InitializeComponent();
		}

		public string getItemCode()
		{
			return wpfDetailsIncident1.getItemCode();
		}
	}
}
