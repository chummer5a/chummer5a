using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chummer
{
    public class TextEventArgs : EventArgs
    {
        private readonly string _strText;

        public TextEventArgs(string strText)
        {
            _strText = strText;
        }

        public string Text => _strText;
    }
}
