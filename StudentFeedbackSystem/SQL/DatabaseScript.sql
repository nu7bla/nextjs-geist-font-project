-- Create Database
CREATE DATABASE StudentFeedbackDB;
GO

USE StudentFeedbackDB;
GO

-- Create Users table
CREATE TABLE Users (
    UserID INT PRIMARY KEY IDENTITY(1,1),
    UserName NVARCHAR(100) NOT NULL,
    UserType NVARCHAR(50) NOT NULL CHECK (UserType IN ('Student', 'Teacher', 'Admin')),
    LoginCode NVARCHAR(50) UNIQUE NOT NULL
);
GO

-- Create Subjects table
CREATE TABLE Subjects (
    SubjectID INT PRIMARY KEY IDENTITY(1,1),
    SubjectName NVARCHAR(100) NOT NULL,
    TeacherID INT NOT NULL,
    FOREIGN KEY (TeacherID) REFERENCES Users(UserID)
);
GO

-- Create Enrollments table
CREATE TABLE Enrollments (
    EnrollmentID INT PRIMARY KEY IDENTITY(1,1),
    UserID INT NOT NULL,
    SubjectID INT NOT NULL,
    FOREIGN KEY (UserID) REFERENCES Users(UserID),
    FOREIGN KEY (SubjectID) REFERENCES Subjects(SubjectID)
);
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
    FOREIGN KEY (EnrollmentID) REFERENCES Enrollments(EnrollmentID)
);
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

-- Insert sample data

-- Insert Admin
INSERT INTO Users (UserName, UserType, LoginCode)
VALUES ('System Admin', 'Admin', 'ADMIN001');
GO

-- Insert Teachers
INSERT INTO Users (UserName, UserType, LoginCode)
VALUES 
    ('John Smith', 'Teacher', 'TCHR001'),
    ('Mary Johnson', 'Teacher', 'TCHR002'),
    ('Robert Wilson', 'Teacher', 'TCHR003');
GO

-- Insert Students
INSERT INTO Users (UserName, UserType, LoginCode)
VALUES 
    ('Alice Brown', 'Student', 'STU001'),
    ('Bob Davis', 'Student', 'STU002'),
    ('Carol White', 'Student', 'STU003'),
    ('David Miller', 'Student', 'STU004');
GO

-- Insert Subjects
INSERT INTO Subjects (SubjectName, TeacherID)
VALUES 
    ('Mathematics', (SELECT UserID FROM Users WHERE LoginCode = 'TCHR001')),
    ('Physics', (SELECT UserID FROM Users WHERE LoginCode = 'TCHR002')),
    ('Computer Science', (SELECT UserID FROM Users WHERE LoginCode = 'TCHR003'));
GO

-- Insert Enrollments
INSERT INTO Enrollments (UserID, SubjectID)
SELECT u.UserID, s.SubjectID
FROM Users u
CROSS JOIN Subjects s
WHERE u.UserType = 'Student';
GO

-- Insert Sample Feedback
INSERT INTO Feedback (EnrollmentID, Q1, Q2, Q3, Q4, Q5, Comments)
SELECT 
    e.EnrollmentID,
    FLOOR(RAND()*(5-3+1))+3, -- Random rating between 3 and 5
    FLOOR(RAND()*(5-3+1))+3,
    FLOOR(RAND()*(5-3+1))+3,
    FLOOR(RAND()*(5-3+1))+3,
    FLOOR(RAND()*(5-3+1))+3,
    'Sample feedback comment for the subject.'
FROM Enrollments e;
GO

-- Create Stored Procedures

-- Get User Details
CREATE PROCEDURE sp_GetUserDetails
    @LoginCode NVARCHAR(50),
    @UserType NVARCHAR(50)
AS
BEGIN
    SELECT UserID, UserName, UserType
    FROM Users
    WHERE LoginCode = @LoginCode AND UserType = @UserType;
END
GO

-- Get Enrolled Subjects
CREATE PROCEDURE sp_GetEnrolledSubjects
    @StudentID INT
AS
BEGIN
    SELECT s.SubjectID, s.SubjectName, u.UserName as TeacherName
    FROM Subjects s
    INNER JOIN Enrollments e ON s.SubjectID = e.SubjectID
    INNER JOIN Users u ON s.TeacherID = u.UserID
    WHERE e.UserID = @StudentID;
END
GO

-- Submit Feedback
CREATE PROCEDURE sp_SubmitFeedback
    @EnrollmentID INT,
    @Q1 INT,
    @Q2 INT,
    @Q3 INT,
    @Q4 INT,
    @Q5 INT,
    @Comments NVARCHAR(MAX)
AS
BEGIN
    INSERT INTO Feedback (EnrollmentID, Q1, Q2, Q3, Q4, Q5, Comments)
    VALUES (@EnrollmentID, @Q1, @Q2, @Q3, @Q4, @Q5, @Comments);
END
GO

-- Get Teacher's Feedback
CREATE PROCEDURE sp_GetTeacherFeedback
    @TeacherID INT
AS
BEGIN
    SELECT 
        s.SubjectName,
        f.SubmittedOn,
        f.Q1, f.Q2, f.Q3, f.Q4, f.Q5,
        f.Comments
    FROM Feedback f
    INNER JOIN Enrollments e ON f.EnrollmentID = e.EnrollmentID
    INNER JOIN Subjects s ON e.SubjectID = s.SubjectID
    WHERE s.TeacherID = @TeacherID
    ORDER BY f.SubmittedOn DESC;
END
GO

-- Generate Login Code
CREATE PROCEDURE sp_GenerateLoginCode
    @UserType NVARCHAR(50)
AS
BEGIN
    DECLARE @Code NVARCHAR(50);
    SET @Code = CONCAT(
        UPPER(LEFT(@UserType, 1)),
        RIGHT('000' + CAST((SELECT COUNT(*) + 1 FROM LoginCodes WHERE UserType = @UserType) AS VARCHAR), 3)
    );
    
    INSERT INTO LoginCodes (Code, UserType)
    VALUES (@Code, @UserType);
    
    SELECT @Code AS GeneratedCode;
END
GO
