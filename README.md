# MDH.DriftavbrottKlient

En .NET-klient som kan kommunicera med mdh-driftavbrott-service.

## Användning, konfiguration i appconfig

Här följer ett exempel på lite C#-kod:

```C#
    DriftavbrottKlient client = new DriftavbrottKlient();
    String[] kanaler = new [] { "ladok.backup" };
    try
    {
      IEnumerable<driftavbrottType> driftavbrott = driftavbrottKlient.GetPagaendeDriftavbrott(kanaler);
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
    DriftavbrottKlient client = new DriftavbrottKlient("server.domain", 23456, "mitt-system");
    String[] kanaler = new [] { "ladok.backup" };
    try
    {
      IEnumerable<driftavbrottType> driftavbrott = driftavbrottKlient.GetPagaendeDriftavbrott(kanaler);
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
    <section name="se.mdh.driftavbrottklient" type="SE.MDH.DriftavbrottKlient.Configurations.ConfigurationHandler, se.mdh.driftavbrottklient"/>
  </configSections>

  <!-- Exempelkonfiguration -->
  <se.mdh.driftavbrottklient server="localhost" port="3301" systemid="DriftavbrottKlient-Test" />
</configuration>
```
