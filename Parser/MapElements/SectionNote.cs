using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parser.MapElements
{
    public class SectionNote
    {
        public List<RNote> rNotes = new List<RNote>();
        public ENoteType type { get; set; }

        public void add(RNote rNote)
        {
            rNotes.Add(rNote);
        }
    }

    public class RNote
    {        
        public decimal startTime { get; set; }
        public int position { get; set; }
        public decimal length { get; set; }
    }
}
