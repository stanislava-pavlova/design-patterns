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

        public TreeNode(string contactID,  Contact contact)
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
}