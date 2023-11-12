using System.Data.SQLite;


namespace DatabaseApp
{
    sealed class Database : IDisposable {
        private static Database? instance;
        public static SQLiteConnection? connection;
        private bool disposedValue;

        private Database(string fileName)
        {
            if (!File.Exists(fileName))
            {
                SQLiteConnection.CreateFile(fileName);
            }
            string connectionString =
                $"Data Source={fileName};Version=3";
            connection = new SQLiteConnection(connectionString);
            connection.Open();
            Initialize();

        }

        public static Database GetInstance(string fileName)
        {
            if(instance == null)
            {
                instance = new Database(fileName);
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
                    ContactType TEXT NOT NULL
                );
            ";
            using(SQLiteCommand command = new SQLiteCommand(
                sql, connection
                )) {
                command.ExecuteNonQuery();            
            }
        }

        public void StoreRecord(string name, string address, 
            string email, string phone, string contacttype)
        {
            string sql = @"
                INSERT INTO contacts(Name, Address, Phone, Email, ContactType)
                VALUES(@name, @address, @phone, @email, @contacttype)
            ";
            using (SQLiteCommand command = new SQLiteCommand(
                sql, connection
                ))
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
                    if(connection != null)
                    {
                        connection.Close();
                        connection = null;
                    }
                    instance = null;
                }
            }
        }

        public List<Contact> RetrieveData(string type) { 
                List<Contact> contacts = new List<Contact>();
            string sql = $"select * from contacts " +
                "WHERE ContactType like '%"+type+"%'; ";

            using (SQLiteCommand command = new SQLiteCommand
                (@sql, connection))
            {
                using(SQLiteDataReader reader = command.ExecuteReader())
                {

                    while (reader.Read())
                    {
                        Contact contact = ContactFactory.CreateContact(type);
                        contact.Name = reader["Name"].ToString();
                        contact.Address = reader["Address"].ToString();
                        contact.Email = reader["Email"].ToString();
                        contact.Phone = reader["Phone"].ToString();
                        contact.ID = reader["ID"].ToString();
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
}