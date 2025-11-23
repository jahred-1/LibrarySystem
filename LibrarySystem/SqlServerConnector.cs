using System;
using System.Text;
using Microsoft.Data.SqlClient;

namespace LibrarySystem
{
    /// <summary>
    /// Helper to provision and validate a SQL Server database for the app.
    /// This does not change existing DatabaseHelper usage but allows you
    /// to create the SQL Server database and tables if you want to migrate.
    /// </summary>
    public static class SqlServerConnector
    {
        // Update this connection string to point to your SQL Server instance.
        // Example for local SQL Server Express (Windows Auth):
        // "Server=.\\SQLEXPRESS;Database=LibrarySystem;Trusted_Connection=True;TrustServerCertificate=True;"
        // Example for default local instance:
        // "Server=localhost;Database=LibrarySystem;Trusted_Connection=True;TrustServerCertificate=True;"
        public static string ConnectionString { get; set; } = "Server=localhost;Database=LibrarySystem;Trusted_Connection=True;TrustServerCertificate=True;";

        /// <summary>
        /// Tests connectivity to SQL Server using the configured connection string.
        /// </summary>
        public static bool TestConnection(out string message)
        {
            try
            {
                using var conn = new SqlConnection(ConnectionString);
                conn.Open();
                message = "Connection successful.";
                return true;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return false;
            }
        }

        /// <summary>
        /// Ensures the database exists and the required tables are created.
        /// This will create the database if it does not exist.
        /// </summary>
        public static bool EnsureDatabaseAndSchema(out string message)
        {
            try
            {
                var builder = new SqlConnectionStringBuilder(ConnectionString);
                var dbName = builder.InitialCatalog;
                if (string.IsNullOrWhiteSpace(dbName)) dbName = "LibrarySystem";

                // Connect to master to create DB if missing
                var masterBuilder = new SqlConnectionStringBuilder(ConnectionString)
                {
                    InitialCatalog = "master"
                };

                using (var conn = new SqlConnection(masterBuilder.ConnectionString))
                {
                    conn.Open();
                    using var cmd = conn.CreateCommand();
                    cmd.CommandText = $@"
IF DB_ID(N'{dbName}') IS NULL
BEGIN
    CREATE DATABASE [{dbName}];
END";
                    cmd.ExecuteNonQuery();
                }

                // Run schema creation against the target DB
                using (var conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();
                    using var cmd = conn.CreateCommand();
                    cmd.CommandText = @"
IF OBJECT_ID('dbo.Students','U') IS NULL
BEGIN
CREATE TABLE dbo.Students (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    StudentId NVARCHAR(100) UNIQUE NOT NULL,
    FullName NVARCHAR(250) NOT NULL,
    Email NVARCHAR(250) UNIQUE NOT NULL,
    Password NVARCHAR(250) NOT NULL,
    Course NVARCHAR(150) NOT NULL,
    Section NVARCHAR(150) NOT NULL,
    CreatedAt DATETIME2 NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1
);
END

IF OBJECT_ID('dbo.Admins','U') IS NULL
BEGIN
CREATE TABLE dbo.Admins (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Email NVARCHAR(250) UNIQUE NOT NULL,
    Password NVARCHAR(250) NOT NULL,
    FullName NVARCHAR(250) NOT NULL,
    CreatedAt DATETIME2 NOT NULL
);
END

IF OBJECT_ID('dbo.Books','U') IS NULL
BEGIN
CREATE TABLE dbo.Books (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Title NVARCHAR(500) NOT NULL,
    Author NVARCHAR(250) NOT NULL,
    ISBN NVARCHAR(100) UNIQUE NOT NULL,
    Category NVARCHAR(200) NOT NULL,
    TotalCopies INT NOT NULL,
    AvailableCopies INT NOT NULL,
    AddedDate DATETIME2 NOT NULL,
    YearPublished INT NULL,
    Description NVARCHAR(MAX) NULL
);
END

IF OBJECT_ID('dbo.BorrowRecords','U') IS NULL
BEGIN
CREATE TABLE dbo.BorrowRecords (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    StudentId INT NOT NULL,
    BookId INT NOT NULL,
    IssueDate DATETIME2 NOT NULL,
    DueDate DATETIME2 NOT NULL,
    ReturnDate DATETIME2 NULL,
    Status NVARCHAR(50) NOT NULL,
    FineAmount DECIMAL(18,2) NULL
);
END

IF OBJECT_ID('dbo.Reservations','U') IS NULL
BEGIN
CREATE TABLE dbo.Reservations (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    StudentId INT NOT NULL,
    BookId INT NOT NULL,
    ReservationDate DATETIME2 NOT NULL,
    HoldUntilDate DATETIME2 NULL,
    Status NVARCHAR(50) NOT NULL
);
END

IF OBJECT_ID('dbo.Announcements','U') IS NULL
BEGIN
CREATE TABLE dbo.Announcements (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Title NVARCHAR(500) NOT NULL,
    Content NVARCHAR(MAX) NOT NULL,
    CreatedDate DATETIME2 NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1
);
END

IF OBJECT_ID('dbo.PasswordResets','U') IS NULL
BEGIN
CREATE TABLE dbo.PasswordResets (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    StudentId INT NOT NULL,
    Code NVARCHAR(250) NOT NULL,
    ExpiresAt DATETIME2 NOT NULL,
    CreatedAt DATETIME2 NOT NULL
);
END
";
                    cmd.ExecuteNonQuery();
                }

                message = "Database and schema are ready.";
                return true;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return false;
            }
        }
    }
}
