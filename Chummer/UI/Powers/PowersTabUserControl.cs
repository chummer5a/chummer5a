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
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using Chummer.Backend.Equipment;
using Chummer.Properties;
using Chummer.UI.Table;

// ReSharper disable StringCompareToIsCultureSpecific

namespace Chummer.UI.Powers
{
    public partial class PowersTabUserControl : UserControl
    {
        // TODO: check, if this can be removed???
        public event PropertyChangedEventHandler MakeDirtyWithCharacterUpdate;

        private TableView<Power> _table;

        public PowersTabUserControl()
        {
            InitializeComponent();
            this.UpdateLightDarkMode();
            this.TranslateWinForm();

            _dropDownList = GenerateDropdownFilter();

            SuspendLayout();
            InitializeTable();
            ResumeLayout();
        }

        private Character _objCharacter;
        private readonly List<Tuple<string, Predicate<Power>>> _dropDownList;
        private bool _blnSearchMode;

        private void PowersTabUserControl_Load(object sender, EventArgs e)
        {
            if (_objCharacter != null)
                return;
            using (new CursorWait(this))
                RealLoad();
        }

        public void RealLoad()
        {
            if (ParentForm is CharacterShared frmParent)
                _objCharacter = frmParent.CharacterObject;
            else
            {
                Utils.BreakIfDebug();
                _objCharacter = new Character();
            }

            if (Utils.IsDesignerMode || Utils.IsRunningInVisualStudio)
                return;

            Stopwatch sw = Stopwatch.StartNew();  //Benchmark, should probably remove in release
            Stopwatch parts = Stopwatch.StartNew();
            //Keep everything visible until ready to display everything. This
            //seems to prevent redrawing everything each time anything is added
            //Not benched, but should be faster

            //Might also be useless horseshit, 2 lines

            //Visible = false;
            SuspendLayout();

            lblPowerPoints.DoOneWayDataBinding("Text", _objCharacter, nameof(Character.DisplayPowerPointsRemaining));

            parts.TaskEnd("MakePowerDisplay()");

            cboDisplayFilter.BeginUpdate();
            cboDisplayFilter.DataSource = null;
            cboDisplayFilter.ValueMember = "Item2";
            cboDisplayFilter.DisplayMember = "Item1";
            cboDisplayFilter.DataSource = _dropDownList;
            cboDisplayFilter.SelectedIndex = 1;
            cboDisplayFilter.MaxDropDownItems = _dropDownList.Count;
            cboDisplayFilter.EndUpdate();

            parts.TaskEnd("_ddl databind");

            //Visible = true;
            //this.ResumeLayout(false);
            //this.PerformLayout();
            parts.TaskEnd("visible");

            _table.Items = _objCharacter.Powers;

            parts.TaskEnd("resize");
            //this.Update();
            ResumeLayout(true);
            //this.PerformLayout();

            _objCharacter.Powers.ListChanged += OnPowersListChanged;
            _objCharacter.PropertyChanged += OnCharacterPropertyChanged;

            sw.Stop();
            Debug.WriteLine("RealLoad() in {0} ms", sw.Elapsed.TotalMilliseconds);
        }

        private void UnbindPowersTabUserControl()
        {
            if (_objCharacter != null)
            {
                _objCharacter.Powers.ListChanged -= OnPowersListChanged;
                _objCharacter.PropertyChanged -= OnCharacterPropertyChanged;
            }
        }

