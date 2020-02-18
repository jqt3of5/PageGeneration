using System;

namespace Page.Generator.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            var task = new BuildPages();
            task.Execute();
        }
    }
}
