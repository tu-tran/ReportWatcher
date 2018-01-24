namespace ReportWatcher.Data.Handlers.Nasdaq
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.Text.RegularExpressions;

    using Utils;

    public sealed class SubSceneDb : ICalendarSource
    {
        /// <summary>
        /// The base URL.
        /// </summary>
        private const string BaseUrl = "http://www.nasdaq.com/";

        /// <summary>
        /// The calendar URL.
        /// </summary>
        private const string CalendarUrl = "http://www.nasdaq.com/earnings/earnings-calendar.aspx";

        /// <summary>
        /// Gets subtitles meta.
        /// </summary>
        /// <param name="date">The date.</param>
        /// <returns>
        /// The query result.
        /// </returns>
        public QueryResult<ReportCalendar> GetCalendar(DateTime date)
        {
            var culture = new CultureInfo("en-US");
            var query = $"{CalendarUrl}?date={date.ToString("yyyy-MMM-dd", culture)}";

            var calendar = new ReportCalendar(this);
            var mainDoc = query.GetDocument(BaseUrl).DocumentNode;
            var rootTable = mainDoc.SelectNodes("//table[@id='ECCompaniesTable']/tr");
            if (rootTable != null)
            {
                foreach (var row in rootTable)
                {
                    var columns = row.SelectNodes("td");
                    if (columns == null)
                    {
                        continue;
                    }

                    var companyNode = columns[1].SelectSingleNode("a");
                    if (companyNode == null)
                    {
                        Trace.TraceError("Failed to find company info node");
                        continue;
                    }

                    var companyRegex = new Regex(@"(?<Name>.*) \((?<Code>.+?)\) Market Cap: (?<MarketCap>.+?)$", RegexOptions.Compiled | RegexOptions.Multiline);
                    var companyMatch = companyRegex.Match(companyNode.InnerText);
                    if (!companyMatch.Success)
                    {
                        Trace.TraceError("Failed to parse company info");
                        continue;
                    }

                    var name = companyMatch.Groups["Name"].Value;
                    var code = companyMatch.Groups["Code"].Value;
                    var marketCapStr = companyMatch.Groups["MarketCap"].Value;

                    var marketCap = ParseCurrencyValue(marketCapStr);

                    var reportDate = DateTime.ParseExact(columns[2].InnerText.TrimDecoded(), "MM/dd/yyyy", CultureInfo.CurrentUICulture);
                    var esp = ParseCurrencyValue(columns[4].InnerText.TrimDecoded());
                    var numOfEsp = double.Parse(columns[5].InnerText.TrimDecoded());
                    var lastEsp = ParseCurrencyValue(columns[7].InnerText.TrimDecoded());

                    var reportEntry = new Report(name, code)
                    {
                        Date = reportDate,
                        Esp = esp,
                        NumberOfEsp = numOfEsp,
                        LastEsp = lastEsp,
                        MarketCap = marketCap
                    };

                    calendar.Add(reportEntry);
                }
            }

            return new QueryResult<ReportCalendar>(Status.Success, calendar);
        }

        /// <summary>
        /// Parses currency value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The value.</returns>
        private static double ParseCurrencyValue(string value)
        {
            var currencyRegex = new Regex(@"\$?(?<Value>.+?)(?<Multiplier>M|B)?$", RegexOptions.Compiled);
            var valueMatch = currencyRegex.Match(value);
            var multiplierMatch = valueMatch.Groups["Multiplier"];
            var multiplier = 1.0;
            if (!valueMatch.Success)
            {
                return default(double);
            }

            var valueStr = valueMatch.Groups["Value"].Value;
            if (valueMatch.Groups["Multiplier"].Success)
            {
                if (multiplierMatch.Value == "M")
                {
                    multiplier = 1000000.0;
                }
                else if (multiplierMatch.Value == "B")
                {
                    multiplier = 1000000000.0;
                }
            }

            double result;
            return double.TryParse(valueStr, out result) ? result * multiplier : default(double);
        }
    }
}