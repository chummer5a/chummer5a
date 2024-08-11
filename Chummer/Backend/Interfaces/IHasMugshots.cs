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
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.XPath;

namespace Chummer
{
    public interface IHasMugshots : IDisposable, IAsyncDisposable
    {
        ThreadSafeList<Image> Mugshots { get; }
        Image MainMugshot { get; set; }
        int MainMugshotIndex { get; set; }

        Task<Image> GetMainMugshotAsync(CancellationToken token = default);

        Task SetMainMugshotAsync(Image value, CancellationToken token = default);

        Task<int> GetMainMugshotIndexAsync(CancellationToken token = default);

        Task SetMainMugshotIndexAsync(int value, CancellationToken token = default);

        Task ModifyMainMugshotIndexAsync(int value, CancellationToken token = default);

        void SaveMugshots(XmlWriter objWriter, CancellationToken token = default);

        Task SaveMugshotsAsync(XmlWriter objWriter, CancellationToken token = default);

        void LoadMugshots(XPathNavigator xmlSavedNode, CancellationToken token = default);

        Task LoadMugshotsAsync(XPathNavigator xmlSavedNode, CancellationToken token = default);

        Task PrintMugshots(XmlWriter objWriter, CancellationToken token = default);
    }
}
