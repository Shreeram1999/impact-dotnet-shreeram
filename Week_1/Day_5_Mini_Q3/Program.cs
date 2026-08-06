ContactCard[] contacts = new ContactCard[5]
{
    new ContactCard("Alice Johnson", "555-0101", "alice@example.com"),
    new ContactCard("Bob Smith", "555-0102", "bob@example.com"),
    new ContactCard("Charlie Davis", "555-0103", "charlie@example.com"),
    new ContactCard("Diana Prince", "555-0104", "diana@example.com"),
    new ContactCard("Ethan Hunt", "555-0105", "ethan@example.com"),
};

// Query is deliberately a different case than the stored data, to prove the
// search is genuinely case-insensitive rather than a lucky exact match.
string query = "charlie davis";
ContactCard? match = ContactDirectory.FindByName(contacts, query);

if (match is { } found) // pattern match against the nullable struct
{
    Console.WriteLine($"Found: {found.Name}, {found.Phone}, {found.Email}");
}
else
{
    Console.WriteLine($"No contact named '{query}' found.");
}
