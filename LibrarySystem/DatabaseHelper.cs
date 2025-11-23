using LibrarySystem.Models;
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

        private static readonly List<Book> Books = new List<Book>();
        private static readonly List<Student> Students = new List<Student>();
        private static readonly List<Admin> Admins = new List<Admin>();
        private static readonly List<BorrowRecord> BorrowRecords = new List<BorrowRecord>();
        private static readonly List<Reservation> Reservations = new List<Reservation>();
        private static readonly List<Announcement> Announcements = new List<Announcement>();
        private static readonly List<PasswordReset> PasswordResets = new List<PasswordReset>();

        private static int _bookId = 1;
        private static int _studentId = 1;
        private static int _adminId = 1;
        private static int _borrowId = 1;
        private static int _reservationId = 1;
        private static int _announcementId = 1;
        private static int _passwordResetId = 1;

        private static readonly string DataFolder;
        private static readonly string AccountsFile;

        static DatabaseHelper()
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            DataFolder = Path.Combine(appData, "LibrarySystem");
            if (!Directory.Exists(DataFolder)) Directory.CreateDirectory(DataFolder);
            AccountsFile = Path.Combine(DataFolder, "accounts.json");
        }

        private static void OnDataChanged()
        {
            try
            {
                DataChanged?.Invoke();
            }
            catch { }
        }

        // Public initialization
        // Load persisted accounts (students/admins/password resets) from JSON.
        // Always seed books in memory (books are not persisted anymore).
        public static void InitializeDatabase()
        {
            lock (_lock)
            {
                var accountsLoaded = LoadAccountsFromFile_NoLock();
                if (!accountsLoaded)
                {
                    SeedAccountDefaults_NoLock();
                    SaveAccountsToFile_NoLock();
                }

                // Always seed books (do not clear or overwrite accounts)
                SeedBooksDefaults_NoLock();
            }

            OnDataChanged();
        }

        // Seed only accounts (called when no accounts.json exists)
        private static void SeedAccountDefaults_NoLock()
        {
            Admins.Clear();
            Students.Clear();
            PasswordResets.Clear();

            _studentId = 1; _adminId = 1; _passwordResetId = 1;

            Admins.Add(new Admin { Id = _adminId++, Email = "libraryadmin@gmail.com", Password = "admin123", FullName = "Library Administrator", CreatedAt = DateTime.Now });

            Students.Add(new Student { Id = _studentId++, StudentId = "S1001", FullName = "Test Student", Email = "student@example.com", Password = "pass", Course = "Computer Science", Section = "A", CreatedAt = DateTime.Now, IsActive = true });
        }

        // Seed only books (always run on startup)
        private static void SeedBooksDefaults_NoLock()
        {
            Books.Clear();

            _bookId = 1;

            var ccsList = new List<Book>
            {
                new Book { Id = _bookId++, Title = "Introduction to Programming", Author = "James Foster", ISBN = "CCS-0001", Category = "College of Computer Studies", TotalCopies = 4, AvailableCopies = 4, AddedDate = DateTime.Now, YearPublished = 2020, Description = "Basic programming concepts using Python." },
                new Book { Id = _bookId++, Title = "Data Structures and Algorithms", Author = "Maria Santos", ISBN = "CCS-0002", Category = "College of Computer Studies", TotalCopies = 3, AvailableCopies = 3, AddedDate = DateTime.Now, YearPublished = 2019, Description = "Core data structures and algorithmic problem solving." },
                new Book { Id = _bookId++, Title = "Database Management Systems", Author = "Eric Tan", ISBN = "CCS-0003", Category = "College of Computer Studies", TotalCopies = 4, AvailableCopies = 4, AddedDate = DateTime.Now, YearPublished = 2018, Description = "Relational databases, SQL, and database design principles." },
                new Book { Id = _bookId++, Title = "Web Development Fundamentals", Author = "Lara Kim", ISBN = "CCS-0004", Category = "College of Computer Studies", TotalCopies = 3, AvailableCopies = 3, AddedDate = DateTime.Now, YearPublished = 2021, Description = "HTML, CSS, JavaScript, and responsive design." },
                new Book { Id = _bookId++, Title = "Object-Oriented Programming", Author = "Daniel Cruz", ISBN = "CCS-0005", Category = "College of Computer Studies", TotalCopies = 4, AvailableCopies = 4, AddedDate = DateTime.Now, YearPublished = 2017, Description = "Concepts and implementation of OOP in Java." },
                new Book { Id = _bookId++, Title = "Software Engineering Principles", Author = "Hannah Reed", ISBN = "CCS-0006", Category = "College of Computer Studies", TotalCopies = 3, AvailableCopies = 3, AddedDate = DateTime.Now, YearPublished = 2019, Description = "Software development methodologies and project management." },
                new Book { Id = _bookId++, Title = "Computer Networks", Author = "Victor Ramos", ISBN = "CCS-0007", Category = "College of Computer Studies", TotalCopies = 4, AvailableCopies = 4, AddedDate = DateTime.Now, YearPublished = 2016, Description = "Network concepts, OSI model, and routing." },
                new Book { Id = _bookId++, Title = "Cybersecurity Essentials", Author = "Ella Parker", ISBN = "CCS-0008", Category = "College of Computer Studies", TotalCopies = 3, AvailableCopies = 3, AddedDate = DateTime.Now, YearPublished = 2020, Description = "Information security concepts and threat mitigation." },
                new Book { Id = _bookId++, Title = "Human-Computer Interaction", Author = "Ian Torres", ISBN = "CCS-0009", Category = "College of Computer Studies", TotalCopies = 3, AvailableCopies = 3, AddedDate = DateTime.Now, YearPublished = 2015, Description = "User-centered interface design and usability." },
                new Book { Id = _bookId++, Title = "Machine Learning Basics", Author = "Olivia Grant", ISBN = "CCS-0010", Category = "College of Computer Studies", TotalCopies = 3, AvailableCopies = 3, AddedDate = DateTime.Now, YearPublished = 2022, Description = "Introduction to machine learning and AI." },
                new Book { Id = _bookId++, Title = "Mobile App Development", Author = "Chris Howard", ISBN = "CCS-0011", Category = "College of Computer Studies", TotalCopies = 4, AvailableCopies = 4, AddedDate = DateTime.Now, YearPublished = 2021, Description = "Building Android and iOS applications." },
                new Book { Id = _bookId++, Title = "Operating Systems Concepts", Author = "Faith Navarro", ISBN = "CCS-0012", Category = "College of Computer Studies", TotalCopies = 3, AvailableCopies = 3, AddedDate = DateTime.Now, YearPublished = 2018, Description = "Fundamentals of operating systems and resource management." },
                new Book { Id = _bookId++, Title = "Systems Analysis and Design", Author = "Kevin Price", ISBN = "CCS-0013", Category = "College of Computer Studies", TotalCopies = 3, AvailableCopies = 3, AddedDate = DateTime.Now, YearPublished = 2016, Description = "Modeling and designing information systems." },
                new Book { Id = _bookId++, Title = "Artificial Intelligence Concepts", Author = "Jenny Cruz", ISBN = "CCS-0014", Category = "College of Computer Studies", TotalCopies = 3, AvailableCopies = 3, AddedDate = DateTime.Now, YearPublished = 2019, Description = "AI applications and intelligent systems." },
                new Book { Id = _bookId++, Title = "Cloud Computing Fundamentals", Author = "Patrick Lane", ISBN = "CCS-0015", Category = "College of Computer Studies", TotalCopies = 4, AvailableCopies = 4, AddedDate = DateTime.Now, YearPublished = 2020, Description = "Cloud architecture and virtualization." },
                new Book { Id = _bookId++, Title = "Computer Architecture", Author = "Derek Lee", ISBN = "CCS-0016", Category = "College of Computer Studies", TotalCopies = 3, AvailableCopies = 3, AddedDate = DateTime.Now, YearPublished = 2017, Description = "Hardware organization and CPU design." },
                new Book { Id = _bookId++, Title = "Parallel Computing", Author = "Nina Wells", ISBN = "CCS-0017", Category = "College of Computer Studies", TotalCopies = 2, AvailableCopies = 2, AddedDate = DateTime.Now, YearPublished = 2014, Description = "Parallel algorithms and distributed systems." },
                new Book { Id = _bookId++, Title = "Data Mining Techniques", Author = "Samuel Ortiz", ISBN = "CCS-0018", Category = "College of Computer Studies", TotalCopies = 3, AvailableCopies = 3, AddedDate = DateTime.Now, YearPublished = 2021, Description = "Extracting insights from large datasets." },
                new Book { Id = _bookId++, Title = "Game Development Essentials", Author = "Troy Mendoza", ISBN = "CCS-0019", Category = "College of Computer Studies", TotalCopies = 3, AvailableCopies = 3, AddedDate = DateTime.Now, YearPublished = 2016, Description = "Game design principles using Unity." },
                new Book { Id = _bookId++, Title = "Computer Forensics", Author = "Zara Mitchell", ISBN = "CCS-0020", Category = "College of Computer Studies", TotalCopies = 2, AvailableCopies = 2, AddedDate = DateTime.Now, YearPublished = 2018, Description = "Digital evidence recovery and cybercrime investigation." }
            };

            Books.AddRange(ccsList);


            // Education: 20 books
            var coedList = new List<Book>
            {
                new Book { Id = _bookId++, Title = "Foundations of Education", Author = "Emily Davis", ISBN = "COED-0001", Category = "College of Education", TotalCopies = 4, AvailableCopies = 4, AddedDate = DateTime.Now, YearPublished = 2018, Description = "Overview of educational systems and teaching foundations." },
                new Book { Id = _bookId++, Title = "Child and Adolescent Development", Author = "Mark Wilson", ISBN = "COED-0002", Category = "College of Education", TotalCopies = 3, AvailableCopies = 3, AddedDate = DateTime.Now, YearPublished = 2017, Description = "Developmental stages of learners." },
                new Book { Id = _bookId++, Title = "Curriculum Development", Author = "Anna Rivera", ISBN = "COED-0003", Category = "College of Education", TotalCopies = 4, AvailableCopies = 4, AddedDate = DateTime.Now, YearPublished = 2019, Description = "Designing and evaluating curriculum." },
                new Book { Id = _bookId++, Title = "Assessment in Learning", Author = "Kenneth Young", ISBN = "COED-0004", Category = "College of Education", TotalCopies = 3, AvailableCopies = 3, AddedDate = DateTime.Now, YearPublished = 2016, Description = "Assessment tools and measurement of learning." },
                new Book { Id = _bookId++, Title = "Educational Psychology", Author = "Sarah Gomez", ISBN = "COED-0005", Category = "College of Education", TotalCopies = 4, AvailableCopies = 4, AddedDate = DateTime.Now, YearPublished = 2020, Description = "Psychology principles in education." },
                new Book { Id = _bookId++, Title = "Teaching Strategies and Methods", Author = "Liam Carter", ISBN = "COED-0006", Category = "College of Education", TotalCopies = 3, AvailableCopies = 3, AddedDate = DateTime.Now, YearPublished = 2015, Description = "Effective teaching techniques." },
                new Book { Id = _bookId++, Title = "Inclusive Education", Author = "Patricia Lim", ISBN = "COED-0007", Category = "College of Education", TotalCopies = 3, AvailableCopies = 3, AddedDate = DateTime.Now, YearPublished = 2018, Description = "Teaching diverse learners." },
                new Book { Id = _bookId++, Title = "Educational Technology", Author = "Nathan Brooks", ISBN = "COED-0008", Category = "College of Education", TotalCopies = 4, AvailableCopies = 4, AddedDate = DateTime.Now, YearPublished = 2021, Description = "Using technology in teaching." },
                new Book { Id = _bookId++, Title = "Classroom Management", Author = "Nicole Adams", ISBN = "COED-0009", Category = "College of Education", TotalCopies = 3, AvailableCopies = 3, AddedDate = DateTime.Now, YearPublished = 2016, Description = "Managing classroom behavior." },
                new Book { Id = _bookId++, Title = "Educational Research Methods", Author = "Oscar Lee", ISBN = "COED-0010", Category = "College of Education", TotalCopies = 3, AvailableCopies = 3, AddedDate = DateTime.Now, YearPublished = 2019, Description = "Research fundamentals for education." },
                new Book { Id = _bookId++, Title = "Literacy Development", Author = "Grace Hunt", ISBN = "COED-0011", Category = "College of Education", TotalCopies = 4, AvailableCopies = 4, AddedDate = DateTime.Now, YearPublished = 2018, Description = "Reading and writing development." },
                new Book { Id = _bookId++, Title = "Educational Leadership", Author = "Henry Cruz", ISBN = "COED-0012", Category = "College of Education", TotalCopies = 3, AvailableCopies = 3, AddedDate = DateTime.Now, YearPublished = 2017, Description = "Leadership skills for educators." },
                new Book { Id = _bookId++, Title = "Philosophy of Education", Author = "Elaine West", ISBN = "COED-0013", Category = "College of Education", TotalCopies = 3, AvailableCopies = 3, AddedDate = DateTime.Now, YearPublished = 2015, Description = "Philosophical foundations of teaching." },
                new Book { Id = _bookId++, Title = "Science Teaching Methods", Author = "Albert Cruz", ISBN = "COED-0014", Category = "College of Education", TotalCopies = 3, AvailableCopies = 3, AddedDate = DateTime.Now, YearPublished = 2020, Description = "Teaching science with engaging strategies." },
                new Book { Id = _bookId++, Title = "Mathematics for Educators", Author = "Donna Roberts", ISBN = "COED-0015", Category = "College of Education", TotalCopies = 4, AvailableCopies = 4, AddedDate = DateTime.Now, YearPublished = 2019, Description = "Math concepts and teaching approaches." },
                new Book { Id = _bookId++, Title = "Educational Assessment Tools", Author = "Isaac Brown", ISBN = "COED-0016", Category = "College of Education", TotalCopies = 2, AvailableCopies = 2, AddedDate = DateTime.Now, YearPublished = 2014, Description = "Tools for evaluating student performance." },
                new Book { Id = _bookId++, Title = "Guidance and Counseling", Author = "Theresa Lane", ISBN = "COED-0017", Category = "College of Education", TotalCopies = 3, AvailableCopies = 3, AddedDate = DateTime.Now, YearPublished = 2016, Description = "Counseling approaches for learners." },
                new Book { Id = _bookId++, Title = "History of Education", Author = "Marvin Ellis", ISBN = "COED-0018", Category = "College of Education", TotalCopies = 3, AvailableCopies = 3, AddedDate = DateTime.Now, YearPublished = 2013, Description = "Evolution of educational practices." },
                new Book { Id = _bookId++, Title = "Technology for Teaching Literacy", Author = "Faye Kim", ISBN = "COED-0019", Category = "College of Education", TotalCopies = 3, AvailableCopies = 3, AddedDate = DateTime.Now, YearPublished = 2021, Description = "Digital tools for literacy education." },
                new Book { Id = _bookId++, Title = "Assessment and Evaluation", Author = "Philip Yang", ISBN = "COED-0020", Category = "College of Education", TotalCopies = 3, AvailableCopies = 3, AddedDate = DateTime.Now, YearPublished = 2017, Description = "Evaluating student learning outcomes." }
            };

            Books.AddRange(coedList);


            // Engineering: 20 books
            var coeList = new List<Book>
            {
                new Book { Id = _bookId++, Title = "Engineering Mechanics", Author = "David Thornton", ISBN = "COE-0001", Category = "College of Engineering", TotalCopies = 4, AvailableCopies = 4, AddedDate = DateTime.Now, YearPublished = 2016, Description = "Statics and dynamics principles." },
                new Book { Id = _bookId++, Title = "Thermodynamics", Author = "Sophia Vale", ISBN = "COE-0002", Category = "College of Engineering", TotalCopies = 3, AvailableCopies = 3, AddedDate = DateTime.Now, YearPublished = 2017, Description = "Energy systems and thermodynamic laws." },
                new Book { Id = _bookId++, Title = "Engineering Mathematics", Author = "John Perez", ISBN = "COE-0003", Category = "College of Engineering", TotalCopies = 4, AvailableCopies = 4, AddedDate = DateTime.Now, YearPublished = 2019, Description = "Math concepts essential for engineering." },
                new Book { Id = _bookId++, Title = "Electrical Circuits", Author = "Kathryn Lee", ISBN = "COE-0004", Category = "College of Engineering", TotalCopies = 3, AvailableCopies = 3, AddedDate = DateTime.Now, YearPublished = 2018, Description = "Circuits and electrical analysis." },
                new Book { Id = _bookId++, Title = "Fluid Mechanics", Author = "Anthony Price", ISBN = "COE-0005", Category = "College of Engineering", TotalCopies = 4, AvailableCopies = 4, AddedDate = DateTime.Now, YearPublished = 2015, Description = "Study of fluid behavior and forces." },
                new Book { Id = _bookId++, Title = "Materials Science", Author = "Grace Choi", ISBN = "COE-0006", Category = "College of Engineering", TotalCopies = 3, AvailableCopies = 3, AddedDate = DateTime.Now, YearPublished = 2020, Description = "Properties of engineering materials." },
                new Book { Id = _bookId++, Title = "Computer-Aided Design (CAD)", Author = "Michael Cruz", ISBN = "COE-0007", Category = "College of Engineering", TotalCopies = 4, AvailableCopies = 4, AddedDate = DateTime.Now, YearPublished = 2021, Description = "Modern engineering design tools." },
                new Book { Id = _bookId++, Title = "Engineering Economics", Author = "Linda Garcia", ISBN = "COE-0008", Category = "College of Engineering", TotalCopies = 3, AvailableCopies = 3, AddedDate = DateTime.Now, YearPublished = 2016, Description = "Economic decision making for engineers." },
                new Book { Id = _bookId++, Title = "Control Systems Engineering", Author = "Peter Ramos", ISBN = "COE-0009", Category = "College of Engineering", TotalCopies = 3, AvailableCopies = 3, AddedDate = DateTime.Now, YearPublished = 2018, Description = "Stability and feedback systems." },
                new Book { Id = _bookId++, Title = "Mechanics of Materials", Author = "Rachel Adams", ISBN = "COE-0010", Category = "College of Engineering", TotalCopies = 3, AvailableCopies = 3, AddedDate = DateTime.Now, YearPublished = 2017, Description = "Stress, strain, and deformation." },
                new Book { Id = _bookId++, Title = "Structural Analysis", Author = "Kyle Sanders", ISBN = "COE-0011", Category = "College of Engineering", TotalCopies = 4, AvailableCopies = 4, AddedDate = DateTime.Now, YearPublished = 2018, Description = "Analyzing structural integrity." },
                new Book { Id = _bookId++, Title = "Environmental Engineering", Author = "Tina Morales", ISBN = "COE-0012", Category = "College of Engineering", TotalCopies = 3, AvailableCopies = 3, AddedDate = DateTime.Now, YearPublished = 2019, Description = "Environmental systems and sustainability." },
                new Book { Id = _bookId++, Title = "Engineering Design Process", Author = "Ryan Ford", ISBN = "COE-0013", Category = "College of Engineering", TotalCopies = 3, AvailableCopies = 3, AddedDate = DateTime.Now, YearPublished = 2015, Description = "Design methodologies in engineering." },
                new Book { Id = _bookId++, Title = "Hydraulics Engineering", Author = "Samantha King", ISBN = "COE-0014", Category = "College of Engineering", TotalCopies = 3, AvailableCopies = 3, AddedDate = DateTime.Now, YearPublished = 2020, Description = "Water flow and hydraulic systems." },
                new Book { Id = _bookId++, Title = "Power Plant Engineering", Author = "Justin Cruz", ISBN = "COE-0015", Category = "College of Engineering", TotalCopies = 3, AvailableCopies = 3, AddedDate = DateTime.Now, YearPublished = 2016, Description = "Power generation systems." },
                new Book { Id = _bookId++, Title = "Heat Transfer", Author = "Isabel Flores", ISBN = "COE-0016", Category = "College of Engineering", TotalCopies = 4, AvailableCopies = 4, AddedDate = DateTime.Now, YearPublished = 2017, Description = "Conduction, convection, and radiation." },
                new Book { Id = _bookId++, Title = "Robotics Engineering", Author = "Zachary Hill", ISBN = "COE-0017", Category = "College of Engineering", TotalCopies = 2, AvailableCopies = 2, AddedDate = DateTime.Now, YearPublished = 2021, Description = "Robot motion, sensors, and controls." },
                new Book { Id = _bookId++, Title = "Transport Engineering", Author = "Pamela Lee", ISBN = "COE-0018", Category = "College of Engineering", TotalCopies = 3, AvailableCopies = 3, AddedDate = DateTime.Now, YearPublished = 2019, Description = "Traffic systems and transportation design." },
                new Book { Id = _bookId++, Title = "Instrumentation and Measurement", Author = "Quinn Torres", ISBN = "COE-0019", Category = "College of Engineering", TotalCopies = 3, AvailableCopies = 3, AddedDate = DateTime.Now, YearPublished = 2016, Description = "Measurement systems and sensors." },
                new Book { Id = _bookId++, Title = "Renewable Energy Systems", Author = "Harold Green", ISBN = "COE-0020", Category = "College of Engineering", TotalCopies = 2, AvailableCopies = 2, AddedDate = DateTime.Now, YearPublished = 2022, Description = "Solar, wind, and eco-friendly systems." }
            };

            Books.AddRange(coeList);


            // Business & Accountancy: 20 books
            var cbaList = new List<Book>
            {
                new Book { Id = _bookId++, Title = "Principles of Accounting", Author = "Laura Bennett", ISBN = "CBA-0001", Category = "College of Business & Accountancy", TotalCopies = 4, AvailableCopies = 4, AddedDate = DateTime.Now, YearPublished = 2016, Description = "Introductory accounting principles for business students." },
                new Book { Id = _bookId++, Title = "Managerial Accounting", Author = "Daniel Kim", ISBN = "CBA-0002", Category = "College of Business & Accountancy", TotalCopies = 3, AvailableCopies = 3, AddedDate = DateTime.Now, YearPublished = 2017, Description = "Techniques and practices in managerial accounting." },
                new Book { Id = _bookId++, Title = "Financial Management", Author = "Evelyn Ortiz", ISBN = "CBA-0003", Category = "College of Business & Accountancy", TotalCopies = 3, AvailableCopies = 3, AddedDate = DateTime.Now, YearPublished = 2018, Description = "Foundations of corporate finance and financial decision making." },
                new Book { Id = _bookId++, Title = "Marketing Management", Author = "Robert Fields", ISBN = "CBA-0004", Category = "College of Business & Accountancy", TotalCopies = 4, AvailableCopies = 4, AddedDate = DateTime.Now, YearPublished = 2019, Description = "Strategies and concepts in modern marketing." },
                new Book { Id = _bookId++, Title = "Business Ethics", Author = "Sandra Liu", ISBN = "CBA-0005", Category = "College of Business & Accountancy", TotalCopies = 2, AvailableCopies = 2, AddedDate = DateTime.Now, YearPublished = 2015, Description = "Ethical issues and professional responsibility in business." },
                new Book { Id = _bookId++, Title = "Corporate Law Basics", Author = "Thomas Reid", ISBN = "CBA-0006", Category = "College of Business & Accountancy", TotalCopies = 3, AvailableCopies = 3, AddedDate = DateTime.Now, YearPublished = 2014, Description = "Overview of corporate legal structures and compliance." },
                new Book { Id = _bookId++, Title = "Auditing Principles", Author = "Martha Diaz", ISBN = "CBA-0007", Category = "College of Business & Accountancy", TotalCopies = 3, AvailableCopies = 3, AddedDate = DateTime.Now, YearPublished = 2016, Description = "Principles and practice of auditing." },
                new Book { Id = _bookId++, Title = "Taxation for Businesses", Author = "Alan Shaw", ISBN = "CBA-0008", Category = "College of Business & Accountancy", TotalCopies = 3, AvailableCopies = 3, AddedDate = DateTime.Now, YearPublished = 2018, Description = "Business taxation fundamentals and filing procedures." },
                new Book { Id = _bookId++, Title = "Investment Analysis", Author = "Nina Park", ISBN = "CBA-0009", Category = "College of Business & Accountancy", TotalCopies = 3, AvailableCopies = 3, AddedDate = DateTime.Now, YearPublished = 2020, Description = "Investment valuation and portfolio management." },
                new Book { Id = _bookId++, Title = "Operations Management", Author = "Gregory Moss", ISBN = "CBA-0010", Category = "College of Business & Accountancy", TotalCopies = 4, AvailableCopies = 4, AddedDate = DateTime.Now, YearPublished = 2017, Description = "Operations strategy and process optimization." },
                new Book { Id = _bookId++, Title = "Entrepreneurship Essentials", Author = "Priya Nair", ISBN = "CBA-0011", Category = "College of Business & Accountancy", TotalCopies = 3, AvailableCopies = 3, AddedDate = DateTime.Now, YearPublished = 2021, Description = "Starting and scaling a successful business." },
                new Book { Id = _bookId++, Title = "Strategic Management", Author = "Oliver Grant", ISBN = "CBA-0012", Category = "College of Business & Accountancy", TotalCopies = 3, AvailableCopies = 3, AddedDate = DateTime.Now, YearPublished = 2015, Description = "Long-term strategy formulation and execution." },
                new Book { Id = _bookId++, Title = "Business Analytics", Author = "Fiona Cheng", ISBN = "CBA-0013", Category = "College of Business & Accountancy", TotalCopies = 3, AvailableCopies = 3, AddedDate = DateTime.Now, YearPublished = 2019, Description = "Data-driven decision making for businesses." },
                new Book { Id = _bookId++, Title = "Human Resource Management", Author = "Ethan Brooks", ISBN = "CBA-0014", Category = "College of Business & Accountancy", TotalCopies = 3, AvailableCopies = 3, AddedDate = DateTime.Now, YearPublished = 2016, Description = "HR practices, recruiting, and employee development." },
                new Book { Id = _bookId++, Title = "Supply Chain Management", Author = "Hannah Kim", ISBN = "CBA-0015", Category = "College of Business & Accountancy", TotalCopies = 3, AvailableCopies = 3, AddedDate = DateTime.Now, YearPublished = 2018, Description = "Logistics, procurement, and supply chain strategy." },
                new Book { Id = _bookId++, Title = "Managerial Economics", Author = "Victor Lane", ISBN = "CBA-0016", Category = "College of Business & Accountancy", TotalCopies = 2, AvailableCopies = 2, AddedDate = DateTime.Now, YearPublished = 2013, Description = "Economic concepts applied to managerial decisions." },
                new Book { Id = _bookId++, Title = "Financial Reporting", Author = "Angela King", ISBN = "CBA-0017", Category = "College of Business & Accountancy", TotalCopies = 3, AvailableCopies = 3, AddedDate = DateTime.Now, YearPublished = 2014, Description = "Standards and practices in financial reporting." },
                new Book { Id = _bookId++, Title = "Risk Management", Author = "Mark Rivera", ISBN = "CBA-0018", Category = "College of Business & Accountancy", TotalCopies = 2, AvailableCopies = 2, AddedDate = DateTime.Now, YearPublished = 2017, Description = "Identifying and mitigating business risks." },
                new Book { Id = _bookId++, Title = "Financial Modeling", Author = "Rita Gomez", ISBN = "CBA-0019", Category = "College of Business & Accountancy", TotalCopies = 3, AvailableCopies = 3, AddedDate = DateTime.Now, YearPublished = 2020, Description = "Building financial models for valuation and forecasting." },
                new Book { Id = _bookId++, Title = "Corporate Finance Cases", Author = "Brian Young", ISBN = "CBA-0020", Category = "College of Business & Accountancy", TotalCopies = 2, AvailableCopies = 2, AddedDate = DateTime.Now, YearPublished = 2012, Description = "Case studies in corporate finance decisions." }
            };

            Books.AddRange(cbaList);
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

        private static void SaveAccountsToFile_NoLock()
        {
            try
            {
                var ds = new AccountStore
                {
                    Students = Students.ToList(),
                    Admins = Admins.ToList(),
                    PasswordResets = PasswordResets.Select(p => new PasswordReset { Id = p.Id, StudentId = p.StudentId, Code = p.Code, ExpiresAt = p.ExpiresAt, CreatedAt = p.CreatedAt }).ToList(),
                    StudentId = _studentId,
                    AdminId = _adminId,
                    PasswordResetId = _passwordResetId
                };

                var opts = new JsonSerializerOptions { WriteIndented = true };
                var json = JsonSerializer.Serialize(ds, opts);
                File.WriteAllText(AccountsFile, json);
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
                if (Books.Any(b => string.Equals(b.ISBN, book.ISBN, StringComparison.OrdinalIgnoreCase))) return false;
                book.Id = _bookId++;
                book.AddedDate = DateTime.Now;
                Books.Add(book);
            }
            OnDataChanged();
            return true;
        }

        public static List<Book> GetAllBooks()
        {
            lock (_lock) { return Books.Select(b => CloneBook(b)).OrderBy(b => b.Title).ToList(); }
        }

        public static Book? GetBookById(int id)
        {
            lock (_lock) { var found = Books.FirstOrDefault(b => b.Id == id); return found == null ? null : CloneBook(found); }
        }

        public static bool UpdateBook(Book book)
        {
            lock (_lock)
            {
                var idx = Books.FindIndex(b => b.Id == book.Id);
                if (idx < 0) return false;
                Books[idx] = book;
            }
            OnDataChanged();
            return true;
        }

        public static bool DeleteBook(int id)
        {
            lock (_lock)
            {
                var removed = Books.RemoveAll(b => b.Id == id) > 0;
                if (removed) OnDataChanged();
                return removed;
            }
        }

        private static Book CloneBook(Book b)
        {
            return new Book
            {
                Id = b.Id,
                Title = b.Title,
                Author = b.Author,
                ISBN = b.ISBN,
                Category = b.Category,
                TotalCopies = b.TotalCopies,
                AvailableCopies = b.AvailableCopies,
                AddedDate = b.AddedDate,
                YearPublished = b.YearPublished,
                Description = b.Description
            };
        }

        // Student operations
        public static bool AddStudent(Student student)
        {
            lock (_lock)
            {
                if (Students.Any(s => string.Equals(s.Email, student.Email, StringComparison.OrdinalIgnoreCase) || string.Equals(s.StudentId, student.StudentId, StringComparison.OrdinalIgnoreCase))) return false;
                student.Id = _studentId++;
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
            lock (_lock) { return Students.FirstOrDefault(s => string.Equals(s.Email, emailOrId, StringComparison.OrdinalIgnoreCase) || string.Equals(s.StudentId, emailOrId, StringComparison.OrdinalIgnoreCase)); }
        }

        public static Student? GetStudentById(int id)
        {
            lock (_lock) { return Students.FirstOrDefault(s => s.Id == id); }
        }

        public static List<Student> GetAllStudents()
        {
            lock (_lock) { return Students.Select(s => s).ToList(); }
        }

        public static bool UpdateStudent(Student student)
        {
            lock (_lock)
            {
                var idx = Students.FindIndex(s => s.Id == student.Id);
                if (idx < 0) return false;
                Students[idx] = student;
                SaveAccountsToFile_NoLock();
            }
            OnDataChanged();
            return true;
        }

        public static bool UpdateStudentPassword(int studentId, string newPassword)
        {
            lock (_lock)
            {
                var s = Students.FirstOrDefault(x => x.Id == studentId);
                if (s == null) return false;
                s.Password = newPassword;
                SaveAccountsToFile_NoLock();
            }
            OnDataChanged();
            return true;
        }

        public static Admin? GetAdminByEmail(string email)
        {
            lock (_lock) { return Admins.FirstOrDefault(a => string.Equals(a.Email, email, StringComparison.OrdinalIgnoreCase)); }
        }

        // Borrowing
        public static bool IssueBook(int studentId, int bookId, int daysToReturn = 14)
        {
            lock (_lock)
            {
                var book = Books.FirstOrDefault(b => b.Id == bookId);
                if (book == null || book.AvailableCopies <= 0) return false;
                book.AvailableCopies -= 1;
                var rec = new BorrowRecord
                {
                    Id = _borrowId++,
                    StudentId = studentId,
                    BookId = bookId,
                    IssueDate = DateTime.Now,
                    DueDate = DateTime.Now.AddDays(daysToReturn),
                    ReturnDate = null,
                    Status = "Issued",
                    FineAmount = null
                };
                BorrowRecords.Add(rec);
            }
            OnDataChanged();
            return true;
        }

        public static bool ReturnBook(int recordId)
        {
            lock (_lock)
            {
                var rec = BorrowRecords.FirstOrDefault(r => r.Id == recordId);
                if (rec == null || rec.Status != "Issued") return false;
                rec.ReturnDate = DateTime.Now;
                rec.Status = "Returned";
                var book = Books.FirstOrDefault(b => b.Id == rec.BookId);
                if (book != null) book.AvailableCopies += 1;
            }
            OnDataChanged();
            return true;
        }

        public static bool RenewBook(int recordId, int additionalDays = 14)
        {
            lock (_lock)
            {
                var rec = BorrowRecords.FirstOrDefault(r => r.Id == recordId && r.Status == "Issued");
                if (rec == null) return false;
                rec.DueDate = rec.DueDate.AddDays(additionalDays);
            }
            OnDataChanged();
            return true;
        }

        public static List<BorrowRecord> GetAllBorrowRecords()
        {
            lock (_lock) { return BorrowRecords.Select(r => r).OrderByDescending(r => r.IssueDate).ToList(); }
        }

        public static List<BorrowRecord> GetBorrowRecordsByStudent(int studentId)
        {
            lock (_lock) { return BorrowRecords.Where(r => r.StudentId == studentId).OrderByDescending(r => r.IssueDate).ToList(); }
        }

        public static List<BorrowRecord> GetOverdueBooks()
        {
            lock (_lock) { return BorrowRecords.Where(r => r.Status == "Issued" && r.DueDate.Date < DateTime.Now.Date).OrderBy(r => r.DueDate).ToList(); }
        }

        // Reservations
        public static bool CreateReservation(int studentId, int bookId)
        {
            lock (_lock)
            {
                if (Reservations.Any(r => r.StudentId == studentId && r.BookId == bookId && r.Status != "Cancelled" && r.Status != "Completed")) return false;
                var book = Books.FirstOrDefault(b => b.Id == bookId);
                var status = (book != null && book.AvailableCopies > 0) ? "Ready" : "Pending";
                var res = new Reservation { Id = _reservationId++, StudentId = studentId, BookId = bookId, ReservationDate = DateTime.Now, HoldUntilDate = DateTime.Now.AddDays(7), Status = status };
                Reservations.Add(res);
            }
            OnDataChanged();
            return true;
        }

        public static List<Reservation> GetReservationsByStudent(int studentId)
        {
            lock (_lock) { return Reservations.Where(r => r.StudentId == studentId && r.Status != "Cancelled" && r.Status != "Completed").OrderByDescending(r => r.ReservationDate).ToList(); }
        }

        public static bool ReservationExists(int studentId, int bookId)
        {
            lock (_lock) { return Reservations.Any(r => r.StudentId == studentId && r.BookId == bookId && r.Status != "Cancelled" && r.Status != "Completed"); }
        }

        public static bool CancelReservation(int reservationId)
        {
            lock (_lock)
            {
                var r = Reservations.FirstOrDefault(x => x.Id == reservationId);
                if (r == null) return false;
                r.Status = "Cancelled";
            }
            OnDataChanged();
            return true;
        }

        public static bool PickUpReservation(int reservationId)
        {
            lock (_lock)
            {
                var r = Reservations.FirstOrDefault(x => x.Id == reservationId && x.Status == "Ready");
                if (r == null) return false;
                var ok = IssueBook(r.StudentId, r.BookId);
                if (!ok) return false;
                r.Status = "Completed";
            }
            OnDataChanged();
            return true;
        }

        // Announcements
        public static bool AddAnnouncement(Announcement announcement)
        {
            lock (_lock)
            {
                announcement.Id = _announcementId++;
                announcement.CreatedDate = DateTime.Now;
                Announcements.Add(announcement);
            }
            OnDataChanged();
            return true;
        }

        public static List<Announcement> GetActiveAnnouncements()
        {
            lock (_lock) { return Announcements.Where(a => a.IsActive).OrderByDescending(a => a.CreatedDate).ToList(); }
        }

        public static List<Announcement> GetAllAnnouncements()
        {
            lock (_lock) { return Announcements.OrderByDescending(a => a.CreatedDate).ToList(); }
        }

        public static bool DeleteAnnouncement(int id)
        {
            lock (_lock)
            {
                var removed = Announcements.RemoveAll(a => a.Id == id) > 0;
                if (removed) OnDataChanged();
                return removed;
            }
        }

        // Password resets
        public static bool CreatePasswordReset(int studentId, string code, DateTime expiresAt)
        {
            lock (_lock)
            {
                PasswordResets.Add(new PasswordReset { Id = _passwordResetId++, StudentId = studentId, Code = code, ExpiresAt = expiresAt, CreatedAt = DateTime.Now });
                SaveAccountsToFile_NoLock();
            }
            return true;
        }

        public static (int Id, int StudentId, string Code, DateTime ExpiresAt)? GetPasswordResetByStudentAndCode(int studentId, string code)
        {
            lock (_lock)
            {
                var pr = PasswordResets.FirstOrDefault(p => p.StudentId == studentId && p.Code == code);
                if (pr == null) return null;
                return (pr.Id, pr.StudentId, pr.Code, pr.ExpiresAt);
            }
        }

        public static bool DeletePasswordReset(int id)
        {
            lock (_lock)
            {
                var removed = PasswordResets.RemoveAll(p => p.Id == id) > 0;
                if (removed) SaveAccountsToFile_NoLock();
                return removed;
            }
        }

        // Helpers
        public static int GetTotalBooks()
        {
            lock (_lock) { return Books.Count; }
        }

        public static int GetTotalStudents()
        {
            lock (_lock) { return Students.Count(s => s.IsActive); }
        }

        public static int GetIssuedBooksCount()
        {
            lock (_lock) { return BorrowRecords.Count(r => r.Status == "Issued"); }
        }

        public static int GetOverdueBooksCount()
        {
            lock (_lock) { return BorrowRecords.Count(r => r.Status == "Issued" && r.DueDate.Date < DateTime.Now.Date); }
        }

        public static BorrowRecord? GetBorrowRecordById(int recordId)
        {
            lock (_lock) { return BorrowRecords.FirstOrDefault(r => r.Id == recordId); }
        }

        public static Student? GetStudentByEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return null;
            lock (_lock) { return Students.FirstOrDefault(s => string.Equals(s.Email, email, StringComparison.OrdinalIgnoreCase)); }
        }

        // Internal model for password reset persistence
        private class PasswordReset
        {
            public int Id { get; set; }
            public int StudentId { get; set; }
            public string Code { get; set; } = string.Empty;
            public DateTime ExpiresAt { get; set; }
            public DateTime CreatedAt { get; set; }
        }
    }
}