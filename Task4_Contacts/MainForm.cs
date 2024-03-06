using System;
using System.Data;
using System.Windows.Forms;
using Npgsql;

namespace Contacts
{
    public partial class MainForm : Form
    {
        private readonly string connectionString = "Host=localhost;Username=postgres;Password=8702;Database=Contacts";
        private NpgsqlConnection connection;
        private NpgsqlDataAdapter adapter;
        private DataTable dataTable;

        public MainForm()
        {
            InitializeComponent();
            connection = new NpgsqlConnection(connectionString);
            adapter = new NpgsqlDataAdapter("SELECT * FROM contacts", connection);
            adapter.InsertCommand = new NpgsqlCommand("INSERT INTO contacts (name, phone) VALUES (@name, @phone)", connection);
            adapter.InsertCommand.Parameters.Add("@name", NpgsqlTypes.NpgsqlDbType.Text, 255, "name");
            adapter.InsertCommand.Parameters.Add("@phone", NpgsqlTypes.NpgsqlDbType.Text, 255, "phone");
            adapter.UpdateCommand = new NpgsqlCommand("UPDATE contacts SET name = @name, phone = @phone WHERE id = @id", connection);
            adapter.UpdateCommand.Parameters.Add("@name", NpgsqlTypes.NpgsqlDbType.Text, 255, "name");
            adapter.UpdateCommand.Parameters.Add("@phone", NpgsqlTypes.NpgsqlDbType.Text, 255, "phone");
            adapter.UpdateCommand.Parameters.Add("@id", NpgsqlTypes.NpgsqlDbType.Integer, 4, "id");
            adapter.DeleteCommand = new NpgsqlCommand("DELETE FROM contacts WHERE id = @id", connection);
            adapter.DeleteCommand.Parameters.Add("@id", NpgsqlTypes.NpgsqlDbType.Integer, 4, "id");
            dataTable = new DataTable();
            adapter.Fill(dataTable);
            dataGridView1.DataSource = dataTable;
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            DataRow newRow = dataTable.NewRow();
            newRow["name"] = txtName.Text;
            newRow["phone"] = txtPhone.Text;
            dataTable.Rows.Add(newRow);
            adapter.Update(dataTable);
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                foreach (DataGridViewRow row in dataGridView1.SelectedRows)
                {
                    int id = (int)row.Cells["id"].Value;
                    string sql = "DELETE FROM contacts WHERE id = @id";
                    using (NpgsqlConnection con = new NpgsqlConnection(connectionString))
                    {
                        con.Open();
                        using (NpgsqlCommand cmd = new NpgsqlCommand(sql, con))
                        {
                            cmd.Parameters.AddWithValue("@id", id);
                            cmd.ExecuteNonQuery();
                        }
                    }
                    Console.WriteLine($"Удалён контакт с id {id}");
                    dataTable.Rows.RemoveAt(row.Index);
                }
                adapter.Update(dataTable);
            }
            else
            {
                Console.WriteLine("Строки не выбраны");
            }
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedCells.Count > 0)
            {
                int rowIndex = dataGridView1.SelectedCells[0].RowIndex;
                dataTable.Rows[rowIndex]["name"] = txtName.Text;
                dataTable.Rows[rowIndex]["phone"] = txtPhone.Text;
                adapter.Update(dataTable);
            }
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            string searchName = txtSearch.Text;
            DataRow[] foundRows = dataTable.Select($"name = '{searchName}'");
            if (foundRows.Length > 0)
            {
                dataGridView1.DataSource = foundRows.CopyToDataTable();
            }
            else
            {
                MessageBox.Show("Контакт не найден.");
            }
        }
    }
}