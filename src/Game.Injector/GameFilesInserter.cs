﻿using System.Text;
using CliWrap;
using Installer.Common.Framework.Extensions;
using Installer.Common.Logger;
using Installer.Common.Service;

namespace Game.Injector;

public sealed class GameFilesInserter : IGameFilesInserter
{
    private readonly InstallerServiceProvider _installerServiceProvider;
    private readonly ILogger _logger;

    private string _tempPath = string.Empty;
    private string _cryptSys = string.Empty;
    private string _cryptDlc = string.Empty;
    private string _msvcp100 = string.Empty;
    private string _msvcr100 = string.Empty;

    public GameFilesInserter(InstallerServiceProvider installerServiceProvider)
    {
        _installerServiceProvider = installerServiceProvider;
        _logger = LogManager.GetLogger();
    }

    public void Initializer(string tempPath)
    {
        _tempPath = tempPath;
        _cryptSys = Path.Combine(_installerServiceProvider.GameLocationInfo.SystemDirectory, "ffxiiicrypt.exe");
        _cryptDlc = Path.Combine(_installerServiceProvider.GameLocationInfo.DlcDirectory, "ffxiiicrypt.exe");
        _msvcp100 = Path.Combine(_installerServiceProvider.GameLocationInfo.SystemDirectory, "msvcp100.dll");
        _msvcr100 = Path.Combine(_installerServiceProvider.GameLocationInfo.SystemDirectory, "msvcr100.dll");

        File.Copy(sourceFileName: _tempPath + @"\ffxiiicrypt.exe", destFileName: _cryptSys, true);
        File.Copy(sourceFileName: _tempPath + @"\ffxiiicrypt.exe", destFileName: _cryptDlc, true);
        File.Copy(_tempPath + @"\msvcp100.dll", destFileName: _msvcp100, overwrite: true);
        File.Copy(_tempPath + @"\msvcr100.dll", _msvcr100, true);
    }

    public async Task Insert(string filelist, string whiteFile, string folder, string workingDir)
    {
        var stdOutBuffer = new StringBuilder(capacity: 5000);
        var stdErrBuffer = new StringBuilder();

        CommandTask<CommandResult> commandTask = Cli.Wrap(targetFilePath: _tempPath + @"\ff13tool.exe")
            .WithArguments(configure: args => args
                .Add(value: "-i").Add(value: "-all").Add(value: "-ff133")
                .Add(value: filelist).Add(value: whiteFile)
                .Add(value: Path.Combine(path1: _tempPath, path2: folder)))
            .WithWorkingDirectory(workingDirPath: workingDir)
            .WithStandardOutputPipe(target: PipeTarget.ToStringBuilder(stringBuilder: stdOutBuffer))
            .WithStandardErrorPipe(target: PipeTarget.ToStringBuilder(stringBuilder: stdErrBuffer))
            .WithValidation(validation: CommandResultValidation.None)
            .ExecuteAsync();

        await commandTask;

        if (stdOutBuffer.Length > 0) _logger.Info(stdOutBuffer.ToString());

        if (stdErrBuffer.Length > 0)
        {
            _logger.Error($"Houve um erro inesperado no arquivo {filelist}");
            _logger.Error(stdOutBuffer.ToString());
        }
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempPath))
            _tempPath.DeleteEvenWhenUsed();

        if (File.Exists(_cryptSys))
            _cryptSys.DeleteEvenWhenUsed();
        if (File.Exists(_cryptDlc))
            _cryptDlc.DeleteEvenWhenUsed();
        if (File.Exists(_msvcp100))
            _msvcp100.DeleteEvenWhenUsed();
        if (File.Exists(_msvcr100))
            _msvcr100.DeleteEvenWhenUsed();
    }
}