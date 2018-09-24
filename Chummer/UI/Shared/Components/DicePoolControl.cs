using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Chummer.UI.Shared.Components
{
    public partial class DicePoolControl : UserControl
    {
        public Action<Character, int> DiceRollerOpenedInt { get; set; }

        private Character _characterObject;
        private IHasDicePool _poolObject;

        public DicePoolControl()
        {
            InitializeComponent();
        }

        private void DicePoolControl_Load(object sender, EventArgs e)
        {
            if (_characterObject != null) return;
            if (ParentForm != null)
                ParentForm.Cursor = Cursors.WaitCursor;

            if (ParentForm is CharacterShared frmParent)
                _characterObject = frmParent.CharacterObject;
            else
            {
                Utils.BreakIfDebug();
                _characterObject = new Character();
            }
            cmdRoll.SetToolTip(LanguageManager.GetString("Tip_DiceRoller", GlobalOptions.Language));
            Utils.DoDatabinding(cmdRoll, "Visible", _characterObject.Options, nameof(CharacterOptions.AllowSkillDiceRolling));

            if (ParentForm != null)
                ParentForm.Cursor = Cursors.Default;
        }

        private void cmdRoll_Click(object sender, EventArgs e)
        {
            DiceRollerOpenedInt(_characterObject, PoolObject.DicePool);
        }

        public IHasDicePool PoolObject
        {
            get => _poolObject;
            set
            {
                _poolObject = value;
                if (value == null) return;
                Utils.DoDatabinding(lblDicePool, "Text", PoolObject, nameof(PoolObject.DicePool));
                Utils.DoDatabinding(lblDicePool, "ToolTipText", PoolObject, nameof(PoolObject.DicePoolTooltip));
            }
        }
    }
}
