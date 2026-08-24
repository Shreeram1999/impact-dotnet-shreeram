# PATTERNS.md - where each pattern shows up later

Task 3.15's deliverable: a map from every pattern touched this week to
where it's expected to reappear in later weeks of the course. The three
called out explicitly in this week's objective - **Repository + Unit of
Work**, **Strategy**, and **async/await** - are the ones most directly
reused, since they're the exact seams the later "web weeks" (from Week 4
onward, per the fresher note) plug real implementations into. The rest are
mapped based on where that kind of problem typically shows up in a
web/API-shaped codebase.

## The three explicitly "reused later" patterns

- **Repository + Unit of Work** (`Day_3_Task_3.8`) → **data layer, Week 7**.
  `IRepository<T>` and `IUnitOfWork` here are backed by a plain in-memory
  `List<T>`. When Week 7 introduces real database access (Entity Framework
  Core or similar), the SAME interfaces get a new implementation backed by
  an actual database - nothing that already depends on `IRepository<T>`
  or `IUnitOfWork` has to change. This is the whole point of building the
  seam now, before there's a real database to plug into it.

- **Strategy** (`Day_3_Task_3.7`) → **runtime choices, especially in a Web
  API's request handling**. `IPaymentStrategy` here picks HOW to pay based
  on a runtime value; the same shape reappears anywhere a web API needs to
  pick between multiple valid algorithms/behaviors at request time - e.g.
  different pricing/discount rules per customer tier, different export
  formats per request, or different notification channels per user
  preference - all without an if/else chain spreading across the codebase.

- **async/await** (`Day_1_Task_3.3`) → **every I/O-bound Web API endpoint,
  from Week 4 onward**. The `FetchUserDataAsync`/`Task.WhenAll` pattern
  here is a stand-in for what every controller action, database query, and
  outbound HTTP call looks like in ASP.NET Core: `async Task<...>` methods
  awaiting I/O without blocking a thread, so the server can keep handling
  other requests while waiting on the database or a downstream service.

## The rest of this week's patterns

- **Observer** (`Day_2_Task_3.6`) → **events / real-time notifications**.
  The `event`-based version (`EventObserver.cs`) is the direct ancestor of
  things like SignalR hubs pushing updates to connected clients, or
  domain events firing when something changes (e.g. "OrderPlaced") that
  other parts of a web app react to.

- **Factory / Factory Method** (`Day_2_Task_3.5`) → **service creation**,
  particularly wherever a Web API needs to construct the "right"
  implementation of something based on a runtime value (a request type, a
  configuration setting, a tenant ID) - the same role .NET's own
  dependency injection container plays when it decides which concrete
  class to hand back for a requested interface.

- **Singleton** (`Day_2_Task_3.4`) → **DI service lifetimes**. Hand-writing
  a Singleton with `Lazy<T>` here is exactly the problem ASP.NET Core's
  built-in dependency injection container solves for you when a service is
  registered as `AddSingleton<T>()` - "give every caller across the whole
  app the same one instance" becomes a one-line registration instead of
  custom code.

- **Adapter** (`Day_3_Task_3.9`, `XmlReportAdapter`) → **integrating
  third-party APIs and libraries**. Any time a later week needs to call an
  external service or library whose interface doesn't match what the rest
  of the app expects (a different data format, a different method
  signature), an Adapter wraps it so the rest of the app only ever talks
  to a consistent interface of our own design.

- **Facade** (`Day_3_Task_3.9`, `OrderFacade`) → **service layer /
  application layer classes**. `OrderFacade.PlaceOrder()` coordinating
  Inventory/Payment/Shipping behind one call is the same shape as a Web
  API's service layer, where one method call from a controller action
  coordinates several lower-level services/repositories underneath.

## The 8 short-note patterns (see `Day_3_Task_3.9/ShortPatternNotes.md`)

- **Builder** → fluent configuration APIs, e.g. building up an EF Core
  query or an `HttpRequestMessage` step by step.
- **Prototype** → cloning fully-configured objects (like a DTO or a test
  fixture) as a starting point instead of rebuilding one from scratch.
- **Decorator** → ASP.NET Core's middleware pipeline, where each
  middleware wraps the next and adds one piece of behavior (logging,
  auth, compression) around the request.
- **Command** → encapsulating "a request to do something" as its own
  object - the same shape used by CQRS-style command/handler libraries
  like MediatR, which shows up a lot in larger ASP.NET Core apps.
- **Template Method** → base controller or base service classes that fix
  the overall steps (validate → process → respond) while letting
  subclasses override individual steps.
- **Mediator** → the same MediatR-style library mentioned under Command:
  a central mediator object that requests/handlers talk through instead
  of talking to each other directly.
- **Chain of Responsibility** → also maps onto the ASP.NET Core middleware
  pipeline (each middleware can either handle a request or pass it to the
  next one in the chain) - Decorator and Chain of Responsibility often
  describe the same real-world pipeline from two different angles.
- **State** → order/workflow status objects (e.g. an order that behaves
  differently depending on whether it's Pending, Shipped, or Cancelled) -
  a natural fit once Week 7+ introduces persisted entities with a status field.
