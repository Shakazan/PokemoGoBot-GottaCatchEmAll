using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PokemonGo.RocketAPI.Helpers;
using PokemonGo.RocketAPI.Logic.Utils;
using Logger = PokemonGo.RocketAPI.Logic.Logging.Logger;
using LogLevel = PokemonGo.RocketAPI.Logic.Logging.LogLevel;
using POGOProtos.Map.Fort;
using PokemonGo.RocketAPI.Extensions;

namespace PokemonGo.RocketAPI.Logic.Tasks
{
    public class FarmPokestopsGPXTask
    {
        public static GPXSegmentLocator SegmentLocator = new GPXSegmentLocator();
        public static async Task Execute(GPXSegmentLocator locator)
        {
            // Update with start location.
            SegmentLocator = locator;

            // List out all the available pokestops.
            var pokestops = await Inventory.GetPokestops();

            var tracks = GetGpxTracks();
            for (SegmentLocator.Track = locator.Track; SegmentLocator.Track < tracks.Count; SegmentLocator.Track++)
            {
                var track = tracks.ElementAt(SegmentLocator.Track);
                var trackSegments = track.Segments;
                for (SegmentLocator.Segment = locator.Segment; SegmentLocator.Segment < trackSegments.Count; SegmentLocator.Segment++)
                {
                    var trackPoints = track.Segments.ElementAt(SegmentLocator.Segment).TrackPoints;
                    for (SegmentLocator.Point = locator.Point; SegmentLocator.Point < trackPoints.Count; SegmentLocator.Point++)
                    {
                        var nextPoint = trackPoints.ElementAt(SegmentLocator.Point);

                        // Slightly randomise the walking path
                        double randomFactor = new Random().Next(1, 9) / 100000;
                        nextPoint.Lat = nextPoint.Lat + randomFactor;
                        nextPoint.Lon = nextPoint.Lon + randomFactor;

                        var distanceCheck = LocationUtils.CalculateDistanceInMeters(Logic._client.CurrentLatitude,
                               Logic._client.CurrentLongitude, Convert.ToDouble(nextPoint.Lat, CultureInfo.InvariantCulture),
                            Convert.ToDouble(nextPoint.Lon, CultureInfo.InvariantCulture));

                        if (distanceCheck > 5000)
                        {
                            Logger.Write(
                                $"Your Target destination of {nextPoint.Lat}, {nextPoint.Lon} is too far from your current position of {Logic._client.CurrentLatitude}, {Logic._client.CurrentLongitude} - Distance: {distanceCheck:0.##}",
                                LogLevel.Error);
                            break;
                        }
                        
                        Logger.Write($"Your Target destination is {nextPoint.Lat}, {nextPoint.Lon} your location is {Logic._client.CurrentLatitude}, {Logic._client.CurrentLongitude} - Distance: {distanceCheck:0.##}", LogLevel.Debug);

                        if (Logic._client.Settings.ExportPokemonToCsvEveryMinutes > 0 && ExportPokemonToCsv._lastExportTime.AddMinutes(Logic._client.Settings.ExportPokemonToCsvEveryMinutes).Ticks < DateTime.Now.Ticks)
                        {
                            var _playerProfile = await Logic._client.Player.GetPlayer();
                            await ExportPokemonToCsv.Execute(_playerProfile.PlayerData);
                        }

                        if (Logic._client.Settings.UseLuckyEggs)
                            await UseLuckyEggTask.Execute();
                        if (Logic._client.Settings.CatchIncensePokemon)
                            await UseIncenseTask.Execute();

                        await
                            Navigation.HumanPathWalking(SegmentLocator.Point, trackPoints.ElementAt(SegmentLocator.Point),
                                async () =>
                                {
                                    // Catch normal map Pokemon
                                    await CatchMapPokemonsTask.Execute();
                                    //Catch Pokestops on the Way
                                    await UseNearbyPokestopsTask.Execute(await Inventory.GetPokestops(true));
                                    //Catch Incense Pokemon
                                    await CatchIncensePokemonsTask.Execute();
                                    return true;
                                });

                    } //end trkpts
                } //end trksegs
            } //end tracks

            // Clear it once finished
            SegmentLocator = new GPXSegmentLocator();
        }

        public static List<GpxReader.Trk> GetGpxTracks()
        {
            var xmlString = File.ReadAllText(Logic._client.Settings.GPXFile);
            var readgpx = new GpxReader(xmlString);
            return readgpx.Tracks;
        }
    }

