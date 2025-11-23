CREATE DATABASE lib_db;

USE lib_db;

CREATE TABLE IF NOT EXISTS Students (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    StudentId VARCHAR(50) UNIQUE NOT NULL,
    FullName VARCHAR(255) NOT NULL,
    Email VARCHAR(255) UNIQUE NOT NULL,
    Password VARCHAR(255) NOT NULL,
    Course VARCHAR(255) NOT NULL,
    Section VARCHAR(50) NOT NULL,
    CreatedAt DATETIME NOT NULL,
    IsActive INT NOT NULL DEFAULT 1
);

CREATE TABLE IF NOT EXISTS Admins (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Email VARCHAR(255) UNIQUE NOT NULL,
    Password VARCHAR(255) NOT NULL,
    FullName VARCHAR(255) NOT NULL,
    CreatedAt DATETIME NOT NULL
);

CREATE TABLE IF NOT EXISTS Books (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Title VARCHAR(500) NOT NULL,
    Author VARCHAR(255) NOT NULL,
    ISBN VARCHAR(100) UNIQUE NOT NULL,
    Category VARCHAR(255) NOT NULL,
    TotalCopies INT NOT NULL,
    AvailableCopies INT NOT NULL,
    AddedDate DATETIME NOT NULL,
    YearPublished INT,
    Description TEXT
);

CREATE TABLE IF NOT EXISTS BorrowRecords (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    StudentId INT NOT NULL,
    BookId INT NOT NULL,
    IssueDate DATETIME NOT NULL,
    DueDate DATETIME NOT NULL,
    ReturnDate DATETIME NULL,
    Status VARCHAR(50) NOT NULL,
    FineAmount DECIMAL(10,2) NULL,
    FOREIGN KEY (StudentId) REFERENCES Students(Id),
    FOREIGN KEY (BookId) REFERENCES Books(Id)
);

CREATE TABLE IF NOT EXISTS Reservations (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    StudentId INT NOT NULL,
    BookId INT NOT NULL,
    ReservationDate DATETIME NOT NULL,
    HoldUntilDate DATETIME NULL,
    Status VARCHAR(50) NOT NULL,
    FOREIGN KEY (StudentId) REFERENCES Students(Id),
    FOREIGN KEY (BookId) REFERENCES Books(Id)
);

CREATE TABLE IF NOT EXISTS Announcements (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Title VARCHAR(500) NOT NULL,
    Content TEXT NOT NULL,
    CreatedDate DATETIME NOT NULL,
    IsActive INT NOT NULL DEFAULT 1
);

CREATE TABLE IF NOT EXISTS PasswordResets (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    StudentId INT NOT NULL,
    Code VARCHAR(50) NOT NULL,
    ExpiresAt DATETIME NOT NULL,
    CreatedAt DATETIME NOT NULL,
    FOREIGN KEY (StudentId) REFERENCES Students(Id)
);

-- Insert Default Admin Account
-- NOTE: The application will automatically hash passwords. These plain text values are for initial setup.
-- If you're importing this SQL directly, you may need to update passwords manually or let the app re-seed with hashed passwords.
INSERT INTO Admins (Email, Password, FullName, CreatedAt) 
VALUES ('libraryadmin@gmail.com', 'admin123', 'Library Administrator', NOW());

-- Insert Default Student Account
-- NOTE: The application will automatically hash passwords. These plain text values are for initial setup.
INSERT INTO Students (StudentId, FullName, Email, Password, Course, Section, CreatedAt, IsActive) 
VALUES ('S1001', 'Test Student', 'student@example.com', 'pass', 'Computer Science', 'A', NOW(), 1);

