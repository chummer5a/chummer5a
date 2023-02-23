/*  This file is part of Chummer5a.
 *
 *  Chummer5a is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  Chummer5a is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with Chummer5a.  If not, see <http://www.gnu.org/licenses/>.
 *
 *  You can obtain the full source code for Chummer5a at
 *  https://github.com/chummer5a/chummer5a
 */
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
