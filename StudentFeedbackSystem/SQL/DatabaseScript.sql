-- Create Database
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'StudentFeedbackDB')
BEGIN
    CREATE DATABASE StudentFeedbackDB;
END
GO

USE StudentFeedbackDB;
GO

-- Drop existing tables if they exist
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Feedback')
    DROP TABLE Feedback;
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Enrollments')
    DROP TABLE Enrollments;
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Subjects')
    DROP TABLE Subjects;
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Users')
    DROP TABLE Users;
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'LoginCodes')
    DROP TABLE LoginCodes;
GO

-- Create LoginCodes table
CREATE TABLE LoginCodes (
    CodeID INT PRIMARY KEY IDENTITY(1,1),
    Code NVARCHAR(50) UNIQUE NOT NULL,
    UserType NVARCHAR(50) NOT NULL CHECK (UserType IN ('Student', 'Teacher', 'Admin')),
    IsUsed BIT DEFAULT 0,
    GeneratedOn DATETIME DEFAULT GETDATE()
);
GO

-- Create Users table
CREATE TABLE Users (
    UserID INT PRIMARY KEY IDENTITY(1,1),
    UserName NVARCHAR(100) NOT NULL,
    UserType NVARCHAR(50) NOT NULL CHECK (UserType IN ('Student', 'Teacher', 'Admin')),
    LoginCode NVARCHAR(50) UNIQUE NOT NULL,
    CONSTRAINT FK_Users_LoginCodes FOREIGN KEY (LoginCode) REFERENCES LoginCodes(Code)
);
GO

-- Create Subjects table
CREATE TABLE Subjects (
    SubjectID INT PRIMARY KEY IDENTITY(1,1),
    SubjectName NVARCHAR(100) NOT NULL,
    TeacherID INT NOT NULL,
    CONSTRAINT FK_Subjects_Teachers FOREIGN KEY (TeacherID) REFERENCES Users(UserID)
);
GO

-- Create index on TeacherID for better performance
CREATE INDEX IX_Subjects_TeacherID ON Subjects(TeacherID);
GO

-- Create Enrollments table
CREATE TABLE Enrollments (
    EnrollmentID INT PRIMARY KEY IDENTITY(1,1),
    UserID INT NOT NULL,
    SubjectID INT NOT NULL,
    CONSTRAINT FK_Enrollments_Users FOREIGN KEY (UserID) REFERENCES Users(UserID),
    CONSTRAINT FK_Enrollments_Subjects FOREIGN KEY (SubjectID) REFERENCES Subjects(SubjectID),
    CONSTRAINT UC_Enrollment UNIQUE (UserID, SubjectID)
);
GO

-- Create indexes for better query performance
CREATE INDEX IX_Enrollments_UserID ON Enrollments(UserID);
CREATE INDEX IX_Enrollments_SubjectID ON Enrollments(SubjectID);
GO

-- Create Feedback table
CREATE TABLE Feedback (
    FeedbackID INT PRIMARY KEY IDENTITY(1,1),
    EnrollmentID INT NOT NULL,
    Q1 INT NOT NULL CHECK (Q1 BETWEEN 1 AND 5),
    Q2 INT NOT NULL CHECK (Q2 BETWEEN 1 AND 5),
    Q3 INT NOT NULL CHECK (Q3 BETWEEN 1 AND 5),
    Q4 INT NOT NULL CHECK (Q4 BETWEEN 1 AND 5),
    Q5 INT NOT NULL CHECK (Q5 BETWEEN 1 AND 5),
    Comments NVARCHAR(MAX),
    SubmittedOn DATETIME DEFAULT GETDATE(),
    CONSTRAINT FK_Feedback_Enrollments FOREIGN KEY (EnrollmentID) REFERENCES Enrollments(EnrollmentID),
    CONSTRAINT UC_Feedback_Enrollment UNIQUE (EnrollmentID)
);
GO

-- Create index for better performance on feedback queries
CREATE INDEX IX_Feedback_SubmittedOn ON Feedback(SubmittedOn);
GO

-- Insert sample data

