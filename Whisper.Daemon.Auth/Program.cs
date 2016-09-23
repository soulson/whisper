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
using System;
using System.IO;
using Westwind.Utilities.Configuration;
using Whisper.Daemon.Auth.CLI;
using Whisper.Daemon.Auth.Config;
using Whisper.Daemon.Auth.Lookup;
using Whisper.Daemon.Auth.Net;
using Whisper.Shared.Database;
using Whisper.Shared.Net;
using Whisper.Shared.Security;

namespace Whisper.Daemon.Auth
{
    internal class Program
    {
        private const string ConfigurationFile = "./Config/authd.config";
        private const string ConfigurationSection = "AuthConfig";
        private const string LogConfigurationFile = "./Config/log4net.config";

        public static void Main(string[] args)
        {
            using (IWindsorContainer container = new WindsorContainer())
            {
                XmlConfigurator.Configure(new FileInfo(args.Length < 2 ? LogConfigurationFile : args[1]));

                AuthConfig config = new AuthConfig();
                config.Initialize(new ConfigurationFileConfigurationProvider<AuthConfig>() { ConfigurationFile = args.Length < 1 ? ConfigurationFile : args[0], ConfigurationSection = ConfigurationSection });

                container.Register(Component.For<AuthConsole>());
                container.Register(Component.For<AuthServer>());
                container.Register(Component.For<AuthConfig>().Instance(config));
                container.Register(Component.For<IRandomSource>().ImplementedBy<SecureRandom>());
                container.Register(Component.For<IWhisperComposerFactory<AuthRequest>>().ImplementedBy<AuthComposerFactory>());
                container.Register(Component.For<IWhisperRequestFactory<AuthRequest, AuthRequestOpcode>>().ImplementedBy<AuthRequestFactory>());

                container.Register(Component.For<IWhisperDatasource>().ImplementedBy<Datasource>()
                    .DependsOn(Dependency.OnValue("address", config.AuthDatabaseAddress))
                    .DependsOn(Dependency.OnValue("port", config.AuthDatabasePort))
                    .DependsOn(Dependency.OnValue("database", config.AuthDatabaseName))
                    .DependsOn(Dependency.OnValue("username", config.AuthDatabaseUser))
                    .DependsOn(Dependency.OnValue("password", config.AuthDatabasePassword))
                );

                AuthConsole console = container.Resolve<AuthConsole>();
                console.Server.Setup(config.AuthDaemonBindAddress, config.AuthDaemonBindPort);
                console.Server.Start();
                console.Run();
                console.Server.Stop();
            }
        }
    }
}
