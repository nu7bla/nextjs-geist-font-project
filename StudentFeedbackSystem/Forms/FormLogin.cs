using System;
using System.Data;
using System.Windows.Forms;
using System.Drawing;

namespace StudentFeedbackSystem
{
    public partial class FormLogin : Form
    {
        private TextBox txtLoginCode;
        private ComboBox cmbUserType;
        private Button btnLogin;

        public FormLogin()
        {
            InitializeComponent();
            SetupForm();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            
            // Form properties
            this.Text = "Student Feedback System - Login";
            this.Size = new Size(400, 300);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            // Create controls
            Label lblLoginCode = new Label
            {
                Text = "Login Code:",
                Location = new Point(50, 50),
                Size = new Size(100, 20),
                Font = new Font("Segoe UI", 9F)
            };

            txtLoginCode = new TextBox
            {
                Location = new Point(150, 50),
                Size = new Size(200, 20),
                Font = new Font("Segoe UI", 9F)
            };

            Label lblUserType = new Label
            {
                Text = "User Type:",
                Location = new Point(50, 90),
                Size = new Size(100, 20),
                Font = new Font("Segoe UI", 9F)
            };

            cmbUserType = new ComboBox
            {
                Location = new Point(150, 90),
                Size = new Size(200, 20),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9F)
            };

            btnLogin = new Button
            {
                Text = "Login",
                Location = new Point(150, 130),
                Size = new Size(100, 30),
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat
            };

            // Add items to combo box
            cmbUserType.Items.AddRange(new string[] { "Student", "Teacher", "Admin" });
            cmbUserType.SelectedIndex = 0;

            // Add event handlers
            btnLogin.Click += new EventHandler(btnLogin_Click);

            // Add controls to form
            this.Controls.AddRange(new Control[] {
                lblLoginCode,
                txtLoginCode,
                lblUserType,
                cmbUserType,
                btnLogin
            });

            this.ResumeLayout(false);
        }

        private void SetupForm()
        {
            // Additional setup if needed
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            string loginCode = txtLoginCode.Text.Trim();
            string userType = cmbUserType.SelectedItem.ToString();

            if (string.IsNullOrEmpty(loginCode))
            {
                MessageBox.Show("Please enter your login code.", "Validation Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // TODO: Add authentication logic here
            // For now, just show different forms based on user type
            Form nextForm = null;

            switch (userType)
            {
                case "Student":
                    nextForm = new FormStudentDashboard();
                    break;
                case "Teacher":
                    nextForm = new FormTeacherDashboard();
                    break;
                case "Admin":
                    nextForm = new FormAdminDashboard();
                    break;
            }

            if (nextForm != null)
            {
                this.Hide();
                nextForm.ShowDialog();
                this.Close();
            }
        }
    }
}
