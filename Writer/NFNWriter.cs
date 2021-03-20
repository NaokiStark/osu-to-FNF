using Newtonsoft.Json.Linq;
using Parser;
using Parser.MapElements;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Writer
{
    public class NFNWriter
    {
        private Map _map { get; set; }

        public NFNWriter(Map map)
        {
            _map = map;
        }

        public async void Write(string path)
        {

            JObject jonode = new JObject();

            JObject jo = new JObject();
            jo["song"] = _map.Song;

            JArray notes = new JArray();

            JObject noteo = new JObject();
            noteo["lengthInSteps"] = (int)16;
            noteo["bpm"] = (int)_map.Notes[0].bpm;
            noteo["changeBPM"] = _map.Notes[0].changeBPM;
            noteo["mustHitSection"] = _map.Notes[0].mustHitSection;
            noteo["typeOfSection"] = (int)_map.Notes[0].typeOfSection;

            JArray osectionNoteArr = new JArray();

            noteo["sectionNotes"] = osectionNoteArr;

            notes.Add(noteo);
            foreach (Note note in _map.Notes)
            {
                JObject noteJo = new JObject();
                noteJo["lengthInSteps"] = (int)note.lengthInSteps;
                noteJo["bpm"] = (int)note.bpm;
                noteJo["changeBPM"] = note.changeBPM;
                noteJo["mustHitSection"] = note.mustHitSection;
                noteJo["typeOfSection"] = (int)note.typeOfSection;

                JArray sectionNoteArr = new JArray();

                foreach (RNote rNote in note.sectionNotes.rNotes)
                {
                    JArray rnotes = new JArray();
                    rnotes.Add((int)rNote.startTime);
                    rnotes.Add((int)(rNote.position - 1));
                    string c = rNote.length.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture);
                    if (c == "0.0")
                    {
                        rnotes.Add((int)0);
                    }
                    else
                    {
                        rnotes.Add((int)rNote.length);
                    }
                    sectionNoteArr.Add(rnotes);
                }

                noteJo["sectionNotes"] = sectionNoteArr;

                notes.Add(noteJo);
            }

            jo["player1"] = _map.player2;
            jo["player2"] = _map.player1;
            jo["needsVoices"] = _map.needsVoices;
            jo["sectionLengths"] = new JArray();
            jo["sections"] = _map.Notes.Count;
            jo["notes"] = notes;
            jo["bpm"] = (int)_map.Notes[0].bpm;
            jo["speed"] = _map.Speed;


            jonode["song"] = jo;
            jonode["notes"] = notes;

            string json = jonode.ToString().Replace(" ", "");
            json = Regex.Replace(json, @"\r\n?|\n", "");

            byte[] realdata = UTF8Encoding.UTF8.GetBytes(json);
            //byte[] pad = new byte[(realdata.Length/2)-1];
            //for (int a = 0; a < pad.Length; a++)
            //{
            //    pad[a] = 0;
            //}


            using (BinaryWriter sw = new BinaryWriter(new StreamWriter(path).BaseStream))
            {
                sw.Write(realdata);
                //sw.Write(pad);
                //await sw.WriteAsync(json);
            }


        }
    }
}
