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

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void calcToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            double CollBalance= 0, TotCalcBal= 0, TotGCpn= 0, TotPT= 0, TotOTerm= 0, TotRTerm= 0, TotPrice= 0,
            UCollBalance= 0, UTotCalcBal= 0, TotOPrincipal= 0;


            for (int i = 0; i < cOLLATERALDataGridView.RowCount; i++)
            {
                CollBalance += Convert.ToDouble(cOLLATERALDataGridView.Rows[i].Cells[2].Value);
           /*     RecordCalcBal= Convert.ToDouble(cOLLATERALDataGridView.Rows[i].Cells[2].Value); FieldByName('CalcBal').AsFloat;
                               TotCalcBal= TotCalcBal + RecordCalcBal;
                               UTotCalcBal= UTotCalcBal + FieldByName('UCalcBal').AsFloat;
                               TotGCpn= TotGCPn + FieldByName('GCpn').AsFloat * RecordCalcBal;
                               TotPT= TotPT + FieldByName('P/T').AsFloat * RecordCalcBal;
                               TotOTerm= TotOTerm + FieldByName('OTerm').AsFloat * RecordCalcBal;
                               TotRTerm= TotRTerm + FieldByName('RTerm').AsFloat * RecordCalcBal;
                               TotPrice= TotPRice + FieldByName('Price').AsFloat * RecordCalcBal;
                               TotOPrincipal= TotOPrincipal + FieldByName('OPrincipal').AsFloat;
                               Next;
                               end; 
                               

            */
            }

            label11.Text = string.Format("{0:$###,###,###.00}", CollBalance);

                 /*              Label2.Caption= FloatToStrF(TotGCpn / TotCalcBal, ffNumber, 6, 4);
                               Label3.Caption= FloatToStrF(TotPT / TotCalcBal, ffNumber, 6, 4);
                               Label4.Caption= FloatToStrF(TotOTerm / TotCalcBal, ffNumber, 5, 2);
                               Label5.Caption= FloatToStrF(TotRTerm / TotCalcBal, ffNumber, 5, 2);
                               Label6.Caption= FloatToStrF(TotPrice / TotCalcBal, ffNumber, 7, 4);
                               Label7.Caption= FloatToStrF(TotCalcBal, ffNumber, 15, 2);
                               Label15.Caption= FloatToStrF(UCollBalance, ffNumber, 15, 2);
                               Label16.Caption= FloatToStrF(UTotCalcBal, ffNumber, 15, 2);
                               Label19.Caption= FloatToStrF(TotOPrincipal, ffNumber, 15, 2);
                               EnableControls;
                               First;
                               end;
                               end;
                               */
           
        }



        private void cOLLATERALBindingNavigator_RefreshItems(object sender, EventArgs e)
        {

        }

        private void CollateralTableCalcFields()
        {



            double LastSerialRemainder, RecordBalance, URecordBalance,
            FirstSerial, LastSerial, TotalSerial, UnitsRemaining;
            int CollType;



 /*           begin
             with CollateralTable do
                begin
                FirstSerial:= FieldByName('1st Serial').AsInteger;
            LastSerial:= FieldByName('Lst Serial').AsInteger;
            TotalSerial:= FieldByName('Tot Serial').AsInteger;
            RecordBalance:= FieldByName('Balance').AsFloat;
            if FieldByName('OPrincipal').AsFloat = 0 then
               URecordBalance:= FieldByName('Balance').AsFloat * FieldByName('Factor').AsFloat
   else URecordBalance:= FieldByName('OPrincipal').AsFloat * FieldByName('Factor').AsFloat;
            CollType:= FieldByName('Type').AsInteger;
            { Compute CalcBal}
            UnitsRemaining:= Int(RecordBalance / 25000);
            if LastSerial = TotalSerial Then
              LastSerialRemainder:= RecordBalance - UnitsRemaining * 25000
   Else LastSerialRemainder := 0;
            If not(CollType = 1) Then
             FieldByName('CalcBal').AsFloat:= RecordBalance
    Else
   FieldByName('CalcBal').AsFloat:= (Max(LastSerial, TotalSerial - UnitsRemaining) -
   Max(FirstSerial - 1, TotalSerial - UnitsRemaining)) * 25000 + LastSerialRemainder;
            { Compute UCalcBalance}
            UnitsRemaining:= Int(URecordBalance / 25000);
            if LastSerial = TotalSerial Then
              // LastSerialRemainder:= URecordBalance-UnitsRemaining*25000
              Else LastSerialRemainder:= 0;
            FieldByName('UBalance').AsFloat:= URecordBalance;
            If not(CollType = 1) Then
             FieldByName('UCalcBal').AsFloat:= URecordBalance
    Else
   FieldByName('UCalcBal').AsFloat:= (Max(LastSerial, TotalSerial - UnitsRemaining) -
   Max(FirstSerial - 1, TotalSerial - UnitsRemaining)) * 25000 + LastSerialRemainder;

            end;
            end;
*/
        }
    }
}
