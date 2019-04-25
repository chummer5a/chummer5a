using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Microsoft.AspNetCore.Identity;

namespace ChummerHub.Models.V1
{
    public class ResultSinnerDelete : ResultBase
    {
        private bool Deleted { get; set; }

        public ResultSinnerDelete(Exception e) : base(e)
        {

        }

        public ResultSinnerDelete(bool deleted)
        {
            Deleted = deleted;
        }
    }
}
