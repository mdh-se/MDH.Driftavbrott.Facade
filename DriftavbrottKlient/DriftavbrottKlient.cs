#region Referenser

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;

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

    private bool myHttps;

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
        myHttps = CurrentConfiguration.Https;
      }
      catch (Exception e)
      {
        throw new ConfigurationErrorsException("Felaktig konfiguration.", e);
      }
    }
    /// <summary>Kontruktor som används vid programatisk konfiguration.</summary>
    /// <param name="server">Server</param>
    /// <param name="port">Port</param>
    /// <param name="systemid">Systemets namn</param>
    /// <param name="https">Https</param>
    /// <exception cref="ArgumentNullException"></exception>
    public DriftavbrottKlient(string server, int port, string systemid, bool https = false)
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
      myHttps = https;
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
          .Scheme(myHttps ? UriScheme.Https : UriScheme.Http)
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
      restRequest.AddHeader("Accept", "application/xml,application/json");

      // Gör anropet
      IRestResponse<driftavbrottType> restResponse = restClient.Execute<driftavbrottType>(restRequest);

      // Fick vi något svar alls?
      if(restResponse !=null)
      {
        // Hämta HTTP statuskoden i numerisk form (ex: 200)
        Int32 numericStatusCode = (Int32) restResponse.StatusCode;

        // Servern returnerade 404 eller 406 (HTTP Statuskod=404)
        if (restResponse.StatusCode == HttpStatusCode.NotFound)
        {
          throw new ApplicationException($"#Ett fel inträffade. ResponseCode={numericStatusCode} {restResponse.StatusCode}, ResponseServer={restResponse.Server}, RequestBaseUrl={restClient.BaseHost}{restClient.BaseUrl}.", new HttpException(404, "File Not Found"));
        }

        // Servern returnerade inga driftavbrott alls (HTTP Statuskod=204, innehåll saknas)
        if (restResponse.StatusCode == HttpStatusCode.NoContent)
        {
          return Enumerable.Empty<driftavbrottType>();
        }

        // Servern returnerade eventuella driftavbrott (HTTP Statuskod=200)
        if (restResponse.StatusCode == HttpStatusCode.OK)
        {
          // Data saknas, inga driftavbrott finns
          if (restResponse.Data == null)
          {
            return Enumerable.Empty<driftavbrottType>();
          }
          return new[] { restResponse.Data };
        }

        // Servern returnerade någon form av annan statuskod som ej behandlas specifikt
        throw new ApplicationException($"#Ett fel inträffade. ResponseCode={numericStatusCode} {restResponse.StatusCode}, ResponseServer={restResponse.Server}, RequestBaseUrl={restClient.BaseHost}{restClient.BaseUrl}.");
      }

      // Servern returnerade inget svar (Response) alls
      throw new ApplicationException($"#Ett oväntat fel inträffade. Inget svar finns att behandla. RequestBaseUrl={restClient.BaseHost}{restClient.BaseUrl}.");
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