namespace ReportWatcher.WPF.Views
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.IO;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Threading;
    using System.Windows;
    using System.Windows.Data;
    using System.Windows.Forms;
    using System.Windows.Input;

    using Data;

    using ServiceStack;
    using ServiceStack.Text;

    using KeyEventArgs = System.Windows.Input.KeyEventArgs;
    using TextBox = System.Windows.Controls.TextBox;

    /// <summary>Interaction logic for SelectionWindow.xaml</summary>
    public partial class MainView : INotifyPropertyChanged
    {
        /// <summary>The selections.</summary>
        private readonly ObservableCollection<Report> reports = new ObservableCollection<Report>();

        /// <summary>Is disposing.</summary>
        private bool disposing;

        /// <summary>The cancellation token source for windows hidden event.</summary>
        private CancellationTokenSource hideCancellationToken;

        /// <summary>The last position.</summary>
        private Tuple<double, double> lastPosition;

        /// <summary>The selected item.</summary>
        private Report selectedItem;

        /// <summary>The status.</summary>
        private string status;

        /// <summary>The title text.</summary>
        private string titleText;

        /// <summary>
        /// The refresh command
        /// </summary>
        private ICommand refreshCommand;

        /// <summary>Initializes a new instance of the <see cref="MainView" /> class.</summary>
        internal MainView()
        {
            this.InitializeComponent();
        }

        /// <summary>Gets the selected item.</summary>
        public Report SelectedItem
        {
            get => this.selectedItem;

            set
            {
                this.selectedItem = value;
                this.RaisePropertyChanged();
            }
        }

        /// <summary>Gets the selections.</summary>
        public ObservableCollection<Report> Reports => this.reports;

        /// <summary>Gets a value indicating whether the selection has been made.</summary>
        public Status SelectionState { get; private set; }

        public ICommand ExportCsvCommand
        {
            get
            {
                return new ActionCommand(() =>
                {
                    var result = CsvSerializer.SerializeToCsv(this.Reports);
                    var target = Path.Combine(
                        Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) ?? string.Empty,
                        Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly().Location) + ".csv");
                    File.WriteAllText(target, result, Encoding.Default);
                });
            }
        }

        /// <summary>
        /// Gets or sets the refresh command.
        /// </summary>
        public ICommand RefreshCommand
        {
            get { return this.refreshCommand; }
            set
            {
                this.refreshCommand = value;
                this.RaisePropertyChanged();
            }
        }

        /// <summary>Gets or sets the status.</summary>
        public string Status
        {
            get => this.status;

            set
            {
                this.status = value;
                this.RaisePropertyChanged();
            }
        }

        /// <summary>Gets or sets the title text.</summary>
        public string TitleText
        {
            get => this.titleText;

            set
            {
                this.titleText = value;
                this.RaisePropertyChanged();
                this.QueryBox.Visibility = string.IsNullOrEmpty(value) ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        /// <summary>The property changed.</summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>The set progress.</summary>
        /// <param name="title">The title.</param>
        /// <param name="newStatus">The status.</param>
        internal void SetProgress(string title, string newStatus)
        {
            if (!this.Dispatcher.CheckAccess())
            {
                this.Dispatcher.Invoke(() => this.SetProgress(title, newStatus));
                return;
            }

            this.ProgressBar.Visibility = Visibility.Visible;
            this.TitleText = title;
            this.Status = newStatus;
        }

        /// <summary>Sets the progress.</summary>
        /// <param name="done">Done.</param>
        /// <param name="total">Total</param>
        internal void SetProgress(double done, double total)
        {
            if (!this.Dispatcher.CheckAccess())
            {
                this.Dispatcher.Invoke(() => this.SetProgress(done, total));
                return;
            }

            this.ProgressBar.Visibility = Visibility.Visible;
            this.ProgressBar.Value = done;
            this.ProgressBar.Maximum = total;
            this.ProgressBar.IsIndeterminate = done < 1;
        }

        /// <summary>
        /// Increases progress.
        /// </summary>
        internal void IncreaseProgress()
        {
            if (!this.Dispatcher.CheckAccess())
            {
                this.Dispatcher.Invoke(this.IncreaseProgress);
                return;
            }

            if (this.ProgressBar.Value >= this.ProgressBar.Maximum)
            {
                return;
            }

            this.SetProgress(this.ProgressBar.Value + 1.0, this.ProgressBar.Maximum);
        }

        /// <summary>The set selections.</summary>
        /// <param name="data">The data.</param>
        /// <param name="title">The title.</param>
        /// <param name="status">The status.</param>
        internal void SetSelections(ICollection<Report> data, string title, string status, CancellationTokenSource token)
        {
            if (!this.Dispatcher.CheckAccess())
            {
                this.Dispatcher.Invoke(() => this.SetSelections(data, title, status, token));
                return;
            }

            this.hideCancellationToken = token;
            this.reports.Clear();
            foreach (var itemData in data)
                this.reports.Add(itemData);

            this.TitleText = title;
            this.Status = status;
            this.ProgressBar.Visibility = Visibility.Collapsed;
            this.ProgressBar.IsIndeterminate = false;
            this.lastPosition = null;
            this.AutoSize();
            if (!this.IsVisible)
            {
                this.AutoPosition();
                this.Show();
            }
        }

        /// <summary>The list box item mouse double click.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The eventArgs.</param>
        protected void ListBoxItemMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            this.Accept(Data.Status.Success, false);
        }

        /// <summary>The raise property changed.</summary>
        /// <param name="propertyName">The property name.</param>
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (propertyName == null)
            {
                throw new ArgumentNullException("propertyName");
            }

            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        /// <summary>The accept.</summary>
        internal void Accept(Status result = Data.Status.Success, bool hide = true)
        {
            if (!this.Dispatcher.CheckAccess())
            {
                this.Dispatcher.Invoke(() => this.Accept(result, hide));
                return;
            }

            this.SelectionState = result;
            if (hide)
            {
                this.Hide();
            }
            else
            {
                this.OnNotifyToken();
            }
        }

        /// <summary>Auto size.</summary>
        internal void AutoSize()
        {
            if (this.WindowState == WindowState.Maximized)
            {
                return;
            }

            this.SizeToContent = SizeToContent.WidthAndHeight;
            this.SizeToContent = SizeToContent.Manual;
            var mousePosition = Control.MousePosition;
            var activeScreenArea = Screen.FromPoint(mousePosition).WorkingArea;
            if (this.Left + this.ActualWidth > activeScreenArea.Right)
            {
                var newWidth = activeScreenArea.Right - this.Left;
                if (newWidth > 0)
                {
                    this.Width = newWidth;
                }
            }

            if (this.Top + this.ActualHeight > activeScreenArea.Bottom)
            {
                var newHeight = activeScreenArea.Bottom - this.Top;
                if (newHeight > 0)
                {
                    this.Height = newHeight;
                }
            }
        }

        /// <summary>Auto position.</summary>
        private void AutoPosition()
        {
            if (this.WindowState != WindowState.Normal)
            {
                return;
            }

            this.SizeToContent = SizeToContent.WidthAndHeight;
            this.SizeToContent = SizeToContent.Manual;
            if (this.lastPosition == null)
            {
                var mousePosition = Control.MousePosition;
                var activeScreenArea = Screen.FromPoint(mousePosition).WorkingArea;
                if (this.ActualWidth > activeScreenArea.Width)
                {
                    this.Width = activeScreenArea.Width;
                }

                if (this.ActualHeight > activeScreenArea.Height)
                {
                    this.Height = activeScreenArea.Height;
                }

                var left = mousePosition.X - this.ActualWidth / 2;
                var top = mousePosition.Y - this.ActualHeight / 2;

                if (left < SystemParameters.VirtualScreenLeft)
                {
                    left = SystemParameters.VirtualScreenLeft;
                }
                else if (left + this.ActualWidth > SystemParameters.VirtualScreenWidth)
                {
                    left = SystemParameters.VirtualScreenWidth - this.ActualWidth;
                }

                if (top < SystemParameters.VirtualScreenTop)
                {
                    top = SystemParameters.VirtualScreenTop;
                }
                else if (top + this.ActualHeight > SystemParameters.VirtualScreenHeight)
                {
                    top = SystemParameters.VirtualScreenHeight - this.ActualHeight;
                }

                this.Left = left;
                this.Top = top;
            }
            else
            {
                this.Left = this.lastPosition.Item1;
                this.Top = this.lastPosition.Item2;
            }
        }

        /// <summary>The main window_ on content rendered.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The eventArgs.</param>
        private void MainWindow_OnContentRendered(object sender, EventArgs e)
        {
            this.SizeToContent = SizeToContent.Manual;
        }

        /// <summary>When the visibility is changed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arguments.</param>
        private void MainWindow_OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue.Equals(false))
            {
                this.lastPosition = new Tuple<double, double>(this.Left, this.Top);
                this.OnNotifyToken();
            }
            else
            {
                this.AutoPosition();
            }
        }

        private void OnNotifyToken()
        {
            if (this.hideCancellationToken != null)
            {
                this.hideCancellationToken.Cancel();
                this.hideCancellationToken = null;
            }
        }

        /// <summary>The main window_ on preview key up.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The eventArgs.</param>
        private void MainWindow_OnPreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                this.Close();
            }
        }

        /// <summary>Queries the box got keyboard focus.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="KeyboardFocusChangedEventArgs" /> instance containing the event data.</param>
        private void QueryBoxGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            var tb = sender as TextBox;
            if (tb != null && tb.IsKeyboardFocusWithin && e.OriginalSource == sender)
            {
                ((TextBox) sender).SelectAll();
            }
        }

        /// <summary>Selectivelies the ignore mouse button.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="MouseButtonEventArgs" /> instance containing the event data.</param>
        /// <exception cref="NotImplementedException"></exception>
        private void SelectivelyIgnoreMouseButton(object sender, MouseButtonEventArgs e)
        {
            var tb = sender as TextBox;
            if (tb != null && !tb.IsKeyboardFocusWithin)
            {
                e.Handled = true;
                tb.Focus();
            }
        }
    }
}