namespace SOA___Assignment_2___Web_Services
{
    partial class MainForm
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
            this.cmbService = new System.Windows.Forms.ComboBox();
            this.cmbMethod = new System.Windows.Forms.ComboBox();
            this.lblService = new System.Windows.Forms.Label();
            this.lblMethod = new System.Windows.Forms.Label();
            this.lblResponse = new System.Windows.Forms.Label();
            this.gridResponse = new System.Windows.Forms.DataGridView();
            this.btnInvoke = new System.Windows.Forms.Button();
            this.gridArguments = new System.Windows.Forms.DataGridView();
            this.grpArgumentControls = new System.Windows.Forms.GroupBox();
            this.Results = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.gridResponse)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridArguments)).BeginInit();
            this.SuspendLayout();
            // 
            // cmbService
            // 
            this.cmbService.FormattingEnabled = true;
            this.cmbService.Location = new System.Drawing.Point(100, 15);
            this.cmbService.Margin = new System.Windows.Forms.Padding(4);
            this.cmbService.Name = "cmbService";
            this.cmbService.Size = new System.Drawing.Size(160, 24);
            this.cmbService.TabIndex = 0;
            // 
            // cmbMethod
            // 
            this.cmbMethod.FormattingEnabled = true;
            this.cmbMethod.Location = new System.Drawing.Point(100, 48);
            this.cmbMethod.Margin = new System.Windows.Forms.Padding(4);
            this.cmbMethod.Name = "cmbMethod";
            this.cmbMethod.Size = new System.Drawing.Size(160, 24);
            this.cmbMethod.TabIndex = 1;
            // 
            // lblService
            // 
            this.lblService.AutoSize = true;
            this.lblService.Location = new System.Drawing.Point(35, 18);
            this.lblService.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblService.Name = "lblService";
            this.lblService.Size = new System.Drawing.Size(55, 17);
            this.lblService.TabIndex = 2;
            this.lblService.Text = "Service";
            // 
            // lblMethod
            // 
            this.lblMethod.AutoSize = true;
            this.lblMethod.Location = new System.Drawing.Point(35, 52);
            this.lblMethod.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblMethod.Name = "lblMethod";
            this.lblMethod.Size = new System.Drawing.Size(55, 17);
            this.lblMethod.TabIndex = 3;
            this.lblMethod.Text = "Method";
            // 
            // lblResponse
            // 
            this.lblResponse.AutoSize = true;
            this.lblResponse.Location = new System.Drawing.Point(19, 313);
            this.lblResponse.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblResponse.Name = "lblResponse";
            this.lblResponse.Size = new System.Drawing.Size(72, 17);
            this.lblResponse.TabIndex = 4;
            this.lblResponse.Text = "Response";
            // 
            // gridResponse
            // 
            this.gridResponse.AllowUserToAddRows = false;
            this.gridResponse.AllowUserToDeleteRows = false;
            this.gridResponse.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.gridResponse.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Results});
            this.gridResponse.Location = new System.Drawing.Point(100, 313);
            this.gridResponse.Margin = new System.Windows.Forms.Padding(4);
            this.gridResponse.Name = "gridResponse";
            this.gridResponse.ReadOnly = true;
            this.gridResponse.Size = new System.Drawing.Size(529, 185);
            this.gridResponse.TabIndex = 4;
            // 
            // btnInvoke
            // 
            this.btnInvoke.Location = new System.Drawing.Point(100, 277);
            this.btnInvoke.Margin = new System.Windows.Forms.Padding(4);
            this.btnInvoke.Name = "btnInvoke";
            this.btnInvoke.Size = new System.Drawing.Size(100, 28);
            this.btnInvoke.TabIndex = 3;
            this.btnInvoke.Text = "Invoke";
            this.btnInvoke.UseVisualStyleBackColor = true;
            this.btnInvoke.Click += new System.EventHandler(this.btnInvoke_ClickAsync);
            // 
            // gridArguments
            // 
            this.gridArguments.AllowUserToAddRows = false;
            this.gridArguments.AllowUserToDeleteRows = false;
            this.gridArguments.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.gridArguments.Location = new System.Drawing.Point(570, 85);
            this.gridArguments.Margin = new System.Windows.Forms.Padding(4);
            this.gridArguments.Name = "gridArguments";
            this.gridArguments.Size = new System.Drawing.Size(59, 185);
            this.gridArguments.TabIndex = 2;
            // 
            // grpArgumentControls
            // 
            this.grpArgumentControls.Location = new System.Drawing.Point(38, 85);
            this.grpArgumentControls.Name = "grpArgumentControls";
            this.grpArgumentControls.Size = new System.Drawing.Size(515, 185);
            this.grpArgumentControls.TabIndex = 8;
            this.grpArgumentControls.TabStop = false;
            this.grpArgumentControls.Text = "Arguments";
            // 
            // Results
            // 
            this.Results.HeaderText = "Results";
            this.Results.Name = "Results";
            this.Results.ReadOnly = true;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(645, 510);
            this.Controls.Add(this.grpArgumentControls);
            this.Controls.Add(this.gridArguments);
            this.Controls.Add(this.btnInvoke);
            this.Controls.Add(this.gridResponse);
            this.Controls.Add(this.lblResponse);
            this.Controls.Add(this.lblMethod);
            this.Controls.Add(this.lblService);
            this.Controls.Add(this.cmbMethod);
            this.Controls.Add(this.cmbService);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "MainForm";
            this.Text = "Assignment 2 - Web Services";
            ((System.ComponentModel.ISupportInitialize)(this.gridResponse)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridArguments)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox cmbService;
        private System.Windows.Forms.ComboBox cmbMethod;
        private System.Windows.Forms.Label lblService;
        private System.Windows.Forms.Label lblMethod;
        private System.Windows.Forms.Label lblResponse;
        private System.Windows.Forms.DataGridView gridResponse;
		private System.Windows.Forms.Button btnInvoke;
        private System.Windows.Forms.DataGridView gridArguments;
        private System.Windows.Forms.GroupBox grpArgumentControls;
        private System.Windows.Forms.DataGridViewTextBoxColumn Results;
    }
}

