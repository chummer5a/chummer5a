using System;
using System.Collections.Generic;

namespace ChummerHub.Models.V1
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'UploadInfoObject'
    public class UploadInfoObject
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'UploadInfoObject'
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'UploadInfoObject.UploadDateTime'
        public DateTime? UploadDateTime { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'UploadInfoObject.UploadDateTime'

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'UploadInfoObject.Client'
        public UploadClient Client { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'UploadInfoObject.Client'

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'UploadInfoObject.SINners'
        public IEnumerable<SINner> SINners { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'UploadInfoObject.SINners'

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'UploadInfoObject.Groupname'
        public String Groupname { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'UploadInfoObject.Groupname'
    }
}
