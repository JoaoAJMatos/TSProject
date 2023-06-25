using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Projeto_Tópicos_de_Segurança
{
    public partial class Form1 : Form
    {
        Client _clientInstance = Client.GetInstance();

        public Form1()
        {
            InitializeComponent();
        }

       
        private void button1_Click(object sender, EventArgs e)
        {
            this.Hide();
            Form3 f3 = new Form3();
            f3.Closed += (s, args) => this.Close();
            f3.Show();
        }

        // Connect to the server and perform the handshake.
        // If the connection is not succesful, retry every 5 seconds
        private void Form1_Load(object sender, EventArgs e)
        {
            if (!_clientInstance._isConnected) _clientInstance.ConnectToServer("127.0.0.1", 4589);

            if (this.IsHandleCreated)
            {
                if (_clientInstance._isConnected)
                {
                    labelConnectionStatus.ForeColor = Color.Green;
                    labelConnectionStatus.Text = "Connected";
                }
                else
                {
                    labelConnectionStatus.ForeColor = Color.Red;
                    labelConnectionStatus.Text = "Not Connected";
                }
            }
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            this.Hide();
            Form2 f2 = new Form2();
            f2.Closed += (s, args) => this.Close();
            f2.Show();
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            if (!_clientInstance._isConnected)
            {
                MessageBox.Show("Could not connect to the server. Please try again later.");
                return;
            }

            string username = txtUsername.Text;
            string password = txtPassword.Text;

            if (username == "" || password == "")
            {
                MessageBox.Show("Please fill in all the fields.");
            }
            else
            {
                if(!_clientInstance.Login(username, password))
                {
                    MessageBox.Show("Invalid username or password.");
                    txtPassword.Text = "";
                }
                else
                {
                    this.Hide();
                    Form3 f3 = new Form3();
                    f3.Show();
                }
            }
        }

        private void checkBoxShowPassword_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxShowPassword.Checked)
            {
                txtPassword.PasswordChar = '\0';
            }
            else
            {
                txtPassword.PasswordChar = '*';
            }
        }

        // Disconnect from the server when the form is closed.
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_clientInstance._isConnected) _clientInstance.Disconnect();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {

        }
    }
}
