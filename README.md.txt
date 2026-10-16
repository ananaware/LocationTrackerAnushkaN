# Location Tracker (.NET MAUI)

A multiplatform app built with C# and .NET MAUI. It saves the device location
to a SQLite database every few seconds and shows the saved points on a map as a
simple heat map (blue = few nearby points, red = many nearby points).

## Features
- Reads the device location with the MAUI Geolocation API
- Saves each point (latitude, longitude, time) to a local SQLite database
- Shows a Google map using the .NET MAUI Maps component
- Draws a heat map by adding colored circles to the map
- Start / Stop tracking button

## Project structure
| File | Purpose |
|------|---------|
| `Models/LocationPoint.cs` | The data saved for each location |
| `Services/DatabaseService.cs` | Creates the SQLite database and saves / reads points |
| `Services/LocationTrackingService.cs` | Reads the location every 5 seconds and saves it |
| `Services/HeatMapBuilder.cs` | Turns saved points into colored map circles |
| `MainPage.xaml` / `MainPage.xaml.cs` | The "MainView" screen with the map and button |
| `MauiProgram.cs` | App setup and dependency injection |
| `Platforms/Android/AndroidManifest.xml` | Android permissions and Google Maps key |

## How to run (Android)
1. Install Visual Studio with the **.NET Multi-platform App UI development** workload.
2. Get a Google Maps API key and enable **Maps SDK for Android** in Google Cloud.
3. Open `Platforms/Android/AndroidManifest.xml` and replace
   `YOUR_GOOGLE_MAPS_API_KEY` with your key. Do not upload your real key to GitHub.
4. Start an Android emulator (with Google APIs) and run the app.
5. Allow location access, then tap **Start Tracking**.
   In the emulator, use Extended controls > Location to set or play a route.

## Built with
- C# and .NET 10 MAUI
- Microsoft.Maui.Controls.Maps
- sqlite-net-pcl