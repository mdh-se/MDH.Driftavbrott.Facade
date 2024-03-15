namespace SE.MDH.DriftavbrottKlient
{
  /// <summary>
  /// Status på ett driftavbrotts event.
  /// </summary>
  public enum DriftavbrottStatus
  {
    /// <summary>
    /// Ett driftavbrott är pågående.
    /// </summary>
    Pågående,
    /// <summary>
    /// Ett driftavbrott har upphört.
    /// </summary>
    Upphört,
    /// <summary>
    /// Ingen status tillgänglig.
    /// </summary>
    Saknas
  }
}