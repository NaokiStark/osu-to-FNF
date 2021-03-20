using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parser.MapElements
{
    public class Note
    {
        public decimal lengthInSteps { get; set; }
        public int bpm { get; set; }
        public bool changeBPM { get; set; }
        public bool mustHitSection { get; set; }
        public SectionNote sectionNotes { get; set; }
        public int typeOfSection { get; set; }
    }
}
