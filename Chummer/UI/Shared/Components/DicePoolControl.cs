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
        public event Action<Character, int> DiceRollerOpenedInt;

        private Character _characterObject;
        private int _dicePool = 0;
        private string _dicePoolTooltip = string.Empty;

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

            lblDicePool.DoDatabinding("Text", this, nameof(this._dicePool));
            lblDicePool.DoDatabinding("ToolTipText", this, nameof(this._dicePoolTooltip));

            cmdRoll.SetToolTip(LanguageManager.GetString("Tip_DiceRoller", GlobalOptions.Language));
            cmdRoll.Visible = _characterObject.Options.AllowSkillDiceRolling;

            if (ParentForm != null)
                ParentForm.Cursor = Cursors.Default;
        }

        private void cmdRoll_Click(object sender, EventArgs e)
        {
            DiceRollerOpenedInt?.Invoke(_characterObject, _dicePool);
        }

        public IHasDicePool PoolObject
        {
            set
            {
                if (value == null) return;
                _dicePool = value.DicePool;
                _dicePoolTooltip = value.DicePoolTooltip;
            }
        }
    }
}
