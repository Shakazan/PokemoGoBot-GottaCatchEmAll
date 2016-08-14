using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PokemonGo.RocketAPI.Logic.Utils;
using Logger = PokemonGo.RocketAPI.Logic.Logging.Logger;
using LogLevel = PokemonGo.RocketAPI.Logic.Logging.LogLevel;

namespace PokemonGo.RocketAPI.Logic.Tasks
{
    public class TransferPokemonTask
    {
        public static async Task Execute()
        {
            await Inventory.GetCachedInventory(true);
            var pokemonToTransfer = await Inventory.GetPokemonToTransfer(Logic._clientSettings.NotTransferPokemonsThatCanEvolve, Logic._clientSettings.PrioritizeIVOverCP, Logic._clientSettings.PokemonsToNotTransfer);
            if (pokemonToTransfer == null || !pokemonToTransfer.Any())
                return;

            Logger.Write($"Found {pokemonToTransfer.Count()} Pokemon for Transfer:", LogLevel.Debug);
            foreach (var pokemon in pokemonToTransfer)
            {
                await Logic._client.Inventory.TransferPokemon(pokemon.Id);

                await Inventory.GetCachedInventory(true);
                var myPokemonSettings = await Inventory.GetPokemonSettings();
                var pokemonSettings = myPokemonSettings.ToList();
                var myPokemonFamilies = await Inventory.GetPokemonFamilies();
                var pokemonFamilies = myPokemonFamilies.ToArray();
                var settings = pokemonSettings.Single(x => x.PokemonId == pokemon.PokemonId);
                var familyCandy = pokemonFamilies.Single(x => settings.FamilyId == x.FamilyId);
                var familyCandies = $"{familyCandy.Candy_}";

                BotStats.PokemonTransferedThisSession += 1;

                var bestRankingPokemon = await Inventory.GetHighestPokemonOfTypeByRanking(pokemon);
                var bestIVPokemon = await Inventory.GetHighestPokemonOfTypeByIv(pokemon);
                var bestCPPokemon = await Inventory.GetHighestPokemonOfTypeByCp(pokemon);
                var bestPokemonInfo = $"Best Rank: {PokemonInfo.DisplayPokemonDetails(bestRankingPokemon, Logic._clientSettings.PrioritizeFactor)} |\r\n BestCP: {PokemonInfo.DisplayPokemonDetails(bestCPPokemon, Logic._clientSettings.PrioritizeFactor)} |\r\n Best IV: {PokemonInfo.DisplayPokemonDetails(bestIVPokemon, Logic._clientSettings.PrioritizeFactor)}";

                Logger.Write($"{pokemon.PokemonId} {PokemonInfo.DisplayPokemonDetails(pokemon, Logic._clientSettings.PrioritizeFactor)} |\r\n {bestPokemonInfo} |\r\n Family Candies: {familyCandies}", LogLevel.Transfer);
            }

            await BotStats.GetPokemonCount();
            BotStats.UpdateConsoleTitle();
        }
    }
}
