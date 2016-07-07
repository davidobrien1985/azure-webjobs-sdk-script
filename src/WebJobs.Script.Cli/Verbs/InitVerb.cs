// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Colors.Net;
using Microsoft.Azure.WebJobs.Script;
using NCli;
using WebJobs.Script.Cli.Common;
using WebJobs.Script.Cli.Interfaces;
using static WebJobs.Script.Cli.Common.OutputTheme;

namespace WebJobs.Script.Cli.Verbs
{
    [Verb(HelpText = "Creates .gitignore, and host.json. Runs git init .")]
    internal class InitVerb : BaseVerb
    {
        [Option("vsc", DefaultValue = SourceControl.Git, HelpText = "Version Control Software. Git or Hg")]
        public SourceControl SourceControl { get; set; }

        internal readonly Dictionary<string, string> fileToContentMap = new Dictionary<string, string>
        {
            { ".gitignore",  @"
bin
obj
csx
.vs
edge
Publish
.vscode

*.user
*.suo
*.cscfg
*.Cache
project.lock.json

/packages
/TestResults

/tools/NuGet.exe
/App_Data
/secrets
/data
.secrets
"},
            { ScriptConstants.HostMetadataFileName, $"{{\"id\":\"{ Guid.NewGuid().ToString("N") }\"}}" },
            { SecretsManager.SecretsFilePath, string.Empty }
        };

        public InitVerb(ITipsManager tipsManager)
            : base(tipsManager)
        {
        }

        public override async Task RunAsync()
        {
            if (SourceControl != SourceControl.Git)
            {
                throw new Exception("Only Git is supported right now for vsc");
            }

            foreach (var pair in fileToContentMap)
            {
                if (!FileSystemHelpers.FileExists(pair.Key))
                {
                    ColoredConsole.WriteLine($"Writing {pair.Key}");
                    await FileSystemHelpers.WriteAllTextToFileAsync(pair.Key, pair.Value);
                }
                else
                {
                    ColoredConsole.WriteLine($"{pair.Key} already exists. Skipped!");
                }
            }

            var checkGitRepoExe = new Executable("git", "rev-parse --git-dir");
            var result = await checkGitRepoExe.RunAsync();
            if (result != 0)
            {
                var exe = new Executable("git", $"init");
                await exe.RunAsync(l => ColoredConsole.WriteLine(l), l => ColoredConsole.Error.WriteLine(l));
            }
            else
            {
                ColoredConsole.WriteLine("Directory already a git repository.");
            }
            _tipsManager.DisplayTips($"{TitleColor("Tip:")} run {ExampleColor("func new")} to create your first function.");
        }
    }
}
