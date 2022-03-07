using System;
using System.Threading.Tasks;
using System.Xml;

namespace Chummer
{
    public struct XmlElementWriteHelper : IDisposable, IAsyncDisposable
    {
        private readonly XmlWriter _objWriter;

        public static XmlElementWriteHelper StartElement(XmlWriter objWriter, string localName)
        {
            objWriter.WriteStartElement(localName);
            return new XmlElementWriteHelper(objWriter);
        }

        public static async Task<XmlElementWriteHelper> StartElementAsync(XmlWriter objWriter, string localName)
        {
            await objWriter.WriteStartElementAsync(localName);
            return new XmlElementWriteHelper(objWriter);
        }

        private XmlElementWriteHelper(XmlWriter objWriter)
        {
            _objWriter = objWriter;
        }


        /// <inheritdoc />
        public void Dispose()
        {
            _objWriter.WriteEndElement();
        }

        /// <inheritdoc />
        public ValueTask DisposeAsync()
        {
            return new ValueTask(_objWriter.WriteEndElementAsync());
        }
    }
}
