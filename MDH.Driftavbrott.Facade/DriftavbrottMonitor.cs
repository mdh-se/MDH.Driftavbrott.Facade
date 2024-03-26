using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using SE.MDH.Driftavbrott.Modell;

namespace SE.MDH.DriftavbrottKlient
{
  /// <summary>
  /// Monitor som etablerar händelser när ett driftavbrott startar eller slutar på någon av angivna kanaler.
  /// </summary>
  public class DriftavbrottMonitor : IDisposable
  {
    #region medlemmar

    /// <summary>
    /// Arbetsklassen
    /// </summary>
    private BgWorker workerClass;
    /// <summary>
    /// Arbetstråden
    /// </summary>
    private Thread workerThread;
    /// <summary>
    /// Händelsedelegat
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    public delegate void DriftavbrottStatusHandler(object sender, DriftavbrottStatusEvent args);
    /// <summary>
    /// Status event
    /// </summary>
    public event DriftavbrottStatusHandler DriftavbrottStatus;
    /// <summary>
    /// Händelsedelegat
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    public delegate void ErrorOccurredHandler(object sender, ErrorEvent args);
    /// <summary>
    /// Error event
    /// </summary>
    public event ErrorOccurredHandler ErrorOccurred;

    #endregion

    #region Egenskaper

    /// <summary>
    /// Indikerar att instansen skrotats.
    /// </summary>
    public bool Disposed { get; private set; }

    /// <summary>Url till den rest-api som används</summary>
    public string ServiceUrl => workerClass.ServiceUrl;
    
    #endregion

    #region konstruktor

    /// <summary>
    /// Skapar instansen
    /// </summary>
    /// <param name="kanaler">Driftavbrottskanaler</param>
    /// <param name="system">Namnet på den anropande komponenten</param>
    public DriftavbrottMonitor(IEnumerable<string> kanaler, string system)
    {
      workerClass = new BgWorker(new DriftavbrottKlient(), kanaler, system);
      workerClass.DriftavbrottStatus += workerClassDriftavbrottStatus;
      workerClass.ErrorOccurred += workerClassOnErrorOccurred;
      workerThread = new Thread(workerClass.Start);
      workerThread.Start();
      while (workerThread.IsAlive != true) { }
    }

    /// <summary>
    /// Skapar instansen
    /// </summary>
    /// <param name="kanaler">Driftavbrottskanaler</param>
    /// <param name="system">Namnet på den anropande komponenten</param>
    /// <param name="config">Samling med konfigurationsparametrar, måste innehålla "url".</param>
    public DriftavbrottMonitor(IEnumerable<string> kanaler, string system, NameValueCollection config)
    {
      workerClass = new BgWorker(new DriftavbrottKlient(config), kanaler, system);
      workerClass.DriftavbrottStatus += workerClassDriftavbrottStatus;
      workerClass.ErrorOccurred += workerClassOnErrorOccurred;
      workerThread = new Thread(workerClass.Start);
      workerThread.Start();
      while (workerThread.IsAlive != true) { }
    }

    #endregion

    #region privata metoder

    /// <summary>Hanterar status event</summary>
    /// <param name="sender">Avsändare</param>
    /// <param name="args">Statusargument</param>
    private void workerClassDriftavbrottStatus(object sender, DriftavbrottStatusEvent args)
    {
      DriftavbrottStatus?.Invoke(this, args);
    }
    /// <summary>Hanterar error event</summary>
    /// <param name="sender">Avsändare</param>
    /// <param name="args">Errorargument</param>
    private void workerClassOnErrorOccurred(object sender, ErrorEvent args)
    {
      ErrorOccurred?.Invoke(this, args);
    }

    #endregion

    #region IDisposable implmentation

    /// <summary>
    /// Skrotar instansen och frigör resurser.
    /// </summary>
    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }
    /// <summary>
    /// Skrotar instansen och frigör resurser.
    /// </summary>
    protected virtual void Dispose(bool doIt)
    {
      if (Disposed)
        return;
      if (doIt)
      {
        Disposed = true;
        workerClass.RequestStop();
        workerThread.Join(1000);
        workerClass.DriftavbrottStatus -= workerClassDriftavbrottStatus;
        workerClass.ErrorOccurred -= workerClassOnErrorOccurred;
      }
    }

    #endregion

    #region Subklasser

    /// <summary>
    /// Arbetsklassen.
    /// </summary>
    internal class BgWorker
    {
      #region medlemmar

      private string system;
      /// <summary>
      /// Driftavbrottsklient.
      /// </summary>
      private DriftavbrottKlient client = null;
      /// <summary>
      /// Styrning
      /// </summary>
      private volatile bool shouldStop;
      /// <summary>
      /// Kanaler som monitorn skall använda
      /// </summary>
      private Dictionary<String, Kanal> kanalStatus = new Dictionary<String, Kanal>();
      /// <summary>
      /// Händelsedelegat
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="args"></param>
      internal delegate void DriftavbrottStatusHandler(object sender, DriftavbrottStatusEvent args);
      /// <summary>
      /// Event
      /// </summary>
      internal event DriftavbrottStatusHandler DriftavbrottStatus;
      /// <summary>
      /// Händelsedelegat
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="args"></param>
      public delegate void ErrorOccurredHandler(object sender, ErrorEvent args);
      /// <summary>
      /// Error event
      /// </summary>
      public event ErrorOccurredHandler ErrorOccurred;

