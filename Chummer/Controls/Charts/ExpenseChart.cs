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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media;
using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;

namespace Chummer.UI.Charts
{
    public partial class ExpenseChart : UserControl
    {
        private Character _objCharacter;
        private readonly LineSeries _objMainSeries;
        private readonly Axis _objYAxis;
        private static readonly SolidColorBrush s_ObjKarmaFillBrush = new SolidColorBrush(Color.FromArgb(0x7F, 0, 0, 0xFF));
        private static readonly SolidColorBrush s_ObjNuyenFillBrush = new SolidColorBrush(Color.FromArgb(0x7F, 0xFF, 0, 0));

        public ExpenseChart()
        {
            InitializeComponent();
            _objMainSeries = new LineSeries
            {
                Title = LanguageManager.GetString("String_KarmaRemaining"),
                Values = ExpenseValues,
                LineSmoothness = 0.1,
                Stroke = Brushes.Blue,
                Fill = s_ObjKarmaFillBrush,
                PointGeometrySize = 8
            };
            chtCartesian.Series = new SeriesCollection
            {
                _objMainSeries
            };
            chtCartesian.AxisX.Add(new Axis
            {
                LabelFormatter = val => new DateTime((long)val).ToString(GlobalSettings.CustomDateTimeFormats
                    ? GlobalSettings.CustomDateFormat
                      + ' ' + GlobalSettings.CustomTimeFormat
                    : GlobalSettings.CultureInfo.DateTimeFormat.ShortDatePattern
                      + ' ' + GlobalSettings.CultureInfo.DateTimeFormat.ShortTimePattern, GlobalSettings.CultureInfo)
            });
            _objYAxis = new Axis
            {
                Title = LanguageManager.GetString("String_Karma"),
                LabelFormatter = val => val.ToString("#,0.##", GlobalSettings.CultureInfo)
            };
            chtCartesian.AxisY.Add(_objYAxis);
        }

        private async void ExpenseChart_Load(object sender, EventArgs e)
        {
            if (ParentForm is CharacterShared frmParent && frmParent.CharacterObject != null)
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
                await this.DoThreadSafeAsync(x => x.Disposed += (o, args) => objCharacter.Dispose()).ConfigureAwait(false);
                Utils.BreakIfDebug();
            }
            if (NuyenMode)
            {
                await _objYAxis.DoThreadSafeAsync(x => x.LabelFormatter = val =>
                                                      val.ToString((_objCharacter?.Settings.NuyenFormat ?? "#,0.##") + LanguageManager.GetString("String_NuyenSymbol"),
                                                                   GlobalSettings.CultureInfo)).ConfigureAwait(false);
            }
        }

        public Task NormalizeYAxis(CancellationToken token = default)
        {
            if (ExpenseValues.Count == 0)
                return Task.CompletedTask;
            double dblActualMin = ExpenseValues[0].Value;
            double dblActualMax = dblActualMin;
            foreach (double dblValue in ExpenseValues.Select(x => x.Value))
            {
                if (dblActualMax < dblValue)
                    dblActualMax = dblValue;
                else if (dblActualMin > dblValue)
                    dblActualMin = dblValue;
            }
            if (NuyenMode)
            {
                dblActualMax = Math.Max(Math.Ceiling(dblActualMax / 5000.0), Math.Floor(dblActualMin / 5000.0) + 1) * 5000.0;
                dblActualMin = Math.Floor(dblActualMin / 5000.0) * 5000.0;
            }
            else
            {
                dblActualMax = Math.Max(Math.Ceiling(dblActualMax / 5.0), Math.Floor(dblActualMin / 5.0) + 1) * 5.0;
                dblActualMin = Math.Floor(dblActualMin / 5.0) * 5.0;
            }

            return _objYAxis.DoThreadSafeAsync(x =>
            {
                x.MinValue = dblActualMin;
                x.MaxValue = dblActualMax;
            }, token);
        }

        #region Properties

        [CLSCompliant(false)]
        public ChartValues<DateTimePoint> ExpenseValues { get; } = new ChartValues<DateTimePoint>();

        private bool _blnNuyenMode;

        public bool NuyenMode
        {
            get => _blnNuyenMode;
            set
            {
                if (_blnNuyenMode == value)
                    return;
                _blnNuyenMode = value;
                chtCartesian.DoThreadSafe(x => x.SuspendLayout());
                try
                {
                    if (value)
                    {
                        _objYAxis.DoThreadSafe(x =>
                        {
                            x.Title = LanguageManager.GetString("Label_SummaryNuyen");
                            x.LabelFormatter = val =>
                                val.ToString((_objCharacter?.Settings.NuyenFormat ?? "#,0.##") + LanguageManager.GetString("String_NuyenSymbol"),
                                             GlobalSettings.CultureInfo);
                        });
                        _objMainSeries.DoThreadSafe(x =>
                        {
                            x.Title = LanguageManager.GetString("String_NuyenRemaining");
                            x.Stroke = Brushes.Red;
                            x.Fill = s_ObjNuyenFillBrush;
                        });
                    }
                    else
                    {
                        _objYAxis.DoThreadSafe(x =>
                        {
                            x.Title = LanguageManager.GetString("String_Karma");
                            x.LabelFormatter = val => val.ToString("#,0.##", GlobalSettings.CultureInfo);
                        });
                        _objMainSeries.DoThreadSafe(x =>
                        {
                            x.Title = LanguageManager.GetString("String_KarmaRemaining");
                            x.Stroke = Brushes.Blue;
                            x.Fill = s_ObjKarmaFillBrush;
                        });
                    }
                }
                finally
                {
                    chtCartesian.DoThreadSafe(x => x.ResumeLayout());
                }
            }
        }

        #endregion Properties
    }
}
