using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace csse.Logging
{
    public static class LoggerFactory
    {
        public static ILogger Create(Configuration conf)
        {
            ILogger logger;
            try
            {
                logger = new Logger(conf);
            }
            catch (Exception outerException)
            {
                Console.Error.Write("Error creating logger... logging deactivated " + outerException.Message);
                logger = new NullLogger();
            }

            return logger;
        }
    }
}
