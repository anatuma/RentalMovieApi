# Movie Rental API
This is a simple Web API designed to manage customer movie rentals.
Instead of relying on a massive ORM, this project goes old-school and talks directly to an SQL Server database using **raw ADO.NET**. It has a clean separation of concerns (Repositories + Services), transactional safety for multi-item checkouts and handles relational database mapping without the bulk.

## What this API does
* **Track Customer Rentals:** Fetch a nested list of a customer’s entire rental history (including dates, status and which movies they checked out).
* **No-ORM Nested Mapping:** Instead of using EF Core, we query flat SQL rows with JOINs and group them in C# into clean, structured DTOs.
* **Rent Movies (with Rules):** Customers can check out multiple movies at once. The API runs several checks before letting them proceed:
    * Does the customer exist?
    * Are the movies actually in our catalog?
    * **No Double-Renting:** Customers can't rent a movie they already have active on their account!
* **Safe Transactions:** Creating a rental writes to both `Rental` and `Rental_Item` tables. If anything fails mid-way, the transaction rolls back safely to keep the database consistent.

## The tech under the hood
* **C# + .NET 8 (or 9)** with Web API.
* **Microsoft.Data.SqlClient** (Pure ADO.NET).
* **SQL Server** for the database.
* **Swagger** for testing the endpoints.

## Getting started
1. **Set up your database:** Ensure you have SQL Server running and a database ready.
2. **Hook up the connection:** Paste your connection string into `appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=YOUR_SERVER;Database=MovieRentalDB;Trusted_Connection=True;TrustServerCertificate=True;"
     }
   }