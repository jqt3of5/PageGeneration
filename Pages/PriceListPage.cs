﻿using Page.Core;
using System;

namespace Pages
{
    public class EstimateWindow
    {
        public EstimateWindow(string name)
        {

        }
    }

    public interface Page<T> where T : EstimateWindow
    {
        T Window { get; }
    }

    [Page(WindowType = typeof(EstimateWindow))]
    public interface CauseOfLossPage : Page<EstimateWindow>
    {
        [Element(Locator="element1")]
        Element Element1 { get; }
        
        [Element(Locator = "element2")]
        Element Element2 { get; }

        String element3 { get; }

        [Element(Locator = "element4")]
        String element4 { get; }
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