      internal string ServiceUrl => client.ServiceUrl;

      #endregion

      #region Konstruktor
      /// <summary>
      /// Skapar instansen.
      /// </summary>
      /// <param name="client">DriftavbrottKlient</param>
      /// <param name="kanaler"></param>
      /// <param name="system">Namnet på den anropande komponenten</param>
      public BgWorker(DriftavbrottKlient client, IEnumerable<string> kanaler, string system)
      {
        this.client = client;
        this.system = system;
        foreach (string s in kanaler)
        {
          kanalStatus.Add(s, new Kanal(s));
        }
      }
      #endregion

      #region publika metoder
      /// <summary>
      /// Startar kontrollen.
      /// </summary>
      public void Start()
      {
        shouldStop = false;
        doWork();
        if (client?.Disposed == false)
          client.Dispose();
      }
      /// <summary>
      /// Stoppar kontrollen.
      /// </summary>
      public void RequestStop()
      {
        shouldStop = true;
      }
      #endregion

      #region privata metoder

      /// <summary>
      /// Etablerar händelse vid statusförändringar.
      /// </summary>
      /// <param name="evnt">DriftavbrottStatusEvent</param>
      private void onDriftavbrottStatusChanged(DriftavbrottStatusEvent evnt)
      {
        DriftavbrottStatus?.Invoke(this, evnt);
      }

      /// <summary>
      /// Etablerar händelse vid fel.
      /// </summary>
      /// <param name="evnt">ErrorEvent</param>
      private void onErrorOccurred(ErrorEvent evnt)
      {
        ErrorOccurred?.Invoke(this, evnt);
      }

      /// <summary>
      /// Kontrollerar driftavbrott och hanterar event.
      /// </summary>
      private void doWork()
      {
        while (shouldStop == false)
        {
          string[] kanaler = kanalStatus.Keys.ToArray();
          List<driftavbrottType> kommandeAvbrott = new List<driftavbrottType>();

          try
          {
            kommandeAvbrott.AddRange(client.GetPagaendeDriftavbrott(kanaler, system));
          }
          catch (ApplicationException e)
          {
            onErrorOccurred(new ErrorEvent(e.Message, ErrorEvent.ErrorNivå.Warn, e));
          }
          catch (Exception e)
          {
            if (e.InnerException != null && e.InnerException.Message.Equals("File Not Found"))
            {
              onErrorOccurred(new ErrorEvent("Ett eventueltt konfigurationsfel inträffade.", ErrorEvent.ErrorNivå.Warn, e));
            }
            else
            {
              throw;
            }
          }
          
          foreach (driftavbrottType avbrott in kommandeAvbrott)
          {
            if (DateTime.Now.AddSeconds(10) > avbrott.start)
            {
              if (kanalStatus[avbrott.kanal].Status != MDH.DriftavbrottKlient.DriftavbrottStatus.Pågående)
              {
                kanalStatus[avbrott.kanal].Status = MDH.DriftavbrottKlient.DriftavbrottStatus.Pågående;
                kanalStatus[avbrott.kanal].Start = avbrott.start;
                kanalStatus[avbrott.kanal].Slut = avbrott.slut;
                onDriftavbrottStatusChanged(
                  new DriftavbrottStatusEvent(
                  MDH.DriftavbrottKlient.DriftavbrottStatus.Pågående,
                  avbrott.kanal,
                  avbrott.meddelande_sv,
                  avbrott.meddelande_en));
              }
              else
              {
                kanalStatus[avbrott.kanal].Start = avbrott.start;
                kanalStatus[avbrott.kanal].Slut = avbrott.slut;
              }
            }
            else
            {
              kanalStatus[avbrott.kanal].Start = avbrott.start;
              kanalStatus[avbrott.kanal].Slut = avbrott.slut;
            }
            if (shouldStop)
            {
              break;
            }
          }

          foreach (var kanal in kanalStatus)
          {
            if (kanal.Value.Slut < DateTime.Now)
            {
              if (kanal.Value.Status == MDH.DriftavbrottKlient.DriftavbrottStatus.Pågående)
              {
                kanal.Value.Status = MDH.DriftavbrottKlient.DriftavbrottStatus.Upphört;
                onDriftavbrottStatusChanged(
                  new DriftavbrottStatusEvent(
                    MDH.DriftavbrottKlient.DriftavbrottStatus.Upphört,
                    kanal.Value.Name,
                    string.Empty,
                    string.Empty));
              }
            }
          }

          #region timer

          if (shouldStop == false)
          {
            for (int i = 0; i < 120; i++)
            {
              Thread.Sleep(500);
              if (shouldStop)
              {
                break;
              }
            }
          }

          #endregion
        }
      }

      #endregion
    }

    /// <summary>
    /// Kanal
    /// </summary>
    internal class Kanal
    {
      public string Name { get; }

      public DateTime Start { get; set; }

      public DateTime Slut { get; set; }

      public MDH.DriftavbrottKlient.DriftavbrottStatus Status { get; set; }

      public Kanal(string name)
      {
        Name = name;
        Start = DateTime.MaxValue;
        Slut = DateTime.MaxValue;
        Status = MDH.DriftavbrottKlient.DriftavbrottStatus.Saknas;
      }
    }

    #endregion
  }
}
