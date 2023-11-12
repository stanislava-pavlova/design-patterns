using System;
using System.Data.SQLite;
using System.IO;

namespace DatabaseApp
{
    abstract class Contact
    {
        public string? Name { get; set; }
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }

        public abstract string GetContactType();
    }

    class WorkContact : Contact
    {
        public override string GetContactType()
        {
            return "Work";
        }
    }

    class RelativeContact : Contact
    {
        public override string GetContactType()
        {
            return "Relative";
        }
    }

    class UniversityContact : Contact
    {
        public override string GetContactType()
        {
            return "University";
        }
    }

    sealed class ContactFactory
    {
        private ContactFactory() { } // подсигурява че contact factory няма да се инициализира и ще върне ? singleton

        public static Contact CreateContact(string type)
        {
            switch (type)
            {
                case "Work":
                    return new WorkContact();
                case "Relative":
                    return new RelativeContact();
                case "University":
                    return new UniversityContact();
                default:
                    throw new ArgumentException("Invalid type");
            }
        }
    }

    sealed class Database : IDisposable
    {
        private static Database? instance;
        private static SQLiteConnection? connection;
        private bool disposedValue;

        private Database(string fileName)
        {
            if (!File.Exists(fileName))
            {
                SQLiteConnection.CreateFile(fileName);
            }
            string connectionString = $"Data Source={fileName};Version=3";

            connection = new SQLiteConnection(connectionString);
            connection.Open();
            Initialize();
        }

        public static Database GetInstance(string filename)
        {
            if (instance == null)
            {
                instance = new Database(filename);
            }
            return instance;
        }

        public void Initialize()
        {
            string sql = @"
                CREATE TABLE IF NOT EXISTS contacts(
                    ID INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name Text NOT NULL,
                    Address Text NOT NULL,
                    Phone Text NOT NULL,
                    Email Text NOT NULL,
                    ContactType TEXT NOT NuLL
                );
            ";
            using (SQLiteCommand command = new SQLiteCommand(sql, connection))
            {
                command.ExecuteNonQuery();
            }
        }

        public void StoreRecord(string name, string address, string email, string phone, string contacttype)
        {
            string sql = @"
                INSERT INTO contacts(Name, Address, Phone, Email, ContactType)
                VALUES(@name, @address, @phone, @email, @contacttype)
            ";
            using (SQLiteCommand command = new SQLiteCommand(sql, connection))
            {
                command.Parameters.AddWithValue("name", name);
                command.Parameters.AddWithValue("address", address);
                command.Parameters.AddWithValue("phone", phone);
                command.Parameters.AddWithValue("email", email);
                command.Parameters.AddWithValue("contacttype", contacttype);
                command.ExecuteNonQuery();
            }
        }

        public void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (instance != null)
                {
                    if (connection != null)
                    {
                        connection.Close();
                        connection = null;
                    }
                    instance = null;
                }
            }
        }

        public List<Contact> RetrieveData(string type)
        {
            List<Contact> contacts = new List<Contact>();
            string sql = $"select * from contacts " +
                         "WHERE ContactType like '%" + type + "%'; "; 
            // ако type е празен стринг, ще върне всички контакти, независимо от type-a

            using (SQLiteCommand command = new SQLiteCommand(@sql, connection))
            {
                using (SQLiteDataReader reader = command.ExecuteReader()) // reader e disposable object  и е добре да го слагаме в using
                {
                    while (reader.Read())
                    {
                        Contact contact = ContactFactory.CreateContact(type);
                        contact.Name = reader["Name"].ToString();
                        contact.Address = reader["Address"].ToString();
                        contact.Email = reader["Email"].ToString();
                        contact.Phone = reader["Phone"].ToString();
                        contacts.Add(contact);
                    }
                }
            }

            return contacts;

        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~Database()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            using (Database db = Database.GetInstance("KONTAKTI.db"))
            {
                db.StoreRecord("Rainy", "addressss", "0864648", "test@email.com", "University");
                db.StoreRecord("Sunny", "addressss2", "787980", "test2@email.com", "Work");

                List<Contact> conacts = db.RetrieveData("University");
                foreach(var item in conacts)
                {
                    Console.WriteLine($"Contact Name: {item.Name}, " +
                        $"Contact Email: {item.Email} " +
                        $"Contact Phone: {item.Phone}");
                }
            }
        }
    }
}