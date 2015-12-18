using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using Chummer.Skills;
using Chummer.UI.Shared;

namespace Chummer.UI
{
	public partial class frmSkillTest : Form
	{
		Character objCharacter;

		public frmSkillTest(Character character)
		{
			objCharacter = character;
			InitializeComponent();
		}

		private void frmSkillTest_Load(object sender, EventArgs e)
		{
			//objCharacter = new Character();
			//objCharacter.SettingsFile = Path.GetFileName(Directory.GetFiles(Path.Combine(Environment.CurrentDirectory, "settings"), "*.xml")[0]);
			//objCharacter.BuildMethod = CharacterBuildMethod.Priority;
			
			//frmPriorityMetatype frmSelectMetatype = new frmPriorityMetatype(objCharacter);
			//frmSelectMetatype.ShowDialog();

			//if (frmSelectMetatype.DialogResult == DialogResult.Cancel)
			//{ throw new Exception("Don't do that shit"); }

			//// Add the Unarmed Attack Weapon to the character.
			//XmlDocument objXmlDocument = XmlManager.Instance.Load("weapons.xml");
			//XmlNode objXmlWeapon = objXmlDocument.SelectSingleNode("/chummer/weapons/weapon[name = \"Unarmed Attack\"]");
			//TreeNode objDummy = new TreeNode();
			//Weapon objWeapon = new Weapon(objCharacter);
			//objWeapon.Create(objXmlWeapon, objCharacter, objDummy, null, null, null);
			//objCharacter.Weapons.Add(objWeapon);

			skillsTabUserControl1.ObjCharacter = objCharacter;

		}

		private void DisplayOnResize(object sender, EventArgs eventArgs)
		{
			
		}

		private void button1_Click(object sender, EventArgs e)
		{
			//XmlDocument doc = XmlManager.Instance.Load("skills.xml");
			//XmlNode n = doc.SelectSingleNode("/chummer/skills/skill");
   //         objCharacter.Skills.Add(Skill.FromData(n, objCharacter));

			objCharacter.Skills.RemoveAt(new Random().Next(objCharacter.Skills.Count));
		}

		private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
		{


			//display.Visible = false; //If not invisible it recalculates the layout each time
			//						//Speedup is somewhere between 1-2 orders magnitude
			//if (comboBox1.SelectedIndex == 0)
			//{
			//	display.Filter(x => true);
			//}
			//else
			//{
			//	display.Filter(x => (x.Rating > 0));
			//}
			//display.Visible = true;
		}
	}
}
