using System.Collections.Generic;

namespace ReportWatcher.Data.Handlers.Nasdaq
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
    public class Headers
    {
        public string time { get; set; }
        public string symbol { get; set; }
        public string name { get; set; }
        public string marketCap { get; set; }
        public string fiscalQuarterEnding { get; set; }
        public string epsForecast { get; set; }
        public string noOfEsts { get; set; }
        public string lastYearRptDt { get; set; }
        public string lastYearEPS { get; set; }
    }

    public class Row
    {
        public string lastYearRptDt { get; set; }
        public string lastYearEPS { get; set; }
        public string time { get; set; }
        public string symbol { get; set; }
        public string name { get; set; }
        public string marketCap { get; set; }
        public string fiscalQuarterEnding { get; set; }
        public string epsForecast { get; set; }
        public string noOfEsts { get; set; }
    }

    public class Data
    {
        public Headers headers { get; set; }
        public List<Row> rows { get; set; }
    }

    public class Status
    {
        public int rCode { get; set; }
        public object bCodeMessage { get; set; }
        public object developerMessage { get; set; }
    }

    public class Root
    {
        public Data data { get; set; }
        public object message { get; set; }
        public Status status { get; set; }
    }



}
