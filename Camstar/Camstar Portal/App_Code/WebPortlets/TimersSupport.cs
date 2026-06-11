// Copyright Siemens 2023  
using Camstar.WebPortal.FormsFramework.Utilities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using OM = Camstar.WCF.ObjectStack;


namespace Camstar.WebPortal.WebPortlets
{
    public class TimersSupport
    {
        public static void AdjustGridSnapData(DataTable dataWindowTable, TimeSpan offset, string[] fields)
        {
            if (dataWindowTable.Columns.Contains("TimersCount"))
            {
                foreach (var r in dataWindowTable.Rows.OfType<DataRow>())
                {
                    if (!(r["TimersCount"] is DBNull) && r["TimersCount"] is int)
                    {
                        // Correct times with timezone offset
                        r.BeginEdit();
                        foreach (var c in fields)
                        {
                            if (dataWindowTable.Columns.Contains(c))
							{
                                if (r[c] != null && !(r[c] is DBNull) && r[c] is DateTime)
                                {
                                    r[c] = ((DateTime)r[c]) - offset;
                                }
                            }
                        }
                        r.AcceptChanges();
                    }
                }
            }
        }

        public static bool GetContainerTimers(AjaxTransition transition)
        {
            var containerName = transition.CommandParameters;
            var tl = new List<Timer>();

            var prof = FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile;
            var svc = new WCF.Services.ContainerTxnService(prof);
            var data = new OM.ContainerTxn();
            data.Container = new OM.ContainerRef(containerName);
            var req = new WCF.Services.ContainerTxn_Request
            {
                Info = new OM.ContainerTxn_Info
                {
                    CurrentContainerStatus = new OM.CurrentContainerStatus_Info
                    {
                        Timers = new OM.Timer_Info { RequestValue = true }
                    }
                }
            };
            WCF.Services.ContainerTxn_Result res;
            var state = svc.Load(data, req, out res);
            if (state.IsSuccess)
            {
                if (res.Value.CurrentContainerStatus.Timers != null)
                {
                    foreach (var t in res.Value.CurrentContainerStatus.Timers)
                    {
                        tl.Add(new Timer(t, prof.UTCOffset));
                    }
                }
            }

            var d = Newtonsoft.Json.JsonConvert.SerializeObject(tl);

            transition.Response = new[] { new ResponseSection(ResponseType.Command, transition.ID, new HTMLData(d)) };
            return true;
        }

        public class Timer
        {
            public virtual OM.Primitive<string> ProcessTimerName { get; set; }
            public virtual OM.Primitive<string> ProcessTimerRevision { get; set; }
            public virtual OM.Primitive<DateTime> MinEndWarningTimeGMT { get; set; }
            public virtual OM.Primitive<string> MinWarningTimeColor { get; set; }
            public virtual OM.Primitive<DateTime> MinEndTimeGMT { get; set; }
            public virtual OM.Primitive<string> MinTimeColor { get; set; }
            public virtual OM.Primitive<DateTime> MaxEndWarningTimeGMT { get; set; }
            public virtual OM.Primitive<string> MaxWarningTimeColor { get; set; }
            public virtual OM.Primitive<DateTime> MaxEndTimeGMT { get; set; }
            public virtual OM.Primitive<string> MaxTimeColor { get; set; }

            public virtual int TimerNumber { get; set; }
            private DataRow row;
            private TimeSpan offset;

            public Timer()
            {
            }

            public Timer(string name, string rev)
            {
                ProcessTimerName = name;
                ProcessTimerRevision = rev;
            }

            public Timer(OM.Timer t, TimeSpan timeoffset)
            {
                offset = timeoffset;
                ProcessTimerName = getVal(t.ProcessTimerName);
                ProcessTimerRevision = getVal(t.ProcessTimerRevision);
                MinEndWarningTimeGMT = getVal(t.MinEndWarningTimeGMT);
                MinWarningTimeColor = getVal(t.MinWarningTimeColor);
                MinEndTimeGMT = getVal(t.MinEndTimeGMT);
                MinTimeColor = getVal(t.MinTimeColor);
                MaxEndWarningTimeGMT = getVal(t.MaxEndWarningTimeGMT);
                MaxWarningTimeColor = getVal(t.MaxWarningTimeColor);
                MaxEndTimeGMT = getVal(t.MaxEndTimeGMT);
                MaxTimeColor = getVal(t.MaxTimeColor);
            }

            public Timer(DataRow r, int pos)
            {
                row = r;
                TimerNumber = pos;

                ProcessTimerName = getItem("ProcessTimerName");
                ProcessTimerRevision = getItem("ProcessTimerRevision");
                MinEndWarningTimeGMT = getItemTime("MinEndWarningTimeGMT");
                MinWarningTimeColor = getItem("MinWarningTimeColor");
                MinEndTimeGMT = getItemTime("MinEndTimeGMT");
                MinTimeColor = getItem("MinTimeColor");
                MaxEndWarningTimeGMT = getItemTime("MaxEndWarningTimeGMT");
                MaxWarningTimeColor = getItem("MaxWarningTimeColor");
                MaxEndTimeGMT = getItemTime("MaxEndTimeGMT");
                MaxTimeColor = getItem("MaxTimeColor");
            }

            protected virtual string getItem(string colName)
            {
                return (row[colName] as string).Split(',')[TimerNumber];
            }
            protected virtual DateTime getItemTime(string colName)
            {
                var v = (row[colName] as string).Split(',')[TimerNumber];
                return DateTime.Parse(v);
            }
            protected virtual OM.Primitive<string> getVal(OM.Primitive<string> f)
            {
                return f;
            }
            protected virtual OM.Primitive<DateTime> getVal(WCF.ObjectStack.Primitive<DateTime> f)
            {
                return f;
            }
        }
    }
}
