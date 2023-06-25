using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Projeto_Tópicos_de_Segurança
{
    public partial class UserCard : UserControl
    {
        Client _clientInstance = Client.GetInstance();

        public UserCard()
        {
            InitializeComponent();
        }

        #region Properties
        
        private string _title;
        private string _channelUUID;
        private Image _icon;

        [Category("Custom Props")]
        public string Title
        {
            get { return _title; }
            set { _title = value; lbTitle.Text = value; }
        }

        [Category("Custom Props")]
        public Image Icon
        {
            get { return _icon; }
            set { _icon = value; pictureIconUser.Image = value; }
        }

        [Category("Custom Props")]
        public string ChannelUUID
        {
            get { return _channelUUID; }
            set { _channelUUID = value; }
        }

        #endregion

        private void ListUser_Click(object sender, EventArgs e)
        {
            _clientInstance._activeChannelUUID = _channelUUID;
        }

        private void ListUser_MouseEnter(object sender, EventArgs e)
        {
            this.BackColor = Color.Silver;
        }

        private void ListUser_MouseLeave(object sender, EventArgs e)
        {
            if (_clientInstance._activeChannelUUID != _channelUUID)
                this.BackColor = Color.White;
        }
    }
}
