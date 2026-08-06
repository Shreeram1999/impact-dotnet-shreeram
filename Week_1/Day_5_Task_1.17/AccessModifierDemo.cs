// Access-modifier matrix:
//   public              -> accessible from anywhere: same class, derived
//                           classes, same assembly, other assemblies.
//   private             -> accessible only inside the SAME class (not even
//                           derived classes can see it).
//   protected           -> same class + any class that DERIVES from it,
//                           regardless of assembly.
//   internal            -> same class + anywhere else in the SAME assembly
//                           only (not derived classes in other assemblies).
//   protected internal  -> UNION of protected and internal: same assembly
//                           OR any derived class in any assembly.
//   private protected   -> INTERSECTION of protected and internal: only
//                           derived classes that are ALSO in the same
//                           assembly.
public class AccessModifierDemo
{
    public int PublicField = 1;
    private int privateField = 2;
    protected int ProtectedField = 3;
    internal int InternalField = 4;
    protected internal int ProtectedInternalField = 5;
    private protected int PrivateProtectedField = 6;

    public void ShowFromInside()
    {
        // Every field is reachable here — "inside the declaring class" is
        // the one context where all six modifiers grant access.
        Console.WriteLine($"Inside class: public={PublicField}, private={privateField}, " +
            $"protected={ProtectedField}, internal={InternalField}, " +
            $"protectedInternal={ProtectedInternalField}, privateProtected={PrivateProtectedField}");
    }
}

public class DerivedAccessDemo : AccessModifierDemo
{
    public void ShowFromDerived()
    {
        // Console.WriteLine(privateField);
        //   -> CS0122: 'AccessModifierDemo.privateField' is inaccessible
        //      due to its protection level. "private" never crosses a class
        //      boundary, not even to a subclass.

        Console.WriteLine($"From derived class: protected={ProtectedField}, " +
            $"internal={InternalField}, protectedInternal={ProtectedInternalField}, " +
            $"privateProtected={PrivateProtectedField} " +
            "(both work here because this derived class is in the SAME assembly)");
    }
}
