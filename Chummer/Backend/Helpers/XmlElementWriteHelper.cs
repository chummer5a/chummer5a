using System;
using System.Threading.Tasks;
using System.Xml;

namespace Chummer
{
    public readonly struct XmlElementWriteHelper : IDisposable, IAsyncDisposable, IEquatable<XmlElementWriteHelper>
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

        /// <inheritdoc />
        public bool Equals(XmlElementWriteHelper other)
        {
            return Equals(_objWriter, other._objWriter);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return obj is XmlElementWriteHelper other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return (_objWriter != null ? _objWriter.GetHashCode() : 0);
        }

        public static bool operator ==(XmlElementWriteHelper left, XmlElementWriteHelper right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(XmlElementWriteHelper left, XmlElementWriteHelper right)
        {
            return !(left == right);
        }
    }
}
