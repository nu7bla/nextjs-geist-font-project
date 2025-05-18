using System;
using System.Drawing;
using System.Windows.Forms;
using System.Data;
using System.Data.SqlClient;
using StudentFeedbackSystem.Data;

namespace StudentFeedbackSystem.Forms
{
    public partial class FormTeacherDashboard : Form
    {
        private readonly int teacherId;
        private DataGridView dgvFeedback;
        private ComboBox cmbSubjects;
        private Label lblAverageRating;
        private Button btnRefresh;
        private Label lblWelcome;
        private Label lblFeedbackCount;
        private Timer refreshTimer;

        public FormTeacherDashboard(int teacherId)
        {
            this.teacherId = teacherId;
            InitializeComponent();
            SetupRefreshTimer();
            LoadTeacherName();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            LoadSubjects();
            LoadFeedbackData();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            if (refreshTimer != null)
            {
                refreshTimer.Stop();
                refreshTimer.Dispose();
            }
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // Form properties
            this.Text = "Teacher Dashboard";
            this.Size = new Size(1000, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            // Welcome Label
            lblWelcome = new Label
            {
                Text = "Welcome, Teacher",
                Location = new Point(20, 20),
                Size = new Size(400, 30),
                Font = new Font("Segoe UI", 15F, FontStyle.Bold)
            };

            // Subject Selection
            Label lblSubject = new Label
            {
                Text = "Select Subject:",
                Location = new Point(20, 70),
                Size = new Size(100, 25),
                Font = new Font("Segoe UI", 9F)
            };

            cmbSubjects = new ComboBox
            {
                Location = new Point(120, 70),
                Size = new Size(250, 25),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9F)
            };

            btnRefresh = new Button
            {
                Text = "Refresh",
                Location = new Point(380, 70),
                Size = new Size(100, 25),
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9F),
                FlatStyle = FlatStyle.Flat
            };

            lblAverageRating = new Label
            {
                Text = "Average Rating: N/A",
                Location = new Point(500, 70),
                Size = new Size(200, 25),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };

            lblFeedbackCount = new Label
            {
                Text = "Total Feedback: 0",
                Location = new Point(720, 70),
                Size = new Size(200, 25),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };

            // Feedback Grid
            dgvFeedback = new DataGridView
            {
                Location = new Point(20, 110),
                Size = new Size(940, 420),
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                Font = new Font("Segoe UI", 9F)
            };

            dgvFeedback.Columns.AddRange(new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn { Name = "Date", HeaderText = "Date", Width = 120 },
                new DataGridViewTextBoxColumn { Name = "Q1", HeaderText = "Teaching Clarity", Width = 100 },
                new DataGridViewTextBoxColumn { Name = "Q2", HeaderText = "Course Content", Width = 100 },
                new DataGridViewTextBoxColumn { Name = "Q3", HeaderText = "Teaching Methods", Width = 100 },
                new DataGridViewTextBoxColumn { Name = "Q4", HeaderText = "Availability", Width = 100 },
                new DataGridViewTextBoxColumn { Name = "Q5", HeaderText = "Overall Experience", Width = 100 },
                new DataGridViewTextBoxColumn { Name = "Comments", HeaderText = "Comments", Width = 300 }
            });

            // Event handlers
            cmbSubjects.SelectedIndexChanged += (s, e) => LoadFeedbackData();
            btnRefresh.Click += (s, e) => LoadFeedbackData();

            // Add controls to form
            this.Controls.AddRange(new Control[] {
                lblWelcome,
                lblSubject,
                cmbSubjects,
                btnRefresh,
                lblAverageRating,
                lblFeedbackCount,
                dgvFeedback
            });

            this.ResumeLayout(false);
        }

        private void SetupRefreshTimer()
        {
            refreshTimer = new Timer
            {
                Interval = 30000 // Refresh every 30 seconds
            };
            refreshTimer.Tick += (s, e) => LoadFeedbackData();
            refreshTimer.Start();
        }

        private void LoadTeacherName()
        {
            try
            {
                string query = "SELECT UserName FROM Users WHERE UserID = @TeacherID";
                SqlParameter[] parameters = {
                    new SqlParameter("@TeacherID", teacherId)
                };

                DataTable dt = DBConnection.ExecuteQuery(query, parameters);
                if (dt.Rows.Count > 0)
                {
                    lblWelcome.Text = $"Welcome, {dt.Rows[0]["UserName"]}";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading teacher info: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadSubjects()
        {
            try
            {
                DataTable dt = DBConnection.GetTeacherSubjects(teacherId);
                cmbSubjects.Items.Clear();

                foreach (DataRow row in dt.Rows)
                {
                    string subjectName = row["SubjectName"].ToString();
                    int feedbackCount = Convert.ToInt32(row["FeedbackCount"]);
                    cmbSubjects.Items.Add(new SubjectItem(subjectName, feedbackCount));
                }

                if (cmbSubjects.Items.Count > 0)
                {
                    cmbSubjects.SelectedIndex = 0;
                }
                else
                {
                    MessageBox.Show("No subjects found for this teacher.", "Information",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading subjects: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadFeedbackData()
        {
            if (cmbSubjects.SelectedItem == null) return;

            try
            {
                SubjectItem selectedSubject = (SubjectItem)cmbSubjects.SelectedItem;
                DataTable dt = DBConnection.GetTeacherFeedback(teacherId, selectedSubject.Name);
                
                dgvFeedback.Rows.Clear();
                double totalRating = 0;
                int ratingCount = 0;

                foreach (DataRow row in dt.Rows)
                {
                    dgvFeedback.Rows.Add(
                        Convert.ToDateTime(row["SubmittedOn"]).ToString("yyyy-MM-dd HH:mm"),
                        row["Q1"],
                        row["Q2"],
                        row["Q3"],
                        row["Q4"],
                        row["Q5"],
                        row["Comments"]
                    );

                    double feedbackAvg = (
                        Convert.ToDouble(row["Q1"]) +
                        Convert.ToDouble(row["Q2"]) +
                        Convert.ToDouble(row["Q3"]) +
                        Convert.ToDouble(row["Q4"]) +
                        Convert.ToDouble(row["Q5"])
                    ) / 5.0;

                    totalRating += feedbackAvg;
                    ratingCount++;
                }

                double overallAverage = ratingCount > 0 ? totalRating / ratingCount : 0;
                lblAverageRating.Text = $"Average Rating: {overallAverage:F2}/5.0";
                lblFeedbackCount.Text = $"Total Feedback: {ratingCount}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading feedback: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private class SubjectItem
        {
            public string Name { get; }
            public int FeedbackCount { get; }

            public SubjectItem(string name, int feedbackCount)
            {
                Name = name;
                FeedbackCount = feedbackCount;
            }

            public override string ToString()
            {
                return $"{Name} ({FeedbackCount} feedback)";
            }
        }
    }
}
