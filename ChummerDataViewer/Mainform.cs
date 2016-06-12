using System;
using System.Windows.Forms;
using ChummerDataViewer.Model;

namespace ChummerDataViewer
{
	public partial class Mainform : Form
	{
		private DynamoDbLoader loader;
		public Mainform()
		{
			InitializeComponent();
		}

		private void Mainform_Load(object sender, EventArgs e)
		{
			if (!PersistentState.Setup)
			{
				SetupForm setupForm = new SetupForm();
				DialogResult result = setupForm.ShowDialog();

				if (result != DialogResult.OK)
				{
					Application.Exit();
				}

				PersistentState.Initialize(setupForm.Id, setupForm.Key);

			}

			loader = new DynamoDbLoader();
		}
	}
}
