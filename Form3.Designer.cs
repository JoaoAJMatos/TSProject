namespace Projeto_Tópicos_de_Segurança
{
    partial class Form3
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
            this.panel2 = new System.Windows.Forms.Panel();
            this.btnLogout = new System.Windows.Forms.Button();
            this.btnEditProfile = new System.Windows.Forms.Button();
            this.labelUsername = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.panel3 = new System.Windows.Forms.Panel();
            this.flowLayoutPanelChannels = new System.Windows.Forms.FlowLayoutPanel();
            this.panelContacts = new System.Windows.Forms.Panel();
            this.txtSearchContacts = new System.Windows.Forms.TextBox();
            this.btnSearchName = new System.Windows.Forms.Button();
            this.btnAddContact = new System.Windows.Forms.Button();
            this.labelChannelCount = new System.Windows.Forms.Label();
            this.lbContacts = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.labelRemainingCharacters = new System.Windows.Forms.Label();
            this.SendMessage = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.txtMessage = new System.Windows.Forms.TextBox();
            this.flowLayoutPanelMessages = new System.Windows.Forms.FlowLayoutPanel();
            this.buttonAtualizar = new System.Windows.Forms.Button();
            this.panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.panel3.SuspendLayout();
            this.panelContacts.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel2
            // 
            this.panel2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel2.Controls.Add(this.btnLogout);
            this.panel2.Controls.Add(this.btnEditProfile);
            this.panel2.Controls.Add(this.labelUsername);
            this.panel2.Controls.Add(this.pictureBox1);
            this.panel2.Location = new System.Drawing.Point(0, 327);
            this.panel2.Margin = new System.Windows.Forms.Padding(2);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(289, 79);
            this.panel2.TabIndex = 2;
            // 
            // btnLogout
            // 
            this.btnLogout.BackgroundImage = global::Projeto_Tópicos_de_Segurança.Properties.Resources.leave;
            this.btnLogout.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.btnLogout.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnLogout.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnLogout.ForeColor = System.Drawing.Color.Transparent;
            this.btnLogout.ImageAlign = System.Drawing.ContentAlignment.TopLeft;
            this.btnLogout.Location = new System.Drawing.Point(247, 26);
            this.btnLogout.Name = "btnLogout";
            this.btnLogout.Size = new System.Drawing.Size(25, 32);
            this.btnLogout.TabIndex = 3;
            this.btnLogout.UseVisualStyleBackColor = true;
            this.btnLogout.Click += new System.EventHandler(this.btnLogout_Click);
            // 
            // btnEditProfile
            // 
            this.btnEditProfile.BackColor = System.Drawing.Color.Transparent;
            this.btnEditProfile.BackgroundImage = global::Projeto_Tópicos_de_Segurança.Properties.Resources.editar_2_;
            this.btnEditProfile.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.btnEditProfile.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnEditProfile.FlatAppearance.BorderSize = 0;
            this.btnEditProfile.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnEditProfile.Location = new System.Drawing.Point(207, 26);
            this.btnEditProfile.Margin = new System.Windows.Forms.Padding(2);
            this.btnEditProfile.Name = "btnEditProfile";
            this.btnEditProfile.Size = new System.Drawing.Size(25, 29);
            this.btnEditProfile.TabIndex = 2;
            this.btnEditProfile.UseVisualStyleBackColor = false;
            this.btnEditProfile.Click += new System.EventHandler(this.btnEditProfile_Click);
            // 
            // labelUsername
            // 
            this.labelUsername.AutoSize = true;
            this.labelUsername.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelUsername.Location = new System.Drawing.Point(76, 32);
            this.labelUsername.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelUsername.Name = "labelUsername";
            this.labelUsername.Size = new System.Drawing.Size(99, 20);
            this.labelUsername.TabIndex = 1;
            this.labelUsername.Text = "Profile Name";
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::Projeto_Tópicos_de_Segurança.Properties.Resources.andrew;
            this.pictureBox1.Location = new System.Drawing.Point(9, 11);
            this.pictureBox1.Margin = new System.Windows.Forms.Padding(2);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(52, 57);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.buttonAtualizar);
            this.panel3.Controls.Add(this.flowLayoutPanelChannels);
            this.panel3.Controls.Add(this.panelContacts);
            this.panel3.Location = new System.Drawing.Point(0, 0);
            this.panel3.Margin = new System.Windows.Forms.Padding(2);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(289, 327);
            this.panel3.TabIndex = 3;
            // 
            // flowLayoutPanelChannels
            // 
            this.flowLayoutPanelChannels.AutoScroll = true;
            this.flowLayoutPanelChannels.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.flowLayoutPanelChannels.Location = new System.Drawing.Point(0, 70);
            this.flowLayoutPanelChannels.Margin = new System.Windows.Forms.Padding(2);
            this.flowLayoutPanelChannels.Name = "flowLayoutPanelChannels";
            this.flowLayoutPanelChannels.Size = new System.Drawing.Size(289, 233);
            this.flowLayoutPanelChannels.TabIndex = 1;
            // 
            // panelContacts
            // 
            this.panelContacts.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelContacts.Controls.Add(this.txtSearchContacts);
            this.panelContacts.Controls.Add(this.btnSearchName);
            this.panelContacts.Controls.Add(this.btnAddContact);
            this.panelContacts.Controls.Add(this.labelChannelCount);
            this.panelContacts.Controls.Add(this.lbContacts);
            this.panelContacts.Location = new System.Drawing.Point(0, 0);
            this.panelContacts.Margin = new System.Windows.Forms.Padding(2);
            this.panelContacts.Name = "panelContacts";
            this.panelContacts.Size = new System.Drawing.Size(289, 73);
            this.panelContacts.TabIndex = 0;
            // 
            // txtSearchContacts
            // 
            this.txtSearchContacts.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtSearchContacts.Location = new System.Drawing.Point(23, 18);
            this.txtSearchContacts.Margin = new System.Windows.Forms.Padding(2);
            this.txtSearchContacts.Multiline = true;
            this.txtSearchContacts.Name = "txtSearchContacts";
            this.txtSearchContacts.Size = new System.Drawing.Size(209, 28);
            this.txtSearchContacts.TabIndex = 10;
            this.txtSearchContacts.Visible = false;
            this.txtSearchContacts.TextChanged += new System.EventHandler(this.txtSearchContacts_TextChanged);
            // 
            // btnSearchName
            // 
            this.btnSearchName.BackColor = System.Drawing.Color.Transparent;
            this.btnSearchName.BackgroundImage = global::Projeto_Tópicos_de_Segurança.Properties.Resources.magnifier;
            this.btnSearchName.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.btnSearchName.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSearchName.FlatAppearance.BorderSize = 0;
            this.btnSearchName.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSearchName.Location = new System.Drawing.Point(241, 18);
            this.btnSearchName.Margin = new System.Windows.Forms.Padding(2);
            this.btnSearchName.Name = "btnSearchName";
            this.btnSearchName.Size = new System.Drawing.Size(24, 26);
            this.btnSearchName.TabIndex = 4;
            this.btnSearchName.UseVisualStyleBackColor = false;
            this.btnSearchName.Click += new System.EventHandler(this.btnSearchName_Click);
            // 
            // btnAddContact
            // 
            this.btnAddContact.BackColor = System.Drawing.Color.Transparent;
            this.btnAddContact.BackgroundImage = global::Projeto_Tópicos_de_Segurança.Properties.Resources.add_friend;
            this.btnAddContact.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.btnAddContact.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnAddContact.FlatAppearance.BorderSize = 0;
            this.btnAddContact.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAddContact.Location = new System.Drawing.Point(196, 20);
            this.btnAddContact.Margin = new System.Windows.Forms.Padding(2);
            this.btnAddContact.Name = "btnAddContact";
            this.btnAddContact.Size = new System.Drawing.Size(24, 26);
            this.btnAddContact.TabIndex = 3;
            this.btnAddContact.UseVisualStyleBackColor = false;
            this.btnAddContact.Click += new System.EventHandler(this.btnAddContact_Click);
            // 
            // labelChannelCount
            // 
            this.labelChannelCount.AutoSize = true;
            this.labelChannelCount.BackColor = System.Drawing.Color.Transparent;
            this.labelChannelCount.Location = new System.Drawing.Point(97, 26);
            this.labelChannelCount.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelChannelCount.Name = "labelChannelCount";
            this.labelChannelCount.Size = new System.Drawing.Size(31, 13);
            this.labelChannelCount.TabIndex = 1;
            this.labelChannelCount.Text = "(99+)";
            // 
            // lbContacts
            // 
            this.lbContacts.AutoSize = true;
            this.lbContacts.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbContacts.Location = new System.Drawing.Point(19, 26);
            this.lbContacts.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lbContacts.Name = "lbContacts";
            this.lbContacts.Size = new System.Drawing.Size(81, 20);
            this.lbContacts.TabIndex = 0;
            this.lbContacts.Text = "Contacts";
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Controls.Add(this.labelRemainingCharacters);
            this.panel1.Controls.Add(this.SendMessage);
            this.panel1.Controls.Add(this.button4);
            this.panel1.Controls.Add(this.txtMessage);
            this.panel1.Location = new System.Drawing.Point(289, 327);
            this.panel1.Margin = new System.Windows.Forms.Padding(2);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(596, 79);
            this.panel1.TabIndex = 4;
            this.panel1.Paint += new System.Windows.Forms.PaintEventHandler(this.panel1_Paint);
            // 
            // labelRemainingCharacters
            // 
            this.labelRemainingCharacters.AutoSize = true;
            this.labelRemainingCharacters.Location = new System.Drawing.Point(18, 34);
            this.labelRemainingCharacters.Name = "labelRemainingCharacters";
            this.labelRemainingCharacters.Size = new System.Drawing.Size(81, 13);
            this.labelRemainingCharacters.TabIndex = 7;
            this.labelRemainingCharacters.Text = "Remaining: 500";
            // 
            // SendMessage
            // 
            this.SendMessage.BackColor = System.Drawing.Color.Transparent;
            this.SendMessage.BackgroundImage = global::Projeto_Tópicos_de_Segurança.Properties.Resources.enviar;
            this.SendMessage.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.SendMessage.Cursor = System.Windows.Forms.Cursors.Hand;
            this.SendMessage.FlatAppearance.BorderSize = 0;
            this.SendMessage.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.SendMessage.Location = new System.Drawing.Point(541, 26);
            this.SendMessage.Margin = new System.Windows.Forms.Padding(2);
            this.SendMessage.Name = "SendMessage";
            this.SendMessage.Size = new System.Drawing.Size(27, 29);
            this.SendMessage.TabIndex = 6;
            this.SendMessage.UseVisualStyleBackColor = false;
            this.SendMessage.Click += new System.EventHandler(this.SendMessage_Click);
            // 
            // button4
            // 
            this.button4.BackColor = System.Drawing.Color.Transparent;
            this.button4.BackgroundImage = global::Projeto_Tópicos_de_Segurança.Properties.Resources.anexar_simbolo_diagonal_de_clipe_de_papel;
            this.button4.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.button4.Cursor = System.Windows.Forms.Cursors.Hand;
            this.button4.FlatAppearance.BorderSize = 0;
            this.button4.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button4.Location = new System.Drawing.Point(494, 26);
            this.button4.Margin = new System.Windows.Forms.Padding(2);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(27, 29);
            this.button4.TabIndex = 5;
            this.button4.UseVisualStyleBackColor = false;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // txtMessage
            // 
            this.txtMessage.BackColor = System.Drawing.Color.Silver;
            this.txtMessage.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtMessage.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtMessage.Location = new System.Drawing.Point(104, 29);
            this.txtMessage.Margin = new System.Windows.Forms.Padding(2);
            this.txtMessage.Name = "txtMessage";
            this.txtMessage.Size = new System.Drawing.Size(376, 23);
            this.txtMessage.TabIndex = 0;
            this.txtMessage.TextChanged += new System.EventHandler(this.txtMessage_TextChanged);
            this.txtMessage.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtMessage_KeyDown);
            // 
            // flowLayoutPanelMessages
            // 
            this.flowLayoutPanelMessages.AutoScroll = true;
            this.flowLayoutPanelMessages.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.flowLayoutPanelMessages.Location = new System.Drawing.Point(289, 0);
            this.flowLayoutPanelMessages.Name = "flowLayoutPanelMessages";
            this.flowLayoutPanelMessages.Size = new System.Drawing.Size(596, 327);
            this.flowLayoutPanelMessages.TabIndex = 0;
            // 
            // buttonAtualizar
            // 
            this.buttonAtualizar.BackColor = System.Drawing.Color.Gainsboro;
            this.buttonAtualizar.Location = new System.Drawing.Point(0, 302);
            this.buttonAtualizar.Name = "buttonAtualizar";
            this.buttonAtualizar.Size = new System.Drawing.Size(289, 25);
            this.buttonAtualizar.TabIndex = 2;
            this.buttonAtualizar.Text = "Update Friends List";
            this.buttonAtualizar.UseVisualStyleBackColor = false;
            this.buttonAtualizar.Click += new System.EventHandler(this.buttonAtualizar_Click);
            // 
            // Form3
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(884, 405);
            this.Controls.Add(this.flowLayoutPanelMessages);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.panel3);
            this.Controls.Add(this.panel2);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "Form3";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "IPL Chat";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form3_FormClosing);
            this.Load += new System.EventHandler(this.Form3_Load);
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.panel3.ResumeLayout(false);
            this.panelContacts.ResumeLayout(false);
            this.panelContacts.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Label labelUsername;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Button btnEditProfile;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Panel panelContacts;
        private System.Windows.Forms.Button btnAddContact;
        private System.Windows.Forms.Label labelChannelCount;
        private System.Windows.Forms.Label lbContacts;
        private System.Windows.Forms.Button btnSearchName;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelChannels;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button SendMessage;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.TextBox txtMessage;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelMessages;
        private System.Windows.Forms.TextBox txtSearchContacts;
        private System.Windows.Forms.Label labelRemainingCharacters;
        private System.Windows.Forms.Button btnLogout;
        private System.Windows.Forms.Button buttonAtualizar;
    }
}