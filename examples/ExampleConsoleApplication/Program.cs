using System;
using log4net;
using log4net.Config;

namespace ExampleConsoleApplication
{
    internal class Program
    {
        private static readonly ILog _logger = LogManager.GetLogger("main");

        private static void Main()
        {
            XmlConfigurator.Configure();

            _logger.Debug("test message from example app");

            _logger.Info("This is an info message");

            _logger.Error("Oops, here's an error");

            Console.ReadLine();
        }
    }
}