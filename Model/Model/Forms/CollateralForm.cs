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
            CollateralTableCalcFields();

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void calcToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            double CollBalance= 0, RecordCalcBal = 0, TotCalcBal = 0, TotGCpn= 0, TotPT= 0, TotOTerm= 0, TotRTerm= 0, TotPrice= 0,
            UCollBalance= 0, UTotCalcBal= 0, TotOPrincipal= 0;


            for (int i = 0; i < cOLLATERALDataGridView.RowCount; i++)
            {
                CollBalance += Convert.ToDouble(cOLLATERALDataGridView.Rows[i].Cells[2].Value); //Balance
                RecordCalcBal = Convert.ToDouble(cOLLATERALDataGridView.Rows[i].Cells[3].Value);  //CalcBalance
                TotCalcBal += RecordCalcBal;
                
                TotGCpn += Convert.ToDouble(cOLLATERALDataGridView.Rows[i].Cells[5].Value)* RecordCalcBal;
                TotPT += Convert.ToDouble(cOLLATERALDataGridView.Rows[i].Cells[6].Value) * RecordCalcBal;
                TotOTerm += Convert.ToDouble(cOLLATERALDataGridView.Rows[i].Cells[7].Value) * RecordCalcBal;
                TotRTerm += Convert.ToDouble(cOLLATERALDataGridView.Rows[i].Cells[8].Value) * RecordCalcBal;
                TotPrice += Convert.ToDouble(cOLLATERALDataGridView.Rows[i].Cells[14].Value) * RecordCalcBal; //Price
                TotOPrincipal += Convert.ToDouble(cOLLATERALDataGridView.Rows[i].Cells[15].Value); //OPrincipal
                UCollBalance += Convert.ToDouble(cOLLATERALDataGridView.Rows[i].Cells[19].Value); //UCollBalance
                UTotCalcBal += Convert.ToDouble(cOLLATERALDataGridView.Rows[i].Cells[20].Value);
            }
            label11.Text = string.Format("{0:$###,###,###.00}", CollBalance);
            label12.Text = string.Format("{0:$###,###,###.00}", TotCalcBal);
            
            label13.Text = string.Format("{0:###.0000}", TotGCpn / TotCalcBal);
            label14.Text = string.Format("{0:###.0000}", TotPT / TotCalcBal);
            label15.Text = string.Format("{0:###.00}", TotOTerm / TotCalcBal);
            label16.Text = string.Format("{0:###.00}", TotRTerm / TotCalcBal);
            label17.Text = string.Format("{0:#,###.0000}", TotPrice / TotCalcBal);
            
            label18.Text = string.Format("{0:$###,###,###.00}", TotOPrincipal);
            label19.Text = string.Format("{0:$###,###,###.00}", UCollBalance);
            label20.Text = string.Format("{0:$###,###,###.00}", UTotCalcBal);
            
            
        }



        private void cOLLATERALBindingNavigator_RefreshItems(object sender, EventArgs e)
        {

        }

        private void CollateralTableCalcFields()
        {



            double LastSerialRemainder, RecordBalance, URecordBalance;
            int FirstSerial, LastSerial, TotalSerial, UnitsRemaining;
            int CollType;
            

            for (int i = 0; i < cOLLATERALDataGridView.RowCount-1; i++)
            {
                try {FirstSerial = Convert.ToInt32(cOLLATERALDataGridView.Rows[i].Cells[10].Value);}  catch (Exception){FirstSerial = 0;}
                try { LastSerial = Convert.ToInt32(cOLLATERALDataGridView.Rows[i].Cells[11].Value); } catch (Exception) { LastSerial = 0; }
                try { TotalSerial = Convert.ToInt32(cOLLATERALDataGridView.Rows[i].Cells[12].Value); } catch (Exception) { TotalSerial = 0; }
                try { RecordBalance = Convert.ToDouble(cOLLATERALDataGridView.Rows[i].Cells[2].Value); } catch (Exception) { RecordBalance = 0; } //Balance
                if (Convert.ToDouble(cOLLATERALDataGridView.Rows[i].Cells[15].Value) == 0) // if OPrincipal == 0 then URecordBalance = Balance*Factor else URecordBalance = OPrincipal*Factor
                    URecordBalance = Convert.ToDouble(cOLLATERALDataGridView.Rows[i].Cells[2].Value) * Convert.ToDouble(cOLLATERALDataGridView.Rows[i].Cells[18].Value);
                else URecordBalance = Convert.ToDouble(cOLLATERALDataGridView.Rows[i].Cells[15].Value) * Convert.ToDouble(cOLLATERALDataGridView.Rows[i].Cells[18].Value);
                CollType = Convert.ToInt16(cOLLATERALDataGridView.Rows[i].Cells[4].Value); // Type
                // Compute CalcBal
                UnitsRemaining = (int)(RecordBalance / 25000);
                if (LastSerial == TotalSerial)
                    LastSerialRemainder = RecordBalance - UnitsRemaining * 25000;
                else LastSerialRemainder = 0;
                if (CollType != 1)
                    cOLLATERALDataGridView.Rows[i].Cells[3].Value = RecordBalance; //CalcBalance = RecordBalance
                else
                    cOLLATERALDataGridView.Rows[i].Cells[3].Value = (Math.Max(LastSerial, TotalSerial - UnitsRemaining) -
                        Math.Max(FirstSerial - 1, TotalSerial - UnitsRemaining)) * 25000 + LastSerialRemainder;  //CalcBalance = ...
                // Compute UCalcBalance
                UnitsRemaining = (int)(URecordBalance / 25000);
                if (LastSerial == TotalSerial)
                    LastSerialRemainder = URecordBalance - UnitsRemaining * 25000;
                else LastSerialRemainder = 0;
                // CalcBalance = = URecordBalance;
                cOLLATERALDataGridView.Rows[i].Cells[19].Value = URecordBalance; // UBalance =
                if (CollType != 1)
                    cOLLATERALDataGridView.Rows[i].Cells[20].Value = URecordBalance;  // UCalcBalance = 
                else
                    cOLLATERALDataGridView.Rows[i].Cells[20].Value = (Math.Max(LastSerial, TotalSerial - UnitsRemaining) -
                        Math.Max(FirstSerial - 1, TotalSerial - UnitsRemaining)) * 25000 + LastSerialRemainder;  // UCalcBalance =

            }
        }
    }
}
