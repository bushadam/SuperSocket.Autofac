using System.Collections.Generic;
using Autofac;
using Autofac.Builder;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Protocol;

namespace SuperSocket.Autofac
{
    public abstract class AutofacAppServer<TAppSession, TRequestInfo> : AppServer<TAppSession, TRequestInfo>
        where TAppSession : AppSession<TAppSession, TRequestInfo>, IAppSession, new()
        where TRequestInfo : class, IRequestInfo
    {
        protected IContainer Container { get; private set; }

        public AutofacAppServer():base()
        {
            Initialized();
        }

        protected AutofacAppServer(IReceiveFilterFactory<TRequestInfo> protocol)
            : base(protocol)
        {
            Initialized();
        }

        protected override bool SetupCommandLoaders(List<ICommandLoader<ICommand<TAppSession, TRequestInfo>>> commandLoaders)
        {
            var loader = Container.Resolve<ICommandLoader<ICommand<TAppSession, TRequestInfo>>>();
            commandLoaders.Add(loader);
            return true;
        }

        public bool Initialized()
        {
            Container = GetApplicationContainer();
            var builder = new ContainerBuilder();
            builder.RegisterType<AutofacCommandLoader<ICommand<TAppSession, TRequestInfo>>>().As<ICommandLoader<ICommand<TAppSession, TRequestInfo>>>();
            builder.Update(Container.ComponentRegistry);
            ConfigureApplicationContainer(Container);
            return true;
        }

        protected abstract void ConfigureApplicationContainer(ILifetimeScope lifetimeScope);
        
        protected virtual IContainer GetApplicationContainer()
        {
            return new ContainerBuilder().Build(ContainerBuildOptions.None);
        }
        
    }
}