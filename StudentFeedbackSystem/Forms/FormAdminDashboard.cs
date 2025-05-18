using System;
using System.Drawing;
using System.Windows.Forms;
using System.Data;
using System.Data.SqlClient;
using StudentFeedbackSystem.Data;

namespace StudentFeedbackSystem.Forms
{
    public partial class FormAdminDashboard : Form
    {
        private TabControl tabControl;
        private DataGridView dgvCodes;
        private DataGridView dgvSubjects;
        private ComboBox cmbUserType;
        private ComboBox cmbTeachers;
        private TextBox txtSubjectName;
        private Button btnGenerateCode;
        private Button btnAddSubject;
        private Button btnDeleteSubject;
        private TextBox txtGeneratedCode;
        private Label lblStudentCount;
        private Label lblTeacherCount;
        private Label lblSubjectCount;
        private Label lblFeedbackCount;
        private Timer refreshTimer;

        public FormAdminDashboard()
        {
            InitializeComponent();
            this.Load += new EventHandler(FormAdminDashboard_Load);
        }

        private void FormAdminDashboard_Load(object sender, EventArgs e)
        {
            LoadStatistics();
            LoadGeneratedCodes();
            LoadTeachers(); // Load teachers for the combobox
            LoadSubjects();
        }

        private void LoadTeachers()
        {
            try
            {
                DataTable dt = DBConnection.GetAvailableTeachers();
                
                // Clear existing items
                if (cmbTeachers.Items.Count > 0)
                    cmbTeachers.Items.Clear();

                // Set up the combobox
                cmbTeachers.DisplayMember = "UserName";
                cmbTeachers.ValueMember = "UserID";
                cmbTeachers.DataSource = dt;

                // Select the first item if available
                if (dt.Rows.Count > 0)
                    cmbTeachers.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading teachers: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // Form properties
            this.Text = "Admin Dashboard";
            this.Size = new Size(1000, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            // Tab Control
            tabControl = new TabControl
            {
                Location = new Point(20, 20),
                Size = new Size(940, 620),
                Font = new Font("Segoe UI", 9F)
            };

            // Create tabs
            TabPage tabStats = CreateStatisticsTab();
            TabPage tabCodes = CreateLoginCodesTab();
            TabPage tabSubjects = CreateSubjectsTab();

            // Add tabs to tab control
            tabControl.TabPages.AddRange(new TabPage[] { tabStats, tabCodes, tabSubjects });

            // Add tab control to form
            this.Controls.Add(tabControl);

            this.ResumeLayout(false);
        }

        private TabPage CreateStatisticsTab()
        {
            TabPage tabStats = new TabPage("System Statistics");
            
            Label lblStats = new Label
            {
                Text = "Current System Statistics",
                Location = new Point(20, 20),
                Size = new Size(680, 30),
                Font = new Font("Segoe UI", 12F, FontStyle.Bold)
            };

            lblStudentCount = new Label
            {
                Text = "Total Students: 0",
                Location = new Point(20, 70),
                Size = new Size(200, 25),
                Font = new Font("Segoe UI", 10F)
            };

            lblTeacherCount = new Label
            {
                Text = "Total Teachers: 0",
                Location = new Point(20, 105),
                Size = new Size(200, 25),
                Font = new Font("Segoe UI", 10F)
            };

            lblSubjectCount = new Label
            {
                Text = "Total Subjects: 0",
                Location = new Point(20, 140),
                Size = new Size(200, 25),
                Font = new Font("Segoe UI", 10F)
            };

            lblFeedbackCount = new Label
            {
                Text = "Total Feedback Submissions: 0",
                Location = new Point(20, 175),
                Size = new Size(200, 25),
                Font = new Font("Segoe UI", 10F)
            };

            tabStats.Controls.AddRange(new Control[] {
                lblStats,
                lblStudentCount,
                lblTeacherCount,
                lblSubjectCount,
                lblFeedbackCount
            });

            return tabStats;
        }

        private TabPage CreateLoginCodesTab()
        {
            TabPage tabCodes = new TabPage("Login Codes");

            Label lblGenerate = new Label
            {
                Text = "Generate New Login Code",
                Location = new Point(20, 20),
                Size = new Size(680, 30),
                Font = new Font("Segoe UI", 12F, FontStyle.Bold)
            };

            Label lblUserType = new Label
            {
                Text = "User Type:",
                Location = new Point(20, 60),
                Size = new Size(80, 25)
            };

            cmbUserType = new ComboBox
            {
                Location = new Point(100, 60),
                Size = new Size(150, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            btnGenerateCode = new Button
            {
                Text = "Generate Code",
                Location = new Point(270, 60),
                Size = new Size(120, 25),
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };

            txtGeneratedCode = new TextBox
            {
                Location = new Point(410, 60),
                Size = new Size(150, 25),
                ReadOnly = true,
                TextAlign = HorizontalAlignment.Center,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };

            dgvCodes = new DataGridView
            {
                Location = new Point(20, 100),
                Size = new Size(880, 470),
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                AllowUserToAddRows = false,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };

            dgvCodes.Columns.AddRange(new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn { Name = "Code", HeaderText = "Login Code" },
                new DataGridViewTextBoxColumn { Name = "UserType", HeaderText = "User Type" },
                new DataGridViewTextBoxColumn { Name = "Status", HeaderText = "Status" },
                new DataGridViewTextBoxColumn { Name = "GeneratedOn", HeaderText = "Generated On" }
            });

            cmbUserType.Items.AddRange(new string[] { "Student", "Teacher" });
            cmbUserType.SelectedIndex = 0;

            btnGenerateCode.Click += new EventHandler(btnGenerateCode_Click);

            tabCodes.Controls.AddRange(new Control[] {
                lblGenerate,
                lblUserType,
                cmbUserType,
                btnGenerateCode,
                txtGeneratedCode,
                dgvCodes
            });

            return tabCodes;
        }

        private TabPage CreateSubjectsTab()
        {
            TabPage tabSubjects = new TabPage("Manage Subjects");

            Label lblSubjects = new Label
            {
                Text = "Subject Management",
                Location = new Point(20, 20),
                Size = new Size(680, 30),
                Font = new Font("Segoe UI", 12F, FontStyle.Bold)
            };

            // Add Subject Controls
            Label lblTeacher = new Label
            {
                Text = "Teacher:",
                Location = new Point(20, 60),
                Size = new Size(80, 25),
                Font = new Font("Segoe UI", 9F)
            };

            cmbTeachers = new ComboBox
            {
                Location = new Point(100, 60),
                Size = new Size(200, 25),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9F)
            };

            Label lblSubjectName = new Label
            {
                Text = "Subject Name:",
                Location = new Point(320, 60),
                Size = new Size(90, 25),
                Font = new Font("Segoe UI", 9F)
            };

            txtSubjectName = new TextBox
            {
                Location = new Point(410, 60),
                Size = new Size(200, 25),
                Font = new Font("Segoe UI", 9F)
            };

            btnAddSubject = new Button
            {
                Text = "Add Subject",
                Location = new Point(620, 60),
                Size = new Size(100, 25),
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9F),
                FlatStyle = FlatStyle.Flat
            };

            btnDeleteSubject = new Button
            {
                Text = "Delete Subject",
                Location = new Point(730, 60),
                Size = new Size(100, 25),
                BackColor = Color.FromArgb(220, 53, 69),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9F),
                FlatStyle = FlatStyle.Flat
            };

            dgvSubjects = new DataGridView
            {
                Location = new Point(20, 100),
                Size = new Size(880, 470),
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                AllowUserToAddRows = false,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                Font = new Font("Segoe UI", 9F)
            };

            dgvSubjects.Columns.AddRange(new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn { Name = "SubjectID", HeaderText = "ID", Visible = false },
                new DataGridViewTextBoxColumn { Name = "SubjectName", HeaderText = "Subject Name" },
                new DataGridViewTextBoxColumn { Name = "TeacherName", HeaderText = "Teacher" },
                new DataGridViewTextBoxColumn { Name = "EnrollmentCount", HeaderText = "Enrollments" },
                new DataGridViewTextBoxColumn { Name = "FeedbackCount", HeaderText = "Feedback" }
            });

