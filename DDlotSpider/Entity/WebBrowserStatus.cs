using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DDlotSpider.Entity
{
    public enum BrowserStatus
    {
        Free = 0,
        Busy = 9
    }

    public enum AppMode
    {
        Test,
        Prod
    }

    public enum ProgressStatus
    {
        NotFound = -1,
        UnSearch = 0,
        Search = 9,
        Done = 1
    }

    public enum InputMode {
        Auto,
        Manual
    }
}
