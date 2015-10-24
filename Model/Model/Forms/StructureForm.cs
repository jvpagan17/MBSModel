using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Model
{
    public partial class StructureForm : Form
    {
        public StructureForm()
        {
            InitializeComponent();
        }

        private void optionsToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void sTRUCTUREBindingNavigatorSaveItem_Click(object sender, EventArgs e)
        {
            this.Validate();
            this.sTRUCTUREBindingSource.EndEdit();
            this.tableAdapterManager.UpdateAll(this.dBfilesDataSet);

        }

        private void StructureForm_Load(object sender, EventArgs e)
        {
            // TODO: This line of code loads data into the 'dBfilesDataSet.STRUCTURE' table. You can move, or remove it, as needed.
            this.sTRUCTURETableAdapter.Fill(this.dBfilesDataSet.STRUCTURE);

        }

        private void collateralToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CollateralForm newForm = new CollateralForm();
            newForm.Show();
        }
    }
}
