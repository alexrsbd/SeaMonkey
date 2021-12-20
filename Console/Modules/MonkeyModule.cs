using Autofac;

namespace SeaMonkey.Modules
{
    // ReSharper disable once UnusedType.Global
    public class MonkeyModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<Startup>()
                .As<IStartup>()
                .InstancePerLifetimeScope();

            builder.RegisterType<TroopRunner>()
                .As<ITroopRunner>()
                .InstancePerLifetimeScope();

            builder.RegisterType<TroopRequestParser>()
                .As<ITroopRequestParser>()
                .InstancePerLifetimeScope();
        }
    }
}