namespace ReportWatcher.Data.Handlers
{
    using System;

    /// <summary>
    /// The <see cref="ICalendarSource" /> interfaces.
    /// </summary>
    public interface ICalendarSource
    {
        /// <summary>
        /// Gets subtitles meta.
        /// </summary>
        /// <param name="date">The date.</param>
        /// <returns>
        /// The query result.
        /// </returns>
        QueryResult<ReportCalendar> GetCalendar(DateTime date);
    }
}