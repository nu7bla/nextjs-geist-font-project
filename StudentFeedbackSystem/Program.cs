using System;
using System.Windows.Forms;
using StudentFeedbackSystem.Forms;

namespace StudentFeedbackSystem
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                // Test database connection before starting
                if (!Data.DBConnection.TestConnection())
                {
                    MessageBox.Show("Could not connect to the database. Please check your connection settings.",
                        "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                Application.Run(new FormLogin());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An unexpected error occurred: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
