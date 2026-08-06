public static class ContactDirectory
{
    // OrdinalIgnoreCase compares without regard to letter casing, so a
    // lowercase query like "charlie davis" matches stored data "Charlie Davis".
    public static ContactCard? FindByName(ContactCard[] contacts, string name)
    {
        foreach (ContactCard contact in contacts)
        {
            if (string.Equals(contact.Name, name, StringComparison.OrdinalIgnoreCase))
            {
                return contact;
            }
        }
        return null;
    }
}