        private void OnCharacterPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Character.PowerPointsTotal) || e.PropertyName == nameof(Character.PowerPointsUsed))
                CalculatePowerPoints();
        }

        private void OnPowersListChanged(object sender, ListChangedEventArgs e)
        {
            switch (e.ListChangedType)
            {
                case ListChangedType.ItemChanged:
                    {
                        string propertyName = e.PropertyDescriptor?.Name;
                        if (propertyName == nameof(Power.FreeLevels) || propertyName == nameof(Power.TotalRating))
                        {
                            // recalculation of power points on rating/free levels change
                            CalculatePowerPoints();
                        }
                        break;
                    }
                case ListChangedType.Reset:
                case ListChangedType.ItemAdded:
                case ListChangedType.ItemDeleted:
                    CalculatePowerPoints();
                    break;
            }
        }

        private static List<Tuple<string, Predicate<Power>>> GenerateDropdownFilter()
        {
            List<Tuple<string, Predicate<Power>>> ret = new List<Tuple<string, Predicate<Power>>>(4)
            {
                new Tuple<string, Predicate<Power>>(LanguageManager.GetString("String_Search"),
                    null),
                new Tuple<string, Predicate<Power>>(LanguageManager.GetString("String_PowerFilterAll"),
                    power => true),
                new Tuple<string, Predicate<Power>>(LanguageManager.GetString("String_PowerFilterRatingAboveZero"),
                    power => power.Rating > 0),
                new Tuple<string, Predicate<Power>>(LanguageManager.GetString("String_PowerFilterRatingZero"),
                    power => power.Rating == 0)
            };

            /*
            using (XmlNodeList xmlPowerCategoryList = XmlManager.Load("powers.xml", objCharacter.Settings.CustomDataDictionary).SelectNodes("/chummer/categories/category"))
                if (xmlPowerCategoryList != null)
                    foreach (XmlNode xmlCategoryNode in xmlPowerCategoryList)
                    {
                        string strName = xmlCategoryNode.InnerText;
                        ret.Add(new Tuple<string, Predicate<Power>>(
                            LanguageManager.GetString("Label_Category") + LanguageManager.GetString("String_Space") + (xmlCategoryNode.Attributes?["translate"]?.InnerText ?? strName),
                            power => power.Category == strName));
                    }
                    */

            return ret;
        }

        private void cboDisplayFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cboDisplayFilter.SelectedItem is Tuple<string, Predicate<Power>> selectedItem)
            {
                if (selectedItem.Item2 == null)
                {
                    cboDisplayFilter.DropDownStyle = ComboBoxStyle.DropDown;
                    _blnSearchMode = true;
                    cboDisplayFilter.Text = string.Empty;
                }
                else
                {
                    cboDisplayFilter.DropDownStyle = ComboBoxStyle.DropDownList;
                    _blnSearchMode = false;
                    _table.Filter = selectedItem.Item2;
                }
            }
        }

        private void cboDisplayFilter_TextUpdate(object sender, EventArgs e)
        {
            if (_blnSearchMode)
            {
                _table.Filter = (power => GlobalSettings.InvariantCultureInfo.CompareInfo.IndexOf(power.CurrentDisplayName, cboDisplayFilter.Text, CompareOptions.IgnoreCase) >= 0);
            }
        }

        private void cmdAddPower_Click(object sender, EventArgs e)
        {
            // Open the Cyberware XML file and locate the selected piece.
            XmlDocument objXmlDocument = _objCharacter.LoadData("powers.xml");
            bool blnAddAgain;

            do
            {
                using (frmSelectPower frmPickPower = new frmSelectPower(_objCharacter))
                {
                    frmPickPower.ShowDialogSafe(this);

                    // Make sure the dialogue window was not canceled.
                    if (frmPickPower.DialogResult == DialogResult.Cancel)
                        break;

                    blnAddAgain = frmPickPower.AddAgain;

                    Power objPower = new Power(_objCharacter);

                    XmlNode objXmlPower = objXmlDocument.SelectSingleNode("/chummer/powers/power[id = " + frmPickPower.SelectedPower.CleanXPath() + "]");
                    if (objPower.Create(objXmlPower))
                    {
                        _objCharacter.Powers.Add(objPower);

                        MakeDirtyWithCharacterUpdate?.Invoke(null, null);
                    }
                }
            }
            while (blnAddAgain);
        }

        /// <summary>
        /// Calculate the number of Adept Power Points used.
        /// </summary>
        public void CalculatePowerPoints()
        {
            decimal decPowerPointsTotal = _objCharacter.PowerPointsTotal;
            decimal decPowerPointsRemaining = decPowerPointsTotal - _objCharacter.PowerPointsUsed;
            lblPowerPoints.Text = string.Format(GlobalSettings.CultureInfo, "{1}{0}({2}{0}{3})",
                LanguageManager.GetString("String_Space"), decPowerPointsTotal, decPowerPointsRemaining, LanguageManager.GetString("String_Remaining"));
        }

        private void InitializeTable()
        {
            _table = new TableView<Power>
            {
                Dock = DockStyle.Top,
                ToolTip = _tipTooltip
            };
            // create columns
            TableColumn<Power> nameColumn = new TableColumn<Power>(() => new TextTableCell())
            {
                Text = "Power",
                Extractor = (power => power.CurrentDisplayName),
                Tag = "String_Power",
                Sorter = (name1, name2) => string.Compare((string)name1, (string)name2, GlobalSettings.CultureInfo, CompareOptions.Ordinal)
            };
            nameColumn.AddDependency(nameof(Power.CurrentDisplayName));

            TableColumn<Power> actionColumn = new TableColumn<Power>(() => new TextTableCell())
            {
                Text = "Action",
                Extractor = (power => power.DisplayAction),
                Tag = "ColumnHeader_Action",
                Sorter = (action1, action2) => string.Compare((string)action1, (string)action2, GlobalSettings.CultureInfo, CompareOptions.Ordinal)
            };
            actionColumn.AddDependency(nameof(Power.DisplayAction));

            TableColumn<Power> ratingColumn = new TableColumn<Power>(() => new SpinnerTableCell<Power>(_table)
            {
                EnabledExtractor = (p => p.LevelsEnabled),
                MaxExtractor = (p => Math.Max(p.TotalMaximumLevels - p.FreeLevels, 0)),
                ValueUpdater = (p, newRating) =>
                {
                    int delta = ((int)newRating) - p.Rating;
                    if (delta != 0)
                    {
                        p.Rating += delta;
                    }
                },
                MinExtractor = (p => 0),
                ValueGetter = (p => p.Rating),
            })
            {
                Text = "Rating",
                Tag = "String_Rating",
                Sorter = (o1, o2) =>
                {
                    if (o1 is Power objPower1 && o2 is Power objPower2)
                        return objPower1.Rating - objPower2.Rating;
                    string strMessage = "Can't sort an Object of Type " + o1.GetType() +
                                        " against another one of Type " + o2.GetType() + " in the ratingColumn." +
                                        Environment.NewLine + "Both objects SHOULD be of the type \"Power\".";
                    throw new ArgumentException(strMessage, nameof(o1));
                },
            };

            ratingColumn.AddDependency(nameof(Power.LevelsEnabled));
            ratingColumn.AddDependency(nameof(Power.FreeLevels));
            ratingColumn.AddDependency(nameof(Power.TotalMaximumLevels));
            ratingColumn.AddDependency(nameof(Power.TotalRating));
            TableColumn<Power> totalRatingColumn = new TableColumn<Power>(() => new TextTableCell())
            {
                Text = "Total Rating",
                Extractor = (power => power.TotalRating),
                Tag = "String_TotalRating",
                Sorter = (o1, o2) =>
                {
                    if (o1 is Power objPower1 && o2 is Power objPower2)
                        return objPower1.TotalRating - objPower2.TotalRating;
                    string strMessage = "Can't sort an Object of Type " + o1.GetType() +
                                        " against another one of Type " + o2.GetType() + " in the totalRatingColumn." +
                                        Environment.NewLine + "Both objects SHOULD be of the type \"Power\".";
                    throw new ArgumentException(strMessage, nameof(o1));
                },
            };
            totalRatingColumn.AddDependency(nameof(Power.TotalRating));

            TableColumn<Power> powerPointsColumn = new TableColumn<Power>(() => new TextTableCell())
            {
                Text = "Power Points",
                Extractor = (power => power.DisplayPoints),
                Tag = "ColumnHeader_Power_Points",
                ToolTipExtractor = (item => item.ToolTip)
            };
            powerPointsColumn.AddDependency(nameof(Power.DisplayPoints));
            powerPointsColumn.AddDependency(nameof(Power.ToolTip));

            TableColumn<Power> sourceColumn = new TableColumn<Power>(() => new TextTableCell
            {
                Cursor = Cursors.Hand
            })
            {
                Text = "Source",
                Extractor = (power => power.SourceDetail),
                Tag = "Label_Source",
                ToolTipExtractor = (item => item.SourceDetail.LanguageBookTooltip),
            };
            powerPointsColumn.AddDependency(nameof(Power.Source));

            TableColumn<Power> adeptWayColumn = new TableColumn<Power>(() => new CheckBoxTableCell<Power>
            {
                ValueGetter = p => p.DiscountedAdeptWay,
                ValueUpdater = (p, check) => p.DiscountedAdeptWay = check,
                VisibleExtractor = p => p.AdeptWayDiscountEnabled,
                EnabledExtractor = p => (p.CharacterObject.AllowAdeptWayPowerDiscount || p.DiscountedAdeptWay),
                Alignment = Alignment.Center
            })
            {
                Text = "Adept Way",
                Tag = "Checkbox_Power_AdeptWay"
            };
            adeptWayColumn.AddDependency(nameof(Power.DiscountedAdeptWay));
            adeptWayColumn.AddDependency(nameof(Power.AdeptWayDiscountEnabled));
            adeptWayColumn.AddDependency(nameof(Character.AllowAdeptWayPowerDiscount));
            adeptWayColumn.AddDependency(nameof(Power.Rating));

            /*
             TableColumn<Power> geasColumn = new TableColumn<Power>(() => new CheckBoxTableCell<Power>()
            {
                ValueGetter = (p => p.DiscountedGeas),
                ValueUpdater = (p, check) => p.DiscountedGeas = check,
                Alignment = Alignment.Center
            })
            {
                Text = "Geas",
                Tag = "Checkbox_Power_Geas"
            };
            geasColumn.AddDependency(nameof(Power.DiscountedGeas));
            */

            TableColumn<Power> noteColumn = new TableColumn<Power>(() => new ButtonTableCell<Power>(new PictureBox
            {
                Image = Resources.note_edit,
                Size = GetImageSize(Resources.note_edit),
            })
            {
                ClickHandler = p =>
                {
                    using (frmNotes frmPowerNotes = new frmNotes(p.Notes, p.NotesColor))
                    {
                        frmPowerNotes.ShowDialogSafe(this);
                        if (frmPowerNotes.DialogResult == DialogResult.OK)
                            p.Notes = frmPowerNotes.Notes;
                    }
                },
                Alignment = Alignment.Center
            })
            {
                Text = "Notes",
                Tag = "ColumnHeader_Notes",
                ToolTipExtractor = (p =>
                {
                    string strTooltip = LanguageManager.GetString("Tip_Power_EditNotes");
                    if (!string.IsNullOrEmpty(p.Notes))
                        strTooltip += Environment.NewLine + Environment.NewLine + p.Notes.RtfToPlainText();
                    return strTooltip.WordWrap();
                })
            };
            noteColumn.AddDependency(nameof(Power.Notes));

            TableColumn<Power> deleteColumn = new TableColumn<Power>(() => new ButtonTableCell<Power>(new Button
            {
                Text = LanguageManager.GetString("String_Delete"),
                Tag = "String_Delete",
                Dock = DockStyle.Fill
            })
            {
                ClickHandler = p =>
                {
                    //Cache the parentform prior to deletion, otherwise the relationship is broken.
                    Form frmParent = ParentForm;
                    if (p.FreeLevels > 0)
                    {
                        string strImprovementSourceName = p.CharacterObject.Improvements.FirstOrDefault(x => x.ImproveType == Improvement.ImprovementType.AdeptPowerFreePoints && x.ImprovedName == p.Name && x.UniqueName == p.Extra)?.SourceName;
                        Gear objGear = p.CharacterObject.Gear.FirstOrDefault(x => x.Bonded && x.InternalId == strImprovementSourceName);
                        if (objGear != null)
                        {
                            objGear.Equipped = false;
                            objGear.Extra = string.Empty;
                        }
                    }
                    p.DeletePower();
                    p.UnbindPower();

                    if (frmParent is CharacterShared objParent)
                        objParent.IsCharacterUpdateRequested = true;
                },
                EnabledExtractor = (p => p.FreeLevels == 0)
            });
            deleteColumn.AddDependency(nameof(Power.FreeLevels));

            _table.Columns.Add(nameColumn);
            _table.Columns.Add(actionColumn);
            _table.Columns.Add(ratingColumn);
            _table.Columns.Add(totalRatingColumn);
            _table.Columns.Add(powerPointsColumn);
            _table.Columns.Add(adeptWayColumn);
            //_table.Columns.Add(geasColumn);
            _table.Columns.Add(noteColumn);
            _table.Columns.Add(sourceColumn);
            _table.Columns.Add(deleteColumn);
            _table.UpdateLightDarkMode();
            _table.TranslateWinForm();
            pnlPowers.Controls.Add(_table);
        }

        private static Size GetImageSize(Image image)
        {
            return new Size(image.Width, image.Height);
        }
    }
}
