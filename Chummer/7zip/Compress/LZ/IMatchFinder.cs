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

        ValueTask InitAsync(CancellationToken token = default);

        void ReleaseStream();

        byte GetIndexByte(int index);

        uint GetMatchLen(int index, uint distance, uint limit);

        uint GetNumAvailableBytes();
    }

    internal interface IMatchFinder : IInWindowStream
    {
        void Create(uint historySize, uint keepAddBufferBefore,
                uint matchMaxLen, uint keepAddBufferAfter);

        uint GetMatches(uint[] distances);

        ValueTask<uint> GetMatchesAsync(uint[] distances, CancellationToken token = default);

        void Skip(uint num);

        ValueTask SkipAsync(uint num, CancellationToken token = default);
    }
}