            btnAddSubject.Click += new EventHandler(btnAddSubject_Click);
            btnDeleteSubject.Click += new EventHandler(btnDeleteSubject_Click);

            tabSubjects.Controls.AddRange(new Control[] {
                lblSubjects,
                lblTeacher,
                cmbTeachers,
                lblSubjectName,
                txtSubjectName,
                btnAddSubject,
                btnDeleteSubject,
                dgvSubjects
            });

            return tabSubjects;
        }

        private void SetupRefreshTimer()
        {
            refreshTimer = new Timer { Interval = 5000 };
            refreshTimer.Tick += (s, e) => 
            {
                LoadStatistics();
                if (tabControl.SelectedTab.Text == "Manage Subjects")
                    LoadSubjects();
            };
            refreshTimer.Start();
        }

        private void LoadStatistics()
        {
            try
            {
                DataTable dt = DBConnection.GetSystemStats();
                if (dt.Rows.Count > 0)
                {
                    DataRow row = dt.Rows[0];
                    lblStudentCount.Text = $"Total Students: {row["StudentCount"]}";
                    lblTeacherCount.Text = $"Total Teachers: {row["TeacherCount"]}";
                    lblSubjectCount.Text = $"Total Subjects: {row["SubjectCount"]}";
                    lblFeedbackCount.Text = $"Total Feedback Submissions: {row["FeedbackCount"]}";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading statistics: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadGeneratedCodes()
        {
            try
            {
                string query = @"
                    SELECT Code, UserType, 
                           CASE WHEN IsUsed = 1 THEN 'Used' ELSE 'Unused' END as Status,
                           GeneratedOn
                    FROM LoginCodes
                    ORDER BY GeneratedOn DESC";

                DataTable dt = DBConnection.ExecuteQuery(query);
                dgvCodes.Rows.Clear();

                foreach (DataRow row in dt.Rows)
                {
                    dgvCodes.Rows.Add(
                        row["Code"],
                        row["UserType"],
                        row["Status"],
                        Convert.ToDateTime(row["GeneratedOn"]).ToString("yyyy-MM-dd HH:mm")
                    );
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading codes: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadSubjects()
        {
            try
            {
                DataTable dt = DBConnection.GetAllSubjects();
                dgvSubjects.Rows.Clear();

                foreach (DataRow row in dt.Rows)
                {
                    dgvSubjects.Rows.Add(
                        row["SubjectID"],
                        row["SubjectName"],
                        row["TeacherName"],
                        row["EnrollmentCount"],
                        row["FeedbackCount"]
                    );
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading subjects: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnGenerateCode_Click(object sender, EventArgs e)
        {
            try
            {
                if (cmbUserType.SelectedItem == null)
                {
                    MessageBox.Show("Please select a user type.", "Validation Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string userType = cmbUserType.SelectedItem.ToString();
                DataTable dt = DBConnection.GenerateLoginCode(userType);

                if (dt.Rows.Count > 0)
                {
                    string code = dt.Rows[0]["GeneratedCode"].ToString();
                    txtGeneratedCode.Text = code;
                    LoadGeneratedCodes();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating code: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnAddSubject_Click(object sender, EventArgs e)
        {
            try
            {
                if (cmbTeachers.SelectedValue == null)
                {
                    MessageBox.Show("Please select a teacher.", "Validation Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string subjectName = txtSubjectName.Text.Trim();
                if (string.IsNullOrEmpty(subjectName))
                {
                    MessageBox.Show("Please enter a subject name.", "Validation Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                int teacherId = Convert.ToInt32(cmbTeachers.SelectedValue);
                DBConnection.AddSubject(subjectName, teacherId);

                MessageBox.Show("Subject added successfully.", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                txtSubjectName.Clear();
                LoadSubjects();
                LoadStatistics();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding subject: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnDeleteSubject_Click(object sender, EventArgs e)
        {
            try
            {
                if (dgvSubjects.SelectedRows.Count == 0)
                {
                    MessageBox.Show("Please select a subject to delete.", "Validation Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                int subjectId = Convert.ToInt32(dgvSubjects.SelectedRows[0].Cells["SubjectID"].Value);
                int feedbackCount = Convert.ToInt32(dgvSubjects.SelectedRows[0].Cells["FeedbackCount"].Value);

                if (feedbackCount > 0)
                {
                    MessageBox.Show("Cannot delete subject that has feedback.", "Validation Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (MessageBox.Show("Are you sure you want to delete this subject?", "Confirm Delete",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    DBConnection.DeleteSubject(subjectId);

                    MessageBox.Show("Subject deleted successfully.", "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);

                    LoadSubjects();
                    LoadStatistics();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting subject: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
