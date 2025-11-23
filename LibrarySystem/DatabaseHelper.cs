using LibrarySystem.Models;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace LibrarySystem
{
    public static partial class DatabaseHelper
    {
        public static event Action? DataChanged;

        private static readonly object _lock = new object();
        private static readonly string DataFolder;
        private static readonly string DatabasePath;

        static DatabaseHelper()
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            DataFolder = Path.Combine(appData, "LibrarySystem");
            if (!Directory.Exists(DataFolder)) Directory.CreateDirectory(DataFolder);
            DatabasePath = Path.Combine(DataFolder, "LibrarySystem.db");
        }

        private static void OnDataChanged()
        {
            try
            {
                DataChanged?.Invoke();
            }
            catch { }
        }

        private static string GetConnectionString()
        {
            return $"Data Source={DatabasePath};";
        }

        // Public initialization - creates database and tables if needed
        public static void InitializeDatabase()
        {
            lock (_lock)
            {
                EnsureDatabaseAndTables();
                
                // Migrate from JSON if it exists and database is empty
                var accountsFile = Path.Combine(DataFolder, "accounts.json");
                if (File.Exists(accountsFile))
                {
                    MigrateFromJson(accountsFile);
                }

                // Seed default data if database is empty
                if (GetTotalBooks() == 0)
                {
                    SeedBooksDefaults();
                }
                if (GetTotalStudents() == 0 && GetAdminByEmail("libraryadmin@gmail.com") == null)
                {
                    SeedAccountDefaults();
                }
            }

            OnDataChanged();
        }

        private static void EnsureDatabaseAndTables()
        {
            using var conn = new SqliteConnection(GetConnectionString());
            conn.Open();

            // Create tables
            var createTables = @"
                CREATE TABLE IF NOT EXISTS Students (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    StudentId TEXT UNIQUE NOT NULL,
                    FullName TEXT NOT NULL,
                    Email TEXT UNIQUE NOT NULL,
                    Password TEXT NOT NULL,
                    Course TEXT NOT NULL,
                    Section TEXT NOT NULL,
                    CreatedAt TEXT NOT NULL,
                    IsActive INTEGER NOT NULL DEFAULT 1
                );

                CREATE TABLE IF NOT EXISTS Admins (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Email TEXT UNIQUE NOT NULL,
                    Password TEXT NOT NULL,
                    FullName TEXT NOT NULL,
                    CreatedAt TEXT NOT NULL
                );

                CREATE TABLE IF NOT EXISTS Books (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Title TEXT NOT NULL,
                    Author TEXT NOT NULL,
                    ISBN TEXT UNIQUE NOT NULL,
                    Category TEXT NOT NULL,
                    TotalCopies INTEGER NOT NULL,
                    AvailableCopies INTEGER NOT NULL,
                    AddedDate TEXT NOT NULL,
                    YearPublished INTEGER,
                    Description TEXT
                );

                CREATE TABLE IF NOT EXISTS BorrowRecords (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    StudentId INTEGER NOT NULL,
                    BookId INTEGER NOT NULL,
                    IssueDate TEXT NOT NULL,
                    DueDate TEXT NOT NULL,
                    ReturnDate TEXT,
                    Status TEXT NOT NULL,
                    FineAmount REAL
                );

                CREATE TABLE IF NOT EXISTS Reservations (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    StudentId INTEGER NOT NULL,
                    BookId INTEGER NOT NULL,
                    ReservationDate TEXT NOT NULL,
                    HoldUntilDate TEXT,
                    Status TEXT NOT NULL
                );

                CREATE TABLE IF NOT EXISTS Announcements (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Title TEXT NOT NULL,
                    Content TEXT NOT NULL,
                    CreatedDate TEXT NOT NULL,
                    IsActive INTEGER NOT NULL DEFAULT 1
                );

                CREATE TABLE IF NOT EXISTS PasswordResets (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    StudentId INTEGER NOT NULL,
                    Code TEXT NOT NULL,
                    ExpiresAt TEXT NOT NULL,
                    CreatedAt TEXT NOT NULL
                );
            ";

            using var cmd = new SqliteCommand(createTables, conn);
            cmd.ExecuteNonQuery();
        }

        private static void SeedAccountDefaults()
        {
            using var conn = new SqliteConnection(GetConnectionString());
            conn.Open();

            // Insert default admin
            using var adminCmd = new SqliteCommand(
                "INSERT INTO Admins (Email, Password, FullName, CreatedAt) VALUES (@email, @password, @fullName, @createdAt)",
                conn);
            adminCmd.Parameters.AddWithValue("@email", "libraryadmin@gmail.com");
            adminCmd.Parameters.AddWithValue("@password", "admin123");
            adminCmd.Parameters.AddWithValue("@fullName", "Library Administrator");
            adminCmd.Parameters.AddWithValue("@createdAt", DateTime.Now.ToString("O"));
            adminCmd.ExecuteNonQuery();

            // Insert default student
            using var studentCmd = new SqliteCommand(
                "INSERT INTO Students (StudentId, FullName, Email, Password, Course, Section, CreatedAt, IsActive) VALUES (@studentId, @fullName, @email, @password, @course, @section, @createdAt, @isActive)",
                conn);
            studentCmd.Parameters.AddWithValue("@studentId", "S1001");
            studentCmd.Parameters.AddWithValue("@fullName", "Test Student");
            studentCmd.Parameters.AddWithValue("@email", "student@example.com");
            studentCmd.Parameters.AddWithValue("@password", "pass");
            studentCmd.Parameters.AddWithValue("@course", "Computer Science");
            studentCmd.Parameters.AddWithValue("@section", "A");
            studentCmd.Parameters.AddWithValue("@createdAt", DateTime.Now.ToString("O"));
            studentCmd.Parameters.AddWithValue("@isActive", 1);
            studentCmd.ExecuteNonQuery();
        }

        private static void MigrateFromJson(string jsonFile)
        {
            try
            {
                if (!File.Exists(jsonFile)) return;
                var json = File.ReadAllText(jsonFile);
                var ds = JsonSerializer.Deserialize<AccountStore>(json);
                if (ds == null) return;

                using var conn = new SqliteConnection(GetConnectionString());
                conn.Open();

                // Migrate Admins
                foreach (var admin in ds.Admins ?? new List<Admin>())
                {
                    using var cmd = new SqliteCommand(
                        "INSERT OR IGNORE INTO Admins (Id, Email, Password, FullName, CreatedAt) VALUES (@id, @email, @password, @fullName, @createdAt)",
                        conn);
                    cmd.Parameters.AddWithValue("@id", admin.Id);
                    cmd.Parameters.AddWithValue("@email", admin.Email);
                    cmd.Parameters.AddWithValue("@password", admin.Password);
                    cmd.Parameters.AddWithValue("@fullName", admin.FullName);
                    cmd.Parameters.AddWithValue("@createdAt", admin.CreatedAt.ToString("O"));
                    cmd.ExecuteNonQuery();
                }

                // Migrate Students
                foreach (var student in ds.Students ?? new List<Student>())
                {
                    using var cmd = new SqliteCommand(
                        "INSERT OR IGNORE INTO Students (Id, StudentId, FullName, Email, Password, Course, Section, CreatedAt, IsActive) VALUES (@id, @studentId, @fullName, @email, @password, @course, @section, @createdAt, @isActive)",
                        conn);
                    cmd.Parameters.AddWithValue("@id", student.Id);
                    cmd.Parameters.AddWithValue("@studentId", student.StudentId);
                    cmd.Parameters.AddWithValue("@fullName", student.FullName);
                    cmd.Parameters.AddWithValue("@email", student.Email);
                    cmd.Parameters.AddWithValue("@password", student.Password);
                    cmd.Parameters.AddWithValue("@course", student.Course);
                    cmd.Parameters.AddWithValue("@section", student.Section);
                    cmd.Parameters.AddWithValue("@createdAt", student.CreatedAt.ToString("O"));
                    cmd.Parameters.AddWithValue("@isActive", student.IsActive ? 1 : 0);
                    cmd.ExecuteNonQuery();
                }

                // Migrate PasswordResets
                foreach (var pr in ds.PasswordResets ?? new List<PasswordReset>())
                {
                    using var cmd = new SqliteCommand(
                        "INSERT OR IGNORE INTO PasswordResets (Id, StudentId, Code, ExpiresAt, CreatedAt) VALUES (@id, @studentId, @code, @expiresAt, @createdAt)",
                        conn);
                    cmd.Parameters.AddWithValue("@id", pr.Id);
                    cmd.Parameters.AddWithValue("@studentId", pr.StudentId);
                    cmd.Parameters.AddWithValue("@code", pr.Code);
                    cmd.Parameters.AddWithValue("@expiresAt", pr.ExpiresAt.ToString("O"));
                    cmd.Parameters.AddWithValue("@createdAt", pr.CreatedAt.ToString("O"));
                    cmd.ExecuteNonQuery();
                }
            }
            catch { }
        }

        private static void SeedBooksDefaults()
        {
            using var conn = new SqliteConnection(GetConnectionString());
            conn.Open();

            var books = new List<Book>
            {
                new Book { Title = "Introduction to Programming", Author = "James Foster", ISBN = "CCS-0001", Category = "College of Computer Studies", TotalCopies = 4, AvailableCopies = 4, AddedDate = DateTime.Now, YearPublished = 2020, Description = "Basic programming concepts using Python." },
                new Book { Title = "Data Structures and Algorithms", Author = "Maria Santos", ISBN = "CCS-0002", Category = "College of Computer Studies", TotalCopies = 3, AvailableCopies = 3, AddedDate = DateTime.Now, YearPublished = 2019, Description = "Core data structures and algorithmic problem solving." },
                new Book { Title = "Database Management Systems", Author = "Eric Tan", ISBN = "CCS-0003", Category = "College of Computer Studies", TotalCopies = 4, AvailableCopies = 4, AddedDate = DateTime.Now, YearPublished = 2018, Description = "Relational databases, SQL, and database design principles." },
                new Book { Title = "Web Development Fundamentals", Author = "Lara Kim", ISBN = "CCS-0004", Category = "College of Computer Studies", TotalCopies = 3, AvailableCopies = 3, AddedDate = DateTime.Now, YearPublished = 2021, Description = "HTML, CSS, JavaScript, and responsive design." },
                new Book { Title = "Object-Oriented Programming", Author = "Daniel Cruz", ISBN = "CCS-0005", Category = "College of Computer Studies", TotalCopies = 4, AvailableCopies = 4, AddedDate = DateTime.Now, YearPublished = 2017, Description = "Concepts and implementation of OOP in Java." },
                new Book { Title = "Software Engineering Principles", Author = "Hannah Reed", ISBN = "CCS-0006", Category = "College of Computer Studies", TotalCopies = 3, AvailableCopies = 3, AddedDate = DateTime.Now, YearPublished = 2019, Description = "Software development methodologies and project management." },
                new Book { Title = "Computer Networks", Author = "Victor Ramos", ISBN = "CCS-0007", Category = "College of Computer Studies", TotalCopies = 4, AvailableCopies = 4, AddedDate = DateTime.Now, YearPublished = 2016, Description = "Network concepts, OSI model, and routing." },
                new Book { Title = "Cybersecurity Essentials", Author = "Ella Parker", ISBN = "CCS-0008", Category = "College of Computer Studies", TotalCopies = 3, AvailableCopies = 3, AddedDate = DateTime.Now, YearPublished = 2020, Description = "Information security concepts and threat mitigation." },
                new Book { Title = "Human-Computer Interaction", Author = "Ian Torres", ISBN = "CCS-0009", Category = "College of Computer Studies", TotalCopies = 3, AvailableCopies = 3, AddedDate = DateTime.Now, YearPublished = 2015, Description = "User-centered interface design and usability." },
                new Book { Title = "Machine Learning Basics", Author = "Olivia Grant", ISBN = "CCS-0010", Category = "College of Computer Studies", TotalCopies = 3, AvailableCopies = 3, AddedDate = DateTime.Now, YearPublished = 2022, Description = "Introduction to machine learning and AI." },
                new Book { Title = "Mobile App Development", Author = "Chris Howard", ISBN = "CCS-0011", Category = "College of Computer Studies", TotalCopies = 4, AvailableCopies = 4, AddedDate = DateTime.Now, YearPublished = 2021, Description = "Building Android and iOS applications." },
                new Book { Title = "Operating Systems Concepts", Author = "Faith Navarro", ISBN = "CCS-0012", Category = "College of Computer Studies", TotalCopies = 3, AvailableCopies = 3, AddedDate = DateTime.Now, YearPublished = 2018, Description = "Fundamentals of operating systems and resource management." },
                new Book { Title = "Systems Analysis and Design", Author = "Kevin Price", ISBN = "CCS-0013", Category = "College of Computer Studies", TotalCopies = 3, AvailableCopies = 3, AddedDate = DateTime.Now, YearPublished = 2016, Description = "Modeling and designing information systems." },
                new Book { Title = "Artificial Intelligence Concepts", Author = "Jenny Cruz", ISBN = "CCS-0014", Category = "College of Computer Studies", TotalCopies = 3, AvailableCopies = 3, AddedDate = DateTime.Now, YearPublished = 2019, Description = "AI applications and intelligent systems." },
                new Book { Title = "Cloud Computing Fundamentals", Author = "Patrick Lane", ISBN = "CCS-0015", Category = "College of Computer Studies", TotalCopies = 4, AvailableCopies = 4, AddedDate = DateTime.Now, YearPublished = 2020, Description = "Cloud architecture and virtualization." },
                new Book { Title = "Computer Architecture", Author = "Derek Lee", ISBN = "CCS-0016", Category = "College of Computer Studies", TotalCopies = 3, AvailableCopies = 3, AddedDate = DateTime.Now, YearPublished = 2017, Description = "Hardware organization and CPU design." },
                new Book { Title = "Parallel Computing", Author = "Nina Wells", ISBN = "CCS-0017", Category = "College of Computer Studies", TotalCopies = 2, AvailableCopies = 2, AddedDate = DateTime.Now, YearPublished = 2014, Description = "Parallel algorithms and distributed systems." },
                new Book { Title = "Data Mining Techniques", Author = "Samuel Ortiz", ISBN = "CCS-0018", Category = "College of Computer Studies", TotalCopies = 3, AvailableCopies = 3, AddedDate = DateTime.Now, YearPublished = 2021, Description = "Extracting insights from large datasets." },
                new Book { Title = "Game Development Essentials", Author = "Troy Mendoza", ISBN = "CCS-0019", Category = "College of Computer Studies", TotalCopies = 3, AvailableCopies = 3, AddedDate = DateTime.Now, YearPublished = 2016, Description = "Game design principles using Unity." },
                new Book { Title = "Computer Forensics", Author = "Zara Mitchell", ISBN = "CCS-0020", Category = "College of Computer Studies", TotalCopies = 2, AvailableCopies = 2, AddedDate = DateTime.Now, YearPublished = 2018, Description = "Digital evidence recovery and cybercrime investigation." }
            };

            // Education: 20 books
            var coedList = new List<Book>
            {
                new Book { Title = "Foundations of Education", Author = "Emily Davis", ISBN = "COED-0001", Category = "College of Education", TotalCopies = 4, AvailableCopies = 4, AddedDate = DateTime.Now, YearPublished = 2018, Description = "Overview of educational systems and teaching foundations." },
                new Book { Title = "Child and Adolescent Development", Author = "Mark Wilson", ISBN = "COED-0002", Category = "College of Education", TotalCopies = 3, AvailableCopies = 3, AddedDate = DateTime.Now, YearPublished = 2017, Description = "Developmental stages of learners." },
                new Book { Title = "Curriculum Development", Author = "Anna Rivera", ISBN = "COED-0003", Category = "College of Education", TotalCopies = 4, AvailableCopies = 4, AddedDate = DateTime.Now, YearPublished = 2019, Description = "Designing and evaluating curriculum." },
                new Book { Title = "Assessment in Learning", Author = "Kenneth Young", ISBN = "COED-0004", Category = "College of Education", TotalCopies = 3, AvailableCopies = 3, AddedDate = DateTime.Now, YearPublished = 2016, Description = "Assessment tools and measurement of learning." },
                new Book { Title = "Educational Psychology", Author = "Sarah Gomez", ISBN = "COED-0005", Category = "College of Education", TotalCopies = 4, AvailableCopies = 4, AddedDate = DateTime.Now, YearPublished = 2020, Description = "Psychology principles in education." },
                new Book { Title = "Teaching Strategies and Methods", Author = "Liam Carter", ISBN = "COED-0006", Category = "College of Education", TotalCopies = 3, AvailableCopies = 3, AddedDate = DateTime.Now, YearPublished = 2015, Description = "Effective teaching techniques." },
                new Book { Title = "Inclusive Education", Author = "Patricia Lim", ISBN = "COED-0007", Category = "College of Education", TotalCopies = 3, AvailableCopies = 3, AddedDate = DateTime.Now, YearPublished = 2018, Description = "Teaching diverse learners." },
                new Book { Title = "Educational Technology", Author = "Nathan Brooks", ISBN = "COED-0008", Category = "College of Education", TotalCopies = 4, AvailableCopies = 4, AddedDate = DateTime.Now, YearPublished = 2021, Description = "Using technology in teaching." },
                new Book { Title = "Classroom Management", Author = "Nicole Adams", ISBN = "COED-0009", Category = "College of Education", TotalCopies = 3, AvailableCopies = 3, AddedDate = DateTime.Now, YearPublished = 2016, Description = "Managing classroom behavior." },
                new Book { Title = "Educational Research Methods", Author = "Oscar Lee", ISBN = "COED-0010", Category = "College of Education", TotalCopies = 3, AvailableCopies = 3, AddedDate = DateTime.Now, YearPublished = 2019, Description = "Research fundamentals for education." },
                new Book { Title = "Literacy Development", Author = "Grace Hunt", ISBN = "COED-0011", Category = "College of Education", TotalCopies = 4, AvailableCopies = 4, AddedDate = DateTime.Now, YearPublished = 2018, Description = "Reading and writing development." },
                new Book { Title = "Educational Leadership", Author = "Henry Cruz", ISBN = "COED-0012", Category = "College of Education", TotalCopies = 3, AvailableCopies = 3, AddedDate = DateTime.Now, YearPublished = 2017, Description = "Leadership skills for educators." },
                new Book { Title = "Philosophy of Education", Author = "Elaine West", ISBN = "COED-0013", Category = "College of Education", TotalCopies = 3, AvailableCopies = 3, AddedDate = DateTime.Now, YearPublished = 2015, Description = "Philosophical foundations of teaching." },
                new Book { Title = "Science Teaching Methods", Author = "Albert Cruz", ISBN = "COED-0014", Category = "College of Education", TotalCopies = 3, AvailableCopies = 3, AddedDate = DateTime.Now, YearPublished = 2020, Description = "Teaching science with engaging strategies." },
                new Book { Title = "Mathematics for Educators", Author = "Donna Roberts", ISBN = "COED-0015", Category = "College of Education", TotalCopies = 4, AvailableCopies = 4, AddedDate = DateTime.Now, YearPublished = 2019, Description = "Math concepts and teaching approaches." },
                new Book { Title = "Educational Assessment Tools", Author = "Isaac Brown", ISBN = "COED-0016", Category = "College of Education", TotalCopies = 2, AvailableCopies = 2, AddedDate = DateTime.Now, YearPublished = 2014, Description = "Tools for evaluating student performance." },
                new Book { Title = "Guidance and Counseling", Author = "Theresa Lane", ISBN = "COED-0017", Category = "College of Education", TotalCopies = 3, AvailableCopies = 3, AddedDate = DateTime.Now, YearPublished = 2016, Description = "Counseling approaches for learners." },
                new Book { Title = "History of Education", Author = "Marvin Ellis", ISBN = "COED-0018", Category = "College of Education", TotalCopies = 3, AvailableCopies = 3, AddedDate = DateTime.Now, YearPublished = 2013, Description = "Evolution of educational practices." },
                new Book { Title = "Technology for Teaching Literacy", Author = "Faye Kim", ISBN = "COED-0019", Category = "College of Education", TotalCopies = 3, AvailableCopies = 3, AddedDate = DateTime.Now, YearPublished = 2021, Description = "Digital tools for literacy education." },
                new Book { Title = "Assessment and Evaluation", Author = "Philip Yang", ISBN = "COED-0020", Category = "College of Education", TotalCopies = 3, AvailableCopies = 3, AddedDate = DateTime.Now, YearPublished = 2017, Description = "Evaluating student learning outcomes." }
            };

            Books.AddRange(coedList);


            // Engineering: 20 books
            var coeList = new List<Book>
            {
                new Book { Title = "Engineering Mechanics", Author = "David Thornton", ISBN = "COE-0001", Category = "College of Engineering", TotalCopies = 4, AvailableCopies = 4, AddedDate = DateTime.Now, YearPublished = 2016, Description = "Statics and dynamics principles." },
                new Book { Title = "Thermodynamics", Author = "Sophia Vale", ISBN = "COE-0002", Category = "College of Engineering", TotalCopies = 3, AvailableCopies = 3, AddedDate = DateTime.Now, YearPublished = 2017, Description = "Energy systems and thermodynamic laws." },
                new Book { Title = "Engineering Mathematics", Author = "John Perez", ISBN = "COE-0003", Category = "College of Engineering", TotalCopies = 4, AvailableCopies = 4, AddedDate = DateTime.Now, YearPublished = 2019, Description = "Math concepts essential for engineering." },
                new Book { Title = "Electrical Circuits", Author = "Kathryn Lee", ISBN = "COE-0004", Category = "College of Engineering", TotalCopies = 3, AvailableCopies = 3, AddedDate = DateTime.Now, YearPublished = 2018, Description = "Circuits and electrical analysis." },
                new Book { Title = "Fluid Mechanics", Author = "Anthony Price", ISBN = "COE-0005", Category = "College of Engineering", TotalCopies = 4, AvailableCopies = 4, AddedDate = DateTime.Now, YearPublished = 2015, Description = "Study of fluid behavior and forces." },
                new Book { Title = "Materials Science", Author = "Grace Choi", ISBN = "COE-0006", Category = "College of Engineering", TotalCopies = 3, AvailableCopies = 3, AddedDate = DateTime.Now, YearPublished = 2020, Description = "Properties of engineering materials." },
                new Book { Title = "Computer-Aided Design (CAD)", Author = "Michael Cruz", ISBN = "COE-0007", Category = "College of Engineering", TotalCopies = 4, AvailableCopies = 4, AddedDate = DateTime.Now, YearPublished = 2021, Description = "Modern engineering design tools." },
                new Book { Title = "Engineering Economics", Author = "Linda Garcia", ISBN = "COE-0008", Category = "College of Engineering", TotalCopies = 3, AvailableCopies = 3, AddedDate = DateTime.Now, YearPublished = 2016, Description = "Economic decision making for engineers." },
                new Book { Title = "Control Systems Engineering", Author = "Peter Ramos", ISBN = "COE-0009", Category = "College of Engineering", TotalCopies = 3, AvailableCopies = 3, AddedDate = DateTime.Now, YearPublished = 2018, Description = "Stability and feedback systems." },
                new Book { Title = "Mechanics of Materials", Author = "Rachel Adams", ISBN = "COE-0010", Category = "College of Engineering", TotalCopies = 3, AvailableCopies = 3, AddedDate = DateTime.Now, YearPublished = 2017, Description = "Stress, strain, and deformation." },
                new Book { Title = "Structural Analysis", Author = "Kyle Sanders", ISBN = "COE-0011", Category = "College of Engineering", TotalCopies = 4, AvailableCopies = 4, AddedDate = DateTime.Now, YearPublished = 2018, Description = "Analyzing structural integrity." },
                new Book { Title = "Environmental Engineering", Author = "Tina Morales", ISBN = "COE-0012", Category = "College of Engineering", TotalCopies = 3, AvailableCopies = 3, AddedDate = DateTime.Now, YearPublished = 2019, Description = "Environmental systems and sustainability." },
                new Book { Title = "Engineering Design Process", Author = "Ryan Ford", ISBN = "COE-0013", Category = "College of Engineering", TotalCopies = 3, AvailableCopies = 3, AddedDate = DateTime.Now, YearPublished = 2015, Description = "Design methodologies in engineering." },
                new Book { Title = "Hydraulics Engineering", Author = "Samantha King", ISBN = "COE-0014", Category = "College of Engineering", TotalCopies = 3, AvailableCopies = 3, AddedDate = DateTime.Now, YearPublished = 2020, Description = "Water flow and hydraulic systems." },
                new Book { Title = "Power Plant Engineering", Author = "Justin Cruz", ISBN = "COE-0015", Category = "College of Engineering", TotalCopies = 3, AvailableCopies = 3, AddedDate = DateTime.Now, YearPublished = 2016, Description = "Power generation systems." },
                new Book { Title = "Heat Transfer", Author = "Isabel Flores", ISBN = "COE-0016", Category = "College of Engineering", TotalCopies = 4, AvailableCopies = 4, AddedDate = DateTime.Now, YearPublished = 2017, Description = "Conduction, convection, and radiation." },
                new Book { Title = "Robotics Engineering", Author = "Zachary Hill", ISBN = "COE-0017", Category = "College of Engineering", TotalCopies = 2, AvailableCopies = 2, AddedDate = DateTime.Now, YearPublished = 2021, Description = "Robot motion, sensors, and controls." },
                new Book { Title = "Transport Engineering", Author = "Pamela Lee", ISBN = "COE-0018", Category = "College of Engineering", TotalCopies = 3, AvailableCopies = 3, AddedDate = DateTime.Now, YearPublished = 2019, Description = "Traffic systems and transportation design." },
                new Book { Title = "Instrumentation and Measurement", Author = "Quinn Torres", ISBN = "COE-0019", Category = "College of Engineering", TotalCopies = 3, AvailableCopies = 3, AddedDate = DateTime.Now, YearPublished = 2016, Description = "Measurement systems and sensors." },
                new Book { Title = "Renewable Energy Systems", Author = "Harold Green", ISBN = "COE-0020", Category = "College of Engineering", TotalCopies = 2, AvailableCopies = 2, AddedDate = DateTime.Now, YearPublished = 2022, Description = "Solar, wind, and eco-friendly systems." }
            };

            Books.AddRange(coeList);


            // Business & Accountancy: 20 books
            var cbaList = new List<Book>
            {
                new Book { Title = "Principles of Accounting", Author = "Laura Bennett", ISBN = "CBA-0001", Category = "College of Business & Accountancy", TotalCopies = 4, AvailableCopies = 4, AddedDate = DateTime.Now, YearPublished = 2016, Description = "Introductory accounting principles for business students." },
                new Book { Title = "Managerial Accounting", Author = "Daniel Kim", ISBN = "CBA-0002", Category = "College of Business & Accountancy", TotalCopies = 3, AvailableCopies = 3, AddedDate = DateTime.Now, YearPublished = 2017, Description = "Techniques and practices in managerial accounting." },
                new Book { Title = "Financial Management", Author = "Evelyn Ortiz", ISBN = "CBA-0003", Category = "College of Business & Accountancy", TotalCopies = 3, AvailableCopies = 3, AddedDate = DateTime.Now, YearPublished = 2018, Description = "Foundations of corporate finance and financial decision making." },
                new Book { Title = "Marketing Management", Author = "Robert Fields", ISBN = "CBA-0004", Category = "College of Business & Accountancy", TotalCopies = 4, AvailableCopies = 4, AddedDate = DateTime.Now, YearPublished = 2019, Description = "Strategies and concepts in modern marketing." },
                new Book { Title = "Business Ethics", Author = "Sandra Liu", ISBN = "CBA-0005", Category = "College of Business & Accountancy", TotalCopies = 2, AvailableCopies = 2, AddedDate = DateTime.Now, YearPublished = 2015, Description = "Ethical issues and professional responsibility in business." },
                new Book { Title = "Corporate Law Basics", Author = "Thomas Reid", ISBN = "CBA-0006", Category = "College of Business & Accountancy", TotalCopies = 3, AvailableCopies = 3, AddedDate = DateTime.Now, YearPublished = 2014, Description = "Overview of corporate legal structures and compliance." },
                new Book { Title = "Auditing Principles", Author = "Martha Diaz", ISBN = "CBA-0007", Category = "College of Business & Accountancy", TotalCopies = 3, AvailableCopies = 3, AddedDate = DateTime.Now, YearPublished = 2016, Description = "Principles and practice of auditing." },
                new Book { Title = "Taxation for Businesses", Author = "Alan Shaw", ISBN = "CBA-0008", Category = "College of Business & Accountancy", TotalCopies = 3, AvailableCopies = 3, AddedDate = DateTime.Now, YearPublished = 2018, Description = "Business taxation fundamentals and filing procedures." },
                new Book { Title = "Investment Analysis", Author = "Nina Park", ISBN = "CBA-0009", Category = "College of Business & Accountancy", TotalCopies = 3, AvailableCopies = 3, AddedDate = DateTime.Now, YearPublished = 2020, Description = "Investment valuation and portfolio management." },
                new Book { Title = "Operations Management", Author = "Gregory Moss", ISBN = "CBA-0010", Category = "College of Business & Accountancy", TotalCopies = 4, AvailableCopies = 4, AddedDate = DateTime.Now, YearPublished = 2017, Description = "Operations strategy and process optimization." },
                new Book { Title = "Entrepreneurship Essentials", Author = "Priya Nair", ISBN = "CBA-0011", Category = "College of Business & Accountancy", TotalCopies = 3, AvailableCopies = 3, AddedDate = DateTime.Now, YearPublished = 2021, Description = "Starting and scaling a successful business." },
                new Book { Title = "Strategic Management", Author = "Oliver Grant", ISBN = "CBA-0012", Category = "College of Business & Accountancy", TotalCopies = 3, AvailableCopies = 3, AddedDate = DateTime.Now, YearPublished = 2015, Description = "Long-term strategy formulation and execution." },
                new Book { Title = "Business Analytics", Author = "Fiona Cheng", ISBN = "CBA-0013", Category = "College of Business & Accountancy", TotalCopies = 3, AvailableCopies = 3, AddedDate = DateTime.Now, YearPublished = 2019, Description = "Data-driven decision making for businesses." },
                new Book { Title = "Human Resource Management", Author = "Ethan Brooks", ISBN = "CBA-0014", Category = "College of Business & Accountancy", TotalCopies = 3, AvailableCopies = 3, AddedDate = DateTime.Now, YearPublished = 2016, Description = "HR practices, recruiting, and employee development." },
                new Book { Title = "Supply Chain Management", Author = "Hannah Kim", ISBN = "CBA-0015", Category = "College of Business & Accountancy", TotalCopies = 3, AvailableCopies = 3, AddedDate = DateTime.Now, YearPublished = 2018, Description = "Logistics, procurement, and supply chain strategy." },
                new Book { Title = "Managerial Economics", Author = "Victor Lane", ISBN = "CBA-0016", Category = "College of Business & Accountancy", TotalCopies = 2, AvailableCopies = 2, AddedDate = DateTime.Now, YearPublished = 2013, Description = "Economic concepts applied to managerial decisions." },
                new Book { Title = "Financial Reporting", Author = "Angela King", ISBN = "CBA-0017", Category = "College of Business & Accountancy", TotalCopies = 3, AvailableCopies = 3, AddedDate = DateTime.Now, YearPublished = 2014, Description = "Standards and practices in financial reporting." },
                new Book { Title = "Risk Management", Author = "Mark Rivera", ISBN = "CBA-0018", Category = "College of Business & Accountancy", TotalCopies = 2, AvailableCopies = 2, AddedDate = DateTime.Now, YearPublished = 2017, Description = "Identifying and mitigating business risks." },
                new Book { Title = "Financial Modeling", Author = "Rita Gomez", ISBN = "CBA-0019", Category = "College of Business & Accountancy", TotalCopies = 3, AvailableCopies = 3, AddedDate = DateTime.Now, YearPublished = 2020, Description = "Building financial models for valuation and forecasting." },
                new Book { Title = "Corporate Finance Cases", Author = "Brian Young", ISBN = "CBA-0020", Category = "College of Business & Accountancy", TotalCopies = 2, AvailableCopies = 2, AddedDate = DateTime.Now, YearPublished = 2012, Description = "Case studies in corporate finance decisions." }
            };

            // Combine all book lists
            books.AddRange(coedList);
            books.AddRange(coeList);
            books.AddRange(cbaList);

            // Insert all books into database
            foreach (var book in books)
            {
                using var cmd = new SqliteCommand(
                    "INSERT INTO Books (Title, Author, ISBN, Category, TotalCopies, AvailableCopies, AddedDate, YearPublished, Description) VALUES (@title, @author, @isbn, @category, @totalCopies, @availableCopies, @addedDate, @yearPublished, @description)",
                    conn);
                cmd.Parameters.AddWithValue("@title", book.Title);
                cmd.Parameters.AddWithValue("@author", book.Author);
                cmd.Parameters.AddWithValue("@isbn", book.ISBN);
                cmd.Parameters.AddWithValue("@category", book.Category);
                cmd.Parameters.AddWithValue("@totalCopies", book.TotalCopies);
                cmd.Parameters.AddWithValue("@availableCopies", book.AvailableCopies);
                cmd.Parameters.AddWithValue("@addedDate", book.AddedDate.ToString("O"));
                cmd.Parameters.AddWithValue("@yearPublished", book.YearPublished.HasValue ? (object)book.YearPublished.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@description", book.Description);
                cmd.ExecuteNonQuery();
            }
        }

        // Account persistence helpers (serialize only accounts: Students, Admins, PasswordResets)
        private class AccountStore
        {
            public List<Student> Students { get; set; } = new List<Student>();
            public List<Admin> Admins { get; set; } = new List<Admin>();
            public List<PasswordReset> PasswordResets { get; set; } = new List<PasswordReset>();

            public int StudentId { get; set; }
            public int AdminId { get; set; }
            public int PasswordResetId { get; set; }
        }

        private class PasswordReset
        {
            public int Id { get; set; }
            public int StudentId { get; set; }
            public string Code { get; set; } = string.Empty;
            public DateTime ExpiresAt { get; set; }
            public DateTime CreatedAt { get; set; }
        }
            catch
            {
                // ignore
            }
        }

        private static bool LoadAccountsFromFile_NoLock()
        {
            try
            {
                if (!File.Exists(AccountsFile)) return false;
                var json = File.ReadAllText(AccountsFile);
                var ds = JsonSerializer.Deserialize<AccountStore>(json);
                if (ds == null) return false;

                Students.Clear(); Students.AddRange(ds.Students ?? new List<Student>());
                Admins.Clear(); Admins.AddRange(ds.Admins ?? new List<Admin>());

                PasswordResets.Clear();
                if (ds.PasswordResets != null)
                {
                    foreach (var p in ds.PasswordResets)
                    {
                        PasswordResets.Add(new PasswordReset { Id = p.Id, StudentId = p.StudentId, Code = p.Code, ExpiresAt = p.ExpiresAt, CreatedAt = p.CreatedAt });
                    }
                }

                _studentId = ds.StudentId == 0 ? (Students.Any() ? Students.Max(s => s.Id) + 1 : 1) : ds.StudentId;
                _adminId = ds.AdminId == 0 ? (Admins.Any() ? Admins.Max(a => a.Id) + 1 : 1) : ds.AdminId;
                _passwordResetId = ds.PasswordResetId == 0 ? (PasswordResets.Any() ? PasswordResets.Max(p => p.Id) + 1 : 1) : ds.PasswordResetId;

                return true;
            }
            catch
            {
                return false;
            }
        }

        // Public API implementations (books, students, borrows, reservations, announcements, password resets, stats)

        public static bool AddBook(Book book)
        {
            lock (_lock)
            {
                using var conn = new SqliteConnection(GetConnectionString());
                conn.Open();

                // Check if ISBN already exists
                using var checkCmd = new SqliteCommand("SELECT COUNT(*) FROM Books WHERE ISBN = @isbn", conn);
                checkCmd.Parameters.AddWithValue("@isbn", book.ISBN);
                if (Convert.ToInt32(checkCmd.ExecuteScalar()) > 0) return false;

                using var cmd = new SqliteCommand(
                    "INSERT INTO Books (Title, Author, ISBN, Category, TotalCopies, AvailableCopies, AddedDate, YearPublished, Description) VALUES (@title, @author, @isbn, @category, @totalCopies, @availableCopies, @addedDate, @yearPublished, @description); SELECT last_insert_rowid();",
                    conn);
                cmd.Parameters.AddWithValue("@title", book.Title);
                cmd.Parameters.AddWithValue("@author", book.Author);
                cmd.Parameters.AddWithValue("@isbn", book.ISBN);
                cmd.Parameters.AddWithValue("@category", book.Category);
                cmd.Parameters.AddWithValue("@totalCopies", book.TotalCopies);
                cmd.Parameters.AddWithValue("@availableCopies", book.AvailableCopies);
                cmd.Parameters.AddWithValue("@addedDate", DateTime.Now.ToString("O"));
                cmd.Parameters.AddWithValue("@yearPublished", book.YearPublished.HasValue ? (object)book.YearPublished.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@description", book.Description);
                book.Id = Convert.ToInt32(cmd.ExecuteScalar());
            }
            OnDataChanged();
            return true;
        }

        public static List<Book> GetAllBooks()
        {
            lock (_lock)
            {
                var books = new List<Book>();
                using var conn = new SqliteConnection(GetConnectionString());
                conn.Open();
                using var cmd = new SqliteCommand("SELECT * FROM Books ORDER BY Title", conn);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    books.Add(ReadBook(reader));
                }
                return books;
            }
        }

        public static Book? GetBookById(int id)
        {
            lock (_lock)
            {
                using var conn = new SqliteConnection(GetConnectionString());
                conn.Open();
                using var cmd = new SqliteCommand("SELECT * FROM Books WHERE Id = @id", conn);
                cmd.Parameters.AddWithValue("@id", id);
                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    return ReadBook(reader);
                }
                return null;
            }
        }

        public static bool UpdateBook(Book book)
        {
            lock (_lock)
            {
                using var conn = new SqliteConnection(GetConnectionString());
                conn.Open();
                using var cmd = new SqliteCommand(
                    "UPDATE Books SET Title = @title, Author = @author, ISBN = @isbn, Category = @category, TotalCopies = @totalCopies, AvailableCopies = @availableCopies, YearPublished = @yearPublished, Description = @description WHERE Id = @id",
                    conn);
                cmd.Parameters.AddWithValue("@id", book.Id);
                cmd.Parameters.AddWithValue("@title", book.Title);
                cmd.Parameters.AddWithValue("@author", book.Author);
                cmd.Parameters.AddWithValue("@isbn", book.ISBN);
                cmd.Parameters.AddWithValue("@category", book.Category);
                cmd.Parameters.AddWithValue("@totalCopies", book.TotalCopies);
                cmd.Parameters.AddWithValue("@availableCopies", book.AvailableCopies);
                cmd.Parameters.AddWithValue("@yearPublished", book.YearPublished.HasValue ? (object)book.YearPublished.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@description", book.Description);
                var rowsAffected = cmd.ExecuteNonQuery();
                if (rowsAffected > 0)
                {
                    OnDataChanged();
                    return true;
                }
                return false;
            }
        }

        public static bool DeleteBook(int id)
        {
            lock (_lock)
            {
                using var conn = new SqliteConnection(GetConnectionString());
                conn.Open();
                using var cmd = new SqliteCommand("DELETE FROM Books WHERE Id = @id", conn);
                cmd.Parameters.AddWithValue("@id", id);
                var rowsAffected = cmd.ExecuteNonQuery();
                if (rowsAffected > 0)
                {
                    OnDataChanged();
                    return true;
                }
                return false;
            }
        }

        private static Book ReadBook(SqliteDataReader reader)
        {
            return new Book
            {
                Id = reader.GetInt32(0),
                Title = reader.GetString(1),
                Author = reader.GetString(2),
                ISBN = reader.GetString(3),
                Category = reader.GetString(4),
                TotalCopies = reader.GetInt32(5),
                AvailableCopies = reader.GetInt32(6),
                AddedDate = DateTime.Parse(reader.GetString(7)),
                YearPublished = reader.IsDBNull(8) ? null : reader.GetInt32(8),
                Description = reader.IsDBNull(9) ? string.Empty : reader.GetString(9)
            };
        }

        // Student operations
        public static bool AddStudent(Student student)
        {
            lock (_lock)
            {
                using var conn = new SqliteConnection(GetConnectionString());
                conn.Open();

                // Check if email or studentId already exists
                using var checkCmd = new SqliteCommand("SELECT COUNT(*) FROM Students WHERE Email = @email OR StudentId = @studentId", conn);
                checkCmd.Parameters.AddWithValue("@email", student.Email);
                checkCmd.Parameters.AddWithValue("@studentId", student.StudentId);
                if (Convert.ToInt32(checkCmd.ExecuteScalar()) > 0) return false;

                if (string.IsNullOrWhiteSpace(student.StudentId))
                {
                    // Get next student ID
                    using var maxCmd = new SqliteCommand("SELECT COALESCE(MAX(Id), 0) FROM Students", conn);
                    var maxId = Convert.ToInt32(maxCmd.ExecuteScalar());
                    student.StudentId = "S" + (maxId + 1).ToString("D4");
                }

                using var cmd = new SqliteCommand(
                    "INSERT INTO Students (StudentId, FullName, Email, Password, Course, Section, CreatedAt, IsActive) VALUES (@studentId, @fullName, @email, @password, @course, @section, @createdAt, @isActive); SELECT last_insert_rowid();",
                    conn);
                cmd.Parameters.AddWithValue("@studentId", student.StudentId);
                cmd.Parameters.AddWithValue("@fullName", student.FullName);
                cmd.Parameters.AddWithValue("@email", student.Email);
                cmd.Parameters.AddWithValue("@password", student.Password);
                cmd.Parameters.AddWithValue("@course", student.Course);
                cmd.Parameters.AddWithValue("@section", student.Section);
                cmd.Parameters.AddWithValue("@createdAt", DateTime.Now.ToString("O"));
                cmd.Parameters.AddWithValue("@isActive", 1);
                student.Id = Convert.ToInt32(cmd.ExecuteScalar());
                student.CreatedAt = DateTime.Now;
                student.IsActive = true;
                if (string.IsNullOrWhiteSpace(student.StudentId)) student.StudentId = "S" + student.Id.ToString("D4");
                Students.Add(student);
                SaveAccountsToFile_NoLock();
            }
            OnDataChanged();
            return true;
        }

        public static Student? GetStudentByEmailOrId(string emailOrId)
        {
            lock (_lock)
            {
                using var conn = new SqliteConnection(GetConnectionString());
                conn.Open();
                using var cmd = new SqliteCommand("SELECT * FROM Students WHERE Email = @emailOrId OR StudentId = @emailOrId", conn);
                cmd.Parameters.AddWithValue("@emailOrId", emailOrId);
                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    return ReadStudent(reader);
                }
                return null;
            }
        }

        public static Student? GetStudentById(int id)
        {
            lock (_lock)
            {
                using var conn = new SqliteConnection(GetConnectionString());
                conn.Open();
                using var cmd = new SqliteCommand("SELECT * FROM Students WHERE Id = @id", conn);
                cmd.Parameters.AddWithValue("@id", id);
                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    return ReadStudent(reader);
                }
                return null;
            }
        }

        public static List<Student> GetAllStudents()
        {
            lock (_lock)
            {
                var students = new List<Student>();
                using var conn = new SqliteConnection(GetConnectionString());
                conn.Open();
                using var cmd = new SqliteCommand("SELECT * FROM Students", conn);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    students.Add(ReadStudent(reader));
                }
                return students;
            }
        }

        public static bool UpdateStudent(Student student)
        {
            lock (_lock)
            {
                using var conn = new SqliteConnection(GetConnectionString());
                conn.Open();
                using var cmd = new SqliteCommand(
                    "UPDATE Students SET StudentId = @studentId, FullName = @fullName, Email = @email, Password = @password, Course = @course, Section = @section, IsActive = @isActive WHERE Id = @id",
                    conn);
                cmd.Parameters.AddWithValue("@id", student.Id);
                cmd.Parameters.AddWithValue("@studentId", student.StudentId);
                cmd.Parameters.AddWithValue("@fullName", student.FullName);
                cmd.Parameters.AddWithValue("@email", student.Email);
                cmd.Parameters.AddWithValue("@password", student.Password);
                cmd.Parameters.AddWithValue("@course", student.Course);
                cmd.Parameters.AddWithValue("@section", student.Section);
                cmd.Parameters.AddWithValue("@isActive", student.IsActive ? 1 : 0);
                var rowsAffected = cmd.ExecuteNonQuery();
                if (rowsAffected > 0)
                {
                    OnDataChanged();
                    return true;
                }
                return false;
            }
        }

        public static bool UpdateStudentPassword(int studentId, string newPassword)
        {
            lock (_lock)
            {
                using var conn = new SqliteConnection(GetConnectionString());
                conn.Open();
                using var cmd = new SqliteCommand("UPDATE Students SET Password = @password WHERE Id = @id", conn);
                cmd.Parameters.AddWithValue("@id", studentId);
                cmd.Parameters.AddWithValue("@password", newPassword);
                var rowsAffected = cmd.ExecuteNonQuery();
                if (rowsAffected > 0)
                {
                    OnDataChanged();
                    return true;
                }
                return false;
            }
        }

        public static Admin? GetAdminByEmail(string email)
        {
            lock (_lock)
            {
                using var conn = new SqliteConnection(GetConnectionString());
                conn.Open();
                using var cmd = new SqliteCommand("SELECT * FROM Admins WHERE Email = @email", conn);
                cmd.Parameters.AddWithValue("@email", email);
                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    return ReadAdmin(reader);
                }
                return null;
            }
        }

        private static Student ReadStudent(SqliteDataReader reader)
        {
            return new Student
            {
                Id = reader.GetInt32(0),
                StudentId = reader.GetString(1),
                FullName = reader.GetString(2),
                Email = reader.GetString(3),
                Password = reader.GetString(4),
                Course = reader.GetString(5),
                Section = reader.GetString(6),
                CreatedAt = DateTime.Parse(reader.GetString(7)),
                IsActive = reader.GetInt32(8) == 1
            };
        }

        private static Admin ReadAdmin(SqliteDataReader reader)
        {
            return new Admin
            {
                Id = reader.GetInt32(0),
                Email = reader.GetString(1),
                Password = reader.GetString(2),
                FullName = reader.GetString(3),
                CreatedAt = DateTime.Parse(reader.GetString(4))
            };
        }

        // Borrowing
        public static bool IssueBook(int studentId, int bookId, int daysToReturn = 14)
        {
            lock (_lock)
            {
                using var conn = new SqliteConnection(GetConnectionString());
                conn.Open();

                // Check if book exists and has available copies
                using var checkCmd = new SqliteCommand("SELECT AvailableCopies FROM Books WHERE Id = @id", conn);
                checkCmd.Parameters.AddWithValue("@id", bookId);
                var result = checkCmd.ExecuteScalar();
                if (result == null || Convert.ToInt32(result) <= 0) return false;

                // Decrease available copies
                using var updateCmd = new SqliteCommand("UPDATE Books SET AvailableCopies = AvailableCopies - 1 WHERE Id = @id", conn);
                updateCmd.Parameters.AddWithValue("@id", bookId);
                updateCmd.ExecuteNonQuery();

                // Insert borrow record
                using var cmd = new SqliteCommand(
                    "INSERT INTO BorrowRecords (StudentId, BookId, IssueDate, DueDate, ReturnDate, Status, FineAmount) VALUES (@studentId, @bookId, @issueDate, @dueDate, @returnDate, @status, @fineAmount); SELECT last_insert_rowid();",
                    conn);
                cmd.Parameters.AddWithValue("@studentId", studentId);
                cmd.Parameters.AddWithValue("@bookId", bookId);
                cmd.Parameters.AddWithValue("@issueDate", DateTime.Now.ToString("O"));
                cmd.Parameters.AddWithValue("@dueDate", DateTime.Now.AddDays(daysToReturn).ToString("O"));
                cmd.Parameters.AddWithValue("@returnDate", DBNull.Value);
                cmd.Parameters.AddWithValue("@status", "Issued");
                cmd.Parameters.AddWithValue("@fineAmount", DBNull.Value);
                cmd.ExecuteScalar();
            }
            OnDataChanged();
            return true;
        }

        public static bool ReturnBook(int recordId)
        {
            lock (_lock)
            {
                using var conn = new SqliteConnection(GetConnectionString());
                conn.Open();

                // Get the borrow record
                using var getCmd = new SqliteCommand("SELECT BookId, Status FROM BorrowRecords WHERE Id = @id", conn);
                getCmd.Parameters.AddWithValue("@id", recordId);
                using var reader = getCmd.ExecuteReader();
                if (!reader.Read() || reader.GetString(1) != "Issued") return false;
                var bookId = reader.GetInt32(0);
                reader.Close();

                // Update borrow record
                using var updateCmd = new SqliteCommand(
                    "UPDATE BorrowRecords SET ReturnDate = @returnDate, Status = @status WHERE Id = @id",
                    conn);
                updateCmd.Parameters.AddWithValue("@id", recordId);
                updateCmd.Parameters.AddWithValue("@returnDate", DateTime.Now.ToString("O"));
                updateCmd.Parameters.AddWithValue("@status", "Returned");
                updateCmd.ExecuteNonQuery();

                // Increase available copies
                using var bookCmd = new SqliteCommand("UPDATE Books SET AvailableCopies = AvailableCopies + 1 WHERE Id = @id", conn);
                bookCmd.Parameters.AddWithValue("@id", bookId);
                bookCmd.ExecuteNonQuery();
            }
            OnDataChanged();
            return true;
        }

        public static bool RenewBook(int recordId, int additionalDays = 14)
        {
            lock (_lock)
            {
                using var conn = new SqliteConnection(GetConnectionString());
                conn.Open();

                // Get current due date
                using var getCmd = new SqliteCommand("SELECT DueDate, Status FROM BorrowRecords WHERE Id = @id", conn);
                getCmd.Parameters.AddWithValue("@id", recordId);
                using var reader = getCmd.ExecuteReader();
                if (!reader.Read() || reader.GetString(1) != "Issued") return false;
                var currentDueDate = DateTime.Parse(reader.GetString(0));
                reader.Close();

                // Update due date
                using var cmd = new SqliteCommand("UPDATE BorrowRecords SET DueDate = @dueDate WHERE Id = @id", conn);
                cmd.Parameters.AddWithValue("@id", recordId);
                cmd.Parameters.AddWithValue("@dueDate", currentDueDate.AddDays(additionalDays).ToString("O"));
                var rowsAffected = cmd.ExecuteNonQuery();
                if (rowsAffected > 0)
                {
                    OnDataChanged();
                    return true;
                }
                return false;
            }
        }

        public static List<BorrowRecord> GetAllBorrowRecords()
        {
            lock (_lock)
            {
                var records = new List<BorrowRecord>();
                using var conn = new SqliteConnection(GetConnectionString());
                conn.Open();
                using var cmd = new SqliteCommand("SELECT * FROM BorrowRecords ORDER BY IssueDate DESC", conn);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    records.Add(ReadBorrowRecord(reader));
                }
                return records;
            }
        }

        public static List<BorrowRecord> GetBorrowRecordsByStudent(int studentId)
        {
            lock (_lock)
            {
                var records = new List<BorrowRecord>();
                using var conn = new SqliteConnection(GetConnectionString());
                conn.Open();
                using var cmd = new SqliteCommand("SELECT * FROM BorrowRecords WHERE StudentId = @studentId ORDER BY IssueDate DESC", conn);
                cmd.Parameters.AddWithValue("@studentId", studentId);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    records.Add(ReadBorrowRecord(reader));
                }
                return records;
            }
        }

        public static List<BorrowRecord> GetOverdueBooks()
        {
            lock (_lock)
            {
                var records = new List<BorrowRecord>();
                using var conn = new SqliteConnection(GetConnectionString());
                conn.Open();
                using var cmd = new SqliteCommand("SELECT * FROM BorrowRecords WHERE Status = 'Issued' AND date(DueDate) < date('now') ORDER BY DueDate", conn);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    records.Add(ReadBorrowRecord(reader));
                }
                return records;
            }
        }

        public static BorrowRecord? GetBorrowRecordById(int recordId)
        {
            lock (_lock)
            {
                using var conn = new SqliteConnection(GetConnectionString());
                conn.Open();
                using var cmd = new SqliteCommand("SELECT * FROM BorrowRecords WHERE Id = @id", conn);
                cmd.Parameters.AddWithValue("@id", recordId);
                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    return ReadBorrowRecord(reader);
                }
                return null;
            }
        }

        private static BorrowRecord ReadBorrowRecord(SqliteDataReader reader)
        {
            return new BorrowRecord
            {
                Id = reader.GetInt32(0),
                StudentId = reader.GetInt32(1),
                BookId = reader.GetInt32(2),
                IssueDate = DateTime.Parse(reader.GetString(3)),
                DueDate = DateTime.Parse(reader.GetString(4)),
                ReturnDate = reader.IsDBNull(5) ? null : DateTime.Parse(reader.GetString(5)),
                Status = reader.GetString(6),
                FineAmount = reader.IsDBNull(7) ? null : reader.GetDecimal(7)
            };
        }

        // Reservations
        public static bool CreateReservation(int studentId, int bookId)
        {
            lock (_lock)
            {
                using var conn = new SqliteConnection(GetConnectionString());
                conn.Open();

                // Check if reservation already exists
                using var checkCmd = new SqliteCommand(
                    "SELECT COUNT(*) FROM Reservations WHERE StudentId = @studentId AND BookId = @bookId AND Status NOT IN ('Cancelled', 'Completed')",
                    conn);
                checkCmd.Parameters.AddWithValue("@studentId", studentId);
                checkCmd.Parameters.AddWithValue("@bookId", bookId);
                if (Convert.ToInt32(checkCmd.ExecuteScalar()) > 0) return false;

                // Check if book is available
                using var bookCmd = new SqliteCommand("SELECT AvailableCopies FROM Books WHERE Id = @id", conn);
                bookCmd.Parameters.AddWithValue("@id", bookId);
                var availableCopies = Convert.ToInt32(bookCmd.ExecuteScalar());
                var status = availableCopies > 0 ? "Ready" : "Pending";

                using var cmd = new SqliteCommand(
                    "INSERT INTO Reservations (StudentId, BookId, ReservationDate, HoldUntilDate, Status) VALUES (@studentId, @bookId, @reservationDate, @holdUntilDate, @status); SELECT last_insert_rowid();",
                    conn);
                cmd.Parameters.AddWithValue("@studentId", studentId);
                cmd.Parameters.AddWithValue("@bookId", bookId);
                cmd.Parameters.AddWithValue("@reservationDate", DateTime.Now.ToString("O"));
                cmd.Parameters.AddWithValue("@holdUntilDate", DateTime.Now.AddDays(7).ToString("O"));
                cmd.Parameters.AddWithValue("@status", status);
                cmd.ExecuteScalar();
            }
            OnDataChanged();
            return true;
        }

        public static List<Reservation> GetReservationsByStudent(int studentId)
        {
            lock (_lock)
            {
                var reservations = new List<Reservation>();
                using var conn = new SqliteConnection(GetConnectionString());
                conn.Open();
                using var cmd = new SqliteCommand(
                    "SELECT * FROM Reservations WHERE StudentId = @studentId AND Status NOT IN ('Cancelled', 'Completed') ORDER BY ReservationDate DESC",
                    conn);
                cmd.Parameters.AddWithValue("@studentId", studentId);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    reservations.Add(ReadReservation(reader));
                }
                return reservations;
            }
        }

        public static bool ReservationExists(int studentId, int bookId)
        {
            lock (_lock)
            {
                using var conn = new SqliteConnection(GetConnectionString());
                conn.Open();
                using var cmd = new SqliteCommand(
                    "SELECT COUNT(*) FROM Reservations WHERE StudentId = @studentId AND BookId = @bookId AND Status NOT IN ('Cancelled', 'Completed')",
                    conn);
                cmd.Parameters.AddWithValue("@studentId", studentId);
                cmd.Parameters.AddWithValue("@bookId", bookId);
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }

        public static bool CancelReservation(int reservationId)
        {
            lock (_lock)
            {
                using var conn = new SqliteConnection(GetConnectionString());
                conn.Open();
                using var cmd = new SqliteCommand("UPDATE Reservations SET Status = 'Cancelled' WHERE Id = @id", conn);
                cmd.Parameters.AddWithValue("@id", reservationId);
                var rowsAffected = cmd.ExecuteNonQuery();
                if (rowsAffected > 0)
                {
                    OnDataChanged();
                    return true;
                }
                return false;
            }
        }

        public static bool PickUpReservation(int reservationId)
        {
            lock (_lock)
            {
                using var conn = new SqliteConnection(GetConnectionString());
                conn.Open();

                // Get reservation
                using var getCmd = new SqliteCommand("SELECT StudentId, BookId, Status FROM Reservations WHERE Id = @id", conn);
                getCmd.Parameters.AddWithValue("@id", reservationId);
                using var reader = getCmd.ExecuteReader();
                if (!reader.Read() || reader.GetString(2) != "Ready") return false;
                var studentId = reader.GetInt32(0);
                var bookId = reader.GetInt32(1);
                reader.Close();

                // Issue the book (inline logic to avoid nested lock)
                using var checkCmd = new SqliteCommand("SELECT AvailableCopies FROM Books WHERE Id = @id", conn);
                checkCmd.Parameters.AddWithValue("@id", bookId);
                var result = checkCmd.ExecuteScalar();
                if (result == null || Convert.ToInt32(result) <= 0) return false;

                using var updateCmd = new SqliteCommand("UPDATE Books SET AvailableCopies = AvailableCopies - 1 WHERE Id = @id", conn);
                updateCmd.Parameters.AddWithValue("@id", bookId);
                updateCmd.ExecuteNonQuery();

                using var borrowCmd = new SqliteCommand(
                    "INSERT INTO BorrowRecords (StudentId, BookId, IssueDate, DueDate, ReturnDate, Status, FineAmount) VALUES (@studentId, @bookId, @issueDate, @dueDate, @returnDate, @status, @fineAmount)",
                    conn);
                borrowCmd.Parameters.AddWithValue("@studentId", studentId);
                borrowCmd.Parameters.AddWithValue("@bookId", bookId);
                borrowCmd.Parameters.AddWithValue("@issueDate", DateTime.Now.ToString("O"));
                borrowCmd.Parameters.AddWithValue("@dueDate", DateTime.Now.AddDays(14).ToString("O"));
                borrowCmd.Parameters.AddWithValue("@returnDate", DBNull.Value);
                borrowCmd.Parameters.AddWithValue("@status", "Issued");
                borrowCmd.Parameters.AddWithValue("@fineAmount", DBNull.Value);
                borrowCmd.ExecuteNonQuery();

                // Update reservation status
                using var resCmd = new SqliteCommand("UPDATE Reservations SET Status = 'Completed' WHERE Id = @id", conn);
                resCmd.Parameters.AddWithValue("@id", reservationId);
                resCmd.ExecuteNonQuery();
            }
            OnDataChanged();
            return true;
        }

        private static Reservation ReadReservation(SqliteDataReader reader)
        {
            return new Reservation
            {
                Id = reader.GetInt32(0),
                StudentId = reader.GetInt32(1),
                BookId = reader.GetInt32(2),
                ReservationDate = DateTime.Parse(reader.GetString(3)),
                HoldUntilDate = reader.IsDBNull(4) ? null : DateTime.Parse(reader.GetString(4)),
                Status = reader.GetString(5)
            };
        }

        // Announcements
        public static bool AddAnnouncement(Announcement announcement)
        {
            lock (_lock)
            {
                using var conn = new SqliteConnection(GetConnectionString());
                conn.Open();
                using var cmd = new SqliteCommand(
                    "INSERT INTO Announcements (Title, Content, CreatedDate, IsActive) VALUES (@title, @content, @createdDate, @isActive); SELECT last_insert_rowid();",
                    conn);
                cmd.Parameters.AddWithValue("@title", announcement.Title);
                cmd.Parameters.AddWithValue("@content", announcement.Content);
                cmd.Parameters.AddWithValue("@createdDate", DateTime.Now.ToString("O"));
                cmd.Parameters.AddWithValue("@isActive", announcement.IsActive ? 1 : 0);
                announcement.Id = Convert.ToInt32(cmd.ExecuteScalar());
                announcement.CreatedDate = DateTime.Now;
                Announcements.Add(announcement);
            }
            OnDataChanged();
            return true;
        }

        public static List<Announcement> GetActiveAnnouncements()
        {
            lock (_lock)
            {
                var announcements = new List<Announcement>();
                using var conn = new SqliteConnection(GetConnectionString());
                conn.Open();
                using var cmd = new SqliteCommand("SELECT * FROM Announcements WHERE IsActive = 1 ORDER BY CreatedDate DESC", conn);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    announcements.Add(ReadAnnouncement(reader));
                }
                return announcements;
            }
        }

        public static List<Announcement> GetAllAnnouncements()
        {
            lock (_lock)
            {
                var announcements = new List<Announcement>();
                using var conn = new SqliteConnection(GetConnectionString());
                conn.Open();
                using var cmd = new SqliteCommand("SELECT * FROM Announcements ORDER BY CreatedDate DESC", conn);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    announcements.Add(ReadAnnouncement(reader));
                }
                return announcements;
            }
        }

        public static bool DeleteAnnouncement(int id)
        {
            lock (_lock)
            {
                using var conn = new SqliteConnection(GetConnectionString());
                conn.Open();
                using var cmd = new SqliteCommand("DELETE FROM Announcements WHERE Id = @id", conn);
                cmd.Parameters.AddWithValue("@id", id);
                var rowsAffected = cmd.ExecuteNonQuery();
                if (rowsAffected > 0)
                {
                    OnDataChanged();
                    return true;
                }
                return false;
            }
        }

        private static Announcement ReadAnnouncement(SqliteDataReader reader)
        {
            return new Announcement
            {
                Id = reader.GetInt32(0),
                Title = reader.GetString(1),
                Content = reader.GetString(2),
                CreatedDate = DateTime.Parse(reader.GetString(3)),
                IsActive = reader.GetInt32(4) == 1
            };
        }

        // Password resets
        public static bool CreatePasswordReset(int studentId, string code, DateTime expiresAt)
        {
            lock (_lock)
            {
                using var conn = new SqliteConnection(GetConnectionString());
                conn.Open();
                using var cmd = new SqliteCommand(
                    "INSERT INTO PasswordResets (StudentId, Code, ExpiresAt, CreatedAt) VALUES (@studentId, @code, @expiresAt, @createdAt); SELECT last_insert_rowid();",
                    conn);
                cmd.Parameters.AddWithValue("@studentId", studentId);
                cmd.Parameters.AddWithValue("@code", code);
                cmd.Parameters.AddWithValue("@expiresAt", expiresAt.ToString("O"));
                cmd.Parameters.AddWithValue("@createdAt", DateTime.Now.ToString("O"));
                cmd.ExecuteScalar();
            }
            return true;
        }

        public static (int Id, int StudentId, string Code, DateTime ExpiresAt)? GetPasswordResetByStudentAndCode(int studentId, string code)
        {
            lock (_lock)
            {
                using var conn = new SqliteConnection(GetConnectionString());
                conn.Open();
                using var cmd = new SqliteCommand("SELECT Id, StudentId, Code, ExpiresAt FROM PasswordResets WHERE StudentId = @studentId AND Code = @code", conn);
                cmd.Parameters.AddWithValue("@studentId", studentId);
                cmd.Parameters.AddWithValue("@code", code);
                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    return (reader.GetInt32(0), reader.GetInt32(1), reader.GetString(2), DateTime.Parse(reader.GetString(3)));
                }
                return null;
            }
        }

        public static bool DeletePasswordReset(int id)
        {
            lock (_lock)
            {
                using var conn = new SqliteConnection(GetConnectionString());
                conn.Open();
                using var cmd = new SqliteCommand("DELETE FROM PasswordResets WHERE Id = @id", conn);
                cmd.Parameters.AddWithValue("@id", id);
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        // Helpers
        public static int GetTotalBooks()
        {
            lock (_lock)
            {
                using var conn = new SqliteConnection(GetConnectionString());
                conn.Open();
                using var cmd = new SqliteCommand("SELECT COUNT(*) FROM Books", conn);
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        public static int GetTotalStudents()
        {
            lock (_lock)
            {
                using var conn = new SqliteConnection(GetConnectionString());
                conn.Open();
                using var cmd = new SqliteCommand("SELECT COUNT(*) FROM Students WHERE IsActive = 1", conn);
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        public static int GetIssuedBooksCount()
        {
            lock (_lock)
            {
                using var conn = new SqliteConnection(GetConnectionString());
                conn.Open();
                using var cmd = new SqliteCommand("SELECT COUNT(*) FROM BorrowRecords WHERE Status = 'Issued'", conn);
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        public static int GetOverdueBooksCount()
        {
            lock (_lock)
            {
                using var conn = new SqliteConnection(GetConnectionString());
                conn.Open();
                using var cmd = new SqliteCommand("SELECT COUNT(*) FROM BorrowRecords WHERE Status = 'Issued' AND date(DueDate) < date('now')", conn);
                return Convert.ToInt32(cmd.ExecuteScalar());
            }

        public static BorrowRecord? GetBorrowRecordById(int recordId)
        {
            lock (_lock) { return BorrowRecords.FirstOrDefault(r => r.Id == recordId); }
        }

        public static Student? GetStudentByEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return null;
            lock (_lock)
            {
                using var conn = new SqliteConnection(GetConnectionString());
                conn.Open();
                using var cmd = new SqliteCommand("SELECT * FROM Students WHERE Email = @email", conn);
                cmd.Parameters.AddWithValue("@email", email);
                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    return ReadStudent(reader);
                }
                return null;
            }
        }
    }
}