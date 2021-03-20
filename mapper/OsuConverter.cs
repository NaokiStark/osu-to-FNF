using kyun.Beatmap;
using kyun.game.Beatmap;
using kyun.OsuUtils;
using osuBMParser;
using Parser;
using Parser.MapElements;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Writer;
using Vector2 = System.Numerics.Vector2;
namespace mapper
{
    public class OsuConverter
    {
        //Test 
        public int steps = 16;
        public int maxNotePerStep = 4;

        OsuBeatMap bmp { get; set; }
        MainForm i;
        string _toSave { get; set; }
        public OsuConverter(string beatmap, string toSave, MainForm instance)
        {
            i = instance;

            bmp = OsuBeatMap.FromFile(beatmap, true);
            _toSave = toSave;

        }

        public bool Convert(string name, bool optHitsounds, int db, AltPlayerOpts playerOpts, NFNOpts opts)
        {
            i.label11.Text = "Parsing osu beatmap";
            i.progressBar1.Value = 10;

            name = FirstCharToUpper(name);
            try
            {
                uint seed = uint.Parse(DateTime.Now.Year.ToString() + DateTime.Now.Day.ToString() + DateTime.Now.Second.ToString());

                Troschuetz.Random.Generators.NR3Q2Generator rnd = new Troschuetz.Random.Generators.NR3Q2Generator(seed);

                // Load custom sampleset
                var customHitsounds = new Dictionary<int, string>();
                var customSampleSet = CustomSampleSet.LoadFromBeatmap((new FileInfo(bmp.SongPath)).DirectoryName);

                maxNotePerStep = playerOpts.maxNotesPerPlayer;

                Map map = new Map
                {
                    Song = name,
                    Speed = opts.speed,
                    player1 = opts.enemy,
                    player2 = opts.player,
                    Notes = new List<Note>()
                };

                bool alter = false;

                int actualStep = 0;
                int actualNotes = 0;

                Note note = new Note
                {
                    lengthInSteps = steps,
                    bpm = (int)decimal.Round((decimal)1000 / (decimal)bmp.BPM * (decimal)60),
                    changeBPM = false,
                    mustHitSection = (playerOpts.type == 1) ? rnd.NextBoolean() : (playerOpts.type == 2) ? (playerOpts.selectedPlayer == 1) : alter,
                    typeOfSection = 0,
                    sectionNotes = new SectionNote(),
                };

                foreach (IHitObj hobj in bmp.HitObjects)
                {
                    if (actualNotes >= maxNotePerStep && playerOpts.type == 1)
                    {
                        actualNotes = 0;
                        map.Notes.Add(note);
                        note = new Note
                        {
                            lengthInSteps = steps,
                            bpm = (int)decimal.Round((decimal)1000 / (decimal)bmp.BPM * (decimal)60),
                            changeBPM = false,
                            mustHitSection = rnd.NextBoolean(),
                            typeOfSection = 0,
                            sectionNotes = new SectionNote(),
                        };
                    }
                    else if (playerOpts.type == 0 && hobj.isNewCombo)
                    {
                        alter = !alter;
                        actualNotes = 0;
                        map.Notes.Add(note);
                        note = new Note
                        {
                            lengthInSteps = steps,
                            bpm = (int)decimal.Round((decimal)1000 / (decimal)bmp.BPM * (decimal)60),
                            changeBPM = false,
                            mustHitSection = alter,
                            typeOfSection = 0,
                            sectionNotes = new SectionNote(),
                        };
                    }
                    else if (actualNotes >= maxNotePerStep && playerOpts.type == 2)
                    {
                        actualNotes = 0;
                        map.Notes.Add(note);
                        note = new Note
                        {
                            lengthInSteps = steps,
                            bpm = (int)decimal.Round((decimal)1000 / (decimal)bmp.BPM * (decimal)60),
                            changeBPM = false,
                            mustHitSection = (playerOpts.selectedPlayer == 1),
                            typeOfSection = 0,
                            sectionNotes = new SectionNote(),
                        };

                    }
                    actualNotes++;

                    if (hobj is HitButton)
                    {

                        HitButton hb = (HitButton)hobj;
                        int colPos = getPositionInCol(hb.OsuLocation);
                        decimal startTime = hb.StartTime;
                        decimal length = 0;
                        note.sectionNotes.rNotes.Add(new RNote
                        {
                            startTime = startTime,
                            length = length,
                            position = colPos,
                        });

                        int hitsound = hb.HitSound;
                        TimingPoint tm = bmp.GetTimingPointFor((long)startTime);
                        string sample = customSampleSet.GetSample(tm.SampleType, hitsound, tm.SampleSet);

                        if (!string.IsNullOrWhiteSpace(sample))
                        {
                            try
                            {
                                customHitsounds.Add((int)startTime, (new FileInfo(sample)).Name);
                            }
                            catch
                            {
                                int itr = 1;
                                while (customHitsounds.ContainsKey((int)startTime + itr))
                                {
                                    itr++;
                                }
                                customHitsounds.Add((int)startTime + itr, (new FileInfo(sample)).Name);
                            }
                        }
                    }
                    else
                    {
                        HitHolder hh = (HitHolder)hobj;
                        int colPos = getPositionInCol(hh.OsuLocation);
                        decimal startTime = hh.StartTime;
                        decimal length = hh.Length;
                        note.sectionNotes.rNotes.Add(new RNote
                        {
                            startTime = startTime,
                            length = length,
                            position = colPos,
                        });

                        int hitsound = hh.HitSound;
                        TimingPoint tm = bmp.GetTimingPointFor((long)startTime);
                        TimingPoint tmEnd = bmp.GetTimingPointFor((long)hh.EndTime);

                        string sample = customSampleSet.GetSample(tm.SampleType, hitsound, tm.SampleSet);

                        if (!string.IsNullOrWhiteSpace(sample))
                        {
                            try
                            {
                                customHitsounds.Add((int)startTime, (new FileInfo(sample)).Name);
                            }
                            catch
                            {
                                int itr = 1;
                                while (customHitsounds.ContainsKey((int)startTime + itr))
                                {
                                    itr++;
                                }
                                customHitsounds.Add((int)startTime + itr, (new FileInfo(sample)).Name);
                            }
                        }

                        sample = customSampleSet.GetSample(tmEnd.SampleType, hitsound, tmEnd.SampleSet);

                        if (!string.IsNullOrWhiteSpace(sample))
                        {
                            try
                            {
                                customHitsounds.Add((int)hh.EndTime, (new FileInfo(sample)).Name);
                            }
                            catch
                            {
                                int itr = 1;
                                while (customHitsounds.ContainsKey((int)hh.EndTime + itr))
                                {
                                    itr++;
                                }
                                customHitsounds.Add((int)hh.EndTime + itr, (new FileInfo(sample)).Name);
                            }
                        }
                    }

                }

                map.needsVoices = (customHitsounds.Count > 0) ? optHitsounds : false;
                i.label11.Text = "Saving FNF chart";
                i.progressBar1.Value = 40;

                NFNWriter wr = new NFNWriter(map);

                string oldsave = _toSave;
                _toSave = Path.Combine(_toSave, "assets");

                string pathMusic = Path.Combine(_toSave, "music");
                string pathPatternFolder = Path.Combine(_toSave, "data", name.ToLower());
                string pathDataFolder = Path.Combine(_toSave, "data");
                if (opts.dropfiles)
                {
                    _toSave = Path.Combine(oldsave, $"{name}_" + rnd.NextUInt());
                    if (!Directory.Exists(_toSave))
                    {
                        Directory.CreateDirectory(_toSave);
                    }
                    pathMusic =
                    pathDataFolder =
                    pathPatternFolder = _toSave;

                }


                if (!Directory.Exists(pathPatternFolder))
                {
                    Directory.CreateDirectory(pathPatternFolder);
                }

                string pathPatterns = Path.Combine(_toSave, "data", name, $"{name.ToLower()}.json");
                if (opts.dropfiles) { 
                    pathPatterns = Path.Combine(_toSave, $"{name.ToLower()}.json"); 
                }

                wr.Write(pathPatterns);

                i.label11.Text = "Saving audio file to ogg";
                i.progressBar1.Value = 60;
                string newfile = $"{name}_Inst.ogg";
                string voicesfile = $"{name}_Voices.ogg";
                if (opts.dropfiles)
                {
                    newfile = "Inst.ogg";
                    voicesfile = "Voices.ogg";
                }

                convertAudio(bmp.SongPath, Path.Combine(pathMusic, newfile));
                //WriteFreePlayFile(pathDataFolder, name);

                if (optHitsounds && customHitsounds.Count > 0)
                {
                    i.label11.Text = "Hitsounds -> Voice.ogg, this'll take some time";
                    i.progressBar1.Value = 80;
                    mergeHitsounds(Path.Combine(pathMusic, newfile), customHitsounds, Path.Combine(pathMusic, voicesfile), (new FileInfo(bmp.SongPath)).DirectoryName, db);
                }
            }
            catch
            {
                return false;
            }

            return true;
        }


