using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using SE.MDH.DriftavbrottKlient.Model;

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
    /// Event
    /// </summary>
    public event DriftavbrottStatusHandler DriftavbrottStatus;

    #endregion

    #region Egenskaper

    /// <summary>
    /// Indikerar att instansen skrotats.
    /// </summary>
    public bool Disposed { get; private set; }

    #endregion

    #region konstruktor

    /// <summary>
    /// Skapar instansen
    /// </summary>
    /// <param name="kanaler">Driftavbrottskanaler</param>
    public DriftavbrottMonitor(IEnumerable<string> kanaler)
    {
      workerClass = new BgWorker(new DriftavbrottKlient(), kanaler);
      workerClass.DriftavbrottStatus += WorkerClass_DriftavbrottStatus;
      workerThread = new Thread(workerClass.Start);
      workerThread.Start();
      while (workerThread.IsAlive != true) {}
    }

    /// <summary>
    /// Skapar instansen
    /// </summary>
    /// <param name="kanaler">Driftavbrottskanaler</param>
    /// <param name="server">Server</param>
    /// <param name="port">Port</param>
    /// <param name="systemid">Systemets namn</param>
    /// <param name="https">Https</param>
    public DriftavbrottMonitor(IEnumerable<string> kanaler, string server, int port, string systemid, bool https = false)
    {
      workerClass = new BgWorker(new DriftavbrottKlient(server, port, systemid, https), kanaler);
      workerClass.DriftavbrottStatus += WorkerClass_DriftavbrottStatus;
      workerThread = new Thread(workerClass.Start);
      workerThread.Start();
      while (workerThread.IsAlive != true) {}
    }

    #endregion

    #region privata metoder

    /// <summary>
    /// Hanterar event
    /// </summary>
    /// <param name="sender">Avsändare</param>
    /// <param name="args"DriftavbrottStatusEvent></param>
    private void WorkerClass_DriftavbrottStatus(object sender, DriftavbrottStatusEvent args)
    {
      DriftavbrottStatus?.Invoke(this, args);
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
        workerClass.DriftavbrottStatus -= WorkerClass_DriftavbrottStatus;
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
      private Dictionary<String, DriftavbrottStatus> kanalStatus = new Dictionary<string, DriftavbrottStatus>();
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

      #endregion

      #region Konstruktor
      /// <summary>
      /// Skapar instansen.
      /// </summary>
      /// <param name="client">DriftavbrottKlient</param>
      /// <param name="kanaler"></param>
      public BgWorker(DriftavbrottKlient client, IEnumerable<string> kanaler)
      {
        this.client = client;
        foreach (string s in kanaler)
        {
          kanalStatus.Add(s, MDH.DriftavbrottKlient.DriftavbrottStatus.Saknas);
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
      /// Etablerar händelse.
      /// </summary>
      /// <param name="evnt">DriftavbrottStatusEvent</param>
      private void OnDriftavbrottStatusChanged(DriftavbrottStatusEvent evnt)
      {
        DriftavbrottStatus?.Invoke(this, evnt);
      }

      /// <summary>
      /// Kontrollerar driftavbrott och hanterar event.
      /// </summary>
      private void doWork()
      {
        while (shouldStop == false)
        {
          string[] kanaler = kanalStatus.Keys.ToArray();
          foreach (driftavbrottType avbrott in client.GetPagaendeDriftavbrott(kanaler))
          {
            if (DateTime.Now.AddSeconds(-10) > avbrott.start)
            {
              if (kanalStatus[avbrott.kanal] != MDH.DriftavbrottKlient.DriftavbrottStatus.Pågående)
              {
                kanalStatus[avbrott.kanal] = MDH.DriftavbrottKlient.DriftavbrottStatus.Pågående;
                OnDriftavbrottStatusChanged(
                  new DriftavbrottStatusEvent(
                  MDH.DriftavbrottKlient.DriftavbrottStatus.Pågående, 
                  avbrott.kanal, 
                  avbrott.meddelande_sv,
                  avbrott.meddelande_en));
              }
            }
            if (DateTime.Now > avbrott.slut)
            {
              if (kanalStatus[avbrott.kanal] == MDH.DriftavbrottKlient.DriftavbrottStatus.Pågående)
              {
                kanalStatus[avbrott.kanal] = MDH.DriftavbrottKlient.DriftavbrottStatus.Upphört;
                OnDriftavbrottStatusChanged(
                  new DriftavbrottStatusEvent(
                    MDH.DriftavbrottKlient.DriftavbrottStatus.Upphört,
                    avbrott.kanal,
                    string.Empty,
                    string.Empty));
              }
            }
            if (shouldStop)
            {
              break;
            }
          }

          #region timer

          if (shouldStop == false)
          {
            for (int i = 0; i < 20; i++)
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

    #endregion
  }
}
