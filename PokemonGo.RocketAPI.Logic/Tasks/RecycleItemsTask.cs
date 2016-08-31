﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PokemonGo.RocketAPI.Logic.Utils;
using POGOProtos.Inventory.Item;
using Logger = PokemonGo.RocketAPI.Logic.Logging.Logger;
using LogLevel = PokemonGo.RocketAPI.Logic.Logging.LogLevel;
using System.Threading;

namespace PokemonGo.RocketAPI.Logic.Tasks
{
    public class RecycleItemsTask
    {
        public static int _recycleCounter = 0;

        public static async Task Execute()
        {
            await Inventory.GetCachedInventory(true);
            var items = await Inventory.GetItemsToRecycle(Logic._clientSettings);
            if (items == null || !items.Any())
                return;

            Logger.Write($"Found {items.Count()} Recyclable {(items.Count() == 1 ? "Item" : "Items")}:", LogLevel.Debug);
            foreach (var item in items)
            {
                await Logic._client.Inventory.RecycleItem((ItemId)item.ItemId, item.Count);
                Logger.Write($"{item.Count}x {(ItemId)item.ItemId}", LogLevel.Recycling);

                BotStats.ItemsRemovedThisSession += item.Count;

                Thread.Sleep(2000);
            }

            Logger.UpdateTitle(BotStats.ConsoleTitle());
            _recycleCounter = 0;
        }
    }
}
