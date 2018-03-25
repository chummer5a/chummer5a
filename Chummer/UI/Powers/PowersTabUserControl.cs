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
using Chummer.Backend.Powers;
using Chummer.UI.Shared;
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
            LanguageManager.TranslateWinForm(GlobalOptions.Language, this);

            _dropDownList = GenerateDropdownFilter();

            SuspendLayout();
            InitializeTable();
            ResumeLayout();
        }

        public void MissingDatabindingsWorkaround()
        {
            //TODO: Databind this
            CalculatePowerPoints();
        }
        
        private Character _objCharacter;
        private readonly IList<Tuple<string, Predicate<Power>>> _dropDownList;
        private bool _blnSearchMode;
        
        private void PowersTabUserControl_Load(object sender, EventArgs e)
        {
            if (_objCharacter == null)
            {
                if (ParentForm != null)
                    ParentForm.Cursor = Cursors.WaitCursor;
                RealLoad();
                if (ParentForm != null)
                    ParentForm.Cursor = Cursors.Default;
            }
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

            _objCharacter.Powers.ListChanged += (sender, e) => {
                if (e.ListChangedType == ListChangedType.ItemChanged)
                {
                    string propertyName = e.PropertyDescriptor.Name;
                    if (propertyName == nameof(Power.FreeLevels) || propertyName == nameof(Power.TotalRating))
                    {
                        // recalculation of power points on rating/free levels change
                        CalculatePowerPoints();
                    }
                }
            };

            Stopwatch sw = Stopwatch.StartNew();  //Benchmark, should probably remove in release 
            Stopwatch parts = Stopwatch.StartNew();
            //Keep everything visible until ready to display everything. This 
            //seems to prevent redrawing everything each time anything is added
            //Not benched, but should be faster

            //Might also be useless horseshit, 2 lines

            //Visible = false;
            SuspendLayout();
            DoubleBuffered = true;

            CalculatePowerPoints();

            parts.TaskEnd("MakePowerDisplay()");

            cboDisplayFilter.DataSource = _dropDownList;
            cboDisplayFilter.ValueMember = "Item2";
            cboDisplayFilter.DisplayMember = "Item1";
            cboDisplayFilter.SelectedIndex = 1;
            cboDisplayFilter.MaxDropDownItems = _dropDownList.Count;

            parts.TaskEnd("_ddl databind");

            //Visible = true;
            //this.ResumeLayout(false);
            //this.PerformLayout();
            parts.TaskEnd("visible");

            _table.Height = pnlPowers.Height - _table.Top;
            _table.Width = pnlPowers.Width - _table.Left;
            _table.Items = _objCharacter.Powers;

            parts.TaskEnd("resize");
            //this.Update();
            ResumeLayout(true);
            //this.PerformLayout();
            sw.Stop();
            Debug.WriteLine("RealLoad() in {0} ms", sw.Elapsed.TotalMilliseconds);
        }
        
        private static IList<Tuple<string, Predicate<Power>>> GenerateDropdownFilter()
        {
            List<Tuple<string, Predicate<Power>>> ret = new List<Tuple<string, Predicate<Power>>>
            {
                new Tuple<string, Predicate<Power>>(LanguageManager.GetString("String_Search", GlobalOptions.Language), null),
                new Tuple<string, Predicate<Power>>(LanguageManager.GetString("String_PowerFilterAll", GlobalOptions.Language), power => true),
                new Tuple<string, Predicate<Power>>(LanguageManager.GetString("String_PowerFilterRatingAboveZero", GlobalOptions.Language), power => power.Rating > 0),
                new Tuple<string, Predicate<Power>>(LanguageManager.GetString("String_PowerFilterRatingZero", GlobalOptions.Language), power => power.Rating == 0)
            };

            /*
            using (XmlNodeList xmlPowerCategoryList = XmlManager.Load("powers.xml").SelectNodes("/chummer/categories/category"))
                if (xmlPowerCategoryList != null)
                    foreach (XmlNode xmlCategoryNode in xmlPowerCategoryList)
                    {
                        string strName = xmlCategoryNode.InnerText;
                        ret.Add(new Tuple<string, Predicate<Power>>(
                            $"{LanguageManager.GetString("Label_Category", GlobalOptions.Language)} {xmlCategoryNode.Attributes?["translate"]?.InnerText ?? strName}",
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
                _table.Filter = (power => GlobalOptions.InvariantCultureInfo.CompareInfo.IndexOf(power.DisplayName, cboDisplayFilter.Text, CompareOptions.IgnoreCase) >= 0);
            }
        }

        private void cmdAddPower_Click(object sender, EventArgs e)
        {
            // Open the Cyberware XML file and locate the selected piece.
            XmlDocument objXmlDocument = XmlManager.Load("powers.xml");
            bool blnAddAgain;

            do
            {
                frmSelectPower frmPickPower = new frmSelectPower(_objCharacter);
                frmPickPower.ShowDialog(this);

                // Make sure the dialogue window was not canceled.
                if (frmPickPower.DialogResult == DialogResult.Cancel)
                {
                    frmPickPower.Dispose();
                    break;
                }
                blnAddAgain = frmPickPower.AddAgain;

                Power objPower = new Power(_objCharacter);

                XmlNode objXmlPower = objXmlDocument.SelectSingleNode("/chummer/powers/power[id = \"" + frmPickPower.SelectedPower + "\"]");
                frmPickPower.Dispose();
                if (objPower.Create(objXmlPower))
                {
                    _objCharacter.Powers.Add(objPower);
                    MissingDatabindingsWorkaround();
                }
            }
            while (blnAddAgain);
        }

        public void RefreshPowerInfo(object sender, EventArgs e)
        {
            CalculatePowerPoints();
        }

        /// <summary>
        /// Calculate the number of Adept Power Points used.
        /// </summary>
        public void CalculatePowerPoints()
        {
            int intPowerPointsTotal = PowerPointsTotal;
            decimal decPowerPointsRemaining = intPowerPointsTotal - _objCharacter.Powers.AsParallel().Sum(objPower => objPower.PowerPoints);
            lblPowerPoints.Text = string.Format("{0} ({1} " + LanguageManager.GetString("String_Remaining", GlobalOptions.Language) + ')', intPowerPointsTotal, decPowerPointsRemaining);
        }

        private int PowerPointsTotal
        {
            get
            {
                int intMAG;
                if (_objCharacter.IsMysticAdept)
                {
                    // If both Adept and Magician are enabled, this is a Mystic Adept, so use the MAG amount assigned to this portion.
                    intMAG = _objCharacter.Options.MysAdeptSecondMAGAttribute ? _objCharacter.MAGAdept.TotalValue : _objCharacter.MysticAdeptPowerPoints;
                }
                else
                {
                    // The character is just an Adept, so use the full value.
                    intMAG = _objCharacter.MAG.TotalValue;
                }

                // Add any Power Point Improvements to MAG.
                intMAG += ImprovementManager.ValueOf(_objCharacter, Improvement.ImprovementType.AdeptPowerPoints);

                return Math.Max(intMAG, 0);
            }
        }

        private void InitializeTable()
        {
            _table = new TableView<Power>()
            {
                Location = new Point(3, 3)
            };
            _table.ToolTip = _tipTooltip;
            // create columns
            TableColumn<Power> nameColumn = new TableColumn<Power>(() => new TextTableCell())
            {
                Text = "Power",
                Extractor = (power => power.DisplayName),
                Tag = "String_Power",
                Sorter = (name1, name2) => string.Compare((string)name1, (string)name2)
            };
            nameColumn.AddDependency(nameof(Power.DisplayName));

            TableColumn<Power> actionColumn = new TableColumn<Power>(() => new TextTableCell())
            {
                Text = "Action",
                Extractor = (power => power.DisplayAction),
                Tag = "ColumnHeader_Action",
                Sorter = (action1, action2) => string.Compare((string)action1, (string)action2)
            };
            actionColumn.AddDependency(nameof(Power.DisplayAction));

            TableColumn<Power> ratingColumn = new TableColumn<Power>(() => new SpinnerTableCell<Power>(_table)
            {
                EnabledExtractor = (p => p.LevelsEnabled),
                MinExtractor = (p => p.FreeLevels),
                MaxExtractor = (p => p.TotalMaximumLevels),
                ValueUpdater = (p, newRating) =>
                {
                    int delta = ((int)newRating) - p.TotalRating;
                    if (delta != 0)
                    {
                        p.Rating += delta;
                    }

                },
                ValueGetter = (p => p.TotalRating),
            })
            {
                Text = "Rating",
                Tag = "String_Rating",
                Sorter = (o1, o2) => ((Power)o1).Rating - ((Power)o2).Rating
            };
            ratingColumn.AddDependency(nameof(Power.LevelsEnabled));
            ratingColumn.AddDependency(nameof(Power.FreeLevels));
            ratingColumn.AddDependency(nameof(Power.TotalMaximumLevels));
            ratingColumn.AddDependency(nameof(Power.TotalRating));

            TableColumn<Power> powerPointsColumn = new TableColumn<Power>(() => new TextTableCell())
            {
                Text = "Power Points",
                Extractor = (power => power.DisplayPoints),
                Tag = "ColumnHeader_Power_Points",
                ToolTipExtractor = (item => item.ToolTip)
            };
            powerPointsColumn.AddDependency(nameof(Power.DisplayPoints));
            powerPointsColumn.AddDependency(nameof(Power.ToolTip));

            TableColumn<Power> adeptWayColumn = new TableColumn<Power>(() => new CheckBoxTableCell<Power>()
            {
                ValueGetter = (p => p.DiscountedAdeptWay),
                ValueUpdater = (p, check) => p.DiscountedAdeptWay = check,
                VisibleExtractor = (p => p.AdeptWayDiscountEnabled),
                Alignment = Alignment.Center
            })
            {
                Text = "Adept Way",
                Tag = "Checkbox_Power_AdeptWay"
            };
            adeptWayColumn.AddDependency(nameof(Power.DiscountedAdeptWay));
            adeptWayColumn.AddDependency(nameof(Power.AdeptWayDiscountEnabled));
            adeptWayColumn.AddDependency(nameof(Power.Rating));

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

            TableColumn<Power> noteColumn = new TableColumn<Power>(() => new ButtonTableCell<Power>(new PictureBox()
            {
                Image = Chummer.Properties.Resources.note_edit,
                Size = GetImageSize(Chummer.Properties.Resources.note_edit),
            })
            {
                ClickHandler = p => {
                    frmNotes frmPowerNotes = new frmNotes
                    {
                        Notes = p.Notes
                    };
                    frmPowerNotes.ShowDialog(this);

                    if (frmPowerNotes.DialogResult == DialogResult.OK)
                        p.Notes = frmPowerNotes.Notes;
                    p.ForceEvent(nameof(Power.Notes));
                },
                Alignment = Alignment.Center
            })
            {
                Text = "Notes",
                Tag = "ColumnHeader_Notes",
                ToolTipExtractor = (p => {
                    string strTooltip = LanguageManager.GetString("Tip_Power_EditNotes", GlobalOptions.Language);
                    if (!string.IsNullOrEmpty(p.Notes))
                        strTooltip += Environment.NewLine + Environment.NewLine + p.Notes;
                    return strTooltip.WordWrap(100);
                })
            };
            noteColumn.AddDependency(nameof(Power.Notes));

            TableColumn<Power> deleteColumn = new TableColumn<Power>(() => new ButtonTableCell<Power>(new Button() { Text = "Delete", Tag = "String_Delete", BackColor = SystemColors.Control })
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
                    p.Deleting = true;
                    ImprovementManager.RemoveImprovements(p.CharacterObject, Improvement.ImprovementSource.Power, p.InternalId);
                    p.CharacterObject.Powers.Remove(p);
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
            _table.Columns.Add(powerPointsColumn);
            _table.Columns.Add(adeptWayColumn);
            _table.Columns.Add(geasColumn);
            _table.Columns.Add(noteColumn);
            _table.Columns.Add(deleteColumn);
            LanguageManager.TranslateWinForm(GlobalOptions.Language, _table);

            pnlPowers.Controls.Add(_table);
        }

        private static Size GetImageSize(Image image)
        {
            return new Size(image.Width, image.Height);
        }
    }
}