        public void mergeHitsounds(string audiofile, Dictionary<int, string> customHitsounds, string result, string workingdir, int vol)
        {
            if (customHitsounds.Count < 1)
            {
                return;
            }
            HitSoundsMerger.MergeAudio(audiofile, customHitsounds, result, workingdir, vol, i);
        }

        public void convertAudio(string input, string output)
        {
            ProcessStartInfo st = new ProcessStartInfo()
            {
                Arguments = $"-i \"{input}\" -c:a libvorbis -q:a 4 \"{output}\" -y",
                CreateNoWindow = true,
                FileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FFmpeg", "ffmpeg.exe"),
            };

            var proc = Process.Start(st);
            proc.WaitForExit();
        }

        //Useless because HaxeFlixel

        private void WriteFreePlayFile(string dir, string name)
        {
            using (StreamWriter sr = File.AppendText(Path.Combine(dir, "freeplaySonglist.txt")))
            {
                sr.WriteLine(name);
            }
        }

        public static string FirstCharToUpper(string input)
        {
            if (String.IsNullOrEmpty(input))
                throw new ArgumentException("ARGH!");
            return input.First().ToString().ToUpper() + String.Join("", input.Skip(1));
        }
        public int getPositionInCol(Vector2 osuloc)
        {
            //384 = max value of Y in osu!

            int row = 384 / 4;

            int positionInCol = (int)Math.Floor((osuloc.X / (float)row));

            positionInCol = Math.Min(4, positionInCol);
            positionInCol = Math.Max(1, positionInCol);

            return positionInCol;
        }
    }

    public class AltPlayerOpts
    {
        public int type = 0;
        public int maxNotesPerPlayer = 4;
        public int selectedPlayer = 0; // 0 enemy, 1 player
    }

    public class NFNOpts
    {
        public string enemy = "gf";
        public string player = "bf";
        public int speed = 1;
        public bool dropfiles = false;
    }
}
