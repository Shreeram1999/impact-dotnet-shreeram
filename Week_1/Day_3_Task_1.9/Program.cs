// ===== Part 1: two ints (value type) =====
int a = 10;
int b = a;   // COPIES the value 10 into b
b = 99;
Console.WriteLine($"int:   a={a}, b={b}");
// a=10, b=99 -> "b = a" copied the VALUE. a and b are two independent
// storage locations on the stack; changing b has no effect on a.

// ===== Part 2: int[] (reference type) =====
int[] arr1 = { 10, 20, 30 };
int[] arr2 = arr1;   // COPIES the REFERENCE (the address), not the array data
arr2[0] = 999;
Console.WriteLine($"array: arr1[0]={arr1[0]}, arr2[0]={arr2[0]}");
// arr1[0]=999, arr2[0]=999 -> arrays are reference types. "arr2 = arr1" copied
// the pointer to the SAME array object on the heap, so both variables look at
// the same underlying data. Mutating through arr2 is visible through arr1.

// ===== Part 3: Coordinate struct (value type) vs Coordinate class (reference type) =====
CoordinateStruct s1 = new CoordinateStruct { X = 1, Y = 2 };
CoordinateStruct s2 = s1;   // COPIES the whole struct (all its fields)
s2.X = 100;
Console.WriteLine($"struct: s1.X={s1.X}, s2.X={s2.X}");
// s1.X=1, s2.X=100 -> a struct is a value type, so "s2 = s1" copied the fields
// into a brand-new, independent CoordinateStruct. Just like the int example.

CoordinateClass c1 = new CoordinateClass { X = 1, Y = 2 };
CoordinateClass c2 = c1;   // COPIES the REFERENCE to the same object
c2.X = 100;
Console.WriteLine($"class:  c1.X={c1.X}, c2.X={c2.X}");
// c1.X=100, c2.X=100 -> a class is a reference type, so "c2 = c1" copied the
// pointer, not the data. c1 and c2 both point at the one CoordinateClass
// instance on the heap, so the mutation through c2 is visible through c1.
// This mirrors the array case exactly (arrays are also reference types).

struct CoordinateStruct
{
    public int X;
    public int Y;
}

class CoordinateClass
{
    public int X;
    public int Y;
}
