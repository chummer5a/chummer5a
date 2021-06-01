using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChummerHub.Client.Sinners
{
    public class FixedIsoDateTimeOffsetConverter : IsoDateTimeConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(DateTimeOffset) || objectType == typeof(DateTimeOffset?);
        }

        public FixedIsoDateTimeOffsetConverter() : base()
        {
            this.DateTimeStyles = System.Globalization.DateTimeStyles.AssumeUniversal;
        }
    }
}
