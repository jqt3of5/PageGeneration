using System;
using System.Collections.Generic;
using System.Text;

namespace Page.Core
{
    [System.AttributeUsage(System.AttributeTargets.Property)]
    public class ElementAttribute : System.Attribute
    {
        public ElementAttribute(string locator)
        {

        }

        public string FindBy { get; set; } = "";

    }
}
