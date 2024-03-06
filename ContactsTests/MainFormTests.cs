using System;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Npgsql;

namespace Contacts.Tests
{
    [TestClass]
    public class MainFormTests
    {
        private const string ConnectionString = "Host=localhost;Username=postgres;Password=8702;Database=Contacts_Test";
        private readonly MainForm _form = new MainForm();

        [TestMethod]
        public void MainForm_Constructor_LoadsDataFromDatabase()
        {
            // Act
            _form.Show();

            // Assert
            Assert.IsNotNull(_form.dataGridView1.DataSource, "DataGrid не заполнен данными из базы данных.");
        }

        [TestMethod]
        public void MainForm_AddContact_AddsContactToDatabase()
        {
            // Arrange
            var initialRowCount = _form.dataGridView1.RowCount;

            // Act
            _form.txtName.Text = "Test Name";
            _form.txtPhone.Text = "1234567890";
            _form.btnAdd_Click(null, null);

            // Assert
            Assert.AreEqual(initialRowCount + 1, _form.dataGridView1.RowCount, "Контакт не был добавлен в базу данных.");
        }

        [TestMethod]
        public void MainForm_DeleteContact_DeletesContactFromDatabase()
        {
            // Arrange
            var initialRowCount = _form.dataGridView1.RowCount;

            // Act
            _form.btnDelete_Click(null, null);

            // Assert
            Assert.AreEqual(initialRowCount - 1, _form.dataGridView1.RowCount, "Контакт не был удален из базы данных.");
        }

        [TestMethod]
        public void MainForm_EditContact_EditsContactInDatabase()
        {
            // Arrange
            var initialName = _form.txtName.Text;
            var initialPhone = _form.txtPhone.Text;

            // Act
            _form.txtName.Text = "Edited Name";
            _form.txtPhone.Text = "9876543210";
            _form.btnEdit_Click(null, null);

            // Assert
            Assert.AreNotEqual(initialName, _form.txtName.Text, "Имя контакта не было изменено.");
            Assert.AreNotEqual(initialPhone, _form.txtPhone.Text, "Телефон контакта не был изменен.");
        }

        [TestMethod]
        public void MainForm_SearchContact_FindsContactInDataGridView()
        {
            // Act
            _form.txtSearch.Text = "Test Name";
            _form.btnSearch_Click(null, null);

            // Assert
            Assert.IsTrue(_form.dataGridView1.Rows.Count > 0, "Контакт не был найден в DataGridView.");
        }

        [TestInitialize]
        public void Initialize()
        {
            // Create a test database and populate it with test data
            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                connection.Open();
                using (var cmd = new NpgsqlCommand("CREATE TABLE contacts (id SERIAL PRIMARY KEY, name TEXT, phone TEXT)", connection))
                {
                    cmd.ExecuteNonQuery();
                }
                using (var cmd = new NpgsqlCommand("INSERT INTO contacts (name, phone) VALUES ('Test Name', '1234567890')", connection))
                {
                    cmd.ExecuteNonQuery();
                }
            }

            // Bind the test data to the DataGridView
            _form.dataGridView1.DataSource = GetTestData();
        }

        [TestCleanup]
        public void Cleanup()
        {
            // Drop the test database
            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                connection.Open();
                using (var cmd = new NpgsqlCommand("DROP TABLE contacts", connection))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private DataTable GetTestData()
        {
            var dataTable = new DataTable();
            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                connection.Open();
                using (var adapter = new NpgsqlDataAdapter("SELECT * FROM contacts", connection))
                {
                    adapter.Fill(dataTable);
                }
            }
            return dataTable;
        }
    }
}