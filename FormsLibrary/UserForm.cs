using System;
using System.Drawing;
using System.Windows.Forms;
using Common.Models;
using DatabaseManager;

namespace FormsLibrary
{
    public partial class UserForm : Form
    {
        private TextBox txtUsername, txtPassword, txtFullName;
        private CheckBox chkIsActive;
        private Button btnSave, btnCancel;
        private User _user;
        private UserService _userService;

        public event EventHandler UserSaved;

        public UserForm(User user, UserService userService)
        {
            _user = user;
            _userService = userService;
            InitializeComponent();
            SetupForm();
            AddControls();
            LoadUserData();
        }

        private void SetupForm()
        {
            this.Text = _user == null ? "إضافة مستخدم جديد" : "تعديل بيانات المستخدم";
            this.Size = new Size(400, 300);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.RightToLeft = RightToLeft.Yes;
            this.RightToLeftLayout = true;
            this.BackColor = Color.White;
        }

        private void AddControls()
        {
            // اسم المستخدم
            AddLabelAndControl(50, "اسم المستخدم:", "txtUsername", out txtUsername, false);

            // كلمة المرور (تظهر فقط في حالة الإضافة)
            if (_user == null)
            {
                AddLabelAndControl(90, "كلمة المرور:", "txtPassword", out txtPassword, true);
            }

            // الاسم الكامل
            AddLabelAndControl(130, "الاسم الكامل:", "txtFullName", out txtFullName, false);

            // مفعل
            Label lblIsActive = new Label();
            lblIsActive.Text = "مفعل:";
            lblIsActive.Location = new Point(300, 170);
            lblIsActive.Size = new Size(50, 20);
            lblIsActive.Font = new Font("Tahoma", 9);
            lblIsActive.TextAlign = ContentAlignment.MiddleRight;
            this.Controls.Add(lblIsActive);

            chkIsActive = new CheckBox();
            chkIsActive.Location = new Point(250, 170);
            chkIsActive.Size = new Size(20, 20);
            chkIsActive.Checked = true;
            this.Controls.Add(chkIsActive);

            // زر الحفظ
            btnSave = new Button();
            btnSave.Text = "حفظ";
            btnSave.Location = new Point(200, 220);
            btnSave.Size = new Size(80, 30);
            btnSave.Font = new Font("Tahoma", 9, FontStyle.Bold);
            btnSave.BackColor = Color.LightGreen;
            btnSave.Click += BtnSave_Click;
            this.Controls.Add(btnSave);

            // زر الإلغاء
            btnCancel = new Button();
            btnCancel.Text = "إلغاء";
            btnCancel.Location = new Point(110, 220);
            btnCancel.Size = new Size(80, 30);
            btnCancel.Font = new Font("Tahoma", 9);
            btnCancel.BackColor = Color.LightCoral;
            btnCancel.Click += BtnCancel_Click;
            this.Controls.Add(btnCancel);
        }

        private void AddLabelAndControl(int yPos, string labelText, string controlName, out TextBox textBox, bool isPassword)
        {
            // Label
            Label label = new Label();
            label.Text = labelText;
            label.Location = new Point(300, yPos);
            label.Size = new Size(80, 20);
            label.Font = new Font("Tahoma", 9);
            label.TextAlign = ContentAlignment.MiddleRight;
            this.Controls.Add(label);

            // TextBox
            textBox = new TextBox();
            textBox.Name = controlName;
            textBox.Location = new Point(100, yPos);
            textBox.Size = new Size(190, 25);
            textBox.Font = new Font("Tahoma", 9);

            if (isPassword)
                textBox.PasswordChar = '*';

            this.Controls.Add(textBox);
        }

        private void LoadUserData()
        {
            if (_user != null)
            {
                txtUsername.Text = _user.Username;
                txtFullName.Text = _user.FullName;
                chkIsActive.Checked = _user.IsActive;

                // في حالة التعديل، لا يمكن تغيير اسم المستخدم
                txtUsername.Enabled = false;
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (!ValidateInput())
                return;

            try
            {
                if (_user == null)
                {
                    // إضافة مستخدم جديد
                    User newUser = new User
                    {
                        Username = txtUsername.Text.Trim(),
                        Password = txtPassword.Text, // سيتم تشفيرها في الخدمة
                        FullName = txtFullName.Text.Trim(),
                        IsActive = chkIsActive.Checked
                    };

                    bool success = _userService.AddUser(newUser);
                    if (success)
                    {
                        MessageBox.Show("تم إضافة المستخدم بنجاح", "نجاح", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        UserSaved?.Invoke(this, EventArgs.Empty);
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("فشل في إضافة المستخدم", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    // تعديل مستخدم موجود
                    _user.FullName = txtFullName.Text.Trim();
                    _user.IsActive = chkIsActive.Checked;

                    bool success = _userService.UpdateUser(_user);
                    if (success)
                    {
                        MessageBox.Show("تم تعديل بيانات المستخدم بنجاح", "نجاح", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        UserSaved?.Invoke(this, EventArgs.Empty);
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("فشل في تعديل بيانات المستخدم", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في حفظ البيانات: {ex.Message}", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(txtUsername.Text))
            {
                MessageBox.Show("يرجى إدخال اسم المستخدم", "تحذير", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtUsername.Focus();
                return false;
            }

            if (_user == null && string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                MessageBox.Show("يرجى إدخال كلمة المرور", "تحذير", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPassword.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtFullName.Text))
            {
                MessageBox.Show("يرجى إدخال الاسم الكامل", "تحذير", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtFullName.Focus();
                return false;
            }

            return true;
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}