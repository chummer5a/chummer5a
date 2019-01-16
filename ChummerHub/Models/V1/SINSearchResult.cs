using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChummerHub.Models.V1
{
    public class SINSearchResult
    {
        public SINSearchResult()
        {
            SINLists = new List<SINnersList>();
        }
        public List<SINnersList> SINLists { get; set; }

        public string ErrorText { get; set; }
    }

    public class SINnersList
    {
        public string Header { get; set; }

        public List<SINner> SINners { get; set; }

        public List<SINnersList> SINList { get; set; }

        public SINnersList()
        {
            Header = "defaultHeader";
            SINners = new List<SINner>();
            SINList = new List<SINnersList>();
        }
    }
}
