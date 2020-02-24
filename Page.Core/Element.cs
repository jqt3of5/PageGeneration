using System;
using System.Collections.Generic;
using System.Text;

namespace Page.Core
{
    interface Element : TestElementInterface
    {
        void Click();
        void SendText(string text);

        void WaitForExistence();
    }
}
