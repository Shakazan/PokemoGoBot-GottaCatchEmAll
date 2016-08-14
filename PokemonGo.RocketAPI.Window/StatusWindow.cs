using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PokemonGo.RocketAPI.Window
{
    public partial class StatusWindow : Form
    {
        public StatusWindow()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"Button1 clicked");
        }

        public string LabelStatusValue
        {
            get
            {
                return label1.Text;
            }
            set
            {
                label1.Text = value;
            }
        }

    }
}
