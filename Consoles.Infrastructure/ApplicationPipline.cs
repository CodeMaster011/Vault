using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Consoles.Infrastructure
{
    public class ApplicationPipline
    {
        private readonly List<Command> commands = new List<Command>();

        public ApplicationPipline Add(Command command) {
            commands.Add(command);
            return this;
        }

        public Command[] GetPipline() => commands.ToArray();

        public Task Execute(Dictionary<string, string> commandData) {
            var executionPipline = GetExecutionPipline(commandData);
            return Execute(executionPipline, 0);
        }

        public Task Execute(List<InterpreterReadToken> commandData) {
            var executionPipline = GetExecutionPiplineFromToken(commandData);
            return Execute(executionPipline, 0);
        }

        protected Task Execute(List<ExecutionData> executionPipline, int currentIndex) {
            if (currentIndex >= executionPipline.Count)
                return Task.CompletedTask; // break on last index automatically
            var executionData = executionPipline[currentIndex];
            return executionData.Command.Execute(executionData.Parameter, 
                () => Execute(executionPipline, ++currentIndex));
        }

        protected List<ExecutionData> GetExecutionPipline(Dictionary<string, string> commandData) {
            var executionData = new List<ExecutionData>();
            foreach (var command in commands)
            {
                if(commandData.ContainsKey(command.Name))
                    executionData.Add(new ExecutionData(command, commandData[command.Name]));
                else if(!string.IsNullOrEmpty(command.Alias) && commandData.ContainsKey(command.Alias))
                    executionData.Add(new ExecutionData(command, commandData[command.Alias]));
            }
            return executionData;
        }

        protected List<ExecutionData> GetExecutionPiplineFromToken(List<InterpreterReadToken> tokens) {
            var executionData = new List<ExecutionData>();
            foreach (var token in tokens)
            {
                foreach (var command in commands)
                {
                    if(command.CanExecute(token))
                        executionData.Add(new ExecutionData(command, token.Value, token));
                }
            }
            return executionData;
        }

        public class ExecutionData 
        {
            public ExecutionData(Command command, string parameter, InterpreterReadToken readerToken = null)
            {
                Command = command;
                Parameter = parameter;
                ReaderToken = readerToken;
            }

            public Command Command { get; }
            public string Parameter { get; }
            public InterpreterReadToken ReaderToken { get; }
        }
    }

    public static class ApplicationPiplineExtensions
    {
        public static ApplicationPipline Add(this ApplicationPipline pipline, string name, Func<string, Func<Task>, Task> executeHandler) {
            return pipline.Add(new DelegateCommand(name, executeHandler));
        }

        public static ApplicationPipline Add(this ApplicationPipline pipline, string name, string alias, Func<string, Func<Task>, Task> executeHandler) {
            return pipline.Add(new DelegateCommand(name, alias, executeHandler));
        }
    }
}