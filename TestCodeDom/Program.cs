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

}
