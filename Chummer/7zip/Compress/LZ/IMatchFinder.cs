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
// IMatchFinder.cs

using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SevenZip.Compression.LZ
{
    internal interface IInWindowStream
    {
        void SetStream(Stream inStream);

        void Init();

        Task InitAsync(CancellationToken token = default);

        void ReleaseStream();

        byte GetIndexByte(int index);

        int GetMatchLen(int index, int distance, int limit);

        int GetNumAvailableBytes();
    }

    internal interface IMatchFinder : IInWindowStream
    {
        void Create(int historySize, int keepAddBufferBefore,
                int matchMaxLen, int keepAddBufferAfter);

        int GetMatches(int[] distances);

        Task<int> GetMatchesAsync(int[] distances, CancellationToken token = default);

        void Skip(int num);

        Task SkipAsync(int num, CancellationToken token = default);
    }
}
