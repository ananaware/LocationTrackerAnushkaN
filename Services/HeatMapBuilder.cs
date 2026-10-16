using LocationTrackerAnushkaN.Models;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Maps;

namespace LocationTrackerAnushkaN.Services;

/// <summary>
/// Turns saved location points into colored map circles that form a heat map.
/// Areas with many nearby points look red. Areas with few points look blue.
/// </summary>
public static class HeatMapBuilder
{
    // Size of each dot on the map (in meters).
    private const double DotRadiusMeters = 15;

    // Points closer than this (in meters) count as "neighbours".
    private const double NeighbourRadiusMeters = 30;

    // This many neighbours (or more) makes a dot fully red.
    private const int MaxNeighbourCount = 10;

    // Limit how many points are drawn so the map stays fast.
    private const int MaxPointsToDraw = 500;

    // Transparency of the dot fill (0 = invisible, 255 = solid).
    private const int FillAlpha = 120;

    /// <summary>
    /// Creates one map circle for each saved point.
    /// </summary>
    /// <param name="points">The saved location points.</param>
    /// <returns>A list of circles that can be added to the map.</returns>
    public static List<Circle> BuildCircles(IEnumerable<LocationPoint> points)
    {
        // Use only the newest points so drawing stays quick.
        var recentPoints = points
            .OrderByDescending(p => p.Timestamp)
            .Take(MaxPointsToDraw)
            .ToList();

        var circles = new List<Circle>();

        foreach (var point in recentPoints)
        {
            int neighbourCount = CountNeighbours(point, recentPoints);
            circles.Add(CreateCircle(point, neighbourCount));
        }

        return circles;
    }

    /// <summary>
    /// Counts how many other points are close to the given point.
    /// </summary>
    private static int CountNeighbours(LocationPoint point, List<LocationPoint> allPoints)
    {
        var center = new Location(point.Latitude, point.Longitude);
        int count = 0;

        foreach (var other in allPoints)
        {
            if (ReferenceEquals(point, other))
            {
                continue;
            }

            var otherLocation = new Location(other.Latitude, other.Longitude);
            double distanceMeters =
                center.CalculateDistance(otherLocation, DistanceUnits.Kilometers) * 1000;

            if (distanceMeters <= NeighbourRadiusMeters)
            {
                count++;
            }
        }

        return count;
    }

    /// <summary>
    /// Builds one circle whose color goes from blue (few neighbours) to red (many).
    /// </summary>
    private static Circle CreateCircle(LocationPoint point, int neighbourCount)
    {
        // Intensity is a number from 0.0 (cool) to 1.0 (hot).
        double intensity = Math.Min(neighbourCount, MaxNeighbourCount) / (double)MaxNeighbourCount;

        int red = (int)(255 * intensity);
        int blue = (int)(255 * (1 - intensity));

        return new Circle
        {
            Center = new Location(point.Latitude, point.Longitude),
            Radius = Distance.FromMeters(DotRadiusMeters),
            StrokeWidth = 0,
            StrokeColor = Colors.Transparent,
            FillColor = Color.FromRgba(red, 0, blue, FillAlpha)
        };
    }
}