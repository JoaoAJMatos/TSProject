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
    public partial class AddContact : Form
    {
        Client _clientInstance = Client.GetInstance();

        public AddContact()
        {
            InitializeComponent();
        }

        // Performs a search on the database for users whose name match the pattern entered on the textbox.
        // After that, it will populate the search results with the users found.
        // The search results don't need to be exactly right, they can be partial matches.
        private void btnSearchName_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text;
            List<SChannel> users = _clientInstance.SearchUserByName(username);

            flowLayoutPanelSearchResults.Controls.Clear();

            for (int i = 0; i < users.Count; i++)
            {
                SChannel user = users[i];

                UserCard listUser = new UserCard();
                listUser.Title = user._name;
                listUser.ChannelUUID = user._uuid;

                flowLayoutPanelSearchResults.Controls.Add(listUser);

                listUser.Click += new EventHandler(UserCard_Click);
            }
        }

        // Event that is called when the user clicks on the user cards on the search results.
        // This event will subscribe the user to the channel of the user that was clicked.
        // After that, it will update the users list on the main form and close this form.
        private void UserCard_Click(object sender, EventArgs e)
        {
            UserCard userCard = (UserCard)sender;
            _clientInstance.SubscribeToChannel(userCard.Title, userCard.ChannelUUID);
            Form3 form3 = (Form3)Application.OpenForms["Form3"];
            form3.populateUsers();
            this.Close();
        }

        private void AddContact_Load(object sender, EventArgs e)
        {

        }
    }
}
