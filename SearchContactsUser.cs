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
    public partial class SearchContactsUser : UserControl
    {
        public SearchContactsUser()
        {
            InitializeComponent();
        }

        private void btnSearchName_Click(object sender, EventArgs e)
        {
            if (txtSearchContacts.Visible == true)
            {
                txtSearchContacts.Hide();
            }
            else
            {
                txtSearchContacts.Show();
            }
        }
    }
}
