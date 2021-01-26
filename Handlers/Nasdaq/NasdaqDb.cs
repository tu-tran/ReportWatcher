using System.IO;
using System.Net;
using System.Text.RegularExpressions;

using Newtonsoft.Json;

using RestSharp;

namespace ReportWatcher.Data.Handlers.Nasdaq
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.Text.RegularExpressions;

    using Utils;

    public sealed class SubSceneDb : ICalendarSource
    {
        static SubSceneDb()
        {
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
                                                   | SecurityProtocolType.Tls11
                                                   | SecurityProtocolType.Tls12
                                                   | SecurityProtocolType.Ssl3;

        }

        /// <summary>
        /// Gets subtitles meta.
        /// </summary>
        /// <param name="date">The date.</param>
        /// <returns>
        /// The query result.
        /// </returns>
        public QueryResult<ReportCalendar> GetCalendar(DateTime date)
        {
            var calendar = new ReportCalendar(this);
            var req = (HttpWebRequest)WebRequest.Create($"https://api.nasdaq.com/api/calendar/earnings?date={date.ToString("yyyy-MM-dd", new CultureInfo("en-US"))}");
            req.Credentials = CredentialCache.DefaultCredentials;
            req.UserAgent =
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/88.0.4324.96 Safari/537.36 Edg/88.0.705.50";
            req.Headers.Add("accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9");
            var response = req.GetResponse();
            Root resp = null;
            using (var dataStream = response.GetResponseStream())
            {
                var reader = new StreamReader(dataStream);
                var responseFromServer = reader.ReadToEnd();
                resp = JsonConvert.DeserializeObject<Root>(responseFromServer);
            }

            response.Close();
            foreach (var h in resp.data.rows)
            {
            //    var reportEntry = new Report(name, code)
            //    {
            //        Date = reportDate,
            //        Esp = esp,
            //        NumberOfEsp = numOfEsp,
            //        LastEsp = lastEsp,
            //        MarketCap = marketCap
            //    };

            //    calendar.Add(reportEntry);
            }
            return new QueryResult<ReportCalendar>(ReportWatcher.Data.Status.Success, calendar);
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