namespace ChummerHub.Models
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ErrorViewModel'
    public class ErrorViewModel
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ErrorViewModel'
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ErrorViewModel.RequestId'
        public string RequestId { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ErrorViewModel.RequestId'

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ErrorViewModel.ShowRequestId'
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ErrorViewModel.ShowRequestId'
    }
}
