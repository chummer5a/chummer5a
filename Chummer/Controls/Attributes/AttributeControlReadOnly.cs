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
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Chummer.Annotations;
using Chummer.Backend.Attributes;

namespace Chummer.Controls.Attributes
{
    public partial class AttributeControlReadOnly : UserControl
    {
        private readonly CharacterAttrib _objAttribute;
        private readonly Character _objCharacter;
        private readonly BindingSource _objDataSource;

        public AttributeControlReadOnly(CharacterAttrib attribute)
        {
            if (attribute == null)
                return;
            _objAttribute = attribute;
            _objCharacter = attribute.CharacterObject;
            _objDataSource = _objCharacter.AttributeSection.GetAttributeBindingByName(AttributeName);

            InitializeComponent();
            this.UpdateLightDarkMode();
            this.TranslateWinForm();

            SuspendLayout();
            _objCharacter.AttributeSection.PropertyChanged += AttributePropertyChanged;
            
            lblName.DoOneWayDataBinding("Text", _objDataSource, nameof(CharacterAttrib.DisplayNameFormatted));
            lblValue.DoOneWayDataBinding("Text", _objDataSource, nameof(CharacterAttrib.DisplayValue));
            lblLimits.DoOneWayDataBinding("Text", _objDataSource, nameof(CharacterAttrib.AugmentedMetatypeLimits));
            lblValue.DoOneWayDataBinding("ToolTipText", _objDataSource, nameof(CharacterAttrib.ToolTip));

            ResumeLayout();
        }

        private void UnbindAttributeControl()
        {
            _objCharacter.AttributeSection.PropertyChanged -= AttributePropertyChanged;

            foreach (Control objControl in Controls)
            {
                objControl.DataBindings.Clear();
            }
        }

        private void AttributePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(AttributeSection.AttributeCategory))
                return;
            _objDataSource.DataSource = _objCharacter.AttributeSection.GetAttributeByName(AttributeName);
            _objDataSource.ResetBindings(false);
        }

        public void UpdateWidths(int intNameWidth, int intValueWidth, int intLimitsWidth)
        {
            tlpMain.SuspendLayout();

            if (intNameWidth >= 0)
            {
                if (lblName.MinimumSize.Width > intNameWidth)
                    lblName.MinimumSize = new Size(intNameWidth, lblName.MinimumSize.Height);
                if (lblName.MaximumSize.Width != intNameWidth)
                    lblName.MaximumSize = new Size(intNameWidth, lblName.MinimumSize.Height);
                if (lblName.MinimumSize.Width < intNameWidth)
                    lblName.MinimumSize = new Size(intNameWidth, lblName.MinimumSize.Height);
            }
            
            if (intValueWidth >= 0)
            {
                if (lblValue.MinimumSize.Width > intValueWidth)
                    lblValue.MinimumSize = new Size(intValueWidth, lblValue.MinimumSize.Height);
                if (lblValue.MaximumSize.Width != intValueWidth)
                    lblValue.MaximumSize = new Size(intValueWidth, lblValue.MinimumSize.Height);
                if (lblValue.MinimumSize.Width < intValueWidth)
                    lblValue.MinimumSize = new Size(intValueWidth, lblValue.MinimumSize.Height);
            }

            if (intLimitsWidth >= 0)
            {
                if (lblLimits.MinimumSize.Width > intLimitsWidth)
                    lblLimits.MinimumSize = new Size(intLimitsWidth, lblLimits.MinimumSize.Height);
                if (lblLimits.MaximumSize.Width != intLimitsWidth)
                    lblLimits.MaximumSize = new Size(intLimitsWidth, lblLimits.MinimumSize.Height);
                if (lblLimits.MinimumSize.Width < intLimitsWidth)
                    lblLimits.MinimumSize = new Size(intLimitsWidth, lblLimits.MinimumSize.Height);
            }

            tlpMain.ResumeLayout();
        }

        public string AttributeName => _objAttribute.Abbrev;

        [UsedImplicitly]
        public int NameWidth => lblName.PreferredWidth;
    }
}
