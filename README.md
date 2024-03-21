# MDH.Driftavbrott.Facade

En .NET-facade som kan kommunicera med mdh-driftavbrott-service.

## Användning, konfiguration i appconfig

Här följer ett exempel på lite C#-kod:

```C#
    DriftavbrottKlient client = new DriftavbrottKlient();
    String[] kanaler = new [] { "ladok.backup" };
    try
    {
      IEnumerable<driftavbrottType> driftavbrott = driftavbrottKlient.GetPagaendeDriftavbrott(kanaler, "mitt-systems-namn");
      foreach (driftavbrottType driftavbrottType in driftavbrott)
      {
        Console.WriteLine($"[kanal={driftavbrottType.kanal}, start={driftavbrottType.start}, slut={driftavbrottType.slut}]");
      }
      driftavbrottKlient.Dispose();
    }
    catch (Exception e)
    {
      Console.WriteLine(e.Message);
    }

```

## Användning, programatisk konfiguration

Här följer ett exempel på lite C#-kod:

```C#
    NameValueCollection config = new NameValueCollection();
    config.Add("url", "http://localhost:3301/mdh-driftavbrott/v1");

    DriftavbrottKlient client = new DriftavbrottKlient(config);
    String[] kanaler = new [] { "ladok.backup" };
    try
    {
      IEnumerable<driftavbrottType> driftavbrott = driftavbrottKlient.GetPagaendeDriftavbrott(kanaler, "mitt-systems-namn");
      foreach (driftavbrottType driftavbrottType in driftavbrott)
      {
        Console.WriteLine($"[kanal={driftavbrottType.kanal}, start={driftavbrottType.start}, slut={driftavbrottType.slut}]");
      }
      driftavbrottKlient.Dispose();
    }
    catch (Exception e)
    {
      Console.WriteLine(e.Message);
    }

```

## Konfigurering app.Config

Ange följande i applikationens konfiguration:

```XML
<configuration>
  <configSections>
      <section name="se.mdh.driftavbrott.service" type="SE.MDH.DriftavbrottKlient.Configuration.ConfigurationHandler, MDH.Driftavbrott.Facade"/>
  </configSections>

  <!-- Exempelkonfiguration -->
  <se.mdh.driftavbrott.service url="http://localhost:3301/mdh-driftavbrott/v1"/>

</configuration>
```
