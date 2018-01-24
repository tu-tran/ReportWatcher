namespace ReportWatcher.WPF
{
    using System;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Threading;

    using Controllers;

    using Data.Handlers;

    using Views;

    /// <summary>Interaction logic for App.xaml</summary>
    public partial class App
    {
        /// <summary>
        /// The controller.
        /// </summary>
        private MainViewController controller;

        /// <summary>The on startup.</summary>
        /// <param name="e">The e.</param>
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            this.InitializeErrorHandler();
            NotificationView.Initialize();
            var mainView = new WpfView();
            this.MainWindow = mainView.Window;
            this.controller = new MainViewController(mainView, new AggregateCalendar());
            this.controller.Query();
        }

        /// <summary>Raises when there is unhandled exception.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="eventArgs">The event arguments.</param>
        private static void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs eventArgs)
        {
            if (CanIgnoreException(eventArgs.ExceptionObject))
            {
                return;
            }

            var exception = eventArgs.ExceptionObject as Exception;
            var sb = new StringBuilder();

            while (exception != null)
            {
                sb.AppendLine(exception.ToString());
                sb.AppendLine("----------------------");
                exception = exception.InnerException;
            }

            var dialog = new MessageDialog { Title = "Unexpected errors", Message = sb.ToString() };
            dialog.Closed += (o, args) => Current.Dispatcher.InvokeShutdown();
            dialog.Show();
        }

        /// <summary>Raises when there is unhandled exception.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="eventArgs">The event arguments.</param>
        private static void CurrentAppOnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs eventArgs)
        {
            if (CanIgnoreException(eventArgs.Exception))
            {
                eventArgs.Handled = true;
            }
            else
            {
                CurrentDomainOnUnhandledException(sender, new UnhandledExceptionEventArgs(eventArgs.Exception, false));
            }
        }

        /// <summary>Raises when there is unhandled exception.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="eventArgs">The event arguments.</param>
        private static void TaskSchedulerOnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs eventArgs)
        {
            CurrentDomainOnUnhandledException(sender, new UnhandledExceptionEventArgs(eventArgs.Exception, false));
        }

        /// <summary>Initializes the error handler.</summary>
        private void InitializeErrorHandler()
        {
            Current.DispatcherUnhandledException += CurrentAppOnDispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
            TaskScheduler.UnobservedTaskException += TaskSchedulerOnUnobservedTaskException;
        }

        /// <summary>
        /// Determines whether this instance [can ignore exception] the specified exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <returns>
        /// <c>true</c> if this instance [can ignore exception] the specified exception; otherwise, <c>false</c>.
        /// </returns>
        private static bool CanIgnoreException(object exception)
        {
            var comException = exception as COMException;
            if (comException != null && comException.ErrorCode == -2147221040)
            {
                return true;
            }

            return false;
        }
    }
}