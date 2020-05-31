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
using System.Windows.Forms;
using System.Windows.Media;
using LiveCharts;
using LiveCharts.Wpf;

namespace Chummer.UI.Charts
{
    public partial class ExpenseChart : UserControl
    {
        private ChartValues<double> _karma;
        private ChartValues<double> _nuyen;
        private ChartValues<string> _date;

        public ExpenseChart()
        {
            InitializeComponent();
            _karma = new ChartValues<double>();
            _nuyen = new ChartValues<double>();
            _date = new ChartValues<string>();
            cartesianChart1.Series = new SeriesCollection
            {
                new LineSeries
                {
                    Title = LanguageManager.GetString("String_KarmaRemaining"),
                    Values = Karma,
                    LineSmoothness = 1
                },
                new LineSeries
                {
                    Title = LanguageManager.GetString("String_NuyenRemaining"),
                    Values = Nuyen,
                    LineSmoothness = 1
                }
                /*,
                new LineSeries
                {
                    Title = "Series 2",
                    Values = new ChartValues<double> {5, 2, 8, 3},
                    PointGeometry = DefaultGeometries.Square,
                    PointGeometrySize = 15
                }*/
            };
            /*
            cartesianChart1.AxisX.Add(new Axis
            {
                Title = "Month",
                Labels = new[] { "Jan", "Feb", "Mar", "Apr", "May" }
            });

            cartesianChart1.AxisY.Add(new Axis
            {
                Title = "Sales",
                LabelFormatter = value => value.ToString("C")
            });

            //modifying the series collection will animate and update the chart
            cartesianChart1.Series.Add(new LineSeries
            {
                Values = new ChartValues<double> { 5, 3, 2, 4, 5 },
                LineSmoothness = 0, //straight lines, 1 really smooth lines
                PointGeometry = Geometry.Parse("m 25 70.36218 20 -28 -20 22 -8 -6 z"),
                PointGeometrySize = 50,
                PointForeground = Brushes.Gray
            });

            //modifying any series values will also animate and update the chart
            cartesianChart1.Series[2].Values.Add(5d);*/
            cartesianChart1.AxisX.Add(new Axis
            {
                Title = "Month",
                Labels = Date
            });
        }

        #region Properties

        public ChartValues<double> Karma
        {
            get => _karma;
            set => _karma = value;
        }

        public ChartValues<double> Nuyen
        {
            get => _nuyen;
            set => _nuyen = value;
        }

        public ChartValues<string> Date
        {
            get => _date;
            set => _date = value;
        }

        #endregion
    }
}
