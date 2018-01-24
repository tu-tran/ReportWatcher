namespace ReportWatcher.Data
{
    using System.Collections.Generic;

    using Handlers;

    /// <summary>
    /// The <see cref="ReportCalendar" /> class represents a report calendar.
    /// </summary>
    public class ReportCalendar : List<Report>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReportCalendar" /> class.
        /// </summary>
        /// <param name="source">The source.</param>
        public ReportCalendar(ICalendarSource source)
        {
            this.Source = source;
        }

        /// <summary>
        /// Gets the source.
        /// </summary>
        public ICalendarSource Source { get; }
    }
}