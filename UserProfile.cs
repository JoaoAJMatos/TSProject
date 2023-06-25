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
    public partial class UserProfile : Form
    {
        Client clientInstance = Client.GetInstance();

        public UserProfile()
        {
            InitializeComponent();
        }

        private void UserProfile_Load(object sender, EventArgs e)
        {
            labelUsername.Text = clientInstance._username;
        }
    }
}
