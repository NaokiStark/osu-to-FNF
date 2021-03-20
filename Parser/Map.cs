using Parser.MapElements;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Parser
{
    public class Map
    {
        public string Song { get; set; }
        public int Speed { get; set; }

        public bool needsVoices { get; set; }
        public string player1 { get; set; }
        public string player2 { get; set; }

        public List<Note> Notes { get; set; }

        public async Task<Map> FromFile(string path)
        {
            string mapString = "";
            FileInfo fileInfo = new FileInfo(path);
            if (!fileInfo.Exists)
            {
                throw new FileNotFoundException("File not found");
            }

            using (StreamReader sr = new StreamReader(fileInfo.FullName))
            {
                mapString = await sr.ReadToEndAsync();
            }

            // clean null char
            mapString = mapString.Replace("\u0000", "").Replace("\0", "");

            JObject jo = JObject.Parse(mapString);

            var tmp = new Map();

            tmp.Song = (string)jo["song"];

            tmp.Notes = new List<Note>();

            JArray ja = (JArray)jo["notes"];

            // Add notes
            foreach (JObject notejo in ja)
            {
                var tmpNote = new Note();
                tmpNote.lengthInSteps = (decimal)notejo["lengthInSteps"];
                tmpNote.bpm = (int)notejo["bpm"];
                tmpNote.changeBPM = (bool)notejo["changeBPM"];
                tmpNote.mustHitSection = (bool)notejo["mustHitSelection"];
                tmpNote.typeOfSection = (int)notejo["typeOfSection"];
                tmpNote.sectionNotes = new SectionNote();
                JArray sectNotes = (JArray)notejo["sectionNotes"];

                foreach (JArray sectionNote in sectNotes)
                {
                    if (sectionNote.Count > 2)
                    {
                        tmpNote.sectionNotes.rNotes.Add(new RNote
                        {
                            startTime = (decimal)sectionNote[0],
                            position = (int)sectionNote[1],
                            length = (decimal)sectionNote[3],
                        });
                    }
                }
                  
                tmp.Notes.Add(tmpNote);
            }


            return tmp;
        }
    }
}
