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
        public static ReadOnlyCollection<string> Usernames => Array.AsReadOnly(s_LstUsernames);

        private static readonly string[] s_LstUsernames = {
#warning No OAuth token specified, so for everyone past the 59th place in the list, only usernames will be fetched
            "DelnarErsike",
            "Chummer 5 Is Alive<chummer5a>",
            "Archon Megalon<ArchonMegalon>",
            "Johannes Elgaard<joha4270>",
            "Stefan Niewerth<OLStefan>",
            "JoschiGrey",
            "skarsol",
            "Holger Schlegel<holgerschlegel>",
            "Hauke<HaukeW>",
            "HaikenEdge",
            "Luiz Borges<luizbgomide>",
            "chummer78",
            "Youneko17",
            "cnschu",
            "cfresquet",
            "angelforest",
            "Lochabar",
            "Stexinator",
            "Michael Jesse<michaeljesse73>",
            "JokerRouge",
            "wakaba-github",
            "MerGatto<MerGatto>",
            "Minnakht",
            "Ternega",
            "sethsatan",
            "gdelmee",
            "David Dashifen Kees<dashifen>",
            "Skarablood",
            "Claire<MachineMuse>",
            "Djordje Nikolic<Djordje-Nikolic>",
            "Dmitri Suvorov<suvjunmd>",
            "Emil Milanov<emomicrowave>",
            "Kelly Stewart<miscoined>",
            "KoshiirRa",
            "rabbitslayer4",
            "ReallyDarkMatter",
            "argo2445",
            "masterki",
            "rolfman",
            "handcraftedsource",
            "0x6a646f65",
            "AMDX9",
            "Kadrack",
            "Mondlied",
            "kandarian",
            "mudge6",
            "Richard Zang<richard-zang>",
            "Gimbalhawk",
            "APN-Pucky",
            "Alex Es<ElectronicRU>",
            "LeonardoDeQuirm",
            "SquadronROE",
            "Zeidra-Senester",
            "bodison",
            "Gerasimos \"Gerry\" Raptis<graptis>",
            "Johannes Novotny<jonovotny>",
            "Alec<batchii>",
            "hirudan",
            "Bodie Sullivan<BodieSullivan>",
            "Xenomorph-Alpha",
            "dethstrobe",
            "FelixDombek",
            "Fightbackman",
            "Hood281",
            "IsaacAlpharn",
            "Maxis010",
            "GearheadLydia",
            "waffle-iron",
            "Malkleth",
            "MemeticContagion",
            "crazymykl",
            "Moby2kBug",
            "Phil5555",
            "ResonantGhost",
            "botchi09",
            "TDW89",
            "UrsZeidler",
            "Xenryusho",
            "ZachSchaffer",
            "Zitchas",
            "ai-tsurugi",
            "blubderapo",
            "darksnakezero",
            "liclac",
            "exurgent",
            "johnwobrien",
            "mikekacz",
        };
    }
}
