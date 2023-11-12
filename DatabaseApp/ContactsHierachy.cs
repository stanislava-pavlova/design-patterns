using System.Data.SQLite;


namespace DatabaseApp
{
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
            if(currentNode.contactID == parentID)
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
                            using(SQLiteCommand cmd = 
                            new SQLiteCommand(Database.connection))
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
}