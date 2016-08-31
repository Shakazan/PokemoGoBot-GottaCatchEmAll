using POGOProtos.Data;
using PokemonGo.RocketAPI.Logic;
using PokemonGo.RocketAPI.Logic.Logging;
using PokemonGo.RocketAPI.Logic.Tasks;
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
    public partial class MyPokemons : Form
    {
        public SortableBindingList<PokemonView> viewList = new SortableBindingList<PokemonView>();
        BindingSource source = new BindingSource();
   

        public MyPokemons()
        {
            InitializeComponent();
            // RefreshPokemonsList();
            BindColumns();
         
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            RefreshPokemonsList();
            BindColumns();
        }

        private async void RefreshPokemonsList()
        {
            var myPokemonFamilies = await Inventory.GetPokemonFamilies();
            var pokemonFamilies = myPokemonFamilies.ToArray();

            var myPokemonSettings = await Inventory.GetPokemonSettings();
            var pokemonSettings = myPokemonSettings.ToList();

            List<PokemonView> unsortedList = new List<PokemonView>();
            viewList = new SortableBindingList<PokemonView>();
            var myPokemons = await Inventory.GetPokemons();
            myPokemons = myPokemons.OrderBy(x => x.PokemonId).ThenByDescending(x => PokemonInfo.CalculatePokemonRanking(x, PokemonGo.RocketAPI.Logic.Logic._clientSettings.PrioritizeFactor)).ToList();
            foreach (PokemonData pokemon in myPokemons)
            {
                PokemonView poke = new PokemonView();
                poke.Id = pokemon.Id.ToString();
                poke.Name = pokemon.PokemonId.ToString();
                poke.Ranking = PokemonInfo.CalculatePokemonRanking(pokemon, PokemonGo.RocketAPI.Logic.Logic._clientSettings.PrioritizeFactor);
                poke.CP = pokemon.Cp;
                poke.MaxCP = PokemonInfo.CalculateMaxCp(pokemon);
                poke.IV = PokemonInfo.CalculatePokemonPerfection(pokemon);
                poke.Transfer = false;
                poke.Evolve = false;

                var individualPokemonsettings = pokemonSettings.Single(x => x.PokemonId == pokemon.PokemonId);
                poke.FamilyCandies = pokemonFamilies.Single(x => individualPokemonsettings.FamilyId == x.FamilyId).Candy_;
                
                unsortedList.Add(poke);
            }

            
            unsortedList = unsortedList.OrderBy(x => x.Name).ThenByDescending(x => x.Ranking).ToList();
            foreach(var item in unsortedList)
            {
                viewList.Add(item);
            }
            

        }

        private void dgvPokemons_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void BindColumns()
        {
            source.DataSource = viewList;
            dgvPokemons.AutoGenerateColumns = true;
            dgvPokemons.DataSource = source;
            dgvPokemons.ReadOnly = false;
            dgvPokemons.AlternatingRowsDefaultCellStyle.BackColor = Color.LightYellow;

            //DataGridViewTextBoxColumn columnID = new DataGridViewTextBoxColumn();
            //columnID.DataPropertyName = "Id";
            //columnID.Name = "ID";
            //columnID.HeaderText = "I.d.";
            //columnID.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            //columnID.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;

            //DataGridViewTextBoxColumn columnName = new DataGridViewTextBoxColumn();
            //columnName.DataPropertyName = "Name";
            //columnName.Name = "Name";
            //columnName.HeaderText = "Name";
            //columnName.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            //columnName.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;

            //dgvPokemons.Columns.Add(columnID);
            //dgvPokemons.Columns.Add(columnName);
        }

        private async void btnEvolve_Click(object sender, EventArgs e)
        {
            IEnumerable<PokemonData> myPokemons = await Inventory.GetPokemons();
            List<PokemonData> pokemonToEvolve = new List<PokemonData>();

            var checkedRows = from DataGridViewRow r in dgvPokemons.Rows
                              where Convert.ToBoolean(r.Cells["Evolve"].Value) == true
                              select r;

            foreach (var row in checkedRows)
            {
                pokemonToEvolve.Add(myPokemons.First(x => x.Id == Convert.ToUInt64(row.Cells["Id"].Value)));
            }

            DialogResult result = MessageBox.Show($"Do you want to pop a lucky egg first before evolving {pokemonToEvolve.Count} pokemon?", "Use Lucky Egg before Evolving", MessageBoxButtons.YesNoCancel);

            if (result == DialogResult.Cancel)
            {
                return;
            }

            if (result == DialogResult.Yes)
            {
                await UseLuckyEggTask.Execute();
            }

            // TODO: Remove
            await Inventory.GetCachedInventory(true);

            // Fire this off so that the main window can pick it up and run with it.
            Logger.Events.RaiseEvolvePokemonEvent(pokemonToEvolve);
            
        }
    }

    public class PokemonView
    {
        public string Name { get; set; }
        public double Ranking { get; set; }
        public int CP { get; set; }
        public int MaxCP { get; set; }
        public double IV { get; set; }
        public bool Transfer { get; set; }
        public bool Evolve { get; set; }
        public int FamilyCandies { get; set; }
        public string Id { get; set; }

    }
}
