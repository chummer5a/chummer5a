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
using Chummer.UI.Table;

// ReSharper disable StringCompareToIsCultureSpecific

namespace Chummer.UI.Powers
{
    public partial class PowersTabUserControl : UserControl
    {
        // TODO: check, if this can be removed???
        public event PropertyChangedEventHandler MakeDirtyWithCharacterUpdate;
        
        private TableView<AdeptPower> _table;

        public PowersTabUserControl()
        {
            InitializeComponent();
            LanguageManager.TranslateWinForm(GlobalOptions.Language, this);

            _dropDownList = GenerateDropdownFilter();

            SuspendLayout();
            InitializeTable();
            ResumeLayout();
        }
        
        private Character _objCharacter;
        private readonly IList<Tuple<string, Predicate<AdeptPower>>> _dropDownList;
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
                    string propertyName = e.PropertyDescriptor?.Name;
                    if (propertyName == nameof(AdeptPower.FreeLevels) || propertyName == nameof(AdeptPower.TotalRating))
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

            lblPowerPoints.DataBindings.Add("Text", _objCharacter, nameof(Character.DisplayPowerPointsRemaining), false, DataSourceUpdateMode.OnPropertyChanged);

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
        
        private static IList<Tuple<string, Predicate<AdeptPower>>> GenerateDropdownFilter()
        {
            List<Tuple<string, Predicate<AdeptPower>>> ret = new List<Tuple<string, Predicate<AdeptPower>>>
            {
                new Tuple<string, Predicate<AdeptPower>>(LanguageManager.GetString("String_Search", GlobalOptions.Language), null),
                new Tuple<string, Predicate<AdeptPower>>(LanguageManager.GetString("String_PowerFilterAll", GlobalOptions.Language), power => true),
                new Tuple<string, Predicate<AdeptPower>>(LanguageManager.GetString("String_PowerFilterRatingAboveZero", GlobalOptions.Language), power => power.Rating > 0),
                new Tuple<string, Predicate<AdeptPower>>(LanguageManager.GetString("String_PowerFilterRatingZero", GlobalOptions.Language), power => power.Rating == 0)
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
            if (cboDisplayFilter.SelectedItem is Tuple<string, Predicate<AdeptPower>> selectedItem)
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

                AdeptPower objPower = new AdeptPower(_objCharacter);

                XmlNode objXmlPower = objXmlDocument.SelectSingleNode("/chummer/powers/power[id = \"" + frmPickPower.SelectedPower + "\"]");
                frmPickPower.Dispose();
                if (objPower.Create(objXmlPower))
                {
                    _objCharacter.Powers.Add(objPower);

                    MakeDirtyWithCharacterUpdate?.Invoke(null, null);
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
            _table = new TableView<AdeptPower>
            {
                Location = new Point(3, 3),
                ToolTip = _tipTooltip
            };
            // create columns
            TableColumn<AdeptPower> nameColumn = new TableColumn<AdeptPower>(() => new TextTableCell())
            {
                Text = "Power",
                Extractor = (power => power.DisplayName),
                Tag = "String_Power",
                Sorter = (name1, name2) => string.Compare((string)name1, (string)name2, GlobalOptions.CultureInfo, CompareOptions.Ordinal)
            };
            nameColumn.AddDependency(nameof(Power.DisplayName));

            TableColumn<AdeptPower> actionColumn = new TableColumn<AdeptPower>(() => new TextTableCell())
            {
                Text = "Action",
                Extractor = (power => power.DisplayAction),
                Tag = "ColumnHeader_Action",
                Sorter = (action1, action2) => string.Compare((string)action1, (string)action2, GlobalOptions.CultureInfo, CompareOptions.Ordinal)
            };
            actionColumn.AddDependency(nameof(Power.DisplayAction));

            TableColumn<AdeptPower> ratingColumn = new TableColumn<AdeptPower>(() => new SpinnerTableCell<AdeptPower>(_table)
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
                Sorter = (o1, o2) => ((AdeptPower)o1).Rating - ((AdeptPower)o2).Rating
            };


            ratingColumn.AddDependency(nameof(AdeptPower.LevelsEnabled));
            ratingColumn.AddDependency(nameof(AdeptPower.FreeLevels));
            ratingColumn.AddDependency(nameof(AdeptPower.TotalMaximumLevels));
            ratingColumn.AddDependency(nameof(AdeptPower.TotalRating));
            TableColumn<AdeptPower> totalRatingColumn = new TableColumn<AdeptPower>(() => new TextTableCell())
            {
                Text = "Total Rating",
                Extractor = (power => power.TotalRating),
                Tag = "String_TotalRating",
                Sorter = (o1, o2) => ((AdeptPower)o1).TotalRating - ((AdeptPower)o2).TotalRating
            };
            totalRatingColumn.AddDependency(nameof(AdeptPower.TotalRating));

            TableColumn<AdeptPower> powerPointsColumn = new TableColumn<AdeptPower>(() => new TextTableCell())
            {
                Text = "Power Points",
                Extractor = (power => power.DisplayPoints),
                Tag = "ColumnHeader_Power_Points",
                ToolTipExtractor = (item => item.ToolTip)
            };
            powerPointsColumn.AddDependency(nameof(AdeptPower.DisplayPoints));
            powerPointsColumn.AddDependency(nameof(AdeptPower.ToolTip));

            TableColumn<AdeptPower> sourceColumn = new TableColumn<AdeptPower>(() => new TextTableCell())
            {
                Text = "Source",
                Extractor = (power => power.SourceDetail),
                Tag = "Label_Source",
                ToolTipExtractor = (item => item.SourceDetail.LanguageBookTooltip)
            };
            powerPointsColumn.AddDependency(nameof(AdeptPower.Source));

            TableColumn<AdeptPower> adeptWayColumn = new TableColumn<AdeptPower>(() => new CheckBoxTableCell<AdeptPower>()
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
            adeptWayColumn.AddDependency(nameof(AdeptPower.DiscountedAdeptWay));
            adeptWayColumn.AddDependency(nameof(AdeptPower.AdeptWayDiscountEnabled));
            adeptWayColumn.AddDependency(nameof(AdeptPower.Rating));

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

            TableColumn<AdeptPower> noteColumn = new TableColumn<AdeptPower>(() => new ButtonTableCell<AdeptPower>(new PictureBox()
            {
                Image = Properties.Resources.note_edit,
                Size = GetImageSize(Properties.Resources.note_edit),
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
            noteColumn.AddDependency(nameof(AdeptPower.Notes));

            TableColumn<AdeptPower> deleteColumn = new TableColumn<AdeptPower>(() => new ButtonTableCell<AdeptPower>(new Button() { Text = "Delete", Tag = "String_Delete", BackColor = SystemColors.Control })
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
            LanguageManager.TranslateWinForm(GlobalOptions.Language, _table);

            pnlPowers.Controls.Add(_table);
        }

        private static Size GetImageSize(Image image)
        {
            return new Size(image.Width, image.Height);
        }
    }
}
