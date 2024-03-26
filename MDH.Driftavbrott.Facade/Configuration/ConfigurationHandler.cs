#region Referenser

using System.Configuration;

#endregion
namespace SE.MDH.DriftavbrottKlient.Configuration
{
  /// <summary>Konfigurationshanterare</summary>
  internal class ConfigurationHandler : ConfigurationSection
  {
    /// <summary>Url</summary>
    [ConfigurationProperty("serviceUrl", IsRequired = true)]
    public string ServiceUrl
    {
      get => this["serviceUrl"].ToString();
      set => this["serviceUrl"] = value;
    }
  }
}
