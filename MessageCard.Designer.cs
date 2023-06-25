namespace Projeto_Tópicos_de_Segurança
{
    partial class MessageCard
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
            this.pictureIconUser = new System.Windows.Forms.PictureBox();
            this.lbName = new System.Windows.Forms.Label();
            this.lbMessage = new System.Windows.Forms.Label();
            this.lbDate = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.pictureIconUser)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureIconUser
            // 
            this.pictureIconUser.Image = global::Projeto_Tópicos_de_Segurança.Properties.Resources.andrew;
            this.pictureIconUser.Location = new System.Drawing.Point(19, 13);
            this.pictureIconUser.Margin = new System.Windows.Forms.Padding(2);
            this.pictureIconUser.Name = "pictureIconUser";
            this.pictureIconUser.Size = new System.Drawing.Size(64, 70);
            this.pictureIconUser.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureIconUser.TabIndex = 0;
            this.pictureIconUser.TabStop = false;
            // 
            // lbName
            // 
            this.lbName.AutoSize = true;
            this.lbName.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbName.Location = new System.Drawing.Point(98, 13);
            this.lbName.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lbName.Name = "lbName";
            this.lbName.Size = new System.Drawing.Size(51, 20);
            this.lbName.TabIndex = 1;
            this.lbName.Text = "Name";
            // 
            // lbMessage
            // 
            this.lbMessage.Location = new System.Drawing.Point(101, 47);
            this.lbMessage.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lbMessage.Name = "lbMessage";
            this.lbMessage.Size = new System.Drawing.Size(380, 36);
            this.lbMessage.TabIndex = 2;
            this.lbMessage.Text = "some text";
            this.lbMessage.Click += new System.EventHandler(this.lbMessage_Click);
            // 
            // lbDate
            // 
            this.lbDate.AutoSize = true;
            this.lbDate.Location = new System.Drawing.Point(531, 18);
            this.lbDate.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lbDate.Name = "lbDate";
            this.lbDate.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lbDate.Size = new System.Drawing.Size(28, 13);
            this.lbDate.TabIndex = 3;
            this.lbDate.Text = "date";
            // 
            // MessageCard
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Controls.Add(this.lbDate);
            this.Controls.Add(this.lbMessage);
            this.Controls.Add(this.lbName);
            this.Controls.Add(this.pictureIconUser);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "MessageCard";
            this.Size = new System.Drawing.Size(590, 94);
            this.Load += new System.EventHandler(this.MessageCard_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureIconUser)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureIconUser;
        private System.Windows.Forms.Label lbName;
        private System.Windows.Forms.Label lbMessage;
        private System.Windows.Forms.Label lbDate;
    }
}
