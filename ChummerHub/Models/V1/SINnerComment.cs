using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChummerHub.Models.V1
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SINnerComment'
    public class SINnerComment
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SINnerComment'
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SINnerComment.Id'
        public Guid? Id { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SINnerComment.Id'

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SINnerComment.SINnerId'
        public Guid? SINnerId { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SINnerComment.SINnerId'

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SINnerComment.MyDateTime'
        public DateTime MyDateTime { get; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SINnerComment.MyDateTime'

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SINnerComment.Comment'
        public string Comment { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SINnerComment.Comment'

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SINnerComment.Email'
        public string Email { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SINnerComment.Email'

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SINnerComment.Downloads'
        public int Downloads { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SINnerComment.Downloads'
    }
}
