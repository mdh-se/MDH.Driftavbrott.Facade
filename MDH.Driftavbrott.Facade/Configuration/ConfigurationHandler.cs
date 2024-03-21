#region Referenser

using System.Configuration;

#endregion
namespace SE.MDH.DriftavbrottKlient.Configuration
{
  /// <summary>Konfigurationshanterare</summary>
  internal class ConfigurationHandler : ConfigurationSection
  {
    /// <summary>Url</summary>
    [ConfigurationProperty("url", IsRequired = true)]
    public string Url
    {
      get => this["url"].ToString();
      set => this["url"] = value;
    }
  }
}
