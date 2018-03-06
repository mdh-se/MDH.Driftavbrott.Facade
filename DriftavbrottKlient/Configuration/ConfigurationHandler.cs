#region Referenser

using System.Configuration;

#endregion
namespace SE.MDH.DriftavbrottKlient.Configuration
{
  /// <summary>Konfigurationshanterare</summary>
  internal class ConfigurationHandler : ConfigurationSection
  {
    /// <summary>Port</summary>
    [ConfigurationProperty("port", IsRequired = true)]
    public int Port
    {
      get { return (int)this["port"]; }
      set { this["port"] = value; }
    }

    /// <summary>Server</summary>
    [ConfigurationProperty("server", IsRequired = true)]
    public string Server
    {
      get { return (string)this["server"]; }
      set { this["server"] = value; }
    }

    /// <summary>Kännetecknar systemet som anropar mdh-driftavbrott-service</summary>
    [ConfigurationProperty("systemid", IsRequired = true)]
    public string SystemId
    {
      get { return (string)this["systemid"]; }
      set { this["systemid"] = value; }
    }
  }
}
