using System;

namespace Page.Core
{
    [System.AttributeUsage(System.AttributeTargets.Interface) ]  
    public class PageAttribute : System.Attribute
    {
        public Type WindowType { get; set; }
    }
}
