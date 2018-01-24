namespace ReportWatcher.Data
{
    using System;
    using System.Diagnostics;

    /// <summary>The icon.</summary>
    public enum Rating
    {
        /// <summary>The negative.</summary>
        Negative,

        /// <summary>The neutral.</summary>
        Neutral,

        /// <summary>The positive.</summary>
        Positive
    }

    /// <summary>The item data.</summary>
    [DebuggerDisplay("{Name} ({Code}) {MarketCap}")]
    public class Report
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Report" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="code">The description.</param>
        public Report(string name, string code)
        {
            this.Name = name;
            this.Code = code;
        }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        public string Code { get; }

        /// <summary>
        /// Gets or sets the market cap.
        /// </summary>
        public double MarketCap { get; set; }

        /// <summary>
        /// Gets or sets the date.
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Gets or sets the esp.
        /// </summary>
        public double Esp { get; set; }

        /// <summary>
        /// Gets or sets the number of esp.
        /// </summary>
        public double NumberOfEsp { get; set; }

        /// <summary>
        /// Gets or sets the last esp.
        /// </summary>
        public double LastEsp { get; set; }

        /// <summary>
        /// Gets the rating.
        /// </summary>
        public Rating Rating
        {
            get
            {
                var diff = this.Esp - this.LastEsp;
                if (diff > double.Epsilon)
                {
                    return Rating.Positive;
                }

                if (diff < -double.Epsilon)
                {
                    return Rating.Negative;
                }

                return Rating.Neutral;
            }
        }

        /// <summary>
        /// Gets the Year on Year esp.
        /// </summary>
        public double YoYEsp => this.Esp - this.LastEsp;

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" />, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>
        /// <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            var otherReport = obj as Report;
            if (otherReport == null)
            {
                return false;
            }

            return this.Date.Date == otherReport.Date.Date && this.Code == otherReport.Code;
        }
    }
}