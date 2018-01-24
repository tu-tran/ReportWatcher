namespace ReportWatcher.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    using Data;

    /// <summary>
    /// The <see cref="DummyView" /> class represents a dummy view handler.
    /// </summary>
    /// <seealso cref="ReportWatcher.Data.IView" />
    public sealed class DummyView : IView
    {
        /// <summary>
        /// Occurs when refresh is requested.
        /// </summary>
        public event Action RefreshRequested;

        /// <summary>
        /// Adds reports.
        /// </summary>
        /// <param name="report"></param>
        public void AddReport(Report report)
        {
        }

        public void Clear()
        {
        }

        /// <summary>
        /// Notifies a message.
        /// </summary>
        /// <param name="message">The message.</param>
        public void Notify(string message)
        {
            Trace.WriteLine(message);
        }

        /// <summary>
        /// Shows progress.
        /// </summary>
        /// <param name="title">The title.</param>
        /// <param name="status">The status.</param>
        public void ShowProgress(string title, string status)
        {
            Trace.WriteLine(string.Format("[{0}] {1}", title, status));
        }

        /// <summary>
        /// Shows progress.
        /// </summary>
        /// <param name="done">The done.</param>
        /// <param name="total">The total.</param>
        public void ShowProgress(int done, int total)
        {
            Trace.WriteLine(string.Format("Progress: {0}/{1}", done, total));
        }

        /// <summary>
        /// Increases progress.
        /// </summary>
        public void IncreaseProgress()
        {
        }

        /// <summary>
        /// Updates layout.
        /// </summary>
        public void UpdateLayout()
        {
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
        }

        /// <summary>
        /// Occurs when custom action requested.
        /// </summary>
        public event CustomActionDelegate CustomActionRequested;

        /// <summary>
        /// Continues this instance.
        /// </summary>
        public void Continue()
        {
        }

        /// <summary>
        /// Gets the selection.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="title">The title.</param>
        /// <param name="status">The status.</param>
        /// <returns>The query result.</returns>
        public QueryResult<Report> GetSelection(ICollection<Report> data, string title, string status)
        {
            return new QueryResult<Report>(Status.Success, data.FirstOrDefault());
        }
    }
}