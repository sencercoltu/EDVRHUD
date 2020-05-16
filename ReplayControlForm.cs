using LiteDB;
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
using System.Web.Script.Serialization;
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
            ReplayRate = (1000 - tbRate.Value) + 1;
            lblRate.Text = tbRate.Value.ToString();
        }

        private Thread ReplayThread = null;
        private bool ReplayRunning = false;
        private int ReplayRate = 1000;
        private bool ReplayBreakOnFSDJump = false;
        private bool ReplayBreak = false;

        private JavaScriptSerializer Serializer = new JavaScriptSerializer();


        private void btnStartStop_Click(object sender, EventArgs e)
        {
            if (ReplayThread == null)
            {                
                btnStartStop.Text = "Stop";
                dtpStartDate.Enabled = false;
                dtpStartTime.Enabled = false;
                ReplayThread = new Thread(() =>
                {
                    while (ReplayRunning)
                    {
                        if (!ThreadTick())
                            break;
                        Thread.Sleep(ReplayRate);
                    }

                    Invoke((Action)(() =>
                    {                        
                        btnStartStop.Text = "Start";
                        lblTimestamp.BackColor = Form.DefaultBackColor;
                        dtpStartDate.Enabled = true;
                        dtpStartTime.Enabled = true;                        
                    }));

                    ReplayRunning = false;
                    ReplayThread = null;

                })
                { IsBackground = true };
                ReplayRunning = true;
                lblTimestamp.BackColor = Color.Green;
                ReplayThread.Start();
            }
            else
            {
                ReplayRunning = false;                                      
            }
        }

        //private List<string> AllLines = new List<string>();        

        private IList<Dictionary<string, object>> ReplayEntries = new List<Dictionary<string, object>>();


        private DateTime SimTime;        
        private bool ThreadTick()
        {
            if (!ReplayEntries.Any())
            {
                SimTime = dtpStartDate.Value.Date;
                SimTime.Add(dtpStartTime.Value.TimeOfDay);
                var simEndTime = SimTime.Date.AddDays(1).AddSeconds(-1);
                var s = new BsonValue(SimTime.ToString("yyyy-MM-ddTHH:mm:ssZ"));
                var e = new BsonValue(simEndTime.ToString("yyyy-MM-ddTHH:mm:ssZ"));
                ReplayEntries = EDCommon.DBJournal.Find(LiteDB.Query.Between("timestamp", s, e)).ToList();
                NotificationApp.LastJournalTimeStamp = DateTime.MinValue;
                ReplayBreak = false;                
            }

            if (!ReplayEntries.Any())
            {
                return false;
            }


            //get line and 
            var entry = ReplayEntries.First();


            var ts = entry.GetProperty("timestamp", "");

            if (!DateTime.TryParse(ts.ToString(), out var tsdt))
            {
                ReplayEntries.RemoveAt(0);
                return true;
            }
            tsdt = tsdt.ToUniversalTime();

            Invoke((Action)(() => { lblTimestamp.Text = ts + "/" + ReplayEntries.Count; }));

            entry.Remove("_id");
            entry.Remove("SimTime");
            entry["IsReplay"] = true;


            var currLine = Serializer.Serialize(entry);

            if (!ReplayBreak && ReplayBreakOnFSDJump && currLine.Contains("\"event\":\"FSDJump\","))
            {
                ReplayBreak = true;
                return false;
            }
            else
                ReplayBreak = false;

            //check timestamp

            ReplayEntries.RemoveAt(0);

            lock (NotificationApp.ContentLock)
                NotificationApp.CachedContent += currLine + Environment.NewLine;

            return true;
        }

        private void chkAutoPause_CheckedChanged(object sender, EventArgs e)
        {
            ReplayBreakOnFSDJump = chkAutoPause.Checked;
        }

        private void dtpStartDate_ValueChanged(object sender, EventArgs e)
        {
            ReplayEntries.Clear();
        }

        private void dtpStartTime_ValueChanged(object sender, EventArgs e)
        {
            ReplayEntries.Clear();
        }


        private void ReplayControlForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            ReplayRunning = false;
            if (ReplayThread != null)
            {
                ReplayThread.Abort();
                ReplayThread = null;
            }
        }
    }
}
