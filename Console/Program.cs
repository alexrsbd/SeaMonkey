using System;
using Autofac;
using SeaMonkey.Modules;
using Serilog;

namespace SeaMonkey
{
    internal static class Program
    {
        public static readonly Random Rnd = new Random(235346798);

        private static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Console()
                .CreateLogger();

            var container = BuildContainer();

            using var scope = container.BeginLifetimeScope();
            var startup = scope.Resolve<IStartup>();
            startup.Run(args);
        }

        private static IContainer BuildContainer()
        {
            var builder = new ContainerBuilder();
            builder.RegisterModule<MonkeyModule>();
            return builder.Build();
        }
    }
}