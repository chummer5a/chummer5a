using System;

namespace ChummerHub.Models.V1
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ResultGroupPutSetting'
    public class ResultGroupPutSetting : ResultBase
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ResultGroupPutSetting'
    {
        private SINnerGroup MyGroup { get; set; }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ResultGroupPutSetting.ResultGroupPutSetting()'
        public ResultGroupPutSetting()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ResultGroupPutSetting.ResultGroupPutSetting()'
        {

        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ResultGroupPutSetting.ResultGroupPutSetting(SINnerGroup)'
        public ResultGroupPutSetting(SINnerGroup group)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ResultGroupPutSetting.ResultGroupPutSetting(SINnerGroup)'
        {
            MyGroup = group;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ResultGroupPutSetting.ResultGroupPutSetting(Exception)'
        public ResultGroupPutSetting(Exception e) : base(e)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ResultGroupPutSetting.ResultGroupPutSetting(Exception)'
        {

        }

    }
}
