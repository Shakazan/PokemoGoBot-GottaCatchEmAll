using POGOProtos.Data;
using PokemonGo.RocketAPI.Logic;
using PokemonGo.RocketAPI.Logic.Tasks;
using PokemonGo.RocketAPI.Logic.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PokemonGo.RocketAPI.Console
{
    public partial class StatusWindow : Form
    {
        private delegate void UpdateStringMethod(string message);
        private delegate void PokemonDataCollectionMethod(IEnumerable<PokemonData> pokemondata);
        private MyPokemons myPokemonsForm = new MyPokemons();
        private Map mapForm = new Map();

        public StatusWindow()
        {
            Logic.Logging.Logger.Events.RaiseMessageEvent += HandleMessageEvent;
            Logic.Logging.Logger.Events.UpdateTitleEvent += HandleUpdateTitleEvent;
            Logic.Logging.Logger.Events.EvolvePokemonsEvent += HandleEvolvePokemonsEvent;
            InitializeComponent();

            //string title = BotStats.ConsoleTitle();
            //this.Text = title;
        }

        private void HandleEvolvePokemonsEvent(object sender, PokemonDataCollectionEventArgs e)
        {
            EvolvePokemon(e.PokemonCollection);
        }

        private void HandleUpdateTitleEvent(object sender, MessageEventArgs e)
        {
            UpdateTitle(e.Message);
        }

        public void HandleMessageEvent(object sender, MessageEventArgs e)
        {
            UpdateRichTextBox(e.Message);
        }

        public void UpdateTitle(string message)
        {
            if (this.InvokeRequired)
            {

                this.Invoke(
                    new UpdateStringMethod(UpdateTitle), // the method to call back on
                    new object[] { message });
            }
            else
            {
                this.Text = message;
            }
        }

        public void UpdateRichTextBox(string message)
        {


            if (richTextBox1.InvokeRequired)
            {
                // this means we’re on the wrong thread!  
                // use BeginInvoke or Invoke to call back on the 
                // correct thread.
                richTextBox1.Invoke(
                    new UpdateStringMethod(UpdateRichTextBox), // the method to call back on
                    new object[] { message });                              // the list of arguments to pass
            }
            else
            {
                Color color = Color.Black;
                if (message.Contains("(PKMN)"))
                {
                    if (message.Contains("CatchSuccess"))
                    {
                        color = Color.DarkGreen;
                    }
                    else
                    {
                        color = Color.DarkOrange;
                    }
                }
                if (message.Contains("(POKESTOP)"))
                {
                    color = Color.SlateGray;
                }
                else if (message.Contains("(TRANSFER)"))
                {
                    color = Color.Indigo;
                }
                else if (message.Contains("(INFO)"))
                {
                    color = Color.DarkSlateGray;
                }
                else if (message.Contains("(RECYCLING)"))
                {
                    color = Color.DarkSlateGray;
                }
                else if (message.Contains("(BERRY)"))
                {
                    color = Color.DarkMagenta;
                }
                else if (message.Contains("(EGG)"))
                {
                    color = Color.DarkMagenta;
                }
                else if (message.Contains("(INCUBATION)"))
                {
                    color = Color.DarkMagenta;
                }
                else if (message.Contains("(NAVIGATION)"))
                {
                    color = Color.LightGray;
                }
                if (message.Length > 0)
                {
                    richTextBox1.AppendText(message + "\n", color);
                }
            }
        }

        public async void EvolvePokemon(IEnumerable<PokemonData> pokemonsToEvolve)
        {
            if (this.InvokeRequired)
            {

                this.Invoke(
                    new PokemonDataCollectionMethod(EvolvePokemon), // the method to call back on
                    new object[] { pokemonsToEvolve });
            }
            else
            {
                await EvolvePokemonTask.Execute(pokemonsToEvolve);
            }
        }

        private void pokemonsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!myPokemonsForm.Visible)
            {
                myPokemonsForm.Show();
            }
        }

        private void mapToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!mapForm.Visible)
            {
                mapForm.Show();
            }
        }
    }
}
