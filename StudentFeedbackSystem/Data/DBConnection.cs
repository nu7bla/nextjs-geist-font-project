using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Windows.Forms;

namespace StudentFeedbackSystem.Data
{
    public static class DBConnection
    {
        private static string connectionString = @"Data Source=(local)\SQLEXPRESS;Initial Catalog=StudentFeedbackDB;Integrated Security=True";

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
            catch (Exception)
            {
                return false;
            }
        }

        #region Authentication Methods

        public static DataTable ValidateLogin(string loginCode, string userType)
        {
            string query = @"SELECT UserID, UserName, UserType 
                           FROM Users 
                           WHERE LoginCode = @LoginCode 
                           AND UserType = @UserType";

            SqlParameter[] parameters = {
                new SqlParameter("@LoginCode", loginCode),
                new SqlParameter("@UserType", userType)
            };

            return ExecuteQuery(query, parameters);
        }

        #endregion

        #region Student Methods

        public static DataTable GetEnrolledSubjects(int studentId)
        {
            string query = @"SELECT s.SubjectID, s.SubjectName, u.UserName as TeacherName
                           FROM Subjects s
                           INNER JOIN Enrollments e ON s.SubjectID = e.SubjectID
                           INNER JOIN Users u ON s.TeacherID = u.UserID
                           WHERE e.UserID = @StudentID";

            SqlParameter[] parameters = {
                new SqlParameter("@StudentID", studentId)
            };

            return ExecuteQuery(query, parameters);
        }

        public static bool SubmitFeedback(int enrollmentId, int[] ratings, string comments)
        {
            string query = @"INSERT INTO Feedback (EnrollmentID, Q1, Q2, Q3, Q4, Q5, Comments)
                           VALUES (@EnrollmentID, @Q1, @Q2, @Q3, @Q4, @Q5, @Comments)";

            SqlParameter[] parameters = {
                new SqlParameter("@EnrollmentID", enrollmentId),
                new SqlParameter("@Q1", ratings[0]),
                new SqlParameter("@Q2", ratings[1]),
                new SqlParameter("@Q3", ratings[2]),
                new SqlParameter("@Q4", ratings[3]),
                new SqlParameter("@Q5", ratings[4]),
                new SqlParameter("@Comments", comments)
            };

            return ExecuteNonQuery(query, parameters) > 0;
        }

        #endregion

        #region Teacher Methods

        public static DataTable GetTeacherSubjects(int teacherId)
        {
            string query = @"SELECT SubjectID, SubjectName 
                           FROM Subjects 
                           WHERE TeacherID = @TeacherID";

            SqlParameter[] parameters = {
                new SqlParameter("@TeacherID", teacherId)
            };

            return ExecuteQuery(query, parameters);
        }

        public static DataTable GetSubjectFeedback(int subjectId)
        {
            string query = @"SELECT f.SubmittedOn, f.Q1, f.Q2, f.Q3, f.Q4, f.Q5, f.Comments
                           FROM Feedback f
                           INNER JOIN Enrollments e ON f.EnrollmentID = e.EnrollmentID
                           WHERE e.SubjectID = @SubjectID
                           ORDER BY f.SubmittedOn DESC";

            SqlParameter[] parameters = {
                new SqlParameter("@SubjectID", subjectId)
            };

            return ExecuteQuery(query, parameters);
        }

        #endregion

        #region Admin Methods

        public static string GenerateLoginCode(string userType)
        {
            string code = GenerateRandomCode();
            
            string query = @"INSERT INTO LoginCodes (Code, UserType, IsUsed)
                           VALUES (@Code, @UserType, 0)";

            SqlParameter[] parameters = {
                new SqlParameter("@Code", code),
                new SqlParameter("@UserType", userType)
            };

            if (ExecuteNonQuery(query, parameters) > 0)
                return code;
            
            return null;
        }

        public static DataTable GetAllLoginCodes()
        {
            string query = @"SELECT Code, UserType, 
                           CASE WHEN IsUsed = 1 THEN 'Used' ELSE 'Unused' END as Status,
                           GeneratedOn
                           FROM LoginCodes
                           ORDER BY GeneratedOn DESC";

            return ExecuteQuery(query);
        }

        public static DataTable GetSystemStats()
        {
            string query = @"SELECT 
                           (SELECT COUNT(*) FROM Users WHERE UserType = 'Student') as StudentCount,
                           (SELECT COUNT(*) FROM Users WHERE UserType = 'Teacher') as TeacherCount,
                           (SELECT COUNT(*) FROM Subjects) as SubjectCount,
                           (SELECT COUNT(*) FROM Feedback) as FeedbackCount";

            return ExecuteQuery(query);
        }

        #endregion

        #region Helper Methods

        private static DataTable ExecuteQuery(string query, SqlParameter[] parameters = null)
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
                MessageBox.Show($"Database error: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return dt;
        }

        private static int ExecuteNonQuery(string query, SqlParameter[] parameters = null)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        if (parameters != null)
                            cmd.Parameters.AddRange(parameters);

                        conn.Open();
                        return cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Database error: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return 0;
            }
        }

        private static string GenerateRandomCode()
        {
            Random random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, 8)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        #endregion
    }
}
