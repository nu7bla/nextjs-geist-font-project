using System;
using System.Drawing;
using System.Windows.Forms;

namespace StudentFeedbackSystem
{
    public partial class FormStudentDashboard : Form
    {
        private ListBox lstSubjects;
        private Button btnGiveFeedback;
        private Label lblWelcome;

        public FormStudentDashboard()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // Form properties
            this.Text = "Student Dashboard - Student Feedback System";
            this.Size = new Size(800, 500);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            // Welcome Label
            lblWelcome = new Label
            {
                Text = "Welcome Student",
                Location = new Point(20, 20),
                Size = new Size(300, 30),
                Font = new Font("Segoe UI", 15F, FontStyle.Bold)
            };

            // Subjects Label
            Label lblSubjects = new Label
            {
                Text = "Your Enrolled Subjects:",
                Location = new Point(20, 70),
                Size = new Size(200, 20),
                Font = new Font("Segoe UI", 10F)
            };

            // Subjects ListBox
            lstSubjects = new ListBox
            {
                Location = new Point(20, 100),
                Size = new Size(400, 300),
                Font = new Font("Segoe UI", 10F),
                BorderStyle = BorderStyle.FixedSingle
            };

            // Give Feedback Button
            btnGiveFeedback = new Button
            {
                Text = "Give Feedback",
                Location = new Point(20, 410),
                Size = new Size(150, 35),
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat
            };

            // Add dummy data (replace with actual data from database)
            lstSubjects.Items.AddRange(new string[] {
                "Mathematics",
                "Physics",
                "Computer Science",
                "English Literature"
            });

            // Add event handlers
            btnGiveFeedback.Click += new EventHandler(btnGiveFeedback_Click);
            this.Load += new EventHandler(FormStudentDashboard_Load);

            // Add controls to form
            this.Controls.AddRange(new Control[] {
                lblWelcome,
                lblSubjects,
                lstSubjects,
                btnGiveFeedback
            });

            this.ResumeLayout(false);
        }

        private void FormStudentDashboard_Load(object sender, EventArgs e)
        {
            // TODO: Load enrolled subjects from database
            // For now using dummy data added in InitializeComponent
        }

        private void btnGiveFeedback_Click(object sender, EventArgs e)
        {
            if (lstSubjects.SelectedItem == null)
            {
                MessageBox.Show("Please select a subject first.", "Selection Required",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string selectedSubject = lstSubjects.SelectedItem.ToString();
            FormFeedback feedbackForm = new FormFeedback(selectedSubject);
            feedbackForm.ShowDialog();
        }
    }
}
