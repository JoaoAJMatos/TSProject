namespace Projeto_Tópicos_de_Segurança
{
    partial class SearchContactsUser
    {
        /// <summary> 
        /// Variável de designer necessária.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Limpar os recursos que estão sendo usados.
        /// </summary>
        /// <param name="disposing">true se for necessário descartar os recursos gerenciados; caso contrário, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Código gerado pelo Designer de Componentes

        /// <summary> 
        /// Método necessário para suporte ao Designer - não modifique 
        /// o conteúdo deste método com o editor de código.
        /// </summary>
        private void InitializeComponent()
        {
            this.label3 = new System.Windows.Forms.Label();
            this.lbContacts = new System.Windows.Forms.Label();
            this.btnSearchName = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.txtSearchContacts = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.BackColor = System.Drawing.Color.Transparent;
            this.label3.Location = new System.Drawing.Point(120, 30);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(36, 16);
            this.label3.TabIndex = 6;
            this.label3.Text = "(99+)";
            // 
            // lbContacts
            // 
            this.lbContacts.AutoSize = true;
            this.lbContacts.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbContacts.Location = new System.Drawing.Point(16, 30);
            this.lbContacts.Name = "lbContacts";
            this.lbContacts.Size = new System.Drawing.Size(98, 25);
            this.lbContacts.TabIndex = 5;
            this.lbContacts.Text = "Contacts";
            // 
            // btnSearchName
            // 
            this.btnSearchName.BackColor = System.Drawing.Color.Transparent;
            this.btnSearchName.FlatAppearance.BorderSize = 0;
            this.btnSearchName.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSearchName.Image = global::Projeto_Tópicos_de_Segurança.Properties.Resources.magnifier;
            this.btnSearchName.Location = new System.Drawing.Point(304, 20);
            this.btnSearchName.Name = "btnSearchName";
            this.btnSearchName.Size = new System.Drawing.Size(32, 32);
            this.btnSearchName.TabIndex = 8;
            this.btnSearchName.UseVisualStyleBackColor = false;
            this.btnSearchName.Click += new System.EventHandler(this.btnSearchName_Click);
            // 
            // button2
            // 
            this.button2.BackColor = System.Drawing.Color.Transparent;
            this.button2.FlatAppearance.BorderSize = 0;
            this.button2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button2.Image = global::Projeto_Tópicos_de_Segurança.Properties.Resources.add_friend;
            this.button2.Location = new System.Drawing.Point(240, 20);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(32, 32);
            this.button2.TabIndex = 7;
            this.button2.UseVisualStyleBackColor = false;
            // 
            // txtSearchContacts
            // 
            this.txtSearchContacts.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtSearchContacts.Location = new System.Drawing.Point(21, 22);
            this.txtSearchContacts.Multiline = true;
            this.txtSearchContacts.Name = "txtSearchContacts";
            this.txtSearchContacts.Size = new System.Drawing.Size(277, 33);
            this.txtSearchContacts.TabIndex = 9;
            this.txtSearchContacts.Visible = false;
            // 
            // SearchContactsUser
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.txtSearchContacts);
            this.Controls.Add(this.btnSearchName);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.lbContacts);
            this.Name = "SearchContactsUser";
            this.Size = new System.Drawing.Size(368, 82);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnSearchName;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label lbContacts;
        private System.Windows.Forms.TextBox txtSearchContacts;
    }
}
