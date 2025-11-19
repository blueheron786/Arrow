# Arrow

[![.NET](https://github.com/blueheron786/Arrow/actions/workflows/dotnet.yml/badge.svg)](https://github.com/blueheron786/Arrow/actions/workflows/dotnet.yml)

A feature-rich, lightweight website base you can use to build .NET Blazor websites FAST.

Built in C#, Blazor, and .NET 10!

# Included Features

- Local user registration and authentication backed by PostgreSQL + secure cookie sessions
- Password hashing with BCrypt (salted per best practices)
- Automatic schema management via FluentMigrator (users table + future migrations)
- An empty background task that starts and stops with the application
- Email integration
- A basic analytics page that shows home page views

# Using Arrow

1. Clone this repository and open it in VS Code.
2. Update `ConnectionStrings:DefaultConnection` in `Arrow.Blazor/appsettings.Development.json` to match your PostgreSQL instance.
3. Build and run the Blazor Server app (migrations run automatically on startup):

```powershell
cd Arrow
dotnet run --project Arrow.Blazor
```

## Configuring Email

If you plan to send emails in your application, modify `appsettings.Development.json` and fill in the `MailerSend` settings (API key, from address, etc.).

# Best Uses

While you can use Arrow for production-grade code, I recommend using it to prototype quickly and ship working, secure web applications.  Should you find a need to extend and grow your application, or rearchitect it (e.g. to use a Clean Architecture), you can always adapt that later.
