-- Create Database
USE master;
GO

IF EXISTS (SELECT * FROM sys.databases WHERE name = 'StudentFeedbackDB')
BEGIN
    ALTER DATABASE StudentFeedbackDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE StudentFeedbackDB;
END
GO

CREATE DATABASE StudentFeedbackDB;
GO

USE StudentFeedbackDB;
GO

-- Create LoginCodes table first
CREATE TABLE LoginCodes (
    CodeID INT PRIMARY KEY IDENTITY(1,1),
    Code NVARCHAR(50) NOT NULL,
    UserType NVARCHAR(50) NOT NULL CHECK (UserType IN ('Student', 'Teacher', 'Admin')),
    IsUsed BIT DEFAULT 0,
    GeneratedOn DATETIME DEFAULT GETDATE(),
    CONSTRAINT UC_LoginCode UNIQUE (Code)
);
GO

-- Create Users table with foreign key to LoginCodes
CREATE TABLE Users (
    UserID INT PRIMARY KEY IDENTITY(1,1),
    UserName NVARCHAR(100) NOT NULL,
    UserType NVARCHAR(50) NOT NULL CHECK (UserType IN ('Student', 'Teacher', 'Admin')),
    LoginCode NVARCHAR(50) NOT NULL,
    CONSTRAINT FK_Users_LoginCodes FOREIGN KEY (LoginCode) REFERENCES LoginCodes(Code)
);
GO

-- Create Subjects table with foreign key to Users (Teachers)
CREATE TABLE Subjects (
    SubjectID INT PRIMARY KEY IDENTITY(1,1),
    SubjectName NVARCHAR(100) NOT NULL,
    TeacherID INT NOT NULL,
    CONSTRAINT FK_Subjects_Teachers FOREIGN KEY (TeacherID) REFERENCES Users(UserID)
);
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

-- Create stored procedure for generating login codes
CREATE OR ALTER PROCEDURE sp_GenerateLoginCode
    @UserType NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Code NVARCHAR(50);
    DECLARE @Prefix NVARCHAR(10);
    DECLARE @Number INT;
    
    -- Set prefix based on user type
    SET @Prefix = CASE @UserType
        WHEN 'Student' THEN 'STU'
        WHEN 'Teacher' THEN 'TCHR'
        ELSE 'INVALID'
    END;
    
    IF @Prefix = 'INVALID'
    BEGIN
        RAISERROR ('Invalid user type specified.', 16, 1);
        RETURN;
    END;
    
    -- Get the next number
    SELECT @Number = ISNULL(MAX(CAST(SUBSTRING(Code, LEN(@Prefix) + 1, 3) AS INT)), 0) + 1
    FROM LoginCodes
    WHERE Code LIKE @Prefix + '%';
    
    -- Generate the code
    SET @Code = @Prefix + RIGHT('000' + CAST(@Number AS VARCHAR(3)), 3);
    
    -- Insert the new code
    INSERT INTO LoginCodes (Code, UserType, IsUsed)
    VALUES (@Code, @UserType, 0);
    
    -- Return the generated code
    SELECT @Code AS GeneratedCode;
END;
GO

-- Create stored procedure for getting all subjects with details
CREATE OR ALTER PROCEDURE sp_GetAllSubjects
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        s.SubjectID,
        s.SubjectName,
        s.TeacherID,
        u.UserName as TeacherName,
        (SELECT COUNT(*) FROM Enrollments e WHERE e.SubjectID = s.SubjectID) as EnrollmentCount,
        (
            SELECT COUNT(*) 
            FROM Feedback f
            INNER JOIN Enrollments e ON f.EnrollmentID = e.EnrollmentID
            WHERE e.SubjectID = s.SubjectID
        ) as FeedbackCount
    FROM Subjects s
    INNER JOIN Users u ON s.TeacherID = u.UserID
    ORDER BY s.SubjectName;
END;
GO

-- Create stored procedure for adding a subject
CREATE OR ALTER PROCEDURE sp_AddSubject
    @SubjectName NVARCHAR(100),
    @TeacherID INT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Check if subject already exists for this teacher
    IF EXISTS (SELECT 1 FROM Subjects WHERE SubjectName = @SubjectName AND TeacherID = @TeacherID)
    BEGIN
        RAISERROR ('Subject already exists for this teacher.', 16, 1);
        RETURN;
    END
    
    -- Insert new subject
    INSERT INTO Subjects (SubjectName, TeacherID)
    VALUES (@SubjectName, @TeacherID);
    
    SELECT SCOPE_IDENTITY() AS SubjectID;
END;
GO

-- Create stored procedure for deleting a subject
CREATE OR ALTER PROCEDURE sp_DeleteSubject
    @SubjectID INT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Check if subject has any feedback
    IF EXISTS (
        SELECT 1 
        FROM Feedback f
        INNER JOIN Enrollments e ON f.EnrollmentID = e.EnrollmentID
        WHERE e.SubjectID = @SubjectID
    )
    BEGIN
        RAISERROR ('Cannot delete subject that has feedback.', 16, 1);
        RETURN;
    END
    
    BEGIN TRANSACTION;
    
    BEGIN TRY
        -- Delete enrollments first
        DELETE FROM Enrollments WHERE SubjectID = @SubjectID;
        
        -- Delete subject
        DELETE FROM Subjects WHERE SubjectID = @SubjectID;
        
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO

-- Create stored procedure for getting system statistics
CREATE OR ALTER PROCEDURE sp_GetSystemStats
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        (SELECT COUNT(*) FROM Users WHERE UserType = 'Student') as StudentCount,
        (SELECT COUNT(*) FROM Users WHERE UserType = 'Teacher') as TeacherCount,
        (SELECT COUNT(*) FROM Subjects) as SubjectCount,
        (SELECT COUNT(*) FROM Feedback) as FeedbackCount;
END;
GO

-- Create stored procedure for getting available teachers
CREATE OR ALTER PROCEDURE sp_GetAvailableTeachers
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT UserID, UserName
    FROM Users
    WHERE UserType = 'Teacher'
    ORDER BY UserName;
END;
GO

-- Insert sample data

-- Insert Admin login code and user
INSERT INTO LoginCodes (Code, UserType, IsUsed) VALUES ('ADMIN001', 'Admin', 1);
INSERT INTO Users (UserName, UserType, LoginCode) VALUES ('System Admin', 'Admin', 'ADMIN001');
GO

-- Insert Teacher login codes and users
INSERT INTO LoginCodes (Code, UserType, IsUsed) VALUES 
('TCHR001', 'Teacher', 1),
('TCHR002', 'Teacher', 1),
('TCHR003', 'Teacher', 1);

INSERT INTO Users (UserName, UserType, LoginCode) VALUES 
('John Smith', 'Teacher', 'TCHR001'),
('Mary Johnson', 'Teacher', 'TCHR002'),
('Robert Wilson', 'Teacher', 'TCHR003');
GO

-- Insert Student login codes and users
INSERT INTO LoginCodes (Code, UserType, IsUsed) VALUES 
('STU001', 'Student', 1),
('STU002', 'Student', 1),
('STU003', 'Student', 1),
('STU004', 'Student', 1);

INSERT INTO Users (UserName, UserType, LoginCode) VALUES 
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
