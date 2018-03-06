#region Referenser

using System;
using System.Configuration;

#endregion

namespace SE.MDH.DriftavbrottKlient.Configuration
{
  /// <summary>Konfiguration</summary>
  internal static class CurrentConfiguration
  {
    #region Privata konstanter

    /// <summary>Sektion i konfigurationsfilen</summary>
    private const string KONFIGURATIONS_SEKTION = "se.mdh.driftavbrottklient";
    #endregion

    #region privata medlemmar

    /// <summary>Konfigurationen</summary>
    private static ConfigurationHandler myConfiguration;

    #endregion

    #region publika egenskaper

    /// <summary>Port</summary>
    public static int Port => myConfiguration.Port;
    /// <summary>Server</summary>
    public static string Server => myConfiguration.Server;
    /// <summary>System</summary>
    public static string SystemId => myConfiguration.SystemId;

    #endregion

    #region Konstruktor

    /// <summary>Konstruktor</summary>
    static CurrentConfiguration()
    {
      try
      {
        LoadConfiguration();
      }
      catch (Exception ex)
      {
        throw new ConfigurationErrorsException("Kan inte ladda konfigurationen.", ex);
      }
    }

    #endregion

    #region Privata metoder

    /// <summary>Ladda konfiguration från App.config</summary>
    private static void LoadConfiguration()
    {
      myConfiguration = (ConfigurationHandler)ConfigurationManager.GetSection(KONFIGURATIONS_SEKTION);
      if (myConfiguration == null)
      {
        System.Configuration.Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
        foreach (ConfigurationSection section in configuration.Sections)
        {
          if (section.GetType() == typeof(ConfigurationHandler))
          {
            myConfiguration = (ConfigurationHandler)section;
          }
        }
      }
    }

    #endregion
  }
}