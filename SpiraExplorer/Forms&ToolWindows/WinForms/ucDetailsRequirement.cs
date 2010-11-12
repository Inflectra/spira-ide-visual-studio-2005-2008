using System.Windows.Forms;

namespace Inflectra.SpiraTest.IDEIntegration.VisualStudio.WinForms.Forms
{
	public partial class ucDetailsRequirement : UserControl
	{
		public ucDetailsRequirement()
		{
			InitializeComponent();
		}

		public string getItemCode()
		{
			return wpfDetailsRequirement1.getItemCode;
		}
	}
}
