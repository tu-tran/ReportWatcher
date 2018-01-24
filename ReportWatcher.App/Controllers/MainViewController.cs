namespace ReportWatcher.WPF.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;

    using Data;
    using Data.Handlers;

    /// <summary>
    /// The <see cref="MainViewController" /> class.
    /// </summary>
    internal class MainViewController
    {
        /// <summary>
        /// The cache.
        /// </summary>
        private readonly Dictionary<DateTime, List<Report>> cache = new Dictionary<DateTime, List<Report>>();

        /// <summary>
        /// </summary>
        private readonly ICalendarSource db;

        /// <summary>
        /// The view
        /// </summary>
        private readonly IView view;

        /// <summary>Initializes a new instance of the <see cref="MainViewController" /> class.</summary>
        /// <param name="view">The view.</param>
        /// <param name="db">The database.</param>
        public MainViewController(IView view, ICalendarSource db)
        {
            this.view = view;
            this.db = db;
            view.RefreshRequested += () => ThreadPool.QueueUserWorkItem(o => this.DoQueryInBackground(null));
        }

        /// <summary>
        /// Gets the title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Queries this instance.
        /// </summary>
        /// <returns>The query result.</returns>
        public Status Query()
        {
            ThreadPool.QueueUserWorkItem(this.DoQueryInBackground);
            return Status.Success;
        }

        /// <summary>
        /// Does query in background.
        /// </summary>
        /// <param name="state">The state.</param>
        /// <exception cref="NotImplementedException"></exception>
        private void DoQueryInBackground(object state)
        {
            do
            {
                if (!Monitor.TryEnter(this))
                {
                    return;
                }

                try
                {
                    this.cache.Clear();
                    this.view.Clear();
                    var queue = new QueryQueue(this.GetType().Name, Environment.ProcessorCount);
                    var dates = Enumerable.Range(7, 30).Select(i => DateTime.Now.Date.AddDays(i)).ToList();

                    this.view.ShowProgress("Report Earning Calendar", "Querying...");
                    this.view.ShowProgress(0, dates.Count);
                    queue.Start(this.QueryCalendar, dates);

                    this.view.ShowProgress("Report Earning Calendar",
                        $"Retrieved earning reports between [{dates.First().ToShortDateString()} - {dates.Last().ToShortDateString()}]");
                }
                catch (Exception ex)
                {
                    var error = $"Failed to get calendar from {this.db.GetType().Name}: {ex}";
                    this.view.Notify(error);
                }
                finally
                {
                    Monitor.Exit(this);
                }

                this.cache.Clear();
                Thread.Sleep(Convert.ToInt32(TimeSpan.FromHours(1).TotalMilliseconds));
            } while (true);
        }

        /// <summary>
        /// Queries calendar.
        /// </summary>
        /// <param name="date">The date.</param>
        /// <param name="index">The index.</param>
        private void QueryCalendar(DateTime date, int index)
        {
            Trace.Write($"Querying data for {date}...");
            var result = this.db.GetCalendar(date);
            this.view.IncreaseProgress();
            if (result.Status != Status.Success || result.Data == null || result.Data.Count == 0)
            {
                return;
            }

            lock (this.cache)
            {
                List<Report> reportCache;
                if (!this.cache.TryGetValue(date, out reportCache))
                {
                    reportCache = new List<Report>();
                    this.cache[date] = reportCache;
                }

                var shouldAdd = true;
                foreach (var report in result.Data)
                {
                    foreach (var pair in this.cache)
                        if (pair.Value.Contains(report))
                        {
                            shouldAdd = false;
                            Trace.TraceError($"Duplicated data found for [{report.Name} ({report.Code})] in dataset of {pair.Key} and {date}");
                        }

                    if (shouldAdd)
                    {
                        reportCache.Add(report);
                        this.view.AddReport(report);
                    }
                }
            }
        }
    }
}