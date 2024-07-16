using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuplicateDetector.Tests
{
    public abstract class TestBase
    {
        protected TestBase()
        {
            LoggingExtensions.LoggerFactory = LoggerFactory.Create(builder => { });

            // Set language to english for unit tests.
            CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("en-US");
            CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("en-US");
            CultureInfo.CurrentCulture = new CultureInfo("en-US");
            CultureInfo.CurrentUICulture = new CultureInfo("en-US");
        }
    }
}
