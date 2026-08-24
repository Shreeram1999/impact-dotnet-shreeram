# Interface vs Abstract Class

A quick reference comparing the two, since it comes up constantly.

| | Interface | Abstract class |
|---|---|---|
| **Inheritance** | A class can implement **any number** of interfaces at once. | A class can inherit from **only one** class (abstract or not) - C# has single inheritance. |
| **State** | Cannot hold instance fields (no `private int count;`). Just a list of member signatures everyone must implement. | Can hold real fields and shared state, exactly like a normal class. |
| **Constructors** | Cannot have a constructor - there is nothing to construct, it's just a contract. | Can have a constructor, which runs when a subclass is constructed via `: base(...)` (see Week 2's Vehicle/Car chain). |
| **Versioning** | Adding a new member to an interface breaks EVERY class that already implements it (they now fail to compile, since they don't implement the new member) - unless you use a C# default interface method to give it a body. | Adding a new *concrete* method to an abstract class does NOT break existing subclasses - they simply inherit it for free without needing any changes. |

## One scenario favoring each

**Favors an interface:** `IPaymentStrategy` (Task 3.7). Credit card, UPI, and
net banking payments have nothing in common structurally - no shared
fields, no shared base behavior - they just all need to expose one
`Pay(amount)` method. An interface captures exactly that "can do this one
thing" contract without forcing them into an artificial shared class
hierarchy. It also means `ShoppingCart` could just as easily accept a
strategy that ALSO implements some other unrelated interface, since a
class implementing `IPaymentStrategy` isn't locked out of implementing
anything else.

**Favors an abstract class:** `Shape` (Week 2, Task 2.4). Circle and
Rectangle both need the SAME shared method (`DisplayArea()`, which calls
`CalculateArea()` under the hood) - writing that logic once on an abstract
base class and inheriting it is simpler and avoids duplicating identical
code in every shape. An interface couldn't express "here's a method with a
real body that every shape gets for free" nearly as directly.

## Static vs instance - MathHelper vs OrderProcessor

See `MathHelper.cs` and `OrderProcessor.cs` in this folder for the
runnable code. Short version: `MathHelper`'s methods (`Factorial`,
`IsPrime`, `GCD`) are `static` because they're pure calculations with no
state to remember between calls - just like `Math.Sqrt(...)`. `OrderProcessor`'s
methods are instance methods because each processor needs to remember its
OWN list of orders it has handled, and different `OrderProcessor` objects
need that list to stay independent from each other.
