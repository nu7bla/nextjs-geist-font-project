using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace StudentFeedbackSystem.Data
{
    public static class DBConnection
    {
        private static readonly string connectionString = @"Data Source=(local)\SQLEXPRESS;Initial Catalog=StudentFeedbackDB;Integrated Security=True";

        public static bool TestConnection()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        public static DataTable ValidateLogin(string loginCode, string userType)
        {
            try
            {
                string query = @"
                    SELECT u.UserID, u.UserName, u.UserType
                    FROM Users u
                    INNER JOIN LoginCodes lc ON u.LoginCode = lc.Code
                    WHERE u.LoginCode = @LoginCode 
                    AND u.UserType = @UserType
                    AND lc.UserType = @UserType";

                SqlParameter[] parameters = {
                    new SqlParameter("@LoginCode", loginCode),
                    new SqlParameter("@UserType", userType)
                };

                return ExecuteQuery(query, parameters);
            }
            catch (Exception ex)
            {
                throw new Exception($"Login validation error: {ex.Message}");
            }
        }

        public static DataTable GenerateLoginCode(string userType)
        {
            try
            {
                return ExecuteQuery("EXEC sp_GenerateLoginCode @UserType",
                    new SqlParameter("@UserType", userType));
            }
            catch (Exception ex)
            {
                throw new Exception($"Error generating login code: {ex.Message}");
            }
        }

        public static DataTable GetAllSubjects()
        {
            try
            {
                return ExecuteQuery("EXEC sp_GetAllSubjects");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting subjects: {ex.Message}");
            }
        }

        public static DataTable GetAvailableTeachers()
        {
            try
            {
                return ExecuteQuery("EXEC sp_GetAvailableTeachers");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting teachers: {ex.Message}");
            }
        }

        public static void AddSubject(string subjectName, int teacherId)
        {
            try
            {
                ExecuteQuery("EXEC sp_AddSubject @SubjectName, @TeacherID",
                    new SqlParameter("@SubjectName", subjectName),
                    new SqlParameter("@TeacherID", teacherId));
            }
            catch (Exception ex)
            {
                throw new Exception($"Error adding subject: {ex.Message}");
            }
        }

        public static void DeleteSubject(int subjectId)
        {
            try
            {
                ExecuteQuery("EXEC sp_DeleteSubject @SubjectID",
                    new SqlParameter("@SubjectID", subjectId));
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleting subject: {ex.Message}");
            }
        }

        public static DataTable GetSystemStats()
        {
            try
            {
                return ExecuteQuery("EXEC sp_GetSystemStats");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting system stats: {ex.Message}");
            }
        }

        public static bool HasExistingFeedback(int studentId, int subjectId)
        {
            try
            {
                string query = @"
                    SELECT COUNT(*) 
                    FROM Feedback f
                    INNER JOIN Enrollments e ON f.EnrollmentID = e.EnrollmentID
                    WHERE e.UserID = @StudentID AND e.SubjectID = @SubjectID";

                SqlParameter[] parameters = {
                    new SqlParameter("@StudentID", studentId),
                    new SqlParameter("@SubjectID", subjectId)
                };

                DataTable dt = ExecuteQuery(query, parameters);
                return Convert.ToInt32(dt.Rows[0][0]) > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error checking existing feedback: {ex.Message}");
            }
        }

        public static DataTable GetStudentSubjects(int studentId)
        {
            try
            {
                string query = @"
                    SELECT s.SubjectID, s.SubjectName,
                           CASE WHEN f.FeedbackID IS NULL THEN 0 ELSE 1 END as HasFeedback
                    FROM Subjects s
                    INNER JOIN Enrollments e ON s.SubjectID = e.SubjectID
                    LEFT JOIN Feedback f ON e.EnrollmentID = f.EnrollmentID
                    WHERE e.UserID = @StudentID
                    ORDER BY s.SubjectName";

                return ExecuteQuery(query, new SqlParameter("@StudentID", studentId));
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting student subjects: {ex.Message}");
            }
        }

        public static DataTable GetTeacherSubjects(int teacherId)
        {
            try
            {
                string query = @"
                    SELECT SubjectID, SubjectName,
                           (SELECT COUNT(*) FROM Enrollments e 
                            INNER JOIN Feedback f ON e.EnrollmentID = f.EnrollmentID
                            WHERE e.SubjectID = s.SubjectID) as FeedbackCount
                    FROM Subjects s
                    WHERE TeacherID = @TeacherID
                    ORDER BY SubjectName";

                return ExecuteQuery(query, new SqlParameter("@TeacherID", teacherId));
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting teacher subjects: {ex.Message}");
            }
        }

        public static DataTable GetTeacherFeedback(int teacherId, string subjectName)
        {
            try
            {
                string query = @"
                    SELECT 
                        f.SubmittedOn,
                        f.Q1, f.Q2, f.Q3, f.Q4, f.Q5,
                        f.Comments
                    FROM Feedback f
                    INNER JOIN Enrollments e ON f.EnrollmentID = e.EnrollmentID
                    INNER JOIN Subjects s ON e.SubjectID = s.SubjectID
                    WHERE s.TeacherID = @TeacherID
                    AND s.SubjectName = @SubjectName
                    ORDER BY f.SubmittedOn DESC";

                SqlParameter[] parameters = {
                    new SqlParameter("@TeacherID", teacherId),
                    new SqlParameter("@SubjectName", subjectName)
                };

                return ExecuteQuery(query, parameters);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting teacher feedback: {ex.Message}");
            }
        }

        public static bool SubmitFeedback(int studentId, int subjectId, int[] ratings, string comments)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (SqlTransaction transaction = conn.BeginTransaction())
                {
                    try
                    {
                        // Check for existing feedback within transaction
                        if (HasExistingFeedback(studentId, subjectId))
                        {
                            throw new Exception("You have already submitted feedback for this subject.");
                        }

                        // Get EnrollmentID
                        string query = @"
                            SELECT EnrollmentID 
                            FROM Enrollments 
                            WHERE UserID = @StudentID AND SubjectID = @SubjectID";

                        SqlCommand cmd = new SqlCommand(query, conn, transaction);
                        cmd.Parameters.AddWithValue("@StudentID", studentId);
                        cmd.Parameters.AddWithValue("@SubjectID", subjectId);

                        object enrollmentId = cmd.ExecuteScalar();
                        if (enrollmentId == null)
                        {
                            throw new Exception("Enrollment not found.");
                        }

                        // Insert feedback
                        query = @"
                            INSERT INTO Feedback (EnrollmentID, Q1, Q2, Q3, Q4, Q5, Comments)
                            VALUES (@EnrollmentID, @Q1, @Q2, @Q3, @Q4, @Q5, @Comments)";

                        cmd = new SqlCommand(query, conn, transaction);
                        cmd.Parameters.AddWithValue("@EnrollmentID", enrollmentId);
                        cmd.Parameters.AddWithValue("@Q1", ratings[0]);
                        cmd.Parameters.AddWithValue("@Q2", ratings[1]);
                        cmd.Parameters.AddWithValue("@Q3", ratings[2]);
                        cmd.Parameters.AddWithValue("@Q4", ratings[3]);
                        cmd.Parameters.AddWithValue("@Q5", ratings[4]);
                        cmd.Parameters.AddWithValue("@Comments", comments);

                        cmd.ExecuteNonQuery();
                        transaction.Commit();
                        return true;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public static DataTable ExecuteQuery(string query, params SqlParameter[] parameters)
        {
            DataTable dt = new DataTable();
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        if (parameters != null)
                            cmd.Parameters.AddRange(parameters);

                        using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                        {
                            adapter.Fill(dt);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Database error: {ex.Message}");
            }
            return dt;
        }
    }
}
