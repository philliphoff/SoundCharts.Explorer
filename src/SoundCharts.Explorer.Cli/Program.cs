using System.CommandLine;
using SoundCharts.Explorer.Cli.Commands.Tileset;

// Create a root command with some options
var rootCommand = new RootCommand
{
    new Command("tileset")
    {
        new ConvertCommand()
    }
};

rootCommand.Description = "SoundCharts Explorer CLI";

// Parse the incoming args and invoke the handler
return rootCommand.InvokeAsync(args).Result;
