﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PokemonGo.RocketAPI.Logic.Utils;
using POGOProtos.Inventory.Item;
using POGOProtos.Map.Pokemon;
using POGOProtos.Networking.Responses;
using POGOProtos.Map.Fort;
using Logger = PokemonGo.RocketAPI.Logic.Logging.Logger;
using LogLevel = PokemonGo.RocketAPI.Logic.Logging.LogLevel;
using System.Threading;

namespace PokemonGo.RocketAPI.Logic.Tasks
{
    public class CatchPokemonTask
    {
        public static async Task Execute(dynamic encounter, MapPokemon pokemon, FortData currentFortData = null, ulong encounterId = 0)
        {
            // If the encounter is null nothing will work below, so exit now
            if (encounter == null) return;
            float probability = encounter.CaptureProbability?.CaptureProbability_[0];

            var encounterPokemon = encounter is EncounterResponse ? encounter.WildPokemon?.PokemonData : encounter?.PokemonData;
            var catchType = encounter is EncounterResponse ? "Normal" : encounter is DiskEncounterResponse ? "Lure" : "Incense";
            var Id = encounter is EncounterResponse ? pokemon.PokemonId : encounter?.PokemonData.PokemonId;
            var Level = PokemonInfo.GetLevel(encounterPokemon);
            var Cp = encounter is EncounterResponse ? encounter.WildPokemon?.PokemonData?.Cp : encounter?.PokemonData?.Cp ?? 0;
            var MaxCp = PokemonInfo.CalculateMaxCp(encounterPokemon);
            var Iv = PokemonInfo.CalculatePokemonPerfection(encounterPokemon);
            var Perfection = Math.Round(PokemonInfo.CalculatePokemonPerfection(encounterPokemon));
            var Probability = Math.Round(probability * 100, 2);
            var distance = LocationUtils.CalculateDistanceInMeters(Logic._client.CurrentLatitude,
                Logic._client.CurrentLongitude,
                encounter is EncounterResponse || encounter is IncenseEncounterResponse ? pokemon.Latitude : currentFortData.Latitude,
                encounter is EncounterResponse || encounter is IncenseEncounterResponse ? pokemon.Longitude : currentFortData.Longitude);
            double reticleSize = Convert.ToDouble(new Random().Next(175, 195)) / 100D;
            var isSpinning = new Random().Next(0, 1);
            var hitPos = 1;
            string lineEnding = "\n";

            CatchPokemonResponse caughtPokemonResponse;

            var attemptCounter = 1;
            do
            {
                bool useBerry = false;

                // Do we use a berry?
                // Low probability and either:
                // High CP, or High IV pokemon
                if (!float.IsNaN(probability) && probability < 0.35
                    &&
                        (
                            (Level >= BotStats.Currentlevel - 3
                            || Perfection >= Logic._client.Settings.TransferPokemonKeepAllAboveIV
                            || Cp >= Logic._client.Settings.TransferPokemonKeepAllAboveCP)
                        )
                    )
                {
                    useBerry = true;
                }

                // Use a berry if low probability and lots of berries
                if (BotStats.TotalBerries > (Logic._client.Settings.MaxBerries * 0.9))
                {
                    if (!float.IsNaN(probability) && probability < 0.35)
                    {
                        useBerry = true;
                    }
                }

                if (useBerry)
                {
                    await
                        UseBerry(encounter is EncounterResponse || encounter is IncenseEncounterResponse ? pokemon.EncounterId : encounterId,
                        encounter is EncounterResponse || encounter is IncenseEncounterResponse ? pokemon.SpawnPointId : currentFortData?.Id);
                }

                if (Perfection >= Logic._client.Settings.TransferPokemonKeepAllAboveIV
                 || Cp >= Logic._client.Settings.TransferPokemonKeepAllAboveCP
                 || attemptCounter >= 3)
                {
                    // Get a perfect throw
                    reticleSize = 1.9 + (new Random().Next(20, 99) / 1000);
                    isSpinning = 1;
                    hitPos = 1;
                }

                var pokeball = await GetBestBall(encounter, probability);
                if (pokeball == ItemId.ItemUnknown)
                {
                    Logger.Write($"You don't own any Pokeballs :( - We missed a {Id} with CP {Cp}", LogLevel.Warning);
                    return;
                }

                caughtPokemonResponse =
                    await Logic._client.Encounter.CatchPokemon(
                        encounter is EncounterResponse || encounter is IncenseEncounterResponse ? pokemon.EncounterId : encounterId,
                        encounter is EncounterResponse || encounter is IncenseEncounterResponse ? pokemon.SpawnPointId : currentFortData.Id, 
                        pokeball,
                        reticleSize,
                        isSpinning,
                        hitPos);

                if (caughtPokemonResponse.Status == CatchPokemonResponse.Types.CatchStatus.CatchEscape)
                {
                    lineEnding = "";
                }
                else
                {
                    lineEnding = "\n";
                }

                if (caughtPokemonResponse.Status == CatchPokemonResponse.Types.CatchStatus.CatchSuccess)
                {
                    BotStats.ExperienceThisSession += caughtPokemonResponse.CaptureAward.Xp.Sum();
                    BotStats.PokemonCaughtThisSession += 1;
                    await BotStats.GetPokeDexCount();
                    await BotStats.GetPokemonCount();
                    var profile = await Logic._client.Player.GetPlayer();
                    BotStats.TotalStardust = profile.PlayerData.Currencies.ToArray()[1].Amount;
                    
                }

                if (encounter?.CaptureProbability?.CaptureProbability_ != null)
                {
                    Func<ItemId, string> returnRealBallName = a =>
                    {
                        switch (a)
                        {
                            case ItemId.ItemPokeBall:
                                return "Poke";
                            case ItemId.ItemGreatBall:
                                return "Great";
                            case ItemId.ItemUltraBall:
                                return "Ultra";
                            case ItemId.ItemMasterBall:
                                return "Master";
                            default:
                                return "Unknown";
                        }
                    };

                    var catchStatus = attemptCounter > 1
                        ? $"{caughtPokemonResponse.Status} Attempt #{attemptCounter}"
                        : $"{caughtPokemonResponse.Status}";

                    var receivedXp = caughtPokemonResponse.Status == CatchPokemonResponse.Types.CatchStatus.CatchSuccess
                        ? $"and received XP {caughtPokemonResponse.CaptureAward.Xp.Sum()}"
                        : $"";

                    Logger.Write($"({catchStatus} / {catchType}) | {Id} - Lvl {Level} [{ PokemonInfo.DisplayPokemonDetails(encounterPokemon, Logic._clientSettings.PrioritizeFactor)}] | Chance: {Probability} | {distance:0.##}m dist | with a {returnRealBallName(pokeball)}Ball {receivedXp}{lineEnding}", LogLevel.Pokemon);
                }

                // Takes about 15 seconds to catch a pokemon
                // Thread.Sleep(15000);
                ThreadSleep.f_sleep(15);
                attemptCounter++;
            }
            while (caughtPokemonResponse.Status == CatchPokemonResponse.Types.CatchStatus.CatchMissed || caughtPokemonResponse.Status == CatchPokemonResponse.Types.CatchStatus.CatchEscape);
        }
        public static async Task<ItemId> GetBestBall(dynamic encounter, float probability)
        {
            var pokemonCp = encounter is EncounterResponse ? encounter.WildPokemon?.PokemonData?.Cp : encounter?.PokemonData?.Cp ?? 0;
            var iV = Math.Round(PokemonInfo.CalculatePokemonPerfection(encounter is EncounterResponse ? encounter.WildPokemon?.PokemonData : encounter?.PokemonData));

            var items = await Inventory.GetItems();
            var balls = items.Where(i => ((ItemId)i.ItemId == ItemId.ItemPokeBall
                                      || (ItemId)i.ItemId == ItemId.ItemGreatBall
                                      || (ItemId)i.ItemId == ItemId.ItemUltraBall
                                      || (ItemId)i.ItemId == ItemId.ItemMasterBall) && i.Count > 0).GroupBy(i => ((ItemId)i.ItemId)).ToList();
            if (balls.Count == 0) return ItemId.ItemUnknown;

            var pokeBalls = balls.Any(g => g.Key == ItemId.ItemPokeBall);
            var greatBalls = balls.Any(g => g.Key == ItemId.ItemGreatBall);
            var ultraBalls = balls.Any(g => g.Key == ItemId.ItemUltraBall);
            var masterBalls = balls.Any(g => g.Key == ItemId.ItemMasterBall);

            if (masterBalls && pokemonCp >= 1500)
                return ItemId.ItemMasterBall;

            if (ultraBalls && (pokemonCp >= 1000 || (iV >= Logic._client.Settings.TransferPokemonKeepAllAboveIV && probability < 0.40)))
                return ItemId.ItemUltraBall;

            if (greatBalls && (pokemonCp >= 300 || (iV >= Logic._client.Settings.TransferPokemonKeepAllAboveIV && probability < 0.50)))
                return ItemId.ItemGreatBall;

            return balls.OrderBy(g => g.Key).First().Key;
        }

