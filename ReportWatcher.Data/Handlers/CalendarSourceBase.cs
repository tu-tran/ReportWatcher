namespace ReportWatcher.Data.Handlers
{
    using System;

    /// <summary>
    /// The <see cref="CalendarSourceBase" /> is the abstract class for all subtitle data handlers.
    /// </summary>
    public abstract class CalendarSourceBase : ICalendarSource
    {
        /// <param name="date"></param>
        /// <inheritdoc />
        public abstract QueryResult<ReportCalendar> GetCalendar(DateTime date);
    }
}