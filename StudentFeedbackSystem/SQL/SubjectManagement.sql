USE StudentFeedbackDB;
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

-- Create stored procedure for getting all subjects with teacher names
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
