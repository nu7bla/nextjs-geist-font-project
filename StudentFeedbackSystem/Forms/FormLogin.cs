using System;
using System.Data;
using System.Windows.Forms;
using System.Drawing;
using StudentFeedbackSystem.Data;

namespace StudentFeedbackSystem.Forms
{
    public partial class FormLogin : Form
    {
        private TextBox txtLoginCode;
        private ComboBox cmbUserType;
        private Button btnLogin;
        private Label lblTitle;
        private int loginAttempts = 0;
        private const int MaxLoginAttempts = 3;

        public FormLogin()
        {
            InitializeComponent();
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

            // Title Label
            lblTitle = new Label
            {
                Text = "Student Feedback System",
                Location = new Point(50, 20),
                Size = new Size(300, 30),
                Font = new Font("Segoe UI", 15F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter
            };

            // Login Code Label and TextBox
            Label lblLoginCode = new Label
            {
                Text = "Login Code:",
                Location = new Point(50, 80),
                Size = new Size(100, 20),
                Font = new Font("Segoe UI", 9F)
            };

            txtLoginCode = new TextBox
            {
                Location = new Point(150, 80),
                Size = new Size(200, 20),
                Font = new Font("Segoe UI", 9F),
                MaxLength = 50
            };

            // User Type Label and ComboBox
            Label lblUserType = new Label
            {
                Text = "User Type:",
                Location = new Point(50, 120),
                Size = new Size(100, 20),
                Font = new Font("Segoe UI", 9F)
            };

            cmbUserType = new ComboBox
            {
                Location = new Point(150, 120),
                Size = new Size(200, 20),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9F)
            };

            // Login Button
            btnLogin = new Button
            {
                Text = "Login",
                Location = new Point(150, 170),
                Size = new Size(100, 35),
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
            txtLoginCode.KeyPress += new KeyPressEventHandler(txtLoginCode_KeyPress);
            this.KeyPreview = true;
            this.KeyDown += new KeyEventHandler(FormLogin_KeyDown);

            // Add controls to form
            this.Controls.AddRange(new Control[] {
                lblTitle,
                lblLoginCode,
                txtLoginCode,
                lblUserType,
                cmbUserType,
                btnLogin
            });

            this.ResumeLayout(false);
        }

        private void FormLogin_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                btnLogin_Click(sender, e);
            }
        }

        private void txtLoginCode_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Allow only letters, numbers, and backspace
            if (!char.IsLetterOrDigit(e.KeyChar) && e.KeyChar != (char)Keys.Back)
            {
                e.Handled = true;
            }
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            if (loginAttempts >= MaxLoginAttempts)
            {
                MessageBox.Show("Maximum login attempts exceeded. Please try again later.", 
                    "Login Blocked", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                Application.Exit();
                return;
            }

            try
            {
                string loginCode = txtLoginCode.Text.Trim().ToUpper();
                string userType = cmbUserType.SelectedItem.ToString();

                if (string.IsNullOrEmpty(loginCode))
                {
                    MessageBox.Show("Please enter your login code.", "Validation Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtLoginCode.Focus();
                    return;
                }

                DataTable dt = DBConnection.ValidateLogin(loginCode, userType);

                if (dt != null && dt.Rows.Count > 0)
                {
                    int userId = Convert.ToInt32(dt.Rows[0]["UserID"]);
                    string userName = dt.Rows[0]["UserName"].ToString();
                    Form nextForm = null;

                    switch (userType)
                    {
                        case "Student":
                            nextForm = new FormStudentDashboard(userId);
                            break;
                        case "Teacher":
                            nextForm = new FormTeacherDashboard(userId);
                            break;
                        case "Admin":
                            nextForm = new FormAdminDashboard();
                            break;
                    }

                    if (nextForm != null)
                    {
                        this.Hide();
                        nextForm.FormClosed += (s, args) => this.Close();
                        nextForm.Show();
                    }
                }
                else
                {
                    loginAttempts++;
                    int remainingAttempts = MaxLoginAttempts - loginAttempts;
                    
                    MessageBox.Show(
                        $"Invalid login code or user type.\nRemaining attempts: {remainingAttempts}", 
                        "Login Failed",
                        MessageBoxButtons.OK, 
                        MessageBoxIcon.Error);

                    txtLoginCode.Clear();
                    txtLoginCode.Focus();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Login Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtLoginCode.Clear();
                txtLoginCode.Focus();
            }
        }
    }
}
