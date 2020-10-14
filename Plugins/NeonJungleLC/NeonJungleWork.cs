using Chummer;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NeonJungleLC.GoogleSheets;
using NLog;

namespace NeonJungleLC
{
    public class NeonJungleWork
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

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
            else if (active is frmCharacterRoster roster)
            {
                if (roster.treCharacterList.SelectedNode != null)
                {
                    var tagobj = roster.treCharacterList.SelectedNode.Tag;
                    if (tagobj is CharacterCache charCache)
                    {
                        activeChar = Program.MainForm.LoadCharacter(charCache.FilePath, "", false, false).Result;
                    }

                }
            }
            if (activeChar == null)
            {
                Program.MainForm.ShowMessageBox("Please open a character to calculate it.");
                return;
            }
            var flagcalc = new NeonJungleFlagsCalculator(activeChar);
            var useFlags = flagcalc.GetUsedFlags();
            var gsh = new GoogleSheetsHandler();
            var lockedFlags = gsh.GetLockedFlags(activeChar);


        }
    }
}
