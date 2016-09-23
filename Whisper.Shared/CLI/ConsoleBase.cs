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

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Whisper.Shared.Net;
using log4net;

namespace Whisper.Shared.CLI
{
    public abstract class ConsoleBase<TServer, TSession, TRequest>
        where TServer : IWhisperServer<TSession, TRequest>
        where TSession : IWhisperSession<TSession, TRequest>, new()
        where TRequest : class, IWhisperRequest
    {
        public const string ExitCommand = "exit";
        public const string HelpCommand = "help";

        private readonly ILog log = LogManager.GetLogger("Whisper.Shared.CLI.ConsoleBase");

        public ConsoleBase(TServer server)
        {
            Server = server;
            Commands = new Dictionary<string, IWhisperConsoleCommand<TServer, TSession, TRequest>>();

            Type commandType = typeof(IWhisperConsoleCommand<TServer, TSession, TRequest>);
            var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(assembly => assembly.GetTypes()).Where(type => commandType.IsAssignableFrom(type)).Where(type => !type.IsAbstract && !type.IsInterface);

            foreach(var handlerType in types)
            {
                IWhisperConsoleCommand<TServer, TSession, TRequest> commandHandler = (IWhisperConsoleCommand<TServer, TSession, TRequest>)Activator.CreateInstance(handlerType);
                Commands.Add(commandHandler.Name, commandHandler);
                log.DebugFormat("found console command handler for {0}", commandHandler.Name);
            }

            Commands = new ReadOnlyDictionary<string, IWhisperConsoleCommand<TServer, TSession, TRequest>>(Commands);
        }

        public IDictionary<string, IWhisperConsoleCommand<TServer, TSession, TRequest>> Commands
        {
            get;
            private set;
        }

        public TServer Server
        {
            get;
            private set;
        }

        public virtual void Run()
        {
            Console.WriteLine("console enabled. exit command is '{0}'. help command is '{1}'.", ExitCommand, HelpCommand);

            string command;
            do
            {
                command = Console.ReadLine();
                string[] split = command.Split(' ');

                if (split.Length > 0)
                {
                    if (split[0] == HelpCommand)
                        Help();
                    else if (Commands.ContainsKey(split[0]))
                        Commands[split[0]].ExecuteCommand(Server, split);
                }
            } while (command != ExitCommand);
        }

        protected virtual void Help()
        {
            foreach (var commandHandler in Commands)
                Console.WriteLine("{0} {1}", commandHandler.Value.Name, commandHandler.Value.Help);
        }
    }
}
