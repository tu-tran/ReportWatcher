namespace ReportWatcher.Data.Utils
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Net;

    using HtmlAgilityPack;

    /// <summary>
    /// The web extensions.
    /// </summary>
    public static class WebExtensions
    {
        /// <summary>
        /// Gets the web document.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="referrer">The referrer.</param>
        /// <param name="isMobile">A value indicating whether to.</param>
        /// <returns>The web document.</returns>
        public static HtmlDocument GetDocument(this string url, string referrer = "", bool isMobile = true)
        {
            var htmlDoc = new HtmlDocument();
            Stream respStream = null;
            var attempts = 0;

            while (attempts++ < 5 && respStream == null)
                try
                {
                    var request = GetRequest(url, referrer, isMobile);
                    using (respStream = request.GetResponse().GetResponseStream())
                    {
                        if (respStream != null)
                        {
                            htmlDoc.Load(respStream, true);
                            break;
                        }
                    }
                }
                catch
                {
                    // ignored
                }

            return htmlDoc;
        }

        /// <summary>
        /// Gets the web request.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="referrer">The referrer.</param>
        /// <param name="isMobile">A value indicating whether to.</param>
        /// <returns>The web request.</returns>
        public static HttpWebRequest GetRequest(this string url, string referrer = "", bool isMobile = true)
        {
            var request = (HttpWebRequest) WebRequest.Create(url);
            request.UserAgent = GetUserAgent(isMobile);
            request.Referer = referrer;
            request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
            return request;
        }

        /// <summary>
        /// Decodes the specified text.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>The decoded text.</returns>
        public static string Decode(this string text)
        {
            return WebUtility.HtmlDecode(text ?? "");
        }

        /// <summary>
        /// Decodes and trims the specified text.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>The decoded and trimmed text.</returns>
        public static string TrimDecoded(this string text)
        {
            return text.Decode().Trim();
        }

        /// <summary>
        /// URLs parameter encode.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The string.</returns>
        public static string UrlParamEncode(this string value)
        {
            return value.Any(c => !Uri.IsHexDigit(c)) ? WebUtility.UrlEncode(value) : string.Join(string.Empty, value.Select(c => Uri.HexEscape(c)).ToArray());
        }

        /// <summary>
        /// Gets the user agent.
        /// </summary>
        /// <param name="isMobile">A value indicating whether is for mobile.</param>
        /// <returns>The user agent string.</returns>
        public static string GetUserAgent(bool isMobile = true)
        {
            return isMobile
                ? "Mozilla/5.0 (Linux; U; Android 4.2; en-us; SonyC6903 Build/14.1.G.1.518) AppleWebKit/534.30 (KHTML, like Gecko) Version/4.0 Mobile Safari/534.30"
                : "Mozilla/5.0 (Windows NT 6.1; WOW64; Trident/7.0; AS; rv:11.0) like Gecko";
        }
    }
}