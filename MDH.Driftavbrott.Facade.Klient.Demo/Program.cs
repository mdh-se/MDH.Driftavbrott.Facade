using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using SE.MDH.DriftavbrottKlient;
using SE.MDH.Driftavbrott.Modell;

namespace DriftavbrottKlientTest
{
  /// <summary>Det här programmet testar använda DriftavbrottKlient för att fråga om driftavbrott på en testkanal som har namnet 'alltid'.</summary>
  public class Program
  {
    public static void Main(string[] args)
    {
      DriftavbrottKlient driftavbrottKlient=null;
      try
      {
        driftavbrottKlient = GetDriftavbrottKlient(args);        
      }
      catch (Exception e)
      {
        Console.WriteLine("Konfigurationsfel:");
        Console.WriteLine(e);
        Environment.Exit(-1);
      }

      try
      {
        IEnumerable<driftavbrottType> driftavbrott = driftavbrottKlient.GetPagaendeDriftavbrott(new[] { "alltid" }, "DriftavbrottKlient-Test");
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

      //try
      //{
      //  IEnumerable<driftavbrottType> driftavbrott = driftavbrottKlient.GetPagaendeDriftavbrott(new[] { "ladok.backup" });
      //  foreach (driftavbrottType driftavbrottType in driftavbrott)
      //  {
      //    Console.WriteLine($"[kanal={driftavbrottType.kanal}, start={driftavbrottType.start}, slut={driftavbrottType.slut}]");
      //  }
      //  driftavbrottKlient.Dispose();
      //}
      //catch (Exception e)
      //{
      //  Console.WriteLine(e.Message);
      //}

      Console.WriteLine();
      Console.WriteLine("Enter för att avsluta.");
      Console.ReadLine();
      Environment.Exit(0);
    }


    private static DriftavbrottKlient GetDriftavbrottKlient(string[] args)
    {
      if (args.Length == 0)
      {
          return new DriftavbrottKlient();
      }

      NameValueCollection config = new NameValueCollection();
      if (args.Length > 0)
      {
        foreach (var argument in args)
        {
          // leta efter url bland argumenten
          if (argument.ToLower().StartsWith("url="))
          {
            config["serviceUrl"] = argument.Substring(4);
          }
        }
      }

      if (config.Count < 1)
      {
        Console.WriteLine("Korrekt parameter saknas.");
        Console.WriteLine("Ange url som parameter");
        Console.WriteLine("Ex: MDH.Driftavbrott.Facade.Klient.Demo.exe url=https://localhost.mdu.se:3301/mdh-driftavbrott/v1");
        Console.WriteLine("Eller ange inga argument för att använda konfiguration från app.config filen.");
        Console.WriteLine("Ex: MDH.Driftavbrott.Facade.Klient.Demo.exe");
        Environment.Exit(-1);
      }
      
      return new DriftavbrottKlient(config);
    }
  }
}