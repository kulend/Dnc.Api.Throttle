using System;
using System.Collections.Generic;
using System.Text;

namespace Dnc.Api.Restriction
{
    public class ApiRestrictionAttribute : Attribute
    {
        public int LimitCount { set; get; } = 1;

        public TimeSpan Duration { set; get; } = TimeSpan.FromMinutes(1);

        public RecognitionMethod RecognitionMethod { set; get; } = RecognitionMethod.Ip;
    }

    public enum RecognitionMethod
    {
        Ip,

    }
}
