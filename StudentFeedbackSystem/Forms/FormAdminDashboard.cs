using System;
using System.Drawing;
using System.Windows.Forms;
using System.Data;
using StudentFeedbackSystem.Data;

namespace StudentFeedbackSystem.Forms
{
    public partial class FormAdminDashboard : Form
    {
        private TabControl tabControl;
        private DataGridView dgvCodes;
        private ComboBox cmbUserType;
        private Button btnGenerateCode;
        private TextBox txtGeneratedCode;
        private Label lblStudentCount;
        private Label lblTeacherCount;
        private Label lblSubjectCount;
        private Label lblFeedbackCount;
        private Timer refreshTimer;

        public FormAdminDashboard()
        {
            InitializeComponent();
            SetupRefreshTimer();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // Form properties
            this.Text = "Admin Dashboard";
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            // Tab Control
            tabControl = new TabControl
            {
                Location = new Point(20, 20),
                Size = new Size(740, 520),
                Font = new Font("Segoe UI", 9F)
            };

            // Statistics Tab
            TabPage tabStats = new TabPage("System Statistics");
            
            // Statistics Labels
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

            // Login Codes Tab
            TabPage tabCodes = new TabPage("Login Codes");

            // Code Generation Controls
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
                Size = new Size(80, 25),
                Font = new Font("Segoe UI", 9F)
            };

            cmbUserType = new ComboBox
            {
                Location = new Point(100, 60),
                Size = new Size(150, 25),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9F)
            };

            btnGenerateCode = new Button
            {
                Text = "Generate Code",
                Location = new Point(270, 60),
                Size = new Size(120, 25),
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9F),
                FlatStyle = FlatStyle.Flat
            };

            txtGeneratedCode = new TextBox
            {
                Location = new Point(410, 60),
                Size = new Size(150, 25),
                ReadOnly = true,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                TextAlign = HorizontalAlignment.Center
            };

            // Generated Codes Grid
            dgvCodes = new DataGridView
            {
                Location = new Point(20, 100),
                Size = new Size(680, 370),
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

            dgvCodes.Columns.AddRange(new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn { Name = "Code", HeaderText = "Login Code", Width = 100 },
                new DataGridViewTextBoxColumn { Name = "UserType", HeaderText = "User Type", Width = 100 },
                new DataGridViewTextBoxColumn { Name = "Status", HeaderText = "Status", Width = 80 },
                new DataGridViewTextBoxColumn { Name = "GeneratedOn", HeaderText = "Generated On", Width = 150 }
            });

            tabCodes.Controls.AddRange(new Control[] {
                lblGenerate,
                lblUserType,
                cmbUserType,
                btnGenerateCode,
                txtGeneratedCode,
                dgvCodes
            });

            // Add tabs to tab control
            tabControl.TabPages.AddRange(new TabPage[] { tabStats, tabCodes });

            // Add items to combo box
            cmbUserType.Items.AddRange(new string[] { "Student", "Teacher" });
            cmbUserType.SelectedIndex = 0;

            // Event handlers
            btnGenerateCode.Click += new EventHandler(btnGenerateCode_Click);
            this.Load += new EventHandler(FormAdminDashboard_Load);

            // Add tab control to form
            this.Controls.Add(tabControl);

            this.ResumeLayout(false);
        }

        private void SetupRefreshTimer()
        {
            refreshTimer = new Timer
            {
                Interval = 5000 // Refresh every 5 seconds
            };
            refreshTimer.Tick += (s, e) => LoadStatistics();
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
                string query = "EXEC sp_GenerateLoginCode @UserType";
                SqlParameter[] parameters = {
                    new SqlParameter("@UserType", userType)
                };

                DataTable dt = DBConnection.ExecuteQuery(query, parameters);
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

        private void FormAdminDashboard_Load(object sender, EventArgs e)
        {
            LoadStatistics();
            LoadGeneratedCodes();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            refreshTimer?.Stop();
            refreshTimer?.Dispose();
        }
    }
}
