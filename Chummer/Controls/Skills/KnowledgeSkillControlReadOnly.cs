/*  This file is part of Chummer5a.
 *
 *  Chummer5a is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  Chummer5a is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with Chummer5a.  If not, see <http://www.gnu.org/licenses/>.
 *
 *  You can obtain the full source code for Chummer5a at
 *  https://github.com/chummer5a/chummer5a
 */

using System;
using System.Drawing;
using System.Windows.Forms;
using Chummer.Backend.Skills;

namespace Chummer.Controls.Skills
{
    public partial class KnowledgeSkillControlReadOnly : UserControl
    {
        private readonly KnowledgeSkill _objSkill;

        public KnowledgeSkillControlReadOnly(KnowledgeSkill objSkill)
        {
            if (objSkill == null)
                return;
            _objSkill = objSkill;
            InitializeComponent();
            AdjustForDpi();
            this.UpdateLightDarkMode();
            this.TranslateWinForm();

            lblRating.DoOneWayDataBinding("Text", objSkill, nameof(KnowledgeSkill.Rating));
            lblModifiedRating.DoOneWayDataBinding("Text", objSkill, nameof(KnowledgeSkill.DisplayPool));
            lblType.DoOneWayDataBinding("Text", objSkill, nameof(KnowledgeSkill.CurrentDisplayCategory));
            lblName.DoOneWayDataBinding("Text", objSkill, nameof(KnowledgeSkill.WritableName));
            lblSpec.DoOneWayDataBinding("Text", objSkill, nameof(KnowledgeSkill.CurrentDisplaySpecialization));
        }

        private void KnowledgeSkillControlReadOnly_Load(object sender, EventArgs e)
        {
            lblRating.DoOneWayNegatableDataBinding("Visible", _objSkill, nameof(KnowledgeSkill.IsNativeLanguage));
            lblModifiedRating.DoOneWayDataBinding("ToolTipText", _objSkill, nameof(KnowledgeSkill.PoolToolTip));
            lblName.DoOneWayDataBinding("ForeColor", _objSkill, nameof(KnowledgeSkill.PreferredColor));
        }

        public void UnbindKnowledgeSkillControl()
        {
            foreach (Control objControl in Controls)
            {
                objControl.DataBindings.Clear();
            }
        }

        private void KnowledgeSkillControlReadOnly_DpiChangedAfterParent(object sender, EventArgs e)
        {
            AdjustForDpi();
        }

        private void AdjustForDpi()
        {
            using (Graphics g = CreateGraphics())
            {
                if (lblRating != null)
                    lblRating.MinimumSize = new Size((int)(25 * g.DpiX / 96.0f), 0);
                lblModifiedRating.MinimumSize = new Size((int)(50 * g.DpiX / 96.0f), 0);
            }
        }

        /// <summary>
        /// Update the position of controls.
        /// </summary>
        /// <param name="intNameWidth">Width of the Name label</param>
        public void MoveControls(int intNameWidth)
        {
            lblName.MinimumSize = new Size(intNameWidth, lblName.MinimumSize.Height);
        }

        public int NameWidth => lblName.PreferredWidth;
    }
}
