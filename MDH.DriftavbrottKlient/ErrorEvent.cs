using System;

namespace SE.MDH.DriftavbrottKlient
{
  /// <summary>
  /// Envent klass som hanterar error
  /// </summary>
  public class ErrorEvent : EventArgs
  {
    /// <summary>
    /// Nivå
    /// </summary>
    public enum ErrorNivå
    {
      /// <summary>
      /// Warn
      /// </summary>
      Warn,
      /// <summary>
      /// Error
      /// </summary>
      Error,
      /// <summary>
      /// Fatal
      /// </summary>
      Fatal
    }
    /// <summary>
    /// Skapar instansen.
    /// </summary>
    /// <param name="meddelande">Meddelande</param>
    /// <param name="nivå">Nivå</param>
    /// <param name="exception">Stacktrace</param>
    public ErrorEvent(string meddelande, ErrorNivå nivå = ErrorNivå.Warn, Exception exception = null)
    {
      Meddelande = meddelande;
      Nivå = nivå;
      ErrorException = exception;
    }
    /// <summary>
    /// Nivå.
    /// </summary>
    public ErrorNivå Nivå { get; private set; }
    /// <summary>
    /// Meddelande.
    /// </summary>
    public string Meddelande { get; private set; }
    /// <summary>
    /// Stacktrace
    /// </summary>
    public Exception ErrorException { get; private set; }
  }
}