﻿using Game.Injector;
using Installer.Common.Framework;
using Installer.Common.Localizations;
using Installer.Common.Models;
using Installer.Common.Service;
using Installer.Package;
using Installer.LightningReturnFF13.Shared.Interfaces;

namespace Installer.LightningReturnFF13.Shared.Classes;

public class Installer : ITranslationIntaller
{
    private readonly InstallerServiceProvider _installerServiceProvider;
    private readonly IGameFilesInserter _gameFilesInserter;
    private readonly IPackageReader _packageReader;
    private readonly IBackupProvider _backupProvider;

    public Installer(InstallerServiceProvider installerServiceProvider, IGameFilesInserter gameFilesInserter, IPackageReader packageReader, IBackupProvider backupProvider)
    {
        _installerServiceProvider = installerServiceProvider;
        _gameFilesInserter = gameFilesInserter;
        _packageReader = packageReader;
        _backupProvider = backupProvider;
    }
    public async Task Install(LoadingHandler progress)
    {
        if (string.IsNullOrEmpty(_installerServiceProvider.InstallerConfig.GameLocation))
        {
            throw new ServiceException(Localization.Instance.GameDirectoryNot, new FileNotFoundException());
        }

        await _backupProvider.Backup(progress);

        string tempPackage = await _packageReader.ReadPackage(progress);
        if (string.IsNullOrEmpty(tempPackage)) throw new DirectoryNotFoundException("Diretório de extração do pacote não encontrado!");

        progress.Start();
        progress.TotalSteps = _installerServiceProvider.FilesListLrff13.Count;

        _gameFilesInserter.Initializer(tempPackage);

        progress.Title = string.Format(Localization.Instance.TranslationInstallingLoadingTitle, Localization.Instance.LightningReturnFinalFantasy13);
        for (int i = 0; i < _installerServiceProvider.FilesListLrff13.Count; i++)
        {
            GameSysFiles gameSysFiles = _installerServiceProvider.FilesListLrff13[i];
            await _gameFilesInserter.Insert(fileList: gameSysFiles.FileList, whiteFile: gameSysFiles.WhiteFile, folder: gameSysFiles.FileFolder);
            progress.CurrentStep++;
        }

        progress.Finish();
    }
}