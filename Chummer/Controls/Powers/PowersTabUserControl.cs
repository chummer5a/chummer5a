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
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
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
        private TableView<Power> _table;

        public PowersTabUserControl() : this(default)
        {
            // Need to set up constructors like this so that the WinForms designer doesn't freak out
        }

        public PowersTabUserControl(CancellationToken objMyToken)
        {
            _objMyToken = objMyToken;
            InitializeComponent();

            Disposed += (sender, args) => UnbindPowersTabUserControl();

            this.UpdateLightDarkMode(token: objMyToken);
            this.TranslateWinForm(token: objMyToken);

            _dropDownList = GenerateDropdownFilter(objMyToken);

            SuspendLayout();
            try
            {
                InitializeTable();
            }
            finally
            {
                ResumeLayout();
            }
        }

        private Character _objCharacter;
        private List<Tuple<string, Func<Power, Task<bool>>>> _dropDownList;
        private bool _blnSearchMode;

        private CancellationToken _objMyToken;

        public CancellationToken MyToken
        {
            get => _objMyToken;
            set
            {
                if (_objMyToken == value)
                    return;
                _objMyToken = value;
                _dropDownList = GenerateDropdownFilter(value);
            }
        }

        public Character CachedCharacter { get; set; }

        private async void PowersTabUserControl_Load(object sender, EventArgs e)
        {
            if (_objCharacter != null)
                return;
            CursorWait objCursorWait = await CursorWait.NewAsync(this, token: MyToken).ConfigureAwait(false);
            try
            {
                await RealLoad(MyToken, MyToken).ConfigureAwait(false);
            }
            finally
            {
                await objCursorWait.DisposeAsync().ConfigureAwait(false);
            }
        }

        public async ValueTask RealLoad(CancellationToken objMyToken = default, CancellationToken token = default)
        {
            if (CachedCharacter != null)
            {
                if (Interlocked.CompareExchange(ref _objCharacter, CachedCharacter, null) != null)
                    return;
            }
            else if (ParentForm is CharacterShared frmParent && frmParent.CharacterObject != null)
            {
                if (Interlocked.CompareExchange(ref _objCharacter, frmParent.CharacterObject, null) != null)
                    return;
            }
            else
            {
                Character objCharacter = new Character();
                if (Interlocked.CompareExchange(ref _objCharacter, objCharacter, null) != null)
                {
                    await objCharacter.DisposeAsync().ConfigureAwait(false);
                    return;
                }
                await this.DoThreadSafeAsync(x => x.Disposed += (sender, args) => objCharacter.Dispose(), token).ConfigureAwait(false);
                Utils.BreakIfDebug();
            }
            MyToken = objMyToken;

            if (Utils.IsDesignerMode || Utils.IsRunningInVisualStudio)
                return;

            Stopwatch sw = Stopwatch.StartNew();  //Benchmark, should probably remove in release
            //Keep everything visible until ready to display everything. This
            //seems to prevent redrawing everything each time anything is added
            //Not benched, but should be faster

            //Might also be useless horseshit, 2 lines

            //Visible = false;
            await this.DoThreadSafeAsync(x =>
            {
                x.SuspendLayout();
                try
                {
                    Stopwatch parts = Stopwatch.StartNew();

                    parts.TaskEnd("MakePowerDisplay()");

                    cboDisplayFilter.BeginUpdate();
                    try
                    {
                        cboDisplayFilter.DataSource = null;
                        cboDisplayFilter.ValueMember = "Item2";
                        cboDisplayFilter.DisplayMember = "Item1";
                        cboDisplayFilter.DataSource = _dropDownList;
                        cboDisplayFilter.SelectedIndex = 1;
                        cboDisplayFilter.MaxDropDownItems = _dropDownList.Count;
                    }
                    finally
                    {
                        cboDisplayFilter.EndUpdate();
                    }

                    parts.TaskEnd("_ddl databind");

                    //Visible = true;
                    //this.ResumeLayout(false);
                    //this.PerformLayout();
                    parts.TaskEnd("visible");

                    _table.Items = _objCharacter.Powers;

                    parts.TaskEnd("resize");
                }
                finally
                {
                    x.ResumeLayout(true);
                }
            }, token: token).ConfigureAwait(false);
            await lblPowerPoints.RegisterOneWayAsyncDataBindingAsync((x, y) => x.Text = y, _objCharacter,
                                                                     nameof(Character.DisplayPowerPointsRemaining),
                                                                     x => x.GetDisplayPowerPointsRemainingAsync(MyToken).AsTask(),
                                                                     MyToken, MyToken).ConfigureAwait(false);
            sw.Stop();
            Debug.WriteLine("RealLoad() in {0} ms", sw.Elapsed.TotalMilliseconds);

            _objCharacter.Powers.ListChanged += OnPowersListChanged;
            IAsyncDisposable objLocker
                = await _objCharacter.LockObject.EnterWriteLockAsync(MyToken).ConfigureAwait(false);
            try
            {
                _objCharacter.PropertyChanged += OnCharacterPropertyChanged;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        private void UnbindPowersTabUserControl()
        {
            if (_objCharacter?.IsDisposed == false)
            {
                _objCharacter.Powers.ListChanged -= OnPowersListChanged;
                using (_objCharacter.LockObject.EnterWriteLock())
                    _objCharacter.PropertyChanged -= OnCharacterPropertyChanged;
            }
        }

        private async void OnCharacterPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            try
            {
                if (e.PropertyName == nameof(Character.PowerPointsTotal)
                    || e.PropertyName == nameof(Character.PowerPointsUsed))
                    await CalculatePowerPoints(MyToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }

        private async void OnPowersListChanged(object sender, ListChangedEventArgs e)
        {
            try
            {
                switch (e.ListChangedType)
                {
                    case ListChangedType.ItemChanged:
                    {
                        string propertyName = e.PropertyDescriptor?.Name;
                        if (propertyName == nameof(Power.FreeLevels) || propertyName == nameof(Power.TotalRating))
                        {
                            // recalculation of power points on rating/free levels change
                            await CalculatePowerPoints(MyToken).ConfigureAwait(false);
                        }

                        break;
                    }
                    case ListChangedType.Reset:
                    case ListChangedType.ItemAdded:
                    case ListChangedType.ItemDeleted:
                        await CalculatePowerPoints(MyToken).ConfigureAwait(false);
                        break;
                }
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }

        private static List<Tuple<string, Func<Power, Task<bool>>>> GenerateDropdownFilter(CancellationToken objMyToken = default)
        {
            List<Tuple<string, Func<Power, Task<bool>>>> ret = new List<Tuple<string, Func<Power, Task<bool>>>>(4)
            {
                new Tuple<string, Func<Power, Task<bool>>>(LanguageManager.GetString("String_Search", token: objMyToken),
                                                           null),
                new Tuple<string, Func<Power, Task<bool>>>(LanguageManager.GetString("String_PowerFilterAll", token: objMyToken),
                                                           power => Task.FromResult(true)),
                new Tuple<string, Func<Power, Task<bool>>>(LanguageManager.GetString("String_PowerFilterRatingAboveZero", token: objMyToken),
                                                           async power =>
                                                           {
                                                               try
                                                               {
                                                                   return await power.GetRatingAsync(objMyToken).ConfigureAwait(false) > 0;
                                                               }
                                                               catch (OperationCanceledException)
                                                               {
                                                                   return true;
                                                               }
                                                           }),
                new Tuple<string, Func<Power, Task<bool>>>(LanguageManager.GetString("String_PowerFilterRatingZero", token: objMyToken),
                                                           async power =>
                                                           {
                                                               try
                                                               {
                                                                   return await power.GetRatingAsync(objMyToken).ConfigureAwait(false) == 0;
                                                               }
                                                               catch (OperationCanceledException)
                                                               {
                                                                   return true;
                                                               }
                                                           })
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

        private async void cboDisplayFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (!(cboDisplayFilter.SelectedItem is Tuple<string, Func<Power, Task<bool>>> selectedItem))
                    return;
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
                    await _table.SetFilterAsync(selectedItem.Item2, MyToken).ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }

        private async void cboDisplayFilter_TextUpdate(object sender, EventArgs e)
        {
            if (_blnSearchMode)
            {
                await _table.SetFilterAsync(
                    async power =>
                    {
                        try
                        {
                            return GlobalSettings.InvariantCultureInfo.CompareInfo.IndexOf(
                                await power.GetCurrentDisplayNameAsync(token: MyToken).ConfigureAwait(false),
                                await cboDisplayFilter.DoThreadSafeFuncAsync(x => x.Text, token: MyToken).ConfigureAwait(false),
                                CompareOptions.IgnoreCase) >= 0;
                        }
                        catch (OperationCanceledException)
                        {
                            return true;
                        }
                    }, MyToken).ConfigureAwait(false);
            }
        }

        private async void cmdAddPower_Click(object sender, EventArgs e)
        {
            try
            {
                // Open the Cyberware XML file and locate the selected piece.
                XmlDocument objXmlDocument = await _objCharacter.LoadDataAsync("powers.xml", token: MyToken)
                                                                .ConfigureAwait(false);
                bool blnAddAgain;

                do
                {
                    using (ThreadSafeForm<SelectPower> frmPickPower = await ThreadSafeForm<SelectPower>
                                                                            .GetAsync(
                                                                                () => new SelectPower(_objCharacter),
                                                                                MyToken).ConfigureAwait(false))
                    {
                        // Make sure the dialogue window was not canceled.
                        if (await frmPickPower.ShowDialogSafeAsync(_objCharacter, MyToken).ConfigureAwait(false)
                            == DialogResult.Cancel)
                            break;

                        blnAddAgain = frmPickPower.MyForm.AddAgain;

                        Power objPower = new Power(_objCharacter);

                        XmlNode objXmlPower = objXmlDocument.TryGetNodeByNameOrId("/chummer/powers/power", frmPickPower.MyForm.SelectedPower.CleanXPath())
                                              ?? throw new AbortedException();

                        if (objPower.Create(objXmlPower))
                        {
                            await _objCharacter.Powers.AddAsync(objPower, MyToken).ConfigureAwait(false);
                        }
                        else
                            await objPower.DeletePowerAsync(MyToken).ConfigureAwait(false);
                    }
                } while (blnAddAgain);
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }

        /// <summary>
        /// Calculate the number of Adept Power Points used.
        /// </summary>
        public async ValueTask CalculatePowerPoints(CancellationToken token = default)
        {
            decimal decPowerPointsTotal = await _objCharacter.GetPowerPointsTotalAsync(token).ConfigureAwait(false);
            decimal decPowerPointsRemaining = decPowerPointsTotal - await _objCharacter.GetPowerPointsUsedAsync(token).ConfigureAwait(false);
            string strText = string.Format(GlobalSettings.CultureInfo, "{1}{0}({2}{0}{3})",
                                           await LanguageManager.GetStringAsync("String_Space", token: token).ConfigureAwait(false), decPowerPointsTotal,
                                           decPowerPointsRemaining,
                                           await LanguageManager.GetStringAsync("String_Remaining", token: token).ConfigureAwait(false));
            await lblPowerPoints.DoThreadSafeAsync(x => x.Text = strText, token: token).ConfigureAwait(false);
        }

        private void InitializeTable()
        {
            _table = this.DoThreadSafeFunc(() => new TableView<Power>
            {
                Dock = DockStyle.Top
            });
            Disposed += (sender, args) => _table.Dispose();
            // create columns
            TableColumn<Power> nameColumn = this.DoThreadSafeFunc(
                () => new TableColumn<Power>(() => new TextTableCell())
                {
                    Text = "Power",
                    Extractor = (async power =>
                    {
                        try
                        {
                            return await power.GetCurrentDisplayNameAsync(MyToken).ConfigureAwait(false);
                        }
                        catch (OperationCanceledException)
                        {
                            return default;
                        }
                    }),
                    Tag = "String_Power",
                    Sorter = async (name1, name2) =>
                        string.Compare((await name1.ConfigureAwait(false)).ToString(), (await name2.ConfigureAwait(false)).ToString(), GlobalSettings.CultureInfo,
                                       CompareOptions.Ordinal)
                });
            nameColumn.AddDependency(nameof(Power.CurrentDisplayName));

            TableColumn<Power> actionColumn = this.DoThreadSafeFunc(
                () => new TableColumn<Power>(() => new TextTableCell())
                {
                    Text = "Action",
                    Extractor = (async power =>
                    {
                        try
                        {
                            return await power.GetDisplayActionAsync(MyToken).ConfigureAwait(false);
                        }
                        catch (OperationCanceledException)
                        {
                            return default;
                        }
                    }),
                    Tag = "ColumnHeader_Action",
                    Sorter = async (action1, action2) =>
                        string.Compare((await action1.ConfigureAwait(false)).ToString(), (await action2.ConfigureAwait(false)).ToString(), GlobalSettings.CultureInfo,
                                       CompareOptions.Ordinal)
                });
            actionColumn.AddDependency(nameof(Power.DisplayAction));

            TableColumn<Power> ratingColumn = this.DoThreadSafeFunc(() => new TableColumn<Power>(
                                                                        () => new SpinnerTableCell<Power>(_table)
                                                                        {
                                                                            EnabledExtractor = (p => p.LevelsEnabled),
                                                                            MaxExtractor = (p =>
                                                                                Math.Max(
                                                                                    p.TotalMaximumLevels - p.FreeLevels,
                                                                                    0)),
                                                                            ValueUpdater = (p, newRating) =>
                                                                            {
                                                                                int delta = ((int)newRating)
                                                                                    - p.Rating;
                                                                                if (delta != 0)
                                                                                {
                                                                                    p.Rating += delta;
                                                                                }
                                                                            },
                                                                            MinExtractor = (p => 0),
                                                                            ValueGetter = (p => p.Rating)
                                                                        })
            {
                Text = "Rating",
                Tag = "String_Rating",
                Sorter = async (o1, o2) =>
                {
                    try
                    {
                        if (await o1.ConfigureAwait(false) is Power objPower1 && await o2.ConfigureAwait(false) is Power objPower2)
                            return await objPower1.GetRatingAsync(MyToken).ConfigureAwait(false)
                                   - await objPower2.GetRatingAsync(MyToken).ConfigureAwait(false);
                    }
                    catch (OperationCanceledException)
                    {
                        return 0;
                    }

                    string strMessage
                        = "Can't sort an Object of Type "
                        + o1.GetType() +
                        " against another one of Type "
                        + o2.GetType() + " in the ratingColumn."
                        +
                        Environment.NewLine
                        + "Both objects SHOULD be of the type \"Power\".";
                    throw new ArgumentException(
                        strMessage, nameof(o1));
                }
            });

            ratingColumn.AddDependency(nameof(Power.LevelsEnabled));
            ratingColumn.AddDependency(nameof(Power.FreeLevels));
            ratingColumn.AddDependency(nameof(Power.TotalMaximumLevels));
            ratingColumn.AddDependency(nameof(Power.TotalRating));
            TableColumn<Power> totalRatingColumn = this.DoThreadSafeFunc(
                () => new TableColumn<Power>(() => new TextTableCell())
                {
                    Text = "Total Rating",
                    Extractor = (async power =>
                    {
                        try
                        {
                            return await power.GetTotalRatingAsync(MyToken).ConfigureAwait(false);
                        }
                        catch (OperationCanceledException)
                        {
                            return default;
                        }
                    }),
                    Tag = "String_TotalRating",
                    Sorter = async (o1, o2) =>
                    {
                        try
                        {
                            if (await o1.ConfigureAwait(false) is Power objPower1 && await o2.ConfigureAwait(false) is Power objPower2)
                                return await objPower1.GetTotalRatingAsync(MyToken).ConfigureAwait(false)
                                       - await objPower2.GetTotalRatingAsync(MyToken).ConfigureAwait(false);
                        }
                        catch (OperationCanceledException)
                        {
                            return 0;
                        }

                        string strMessage = "Can't sort an Object of Type " + o1.GetType() +
                                            " against another one of Type " + o2.GetType()
                                            + " in the totalRatingColumn." +
                                            Environment.NewLine + "Both objects SHOULD be of the type \"Power\".";
                        throw new ArgumentException(strMessage, nameof(o1));
                    }
                });
            totalRatingColumn.AddDependency(nameof(Power.TotalRating));

            TableColumn<Power> powerPointsColumn = this.DoThreadSafeFunc(
                () => new TableColumn<Power>(() => new TextTableCell())
                {
                    Text = "Power Points",
                    Extractor = (async power =>
                    {
                        try
                        {
                            return await power.GetDisplayPointsAsync(MyToken).ConfigureAwait(false);
                        }
                        catch (OperationCanceledException)
                        {
                            return default;
                        }
                    }),
                    Tag = "ColumnHeader_Power_Points",
                    ToolTipExtractor = async item =>
                    {
                        try
                        {
                            return await item.GetToolTipAsync(MyToken).ConfigureAwait(false);
                        }
                        catch (OperationCanceledException)
                        {
                            return string.Empty;
                        }
                    }
                });
            powerPointsColumn.AddDependency(nameof(Power.DisplayPoints));
            powerPointsColumn.AddDependency(nameof(Power.ToolTip));

            TableColumn<Power> sourceColumn = this.DoThreadSafeFunc(() => new TableColumn<Power>(() => new TextTableCell
            {
                Cursor = Cursors.Hand
            })
            {
                Text = "Source",
                Extractor = (async power =>
                {
                    try
                    {
                        return await power.GetSourceDetailAsync(MyToken).ConfigureAwait(false);
                    }
                    catch (OperationCanceledException)
                    {
                        return default;
                    }
                }),
                Tag = "Label_Source",
                ToolTipExtractor = async item =>
                {
                    try
                    {
                        return (await item.GetSourceDetailAsync(MyToken).ConfigureAwait(false)).LanguageBookTooltip;
                    }
                    catch (OperationCanceledException)
                    {
                        return string.Empty;
                    }
                }
            });
            powerPointsColumn.AddDependency(nameof(Power.Source));

            TableColumn<Power> adeptWayColumn = this.DoThreadSafeFunc(() => new TableColumn<Power>(
                                                                          () => new CheckBoxTableCell<Power>
                                                                          {
                                                                              ValueGetter = p => p.DiscountedAdeptWay,
                                                                              ValueUpdater = (p, check) =>
                                                                                  p.DiscountedAdeptWay = check,
                                                                              VisibleExtractor = p =>
                                                                                  p.AdeptWayDiscountEnabled,
                                                                              EnabledExtractor = p =>
                                                                                  p.CharacterObject
                                                                                      .AllowAdeptWayPowerDiscount
                                                                                  || p.DiscountedAdeptWay,
                                                                              Alignment = Alignment.Center
                                                                          })
            {
                Text = "Adept Way",
                Tag = "Checkbox_Power_AdeptWay"
            });
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

            TableColumn<Power> noteColumn = this.DoThreadSafeFunc(() => new TableColumn<Power>(
                () =>
                {
                    DpiFriendlyImagedButton cmdReturn
                        = new DpiFriendlyImagedButton
                        {
                            ImageDpi96 = Resources.note_edit,
                            ImageDpi192 = Resources.note_edit1,
                            Dock = DockStyle.Fill,
                            AutoSize = true,
                            FlatStyle = FlatStyle.Flat
                        };
                    cmdReturn.FlatAppearance.BorderSize = 0;
                    return new ButtonTableCell<Power>(cmdReturn)
                    {
                        ClickHandler = async p =>
                        {
                            try
                            {
                                using (ThreadSafeForm<EditNotes> frmPowerNotes = await ThreadSafeForm<EditNotes>
                                           .GetAsync(() => new EditNotes(p.Notes, p.NotesColor, MyToken), MyToken)
                                           .ConfigureAwait(false))
                                {
                                    if (await frmPowerNotes.ShowDialogSafeAsync(_objCharacter, MyToken)
                                                           .ConfigureAwait(false) == DialogResult.OK)
                                        p.Notes = frmPowerNotes.MyForm.Notes;
                                }
                            }
                            catch (OperationCanceledException)
                            {
                                //swallow this
                            }
                        },
                        Alignment = Alignment.Center

                    };
                })
            {
                Text = "Notes",
                Tag = "ColumnHeader_Notes",
                ToolTipExtractor = async p =>
                {
                    try
                    {
                        string strTooltip
                            = await LanguageManager.GetStringAsync(
                                "Tip_Power_EditNotes", token: MyToken).ConfigureAwait(false);
                        if (!string.IsNullOrEmpty(p.Notes))
                            strTooltip += Environment.NewLine
                                          + Environment.NewLine
                                          + await p.Notes.RtfToPlainTextAsync(token: MyToken).ConfigureAwait(false);
                        return strTooltip.WordWrap();
                    }
                    catch (OperationCanceledException)
                    {
                        return string.Empty;
                    }
                }
            });
            noteColumn.AddDependency(nameof(Power.Notes));

            TableColumn<Power> deleteColumn = this.DoThreadSafeFunc(() => new TableColumn<Power>(
                () =>
                {
                    DpiFriendlyImagedButton cmdReturn
                        = new DpiFriendlyImagedButton
                        {
                            ImageDpi96 = Resources.delete,
                            ImageDpi192 = Resources.delete1,
                            Dock = DockStyle.Fill,
                            AutoSize = true,
                            FlatStyle = FlatStyle.Flat
                        };
                    cmdReturn.FlatAppearance.BorderSize = 0;
                    return new ButtonTableCell<Power>(cmdReturn)
                    {
                        ClickHandler = async p =>
                        {
                            try
                            {
                                //Cache the parentform prior to deletion, otherwise the relationship is broken.
                                Form frmParent = await this.DoThreadSafeFuncAsync(x => x.ParentForm, token: MyToken)
                                                           .ConfigureAwait(false);
                                if (p.FreeLevels > 0)
                                {
                                    string strExtra = p.Extra;
                                    string strImprovementSourceName
                                        = (await ImprovementManager
                                                 .GetCachedImprovementListForValueOfAsync(
                                                     p.CharacterObject,
                                                     Improvement.ImprovementType.AdeptPowerFreePoints, p.Name,
                                                     token: MyToken).ConfigureAwait(false))
                                          .Find(x => x.UniqueName == strExtra)?.SourceName;
                                    if (!string.IsNullOrWhiteSpace(strImprovementSourceName))
                                    {
                                        Gear objGear = p.CharacterObject.Gear.FindById(strImprovementSourceName);
                                        if (objGear?.Bonded == true)
                                        {
                                            objGear.Equipped = false;
                                            objGear.Extra = string.Empty;
                                        }
                                    }
                                }

                                await p.DeletePowerAsync(MyToken).ConfigureAwait(false);

                                if (frmParent is CharacterShared objParent)
                                    await objParent.RequestCharacterUpdate(MyToken).ConfigureAwait(false);
                            }
                            catch (OperationCanceledException)
                            {
                                //swallow this
                            }
                        },
                        EnabledExtractor = (p => p.FreeLevels == 0)
                    };
                })
            {
                Text = string.Empty,
                ToolTipExtractor = async p =>
                {
                    try
                    {
                        return (await LanguageManager.GetStringAsync("String_Delete", token: MyToken).ConfigureAwait(false))
                            .WordWrap();
                    }
                    catch (OperationCanceledException)
                    {
                        return string.Empty;
                    }
                }
            });
            deleteColumn.AddDependency(nameof(Power.FreeLevels));

            TableColumn<Power> reapplyImprovementsColumn = this.DoThreadSafeFunc(() => new TableColumn<Power>(
                () =>
                {
                    DpiFriendlyImagedButton cmdReturn
                        = new DpiFriendlyImagedButton
                        {
                            ImageDpi96 = Resources.page_refresh,
                            ImageDpi192 = Resources.page_refresh1,
                            Dock = DockStyle.Fill,
                            AutoSize = true,
                            FlatStyle = FlatStyle.Flat
                        };
                    cmdReturn.FlatAppearance.BorderSize = 0;
                    return new ButtonTableCell<Power>(cmdReturn)
                    {
                        ClickHandler = async p =>
                        {
                            try
                            {
                                switch (ParentForm)
                                {
                                    case CharacterCreate
                                        frmCreate:
                                        await frmCreate
                                              .ReapplySpecificImprovements(
                                                  p.InternalId,
                                                  await p.GetCurrentDisplayNameAsync(MyToken).ConfigureAwait(false),
                                                  MyToken)
                                              .ConfigureAwait(false);
                                        break;
                                    case CharacterCareer
                                        frmCareer:
                                        await frmCareer
                                              .ReapplySpecificImprovements(
                                                  p.InternalId,
                                                  await p.GetCurrentDisplayNameAsync(MyToken).ConfigureAwait(false),
                                                  MyToken)
                                              .ConfigureAwait(false);
                                        break;
                                }
                            }
                            catch (OperationCanceledException)
                            {
                                //swallow this
                            }
                        },
                        Alignment = Alignment.Center
                    };
                })
            {
                Text = string.Empty,
                ToolTipExtractor = async p =>
                {
                    try
                    {
                        return (await LanguageManager.GetStringAsync("Menu_SpecialReapplyImprovements", token: MyToken).ConfigureAwait(false))
                            .WordWrap();
                    }
                    catch (OperationCanceledException)
                    {
                        return string.Empty;
                    }
                }
            });

            try
            {
                _table.Columns.Add(nameColumn, MyToken);
                _table.Columns.Add(actionColumn, MyToken);
                _table.Columns.Add(ratingColumn, MyToken);
                _table.Columns.Add(totalRatingColumn, MyToken);
                _table.Columns.Add(powerPointsColumn, MyToken);
                _table.Columns.Add(adeptWayColumn, MyToken);
                //_table.Columns.Add(geasColumn, MyToken);
                _table.Columns.Add(noteColumn, MyToken);
                _table.Columns.Add(sourceColumn, MyToken);
                _table.Columns.Add(deleteColumn, MyToken);
                _table.Columns.Add(reapplyImprovementsColumn, MyToken);
                _table.UpdateLightDarkMode(token: MyToken);
                _table.TranslateWinForm(token: MyToken);
            }
            catch (OperationCanceledException)
            {
                return;
            }
            pnlPowers.Controls.Add(_table);
        }
    }
}