-- Insert Admin
INSERT INTO LoginCodes (Code, UserType, IsUsed) VALUES ('ADMIN001', 'Admin', 1);
INSERT INTO Users (UserName, UserType, LoginCode)
VALUES ('System Admin', 'Admin', 'ADMIN001');
GO

-- Insert Teachers
INSERT INTO LoginCodes (Code, UserType, IsUsed) 
VALUES 
    ('TCHR001', 'Teacher', 1),
    ('TCHR002', 'Teacher', 1),
    ('TCHR003', 'Teacher', 1);

INSERT INTO Users (UserName, UserType, LoginCode)
VALUES 
    ('John Smith', 'Teacher', 'TCHR001'),
    ('Mary Johnson', 'Teacher', 'TCHR002'),
    ('Robert Wilson', 'Teacher', 'TCHR003');
GO

-- Insert Students
INSERT INTO LoginCodes (Code, UserType, IsUsed)
VALUES 
    ('STU001', 'Student', 1),
    ('STU002', 'Student', 1),
    ('STU003', 'Student', 1),
    ('STU004', 'Student', 1);

INSERT INTO Users (UserName, UserType, LoginCode)
VALUES 
    ('Alice Brown', 'Student', 'STU001'),
    ('Bob Davis', 'Student', 'STU002'),
    ('Carol White', 'Student', 'STU003'),
    ('David Miller', 'Student', 'STU004');
GO

-- Insert Subjects
INSERT INTO Subjects (SubjectName, TeacherID)
SELECT 'Mathematics', UserID FROM Users WHERE LoginCode = 'TCHR001'
UNION ALL
SELECT 'Physics', UserID FROM Users WHERE LoginCode = 'TCHR002'
UNION ALL
SELECT 'Computer Science', UserID FROM Users WHERE LoginCode = 'TCHR003';
GO

-- Insert Enrollments (all students in all subjects)
INSERT INTO Enrollments (UserID, SubjectID)
SELECT u.UserID, s.SubjectID
FROM Users u
CROSS JOIN Subjects s
WHERE u.UserType = 'Student';
GO

-- Create Stored Procedures

-- Get User Details
CREATE OR ALTER PROCEDURE sp_GetUserDetails
    @LoginCode NVARCHAR(50),
    @UserType NVARCHAR(50)
AS
BEGIN
    SELECT UserID, UserName, UserType
    FROM Users
    WHERE LoginCode = @LoginCode AND UserType = @UserType;
END
GO

-- Get Student Subjects
CREATE OR ALTER PROCEDURE sp_GetStudentSubjects
    @StudentID INT
AS
BEGIN
    SELECT s.SubjectID, s.SubjectName,
           CASE WHEN f.FeedbackID IS NULL THEN 0 ELSE 1 END as HasFeedback
    FROM Subjects s
    INNER JOIN Enrollments e ON s.SubjectID = e.SubjectID
    LEFT JOIN Feedback f ON e.EnrollmentID = f.EnrollmentID
    WHERE e.UserID = @StudentID;
END
GO

-- Get Teacher Feedback
CREATE OR ALTER PROCEDURE sp_GetTeacherFeedback
    @TeacherID INT,
    @SubjectName NVARCHAR(100)
AS
BEGIN
    SELECT 
        f.SubmittedOn,
        f.Q1, f.Q2, f.Q3, f.Q4, f.Q5,
        f.Comments
    FROM Feedback f
    INNER JOIN Enrollments e ON f.EnrollmentID = e.EnrollmentID
    INNER JOIN Subjects s ON e.SubjectID = s.SubjectID
    WHERE s.TeacherID = @TeacherID
    AND s.SubjectName = @SubjectName
    ORDER BY f.SubmittedOn DESC;
END
GO

-- Get System Statistics
CREATE OR ALTER PROCEDURE sp_GetSystemStats
AS
BEGIN
    SELECT 
        (SELECT COUNT(*) FROM Users WHERE UserType = 'Student') as StudentCount,
        (SELECT COUNT(*) FROM Users WHERE UserType = 'Teacher') as TeacherCount,
        (SELECT COUNT(*) FROM Subjects) as SubjectCount,
        (SELECT COUNT(*) FROM Feedback) as FeedbackCount;
END
GO
