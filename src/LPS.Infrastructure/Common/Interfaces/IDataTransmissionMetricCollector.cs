using System.Threading;

/// <summary>
/// Defines methods for collecting metrics related to data transmission,
/// including the size of data sent or received and the time taken for the operation.
/// </summary>
public interface IDataTransmissionMetricCollector
{
    /// <summary>
    /// Updates the metrics for data sent, including the size of the data and the time taken to upload it.
    /// </summary>
    /// <param name="dataSize">The size of the data sent, in bytes.</param>
    /// <param name="uploadTime">The time taken to upload the data, in milliseconds.</param>
    /// <param name="token">A cancellation token to observe for task cancellation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public void UpdateDataSent(double dataSize, double uploadTime, CancellationToken token);

    /// <summary>
    /// Updates the metrics for data received, including the size of the data and the time taken to download it.
    /// </summary>
    /// <param name="dataSize">The size of the data received, in bytes.</param>
    /// <param name="DownloadTime">The time taken to download the data, in milliseconds.</param>
    /// <param name="token">A cancellation token to observe for task cancellation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public void UpdateDataReceived(double dataSize, double DownloadTime, CancellationToken token);
}
