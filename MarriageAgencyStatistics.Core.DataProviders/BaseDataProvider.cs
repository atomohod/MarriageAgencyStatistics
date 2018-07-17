using System.Linq;
using System.Threading.Tasks;
using AngleSharp.Dom;
using AngleSharp.Dom.Html;
using AngleSharp.Parser.Html;

namespace MarriageAgencyStatistics.Core.DataProviders
{
    public class BaseDataProvider
    {
        private async Task<IHtmlDocument> ParseAsync(string content)
        {
            var parser = new HtmlParser();
            return await parser.ParseAsync(content);
        }

        public async Task<IElement> GetContentAsync(string data)
        {
            var doc = await ParseAsync(data);
            return doc.GetElementsByClassName("contentbox").First();
        }

        private IHtmlDocument Parse(string content)
        {
            var parser = new HtmlParser();
            return parser.Parse(content);
        }

        public IElement GetContent(string data)
        {
            var doc = Parse(data);
            return doc.GetElementsByClassName("contentbox").First();
        }
    }
}