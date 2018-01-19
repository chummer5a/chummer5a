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
using System.Collections.ObjectModel;

namespace Chummer.Properties
{
    public static class Contributors
    {
        public static ReadOnlyCollection<string> Usernames { get { return Array.AsReadOnly(_lstUsernames); } }
        private static readonly string[] _lstUsernames = {
            "Chummer 5 Is Alive<chummer5a>",
            "DelnarErsike",
            "Johannes Elgaard<joha4270>",
            "Stefan Niewerth<OLStefan>",
            "skarsol",
            "Hauke<HaukeW>",
            "HaikenEdge",
            "Holger Schlegel<holgerschlegel>",
            "chummer78",
            "cfresquet",
            "angelforest",
            "Youneko17",
            "Ternega",
            "sethsatan",
            "luizbgomide",
            "MerGatto<MerGatto>",
            "Michael Jesse<michaeljesse73>",
            "Skarablood",
            "Dmitri Suvorov<suvjunmd>",
            "rabbitslayer4",
            "SolitarySky",
            "argo2445",
            "masterki",
            "rolfman",
            "AMDX9",
            "David Dashifen Kees<dashifen>",
            "Kadrack",
            "mudge6",
            "Richard Zang<richard-zang>",
            "Claire<MachineMuse>",
        };
    }
}