-- Insert Books: College of Computer Studies (20 books)
INSERT INTO Books (Title, Author, ISBN, Category, TotalCopies, AvailableCopies, AddedDate, YearPublished, Description) VALUES
('Introduction to Programming', 'James Foster', 'CCS-0001', 'College of Computer Studies', 4, 4, NOW(), 2020, 'Basic programming concepts using Python.'),
('Data Structures and Algorithms', 'Maria Santos', 'CCS-0002', 'College of Computer Studies', 3, 3, NOW(), 2019, 'Core data structures and algorithmic problem solving.'),
('Database Management Systems', 'Eric Tan', 'CCS-0003', 'College of Computer Studies', 4, 4, NOW(), 2018, 'Relational databases, SQL, and database design principles.'),
('Web Development Fundamentals', 'Lara Kim', 'CCS-0004', 'College of Computer Studies', 3, 3, NOW(), 2021, 'HTML, CSS, JavaScript, and responsive design.'),
('Object-Oriented Programming', 'Daniel Cruz', 'CCS-0005', 'College of Computer Studies', 4, 4, NOW(), 2017, 'Concepts and implementation of OOP in Java.'),
('Software Engineering Principles', 'Hannah Reed', 'CCS-0006', 'College of Computer Studies', 3, 3, NOW(), 2019, 'Software development methodologies and project management.'),
('Computer Networks', 'Victor Ramos', 'CCS-0007', 'College of Computer Studies', 4, 4, NOW(), 2016, 'Network concepts, OSI model, and routing.'),
('Cybersecurity Essentials', 'Ella Parker', 'CCS-0008', 'College of Computer Studies', 3, 3, NOW(), 2020, 'Information security concepts and threat mitigation.'),
('Human-Computer Interaction', 'Ian Torres', 'CCS-0009', 'College of Computer Studies', 3, 3, NOW(), 2015, 'User-centered interface design and usability.'),
('Machine Learning Basics', 'Olivia Grant', 'CCS-0010', 'College of Computer Studies', 3, 3, NOW(), 2022, 'Introduction to machine learning and AI.'),
('Mobile App Development', 'Chris Howard', 'CCS-0011', 'College of Computer Studies', 4, 4, NOW(), 2021, 'Building Android and iOS applications.'),
('Operating Systems Concepts', 'Faith Navarro', 'CCS-0012', 'College of Computer Studies', 3, 3, NOW(), 2018, 'Fundamentals of operating systems and resource management.'),
('Systems Analysis and Design', 'Kevin Price', 'CCS-0013', 'College of Computer Studies', 3, 3, NOW(), 2016, 'Modeling and designing information systems.'),
('Artificial Intelligence Concepts', 'Jenny Cruz', 'CCS-0014', 'College of Computer Studies', 3, 3, NOW(), 2019, 'AI applications and intelligent systems.'),
('Cloud Computing Fundamentals', 'Patrick Lane', 'CCS-0015', 'College of Computer Studies', 4, 4, NOW(), 2020, 'Cloud architecture and virtualization.'),
('Computer Architecture', 'Derek Lee', 'CCS-0016', 'College of Computer Studies', 3, 3, NOW(), 2017, 'Hardware organization and CPU design.'),
('Parallel Computing', 'Nina Wells', 'CCS-0017', 'College of Computer Studies', 2, 2, NOW(), 2014, 'Parallel algorithms and distributed systems.'),
('Data Mining Techniques', 'Samuel Ortiz', 'CCS-0018', 'College of Computer Studies', 3, 3, NOW(), 2021, 'Extracting insights from large datasets.'),
('Game Development Essentials', 'Troy Mendoza', 'CCS-0019', 'College of Computer Studies', 3, 3, NOW(), 2016, 'Game design principles using Unity.'),
('Computer Forensics', 'Zara Mitchell', 'CCS-0020', 'College of Computer Studies', 2, 2, NOW(), 2018, 'Digital evidence recovery and cybercrime investigation.');

-- Insert Books: College of Education (20 books)
INSERT INTO Books (Title, Author, ISBN, Category, TotalCopies, AvailableCopies, AddedDate, YearPublished, Description) VALUES
('Foundations of Education', 'Emily Davis', 'COED-0001', 'College of Education', 4, 4, NOW(), 2018, 'Overview of educational systems and teaching foundations.'),
('Child and Adolescent Development', 'Mark Wilson', 'COED-0002', 'College of Education', 3, 3, NOW(), 2017, 'Developmental stages of learners.'),
('Curriculum Development', 'Anna Rivera', 'COED-0003', 'College of Education', 4, 4, NOW(), 2019, 'Designing and evaluating curriculum.'),
('Assessment in Learning', 'Kenneth Young', 'COED-0004', 'College of Education', 3, 3, NOW(), 2016, 'Assessment tools and measurement of learning.'),
('Educational Psychology', 'Sarah Gomez', 'COED-0005', 'College of Education', 4, 4, NOW(), 2020, 'Psychology principles in education.'),
('Teaching Strategies and Methods', 'Liam Carter', 'COED-0006', 'College of Education', 3, 3, NOW(), 2015, 'Effective teaching techniques.'),
('Inclusive Education', 'Patricia Lim', 'COED-0007', 'College of Education', 3, 3, NOW(), 2018, 'Teaching diverse learners.'),
('Educational Technology', 'Nathan Brooks', 'COED-0008', 'College of Education', 4, 4, NOW(), 2021, 'Using technology in teaching.'),
('Classroom Management', 'Nicole Adams', 'COED-0009', 'College of Education', 3, 3, NOW(), 2016, 'Managing classroom behavior.'),
('Educational Research Methods', 'Oscar Lee', 'COED-0010', 'College of Education', 3, 3, NOW(), 2019, 'Research fundamentals for education.'),
('Literacy Development', 'Grace Hunt', 'COED-0011', 'College of Education', 4, 4, NOW(), 2018, 'Reading and writing development.'),
('Educational Leadership', 'Henry Cruz', 'COED-0012', 'College of Education', 3, 3, NOW(), 2017, 'Leadership skills for educators.'),
('Philosophy of Education', 'Elaine West', 'COED-0013', 'College of Education', 3, 3, NOW(), 2015, 'Philosophical foundations of teaching.'),
('Science Teaching Methods', 'Albert Cruz', 'COED-0014', 'College of Education', 3, 3, NOW(), 2020, 'Teaching science with engaging strategies.'),
('Mathematics for Educators', 'Donna Roberts', 'COED-0015', 'College of Education', 4, 4, NOW(), 2019, 'Math concepts and teaching approaches.'),
('Educational Assessment Tools', 'Isaac Brown', 'COED-0016', 'College of Education', 2, 2, NOW(), 2014, 'Tools for evaluating student performance.'),
('Guidance and Counseling', 'Theresa Lane', 'COED-0017', 'College of Education', 3, 3, NOW(), 2016, 'Counseling approaches for learners.'),
('History of Education', 'Marvin Ellis', 'COED-0018', 'College of Education', 3, 3, NOW(), 2013, 'Evolution of educational practices.'),
('Technology for Teaching Literacy', 'Faye Kim', 'COED-0019', 'College of Education', 3, 3, NOW(), 2021, 'Digital tools for literacy education.'),
('Assessment and Evaluation', 'Philip Yang', 'COED-0020', 'College of Education', 3, 3, NOW(), 2017, 'Evaluating student learning outcomes.');

