# Student Management Console App - layer responsibilities

- **Model** (`/Models`) - data and its own invariants only (`Student.Age` rejects anything outside 5-100). No `Console.*`, no `List<T>`, no knowledge that a repository or a menu exists.
- **View** (`/Views`) - console input/output only: prints menus and tables, reads raw input. It has no business `if` statements - it never decides whether an age or a roll number is valid, only whether what the user typed parses as a number.
- **Controller** (`/Controllers`) - orchestration only: reads a menu choice, calls one Service method, hands the result straight to the View. It never formats a message and never touches a `List<T>` directly.
- **Service** (`/Services`) - every business rule lives here (duplicate roll numbers, valid age, non-empty name), and every mutation is recorded to the shared `TransactionLog`. `/Data`'s `IRepository<T>` sits behind it, so the Service never knows whether it's talking to an in-memory list or a real database.
