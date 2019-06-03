using System;

namespace ChummerHub.Client.Model
{
    public class ResultBase
    {
        public Exception MyException { get; set; }
        public bool CallSuccess { get; set; }
        public string ErrorText { get; set; }
        
        public ResultBase()
        {
            CallSuccess = true;
        }

    }
}
