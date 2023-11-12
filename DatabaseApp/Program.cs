using System;
using System.Data.SQLite;
using System.IO;


namespace DatabaseApp
{
    class TreeNode
    {
        public string contactID { get; set; }
        public Contact contact;
        public List<TreeNode> children;
        public abstract class AddChildCallback
        {
            public abstract void AddChild(TreeNode node);
        }
        public TreeNode(string contactID, Contact contact)
        {
            this.contactID = contactID;
            this.contact = contact;
            contact.LoadByID(contactID);
            children = new List<TreeNode>();
        }
        internal void AddChild(TreeNode node, Action<TreeNode> callback)
        {
            children.Add(node);
            callback.Invoke(node);
        }
    }

    class ContactsHierachy
    {
        private Database database;
        public TreeNode root;

        public ContactsHierachy(Database database, string rootID)
        {
            this.database = database;
            WorkContact work = new WorkContact();
            work.LoadByID(rootID);
            
            root = new TreeNode(rootID,  work);
            using(Database db = Database.GetInstance("KONTAKTI.db"))
            {
                string q = @"
                    CREATE TABLE IF NOT EXISTS Hierarchy(
                        ParentID integer,
                        ChildID integer
                    );
                ";
                if(Database.connection != null)
                    using(SQLiteCommand cmd =Database.connection.CreateCommand())
                    {
                        cmd.CommandText = q;
                        cmd.ExecuteNonQuery();
                    }
            }
        }

        public void AddChildTo(string parentID, string childID, TreeNode currentNode)
        {
            if (currentNode.contactID == parentID)
            {
                WorkContact work = new WorkContact();
                work.LoadByID(childID);
                currentNode.AddChild(
                    new TreeNode(childID,  work),
                    child => {
                        string q = @"INSERT INTO Hierarchy(ParentID, ChildID) 
                                     VALUES(@ParentID, @ChildID)                                        
                                    ";
                        using(Database db = Database.GetInstance("KONTAKTI.db"))
                        {
                            using(SQLiteCommand cmd = new SQLiteCommand(Database.connection))
                            {
                                cmd.CommandText = q;
                                cmd.Parameters.AddWithValue("ParentID", parentID);
                                cmd.Parameters.AddWithValue("ChildID", childID);
                                cmd.ExecuteNonQuery();
                            }
                        }
                    }
                    ); 
            }
            else
            {
                foreach(TreeNode node in currentNode.children)
                {
                    AddChildTo(parentID, childID, node);
                }
            }
        }
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            using (Database db = Database.GetInstance("KONTAKTI.db"))
            {
                db.StoreRecord("Rainy", "addressss", "test@email.com", "0864648", "University");
                db.StoreRecord("Sunny", "addressss2", "test2@email.com", "787980", "Work");

                List<Contact> contacts = db.RetrieveData
                    ("University");
                foreach (var item in contacts)
                {
                    Console.WriteLine($"Contact Name: {item.Name}, " +
                        $"Contact Email: {item.Email} " +
                        $"Contact Phone: {item.Phone}");
                }

                ContactsHierachy hierachy = new ContactsHierachy(db, "1");
                WorkContact contact = new WorkContact();
                hierachy.AddChildTo("1", "2", new TreeNode("1", contact));

            }
        }
    }
}