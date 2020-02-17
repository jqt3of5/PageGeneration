using System;
using Page.Core;
using PageGenerator;

namespace TestCodeDom
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");


            var page = PageSource.For<CauseOfLossPage>();

        
            var element = page.Element1;
            
        }
    }

    class EstimateWindow
    {
        public EstimateWindow(string name)
        {

        }
    }

    interface Page<T> where T : EstimateWindow
    {

    }

    interface CauseOfLossPage : Page<EstimateWindow>
    {
        [Element("element1")]
        Element Element1 { get; }

        [Element("element2")]
        Element Element2 { get; }

        
    }

    static class CauseOfLossPageExtensions
    {
        public static Element GetChildRows(this CauseOfLossPage page)
        {
            return page.Element1;
        }
    }


    class Element
    {
        public Element(string findBy)
        {

        }

        public Element child;
    }
}
