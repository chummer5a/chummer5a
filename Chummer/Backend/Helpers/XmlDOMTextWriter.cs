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

using System.IO;
using System.Text;

namespace Chummer
{
    /// <summary>
    /// Copy of the otherwise inaccessible <see cref="System.Xml.XmlDomTextWriter"/> class
    /// </summary>
    internal class XmlDOMTextWriter : System.Xml.XmlTextWriter
    {
        public XmlDOMTextWriter(Stream w, Encoding encoding) : base(w, encoding)
        {
        }

        public XmlDOMTextWriter(string filename, Encoding encoding) : base(filename, encoding)
        {
        }

        public XmlDOMTextWriter(TextWriter w) : base(w)
        {
        }

        public override void WriteStartElement(string prefix, string localName, string ns)
        {
            if (ns.Length == 0 && prefix.Length != 0)
            {
                prefix = "";
            }

            base.WriteStartElement(prefix, localName, ns);
        }

        public override void WriteStartAttribute(string prefix, string localName, string ns)
        {
            if (ns.Length == 0 && prefix.Length != 0)
            {
                prefix = "";
            }

            base.WriteStartAttribute(prefix, localName, ns);
        }
    }
}
