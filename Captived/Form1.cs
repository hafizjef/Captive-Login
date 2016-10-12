using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Http;
using System.Configuration;

namespace Captived
{
    public partial class Form1 : Form
    {
        private string url = "http://172.16.202.1:8090/httpclient.html";
        private string KPUrl = "http://172.16.202.1:8090/live";

        public Form1()
        {
            InitializeComponent();
        }


        private void unmaskPass_CheckedChanged(object sender, EventArgs e)
        {
            passBox.PasswordChar = unmaskPass.Checked ? '\0' : '*';
        }

        private async void doLogin(string username, string password)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var values = new Dictionary<string, string>
                    {
                        { "mode", "191" },
                        { "username", username },
                        { "password", password }
                     };

                    var content = new FormUrlEncodedContent(values);
                    var response = await client.PostAsync(url, content);
                    var responseString = await response.Content.ReadAsStringAsync();

                    if (responseString.Contains("You have successfully logged in"))
                    {
                        // Login Success
                        notifyIcon.BalloonTipText = "Login successfull";
                        notifyIcon.ShowBalloonTip(500);
                    }
                    else
                    {
                        showUI();
                        MessageBox.Show("Usename/Password combination error", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    while (true)
                    {
                        await Task.Delay(180000);
                        System.Net.WebClient webClient = new System.Net.WebClient();
                        webClient.QueryString.Add("mode", "192");
                        webClient.QueryString.Add("username", username);
                        string result = webClient.DownloadString(KPUrl);
                    }
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "Error");
            }
        }

        private void checkState()
        {
            if (usernameBox.Text == "" || passBox.Text == "")
            {
                btnLogin.Enabled = false;
            }
            else
            {
                btnLogin.Enabled = true;
            }
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            doLogin(usernameBox.Text, passBox.Text);

            Properties.Settings.Default.username = usernameBox.Text;
            Properties.Settings.Default.password = passBox.Text;

            Properties.Settings.Default.Save();

            WindowState = FormWindowState.Minimized;

        }

        private void usernameBox_TextChanged(object sender, EventArgs e)
        {
            checkState();
        }

        private void passBox_TextChanged(object sender, EventArgs e)
        {
            checkState();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

            usernameBox.Text = Properties.Settings.Default.username;
            passBox.Text = Properties.Settings.Default.password;
        }

        private void notifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            showUI();
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            minimiseToBackground();
        }

        private void minimiseToBackground()
        {
            if (WindowState == FormWindowState.Minimized)
            {
                ShowInTaskbar = false;
                notifyIcon.BalloonTipText = "WiFi login are running in background to keep your captive session alive";
                notifyIcon.Visible = true;
                notifyIcon.ShowBalloonTip(500);
                Hide();
            }

            else if (WindowState == FormWindowState.Normal)
            {
                notifyIcon.Visible = false;
            }
        }

        private void showUI()
        {
            Show();
            Visible = true;
            ShowInTaskbar = true;
            WindowState = FormWindowState.Normal;
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void showInfoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Programmed By : HafizJef\n", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
