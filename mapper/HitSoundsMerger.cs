using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mapper
{
    public class HitSoundsMerger
    {
        public static void MergeAudio(string input, Dictionary<int, string> files, string result, string workingdir, int vol, MainForm i)
        {
            string ffmpegLoc = @"\\?\" + Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FFmpeg", "ffmpeg.exe");

            string args = $"-i \"{input}\" ";

            string filesInput = " ";

            string filtervariables = "[0]volume=0:precision=fixed[s0];";

            int amix = 1 + files.Count;

            int varCount = 1;
            string filterintVar = "[s0]";

            List<string> inputfiles = new List<string>();

            
            foreach (KeyValuePair<int, string> pair in files)
            {
                if (inputfiles.Contains(pair.Value))
                {
                    int position = inputfiles.IndexOf(pair.Value) + 1;
                    filtervariables += $"[{position}]adelay={pair.Key}|{pair.Key}[s{varCount}];";
                    filterintVar += $"[s{varCount}]";
                }
                else
                {
                    inputfiles.Add(pair.Value);
                    filesInput += $"-i \"{pair.Value}\" ";
                    filtervariables += $"[{inputfiles.Count}]adelay={pair.Key}|{pair.Key}[s{varCount}];";
                    filterintVar += $"[s{varCount}]";
                }

                varCount++;

            }

            string filter_complex_script = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "f.txt");

            string volstr = ((float)vol / 100f).ToString("0.00", CultureInfo.InvariantCulture);
            using (StreamWriter sr = new StreamWriter(filter_complex_script))
            {
                sr.Write($"{filtervariables}{filterintVar}amix={amix},dynaudnorm=p={volstr}:m=100:s=12:g=15");
            }

            string finalArgs = $"{args}{filesInput} -filter_complex_script \"{filter_complex_script}\" -c:v -o \"{result}_low.ogg\" -y";

            ProcessStartInfo st = new ProcessStartInfo()
            {
                Arguments = finalArgs,
                CreateNoWindow = true,
                WorkingDirectory = workingdir,
                FileName = ffmpegLoc,
                RedirectStandardError = true,
                UseShellExecute = false,
            };

            var proc = new Process();
            proc.StartInfo = st;
            proc.ErrorDataReceived += (s, a) =>
            {
                Console.WriteLine(a.Data);
            };
            proc.Start();
            proc.BeginErrorReadLine();
            proc.WaitForExit();
            proc.CancelErrorRead();
            i.label11.Text = "Just a bit more";
            i.progressBar1.Value = 98;
            volume(result, vol);
        }

        public static void volume(string input, int volume)
        {
            string ffmpegLoc = @"\\?\" + Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FFmpeg", "ffmpeg.exe");
            string vol = ((float)volume / 100f).ToString("0.00", CultureInfo.InvariantCulture);
            ProcessStartInfo st = new ProcessStartInfo()
            {
                //Arguments = $"-i \"{input}_low.ogg\" -filter:a \"volume = {volume}dB\" {input} -y",
                Arguments = $"-i \"{input}_low.ogg\" -af \"dynaudnorm=p={vol}:m=100:s=12:g=15\" {input} -y",
                CreateNoWindow = true,
                FileName = ffmpegLoc,
            };

            var proc = Process.Start(st);
            proc.WaitForExit();
        }

    }
}
