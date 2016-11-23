using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autofac;
using SuperSocket.Common;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Config;

namespace SuperSocket.Autofac
{
    public class AutofacCommandLoader<TCommand> : CommandLoaderBase<TCommand>
        where TCommand : class, ICommand
    {
        private readonly ILifetimeScope _lifetimeScope;

        private IAppServer _mAppServer;

        public AutofacCommandLoader(ILifetimeScope lifetimeScope)
        {
            _lifetimeScope = lifetimeScope;
        }

        public override bool Initialize(IRootConfig rootConfig, IAppServer appServer)
        {
            _mAppServer = appServer;
            TryLoadCommandsAssembly();
            return true;
        }

        public virtual bool TryLoadCommandsAssembly()
        {
            var commandAssemblies = new List<Assembly>();

            if (_mAppServer.GetType().Assembly != GetType().Assembly)
                commandAssemblies.Add(_mAppServer.GetType().Assembly);

            var commandAssembly = _mAppServer.Config.Options.GetValue("commandAssembly");

            if (!string.IsNullOrEmpty(commandAssembly))
            {
                OnError("The configuration attribute 'commandAssembly' is not in used, please try to use the child node 'commandAssemblies' instead!");
                return false;
            }

            if (_mAppServer.Config.CommandAssemblies != null && _mAppServer.Config.CommandAssemblies.Any())
            {
                try
                {
                    var definedAssemblies = AssemblyUtil.GetAssembliesFromStrings(_mAppServer.Config.CommandAssemblies.Select(a => a.Assembly).ToArray());

                    if (definedAssemblies.Any())
                        commandAssemblies.AddRange(definedAssemblies);
                }
                catch (Exception e)
                {
                    OnError(new Exception("Failed to load defined command assemblies!", e));
                    return false;
                }
            }

            if (!commandAssemblies.Any())
                commandAssemblies.Add(Assembly.GetEntryAssembly());

            var builder = new ContainerBuilder();
            builder.RegisterAssemblyTypes(commandAssemblies.ToArray());
            builder.Update(_lifetimeScope.ComponentRegistry);

            return true;
        }

        public override bool TryLoadCommands(out IEnumerable<TCommand> commands)
        {
            var targetType = typeof (TCommand);
            var outputCommands = new List<TCommand>();

            foreach (var registration in _lifetimeScope.ComponentRegistry.Registrations)
            {
                if (targetType.IsAssignableFrom(registration.Activator.LimitType))
                {
                    var instance = _lifetimeScope.Resolve(registration.Activator.LimitType) as TCommand;
                    if (instance != null)
                        outputCommands.Add(instance);
                }
            }
            commands = outputCommands;

            return true;
        }
    }
}