namespace ReportWatcher.WPF.Views
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Windows;

    using Data;

    /// <summary>
    /// The <see cref="WpfView" /> class.
    /// </summary>
    internal class WpfView : ThreadedWindowView<MainView>, IView
    {
        public WpfView()
        {
            this.Window.Closed += (s, e) => Application.Current.Dispatcher.Invoke(() => Application.Current.Shutdown(0));
            this.Window.RefreshCommand = new ActionCommand(() =>
            {
                if (this.RefreshRequested != null)
                {
                    this.RefreshRequested();
                }
            });
        }

        /// <summary>
        /// Occurs when refresh is requested.
        /// </summary>
        public event Action RefreshRequested;

        /// <summary>
        /// Adds reports.
        /// </summary>
        /// <param name="report">The report.</param>
        public void AddReport(Report report)
        {
            this.Window.Dispatcher.Invoke(() =>
            {
                this.Window.Reports.Add(report);
            });
        }

        /// <summary>
        /// Clears this instance.
        /// </summary>
        public void Clear()
        {
            this.Window.Dispatcher.Invoke(() =>
            {
                this.Window.Reports.Clear();
            });
        }

        /// <summary>Notifies a message.</summary>
        /// <param name="message">The message.</param>
        public void Notify(string message)
        {
            NotificationView.Show(message);
        }

        /// <summary>The show progress.</summary>
        /// <param name="title">The title.</param>
        /// <param name="status">The status.</param>
        public void ShowProgress(string title, string status)
        {
            this.Window.SetProgress(title, status);
        }

        /// <summary>Sets the progress.</summary>
        /// <param name="done">Done.</param>
        /// <param name="total">Total</param>
        public void ShowProgress(int done, int total)
        {
            this.Window.SetProgress(done, total);
        }

        /// <summary>
        /// Increases progress.
        /// </summary>
        public void IncreaseProgress()
        {
            this.Window.IncreaseProgress();
        }

        /// <summary>
        /// Updates layout.
        /// </summary>
        public void UpdateLayout()
        {
            this.Window.AutoSize();
        }

        /// <summary>Occurs when the view custom action is requested.</summary>
        public event CustomActionDelegate CustomActionRequested;

        /// <summary>Continues the pending operation and cancel any selection.</summary>
        public void Continue()
        {
            this.Window.Accept(Status.Skipped);
        }

        /// <summary>
        /// Gets the selection.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="title">The title.</param>
        /// <param name="status">The status.</param>
        /// <returns>The query result.</returns>
        public virtual QueryResult<Report> GetSelection(ICollection<Report> data, string title, string status)
        {
            var token = new CancellationTokenSource();
            this.Window.SetSelections(data, title, status, token);
            token.Token.WaitHandle.WaitOne();
            return new QueryResult<Report>(this.Window.SelectionState, this.Window.SelectedItem);
        }

        /// <summary>The on custom action.</summary>
        /// <param name="parameter">The parameter.</param>
        /// <param name="actionNames">The action names.</param>
        public void OnCustomAction(object parameter, params string[] actionNames)
        {
            if (this.CustomActionRequested != null)
            {
                ThreadPool.QueueUserWorkItem(o => this.CustomActionRequested(this, parameter, actionNames));
            }
        }

        /// <summary>
        /// Creates the Window view.
        /// </summary>
        /// <returns>The Window view.</returns>
        protected override MainView CreateWindowView()
        {
            return new MainView();
        }
    }
}