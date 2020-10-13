using Chummer;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NeonJungleLC
{
    public class NeonJungleWork
    {
        public IEnumerable<ToolStripMenuItem> GetMenuItems(ToolStripMenuItem input)
        {

            ToolStripMenuItem mnuNJLCCheck = new ToolStripMenuItem
            {
                Name = "mnuNJLCCheck",
                Text = "&Check Flags",
                Image = NeonJungleLC.Properties.Resources.NeutralFlags,
                ImageTransparentColor = Color.Black,
                Size = new Size(148, 22),
                Tag = "Menu_Tools_NeonJungleLCCheckFlags"
            };
            mnuNJLCCheck.Click += mnuNJLCCheck_Click;
            mnuNJLCCheck.UpdateLightDarkMode();
            mnuNJLCCheck.TranslateToolStripItemsRecursively();
            yield return mnuNJLCCheck;

            
        }

        private void mnuNJLCCheck_Click(object sender, EventArgs e)
        {
            var gsh = new GoogleSheetsHandler();
            var active = Program.MainForm.ActiveMdiChild;
            
            Character activeChar = null;
            if (active is frmCareer career)
            {
                activeChar = career.CharacterObject;
            }
            else if (active is frmCreate create)
            {
                activeChar = create.CharacterObject;
            }
            else
            {
                Program.MainForm.ShowMessageBox("Please open a character to calculate it.");
                return;
            }
            var lockedFlags = gsh.GetLockedFlags(activeChar);


        }
    }
}
