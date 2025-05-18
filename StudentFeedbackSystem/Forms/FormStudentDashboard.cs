using System;
using System.Drawing;
using System.Windows.Forms;
using System.Data;
using StudentFeedbackSystem.Data;

namespace StudentFeedbackSystem.Forms
{
    public partial class FormStudentDashboard : Form
    {
        private readonly int studentId;
        private ListBox lstSubjects;
        private Button btnGiveFeedback;
        private Label lblWelcome;
        private Label lblInstructions;
        private Timer refreshTimer;

        public FormStudentDashboard(int studentId)
        {
            this.studentId = studentId;
            InitializeComponent();
            LoadStudentName();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // Form properties
            this.Text = "Student Dashboard";
            this.Size = new Size(800, 500);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            // Welcome Label
            lblWelcome = new Label
            {
                Text = "Welcome Student",
                Location = new Point(20, 20),
                Size = new Size(400, 30),
                Font = new Font("Segoe UI", 15F, FontStyle.Bold)
            };

            // Instructions Label
            lblInstructions = new Label
            {
                Text = "Select a subject and click 'Give Feedback' to provide your feedback.\nSubjects with existing feedback are marked.",
                Location = new Point(20, 60),
                Size = new Size(500, 40),
                Font = new Font("Segoe UI", 9F)
            };

            // Subjects Label
            Label lblSubjects = new Label
            {
                Text = "Your Enrolled Subjects:",
                Location = new Point(20, 110),
                Size = new Size(200, 20),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };

            // Subjects ListBox
            lstSubjects = new ListBox
            {
                Location = new Point(20, 140),
                Size = new Size(400, 250),
                Font = new Font("Segoe UI", 10F),
                BorderStyle = BorderStyle.FixedSingle,
                DrawMode = DrawMode.OwnerDrawFixed
            };

            // Give Feedback Button
            btnGiveFeedback = new Button
            {
                Text = "Give Feedback",
                Location = new Point(20, 400),
                Size = new Size(150, 35),
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat
            };

            // Event handlers
            lstSubjects.DrawItem += new DrawItemEventHandler(lstSubjects_DrawItem);
            btnGiveFeedback.Click += new EventHandler(btnGiveFeedback_Click);
            this.Load += new EventHandler(FormStudentDashboard_Load);

            // Add controls to form
            this.Controls.AddRange(new Control[] {
                lblWelcome,
                lblInstructions,
                lblSubjects,
                lstSubjects,
                btnGiveFeedback
            });

            // Set up refresh timer
            refreshTimer = new Timer
            {
                Interval = 30000 // Refresh every 30 seconds
            };
            refreshTimer.Tick += (s, e) => LoadSubjects();
            refreshTimer.Start();

            this.ResumeLayout(false);
        }

        private void LoadStudentName()
        {
            try
            {
                string query = "SELECT UserName FROM Users WHERE UserID = @StudentID";
                SqlParameter[] parameters = {
                    new SqlParameter("@StudentID", studentId)
                };

                DataTable dt = DBConnection.ExecuteQuery(query, parameters);
                if (dt.Rows.Count > 0)
                {
                    lblWelcome.Text = $"Welcome, {dt.Rows[0]["UserName"]}";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading student info: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadSubjects()
        {
            try
            {
                DataTable dt = DBConnection.GetStudentSubjects(studentId);
                
                // Store selected index
                int selectedIndex = lstSubjects.SelectedIndex;
                
                lstSubjects.Items.Clear();
                foreach (DataRow row in dt.Rows)
                {
                    SubjectItem item = new SubjectItem
                    {
                        SubjectId = Convert.ToInt32(row["SubjectID"]),
                        SubjectName = row["SubjectName"].ToString(),
                        HasFeedback = Convert.ToBoolean(row["HasFeedback"])
                    };
                    lstSubjects.Items.Add(item);
                }

                // Restore selected index if possible
                if (selectedIndex >= 0 && selectedIndex < lstSubjects.Items.Count)
                {
                    lstSubjects.SelectedIndex = selectedIndex;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading subjects: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void lstSubjects_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;

            e.DrawBackground();
            Graphics g = e.Graphics;

            SubjectItem item = (SubjectItem)lstSubjects.Items[e.Index];
            Color textColor = item.HasFeedback ? Color.Gray : Color.Black;
            string text = item.ToString();

            using (Font font = new Font(e.Font, item.HasFeedback ? FontStyle.Italic : FontStyle.Regular))
            {
                g.DrawString(text, font, new SolidBrush(textColor), e.Bounds);
            }

            e.DrawFocusRectangle();
        }

        private void btnGiveFeedback_Click(object sender, EventArgs e)
        {
            if (lstSubjects.SelectedItem == null)
            {
                MessageBox.Show("Please select a subject first.", "Selection Required",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            SubjectItem selectedSubject = (SubjectItem)lstSubjects.SelectedItem;
            
            if (selectedSubject.HasFeedback)
            {
                MessageBox.Show("You have already submitted feedback for this subject.", 
                    "Feedback Exists", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            FormFeedback feedbackForm = new FormFeedback(studentId, selectedSubject.SubjectId);
            feedbackForm.FormClosed += (s, args) => 
            {
                if (feedbackForm.DialogResult == DialogResult.OK)
                {
                    LoadSubjects(); // Refresh the list after successful feedback submission
                }
            };
            feedbackForm.ShowDialog();
        }

        private void FormStudentDashboard_Load(object sender, EventArgs e)
        {
            LoadSubjects();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            refreshTimer?.Stop();
            refreshTimer?.Dispose();
        }

        private class SubjectItem
        {
            public int SubjectId { get; set; }
            public string SubjectName { get; set; }
            public bool HasFeedback { get; set; }

            public override string ToString()
            {
                return HasFeedback ? 
                    $"{SubjectName} (Feedback Submitted)" : 
                    SubjectName;
            }
        }
    }
}
