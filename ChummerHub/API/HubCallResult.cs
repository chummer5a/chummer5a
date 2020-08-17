using System.Runtime.Serialization;

namespace ChummerHub.Models
{
    [DataContract]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'HubCallResult'
    public class HubCallResult
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'HubCallResult'
    {
        [DataMember(Name = "Success")]
        bool Success { get; set; }

        [DataMember(Name = "Message")]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'HubCallResult.MyExceptionMessage'
        public string MyExceptionMessage { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'HubCallResult.MyExceptionMessage'
    }
}
