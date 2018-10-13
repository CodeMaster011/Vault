using System;
using System.Collections.Generic;
using System.Linq;

namespace Consoles.Infrastructure {
    public class CommandInterpreter {
        public CommandInterpreterOptions Options { get; }
        public CommandInterpreter (CommandInterpreterOptions options) {
            Options = options;
        }

        public Dictionary<string, string> Interpret(string[] args) {
            var result = new Dictionary<string, string> ();
            foreach (var item in args) {
                string command = null;
                string value = item;
                var isCommand = false;
                foreach (var identifier in Options.CommandIdentifier) {
                    if (item.StartsWith (identifier)) {
                        // 
                        // var split = item.Split(Options.ValueSeperators, StringSplitOptions.RemoveEmptyEntries);
                        // if(split.Length > 0) {

                        // }
                        if (!Options.KeepCommandIdentifier)
                            command = item.Replace (identifier, string.Empty);
                        else
                            command = item;
                        value = null;
                        isCommand = true;
                        break;
                    }
                }
                if (isCommand)
                    result.Add (command, value);
                else {
                    var lastCommand = result.Keys?.LastOrDefault();
                    if (lastCommand == null) {
                        result.Add (isCommand ? command : value, null);
                        continue;
                    }
                    if (string.IsNullOrEmpty (result[lastCommand]))
                        result[lastCommand] = value;
                    else
                        result.Add (command, null);
                }
            }
            return result;
        }

        public List<InterpreterReadToken> InterpretTokenBase (string[] args) {
            var result = new List<InterpreterReadToken> ();
            foreach (var arg in args) {
                if(IsCommand(arg, out var token)) {
                    if(token == null)throw new InvalidOperationException("Token can not be null");
                    result.Add(token);
                } else {
                    token = result.LastOrDefault();
                    if(token == null)throw new InvalidOperationException("Last token can not be null. Invalid argument structure.");
                    token.SetValue(arg);
                    token.Freeze();
                }
            }
            return result;
        }

        protected bool IsCommand(string value, out InterpreterReadToken token) {
            foreach (var identifier in Options.CommandIdentifier) {
                if (value.StartsWith(identifier)) {
                    string restCommand = value.Replace(identifier, string.Empty);
                    string commandNamespace, parameter;
                    GrabNamespace(restCommand, out commandNamespace, out restCommand);
                    GrabParamenter(restCommand, out parameter, out restCommand);
                    var command = restCommand;
                    token = new InterpreterReadToken(identifier, commandNamespace, command, parameter, null);
                    return true;
                }
            }
            token = null;
            return false;
        }

        protected bool GrabNamespace(string value, out string commandNamespace, out string remaining) {
            var commandNamespaceIndex = value.LastIndexOf(".");
            if(commandNamespaceIndex > 0) {
                commandNamespace = value.Substring(0, commandNamespaceIndex);
                remaining = value.Substring(commandNamespaceIndex + 1);
                return true;
            }
            commandNamespace = null;
            remaining = value;
            return false;
        }

        protected bool GrabParamenter(string value, out string paramenter, out string remaining) {
            var parameterIndex = value.LastIndexOf("-");
            if (parameterIndex > 0) {
                paramenter = value.Substring(parameterIndex + 1);
                remaining = value.Substring(0, parameterIndex);
                return true;
            }
            paramenter = null;
            remaining = value;
            return false;
        }

        public static string[] GetArgsFromString(string s)
        {
            // AAA"BBBB CCCCC"DDDD EEEEE"FFF"GGG
            //  1      2          3       4   5
            var finalResult = new List<string>();
            var splitByCotation = s.Split(new[] { '"' });
            for (int i = 0; i < splitByCotation.Length; i++)
            {
                if((i + 1) % 2 == 0)
                {
                    finalResult.Add(splitByCotation[i]);
                }
                else
                {
                    var sp = splitByCotation[i].Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    finalResult.AddRange(sp);
                }
            }
            return finalResult.ToArray();
        }
    }

    public class CommandInterpreterOptions {
        public string[] ValueSeperators { get; set; } = new [] { "=", " " };
        public string[] CommandIdentifier { get; set; } = new [] { "--", "-" };
        public string[] SubCommandIdentifier { get; set; } = new [] { "-" };
        public bool KeepCommandIdentifier { get; set; }
    }

    public class InterpreterReadToken {
        private bool _isFreezed;
        public bool IsFreezed => _isFreezed;
        public bool CanFreeze => !_isFreezed;
        public string Value { get; private set;}
        public string Identifier { get; }
        public string Namespace { get; }
        public string Command { get; }
        public string Parameter { get; }

        public InterpreterReadToken (string identifier, string cmdCamespace, string command, string parameter, string value) {
            this.Parameter = parameter;
            this.Command = command;
            Identifier = identifier;
            this.Namespace = cmdCamespace;
            this.Value = value;
        }

        public void SetValue(string value) => Value = IsFreezed ? throw new InvalidOperationException("Object is already freezed.") : value;

        public void Freeze() => _isFreezed = IsFreezed ? throw new InvalidOperationException("Object is already freezed.") : true;

        public override string ToString() {
            return $"{Namespace}.{Command}-{Parameter}";
        }
    }
}

// console --command-subcommand parameters
// console --command-subcommand=parameters
// console --namespace.command value --namespace.command-parameter value1