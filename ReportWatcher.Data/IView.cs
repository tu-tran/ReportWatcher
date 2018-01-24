namespace ReportWatcher.Data
{
    using System;
    using System.ComponentModel.Design;

    /// <summary>
    /// The <see cref="CustomActionDelegate" /> delegate provides the signature for.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="parameter">The parameter.</param>
    /// <param name="actionNames">The action names.</param>
    public delegate void CustomActionDelegate(IView sender, object parameter, params string[] actionNames);

    /// <summary>
    /// The <see cref="IView" /> interfaces.
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    public interface IView : IDisposable
    {
        /// <summary>
        /// Occurs when refresh is requested.
        /// </summary>
        event Action RefreshRequested;

        /// <summary>
        /// Adds reports.
        /// </summary>
        /// <param name="report">The report.</param>
        void AddReport(Report report);

        /// <summary>
        /// Clears this instance.
        /// </summary>
        void Clear();

        /// <summary>Notifies a message.</summary>
        /// <param name="message">The message.</param>
        void Notify(string message);

        /// <summary>
        /// Shows progress.
        /// </summary>
        /// <param name="title">The title.</param>
        /// <param name="status">The status.</param>
        void ShowProgress(string title, string status);

        /// <summary>
        /// Shows progress.
        /// </summary>
        /// <param name="done">The done.</param>
        /// <param name="total">The total.</param>
        void ShowProgress(int done, int total);

        /// <summary>
        /// Increases progress.
        /// </summary>
        void IncreaseProgress();

        /// <summary>
        /// Updates layout.
        /// </summary>
        void UpdateLayout();
    }
}