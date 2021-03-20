using kyun.OsuUtils;
using Ookii.Dialogs.WinForms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace mapper
{
    public partial class MainForm : Form
    {

        OsuBeatMap osuBeatMap { get; set; }
        NFNOpts NOpts = new NFNOpts();
        AltPlayerOpts aOpts = new AltPlayerOpts();
        public MainForm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox3.Text))
            {
                MessageBox.Show("Field Name Required");
                return;
            }

            string name = Regex.Replace(textBox3.Text, "[^A-Za-z]", "");
            name = name.Trim().Replace(" ", "-").ToLower();

            button1.Text = "Working...";
            panel2.Visible = true;
            Text = "osu! to FNF - Making conversions, please wait...";
            button1.Enabled = false;
            progressBar1.Value = 0;
            MessageBox.Show("Conversion will start now...");

            var osc = new OsuConverter(textBox1.Text, textBox2.Text, this);
            NOpts.dropfiles = checkBox2.Checked;

            var result = osc.Convert(name, checkBox1.Checked, (int)numericUpDown2.Value, aOpts, NOpts);

            if (result)
            {
                button1.Text = "Convert";
                Text = "osu! to FNF";
                button1.Enabled = true;
                MessageBox.Show("Conversion success", "Sonic says...", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                panel2.Visible = false;
            }
            else
            {
                button1.Text = "Convert";
                Text = "osu! to FNF";
                button1.Enabled = true;
                panel2.Visible = false;
                MessageBox.Show("Conversion error", "Sonic says...", MessageBoxButtons.OK ,MessageBoxIcon.Error);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            var dfd = new VistaFolderBrowserDialog();

            dfd.ShowDialog();

            if (string.IsNullOrWhiteSpace(dfd.SelectedPath))
            {
                button1.Enabled = false;
                MessageBox.Show("Nothing selected.");
                return;
            }

            textBox2.Text = dfd.SelectedPath;

            if (!string.IsNullOrWhiteSpace(textBox1.Text))
            {
                button1.Enabled = true;
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            Process.Start("https://github.com/NaokiStark/osu-to-FNF/blob/main/README.md");
        }

        private void button2_Click(object sender, EventArgs e)
        {

            var ofd = new OpenFileDialog();
            ofd.Filter = "osu beatmap|*.osu";
            ofd.InitialDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "osu!", "Songs");
            ofd.ShowDialog();
            string osubm = ofd.FileName;

            if (osubm.Length < 1)
            {
                MessageBox.Show("Nothing selected.");
                return;
            }

            textBox1.Text = osubm;

            try
            {
                osuBeatMap = OsuBeatMap.FromFile(osubm, true);
                button4.Enabled = true;
                button7.Enabled = true;
                if (!string.IsNullOrWhiteSpace(textBox2.Text))
                {
                    button1.Enabled = true;
                }
            }
            catch
            {
                MessageBox.Show("Beatmap could not be read.");
                button4.Enabled = false;
                button1.Enabled = false;
                textBox1.Text = "";
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (osuBeatMap == null)
            {
                return;
            }

            var text = $"Song: {osuBeatMap.Artist} - {osuBeatMap.Title}\r\n";
            text += $"Diff: {osuBeatMap.Version}\r\n";
            text += $"Bm creator: {osuBeatMap.Creator}\r\n";
            text += $"Objects count: {osuBeatMap.HitObjects.Count}\r\n";

            MessageBox.Show(text, "Beatmap Info");
        }

        private void toggleRadios(RadioButton radio)
        {
            if (radio.Name == "radioButton1")
            {
                aOpts.type = 0;
                numericUpDown3.Enabled = label9.Enabled = comboBox3.Enabled = false;

            }
            else if (radio.Name == "radioButton2")
            {
                aOpts.type = 1;
                aOpts.maxNotesPerPlayer = (int)numericUpDown3.Value;
                numericUpDown3.Enabled = label9.Enabled = true;
                comboBox3.Enabled = false;
            }
            else if (radio.Name == "radioButton3")
            {
                aOpts.type = 2;
                aOpts.selectedPlayer = (comboBox3.Text == "Enemy") ? 0 : 1;
                numericUpDown3.Enabled = label9.Enabled = false;
                comboBox3.Enabled = true;
            }
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            toggleRadios(sender as RadioButton);
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            toggleRadios(sender as RadioButton);
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            toggleRadios(sender as RadioButton);
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox ch = sender as CheckBox;

            label7.Enabled = label8.Enabled = numericUpDown2.Enabled = ch.Checked;
        }

        private void numericUpDown3_ValueChanged(object sender, EventArgs e)
        {
            aOpts.maxNotesPerPlayer = (int)(sender as NumericUpDown).Value;
        }

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            aOpts.selectedPlayer = (comboBox3.Text == "Enemy") ? 0 : 1;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            NOpts.enemy = comboBox1.Text;
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            NOpts.player = comboBox2.Text;
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            NOpts.speed = (int)numericUpDown1.Value;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            ProcessStartInfo processStartInfo = new ProcessStartInfo
            {
                UseShellExecute = true,
                FileName = Path.Combine(textBox2.Text, "Funkin.exe"),
                WorkingDirectory = textBox2.Text,
            };
            Process.Start(processStartInfo);
        }

        private void button7_Click(object sender, EventArgs e)
        {
            Process.Start((new FileInfo(textBox1.Text)).DirectoryName);
        }

        private void MainForm_Load(object sender, EventArgs e)
        {

        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if((sender as CheckBox).Checked)
            {
                MessageBox.Show("This option is only for 'newer' mods, file order is different than version 5, you need to replace this files by you own\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n click ok", "Sonic says...", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }
    }
}
