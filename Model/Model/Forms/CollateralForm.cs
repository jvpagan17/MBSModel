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
    public partial class CollateralForm : Form
    {
        public CollateralForm()
        {
            InitializeComponent();
        }

        private void cOLLATERALBindingNavigatorSaveItem_Click(object sender, EventArgs e)
        {
            this.Validate();
            this.cOLLATERALBindingSource.EndEdit();
            this.tableAdapterManager.UpdateAll(this.dBfilesDataSet);

        }

        private void CollateralForm_Load(object sender, EventArgs e)
        {
            // TODO: This line of code loads data into the 'dBfilesDataSet.COLLATERAL' table. You can move, or remove it, as needed.
            this.cOLLATERALTableAdapter.Fill(this.dBfilesDataSet.COLLATERAL);

        }
    }
}
