using System;
using System.Collections.Generic;
using SE.MDH.DriftavbrottKlient;
using SE.MDH.DriftavbrottKlient.Model;

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
        IEnumerable<driftavbrottType> driftavbrott = driftavbrottKlient.GetPagaendeDriftavbrott(new[] { "alltid" });
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

      try
      {
        IEnumerable<driftavbrottType> driftavbrott = driftavbrottKlient.GetPagaendeDriftavbrott(new[] { "ladok.backup" });
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

      Console.WriteLine();
      Console.WriteLine("Enter för att avsluta.");
      Console.ReadLine();
      Environment.Exit(0);
    }


    private static DriftavbrottKlient GetDriftavbrottKlient(string[] args)
    {
      if (args.Length == 1)
      {
        if (args[0].ToLower().Equals("noconfig"))
        {
          return new DriftavbrottKlient();
        }
      }
      if (args.Length < 2)
      {
        Console.WriteLine("Parameter saknas.");
        Console.WriteLine("Ange server och port");
        Console.WriteLine("Ex: DriftavbrottKlientTest.exe server.domain 2345");
        Console.WriteLine("Eller ange noconfig för att använda konfiguration från app.config filen.");
        Console.WriteLine("Ex: DriftavbrottKlientTest.exe noconfig");
        Environment.Exit(-1);
      }
      if (string.IsNullOrEmpty(args[0]) | string.IsNullOrEmpty(args[1]))
      {
        Console.WriteLine("Parameter saknas.");
        Console.WriteLine("Ange server och port");
        Console.WriteLine("Ex: DriftavbrottKlientTest.exe server.domain 2345");
        Console.WriteLine("Eller ange noconfig för att använda konfiguration från app.config filen.");
        Console.WriteLine("Ex: DriftavbrottKlientTest.exe noconfig");
        Environment.Exit(-1);
      }
      int port;
      if (!Int32.TryParse(args[1], out port))
      {
        Console.WriteLine("Felaktig parameter.");
        Console.WriteLine("Ange server och port");
        Console.WriteLine("Ex: DriftavbrottKlientTest.exe server.domain 2345");
        Environment.Exit(-1);
      }
      return new DriftavbrottKlient(args[0], port, "DriftavbrottKlient-Test");
    }
  }
}