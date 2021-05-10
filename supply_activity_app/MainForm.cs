using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace supply_activity_app
{
    public partial class MainForm : Form
    {
        private List<Contract> contracts = new List<Contract>();
        private List<Provider> providers = new List<Provider>();
        private Rectangle dragBoxFromMouseDown;
        private int rowIndexFromMouseDown;
        private string connectionString = "Data Source=database.db";
        LoginForm loginForm;
        public MainForm(LoginForm loginForm)
        {
            InitializeComponent();
            this.loginForm = loginForm;
        }

        public void DisplayContracts()
        {
            dgvContracts.Rows.Clear();

            foreach (Contract contract in contracts)
            {
                int rowIndex = dgvContracts.Rows.Add(new object[]
                {
                    contract.Provider.Name,
                    contract.SignDate,
                    contract.Validity,
                    contract.Value
                });

                DataGridViewRow row = dgvContracts.Rows[rowIndex];
                row.Tag = contract;
                if(contract.signDate.AddYears((int)contract.Validity) < DateTime.Today)
                {
                    row.DefaultCellStyle.BackColor = Color.Red;
                    row.DefaultCellStyle.ForeColor = Color.Black;
                }
            }
        }

        private void LoadContracts()
        {
            string query = "SELECT * FROM Contracts";

            using (SqliteConnection connection = new SqliteConnection(connectionString))
            {
                SqliteCommand command = new SqliteCommand(query, connection);

                connection.Open();
                using (SqliteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        long id = (long)reader["Id"];
                        string providerName = (string)reader["Provider"];
                        Provider provider = new Provider();
                        bool test = false;
                        foreach (Provider p in providers)
                        {
                            if (providerName == p.Name)
                            {
                                provider = p;
                                test = true;
                            }
                        }
                        DateTime signDate = DateTime.Parse((string)reader["SignDate"]);
                        long validity = (long)reader["Validity"];
                        long value = (long)reader["Value"];

                        if (test)
                        {
                            Contract contract = new Contract(id, provider, signDate, validity, value);
                            contracts.Add(contract);
                        }
                    }
                }
            }
        }

        private void DeleteContract()
        {
            if (dgvContracts.SelectedRows.Count == 0)
            {
                MessageBox.Show("Select the contract whose information you want to delete!");
                return;
            }

            if (MessageBox.Show("Are you sure?", "Delete contract", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                DataGridViewRow row = dgvContracts.SelectedRows[0];
                Contract contract = (Contract)row.Tag;

                string query = "DELETE FROM Contracts WHERE Id=@id";
                using (SqliteConnection connection = new SqliteConnection(connectionString))
                {
                    SqliteCommand command = new SqliteCommand(query, connection);
                    command.Parameters.AddWithValue("@id", contract.Id);

                    connection.Open();
                    command.ExecuteNonQuery();

                    contracts.Remove(contract);
                }
                DisplayContracts();
            }
        }

        private void btnProviders_Click(object sender, EventArgs e)
        {
            ProvidersForm providersForm = new ProvidersForm(this);
            this.Hide();
            if (providersForm.ShowDialog() == DialogResult.Cancel)
            {
                Application.Exit();
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            bool fileExist = File.Exists("providers.bin");
            if (!fileExist)
            {
                MessageBox.Show("There are currently no providers in the list of providers. You must add at least one provider!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            else
            {
                using (FileStream stream = File.OpenRead("providers.bin"))
                {
                    try
                    {
                        providers = (List<Provider>)formatter.Deserialize(stream);
                        AddContractForm addForm = new AddContractForm(contracts, providers);
                        if (addForm.ShowDialog() == DialogResult.OK)
                        {
                            DisplayContracts();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("There are currently no providers in the list of providers. You must add at least one provider!", "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void dgvContracts_MouseDown(object sender, MouseEventArgs e)
        {
            rowIndexFromMouseDown = dgvContracts.HitTest(e.X, e.Y).RowIndex;
            if (rowIndexFromMouseDown != -1)
            {
                Size dragSize = SystemInformation.DragSize;

                dragBoxFromMouseDown = new Rectangle(new Point(e.X - (dragSize.Width / 2),
                                                               e.Y - (dragSize.Height / 2)),
                                    dragSize);
            }
            else
                dragBoxFromMouseDown = Rectangle.Empty;
        }

        private void dgvContracts_MouseMove(object sender, MouseEventArgs e)
        {
            if ((e.Button & MouseButtons.Left) == MouseButtons.Left)
            {
                if (dragBoxFromMouseDown != Rectangle.Empty &&
                    !dragBoxFromMouseDown.Contains(e.X, e.Y))
                {
                    dgvContracts.DoDragDrop(dgvContracts.Rows[rowIndexFromMouseDown], DragDropEffects.Move);
                }
            }
        }

        private void panel1_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Move;
        }

        private void panel1_DragDrop(object sender, DragEventArgs e)
        {
            if (MessageBox.Show("Are you sure?", "Delete contract", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                DataGridViewRow row = e.Data.GetData(typeof(DataGridViewRow)) as DataGridViewRow;
                Contract contract = (Contract)row.Tag;

                string query = "DELETE FROM Contracts WHERE Id=@id";
                using (SqliteConnection connection = new SqliteConnection(connectionString))
                {
                    SqliteCommand command = new SqliteCommand(query, connection);
                    command.Parameters.AddWithValue("@id", contract.Id);

                    connection.Open();
                    command.ExecuteNonQuery();

                    contracts.Remove(contract);
                }

                DisplayContracts();
            }
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.dgvContracts.SelectedRows.Count > 0)
            {
                StringBuilder ClipboardBuillder = new StringBuilder();
                foreach (DataGridViewRow Row in dgvContracts.SelectedRows)
                {
                    foreach (DataGridViewColumn Column in dgvContracts.Columns)
                    {
                        ClipboardBuillder.Append(Row.Cells[Column.Index].FormattedValue.ToString() + ", ");
                    }
                    ClipboardBuillder.Length -= 2;
                    ClipboardBuillder.AppendLine();
                }

                Clipboard.SetText(ClipboardBuillder.ToString());
            }
            else
            {
                MessageBox.Show("This function is only valid if you select the whole row!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            menuStrip1.BackColor = Color.FromArgb(44, 43, 68);
            menuStrip1.ForeColor = Color.White;
            MessageBox.Show("You are logged in", "Login successful");
            BinaryFormatter formatter = new BinaryFormatter();
            bool fileExist = File.Exists("providers.bin");
            if (!fileExist)
            { 
                return;
            }
            else
            {
                using (FileStream stream = File.OpenRead("providers.bin"))
                {
                    try
                    {
                        providers = (List<Provider>)formatter.Deserialize(stream);
                    }
                    catch (Exception ex)
                    {
                    }
                }
            }

            LoadContracts();
            DisplayContracts();
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            bool fileExist = File.Exists("providers.bin");
            if (!fileExist)
            {
                MessageBox.Show("Impossible to edit! The list of currently registered providers is empty!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            else
            {
                using (FileStream stream = File.OpenRead("providers.bin"))
                {
                    try
                    {
                        if (dgvContracts.SelectedRows.Count == 0)
                        {
                            MessageBox.Show("Select the contract whose information you want to edit!");
                            return;
                        }

                        providers = (List<Provider>)formatter.Deserialize(stream);
                        DataGridViewRow row = dgvContracts.SelectedRows[0];
                        Contract contract = (Contract)row.Tag;
                        EditContractForm editForm = new EditContractForm(contract, providers, contracts);
                        if (editForm.ShowDialog() == DialogResult.OK)
                        {
                            DisplayContracts();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Impossible to edit! The list of currently registered providers is empty!", "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void exportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            exportToolStripMenuItem.ForeColor = Color.Black;
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "CSV File | *.csv";
            saveFileDialog.Title = "Export CSV";

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                using (StreamWriter writer = File.CreateText(saveFileDialog.FileName))
                {
                    writer.WriteLine("\"Id \", \"Provider name\", \"Signing date\", \"Validity\", \"Value\"");


                    foreach (Contract contract in contracts)
                    {
                        writer.WriteLine($"\"{contract.Id}\",\"{contract.Provider.Name}\",\"{contract.SignDate}\",\"{contract.Validity}\",\"{contract.Value}\"");
                    }
                }
            }
        }

        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Control && e.KeyCode.ToString() == "E")
            {
                btnEdit_Click(sender, e);
            }

            if (e.Control && e.KeyCode.ToString() == "A")
            {
                btnAdd_Click(sender, e);
            }

            if (e.Control && e.KeyCode.ToString() == "P")
            {
                btnProviders_Click(sender, e);
            }

            if (e.Control && e.KeyCode.ToString() == "L")
            {
                btnLogout_Click(sender, e);
                this.DialogResult = DialogResult.OK;
            }

            if (e.KeyCode == Keys.Escape)
            {
                Application.Exit();
            }

            if (e.Control && e.KeyCode.ToString() == "D")
            {
                DeleteContract();
            }
        }

        private void stergeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DeleteContract();
        }

        private void fileToolStripMenuItem1_MouseHover(object sender, EventArgs e)
        {
            fileToolStripMenuItem1.ForeColor = Color.Black;
        }

        private void fileToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            fileToolStripMenuItem1.ForeColor = Color.Black;
        }

        private void fileToolStripMenuItem1_MouseLeave(object sender, EventArgs e)
        {
            fileToolStripMenuItem1.ForeColor = Color.White;
        }

        private void exportToolStripMenuItem_MouseHover(object sender, EventArgs e)
        {
            exportToolStripMenuItem.ForeColor = Color.Black;
        }

        private void exportToolStripMenuItem_MouseLeave(object sender, EventArgs e)
        {
            exportToolStripMenuItem.ForeColor = Color.White;
        }

        private void suppToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SupportForm supportForm = new SupportForm();
            supportForm.ShowDialog();
        }

        private void btnLogout_Click(object sender, EventArgs e)
        {
            this.Close();
            loginForm.Show();
        }
    }
}
