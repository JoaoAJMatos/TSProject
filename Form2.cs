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
    public partial class Form2 : Form
    {
        Client _clientInstance = Client.GetInstance();

        public Form2()
        {
            InitializeComponent();
        }

        private void Form2_Load(object sender, EventArgs e)
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

        private void btnRegister_Click(object sender, EventArgs e)
        {
            if (!_clientInstance._isConnected)
            {
                MessageBox.Show("Could not connect to the server. Please try again later.");
                return;
            }

            string username = txtUsername.Text;
            string password = txtPassword.Text;
            string confirmpassword = txtConfirmPassword.Text;

            if (username == "" || password == "" || confirmpassword == "")
            {
                MessageBox.Show("Please fill in all the fields.", "Registration Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else if(password != confirmpassword)
            {
                MessageBox.Show("Passwords do not match.", "Registration Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);

                txtPassword.Text = "";
                txtConfirmPassword.Text = "";
                txtPassword.Focus();
            }
            else
            {
                if (_clientInstance.Register(username, password))
                {
                    this.Hide();
                    Form1 f1 = new Form1();
                    f1.Closed += (s, args) => this.Close();
                    f1.Show();
                }
                else
                {
                    MessageBox.Show("An error has occured while trying to register. Try again later.", "Registration Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            this.Hide();
            Form1 f1 = new Form1();
            f1.Closed += (s, args) => this.Close();
            f1.Show();
        }

        private void checkBoxShowPassword_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxShowPassword.Checked)
            {
                txtPassword.PasswordChar = '\0';
                txtConfirmPassword.PasswordChar = '\0';
            }
            else
            {
                txtPassword.PasswordChar = '*';
                txtConfirmPassword.PasswordChar = '*';
            }
        }

        private void Form2_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (_clientInstance._isConnected) _clientInstance.Disconnect();
        }
    }
}
