namespace DatabaseApp
{
    sealed class ContactFactory
    {
        private ContactFactory() { }

        public static Contact CreateContact(string type) { 
        
            switch (type)
            {
                case "Work":
                    return new WorkContact();
                case "Relative":
                    return new RelativeContact();
                case "University":
                    return new UniversityContact();
                default:
                    throw new ArgumentException("invalid type");

            }

        }

    }
}