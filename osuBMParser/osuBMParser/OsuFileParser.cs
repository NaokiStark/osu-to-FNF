using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace osuBMParser
{
    public class OsuFileParser
    {
        #region fields
        private Beatmap beatmap;
        private string path;
        private TimingPoint actualTimmingPoint;

        private bool ParseObj { get; set; }

        #endregion

        #region constructors
        internal OsuFileParser(string path, Beatmap beatmap, bool parseObj = false)
        {
            this.path = path;
            this.beatmap = beatmap;
            ParseObj = parseObj;
        }
        #endregion

        #region methods

        internal void parse()
        {
            
            string[] lines;
            try
            {
                //lines = File.ReadAllLines(path);
                using (StreamReader srtF = new StreamReader(path, System.Text.Encoding.UTF8))
                {
                    string filde = srtF.ReadToEnd();
                    //Bug
                    //lines = filde.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
                    lines = Regex.Split(filde, "[\n]", RegexOptions.Compiled);
                }

            }
            catch (IOException)
            {
                throw;
            }

            foreach (string line in lines)
            {
                if (!ParseObj && ActualSection == "[hitobjects]")
                    return;

                //Skip line if empty
                if (!string.IsNullOrWhiteSpace(line))
                {
                    WHO_WANTS_SHIT(Regex.Replace(line, "\r", "", RegexOptions.Compiled));
                }
            }
        }

        string ActualSection = "version";
        void WHO_WANTS_SHIT(string line)
        {
            if (line.StartsWith("["))
            {
                ActualSection = line.ToLower();
                return;
            }
            else if (line.StartsWith("osu file"))
            {
                ActualSection = "osu";
            }

            switch (ActualSection)
            {
                case "osu":
                case "[general]":
                    normalParse(line);
                    break;
                case "[metadata]":
                    normalParse(line);
                    break;
                case "[difficulty]":
                    normalParse(line);
                    break;
                case "[events]":
                    parseBG(line);
                    parseVideo(line);
                    parseBreak(line);
                    break;
                case "[timingpoints]":
                    timingPointParse(line);
                    break;
                case "[colours]":
                    break;
                case "[hitobjects]":

                    hitObjectParse(line);
                    break;
                case "[editor]":

                    break;
                default:

                    break;
            }
        }

        void parseBG(string data)
        {
            string[] gd = data.Trim().Split(',');

            if (gd.Length < 3)
                return;

            if (gd[0] != "0")
                return;

            string bgParsed = Regex.Replace(gd[2], "\"", "");
            if (bgParsed.ToLower().EndsWith(".jpg") || bgParsed.ToLower().EndsWith(".png"))
            {
                beatmap.Background = bgParsed;
            }
        }

        void parseVideo(string data)
        {
            string[] gd = data.Trim().Split(',');

            if (gd.Length < 3)
                return;

            if (gd[0] != "Video")
                return;

            string bgParsed = Regex.Replace(gd[2], "\"", "");
            int videoStrtUp = toInt(gd[1]);

            beatmap.Video = bgParsed;
            beatmap.VideoStartUp = videoStrtUp;

        }

        void parseBreak(string data)
        {
            string[] gd = data.Trim().Split(',');

            if (gd.Length < 3)
                return;

            if (gd[0] != "2")
                return;

            beatmap.Breaks.Add(new Break()
            {
                Start = toInt(gd[1]),
                End = toInt(gd[2])
            });

        }

        #region parseMethods
        private void normalParse(string data)
        {
            if (data.ToLower().StartsWith("osu file"))
            {
                Regex rg = new Regex(@"osu file format v(\d+)", RegexOptions.IgnoreCase);
                Match mtc = rg.Match(data);
                if (mtc.Groups[1].Value != null)
                {
                    beatmap.FormatVersion = mtc.Groups[1].Value;
                }
                return;
            }
            string[] tokens = data.Split(':');
            tokens[1] = tokens[1].Trim();
            switch (tokens[0].ToLower().Trim())
            {
                case "audiofilename":
                    beatmap.AudioFileName = tokens[1];
                    break;
                case "mode":
                    beatmap.Mode = toInt(tokens[1]);
                    break;
                case "audioleadin":
                    beatmap.AudioLeadIn = toInt(tokens[1]);
                    break;
                case "title":
                    beatmap.Title = tokens[1];
                    break;
                case "artist":
                    beatmap.Artist = tokens[1];
                    break;
                case "creator":
                    beatmap.Creator = tokens[1];
                    break;
                case "version":
                    beatmap.Version = tokens[1];
                    break;
                case "hpdrainrate":
                    beatmap.HpDrainRate = SafeParse(tokens[1]);
                    break;
                case "overalldifficulty":
                    beatmap.OverallDifficulty = SafeParse(tokens[1]);
                    break;
                case "approachrate":
                    beatmap.ApproachRate = SafeParse(tokens[1]);
                    break;
                case "slidermultiplier":
                    beatmap.SliderMultiplier = SafeParse(tokens[1]);
                    break;
                case "slidertickrate":
                    beatmap.SliderTickRate = SafeParse(tokens[1]);
                    break;
                case "source":
                    beatmap.Source = tokens[1];
                    break;
                case "circlesize":
                    beatmap.CircleSize = SafeParse(tokens[1]);
                    break;
                case "tags":
                    List<string> tnks = tokens[1].Split(' ').ToList<string>();
                    tnks.ForEach(x => x = x.ToLower());
                    beatmap.Tags = tnks;
                    break;

            }

        }
        private void timingPointParse(string data)
        {

            string[] tokens = data.Split(',');


            TimingPoint timingPoint = new TimingPoint();

            if (tokens[0] != null) timingPoint.Offset = toInt(tokens[0]);


            if (tokens[1] != null) timingPoint.MsPerBeat = toFloat(tokens[1]);

            if(tokens.Length > 2)
            {
                if (tokens[2] != null) timingPoint.Meter = toInt(tokens[2]);
                if (tokens[3] != null) timingPoint.SampleType = toInt(tokens[3]);
                if (tokens[4] != null) timingPoint.SampleSet = toInt(tokens[4]);
                if (tokens[5] != null) timingPoint.Volume = toInt(tokens[5]);

                if (tokens.Length > 6)
                {
                    if (tokens[6] != null) timingPoint.Inherited = toBool(tokens[6]);
                }
                else
                {
                    timingPoint.Inherited = (timingPoint.MsPerBeat > 0)? true : false;
                }
                
                //if (tokens[6] != null) timingPoint.Inherited = (timingPoint.MsPerBeat > 0);
                if(tokens.Length > 7)
                {
                    if (tokens[7] != null) timingPoint.KiaiMode = toBool(tokens[7]);
                }
                else
                {
                    timingPoint.KiaiMode = false;
                }
               
            }

            beatmap.TimingPoints.Add(timingPoint);

        }

        private void colourParse(string data)
        {
            string[] tokens = data.Split(':');
            if (tokens.Length >= 2)
            {
                string[] colourValues = tokens[1].Split(',');
                if (colourValues.Length >= 3)
                {
                    beatmap.Colours.Add(new ComboColour(byte.Parse(colourValues[0]), byte.Parse(colourValues[1]), byte.Parse(colourValues[2])));
                }
            }
        }

        private void hitObjectParse(string data)
        {

            string[] tokens = data.Split(',');

            bool isManiaLongNote = false;

            if (tokens.Length < 5)
            {
                ///Debug.WriteLine("osuBMParser: Invalid HitObject line, no further information available");
                return; //Not possible to have less arguments than this
            }

            //Create bit array for checking type
            BitArray typeBitArray = new BitArray(new int[] { toInt(tokens[3]) });
            bool[] typeBits = new bool[typeBitArray.Count];
            typeBitArray.CopyTo(typeBits, 0);

            //Create hitObject of correct type
            HitObject hitObject = null;

            if (typeBits[0])
            {
                hitObject = new HitCircle();
            }
            else if (typeBits[1])
            {
                hitObject = new HitSlider();
            }
            else if (typeBits[3])
            {
                hitObject = new HitSpinner();
            }
            else if(typeBits[7])
            {
                hitObject = new HitSlider();
                isManiaLongNote = true;
            }
            else
            {
                return;
            }

                //Debug.WriteLine("osuBMParser: Invalid HitObject line at timestamp: " + tokens[2] + " | Type = " + tokens[3]);
                //return; //This type does not exist
            

            //mania


            //Parse all information for the hitObject

            //Global stuff first
            hitObject.Position = new Vector2(toFloat(tokens[0]), toFloat(tokens[1]));
            hitObject.Time = toInt(tokens[2]);
            hitObject.HitSound = toInt(tokens[4]);
            hitObject.IsNewCombo = typeBits[2];

            //Specific stuff

            actualTimmingPoint = getActualTimingPoint(hitObject.Time);

            if (hitObject is HitCircle)
            {

                if (tokens.Length >= 6 && tokens[5] != null) //Additions
                {
                    //hitObject.Addition = new List<int>(getAdditionsAsIntArray(tokens[5]));
                }

            }

            if (hitObject is HitSlider)
            {


                ((HitSlider)hitObject).SliderTimingPoint = actualTimmingPoint;
                if (tokens.Length >= 6 && tokens[5] != null) //SliderType and HitSliderSegments
                {
                    string[] hitSliderSegments = tokens[5].Split('|');
                    ((HitSlider)hitObject).Type = HitSlider.parseSliderType(hitSliderSegments[0]);

                    //MANIA LONG NOTE
                    if (isManiaLongNote) {
                        ((HitSlider)hitObject).Type = HitSlider.SliderType.LONGNOTE;

                        int endtime = toInt(tokens[5].Split(':')[0].Trim());
                        //Console.WriteLine(tokens[5].Split(':')[0].Trim());
                        
                        ((HitSlider)hitObject).EndTime = endtime;
                    }
                    

                    foreach (string hitSliderSegmentPosition in hitSliderSegments.Skip(1))
                    {
                        string[] positionTokens = hitSliderSegmentPosition.Split(':');
                        if (positionTokens.Length == 2)
                        {
                            ((HitSlider)hitObject).HitSliderSegments.Add(new HitSliderSegment(new Vector2(toFloat(positionTokens[0]), toFloat(positionTokens[1]))));
                        }
                    }
                }

                if (tokens.Length >= 7 && tokens[6] != null)
                {
                    ((HitSlider)hitObject).Repeat = toInt(tokens[6]);
                }

                if (tokens.Length >= 8 && tokens[7] != null)
                {
                    if(tokens[7].Length > 6)
                    {
                        ((HitSlider)hitObject).PixelLength = toFloat(tokens[7].Remove(6, tokens[7].Length-6));
                    }
                    else
                    {
                        ((HitSlider)hitObject).PixelLength = toFloat(tokens[7]);
                    }
                }

                if (tokens.Length >= 9 && tokens[8] != null)
                {
                    //((HitSlider)hitObject).EdgeHitSound = toInt(tokens[8]);
                }

                if (tokens.Length >= 10 && tokens[9] != null)
                {
                    //((HitSlider)hitObject).EdgeAddition = new List<int>(getAdditionsAsIntArray(tokens[9]));
                }

                if (tokens.Length >= 11 && tokens[10] != null)
                {
                    //hitObject.Addition = new List<int>(getAdditionsAsIntArray(tokens[10]));
                }

            }

            if (hitObject is HitSpinner)
            {

                if (tokens.Length >= 6 && tokens[5] != null)
                {
                    ((HitSpinner)hitObject).EndTime = toInt(tokens[5]);
                }

                if (tokens.Length >= 7 && tokens[6] != null)
                {
                    //hitObject.Addition = new List<int>(getAdditionsAsIntArray(tokens[6]));
                }

            }

            beatmap.HitObjects.Add(hitObject);

        }

        private TimingPoint getActualTimingPoint(int time)
        {
            for (var i = beatmap.TimingPoints.Count - 1; i >= 0; i--)
            {
                if (beatmap.TimingPoints[i].Offset <= time && !beatmap.TimingPoints[i].Inherited) { return beatmap.TimingPoints[i]; }
            }
            return beatmap.TimingPoints[0];
        }

        private int[] getAdditionsAsIntArray(string additionToken)
        {

            int[] additions = new int[0];
            try
            {
                additions = Array.ConvertAll(additionToken.Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries), int.Parse);
            }
            catch
            {
                throw;
            }
            return additions;

        }
        #endregion

        private int toInt(string data)
        {
            int result;
            return int.TryParse(data, NumberStyles.Integer, CultureInfo.InvariantCulture, out result) ? result : 0;
        }

        private float toFloat(string data)
        {
            float n = 0f;
            if (data.Contains("1E+"))
            {
                return 1;
            }

            try
            {
                n = float.Parse(data, NumberFormatInfo.InvariantInfo);
            }
            catch
            {
                
            }
            return n;
        }

        private bool toBool(string data)
        {
            return (data.Trim() == "1" || data.Trim().ToLower() == "true");
        }
        #endregion
        float SafeParse(string input)
        {
            if (String.IsNullOrEmpty(input)) { return 0f; } // old throw

            float res;
            if (Single.TryParse(input, NumberStyles.Float, CultureInfo.InvariantCulture, out res))
            {
                return res;
            }

            return 0.0f; // Or perhaps throw your own exception type
        }

    }
}

