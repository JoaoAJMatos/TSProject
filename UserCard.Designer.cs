namespace Projeto_Tópicos_de_Segurança
{
    partial class UserCard
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
            this.lbTitle = new System.Windows.Forms.Label();
            this.pictureIconUser = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureIconUser)).BeginInit();
            this.SuspendLayout();
            // 
            // lbTitle
            // 
            this.lbTitle.AutoSize = true;
            this.lbTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbTitle.Location = new System.Drawing.Point(89, 31);
            this.lbTitle.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lbTitle.Name = "lbTitle";
            this.lbTitle.Size = new System.Drawing.Size(94, 18);
            this.lbTitle.TabIndex = 1;
            this.lbTitle.Text = "Profile Name";
            this.lbTitle.MouseEnter += new System.EventHandler(this.ListUser_MouseEnter);
            this.lbTitle.MouseLeave += new System.EventHandler(this.ListUser_MouseLeave);
            // 
            // pictureIconUser
            // 
            this.pictureIconUser.Image = global::Projeto_Tópicos_de_Segurança.Properties.Resources.andrew;
            this.pictureIconUser.Location = new System.Drawing.Point(14, 12);
            this.pictureIconUser.Margin = new System.Windows.Forms.Padding(2);
            this.pictureIconUser.Name = "pictureIconUser";
            this.pictureIconUser.Size = new System.Drawing.Size(53, 55);
            this.pictureIconUser.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureIconUser.TabIndex = 0;
            this.pictureIconUser.TabStop = false;
            this.pictureIconUser.MouseEnter += new System.EventHandler(this.ListUser_MouseEnter);
            this.pictureIconUser.MouseLeave += new System.EventHandler(this.ListUser_MouseLeave);
            // 
            // UserCard
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Controls.Add(this.lbTitle);
            this.Controls.Add(this.pictureIconUser);
            this.Cursor = System.Windows.Forms.Cursors.Hand;
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "UserCard";
            this.Size = new System.Drawing.Size(281, 80);
            this.Click += new System.EventHandler(this.ListUser_Click);
            this.MouseEnter += new System.EventHandler(this.ListUser_MouseEnter);
            this.MouseLeave += new System.EventHandler(this.ListUser_MouseLeave);
            ((System.ComponentModel.ISupportInitialize)(this.pictureIconUser)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureIconUser;
        private System.Windows.Forms.Label lbTitle;
    }
}
