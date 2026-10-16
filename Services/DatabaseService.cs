using LocationTrackerAnushkaN.Models;
using SQLite;

namespace LocationTrackerAnushkaN.Services;

/// <summary>
/// Handles saving and loading location points in a local SQLite database.
/// </summary>
public class DatabaseService
{
    // Name of the database file stored on the device.
    private const string DatabaseFileName = "locations.db3";

    // The connection is created the first time it is needed.
    private SQLiteAsyncConnection? _database;

    /// <summary>
    /// Full path of the database file in the app's private data folder.
    /// </summary>
    private static string DatabasePath =>
        Path.Combine(FileSystem.AppDataDirectory, DatabaseFileName);

    /// <summary>
    /// Opens the database and creates the table if it does not exist yet.
    /// </summary>
    private async Task InitAsync()
    {
        if (_database is not null)
        {
            return;
        }

        _database = new SQLiteAsyncConnection(
            DatabasePath,
            SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.SharedCache);

        await _database.CreateTableAsync<LocationPoint>();
    }

    /// <summary>
    /// Saves one location point to the database.
    /// </summary>
    /// <param name="point">The location to save.</param>
    /// <returns>The number of rows added.</returns>
    public async Task<int> SaveLocationAsync(LocationPoint point)
    {
        await InitAsync();
        return await _database!.InsertAsync(point);
    }

    /// <summary>
    /// Gets all saved location points.
    /// </summary>
    /// <returns>A list of every saved location.</returns>
    public async Task<List<LocationPoint>> GetLocationsAsync()
    {
        await InitAsync();
        return await _database!.Table<LocationPoint>().ToListAsync();
    }
}