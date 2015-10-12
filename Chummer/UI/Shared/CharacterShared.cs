using System.Windows.Forms;

namespace Chummer
{
	/// <summary>
	/// Contains functionality shared between frmCreate and frmCareer
	/// </summary>
	[System.ComponentModel.DesignerCategory("")]
	public class CharacterShared : Form
	{
		protected Character _objCharacter;
		protected MainController _objController;
		protected CharacterOptions _objOptions;
		protected CommonFunctions _objFunctions;
	}
}
