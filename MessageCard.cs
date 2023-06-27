using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Projeto_Tópicos_de_Segurança
{
    public partial class MessageCard : UserControl
    {
        public MessageCard()
        {
            InitializeComponent();
        }

        #region Properties

        private string _name;
        private string _message;
        private Image _icon;
        private string _date;

        [Category("Custom Props")]
        public string Name
        {
            get { return _name; }
            set { _name = value; labelName.Text = value; }
        }

        [Category("Custom Props")]
        public string Message
        {
            get { return _message; }
            set { _message = value; lbMessage.Text = value; }
        }

        [Category("Custom Props")]
        public Image Icon
        {
            get { return _icon; }
            set { _icon = value; pictureIconUser.Image = value; }
        }

        [Category("Custom Props")]
        public string Date
        {
            get { return _date; }
            set { _date = value; lbDate.Text = value; }
        }
        #endregion

        private void lbMessage_Click(object sender, EventArgs e)
        {

        }

        private void MessageCard_Load(object sender, EventArgs e)
        {

        }
    }
}
