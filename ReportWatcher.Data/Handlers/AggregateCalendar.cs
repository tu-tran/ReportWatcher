namespace ReportWatcher.Data.Handlers
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// The <see cref="AggregateCalendar" /> class.
    /// </summary>
    public class AggregateCalendar : CalendarSourceBase
    {
        /// <summary>
        /// The handlers.
        /// </summary>
        private static readonly List<ICalendarSource> Handlers;

        /// <summary>
        /// Initializes the <see cref="AggregateCalendar" /> class.
        /// </summary>
        static AggregateCalendar()
        {
            Handlers = new List<ICalendarSource>();
            var handlersPath = Path.Combine(Path.GetDirectoryName((Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly()).Location) ?? string.Empty, "Handlers");
            AppDomain.CurrentDomain.AppendPrivatePath(handlersPath);
            if (!Directory.Exists(handlersPath))
            {
                Trace.TraceError("Invalid handlers path: " + handlersPath);
                return;
            }

            var pluginFiles = Directory.GetFiles(handlersPath, "Handlers.*.dll");
            try
            {
                foreach (var file in pluginFiles)
                    try
                    {
                        var assembly = Assembly.LoadFrom(file);
                        var types = assembly.GetTypes().Where(t => typeof(ICalendarSource).IsAssignableFrom(t));
                        foreach (var type in types)
                            try
                            {
                                var db = (ICalendarSource) Activator.CreateInstance(type);
                                Handlers.Add(db);
                            }
                            catch (Exception ex)
                            {
                                Trace.TraceError("Failed to create db {0}: {1}", type, ex);
                            }
                    }
                    catch (Exception ex)
                    {
                        Trace.TraceError("Failed to load {0}: {1}", file, ex);
                    }
            }
            catch (Exception ex)
            {
                Trace.TraceError("Failed to load DB: {0}", ex);
            }
        }

        /// <param name="date"></param>
        /// <inheritdoc />
        public override QueryResult<ReportCalendar> GetCalendar(DateTime date)
        {
            var subtitles = new ReportCalendar(this);
            var statuses = new List<Status>(Handlers.Count);
            var status = Status.Success;
            var tasks = new List<Task>(Handlers.Count);
            var sb = new StringBuilder();

            if (Handlers.Count > 0)
            {
                foreach (var subtitleDb in Handlers)
                {
                    var db = subtitleDb;
                    var dbTask = Task.Run(
                        () =>
                        {
                            Status dbStatus;
                            try
                            {
                                var meta = db.GetCalendar(date);
                                dbStatus = meta.Status;
                                if (dbStatus == Status.Success && meta.Data != null && meta.Data.Count > 0)
                                {
                                    subtitles.AddRange(meta.Data);
                                }
                            }
                            catch (Exception ex)
                            {
                                var error = $"Failed to get report earning calendar from {db}: {ex}";
                                Trace.TraceError(error);
                                dbStatus = Status.Fatal;
                                sb.AppendLine(error);
                            }

                            statuses.Add(dbStatus);
                        });

                    tasks.Add(dbTask);
                }

                Task.WaitAll(tasks.ToArray());
                if (statuses.Distinct().Count() == statuses.Count)
                {
                    status = statuses.First();
                }
                else if (statuses.Any(s => s == Status.Success))
                {
                    status = Status.Success;
                }
                else if (statuses.Any(s => s == Status.Fatal))
                {
                    status = Status.Fatal;
                }
                else if (statuses.Any(s => s == Status.Failure))
                {
                    status = Status.Failure;
                }
            }

            return new QueryResult<ReportCalendar>(status, subtitles, sb.ToString());
        }
    }
}