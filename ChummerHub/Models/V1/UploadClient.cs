using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChummerHub.Models.V1
{

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'UploadClient'
    public class UploadClient
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'UploadClient'
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'UploadClient.Id'
        public Guid Id { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'UploadClient.Id'

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'UploadClient.ChummerVersion'
        public string ChummerVersion { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'UploadClient.ChummerVersion'

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'UploadClient.InstallationId'
        public Guid? InstallationId { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'UploadClient.InstallationId'

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'UploadClient.ClientSecret'
        public string ClientSecret { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'UploadClient.ClientSecret'

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'UploadClient.UploadClient()'
        public UploadClient()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'UploadClient.UploadClient()'
        {
            this.Id = Guid.Empty;
            this.ChummerVersion = new Version().ToString();
            this.ClientSecret = string.Empty;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'UploadClient.UserEmail'
        public string UserEmail { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'UploadClient.UserEmail'


    }
}
