using System;

namespace ChummerHub.Models.V1
{
    public class ResultGroupPutSetting : ResultBase
    {
        private SINnerGroup MyGroup { get; set; }

        public ResultGroupPutSetting()
        {
        
        }

        public ResultGroupPutSetting(SINnerGroup group)
        {
            MyGroup = group;
        }

        public ResultGroupPutSetting(Exception e) : base(e)
        {

        }

    }
}
