using System;
using System.Drawing;
using System.Windows.Forms;
using System.Data;
using StudentFeedbackSystem.Data;

namespace StudentFeedbackSystem.Forms
{
    public partial class FormFeedback : Form
    {
        private readonly int studentId;
        private readonly int subjectId;
        private string subjectName;
        private GroupBox[] questionGroups;
        private RadioButton[][] ratingButtons;
        private TextBox txtComments;
        private Button btnSubmit;
        private Button btnCancel;
        private Label lblSubject;

        private readonly string[] questions = {
            "How would you rate the clarity of teaching?",
            "How would you rate the course content and materials?",
            "How effective were the teaching methods used?",
            "How would you rate the teacher's availability for help?",
            "What is your overall experience with this subject?"
        };

        public FormFeedback(int studentId, int subjectId)
        {
            this.studentId = studentId;
            this.subjectId = subjectId;
            
            // Check for existing feedback before initializing
            if (DBConnection.HasExistingFeedback(studentId, subjectId))
            {
                MessageBox.Show("You have already submitted feedback for this subject.",
                    "Duplicate Feedback", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this.DialogResult = DialogResult.Cancel;
                this.Close();
                return;
            }

            LoadSubjectName();
            InitializeComponent();
        }

        private void LoadSubjectName()
        {
            try
            {
                string query = "SELECT SubjectName FROM Subjects WHERE SubjectID = @SubjectID";
                SqlParameter[] parameters = {
                    new SqlParameter("@SubjectID", subjectId)
                };

                DataTable dt = DBConnection.ExecuteQuery(query, parameters);
                if (dt.Rows.Count > 0)
                {
                    subjectName = dt.Rows[0]["SubjectName"].ToString();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading subject info: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            }
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // Form properties
            this.Text = "Submit Feedback";
            this.Size = new Size(600, 700);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Subject Label
            lblSubject = new Label
            {
                Text = $"Feedback for {subjectName}",
                Location = new Point(20, 20),
                Size = new Size(540, 30),
                Font = new Font("Segoe UI", 12F, FontStyle.Bold)
            };

            // Instructions
            Label lblInstructions = new Label
            {
                Text = "Please rate each aspect from 1 (Poor) to 5 (Excellent)",
                Location = new Point(20, 60),
                Size = new Size(540, 20),
                Font = new Font("Segoe UI", 9F)
            };

            // Initialize arrays
            questionGroups = new GroupBox[5];
            ratingButtons = new RadioButton[5][];

            // Create question groups
            int yPos = 90;
            for (int i = 0; i < 5; i++)
            {
                questionGroups[i] = new GroupBox
                {
                    Text = questions[i],
                    Location = new Point(20, yPos),
                    Size = new Size(540, 60),
                    Font = new Font("Segoe UI", 9F)
                };

                ratingButtons[i] = new RadioButton[5];
                int xPos = 20;
                for (int j = 0; j < 5; j++)
                {
                    ratingButtons[i][j] = new RadioButton
                    {
                        Text = (j + 1).ToString(),
                        Location = new Point(xPos, 25),
                        Size = new Size(90, 20),
                        Tag = j + 1
                    };
                    questionGroups[i].Controls.Add(ratingButtons[i][j]);
                    xPos += 100;
                }

                this.Controls.Add(questionGroups[i]);
                yPos += 80;
            }

            // Comments section
            Label lblComments = new Label
            {
                Text = "Additional Comments (Optional):",
                Location = new Point(20, yPos),
                Size = new Size(540, 20),
                Font = new Font("Segoe UI", 9F)
            };

            txtComments = new TextBox
            {
                Location = new Point(20, yPos + 25),
                Size = new Size(540, 100),
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                Font = new Font("Segoe UI", 9F)
            };

            // Buttons
            btnSubmit = new Button
            {
                Text = "Submit Feedback",
                Location = new Point(320, yPos + 140),
                Size = new Size(120, 35),
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat
            };

            btnCancel = new Button
            {
                Text = "Cancel",
                Location = new Point(450, yPos + 140),
                Size = new Size(100, 35),
                Font = new Font("Segoe UI", 9F),
                FlatStyle = FlatStyle.Flat
            };

            // Event handlers
            btnSubmit.Click += new EventHandler(btnSubmit_Click);
            btnCancel.Click += (s, e) => this.Close();

            // Add remaining controls
            this.Controls.AddRange(new Control[] {
                lblSubject,
                lblInstructions,
                lblComments,
                txtComments,
                btnSubmit,
                btnCancel
            });

            this.ResumeLayout(false);
        }

        private void btnSubmit_Click(object sender, EventArgs e)
        {
            try
            {
                // Validate all questions are answered
                int[] ratings = new int[5];
                for (int i = 0; i < 5; i++)
                {
                    bool questionAnswered = false;
                    foreach (RadioButton rb in ratingButtons[i])
                    {
                        if (rb.Checked)
                        {
                            ratings[i] = Convert.ToInt32(rb.Tag);
                            questionAnswered = true;
                            break;
                        }
                    }

                    if (!questionAnswered)
                    {
                        MessageBox.Show($"Please answer question {i + 1}.", "Validation Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }

                string comments = txtComments.Text.Trim();

                // Submit feedback
                if (DBConnection.SubmitFeedback(studentId, subjectId, ratings, comments))
                {
                    MessageBox.Show("Thank you for your feedback!", "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error submitting feedback: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
