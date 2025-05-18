using System;
using System.Drawing;
using System.Windows.Forms;
using System.Data;

namespace StudentFeedbackSystem
{
    public partial class FormAdminDashboard : Form
    {
        private TabControl tabControl;
        private DataGridView dgvCodes;
        private ComboBox cmbUserType;
        private Button btnGenerateCode;
        private TextBox txtGeneratedCode;

        public FormAdminDashboard()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // Form properties
            this.Text = "Admin Dashboard - Student Feedback System";
            this.Size = new Size(1000, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            // Create TabControl
            tabControl = new TabControl
            {
                Location = new Point(10, 10),
                Size = new Size(965, 540),
                Font = new Font("Segoe UI", 9F)
            };

            // Create Dashboard Tab
            TabPage tabDashboard = new TabPage("Dashboard");
            SetupDashboardTab(tabDashboard);

            // Create Code Generation Tab
            TabPage tabCodeGeneration = new TabPage("Code Generation");
            SetupCodeGenerationTab(tabCodeGeneration);

            // Add tabs to TabControl
            tabControl.TabPages.AddRange(new TabPage[] {
                tabDashboard,
                tabCodeGeneration
            });

            // Add TabControl to form
            this.Controls.Add(tabControl);

            this.ResumeLayout(false);
        }

        private void SetupDashboardTab(TabPage tab)
        {
            // Statistics Labels
            Label lblStats = new Label
            {
                Text = "System Statistics",
                Location = new Point(20, 20),
                Size = new Size(300, 30),
                Font = new Font("Segoe UI", 15F, FontStyle.Bold)
            };

            // Create statistics panels
            TableLayoutPanel statsPanel = new TableLayoutPanel
            {
                Location = new Point(20, 60),
                Size = new Size(900, 100),
                ColumnCount = 4,
                RowCount = 1,
                BackColor = Color.Transparent
            };

            // Add stat boxes
            AddStatBox(statsPanel, "Total Students", "0", 0);
            AddStatBox(statsPanel, "Total Teachers", "0", 1);
            AddStatBox(statsPanel, "Total Subjects", "0", 2);
            AddStatBox(statsPanel, "Total Feedback", "0", 3);

            // Add controls to tab
            tab.Controls.Add(lblStats);
            tab.Controls.Add(statsPanel);
        }

        private void AddStatBox(TableLayoutPanel panel, string title, string value, int column)
        {
            Panel statBox = new Panel
            {
                Size = new Size(200, 80),
                BackColor = Color.FromArgb(240, 240, 240),
                Margin = new Padding(10)
            };

            Label lblTitle = new Label
            {
                Text = title,
                Location = new Point(10, 10),
                Size = new Size(180, 20),
                Font = new Font("Segoe UI", 9F)
            };

            Label lblValue = new Label
            {
                Text = value,
                Location = new Point(10, 35),
                Size = new Size(180, 30),
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter
            };

            statBox.Controls.Add(lblTitle);
            statBox.Controls.Add(lblValue);
            panel.Controls.Add(statBox, column, 0);
        }

        private void SetupCodeGenerationTab(TabPage tab)
        {
            // User Type Selection
            Label lblUserType = new Label
            {
                Text = "Select User Type:",
                Location = new Point(20, 20),
                Size = new Size(100, 25),
                Font = new Font("Segoe UI", 9F)
            };

            cmbUserType = new ComboBox
            {
                Location = new Point(130, 20),
                Size = new Size(200, 25),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9F)
            };
            cmbUserType.Items.AddRange(new string[] { "Student", "Teacher", "Admin" });
            cmbUserType.SelectedIndex = 0;

            // Generate Code Button
            btnGenerateCode = new Button
            {
                Text = "Generate Code",
                Location = new Point(350, 20),
                Size = new Size(120, 25),
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9F),
                FlatStyle = FlatStyle.Flat
            };

            // Generated Code Display
            txtGeneratedCode = new TextBox
            {
                Location = new Point(490, 20),
                Size = new Size(200, 25),
                ReadOnly = true,
                Font = new Font("Segoe UI", 9F)
            };

            // Generated Codes Grid
            dgvCodes = new DataGridView
            {
                Location = new Point(20, 60),
                Size = new Size(900, 400),
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
            dgvCodes.Columns.AddRange(new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn 
                { 
                    Name = "Code",
                    HeaderText = "Login Code",
                    Width = 150
                },
                new DataGridViewTextBoxColumn 
                { 
                    Name = "UserType",
                    HeaderText = "User Type",
                    Width = 100
                },
                new DataGridViewTextBoxColumn 
                { 
                    Name = "Status",
                    HeaderText = "Status",
                    Width = 100
                },
                new DataGridViewTextBoxColumn 
                { 
                    Name = "GeneratedOn",
                    HeaderText = "Generated On",
                    Width = 150
                }
            });

            // Add event handlers
            btnGenerateCode.Click += new EventHandler(btnGenerateCode_Click);

            // Add controls to tab
            tab.Controls.AddRange(new Control[] {
                lblUserType,
                cmbUserType,
                btnGenerateCode,
                txtGeneratedCode,
                dgvCodes
            });
        }

        private void btnGenerateCode_Click(object sender, EventArgs e)
        {
            string userType = cmbUserType.SelectedItem.ToString();
            
            // Generate a random code (replace with your own logic)
            string code = GenerateRandomCode();
            
            // Display the generated code
            txtGeneratedCode.Text = code;

            // Add to grid (replace with database insertion)
            dgvCodes.Rows.Insert(0, code, userType, "Unused", DateTime.Now.ToString());

            MessageBox.Show($"New login code generated for {userType}:\n{code}", 
                "Code Generated", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private string GenerateRandomCode()
        {
            // Generate a random 8-character code
            Random random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, 8)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
