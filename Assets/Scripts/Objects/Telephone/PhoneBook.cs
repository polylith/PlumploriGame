using System.Collections.Generic;

public class PhoneBook : AbstractData
{
    public List<PhoneBookEntry> Entries { get; } = new List<PhoneBookEntry>();

    public void Remove(PhoneBookEntry entry)
    {
        if (null == entry)
            return;

        PhoneBookEntry entry2 = Entries.Find(e => e.Number.Equals(entry.Number));

        if (null == entry2)
            return;

        Entries.Remove(entry2);
    }

    public void Add(PhoneBookEntry entry)
    {
        if (null == entry)
            return;

        PhoneBookEntry entry2 = Entries.Find(e => e.Number.Equals(entry.Number));

        if (null == entry2)
        {
            Entries.Add(entry);
            return;
        }

        entry2.Name = entry.Name;
    }

    public void Load()
    {
        
    }

    public void Save()
    {
        
    }
}
