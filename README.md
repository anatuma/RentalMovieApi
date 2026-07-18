# Movie Rental API

A Web API for movie rentals — checkout multiple titles at once, keep the database honest, and map nested rental history without letting EF Core do all the thinking.

This one uses **raw ADO.NET** on purpose. Flat SQL rows come back from JOINs; C# groups them into clean nested DTOs. Repository + Service layers keep things readable.

## What it does

- **Customer rental history** — nested response: rentals → items → movies, dates, status
- **Rent movies (batch)** — one checkout can include several titles
- **Business checks before write:**
  - Customer must exist
  - Every movie must be in the catalog
  - No renting a movie you already have active on your account
- **Transactions** — writes to `Rental` and `Rental_Item` together; rolls back if anything breaks mid-checkout

## Tech stack

- **C# / ASP.NET Core Web API**
- **ADO.NET** (`Microsoft.Data.SqlClient`)
- **SQL Server**
- **Swagger** in Development

## API endpoints

| Method | Route | Notes |
|--------|-------|-------|
| `GET` | `/api/customers/{id}/rentals` | Full nested rental history |
| `POST` | `/api/customers/{id}/rentals` | Create rental — body: list of movie IDs |

## Run it locally

1. **Create the database** — run `RentalMovieDB_create.sql` against your SQL Server instance.
2. **Check the connection string** in `appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=YOUR_SERVER;Database=RentalMovieDB;Trusted_Connection=True;TrustServerCertificate=True;"
     }
   }
   ```
3. **Run the API:**
   ```bash
   dotnet run
   ```
4. Open Swagger at **http://localhost:5117/swagger**

> **Note:** The solution/project file is named `Revision` internally — leftover from an earlier iteration. The repo name is the honest one.
