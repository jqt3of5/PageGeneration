using Page.Core;
using System;

namespace Pages
{
    public class EstimateWindow
    {
        public EstimateWindow(string name)
        {

        }
    }

    [Page(WindowType = typeof(EstimateWindow))]
    public interface CauseOfLossPage 
    {
        [Element("element1")]
        Element Element1 { get; }
        
        [Element("element2")]
        Element Element2 { get; }

        [Element("element4")]
        Element element4 { get; }
    }

    static class CauseOfLossPageExtensions
    {
        public static Element GetChildRows(this CauseOfLossPage page)
        {
            return page.Element1;
        }
    }

    public class Element
    {
        public Element(string findBy)
        {
        }

        public Element child;
    }
}
