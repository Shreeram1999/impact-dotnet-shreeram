// `Repository<T>` is a GENERIC class - `T` is a placeholder for "some type
// we'll decide later, when we actually use this class". Program.cs creates
// a `Repository<Student>` and a separate `Repository<Product>` from this
// SAME class definition - we only wrote the logic once, but it works for
// both types (and would work for any future type too), fully type-safe
// (a Repository<Student> will never accidentally let you Add a Product).
//
// `where T : class, IEntity, new()` is a "generic constraint" - it limits
// what T is allowed to be:
//   - `class` means T must be a reference type (not a value type like int
//     or a struct), so items can be stored/compared by reference.
//   - `IEntity` means T must implement IEntity, guaranteeing every T has
//     an Id we can search by.
//   - `new()` means T must have a public parameterless constructor, i.e.
//     something has to support `new T()`. Without this constraint, the
//     compiler wouldn't let us write `new T()` inside CreateNew() below,
//     because it can't assume just any T has that kind of constructor.
public class Repository<T> where T : class, IEntity, new()
{
    private readonly List<T> items = new();
    private int nextId = 1;

    // This method only compiles because of the `new()` constraint above -
    // it lets us build a brand-new T from scratch, give it the next Id,
    // and store it, all without knowing the concrete type ahead of time.
    public T CreateNew()
    {
        var item = new T { Id = nextId++ };
        items.Add(item);
        return item;
    }

    public void Add(T item)
    {
        if (item.Id == 0)
            item.Id = nextId++;

        items.Add(item);
    }

    // "Update" here means: find the existing item with the same Id as
    // `updated`, and replace it in the list. This lets Program.cs build a
    // fresh Student object with the SAME Id but a different Grade, and use
    // it to "edit" the stored one.
    public bool Update(T updated)
    {
        var index = items.FindIndex(i => i.Id == updated.Id);
        if (index < 0)
            return false;

        items[index] = updated;
        return true;
    }

    public bool Delete(int id)
    {
        var item = items.FirstOrDefault(i => i.Id == id);
        return item is not null && items.Remove(item);
    }

    // IReadOnlyList<T> lets callers read the items but not modify the
    // list itself (no Add/Remove from outside) - another small bit of
    // encapsulation, similar in spirit to Task 2.1's BankAccount.
    public IReadOnlyList<T> GetAll() => items.AsReadOnly();
}