-- Insert Books: College of Engineering (20 books)
INSERT INTO Books (Title, Author, ISBN, Category, TotalCopies, AvailableCopies, AddedDate, YearPublished, Description) VALUES
('Engineering Mechanics', 'David Thornton', 'COE-0001', 'College of Engineering', 4, 4, NOW(), 2016, 'Statics and dynamics principles.'),
('Thermodynamics', 'Sophia Vale', 'COE-0002', 'College of Engineering', 3, 3, NOW(), 2017, 'Energy systems and thermodynamic laws.'),
('Engineering Mathematics', 'John Perez', 'COE-0003', 'College of Engineering', 4, 4, NOW(), 2019, 'Math concepts essential for engineering.'),
('Electrical Circuits', 'Kathryn Lee', 'COE-0004', 'College of Engineering', 3, 3, NOW(), 2018, 'Circuits and electrical analysis.'),
('Fluid Mechanics', 'Anthony Price', 'COE-0005', 'College of Engineering', 4, 4, NOW(), 2015, 'Study of fluid behavior and forces.'),
('Materials Science', 'Grace Choi', 'COE-0006', 'College of Engineering', 3, 3, NOW(), 2020, 'Properties of engineering materials.'),
('Computer-Aided Design (CAD)', 'Michael Cruz', 'COE-0007', 'College of Engineering', 4, 4, NOW(), 2021, 'Modern engineering design tools.'),
('Engineering Economics', 'Linda Garcia', 'COE-0008', 'College of Engineering', 3, 3, NOW(), 2016, 'Economic decision making for engineers.'),
('Control Systems Engineering', 'Peter Ramos', 'COE-0009', 'College of Engineering', 3, 3, NOW(), 2018, 'Stability and feedback systems.'),
('Mechanics of Materials', 'Rachel Adams', 'COE-0010', 'College of Engineering', 3, 3, NOW(), 2017, 'Stress, strain, and deformation.'),
('Structural Analysis', 'Kyle Sanders', 'COE-0011', 'College of Engineering', 4, 4, NOW(), 2018, 'Analyzing structural integrity.'),
('Environmental Engineering', 'Tina Morales', 'COE-0012', 'College of Engineering', 3, 3, NOW(), 2019, 'Environmental systems and sustainability.'),
('Engineering Design Process', 'Ryan Ford', 'COE-0013', 'College of Engineering', 3, 3, NOW(), 2015, 'Design methodologies in engineering.'),
('Hydraulics Engineering', 'Samantha King', 'COE-0014', 'College of Engineering', 3, 3, NOW(), 2020, 'Water flow and hydraulic systems.'),
('Power Plant Engineering', 'Justin Cruz', 'COE-0015', 'College of Engineering', 3, 3, NOW(), 2016, 'Power generation systems.'),
('Heat Transfer', 'Isabel Flores', 'COE-0016', 'College of Engineering', 4, 4, NOW(), 2017, 'Conduction, convection, and radiation.'),
('Robotics Engineering', 'Zachary Hill', 'COE-0017', 'College of Engineering', 2, 2, NOW(), 2021, 'Robot motion, sensors, and controls.'),
('Transport Engineering', 'Pamela Lee', 'COE-0018', 'College of Engineering', 3, 3, NOW(), 2019, 'Traffic systems and transportation design.'),
('Instrumentation and Measurement', 'Quinn Torres', 'COE-0019', 'College of Engineering', 3, 3, NOW(), 2016, 'Measurement systems and sensors.'),
('Renewable Energy Systems', 'Harold Green', 'COE-0020', 'College of Engineering', 2, 2, NOW(), 2022, 'Solar, wind, and eco-friendly systems.');

