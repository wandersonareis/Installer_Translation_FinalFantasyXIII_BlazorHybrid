﻿using Installer.Common;
using Installer.Common.Framework.Extensions;
using Installer.Common.GameLocation;
using Installer.Common.Localizations;
using Installer.Common.Logger;
using Installer.Common.Models;
using Installer.Common.Service;
using Installer.LightningReturnFF13.Shared.Interfaces;

namespace Installer.LightningReturnFF13.Shared.GameLocation;

public class GameLocationProvider : IInfoProvider
{
    private readonly InstallerServiceProvider _installerServiceProvider;
    private readonly ISteamGameLocation _steamGameLocation;
    private readonly IUserGameLocation _userGameLocation;
    private readonly ILogger _logger = LogManager.GetLogger();

    public GameLocationProvider(InstallerServiceProvider installerServiceProvider, ISteamGameLocation steamGameLocation, IUserGameLocation userGameLocation)
    {
        _installerServiceProvider = installerServiceProvider;
        _steamGameLocation = steamGameLocation;
        _userGameLocation = userGameLocation;
    }
    public bool Provider()
    {

        _installerServiceProvider.InstallerConfig = _installerServiceProvider.ConfigFile.FileIsExists() ?
        JsonHelper.DeserializeFromFile<InstallerConfig>(_installerServiceProvider.ConfigFile) : new InstallerConfig();

        if (_installerServiceProvider.InstallerConfig!.GameLocation.DirectoryIsExists())
        {
            IGameLocationInfo result = new GameLocationInfo(_installerServiceProvider.InstallerConfig.GameLocation);
            if (result.IsValidGamePath())
            {
                _logger.Info(Localization.Instance.GameLocationBySettings);
                _installerServiceProvider.GameLocationInfo = result;
                return true;
            }
        }

        Directory.CreateDirectory(@".\Config");
        JsonHelper.SerializeToFile(_installerServiceProvider.InstallerConfig, _installerServiceProvider.ConfigFile);

        return _steamGameLocation.Provider() || _userGameLocation.Provider();
    }
}