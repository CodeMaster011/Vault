using System;
using System.Threading.Tasks;

namespace Consoles.Infrastructure
{
    public abstract class Command
    {
        public string Name { get; }
        public string Alias { get; }
        public string CommandNamespace { get; }

        public Command(string name) : this(name, null)
        {
            
        }
        public Command(string name, string alias, string commandNamespace = null)
        {
            Name = name;
            Alias = alias;
            CommandNamespace = commandNamespace;
        }

        public virtual Task Execute(string parameters, Func<Task> next) => Task.CompletedTask;

        public virtual Task Execute(InterpreterReadToken token, Func<Task> next) => Execute(token.Value, next);

        public virtual bool CanExecute(InterpreterReadToken token) {
            if((!string.IsNullOrEmpty(CommandNamespace) || !string.IsNullOrEmpty(token.Namespace)) 
                && CommandNamespace != token.Namespace)
                return false;
            if(string.IsNullOrEmpty(token.Command))
                throw new InvalidOperationException("Null command in token.");
            if(!string.IsNullOrEmpty(Alias) && Alias.Equals($"{token.Identifier}{token.Command}", StringComparison.OrdinalIgnoreCase))
                return true;
            return Name.Equals($"{token.Identifier}{token.Command}", StringComparison.OrdinalIgnoreCase);
        }
    }

    public class DelegateCommand : Command
    {
        public DelegateCommand(string name, Func<string, Func<Task>, Task> executeHandler) : base(name)
        {
            ExecuteHandler = executeHandler;
        }

        public DelegateCommand(string name, string alias, Func<string, Func<Task>, Task> executeHandler) : base(name, alias)
        {
            ExecuteHandler = executeHandler;
        }

        public Func<string, Func<Task>, Task> ExecuteHandler { get; }

        public override Task Execute(string parameters, Func<Task> next) {
            return ExecuteHandler.Invoke(parameters, next);
        }
    }
}