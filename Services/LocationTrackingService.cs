using System.Diagnostics;
using LocationTrackerAnushkaN.Models;

namespace LocationTrackerAnushkaN.Services;

/// <summary>
/// Reads the device location at a fixed interval and saves each point
/// to the SQLite database.
/// </summary>
public class LocationTrackingService
{
    // How often (in seconds) a new location is read and saved.
    private const int IntervalSeconds = 5;

    private readonly DatabaseService _databaseService;

    // Used to stop the tracking loop.
    private CancellationTokenSource? _cancellationTokenSource;

    /// <summary>
    /// True while the app is tracking the location.
    /// </summary>
    public bool IsTracking { get; private set; }

    /// <summary>
    /// Raised every time a new location point has been saved.
    /// </summary>
    public event EventHandler<LocationPoint>? LocationSaved;

    /// <summary>
    /// Creates the service with the database it should save to.
    /// </summary>
    /// <param name="databaseService">The database service.</param>
    public LocationTrackingService(DatabaseService databaseService)
    {
        _databaseService = databaseService;
    }

    /// <summary>
    /// Asks the user for permission to use the location, if not already given.
    /// </summary>
    /// <returns>True if the permission was granted.</returns>
    public static async Task<bool> RequestPermissionAsync()
    {
        var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();

        if (status != PermissionStatus.Granted)
        {
            status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
        }

        return status == PermissionStatus.Granted;
    }

    /// <summary>
    /// Starts tracking the location if the permission is granted.
    /// </summary>
    /// <returns>True if tracking started.</returns>
    public async Task<bool> StartAsync()
    {
        if (IsTracking)
        {
            return true;
        }

        if (!await RequestPermissionAsync())
        {
            return false;
        }

        IsTracking = true;
        _cancellationTokenSource = new CancellationTokenSource();

        // Run the loop in the background so the screen stays responsive.
        _ = TrackLoopAsync(_cancellationTokenSource.Token);
        return true;
    }

    /// <summary>
    /// Stops tracking the location.
    /// </summary>
    public void Stop()
    {
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource = null;
        IsTracking = false;
    }

    /// <summary>
    /// Repeats every few seconds: reads the location and saves it.
    /// </summary>
    private async Task TrackLoopAsync(CancellationToken token)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(IntervalSeconds));

        try
        {
            do
            {
                await SaveCurrentLocationAsync();
            }
            while (await timer.WaitForNextTickAsync(token));
        }
        catch (OperationCanceledException)
        {
            // Expected when tracking is stopped.
        }
    }

    /// <summary>
    /// Reads the current location once and saves it to the database.
    /// </summary>
    private async Task SaveCurrentLocationAsync()
    {
        try
        {
            var request = new GeolocationRequest(
                GeolocationAccuracy.Best,
                TimeSpan.FromSeconds(10));

            var location = await Geolocation.Default.GetLocationAsync(request);

            if (location is null)
            {
                return;
            }

            var point = new LocationPoint
            {
                Latitude = location.Latitude,
                Longitude = location.Longitude,
                Timestamp = DateTime.UtcNow
            };

            await _databaseService.SaveLocationAsync(point);
            LocationSaved?.Invoke(this, point);
        }
        catch (Exception ex)
        {
            // Location can fail (GPS off, no permission). Log it and keep going.
            Debug.WriteLine($"Could not read location: {ex.Message}");
        }
    }
}