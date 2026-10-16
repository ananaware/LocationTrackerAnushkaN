using SQLite;

namespace LocationTrackerAnushkaN.Models;

/// <summary>
/// Represents one saved location point from the user's device.
/// Each instance of this class becomes one row in the SQLite table.
/// </summary>
public class LocationPoint
{
    /// <summary>
    /// Unique ID of the row. SQLite creates it automatically.
    /// </summary>
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    /// <summary>
    /// Latitude of the location, in degrees.
    /// </summary>
    public double Latitude { get; set; }

    /// <summary>
    /// Longitude of the location, in degrees.
    /// </summary>
    public double Longitude { get; set; }

    /// <summary>
    /// Date and time (UTC) when the location was recorded.
    /// </summary>
    public DateTime Timestamp { get; set; }
}