    //internal class UseNearbyPokestopsTask
    //{
    //    //Please do not change GetPokeStops() in this file, it's specifically set
    //    //to only find stops within 40 meters
    //    //this is for gpx pathing, we are not going to the pokestops,
    //    //so do not make it more than 40 because it will never get close to those stops.
    //    public static async Task Execute()
    //    {
    //        if (Logic._client.Settings.GPXIgnorePokestops)
    //            return;

    //        var pokestops = await Inventory.GetPokestops(true);

    //        while (pokestops.Any())
    //        {
    //            var pokestop =
    //                pokestops.OrderBy(
    //                    i =>
    //                        LocationUtils.CalculateDistanceInMeters(Logic._client.CurrentLatitude,
    //                            Logic._client.CurrentLongitude, i.Latitude, i.Longitude)).First();
    //            pokestops.Remove(pokestop);

    //            var distance = LocationUtils.CalculateDistanceInMeters(Logic._client.CurrentLatitude, Logic._client.CurrentLongitude, pokestop.Latitude, pokestop.Longitude);

    //            var fortInfo = await Logic._client.Fort.GetFort(pokestop.Id, pokestop.Latitude, pokestop.Longitude);
    //            var latlngDebug = string.Empty;
    //            if (Logic._client.Settings.DebugMode)
    //                latlngDebug = $"| Latitude: {pokestop.Latitude} - Longitude: {pokestop.Longitude}";
    //            Logger.Write($"Name: {fortInfo.Name} in {distance:0.##} m distance {latlngDebug}", LogLevel.Pokestop);

    //            //Catch Lure Pokemon
    //            if (pokestop.LureInfo != null && Logic._client.Settings.CatchLuredPokemon)
    //            {
    //                await CatchLurePokemonsTask.Execute(pokestop);
    //            }

    //            var timesZeroXPawarded = 0;
    //            var fortTry = 0; //Current check
    //            const int retryNumber = 50; //How many times it needs to check to clear softban
    //            const int zeroCheck = 5; //How many times it checks fort before it thinks it's softban
    //            do
    //            {
    //                var fortSearch = await Logic._client.Fort.SearchFort(pokestop.Id, pokestop.Latitude, pokestop.Longitude);
    //                if (fortSearch.ExperienceAwarded > 0 && timesZeroXPawarded > 0) timesZeroXPawarded = 0;
    //                if (fortSearch.ExperienceAwarded == 0)
    //                {
    //                    timesZeroXPawarded++;

    //                    if (timesZeroXPawarded <= zeroCheck) continue;
    //                    if ((int)fortSearch.CooldownCompleteTimestampMs != 0)
    //                    {
    //                        break; // Check if successfully looted, if so program can continue as this was "false alarm".
    //                    }
    //                    fortTry += 1;

    //                    if (Logic._client.Settings.DebugMode)
    //                        Logger.Write($"Seems your Soft-Banned. Trying to Unban via Pokestop Spins. Retry {fortTry} of {retryNumber - zeroCheck}", LogLevel.Warning);

    //                    await RandomHelper.RandomDelay(75, 100);
    //                }
    //                else
    //                {
    //                    BotStats.ExperienceThisSession += fortSearch.ExperienceAwarded;
    //                    Logger.UpdateTitle(BotStats.ConsoleTitle());
    //                    Logger.Write($"XP: {fortSearch.ExperienceAwarded}, Gems: {fortSearch.GemsAwarded}, Items: {StringUtils.GetSummedFriendlyNameOfItemAwardList(fortSearch.ItemsAwarded)}", LogLevel.Pokestop);
    //                    RecycleItemsTask._recycleCounter++;
    //                    HatchEggsTask._hatchUpdateDelayGPX++;
    //                    break; //Continue with program as loot was succesfull.
    //                }
    //            } while (fortTry < retryNumber - zeroCheck);
    //            //Stop trying if softban is cleaned earlier or if 40 times fort looting failed.

    //            if (RecycleItemsTask._recycleCounter >= 5)
    //                await RecycleItemsTask.Execute();
    //            if (HatchEggsTask._hatchUpdateDelayGPX >= 5)
    //                await HatchEggsTask.Execute();


    //            // Throw the event out.
    //            List<FortData> forts = new List<FortData>();
    //            pokestop.CooldownCompleteTimestampMs = DateTime.UtcNow.AddMinutes(15).ToUnixTime();
    //            forts.Add(pokestop);
    //            Logger.Events.RaiseFortsChangedEvent(forts);

    //        }

    //    }
    //}

    public class GPXSegmentLocator
    {
        public int Track = 0;
        public int Segment = 0;
        public int Point = 0;
    }
}