-- Insert Books: College of Business & Accountancy (20 books)
INSERT INTO Books (Title, Author, ISBN, Category, TotalCopies, AvailableCopies, AddedDate, YearPublished, Description) VALUES
('Principles of Accounting', 'Laura Bennett', 'CBA-0001', 'College of Business & Accountancy', 4, 4, NOW(), 2016, 'Introductory accounting principles for business students.'),
('Managerial Accounting', 'Daniel Kim', 'CBA-0002', 'College of Business & Accountancy', 3, 3, NOW(), 2017, 'Techniques and practices in managerial accounting.'),
('Financial Management', 'Evelyn Ortiz', 'CBA-0003', 'College of Business & Accountancy', 3, 3, NOW(), 2018, 'Foundations of corporate finance and financial decision making.'),
('Marketing Management', 'Robert Fields', 'CBA-0004', 'College of Business & Accountancy', 4, 4, NOW(), 2019, 'Strategies and concepts in modern marketing.'),
('Business Ethics', 'Sandra Liu', 'CBA-0005', 'College of Business & Accountancy', 2, 2, NOW(), 2015, 'Ethical issues and professional responsibility in business.'),
('Corporate Law Basics', 'Thomas Reid', 'CBA-0006', 'College of Business & Accountancy', 3, 3, NOW(), 2014, 'Overview of corporate legal structures and compliance.'),
('Auditing Principles', 'Martha Diaz', 'CBA-0007', 'College of Business & Accountancy', 3, 3, NOW(), 2016, 'Principles and practice of auditing.'),
('Taxation for Businesses', 'Alan Shaw', 'CBA-0008', 'College of Business & Accountancy', 3, 3, NOW(), 2018, 'Business taxation fundamentals and filing procedures.'),
('Investment Analysis', 'Nina Park', 'CBA-0009', 'College of Business & Accountancy', 3, 3, NOW(), 2020, 'Investment valuation and portfolio management.'),
('Operations Management', 'Gregory Moss', 'CBA-0010', 'College of Business & Accountancy', 4, 4, NOW(), 2017, 'Operations strategy and process optimization.'),
('Entrepreneurship Essentials', 'Priya Nair', 'CBA-0011', 'College of Business & Accountancy', 3, 3, NOW(), 2021, 'Starting and scaling a successful business.'),
('Strategic Management', 'Oliver Grant', 'CBA-0012', 'College of Business & Accountancy', 3, 3, NOW(), 2015, 'Long-term strategy formulation and execution.'),
('Business Analytics', 'Fiona Cheng', 'CBA-0013', 'College of Business & Accountancy', 3, 3, NOW(), 2019, 'Data-driven decision making for businesses.'),
('Human Resource Management', 'Ethan Brooks', 'CBA-0014', 'College of Business & Accountancy', 3, 3, NOW(), 2016, 'HR practices, recruiting, and employee development.'),
('Supply Chain Management', 'Hannah Kim', 'CBA-0015', 'College of Business & Accountancy', 3, 3, NOW(), 2018, 'Logistics, procurement, and supply chain strategy.'),
('Managerial Economics', 'Victor Lane', 'CBA-0016', 'College of Business & Accountancy', 2, 2, NOW(), 2013, 'Economic concepts applied to managerial decisions.'),
('Financial Reporting', 'Angela King', 'CBA-0017', 'College of Business & Accountancy', 3, 3, NOW(), 2014, 'Standards and practices in financial reporting.'),
('Risk Management', 'Mark Rivera', 'CBA-0018', 'College of Business & Accountancy', 2, 2, NOW(), 2017, 'Identifying and mitigating business risks.'),
('Financial Modeling', 'Rita Gomez', 'CBA-0019', 'College of Business & Accountancy', 3, 3, NOW(), 2020, 'Building financial models for valuation and forecasting.'),
('Corporate Finance Cases', 'Brian Young', 'CBA-0020', 'College of Business & Accountancy', 2, 2, NOW(), 2012, 'Case studies in corporate finance decisions.');

