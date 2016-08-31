using GMap.NET;
using POGOProtos.Data;
using POGOProtos.Map.Fort;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonGo.RocketAPI.Logic
{
    public class MessageEventArgs : EventArgs
    {
        public MessageEventArgs(string s)
        {
            msg = s;
        }
        private string msg;
        public string Message
        {
            get { return msg; }
            set { msg = value; }
        }
    }


    public class PokemonDataCollectionEventArgs : EventArgs
    {
        public PokemonDataCollectionEventArgs(IEnumerable<PokemonData> pokemonList)
        {
            pokemonCollection = pokemonList;
        }
        private IEnumerable<PokemonData> pokemonCollection;
        public IEnumerable<PokemonData> PokemonCollection
        {
            get { return pokemonCollection; }
            set { pokemonCollection = value; }
        }
    }


    public class FortDataCollectionEventArgs : EventArgs
    {
        public FortDataCollectionEventArgs(IEnumerable<FortData> pokemonList)
        {
            pokemonCollection = pokemonList;
        }
        private IEnumerable<FortData> pokemonCollection;
        public IEnumerable<FortData> FortCollection
        {
            get { return pokemonCollection; }
            set { pokemonCollection = value; }
        }
    }


    public class PointLatLngEventArgs : EventArgs
    {
        public PointLatLngEventArgs(PointLatLng latLong)
        {
            LatLong = latLong;
        }
        private PointLatLng LatLong;
        public PointLatLng LatLongPoint
        {
            get { return LatLong; }
            set { LatLong = value; }
        }
    }

}
