using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Zdn3
{
    public partial class MainWindow : Window
    {
        private SQLiteConnection dbConnection;

        public MainWindow()
        {
            InitializeComponent();

            dbConnection = new SQLiteConnection("Data Source=contacts.db;Version=3;");
            dbConnection.Open();

            using (SQLiteCommand cmd = new SQLiteCommand(
                "CREATE TABLE IF NOT EXISTS Contacts (Id INTEGER PRIMARY KEY AUTOINCREMENT, FirstName TEXT, LastName TEXT, Email TEXT, PhoneNumber TEXT);",
                dbConnection))
            {
                cmd.ExecuteNonQuery();
            }

            RefreshContactList();
        }

        private void AddContactButton_Click(object sender, RoutedEventArgs e)
        {
            // Dodawanie nowego kontaktu do bazy danych
            string firstName = FirstNameTextBox.Text;
            string lastName = LastNameTextBox.Text;
            string email = EmailTextBox.Text;
            string phoneNumber = PhoneNumberTextBox.Text;

            if (!string.IsNullOrEmpty(firstName) && !string.IsNullOrEmpty(lastName))
            {
                using (SQLiteCommand cmd = new SQLiteCommand(
                    "INSERT INTO Contacts (FirstName, LastName, Email, PhoneNumber) VALUES (@FirstName, @LastName, @Email, @PhoneNumber);",
                    dbConnection))
                {
                    cmd.Parameters.AddWithValue("@FirstName", firstName);
                    cmd.Parameters.AddWithValue("@LastName", lastName);
                    cmd.Parameters.AddWithValue("@Email", email);
                    cmd.Parameters.AddWithValue("@PhoneNumber", phoneNumber);
                    cmd.ExecuteNonQuery();
                }

                RefreshContactList();
                ClearInputFields();
            }
            else
            {
                MessageBox.Show("Proszę wypełnić imię i nazwisko.");
            }
        }

        private void EditContactButton_Click(object sender, RoutedEventArgs e)
        {
            // Edycja istniejącego kontaktu w bazie danych
            if (ContactDataGrid.SelectedItem is DataRowView selectedContact)
            {
                int contactId = Convert.ToInt32(selectedContact["Id"]);
                string firstName = FirstNameTextBox.Text;
                string lastName = LastNameTextBox.Text;
                string email = EmailTextBox.Text;
                string phoneNumber = PhoneNumberTextBox.Text;

                if (!string.IsNullOrEmpty(firstName) && !string.IsNullOrEmpty(lastName))
                {
                    using (SQLiteCommand cmd = new SQLiteCommand(
                        "UPDATE Contacts SET FirstName = @FirstName, LastName = @LastName, Email = @Email, PhoneNumber = @PhoneNumber WHERE Id = @ContactId;",
                        dbConnection))
                    {
                        cmd.Parameters.AddWithValue("@FirstName", firstName);
                        cmd.Parameters.AddWithValue("@LastName", lastName);
                        cmd.Parameters.AddWithValue("@Email", email);
                        cmd.Parameters.AddWithValue("@PhoneNumber", phoneNumber);
                        cmd.Parameters.AddWithValue("@ContactId", contactId);
                        cmd.ExecuteNonQuery();
                    }

                    RefreshContactList();
                    ClearInputFields();
                }
                else
                {
                    MessageBox.Show("Proszę wypełnić imię i nazwisko.");
                }
            }
        }

        private void DeleteContactButton_Click(object sender, RoutedEventArgs e)
        {
            // Usunięcie kontaktu z bazy danych
            if (ContactDataGrid.SelectedItem is DataRowView selectedContact)
            {
                int contactId = Convert.ToInt32(selectedContact["Id"]);

                using (SQLiteCommand cmd = new SQLiteCommand(
                    "DELETE FROM Contacts WHERE Id = @ContactId;",
                    dbConnection))
                {
                    cmd.Parameters.AddWithValue("@ContactId", contactId);
                    cmd.ExecuteNonQuery();
                }

                RefreshContactList();
                ClearInputFields();
            }
        }

        private void ClearInputFields()
        {
            FirstNameTextBox.Clear();
            LastNameTextBox.Clear();
            EmailTextBox.Clear();
            PhoneNumberTextBox.Clear();
        }

        private void RefreshContactList()
        {
            // Odświeżenie listy kontaktów w DataGrid
            using (SQLiteCommand cmd = new SQLiteCommand(
                "SELECT Id, FirstName, LastName, Email, PhoneNumber FROM Contacts;",
                dbConnection))
            {
                DataTable dataTable = new DataTable();
                SQLiteDataAdapter adapter = new SQLiteDataAdapter(cmd);
                adapter.Fill(dataTable);
                ContactDataGrid.ItemsSource = dataTable.DefaultView;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Zamknięcie połączenia z bazą danych przy zamykaniu aplikacji
            if (dbConnection != null)
            {
                dbConnection.Close();
            }
        }
    }
}
