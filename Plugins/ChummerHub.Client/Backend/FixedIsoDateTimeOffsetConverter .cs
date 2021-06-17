using Newtonsoft.Json.Converters;
using System;

namespace ChummerHub.Client.Sinners
{
    public class FixedIsoDateTimeOffsetConverter : IsoDateTimeConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(DateTimeOffset) || objectType == typeof(DateTimeOffset?);
        }

        public FixedIsoDateTimeOffsetConverter()
        {
            DateTimeStyles = System.Globalization.DateTimeStyles.AssumeUniversal;
        }
    }
}
