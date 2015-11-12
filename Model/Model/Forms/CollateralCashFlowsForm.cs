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
    public partial class CollateralCashFlowsForm : Form
    {
        public CollateralCashFlowsForm()
        {
            InitializeComponent();
        }

        private void CollateralCashFlowsForm_Load(object sender, EventArgs e)
        {
               
            for (int i = 0; i < 361; i++)
            {
                this.collateralCashFlowsGridView.Rows.Add(i, 0, 0, 0, 0, 0, 0, 0);
                //You don't need to set the other properties, they were binded when you put the DataSource in there
                //    grdData.Rows[i].Cells[5].Value = Convert.ToDouble(ds.Tables[0].Rows[i]["Qty"]) * Convert.ToDouble(ds.Tables[0].Rows[i]["Rate"]);
            }
        }

    }
}
