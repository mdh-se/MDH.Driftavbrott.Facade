using System;

namespace SE.MDH.DriftavbrottKlient
{
  /// <summary>
  /// Händelsekalss för driftavbrott
  /// </summary>
  public class DriftavbrottStatusEvent : EventArgs
  {
    /// <summary>
    /// Status
    /// </summary>
    public DriftavbrottStatus Status { get; }
    /// <summary>
    /// Kanal
    /// </summary>
    public string Kanal { get; }
    /// <summary>
    /// Svenskt driftavbrottsmeddelande.
    /// </summary>
    public string MeddelandeSv { get; }
    /// <summary>
    /// Engelskt driftavbrottsmeddelande.
    /// </summary>
    public string MeddelandeEng { get; }

    /// <summary>
    /// Skapar instansen
    /// </summary>
    /// <param name="status">Status</param>
    /// <param name="kanal">Kanal</param>
    public DriftavbrottStatusEvent(DriftavbrottStatus status, string kanal, string meddelandeSv, string meddelandeEng)
    {
      Kanal = kanal;
      Status = status;
      MeddelandeSv = meddelandeSv;
      MeddelandeEng = meddelandeEng;
    }
  }
}
