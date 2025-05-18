using System;
using System.Drawing;
using System.Windows.Forms;

namespace StudentFeedbackSystem
{
    public partial class FormFeedback : Form
    {
        private string subjectName;
        private GroupBox[] questionGroups;
        private RadioButton[][] ratingButtons;
        private TextBox txtComments;
        private Button btnSubmit;

        public FormFeedback(string subject)
        {
            this.subjectName = subject;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // Form properties
            this.Text = $"Feedback - {subjectName}";
            this.Size = new Size(700, 800);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.AutoScroll = true;

            // Title Label
            Label lblTitle = new Label
            {
                Text = $"Feedback Form - {subjectName}",
                Location = new Point(20, 20),
                Size = new Size(400, 30),
                Font = new Font("Segoe UI", 15F, FontStyle.Bold)
            };

            // Initialize arrays for questions
            string[] questions = new string[]
            {
                "How would you rate the clarity of teaching?",
                "How would you rate the course content?",
                "How effective were the teaching methods?",
                "How would you rate the instructor's availability for doubts?",
                "How would you rate the overall learning experience?"
            };

            // Create question groups and rating buttons
            questionGroups = new GroupBox[5];
            ratingButtons = new RadioButton[5][];
            int yPosition = 70;

            for (int i = 0; i < 5; i++)
            {
                // Create group box for each question
                questionGroups[i] = new GroupBox
                {
                    Text = questions[i],
                    Location = new Point(20, yPosition),
                    Size = new Size(640, 80),
                    Font = new Font("Segoe UI", 9F)
                };

                // Create radio buttons for ratings
                ratingButtons[i] = new RadioButton[5];
                for (int j = 0; j < 5; j++)
                {
                    ratingButtons[i][j] = new RadioButton
                    {
                        Text = (j + 1).ToString(),
                        Location = new Point(50 + (j * 120), 30),
                        Size = new Size(100, 20),
                        Tag = j + 1
                    };
                    questionGroups[i].Controls.Add(ratingButtons[i][j]);
                }

                yPosition += 100;
            }

            // Comments section
            Label lblComments = new Label
            {
                Text = "Additional Comments:",
                Location = new Point(20, yPosition),
                Size = new Size(200, 20),
                Font = new Font("Segoe UI", 9F)
            };

            txtComments = new TextBox
            {
                Location = new Point(20, yPosition + 30),
                Size = new Size(640, 100),
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                Font = new Font("Segoe UI", 9F)
            };

            // Submit button
            btnSubmit = new Button
            {
                Text = "Submit Feedback",
                Location = new Point(20, yPosition + 150),
                Size = new Size(150, 35),
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat
            };

            // Add event handlers
            btnSubmit.Click += new EventHandler(btnSubmit_Click);

            // Add controls to form
            this.Controls.Add(lblTitle);
            foreach (GroupBox group in questionGroups)
            {
                this.Controls.Add(group);
            }
            this.Controls.Add(lblComments);
            this.Controls.Add(txtComments);
            this.Controls.Add(btnSubmit);

            this.ResumeLayout(false);
        }

        private void btnSubmit_Click(object sender, EventArgs e)
        {
            // Validate all questions are answered
            for (int i = 0; i < 5; i++)
            {
                bool questionAnswered = false;
                foreach (RadioButton rb in ratingButtons[i])
                {
                    if (rb.Checked)
                    {
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

            // Collect ratings
            int[] ratings = new int[5];
            for (int i = 0; i < 5; i++)
            {
                foreach (RadioButton rb in ratingButtons[i])
                {
                    if (rb.Checked)
                    {
                        ratings[i] = Convert.ToInt32(rb.Tag);
                        break;
                    }
                }
            }

            string comments = txtComments.Text.Trim();

            // TODO: Save feedback to database
            MessageBox.Show("Thank you for your feedback!", "Success",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.Close();
        }
    }
}
