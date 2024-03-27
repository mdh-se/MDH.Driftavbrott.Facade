#region Referenser

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;

// Interna beroenden
using SE.MDH.DriftavbrottKlient.Configuration;
using SE.MDH.Driftavbrott.Modell;

// Externa beroenden
using RestSharp;

#endregion

namespace SE.MDH.DriftavbrottKlient
{
  /// <summary>Klient som används för att fråga efter driftavbrott.</summary>
  public class DriftavbrottKlient : IDisposable
  {
    #region Privata konstanter

    // Konstanter mot REST-resurser och queryparametrar
    private const string PÅGÅENDE_PATH= "/driftavbrott/pagaende";
    private const string KANAL_PARAM = "kanal";
    private const string SYSTEM_PARAM = "system";

    #endregion

    #region privata medlemmar

    /// <summary>
    /// Senaste svaret från driftavbrotttjänsten.
    /// </summary>
    private IEnumerable<driftavbrottType> gällandeSvarFrånDriftavbrott = new List<driftavbrottType>();

    /// <summary>
    /// Tidpunkten för senaste lyckade frågan till driftavbrottstjänsten.
    /// </summary>
    private DateTime senastFråganTillDriftavbrott = DateTime.Now.AddMinutes(-1);

    /// <summary>Url till den service som används</summary>
    private string myServiceUrl;

    private RestClient _restClient;
    private RestClient restClient
    {
      get
      {
        return _restClient ?? (_restClient = new RestClient(myServiceUrl));
      }
    }
    
    #endregion

    #region Publika egenskaper

    /// <summary>Indikerar att instansen är skrotad</summary>
    public bool Disposed { get; private set; }

    /// <summary>Url till den service som används, fast offentligt tillgänglig</summary>
    public string ServiceUrl => myServiceUrl;

    #endregion

    #region Konstruktor

    /// <summary>Konstruktor som används om man anger konfigurationen i App.Config</summary>
    /// <exception cref="ConfigurationErrorsException"></exception>
    public DriftavbrottKlient()
    {
      try
      {
        myServiceUrl = CurrentConfiguration.ServiceUrl;
      }
      catch (Exception e)
      {
        throw new ConfigurationErrorsException("Felaktig konfiguration.", e);
      }
    }
    /// <summary>
    /// Konstruktor som används vid programatisk konfiguration.
    /// </summary>
    /// <param name="config">Samling med konfigurationsparametrar, måste innehålla "url".</param>
    /// <exception cref="ConfigurationErrorsException">Kastas när det finns ett fel i konfigurationen.</exception>
    public DriftavbrottKlient(NameValueCollection config)
    {
      try
      {
        myServiceUrl = config["serviceUrl"];
      }
      catch (Exception e)
      {
        throw new ConfigurationErrorsException($"Felaktig konfiguration.", e);
      }
    }

    #endregion

    #region Publika metoder

    /// <summary>
    /// Hämtar pågående driftavbrott på de kanaler som anges. Om något annat fel inträffar kastas ett ApplicationException.
    /// </summary>
    /// <param name="kanaler">Samling kanaler vars driftavbrott ska hämtas</param>
    /// <param name="system">Namnet på den anropande komponenten</param>
    /// <returns>Ska endast returnera noll eller ett driftavbrott i praktiken</returns>
    /// <exception cref="ApplicationException"></exception>
    public IEnumerable<driftavbrottType> GetPagaendeDriftavbrott(IEnumerable<String> kanaler, String system)
    {
      if (senastFråganTillDriftavbrott < DateTime.Now)
      {
        // Request som skickas
        RestRequest restRequest = new RestRequest(PÅGÅENDE_PATH.TrimStart('/'), Method.Get);
        restRequest.AddHeader("Accept", "application/xml,application/json");
        restRequest.RequestFormat = DataFormat.Xml;

        // lägg till parametrarna
        foreach (var kanal in kanaler)
        {
          restRequest.AddParameter(KANAL_PARAM, kanal);
        }
        restRequest.AddParameter(SYSTEM_PARAM, system);
        
        // Gör anropet
        RestResponse<driftavbrottType> restResponse = restClient.Execute<driftavbrottType>(restRequest);
        
        // Fick vi något svar alls?
        if (restResponse != null)
        {
          if (restResponse.IsSuccessful)
          {
            // Sätter tidpunkten för senaste frågan.
            senastFråganTillDriftavbrott = DateTime.Now.AddMinutes(1);

            // Hämta HTTP statuskoden i numerisk form (ex: 200)
            Int32 numericStatusCode = (Int32)restResponse.StatusCode;

            // Servern returnerade 404 eller 406 (HTTP Statuskod=404)
            if (restResponse.StatusCode == HttpStatusCode.NotFound)
            {
              throw new ApplicationException($"#Driftavbrottstjänsten returnerade 404/406. ResponseCode={numericStatusCode} {restResponse.StatusCode}, ResponseServer={restResponse.Server}, RequestBaseUrl={restClient.Options.BaseHost}{restClient.Options.BaseUrl}.", new HttpException(404, "File Not Found"));
            }

            // Servern returnerade inga driftavbrott alls (HTTP Statuskod=204, innehåll saknas)
            if (restResponse.StatusCode == HttpStatusCode.NoContent)
            {
              gällandeSvarFrånDriftavbrott = Enumerable.Empty<driftavbrottType>();
              return gällandeSvarFrånDriftavbrott;
            }
            // Servern returnerade eventuella driftavbrott (HTTP Statuskod=200)
            if (restResponse.StatusCode == HttpStatusCode.OK)
            {
              // Sätter gällande svar till svar eller tomt om vi inte fick någon data.
              gällandeSvarFrånDriftavbrott = restResponse.Data == null ? Enumerable.Empty<driftavbrottType>() : new[] { restResponse.Data };
              return gällandeSvarFrånDriftavbrott;
            }
            // Servern returnerade någon form av annan statuskod som ej behandlas specifikt
            throw new ApplicationException($"#Driftavbrottstjänsten returnerade en oväntad statuskod. ResponseCode={numericStatusCode} {restResponse.StatusCode}, ResponseServer={restResponse.Server}, RequestBaseUrl={restClient.Options.BaseHost}{restClient.Options.BaseUrl}.");
          }
          throw new ApplicationException($"Driftavbrottstjänsten svarar inte. ResponseServer={restResponse.Server}, RequestBaseUrl={restClient.Options.BaseHost}{restClient.Options.BaseUrl}.");
        }

        // Servern returnerade inget svar (Response) alls
        throw new ApplicationException($"#Fick inget svar från driftavbrottstjänsten. RequestBaseUrl={restClient.Options.BaseHost}{restClient.Options.BaseUrl}.");
      }
      // returnerar senaste svaret.
      return gällandeSvarFrånDriftavbrott;
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