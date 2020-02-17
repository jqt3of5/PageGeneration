using Refit;
using System;

namespace Network
{
    
    public interface GetStuff
    {
        [Get("")]
        string getStuff();
    }
}
