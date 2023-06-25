using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Projeto_Tópicos_de_Segurança
{
    public partial class Form3 : Form
    {
        private Client _clientInstance;

        public Form3()
        {
            InitializeComponent();
            _clientInstance = Client.GetInstance();
            _clientInstance.MessageReceivedCallback = MessageReceived;
        }

        private void Form3_Load(object sender, EventArgs e)
        {
            labelUsername.Text = _clientInstance._username;
            populateUsers();
        }
        
        private void ListUser_Click(object sender, EventArgs e)
        {
            UserCard userCard = (UserCard)sender;

            _clientInstance._activeChannelUUID = userCard.ChannelUUID;

            // Set the color of the selected user card to gray and the others to white
            foreach (UserCard userCard1 in flowLayoutPanelChannels.Controls)
            {
                if (userCard1.ChannelUUID == userCard.ChannelUUID)
                {
                    userCard1.BackColor = Color.Silver;
                }
                else
                {
                    userCard1.BackColor = Color.White;
                }
            }

            _clientInstance.Client2ClientHandshake(userCard.ChannelUUID);
            LoadMessages(userCard.ChannelUUID);
        }

        private void MessageReceived()
        {
            PopulateMessageLayout();
        }

        private void LoadMessages(string channelUUID)
        {
            // TODO: Note: Not needed
        }

        private void PopulateMessageLayout()
        {
            if (InvokeRequired) // To avoid cross-threading exception when updating the UI
            {
                Invoke(new MethodInvoker(delegate
                {
                    flowLayoutPanelMessages.Controls.Clear();
                }));
            }
            else
            {
                flowLayoutPanelMessages.Controls.Clear();
            }

            foreach (var message in _clientInstance._currentChannelMessages)
            {
                string senderName = _clientInstance.GetChannelNameFromUUID(message._senderID);
                byte[] encryptedMessage = message._message;
                byte[] decryptedMessage = _clientInstance._channelAESKeys[_clientInstance._activeChannelUUID].Decrypt(encryptedMessage);
                string messageContent = Encoding.UTF8.GetString(decryptedMessage);

                AppendMessageCard(messageContent, senderName);
            }
        }
            

        // Return the list of user cards in the flow layout pannel channels
        public List<UserCard> GetListedUserCards()
        {
            List<UserCard> userCards = new List<UserCard>();

            foreach (UserCard userCard in flowLayoutPanelChannels.Controls)
            {
                userCards.Add(userCard);
            }

            return userCards;
        }

        public void populateUsers()
        {
            if (InvokeRequired) // To avoid cross-threading exception when updating the UI
            {
                Invoke(new MethodInvoker(delegate
                {
                    flowLayoutPanelChannels.Controls.Clear();
                }));
            }
            else
            {
                flowLayoutPanelChannels.Controls.Clear();
            }

            List<SChannel> channels = _clientInstance.GetSubscribedChannels();

            for (int i = 0; i < channels.Count; i++)
            {
                SChannel channel = channels[i];
                UserCard listUser = new UserCard();
                listUser.Title = channel._name;
                listUser.ChannelUUID = channel._uuid;

                flowLayoutPanelChannels.Controls.Add(listUser);

                listUser.Click += new EventHandler(ListUser_Click);
            }

            labelChannelCount.Text = "(" + flowLayoutPanelChannels.Controls.Count.ToString() + ")";
        }

        private void SendMessage_Click(object sender, EventArgs e)
        {
            if (!_clientInstance._isConnected)
            {
                MessageBox.Show("Could not connect to the server. Please try again later.");
                return;
            }
            
            string msg = txtMessage.Text;
            if (msg.Length < IPLChat.Protocol.Message.MAX_MESSAGE_SIZE)
            {
                if (msg != "")
                {
                    if (_clientInstance._activeChannelUUID == null)
                    {
                        MessageBox.Show("Please select a channel first.");
                        return;
                    }

                    _clientInstance.SendMessage(msg);
                    AppendMessageCard(msg, _clientInstance._username);
                    txtMessage.Text = "";
                    txtMessage.Focus();
                }
            }
            else
            {
                MessageBox.Show("Message is too long! The maximum size for the messages is " + IPLChat.Protocol.Message.MAX_MESSAGE_SIZE + " characters.");
            }
        }

        private void AppendMessageCard(string message, string username = null)
        {
            MessageCard messageCard = new MessageCard();
            messageCard.Message = message;

            messageCard.Date = DateTime.Now.ToString("hh:mm:ss tt");

            if (InvokeRequired) // To avoid cross-threading exception when updating the UI
            {
                Invoke(new MethodInvoker(delegate
                {
                    flowLayoutPanelMessages.Controls.Add(messageCard);
                }));
            }
            else
            {
                flowLayoutPanelMessages.Controls.Add(messageCard);
            }
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
                txtSearchContacts.Focus();
            }

        }

        private void btnEditProfile_Click(object sender, EventArgs e)
        {

            UserProfile userProfile = new UserProfile();

            userProfile.ShowDialog();
            
        }

        private void btnAddContact_Click(object sender, EventArgs e)
        {
            AddContact addContact = new AddContact();

            addContact.ShowDialog();
        }

        private void txtMessage_TextChanged(object sender, EventArgs e)
        {
            labelRemainingCharacters.Text = "Remaining: " + (IPLChat.Protocol.Message.MAX_MESSAGE_SIZE - txtMessage.Text.Length).ToString();
        }

        private void txtMessage_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SendMessage_Click(sender, e);
            }
        }

        private void Form3_FormClosing(object sender, FormClosingEventArgs e)
        {
            _clientInstance.Disconnect();
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)
        {
            // Open the file explorer to select a file
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "All files (*.*)|*.*";
            openFileDialog.RestoreDirectory = true;

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string filePath = openFileDialog.FileName;
                string fileName = openFileDialog.SafeFileName;

                // Send the file to the server
                _clientInstance.SendFile(filePath, fileName);
            }
        }

        private void txtSearchContacts_TextChanged(object sender, EventArgs e)
        {
            // Update the list of users
            List<UserCard> userCards = GetListedUserCards();

            foreach (UserCard userCard in userCards)
            {
                if (userCard.Title.ToLower().Contains(txtSearchContacts.Text.ToLower()))
                {
                    userCard.Show();
                }
                else
                {
                    userCard.Hide();
                }
            }
        }

        private void btnLogout_Click(object sender, EventArgs e)
        {
            _clientInstance.Logout();
            this.Hide();
            Form1 form1 = new Form1();
            form1.ShowDialog();
            this.Close();
        }

        private void buttonAtualizar_Click(object sender, EventArgs e)
        {
            populateUsers();
        }
    }
}
