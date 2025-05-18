using System;
using System.Drawing;
using System.Windows.Forms;
using System.Data;

namespace StudentFeedbackSystem
{
    public partial class FormTeacherDashboard : Form
    {
        private DataGridView dgvFeedback;
        private ComboBox cmbSubjects;
        private Label lblAverageRating;
        private Button btnRefresh;

        public FormTeacherDashboard()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // Form properties
            this.Text = "Teacher Dashboard - Student Feedback System";
            this.Size = new Size(1000, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            // Welcome Label
            Label lblWelcome = new Label
            {
                Text = "Teacher Dashboard",
                Location = new Point(20, 20),
                Size = new Size(300, 30),
                Font = new Font("Segoe UI", 15F, FontStyle.Bold)
            };

            // Subject Filter
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

            // Refresh Button
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

            // Average Rating Label
            lblAverageRating = new Label
            {
                Text = "Average Rating: N/A",
                Location = new Point(500, 70),
                Size = new Size(200, 25),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };

            // Feedback DataGridView
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
                MultiSelect = false
            };

            // Configure DataGridView columns
            dgvFeedback.Columns.AddRange(new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn 
                { 
                    Name = "Date",
                    HeaderText = "Date",
                    Width = 100
                },
                new DataGridViewTextBoxColumn 
                { 
                    Name = "Q1",
                    HeaderText = "Teaching Clarity",
                    Width = 100
                },
                new DataGridViewTextBoxColumn 
                { 
                    Name = "Q2",
                    HeaderText = "Course Content",
                    Width = 100
                },
                new DataGridViewTextBoxColumn 
                { 
                    Name = "Q3",
                    HeaderText = "Teaching Methods",
                    Width = 100
                },
                new DataGridViewTextBoxColumn 
                { 
                    Name = "Q4",
                    HeaderText = "Availability",
                    Width = 100
                },
                new DataGridViewTextBoxColumn 
                { 
                    Name = "Q5",
                    HeaderText = "Overall Experience",
                    Width = 100
                },
                new DataGridViewTextBoxColumn 
                { 
                    Name = "Comments",
                    HeaderText = "Comments",
                    Width = 300
                }
            });

            // Add dummy data (replace with actual data from database)
            cmbSubjects.Items.AddRange(new string[] {
                "Mathematics",
                "Physics",
                "Computer Science"
            });

            // Add event handlers
            this.Load += new EventHandler(FormTeacherDashboard_Load);
            cmbSubjects.SelectedIndexChanged += new EventHandler(cmbSubjects_SelectedIndexChanged);
            btnRefresh.Click += new EventHandler(btnRefresh_Click);

            // Add controls to form
            this.Controls.AddRange(new Control[] {
                lblWelcome,
                lblSubject,
                cmbSubjects,
                btnRefresh,
                lblAverageRating,
                dgvFeedback
            });

            this.ResumeLayout(false);
        }

        private void FormTeacherDashboard_Load(object sender, EventArgs e)
        {
            if (cmbSubjects.Items.Count > 0)
            {
                cmbSubjects.SelectedIndex = 0;
            }
            LoadFeedbackData();
        }

        private void cmbSubjects_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadFeedbackData();
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadFeedbackData();
        }

        private void LoadFeedbackData()
        {
            // TODO: Load actual feedback data from database
            // For now, adding sample data
            dgvFeedback.Rows.Clear();

            if (cmbSubjects.SelectedItem != null)
            {
                // Sample data
                dgvFeedback.Rows.Add(
                    DateTime.Now.ToShortDateString(),
                    4, 5, 4, 5, 4,
                    "Great teaching methods!"
                );

                dgvFeedback.Rows.Add(
                    DateTime.Now.AddDays(-1).ToShortDateString(),
                    5, 4, 5, 4, 5,
                    "Very helpful instructor"
                );

                // Calculate and display average rating
                double avgRating = 4.5; // Replace with actual calculation
                lblAverageRating.Text = $"Average Rating: {avgRating:F1}/5.0";
            }
        }
    }
}
