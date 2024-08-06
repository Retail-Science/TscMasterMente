using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Serilog;
using Serilog.Core;

namespace TscMasterMente.Common
{
    public sealed class LogParts
    {
        public  Logger GetInstance()
        {
            Logger logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .WriteTo.File(Path.Combine(Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName, "logs//app_.log"), rollingInterval: RollingInterval.Day)
                .CreateLogger();

            return logger;
        }
    }
}
