namespace ReportWatcher.Tests
{
    using Data.Handlers;

    using NUnit.Framework;

    using WPF.Controllers;

    [TestFixture]
    public class DataTests
    {
        [Test]
        public void TestMethod()
        {
            var title = "The.Forbidden.Girl.2013.1080p.BluRay.x264.DTS-FGT.mkv";
            var controller = new MainViewController(new DummyView(), new AggregateCalendar());
            var result = controller.Query();
        }
    }
}