        private static async Task UseBerry(ulong encounterId, string spawnPointId)
        {
            var inventoryberries = await Inventory.GetItems();
            var berry = inventoryberries.FirstOrDefault(p => p.ItemId == ItemId.ItemRazzBerry);
            if (berry == null || berry.Count <= 0)
                return;

            await Logic._client.Encounter.UseCaptureItem(encounterId, ItemId.ItemRazzBerry, spawnPointId);
            berry.Count -= 1;
            Logger.Write($"Used Razz Berry, remaining: {berry.Count}", LogLevel.Berry);
            ThreadSleep.f_sleep(4);
        }

        public static async Task<ItemId> GetBestBerry(dynamic encounter, float probability)
        {
            var pokemonCp = encounter is EncounterResponse ? encounter.WildPokemon?.PokemonData?.Cp : encounter?.PokemonData?.Cp ?? 0;
            var iV = Math.Round(PokemonInfo.CalculatePokemonPerfection(encounter is EncounterResponse ? encounter.WildPokemon?.PokemonData : encounter?.PokemonData));

            var items = await Inventory.GetItems();
            var berries = items.Where(i => ((ItemId)i.ItemId == ItemId.ItemRazzBerry
                                        || (ItemId)i.ItemId == ItemId.ItemBlukBerry
                                        || (ItemId)i.ItemId == ItemId.ItemNanabBerry
                                        || (ItemId)i.ItemId == ItemId.ItemWeparBerry
                                        || (ItemId)i.ItemId == ItemId.ItemPinapBerry) && i.Count > 0).GroupBy(i => ((ItemId)i.ItemId)).ToList();
            if (berries.Count == 0 || pokemonCp < 150) return ItemId.ItemUnknown;

            var razzBerryCount = await Inventory.GetItemAmountByType(ItemId.ItemRazzBerry);
            var blukBerryCount = await Inventory.GetItemAmountByType(ItemId.ItemBlukBerry);
            var nanabBerryCount = await Inventory.GetItemAmountByType(ItemId.ItemNanabBerry);
            var weparBerryCount = await Inventory.GetItemAmountByType(ItemId.ItemWeparBerry);
            var pinapBerryCount = await Inventory.GetItemAmountByType(ItemId.ItemPinapBerry);

            if (pinapBerryCount > 0 && pokemonCp >= 2000)
                return ItemId.ItemPinapBerry;

            if (weparBerryCount > 0 && pokemonCp >= 1500)
                return ItemId.ItemWeparBerry;

            if (nanabBerryCount > 0 && (pokemonCp >= 1000 || (iV >= Logic._client.Settings.TransferPokemonKeepAllAboveIV && probability < 0.40)))
                return ItemId.ItemNanabBerry;

            if (blukBerryCount > 0 && (pokemonCp >= 500 || (iV >= Logic._client.Settings.TransferPokemonKeepAllAboveIV && probability < 0.50)))
                return ItemId.ItemBlukBerry;

            if (razzBerryCount > 0 && pokemonCp >= 300)
                return ItemId.ItemRazzBerry;

            return berries.OrderBy(g => g.Key).First().Key;
        }
    }
}
