using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Consoles.Infrastructure
{
    public class CommandCompletionGenerator
    {
        public const string FolderPath = @"/etc/bash_completion.d/";

        public string AppName { get; }
        public ApplicationPipline Pipeline { get; }

        public CommandCompletionGenerator(string appName, ApplicationPipline pipeline)
        {
            AppName = appName;
            Pipeline = pipeline;
        }

        public Task Generate() {
            var filePath = Path.Combine(FolderPath, AppName);
            var data = new StringBuilder();
            data.Append($"__{AppName} ");
            data.Append(@"() 
{
    local cur prev opts
    COMPREPLY=()
");
            data.AppendLine("cur=\"${COMP_WORDS[COMP_CWORD]}\"");
            data.AppendLine("prev=\"${COMP_WORDS[COMP_CWORD-1]}\"");
            data.Append("opts=\"");
            foreach (var command in Pipeline.GetPipline())
            {
                data.Append($"{command.Name} ");
                if(!string.IsNullOrEmpty(command.Alias))
                    data.Append($"{command.Alias} ");
            }
            data.Remove(data.Length - 1, 1); // remove the extras space

            data.AppendLine("\"");
            data.Append(@"if [[ ${cur} == -* ]] ; then
");
    data.Append("COMPREPLY=( $(compgen -W \"${opts}\" -- ${cur}) )");
    data.Append(@"
return 0
fi
}
complete -F ");
            data.Append($"__{AppName} {AppName}");

            File.WriteAllText(filePath, data.ToString());
            return Task.CompletedTask;
        }
    }
}

/*
You'll have to create a new file:

/etc/bash_completion.d/foo
For a static autocompletion (--help / --verbose for instance) add this:

_foo() 
{
    local cur prev opts
    COMPREPLY=()
    cur="${COMP_WORDS[COMP_CWORD]}"
    prev="${COMP_WORDS[COMP_CWORD-1]}"
    opts="--help --verbose --version"

    if [[ ${cur} == -* ]] ; then
        COMPREPLY=( $(compgen -W "${opts}" -- ${cur}) )
        return 0
    fi
}
complete -F _foo foo
COMP_WORDS is an array containing all individual words in the current command line.
COMP_CWORD is an index of the word containing the current cursor position.
COMPREPLY is an array variable from which Bash reads the possible completions.
And the compgen command returns the array of elements from --help, --verbose and --version matching the current word "${cur}":

compgen -W "--help --verbose --version" -- "<userinput>"
Source : http://www.debian-administration.org/articles/316
 */