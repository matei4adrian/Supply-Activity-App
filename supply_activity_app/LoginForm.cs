using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace supply_activity_app
{
    public partial class LoginForm : Form
    {
        private string connectionString = "Data Source=database.db";
        public LoginForm()
        {
            InitializeComponent();
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            if(tbUser.Text.Trim() == "" && tbPass.Text.Trim() == "")
            {
                MessageBox.Show("Empty fields", "Error");
            }
            else
            {
                string query = "SELECT * FROM Users WHERE Username = @username AND Password = @password";
                using (SqliteConnection connection = new SqliteConnection(connectionString))
                {
                    SqliteCommand command = new SqliteCommand(query, connection);
                    command.Parameters.AddWithValue("@username", tbUser.Text);
                    command.Parameters.AddWithValue("@password", tbPass.Text);
                    connection.Open();
                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        string username = "";
                        string password = "";
                        while (reader.Read())
                        {
                            username = (string)reader["Username"];
                            password = (string)reader["Password"];
                        }

                        if (username == "" && password == "")
                        {
                            MessageBox.Show("Login failed", "Error");
                        }
                        else
                        {
                            MainForm mainForm = new MainForm(this);
                            this.Hide();
                            if (mainForm.ShowDialog() == DialogResult.Cancel)
                            {
                                Application.Exit();
                            }
                            tbUser.Text = "";
                            tbPass.Text = "";
                        }
                    }
                }
            }
        }

        private void tbUser_MouseHover(object sender, EventArgs e)
        {
            status.Text = "Enter the username given by the administrator";
        }

        private void tbUser_MouseLeave(object sender, EventArgs e)
        {
            status.Text = "";
        }

        private void tbPass_MouseHover(object sender, EventArgs e)
        {
            status.Text = "Enter the password given by the administrator";
        }

        private void tbPass_MouseLeave(object sender, EventArgs e)
        {
            status.Text = "";
        }

        private void btnLogin_MouseHover(object sender, EventArgs e)
        {
            status.Text = "Press to login";
        }

        private void btnLogin_MouseLeave(object sender, EventArgs e)
        {
            status.Text = "";
        }

        private void LoginForm_Load(object sender, EventArgs e)
        {
            statusStrip1.BackColor = Color.Teal;
            statusStrip1.ForeColor = Color.White;
            toolStrip1.BackColor = Color.FromArgb(44, 43, 68);
        }

        private void LoginForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                this.Close();
            }

            if (e.KeyCode == Keys.Enter)
            {
                btnLogin_Click(sender, e);
            }

            if (e.Control && e.KeyCode.ToString() == "I")
            {
                toolStripButton2_Click(sender, e);
            }
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            SupportForm supportForm = new SupportForm();
            supportForm.ShowDialog();
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            InfoForm infoForm = new InfoForm();
            infoForm.ShowDialog();
        }
    }
}
