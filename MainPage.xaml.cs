using System.Diagnostics;
using LocationTrackerAnushkaN.Models;
using LocationTrackerAnushkaN.Services;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Maps;

namespace LocationTrackerAnushkaN;

/// <summary>
/// The main screen: shows the map with the location heat map
/// and a button to start or stop saving the location.
/// </summary>
public partial class MainPage : ContentPage
{
    // How much of the map to show around the user (in meters).
    private const double MapViewRadiusMeters = 300;

    private readonly DatabaseService _databaseService;
    private readonly LocationTrackingService _trackingService;

    /// <summary>
    /// Creates the page. The services are provided by dependency injection.
    /// </summary>
    /// <param name="databaseService">Reads the saved locations.</param>
    /// <param name="trackingService">Saves the location every few seconds.</param>
    public MainPage(DatabaseService databaseService, LocationTrackingService trackingService)
    {
        InitializeComponent();

        _databaseService = databaseService;
        _trackingService = trackingService;

        // Redraw the heat map whenever a new point is saved.
        _trackingService.LocationSaved += OnLocationSaved;
    }

    /// <summary>
    /// Runs when the page is shown. Draws saved points and centers the map.
    /// </summary>
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await RefreshHeatMapAsync();

        if (await LocationTrackingService.RequestPermissionAsync())
        {
            HeatMap.IsShowingUser = true;
            await CenterMapOnUserAsync();
        }
    }

    /// <summary>
    /// Starts or stops tracking when the button is tapped.
    /// </summary>
    private async void OnTrackingButtonClicked(object? sender, EventArgs e)
    {
        if (_trackingService.IsTracking)
        {
            _trackingService.Stop();
            TrackingButton.Text = "Start Tracking";
            return;
        }

        bool started = await _trackingService.StartAsync();

        if (started)
        {
            HeatMap.IsShowingUser = true;
            TrackingButton.Text = "Stop Tracking";
        }
        else
        {
            await DisplayAlertAsync(
                "Permission needed",
                "Please allow location access so the app can save your location.",
                "OK");
        }
    }

    /// <summary>
    /// Runs when a new location point is saved. Updates the map on the main thread.
    /// </summary>
    private void OnLocationSaved(object? sender, LocationPoint point)
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            await RefreshHeatMapAsync();
            MoveMapTo(new Location(point.Latitude, point.Longitude));
        });
    }

    /// <summary>
    /// Loads all saved points and redraws the heat map circles.
    /// </summary>
    private async Task RefreshHeatMapAsync()
    {
        var points = await _databaseService.GetLocationsAsync();

        HeatMap.MapElements.Clear();

        foreach (var circle in HeatMapBuilder.BuildCircles(points))
        {
            HeatMap.MapElements.Add(circle);
        }
    }

    /// <summary>
    /// Moves the map to the user's current position.
    /// </summary>
    private async Task CenterMapOnUserAsync()
    {
        try
        {
            var location = await Geolocation.Default.GetLastKnownLocationAsync()
                ?? await Geolocation.Default.GetLocationAsync(
                    new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10)));

            if (location is not null)
            {
                MoveMapTo(location);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Could not center the map: {ex.Message}");
        }
    }

    /// <summary>
    /// Zooms the map to the given location.
    /// </summary>
    private void MoveMapTo(Location location)
    {
        var area = MapSpan.FromCenterAndRadius(
            location,
            Distance.FromMeters(MapViewRadiusMeters));

        HeatMap.MoveToRegion(area);
    }
}