using System;
using System.Collections.Generic;
using System.Text;

namespace KnowYourLimits.Strategies
{
    public class BaseRateLimitConfiguration
    {
        public bool EnableHeaders { get; set; } = false;
        public string HeaderPrefix { get; set; } = "X-";
    }
}
