/* 
 * This file is part of the whisper project.
 * Copyright (C) 2016  soulson (a.k.a. foxic)
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as
 * published by the Free Software Foundation, either version 3 of the
 * License, or (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 * 
 * You should have received a copy of the GNU Affero General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using Castle.MicroKernel.Registration;
using Castle.Windsor;
using log4net.Config;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Config;
using System;
using System.IO;
using Westwind.Utilities.Configuration;
using Whisper.Daemon.Shard.CLI;
using Whisper.Daemon.Shard.Config;
using Whisper.Daemon.Shard.Lookup;
using Whisper.Daemon.Shard.Net;
using Whisper.Game.World;
using Whisper.Shared.Database;
using Whisper.Shared.Net;
using Whisper.Shared.Security;

namespace Whisper.Daemon.Shard
{
    internal class Program
    {
        private const string ConfigurationFile = "./Config/shardd.config";
        private const string ConfigurationSection = "ShardConfig";
        private const string LogConfigurationFile = "./Config/log4net.config";

        public static void Main(string[] args)
        {
            using (IWindsorContainer container = new WindsorContainer())
            {
                XmlConfigurator.Configure(new FileInfo(args.Length < 2 ? LogConfigurationFile : args[1]));

                ShardConfig config = new ShardConfig();
                config.Initialize(new ConfigurationFileConfigurationProvider<ShardConfig>() { ConfigurationFile = args.Length < 1 ? ConfigurationFile : args[0], ConfigurationSection = ConfigurationSection });

                container.Register(Component.For<ShardConsole>());
                container.Register(Component.For<ShardConfig>().Instance(config));
                container.Register(Component.For<IRandomSource>().ImplementedBy<SecureRandom>());
                container.Register(Component.For<IWhisperComposerFactory<ShardRequest>>().ImplementedBy<ShardComposerFactory>());
                container.Register(Component.For<IWhisperRequestFactory<ShardRequest, ShardClientOpcode>>().ImplementedBy<ShardRequestFactory>());

                container.Register(Component.For<ShardServer>()
                    .DependsOn(Dependency.OnValue("wauth", new Datasource(config.AuthDatabaseAddress, config.AuthDatabasePort, config.AuthDatabaseName, config.AuthDatabaseUser, config.AuthDatabasePassword)))
                    .DependsOn(Dependency.OnValue("wshard", new Datasource(config.ShardDatabaseAddress, config.ShardDatabasePort, config.ShardDatabaseName, config.ShardDatabaseUser, config.ShardDatabasePassword)))
                );

                container.Register(Component.For<World>()
                    .DependsOn(Dependency.OnValue("wworld", new Datasource(config.WorldDatabaseAddress, config.WorldDatabasePort, config.WorldDatabaseName, config.WorldDatabaseUser, config.WorldDatabasePassword)))
                );
                container.Register(Component.For<Game.World.Shard>());

                ServerConfig serverConfig = new ServerConfig();
                serverConfig.Ip = config.ShardDaemonBindAddress;
                serverConfig.Port = config.ShardDaemonBindPort;
                serverConfig.MaxRequestLength = 2048;
                serverConfig.Mode = SocketMode.Tcp;
                serverConfig.IdleSessionTimeOut = 62;

                ShardConsole console = container.Resolve<ShardConsole>();
                console.Server.Setup(serverConfig);
                console.Server.Start();
                console.Run();
                console.Server.Stop();
            }
        }
    }
}
