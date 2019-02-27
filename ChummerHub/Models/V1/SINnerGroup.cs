using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using ChummerHub.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ChummerHub.Models.V1
{
    public class SINnerGroup
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid? Id { get; set; }

        public bool IsPublic { get; set; }

        
        public string Groupname { get; set; }

        public SINnerGroup()
        {
            //MySINners = new List<SINner>();
        }

        //public List<SINner> MySINners { get; set; }

        public async Task<List<SINner>> GetGroupMembers(ApplicationDbContext context)
        {
            try
            {
                var groupmembers = from a in context.SINners.Include(a => a.MyGroup)
                                   .Include(b => b.SINnerMetaData.Visibility.UserRights)
                                   where a.MyGroup.Id == this.Id
                                   select a;
                return groupmembers.ToList();

            }
            catch(Exception e)
            {
                System.Diagnostics.Trace.TraceError(e.Message, e);
                throw;
            }
        }

        //public async Task<List<SINerUserRight>> GetUserRights(ApplicationDbContext context)
        //{
        //    List<SINerUserRight> result = new List<SINerUserRight>();
        //    try
        //    {
        //        var sinners = await this.GetSinners(context);

        //        foreach(var sinner in sinners)
        //        {
        //            var members = (from a in sinner.SINnerMetaData.Visibility.UserRights select a);
        //            foreach(var member in members)
        //            {
        //                if(!result.Contains(member))
        //                    result.Add(member);
        //            }
        //        }
        //        return result;
        //    }
        //    catch(Exception e)
        //    {
        //        System.Diagnostics.Trace.TraceError(e.Message, e);
        //        throw;
        //    }
        //}
    }
}

