using System.Data.SQLite;

namespace DatabaseApp
{
    abstract class Contact {
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        public string? Phone { get; set; }   
        
        public string? ID { get; set; }

        public abstract string GetContactType();

        public void LoadByID(string id)
        {
            using(Database db = Database.GetInstance("KONTAKTI.db"))
            {
                string q = @"SELECT * FROM contacts WHERE ID = @ID";
                if (Database.connection != null)
                {
                    using (SQLiteCommand command = new SQLiteCommand(Database.connection))
                    {
                        command.CommandText = q;
                        command.Parameters.AddWithValue("ID", id);
                        using(SQLiteDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                this.Name = reader["Name"].ToString();
                                this.Email = reader["Email"].ToString();
                                this.Address = reader["Address"].ToString();
                                this.Phone = reader["Phone"].ToString() ;
                            }
                        }
                    }
                }
                
            }
        }
    
    }
}