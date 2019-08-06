using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Chummer.Classes
{
    static class  ClipboardManager
    {
        private static XmlDocument _xmlClipboard = new XmlDocument();
        /// <summary>
        /// Clipboard.
        /// </summary>
        public static XmlDocument Clipboard
        {
            get => _xmlClipboard;
            set
            {
                if (_xmlClipboard != value)
                {
                    _xmlClipboard = value;
                    ClipboardChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(Clipboard)));
                }
            }
        }

        /// <summary>
        /// Type of data that is currently stored in the clipboard.
        /// </summary>
        public static ClipboardContentType ClipboardContentType { get; set; } = new ClipboardContentType();


        public static event PropertyChangedEventHandler ClipboardChanged;
    }
}
