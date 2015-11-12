namespace Model
{
    partial class CollateralCashFlowsForm
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
            this.collateralCashFlowsGridView = new System.Windows.Forms.DataGridView();
            this.No = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Period = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Balance = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.SPrincipal = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.UPrincipal = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.TPrincipal = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Intererst = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Total = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.collateralCashFlowsGridView)).BeginInit();
            this.SuspendLayout();
            // 
            // collateralCashFlowsGridView
            // 
            this.collateralCashFlowsGridView.AllowUserToOrderColumns = true;
            this.collateralCashFlowsGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.collateralCashFlowsGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.No,
            this.Period,
            this.Balance,
            this.SPrincipal,
            this.UPrincipal,
            this.TPrincipal,
            this.Intererst,
            this.Total});
            this.collateralCashFlowsGridView.Location = new System.Drawing.Point(-1, -1);
            this.collateralCashFlowsGridView.Name = "collateralCashFlowsGridView";
            this.collateralCashFlowsGridView.Size = new System.Drawing.Size(1160, 602);
            this.collateralCashFlowsGridView.TabIndex = 0;
            // 
            // No
            // 
            this.No.HeaderText = "No.";
            this.No.Name = "No";
            // 
            // Period
            // 
            this.Period.HeaderText = "Period";
            this.Period.Name = "Period";
            // 
            // Balance
            // 
            this.Balance.HeaderText = "Balance";
            this.Balance.Name = "Balance";
            // 
            // SPrincipal
            // 
            this.SPrincipal.HeaderText = "Scheduled Principal";
            this.SPrincipal.Name = "SPrincipal";
            // 
            // UPrincipal
            // 
            this.UPrincipal.HeaderText = "Unscheduled Principal";
            this.UPrincipal.Name = "UPrincipal";
            // 
            // TPrincipal
            // 
            this.TPrincipal.HeaderText = "Total Principal";
            this.TPrincipal.Name = "TPrincipal";
            // 
            // Intererst
            // 
            this.Intererst.HeaderText = "Interest";
            this.Intererst.Name = "Intererst";
            // 
            // Total
            // 
            this.Total.HeaderText = "Total";
            this.Total.Name = "Total";
            // 
            // CollateralCashFlowsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1185, 633);
            this.Controls.Add(this.collateralCashFlowsGridView);
            this.Name = "CollateralCashFlowsForm";
            this.Text = "CollateralCashFlowsForm";
            this.Load += new System.EventHandler(this.CollateralCashFlowsForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.collateralCashFlowsGridView)).EndInit();
            this.ResumeLayout(false);
        }


        #endregion

        private System.Windows.Forms.DataGridView collateralCashFlowsGridView;
        private System.Windows.Forms.DataGridViewTextBoxColumn No;
        private System.Windows.Forms.DataGridViewTextBoxColumn Period;
        private System.Windows.Forms.DataGridViewTextBoxColumn Balance;
        private System.Windows.Forms.DataGridViewTextBoxColumn SPrincipal;
        private System.Windows.Forms.DataGridViewTextBoxColumn UPrincipal;
        private System.Windows.Forms.DataGridViewTextBoxColumn TPrincipal;
        private System.Windows.Forms.DataGridViewTextBoxColumn Intererst;
        private System.Windows.Forms.DataGridViewTextBoxColumn Total;
    }
}