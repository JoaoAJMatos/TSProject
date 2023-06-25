namespace Projeto_Tópicos_de_Segurança
{
    partial class AddContact
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
            this.txtUsername = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btnSearchName = new System.Windows.Forms.Button();
            this.flowLayoutPanelSearchResults = new System.Windows.Forms.FlowLayoutPanel();
            this.SuspendLayout();
            // 
            // txtUsername
            // 
            this.txtUsername.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtUsername.Location = new System.Drawing.Point(31, 55);
            this.txtUsername.Margin = new System.Windows.Forms.Padding(2);
            this.txtUsername.Name = "txtUsername";
            this.txtUsername.Size = new System.Drawing.Size(261, 24);
            this.txtUsername.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(27, 32);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(123, 20);
            this.label1.TabIndex = 1;
            this.label1.Text = "Enter username";
            // 
            // btnSearchName
            // 
            this.btnSearchName.BackColor = System.Drawing.Color.Transparent;
            this.btnSearchName.BackgroundImage = global::Projeto_Tópicos_de_Segurança.Properties.Resources.magnifier;
            this.btnSearchName.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.btnSearchName.FlatAppearance.BorderSize = 0;
            this.btnSearchName.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSearchName.Location = new System.Drawing.Point(296, 53);
            this.btnSearchName.Margin = new System.Windows.Forms.Padding(2);
            this.btnSearchName.Name = "btnSearchName";
            this.btnSearchName.Size = new System.Drawing.Size(24, 26);
            this.btnSearchName.TabIndex = 5;
            this.btnSearchName.UseVisualStyleBackColor = false;
            this.btnSearchName.Click += new System.EventHandler(this.btnSearchName_Click);
            // 
            // flowLayoutPanelSearchResults
            // 
            this.flowLayoutPanelSearchResults.AutoScroll = true;
            this.flowLayoutPanelSearchResults.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.flowLayoutPanelSearchResults.Location = new System.Drawing.Point(31, 100);
            this.flowLayoutPanelSearchResults.Margin = new System.Windows.Forms.Padding(2);
            this.flowLayoutPanelSearchResults.Name = "flowLayoutPanelSearchResults";
            this.flowLayoutPanelSearchResults.Size = new System.Drawing.Size(289, 218);
            this.flowLayoutPanelSearchResults.TabIndex = 6;
            // 
            // AddContact
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(364, 327);
            this.Controls.Add(this.flowLayoutPanelSearchResults);
            this.Controls.Add(this.btnSearchName);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtUsername);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "AddContact";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "AddContactUser";
            this.Load += new System.EventHandler(this.AddContact_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtUsername;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnSearchName;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelSearchResults;
    }
}