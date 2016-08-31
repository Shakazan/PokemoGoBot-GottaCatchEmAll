namespace PokemonGo.RocketAPI.Console
{
    partial class MyPokemons
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.panel2 = new System.Windows.Forms.Panel();
            this.dgvPokemons = new System.Windows.Forms.DataGridView();
            this.inventoryBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.myPokemonsBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.btnUseLuckyEgg = new System.Windows.Forms.Button();
            this.btnTransfer = new System.Windows.Forms.Button();
            this.btnEvolve = new System.Windows.Forms.Button();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvPokemons)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.inventoryBindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.myPokemonsBindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btnEvolve);
            this.panel1.Controls.Add(this.btnTransfer);
            this.panel1.Controls.Add(this.btnUseLuckyEgg);
            this.panel1.Controls.Add(this.btnRefresh);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Right;
            this.panel1.Location = new System.Drawing.Point(801, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(167, 418);
            this.panel1.TabIndex = 0;
            // 
            // btnRefresh
            // 
            this.btnRefresh.Location = new System.Drawing.Point(41, 23);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(76, 32);
            this.btnRefresh.TabIndex = 0;
            this.btnRefresh.Text = "Refresh";
            this.btnRefresh.UseVisualStyleBackColor = true;
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.dgvPokemons);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(0, 0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(801, 418);
            this.panel2.TabIndex = 1;
            // 
            // dgvPokemons
            // 
            this.dgvPokemons.AllowUserToAddRows = false;
            this.dgvPokemons.AllowUserToDeleteRows = false;
            this.dgvPokemons.AllowUserToOrderColumns = true;
            this.dgvPokemons.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvPokemons.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvPokemons.Location = new System.Drawing.Point(0, 0);
            this.dgvPokemons.Name = "dgvPokemons";
            this.dgvPokemons.ReadOnly = true;
            this.dgvPokemons.RowTemplate.Height = 24;
            this.dgvPokemons.Size = new System.Drawing.Size(801, 418);
            this.dgvPokemons.TabIndex = 0;
            this.dgvPokemons.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvPokemons_CellContentClick);
            // 
            // inventoryBindingSource
            // 
            this.inventoryBindingSource.DataSource = typeof(PokemonGo.RocketAPI.Logic.Inventory);
            // 
            // myPokemonsBindingSource
            // 
            this.myPokemonsBindingSource.DataSource = typeof(PokemonGo.RocketAPI.Console.MyPokemons);
            // 
            // btnUseLuckyEgg
            // 
            this.btnUseLuckyEgg.Location = new System.Drawing.Point(6, 78);
            this.btnUseLuckyEgg.Name = "btnUseLuckyEgg";
            this.btnUseLuckyEgg.Size = new System.Drawing.Size(117, 32);
            this.btnUseLuckyEgg.TabIndex = 1;
            this.btnUseLuckyEgg.Text = "Use Lucky Egg";
            this.btnUseLuckyEgg.UseVisualStyleBackColor = true;
            // 
            // btnTransfer
            // 
            this.btnTransfer.Location = new System.Drawing.Point(6, 132);
            this.btnTransfer.Name = "btnTransfer";
            this.btnTransfer.Size = new System.Drawing.Size(117, 32);
            this.btnTransfer.TabIndex = 2;
            this.btnTransfer.Text = "Transfer";
            this.btnTransfer.UseVisualStyleBackColor = true;
            // 
            // btnEvolve
            // 
            this.btnEvolve.Location = new System.Drawing.Point(6, 186);
            this.btnEvolve.Name = "btnEvolve";
            this.btnEvolve.Size = new System.Drawing.Size(117, 32);
            this.btnEvolve.TabIndex = 3;
            this.btnEvolve.Text = "Evolve";
            this.btnEvolve.UseVisualStyleBackColor = true;
            this.btnEvolve.Click += new System.EventHandler(this.btnEvolve_Click);
            // 
            // MyPokemons
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(968, 418);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Name = "MyPokemons";
            this.Text = "MyPokemons";
            this.panel1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvPokemons)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.inventoryBindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.myPokemonsBindingSource)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.DataGridView dgvPokemons;
        private System.Windows.Forms.BindingSource inventoryBindingSource;
        private System.Windows.Forms.BindingSource myPokemonsBindingSource;
        private System.Windows.Forms.Button btnEvolve;
        private System.Windows.Forms.Button btnTransfer;
        private System.Windows.Forms.Button btnUseLuckyEgg;
    }
}