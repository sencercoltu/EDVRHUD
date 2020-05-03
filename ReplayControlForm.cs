using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EDVRHUD
{
    public partial class ReplayControlForm : Form
    {
        public ReplayControlForm()
        {
            InitializeComponent();
        }

        private void tbRate_ValueChanged(object sender, EventArgs e)
        {
            ReplayRate = (1 + (100 - tbRate.Value)) * 10;
            lblRate.Text = "X" + tbRate.Value;
        }

        private List<string> Journals = new List<string>();

        private void btnFiles_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Multiselect = true;
                ofd.InitialDirectory = NotificationApp.EDJournalPath;
                ofd.Filter = "Elite Dangerous Journal|Journal.????????????.??.log";
                ofd.Title = "Please select one or more journals to replay";
                ofd.DefaultExt = ".log";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    Journals = ofd.FileNames.OrderBy(f => f).ToList();
                }
            }
            btnStartStop.Enabled = Journals.Count > 0;
        }

        private Thread ReplayThread = null;
        private bool ReplayRunning = false;
        private int ReplayRate = 1;
        private bool ReplayBreakOnFSDJump = false;
        private bool ReplayBreak = false;



        private void btnStartStop_Click(object sender, EventArgs e)
        {
            if (ReplayThread == null)
            {
                btnStartStop.Text = "Stop";
                btnFiles.Enabled = false;
                ReplayThread = new Thread(() =>
                {
                    while (ReplayRunning)
                    {
                        if (!ThreadTick())
                            break;
                        Thread.Sleep(ReplayRate);
                    }
                    ReplayRunning = false;

                    Invoke((Action)(() =>
                    {
                        btnStartStop.Text = "Start";
                        lblTimestamp.BackColor = Form.DefaultBackColor;
                        btnFiles.Enabled = true;
                        ReplayThread = null;
                    }));

                })
                { IsBackground = true };
                ReplayRunning = true;
                lblTimestamp.BackColor = Color.Green;
                ReplayThread.Start();
            }
            else
            {
                if (ReplayThread != null)
                {
                    ReplayRunning = false;
                    ReplayThread = null;                    
                }
            }
        }

        private List<string> AllLines = new List<string>();        

        private bool ThreadTick()
        {
            if (Journals.Count > 0)
            {
                AllLines.Clear();
                foreach (var journal in Journals)
                {
                    using (var fs = File.Open(journal, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        fs.Seek(0, SeekOrigin.Begin);
                        var len = fs.Length;
                        var bytes = new byte[len];
                        fs.Read(bytes, 0, bytes.Length);
                        fs.Close();
                        var strcontent = Encoding.UTF8.GetString(bytes);
                        var lines = strcontent.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                        AllLines.AddRange(lines);
                    }
                }
                Journals.Clear();
                NotificationApp.LastJournalTimeStamp = DateTime.MinValue;


            }

            if (AllLines.Count == 0)
            {
                return false;
            }

            //get line and 
            var currLine = AllLines[0];
            if (string.IsNullOrWhiteSpace(currLine))
            {
                AllLines.RemoveAt(0);
                return true;
            }

            if (!ReplayBreak && ReplayBreakOnFSDJump && currLine.Contains("\"event\":\"FSDJump\","))
            {
                ReplayBreak = true;
                return false;
            }
            else
                ReplayBreak = false;

            //check timestamp
            var tsstart = currLine.IndexOf("\"timestamp\":") + 13;
            var ts = currLine.Substring(tsstart, 20);
            Invoke((Action)(() => { lblTimestamp.Text = ts; }));

            AllLines.RemoveAt(0);

            lock (NotificationApp.ContentLock)
                NotificationApp.CachedContent += currLine + Environment.NewLine;

            return true;

            //EDJournalReplayThread = new Thread(() =>
            //{
            //    //var usedEvents = new[] { "StarClass", "JetConeBoost", "StartJump", "FSDTarget", "FSDJump", "DiscoveryScan", "FSSSignalDiscovered", "FSSDiscoveryScan", "FSSAllBodiesFound", "Scan" };

            //    lock (ContentLock)
            //        CachedContent = "";
            //    LastJournalTimeStamp = DateTime.MinValue;

            //    foreach (var filename in filenames)
            //    {
            //        if (!AppRunning)
            //            return;
            //        var content = File.ReadAllText(filename);
            //        var lines = content.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            //        foreach (var line in lines)
            //        {
            //            if (string.IsNullOrEmpty(line))
            //                continue;

            //            //if (!usedEvents.Any(e => line.Contains("\"" + e + "\"")))
            //            //    continue;
            //            if (Settings.BreakOnFSDJump)
            //            {
            //                if (line.Contains("\"event\":\"FSDJump\","))
            //                {
            //                    while (true)
            //                    {
            //                        if (!AppRunning)
            //                            return;
            //                        if (ReplayNextSystem)
            //                        {
            //                            ReplayNextSystem = false;
            //                            break;
            //                        }
            //                        Thread.Sleep(100);
            //                    }
            //                }
            //            }
            //            lock (ContentLock)
            //                CachedContent += line + Environment.NewLine;
            //            if (!AppRunning)
            //                break;

            //            Thread.Sleep((100 - Settings.JournalReplaySpeed + 1) * 10);
            //        }
            //    }
            //})
            //{ IsBackground = true };
            //EDJournalReplayThread.Start();
        }

        private void chkAutoPause_CheckedChanged(object sender, EventArgs e)
        {
            ReplayBreakOnFSDJump = chkAutoPause.Checked;
        }
    }
}
