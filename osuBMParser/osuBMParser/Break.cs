using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace osuBMParser
{
    public class Break
    {
        public long Start { get; set; }
        public long End { get; set; }
        public long Length { get { return End - Start; } }
    }
}
