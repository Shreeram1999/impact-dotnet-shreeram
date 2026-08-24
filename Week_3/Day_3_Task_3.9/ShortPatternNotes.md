# Short notes on 8 more design patterns

These 8 patterns aren't implemented as runnable code this week (only the
"headline" patterns - Singleton, Factory, Observer, Strategy, Repository +
Unit of Work, Adapter, Facade - are), but it's worth knowing what each one
is for and roughly what it looks like, since you'll likely bump into all of
them in real .NET codebases.

## Builder

Builder separates "constructing a complex object step by step" from "what
the finished object looks like". Instead of one giant constructor with ten
optional parameters, you chain a series of small, readable method calls
(often ending in a `.Build()`), and each call sets one piece of the object.

**Use case:** Constructing a complex `HttpRequestMessage` or a SQL query
string piece by piece - `new QueryBuilder().Select("Name").From("Users").Where("Age > 18").Build()`.

## Prototype

Prototype creates new objects by CLONING an existing "prototype" object,
instead of building a brand-new one from scratch with `new`. This is handy
when creating an object from scratch is expensive, or when you want a new
object that starts out "the same as this one" with just a few tweaks.

**Use case:** Cloning a fully-configured `Employee` record as a starting
template for a new hire in the same role, instead of re-entering every field.

## Decorator

Decorator lets you add extra behavior to an object by WRAPPING it in
another object that implements the same interface, rather than editing the
original class or creating a big tree of subclasses for every combination
of features.

**Use case:** Wrapping a plain `IDataStream` in a `CompressingStream`, then
wrapping THAT in an `EncryptingStream` - each layer adds one capability
without the base stream class needing to know about compression or
encryption at all.

## Command

Command turns "a request to do something" into its own object (with a
method like `Execute()`), instead of calling a method directly. Because the
request is now an object, it can be queued, logged, undone, or passed
around like any other piece of data.

**Use case:** A menu/toolbar button in a desktop app where each button
holds an `ICommand` object (like `SaveCommand`, `UndoCommand`) - the button
itself doesn't know or care what the command actually does.

## Template Method

Template Method puts the overall STEPS of an algorithm in a base class
method, but lets subclasses override individual steps. The base class
controls the order things happen in; subclasses only fill in the details
that differ.

**Use case:** A `DataImporter` base class with a fixed `Import()` method
that calls `ReadFile()`, `Validate()`, `SaveToDatabase()` in that order -
`CsvImporter` and `JsonImporter` subclasses only override `ReadFile()`.

## Mediator

Mediator introduces a single "middleman" object that a group of other
objects all talk to, instead of talking to each other directly. This
avoids a tangled web where every object needs a direct reference to every
other object it might need to notify.

**Use case:** A chat room class that all `User` objects send messages
through - users never message each other directly, only the chat room
(the mediator), which then forwards messages on.

## Chain of Responsibility

Chain of Responsibility passes a request along a chain of handler objects,
where each handler decides "can I deal with this myself, or should I pass
it to the next handler in the chain?" The sender doesn't need to know which
handler will actually end up processing it.

**Use case:** Approving an expense report - a `TeamLeadHandler` approves
amounts under 5,000, otherwise passes it to a `ManagerHandler`, which
approves under 50,000, otherwise passes it further up to a `DirectorHandler`.

## State

State lets an object change its behavior when its internal "state"
changes, by delegating behavior to a separate state object instead of
using a big if/switch on a status field everywhere the object is used. The
object literally swaps out which state object it's currently using.

**Use case:** An `Order` that behaves differently depending on whether it's
in a `PendingState`, `ShippedState`, or `CancelledState` - calling
`order.Cancel()` does something different (or throws) depending on which
state object is currently active.
