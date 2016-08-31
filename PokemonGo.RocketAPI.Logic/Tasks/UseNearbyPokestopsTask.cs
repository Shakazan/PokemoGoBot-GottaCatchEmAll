using POGOProtos.Map.Fort;
using PokemonGo.RocketAPI.Extensions;
using PokemonGo.RocketAPI.Helpers;
using PokemonGo.RocketAPI.Logic.Logging;
using PokemonGo.RocketAPI.Logic.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonGo.RocketAPI.Logic.Tasks
{
    public class UseNearbyPokestopsTask
    {
        //Please do not change GetPokeStops() in this file, it's specifically set
        //to only find stops within 40 meters
        //this is for gpx pathing, we are not going to the pokestops,
        //so do not make it more than 40 because it will never get close to those stops.
        public static async Task Execute(List<FortData> pokestops)
        {
            if (Logic._client.Settings.GPXIgnorePokestops)
                return;

            //var pokestops = await Inventory.GetPokestops(true);
            // Look for any within 40 yards
            pokestops = pokestops.Where(p => LocationUtils.CalculateDistanceInMeters(
                   Logic._client.CurrentLatitude, Logic._client.CurrentLongitude,
                   p.Latitude, p.Longitude) < 40).ToList();

            while (pokestops.Any())
            {
                var pokestop =
                    pokestops.OrderBy(
                        i =>
                            LocationUtils.CalculateDistanceInMeters(Logic._client.CurrentLatitude,
                                Logic._client.CurrentLongitude, i.Latitude, i.Longitude)).First();
                pokestops.Remove(pokestop);

                var distance = LocationUtils.CalculateDistanceInMeters(Logic._client.CurrentLatitude, Logic._client.CurrentLongitude, pokestop.Latitude, pokestop.Longitude);

                var fortInfo = await Logic._client.Fort.GetFort(pokestop.Id, pokestop.Latitude, pokestop.Longitude);
                var latlngDebug = string.Empty;
                if (Logic._client.Settings.DebugMode)
                    latlngDebug = $"| Latitude: {pokestop.Latitude} - Longitude: {pokestop.Longitude}";
                Logger.Write($"Name: {fortInfo.Name} in {distance:0.##} m distance {latlngDebug}", LogLevel.Pokestop);

                //Catch Lure Pokemon
                if (pokestop.LureInfo != null && Logic._client.Settings.CatchLuredPokemon)
                {
                    await CatchLurePokemonsTask.Execute(pokestop);
                }

                var timesZeroXPawarded = 0;
                var fortTry = 0; //Current check
                const int retryNumber = 50; //How many times it needs to check to clear softban
                const int zeroCheck = 5; //How many times it checks fort before it thinks it's softban
                do
                {
                    var fortSearch = await Logic._client.Fort.SearchFort(pokestop.Id, pokestop.Latitude, pokestop.Longitude);
                    if (fortSearch.ExperienceAwarded > 0 && timesZeroXPawarded > 0) timesZeroXPawarded = 0;
                    if (fortSearch.ExperienceAwarded == 0)
                    {
                        timesZeroXPawarded++;

                        if (timesZeroXPawarded <= zeroCheck) continue;
                        if ((int)fortSearch.CooldownCompleteTimestampMs != 0)
                        {
                            break; // Check if successfully looted, if so program can continue as this was "false alarm".
                        }
                        fortTry += 1;

                        if (Logic._client.Settings.DebugMode)
                            Logger.Write($"Seems your Soft-Banned. Trying to Unban via Pokestop Spins. Retry {fortTry} of {retryNumber - zeroCheck}", LogLevel.Warning);

                        await RandomHelper.RandomDelay(75, 100);
                    }
                    else
                    {
                        BotStats.ExperienceThisSession += fortSearch.ExperienceAwarded;
                        Logger.UpdateTitle(BotStats.ConsoleTitle());
                        Logger.Write($"XP: {fortSearch.ExperienceAwarded}, Gems: {fortSearch.GemsAwarded}, Items: {StringUtils.GetSummedFriendlyNameOfItemAwardList(fortSearch.ItemsAwarded)}", LogLevel.Pokestop);
                        RecycleItemsTask._recycleCounter++;
                        HatchEggsTask._hatchUpdateDelayGPX++;
                        break; //Continue with program as loot was succesfull.
                    }
                } while (fortTry < retryNumber - zeroCheck);
                //Stop trying if softban is cleaned earlier or if 40 times fort looting failed.

                if (RecycleItemsTask._recycleCounter >= 5)
                    await RecycleItemsTask.Execute();
                if (HatchEggsTask._hatchUpdateDelayGPX >= 5)
                    await HatchEggsTask.Execute();


                // Throw the event out.
                List<FortData> forts = new List<FortData>();
                pokestop.CooldownCompleteTimestampMs = DateTime.UtcNow.AddMinutes(15).ToUnixTime();
                forts.Add(pokestop);
                Logger.Events.RaiseFortsChangedEvent(forts);

                // Takes about 5 seconds to farm a pokestop
                // Thread.Sleep(15000);
                ThreadSleep.f_sleep(5);
            }

        }
    }
}
