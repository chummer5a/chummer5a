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


using Chummer.Backend.Attributes.OptionAttributes;
using iTextSharp.text.pdf;
using System;
using System.IO;

namespace Chummer
{
    public enum ClipboardContentType
    {
        None = 0,
        Armor,
        ArmorMod,
        Cyberware,
        Gear,
        Lifestyle,
        Vehicle,
        Weapon,
        WeaponAccessory
    }

	public class SourcebookInfo
	{
	    public SourcebookInfo(string code,string name)
	    {
	        Name = name;  //TODO: Localize
	        Code = code;
	    }

        private PdfReader _objPdfReader;

	    #region Properties
	    public string Name { get; }

		public string Code { get; }

        [IsPath(Filter = "PDF Files|*.pdf|All files|*")]
	    public string Path { get; set; } = "";

	    public int Offset { get; set; } = 0;

        internal PdfReader CachedPdfReader
        {
            get
            {
                if (_objPdfReader == null)
                {
                    Uri uriPath = new Uri(Path);
                    if (File.Exists(uriPath.LocalPath))
                    {
                        // using the "partial" param it runs much faster and I couldnt find any downsides to it
                        _objPdfReader = new PdfReader(uriPath.LocalPath, null, true);
                    }
                }
                return _objPdfReader;
            }
        }

        #endregion
    }
}
