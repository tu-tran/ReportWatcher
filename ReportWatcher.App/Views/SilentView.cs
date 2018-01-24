namespace ReportWatcher.WPF.Views
{
    using System.Collections.Generic;
    using System.Linq;

    using Data;

    /// <summary>The silent view.</summary>
    internal sealed class SilentView : WpfView
    {
        /// <summary>
        /// Gets the selection.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="title">The title.</param>
        /// <param name="status">The status.</param>
        /// <returns>The query result.</returns>
        public override QueryResult<Report> GetSelection(ICollection<Report> data, string title, string status)
        {
            if (data == null || !data.Any())
            {
                return new QueryResult<Report>(Status.Failure, null);
            }

            return new QueryResult<Report>(Status.Success, data.FirstOrDefault());
        }
    }
}