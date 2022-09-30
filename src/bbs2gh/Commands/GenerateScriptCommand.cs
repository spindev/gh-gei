using System;
using System.CommandLine;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using OctoshiftCLI.BbsToGithub.Handlers;
using OctoshiftCLI.Commands;
using OctoshiftCLI.Contracts;

namespace OctoshiftCLI.BbsToGithub.Commands;

public class GenerateScriptCommand : CommandBase<GenerateScriptCommandArgs, GenerateScriptCommandHandler>
{
    public GenerateScriptCommand() : base(
            name: "generate-script",
            description: "Generates a migration script. This provides you the ability to review the steps that this tool will take, and optionally modify the script if desired before running it.")
    {
        AddOption(BbsServerUrl);
        AddOption(GithubOrg);
        AddOption(BbsUsername);
        AddOption(BbsPassword);
        AddOption(SshUser);
        AddOption(SshPrivateKey);
        AddOption(SshPort);
        AddOption(Output);
        AddOption(Verbose);
    }

    public Option<string> BbsServerUrl { get; } = new(
        name: "--bbs-server-url",
        description: "The full URL of the Bitbucket Server/Data Center to migrate from.")
    { IsRequired = true };

    public Option<string> GithubOrg { get; } = new("--github-org")
    { IsRequired = true };

    public Option<string> BbsUsername { get; } = new(
        name: "--bbs-username",
        description: "The Bitbucket username of a user with site admin privileges to get the list of all projects and their repos. If not set will be read from BBS_USERNAME environment variable.");

    public Option<string> BbsPassword { get; } = new(
        name: "--bbs-password",
        description: "The Bitbucket password of a user with site admin privileges to get the list of all projects and their repos. If not set will be read from BBS_PASSWORD environment variable." +
                      $"{Environment.NewLine}" +
                      "Note: The password will not get included in the generated script and it has to be set as an env variable before running the script.");

    public Option<string> SshUser { get; } = new(
        name: "--ssh-user",
        description: "The SSH user to be used for downloading the export archive off of the Bitbucket server.")
    { IsRequired = true };

    public Option<string> SshPrivateKey { get; } = new(
        name: "--ssh-private-key",
        description: "The full path of the private key file to be used for downloading the export archive off of the Bitbucket Server using SSH/SFTP.")
    { IsRequired = true };

    public Option<int> SshPort { get; } = new(
        name: "--ssh-port",
        description: "The SSH port (default: 22).",
        getDefaultValue: () => 22);

    public Option<FileInfo> Output { get; } = new(
        name: "--output",
        getDefaultValue: () => new FileInfo("./migrate.ps1"));

    public Option<bool> Verbose { get; } = new("--verbose");

    public override GenerateScriptCommandHandler BuildHandler(GenerateScriptCommandArgs args, ServiceProvider sp)
    {
        if (args is null)
        {
            throw new ArgumentNullException(nameof(args));
        }

        if (sp is null)
        {
            throw new ArgumentNullException(nameof(sp));
        }

        var log = sp.GetRequiredService<OctoLogger>();
        var versionProvider = sp.GetRequiredService<IVersionProvider>();
        var fileSystemProvider = sp.GetRequiredService<FileSystemProvider>();
        var environmentVariableProvider = sp.GetRequiredService<EnvironmentVariableProvider>();

        var bbsApiFactory = sp.GetRequiredService<BbsApiFactory>();
        var bbsApi = bbsApiFactory.Create(args.BbsServerUrl, args.BbsUsername, args.BbsPassword);

        return new GenerateScriptCommandHandler(log, versionProvider, fileSystemProvider, bbsApi, environmentVariableProvider);
    }
}

public class GenerateScriptCommandArgs
{
    public string BbsServerUrl { get; set; }
    public string GithubOrg { get; set; }
    public string BbsUsername { get; set; }
    public string BbsPassword { get; set; }
    public string SshUser { get; set; }
    public string SshPrivateKey { get; set; }
    public string SshPort { get; set; }
    public FileInfo Output { get; set; }
    public bool Verbose { get; set; }
}