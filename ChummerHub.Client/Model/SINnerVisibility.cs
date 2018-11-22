using ChummerHub.Client.Backend;
using SINners.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SINners.Models
{
    public partial class SINnerVisibility
    {
        private List<ApplicationUser> _isVisibleToUserslist;
        public IReadOnlyList<ApplicationUser> IsVisibleToUsers
        {
            get
            {
                if (_isVisibleToUserslist == null)
                {
                    _isVisibleToUserslist = new List<ApplicationUser>();
                    foreach(var uid in this.IsVisibleToUserGuids)
                    {
                        var user = StaticUtils.Client.GetUserByGuid(uid);
                        _isVisibleToUserslist.Add(user);
                    }
                    
                }
                return _isVisibleToUserslist;

            }
        }
    }
}
