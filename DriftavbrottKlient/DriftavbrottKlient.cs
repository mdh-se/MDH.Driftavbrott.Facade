#region Referenser

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Text;

// Interna beroenden
using SE.MDH.DriftavbrottKlient.Configuration;
using SE.MDH.DriftavbrottKlient.Model;

// Externa beroenden
using FluentUri;
using RestSharp;

#endregion

namespace SE.MDH.DriftavbrottKlient
{
  /// <summary>Klient som används för att fråga efter driftavbrott.</summary>
  public class DriftavbrottKlient : IDisposable
  {
    #region Privata konstanter

    // Konstant för bas uri
    private const string BASE_URI = "mdh-driftavbrott/v1";
    // Konstanter mot REST-resurser och queryparametrar
    private const string PÅGÅENDE_PATH= "/driftavbrott/pagaende";
    private const string KANAL_PARAM = "kanal";
    private const string SYSTEM_PARAM = "system";

    #endregion

    #region privata medlemmar

    /// <summary>Server som används</summary>
    private string myServer;
    /// <summary>Port som används</summary>
    private int myPort;
    /// <summary>SystemId som används</summary>
    private string mySystemId;

    #endregion

    #region Publika egenskaper

    /// <summary>Indikerar att instansen är skrotad</summary>
    public bool Disposed { get; private set; }

    #endregion

    #region Kontruktor

    /// <summary>Kontruktor som används om man anger konfigurationen i App.Config</summary>
    /// <exception cref="ConfigurationErrorsException"></exception>
    public DriftavbrottKlient()
    {
      try
      {
        myServer = CurrentConfiguration.Server;
        myPort = CurrentConfiguration.Port;
        mySystemId = CurrentConfiguration.SystemId;
      }
      catch (Exception e)
      {
        throw new ConfigurationErrorsException("Felaktig konfiguration.", e);
      }
    }
    /// <summary>Kontruktor som används vid programatisk konfiguration.</summary>
    /// <exception cref="ArgumentNullException"></exception>
    public DriftavbrottKlient(string server, int port, string systemid)
    {
      if (String.IsNullOrEmpty(server))
      {
        throw new ArgumentNullException(nameof(server), "Server måste anges.");
      }
      if (String.IsNullOrEmpty(systemid))
      {
        throw new ArgumentNullException(nameof(systemid), "SystemId måste anges.");
      }
      myServer = server;
      myPort = port;
      mySystemId = systemid;
    }

    #endregion

    #region Publika metoder

    /// <summary>
    /// Hämtar pågående driftavbrott på de kanaler som anges. Om något
    /// annat fel inträffar kastas ett ApplicationException.
    /// </summary>
    /// <param name="kanaler">Samling kanaler vars driftavbrott ska hämtas</param>
    /// <returns>Ska endast returnera noll eller ett driftavbrott i praktiken</returns>
    /// <exception cref="ApplicationException"></exception>
    public IEnumerable<driftavbrottType> GetPagaendeDriftavbrott(IEnumerable<String> kanaler)
    {
      // Använder ett tredjeparts-lib för att snyggt bygga en URI
      FluentUriBuilder builder = FluentUriBuilder.Create()
          .Scheme(UriScheme.Http)
          .Host(myServer)
          .Port(myPort)
          .Path(BASE_URI + PÅGÅENDE_PATH);

      foreach (var kanal in kanaler)
      {
        builder = builder.QueryParam(KANAL_PARAM, kanal);
      }

      // Släng in systemId som parameter till tjänsten, som alltid är ett krav
      builder = builder.QueryParam(SYSTEM_PARAM, mySystemId);

      // REST client
      RestClient restClient = new RestClient
      {
        // BaseUrl (REST Service EndPoint adress)
        BaseUrl = builder.ToUri(),
        Encoding = Encoding.UTF8
      };

      // Request som skickas
      RestRequest restRequest = new RestRequest
      {
        // Metod (POST, GET, PUT, DELETE)
        // RequestFormat (XML, JSON)
        Method = Method.GET,
        RequestFormat = DataFormat.Xml
      };

      // Lägg till Header (endast XML accepteras)
      restRequest.AddHeader("Accept", "application/xml");

      // Gör anropet
      IRestResponse<driftavbrottType> restResponse = restClient.Execute<driftavbrottType>(restRequest);
      if (restResponse.IsSuccessful)
      {
        // Servern returnerad inga driftavbrott.
        if (restResponse.StatusCode == HttpStatusCode.NotFound)
        {
          return Enumerable.Empty<driftavbrottType>();
        }
        // Servern returnerade driftavbrott.
        if (restResponse.StatusCode == HttpStatusCode.OK)
        {
          if (restResponse.Data == null)
          {
            return Enumerable.Empty<driftavbrottType>();
          }
          else
          {
            return new[] { restResponse.Data };
          }
        }
        throw new ApplicationException($"Oväntat fel inträffade. ResponseCode={restResponse.StatusCode}, ResponseServer={restResponse.Server}, RequestBaseUrl={restClient.BaseHost}{restClient.BaseUrl}.");
      }
      throw new ApplicationException($"Oväntat fel inträffade. ResponseCode={restResponse.StatusCode}, ResponseServer={restResponse.Server}, RequestBaseUrl={restClient.BaseHost}{restClient.BaseUrl}.");
    }

    #endregion
    
    #region IDisposable

    /// <summary>Skrotar instansen och frigör resurser</summary>
    public void Dispose()
    {
      if (!Disposed)
      {
        Dispose(true);
      }
      GC.SuppressFinalize(this);
    }

    /// <summary>Skrotar instansen och frigör resurser</summary>
    protected virtual void Dispose(bool doIt)
    {
      if (doIt)
      {
        Disposed = true;
      }
    }

    /// <summary>Dekonstruktor</summary>
    ~DriftavbrottKlient()
    {
      Dispose(false);
    }

    #endregion
  }
}