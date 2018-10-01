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
			this.btnInvoke = new System.Windows.Forms.Button();
			this.grpArgumentControls = new System.Windows.Forms.GroupBox();
			this.txtResults = new System.Windows.Forms.TextBox();
			this.SuspendLayout();
			// 
			// cmbService
			// 
			this.cmbService.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.cmbService.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cmbService.FormattingEnabled = true;
			this.cmbService.Location = new System.Drawing.Point(61, 6);
			this.cmbService.Name = "cmbService";
			this.cmbService.Size = new System.Drawing.Size(410, 21);
			this.cmbService.TabIndex = 0;
			this.cmbService.SelectedIndexChanged += new System.EventHandler(this.cmbService_SelectedIndexChanged);
			// 
			// cmbMethod
			// 
			this.cmbMethod.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.cmbMethod.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cmbMethod.FormattingEnabled = true;
			this.cmbMethod.Location = new System.Drawing.Point(61, 33);
			this.cmbMethod.Name = "cmbMethod";
			this.cmbMethod.Size = new System.Drawing.Size(410, 21);
			this.cmbMethod.TabIndex = 1;
			this.cmbMethod.SelectedIndexChanged += new System.EventHandler(this.cmbMethod_SelectedIndexChanged);
			// 
			// lblService
			// 
			this.lblService.AutoSize = true;
			this.lblService.Location = new System.Drawing.Point(12, 9);
			this.lblService.Name = "lblService";
			this.lblService.Size = new System.Drawing.Size(43, 13);
			this.lblService.TabIndex = 2;
			this.lblService.Text = "Service";
			// 
			// lblMethod
			// 
			this.lblMethod.AutoSize = true;
			this.lblMethod.Location = new System.Drawing.Point(12, 36);
			this.lblMethod.Name = "lblMethod";
			this.lblMethod.Size = new System.Drawing.Size(43, 13);
			this.lblMethod.TabIndex = 3;
			this.lblMethod.Text = "Method";
			// 
			// btnInvoke
			// 
			this.btnInvoke.Location = new System.Drawing.Point(12, 224);
			this.btnInvoke.Name = "btnInvoke";
			this.btnInvoke.Size = new System.Drawing.Size(75, 23);
			this.btnInvoke.TabIndex = 3;
			this.btnInvoke.Text = "Invoke";
			this.btnInvoke.UseVisualStyleBackColor = true;
			this.btnInvoke.Click += new System.EventHandler(this.btnInvoke_ClickAsync);
			// 
			// grpArgumentControls
			// 
			this.grpArgumentControls.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.grpArgumentControls.Location = new System.Drawing.Point(15, 59);
			this.grpArgumentControls.Margin = new System.Windows.Forms.Padding(2);
			this.grpArgumentControls.Name = "grpArgumentControls";
			this.grpArgumentControls.Padding = new System.Windows.Forms.Padding(2);
			this.grpArgumentControls.Size = new System.Drawing.Size(456, 160);
			this.grpArgumentControls.TabIndex = 8;
			this.grpArgumentControls.TabStop = false;
			this.grpArgumentControls.Text = "Arguments";
			// 
			// txtResults
			// 
			this.txtResults.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txtResults.Location = new System.Drawing.Point(12, 253);
			this.txtResults.Multiline = true;
			this.txtResults.Name = "txtResults";
			this.txtResults.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.txtResults.Size = new System.Drawing.Size(460, 149);
			this.txtResults.TabIndex = 9;
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(484, 414);
			this.Controls.Add(this.txtResults);
			this.Controls.Add(this.grpArgumentControls);
			this.Controls.Add(this.btnInvoke);
			this.Controls.Add(this.lblMethod);
			this.Controls.Add(this.lblService);
			this.Controls.Add(this.cmbMethod);
			this.Controls.Add(this.cmbService);
			this.Name = "MainForm";
			this.Text = "Assignment 2 - Web Services";
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox cmbService;
        private System.Windows.Forms.ComboBox cmbMethod;
        private System.Windows.Forms.Label lblService;
        private System.Windows.Forms.Label lblMethod;
		private System.Windows.Forms.Button btnInvoke;
        private System.Windows.Forms.GroupBox grpArgumentControls;
        private System.Windows.Forms.TextBox txtResults;
    }
}

