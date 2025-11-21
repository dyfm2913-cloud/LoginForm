using System;
using System.Drawing;
using System.Windows.Forms;
using System.Data.SqlClient;
using Common.Models;
using DatabaseManager;
using SettingsManager;
using FormsLibrary;


namespace LoginForm
{
    public partial class LoginForm : Form
    {
        private TextBox txtUsername;
        private TextBox txtPassword;
        private Button btnLogin;
        private Button btnSettings;
        private Button btnExit;
        private SettingsService _settingsService;
        private ConnectionSettings _connectionSettings;
        private DatabaseService _databaseService;
        private UserService _userService;

        public LoginForm()
        {
            InitializeComponent();
            SetupForm();
            AddControls();
            InitializeServices();
        }

        private void InitializeServices()
        {
            try
            {
                _settingsService = new SettingsService();
                _connectionSettings = _settingsService.LoadSettings();

                if (_connectionSettings != null)
                {
                    _databaseService = new DatabaseService(_connectionSettings);
                    _userService = new UserService(_databaseService);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في تهيئة الخدمات: {ex.Message}", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }



        private void TryCreateDefaultUser()
        {
            try
            {
                // اختبار الاتصال أولاً
                if (_databaseService.TestConnection(Common.Enums.DatabaseType.SystemDatabase))
                {
                    // محاولة إنشاء المستخدم الافتراضي
                    bool created = _userService.CreateDefaultUser();
                    if (created)
                    {
                        MessageBox.Show("تم إنشاء مستخدم افتراضي (1/1) بنجاح", "معلومة", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                // لا تعرض رسالة خطأ - قد يكون الجدول موجوداً بالفعل
                Console.WriteLine($"ملاحظة: {ex.Message}");
            }
        }

        private void SetupForm()
        {
            this.Text = "نظام تسجيل الدخول";
            this.Size = new Size(400, 350);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.RightToLeft = RightToLeft.Yes;
            this.RightToLeftLayout = true;
            this.BackColor = Color.White;
        }

        private void AddControls()
        {
            // Label: اسم المستخدم
            Label lblUsername = new Label();
            lblUsername.Text = "اسم المستخدم:";
            lblUsername.Location = new Point(250, 50);
            lblUsername.Size = new Size(100, 20);
            lblUsername.Font = new Font("Tahoma", 10);
            lblUsername.TextAlign = ContentAlignment.MiddleRight;
            this.Controls.Add(lblUsername);

            // TextBox: اسم المستخدم
            txtUsername = new TextBox();
            txtUsername.Location = new Point(50, 80);
            txtUsername.Size = new Size(300, 25);
            txtUsername.Font = new Font("Tahoma", 10);
            this.Controls.Add(txtUsername);

            // Label: كلمة المرور
            Label lblPassword = new Label();
            lblPassword.Text = "كلمة المرور:";
            lblPassword.Location = new Point(250, 120);
            lblPassword.Size = new Size(100, 20);
            lblPassword.Font = new Font("Tahoma", 10);
            lblPassword.TextAlign = ContentAlignment.MiddleRight;
            this.Controls.Add(lblPassword);

            // TextBox: كلمة المرور
            txtPassword = new TextBox();
            txtPassword.Location = new Point(50, 150);
            txtPassword.Size = new Size(300, 25);
            txtPassword.PasswordChar = '*';
            txtPassword.Font = new Font("Tahoma", 10);
            this.Controls.Add(txtPassword);

            // Button: الدخول
            btnLogin = new Button();
            btnLogin.Text = "الدخول";
            btnLogin.Location = new Point(200, 200);
            btnLogin.Size = new Size(80, 30);
            btnLogin.Font = new Font("Tahoma", 10);
            btnLogin.BackColor = Color.LightBlue;
            btnLogin.Click += BtnLogin_Click;
            this.Controls.Add(btnLogin);

            // Button: الإعدادات
            btnSettings = new Button();
            btnSettings.Text = "الإعدادات";
            btnSettings.Location = new Point(110, 200);
            btnSettings.Size = new Size(80, 30);
            btnSettings.Font = new Font("Tahoma", 10);
            btnSettings.BackColor = Color.LightGreen;
            btnSettings.Click += BtnSettings_Click;
            this.Controls.Add(btnSettings);

            // Button: خروج
            btnExit = new Button();
            btnExit.Text = "خروج";
            btnExit.Location = new Point(20, 200);
            btnExit.Size = new Size(80, 30);
            btnExit.Font = new Font("Tahoma", 10);
            btnExit.BackColor = Color.LightCoral;
            btnExit.Click += BtnExit_Click;
            this.Controls.Add(btnExit);
        }


        private void OpenMainForm(User user)
        {
            try
            {
                // اختبار الاتصال بقاعدة البيانات التطبيقية (للتأكد من عمل النظام)
                bool appDbConnected = _databaseService.TestConnection(Common.Enums.DatabaseType.AppDatabase);

                if (!appDbConnected)
                {
                    MessageBox.Show("تم تسجيل الدخول بنجاح ولكن هناك مشكلة في قاعدة البيانات التطبيقية", "تحذير", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

                // فتح الشاشة الرئيسية
                MainForm mainForm = new MainForm(user);
                mainForm.FormClosed += (s, args) => {
                    this.Show(); // إعادة إظهار شاشة التسجيل عند الخروج
                };

                this.Hide();
                mainForm.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في فتح الشاشة الرئيسية: {ex.Message}", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Show(); // إعادة إظهار شاشة التسجيل في حالة الخطأ
            }
        }
        private void BtnLogin_Click(object sender, EventArgs e)
        {
            try
            {
                if (_connectionSettings == null)
                {
                    MessageBox.Show("يجب ضبط إعدادات الاتصال أولاً", "تحذير", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtUsername.Text) || string.IsNullOrWhiteSpace(txtPassword.Text))
                {
                    MessageBox.Show("يرجى إدخال اسم المستخدم وكلمة المرور", "تحذير", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

             //   التأكد من أن إعدادات السيرفر الأول(النظامي) موجودة
                if (_connectionSettings.SystemDatabase == null ||
                    string.IsNullOrWhiteSpace(_connectionSettings.SystemDatabase.ServerName) ||
                    string.IsNullOrWhiteSpace(_connectionSettings.SystemDatabase.DatabaseName))
                {
                    MessageBox.Show("إعدادات قاعدة البيانات النظامية غير صحيحة", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

               // اختبار الاتصال بقاعدة البيانات النظامية فقط
                if (!_databaseService.TestConnection(Common.Enums.DatabaseType.SystemDatabase))
                {
                    MessageBox.Show("فشل الاتصال بقاعدة البيانات النظامية", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

              //  محاولة تسجيل الدخول من القاعدة النظامية فقط
               User user = _userService.AuthenticateUser(txtUsername.Text, txtPassword.Text);

                if (user != null)
                {
                    MessageBox.Show($"مرحباً {user.FullName}! تم تسجيل الدخول بنجاح", "نجاح", MessageBoxButtons.OK, MessageBoxIcon.Information);

                  //  فتح الشاشة الرئيسية
                    OpenMainForm(user);
                }
                else
                {
                    MessageBox.Show("اسم المستخدم أو كلمة المرور غير صحيحة", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في تسجيل الدخول: {ex.Message}", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }




        private bool CreateOrUpdateDefaultUser()
        {
            try
            {
                using (var connection = _databaseService.GetConnection(Common.Enums.DatabaseType.SystemDatabase))
                {
                    connection.Open();

                    // كلمة المرور المشفرة بـ SHA256 للرقم "1"
                    string hashedPassword = "6b86b273ff34fce19d6b804eff5a3f5747ada4eaa22f1d49c01e52ddb7875b4b";

                    // حذف المستخدم إذا كان موجوداً (لتجنب تكرار المفتاح)
                    string deleteQuery = "DELETE FROM Users WHERE Username = '1'";
                    using (var deleteCommand = new SqlCommand(deleteQuery, connection))
                    {
                        deleteCommand.ExecuteNonQuery();
                    }

                    // إضافة المستخدم من جديد
                    string insertQuery = @"INSERT INTO Users (Username, Password, FullName, IsActive, CreatedDate)
                                 VALUES ('1', @Password, 'مستخدم افتراضي', 1, GETDATE())";

                    using (var insertCommand = new SqlCommand(insertQuery, connection))
                    {
                        insertCommand.Parameters.AddWithValue("@Password", hashedPassword);
                        int result = insertCommand.ExecuteNonQuery();
                        return result > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في إنشاء المستخدم: {ex.Message}", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private void DiagnoseLoginIssue(string username, string password)
        {
            try
            {
                using (var connection = _databaseService.GetConnection(Common.Enums.DatabaseType.SystemDatabase))
                {
                    connection.Open();

                    // التحقق من وجود المستخدم
                    string checkUserQuery = "SELECT COUNT(*) FROM Users WHERE Username = @Username";
                    using (var checkCommand = new SqlCommand(checkUserQuery, connection))
                    {
                        checkCommand.Parameters.AddWithValue("@Username", username);
                        int userCount = (int)checkCommand.ExecuteScalar();

                        if (userCount == 0)
                        {
                            MessageBox.Show($"المستخدم '{username}' غير موجود في قاعدة البيانات", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                    }

                    // الحصول على كلمة المرور المخزنة
                    string getPasswordQuery = "SELECT Password FROM Users WHERE Username = @Username";
                    using (var passwordCommand = new SqlCommand(getPasswordQuery, connection))
                    {
                        passwordCommand.Parameters.AddWithValue("@Username", username);
                        string storedPassword = passwordCommand.ExecuteScalar()?.ToString();

                        string inputHashed = Security.PasswordHasher.HashPassword(password);

                        MessageBox.Show($"كلمة المرور المدخلة (مشفرة): {inputHashed}\nكلمة المرور المخزنة: {storedPassword}\nالتطابق: {inputHashed == storedPassword}",
                            "تشخيص", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في التشخيص: {ex.Message}", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnSettings_Click(object sender, EventArgs e)
        {
            try
            {
                using (ConnectionSettingsForm settingsForm = new ConnectionSettingsForm())
                {
                    // ربط حدث حفظ الإعدادات
                    settingsForm.SettingsSaved += () => {
                        InitializeServices(); // إعادة تحميل الإعدادات
                    };

                    this.Hide(); // إخفاء شاشة التسجيل
                    settingsForm.ShowDialog(); // عرض شاشة الإعدادات
                }
            }
            finally
            {
                this.Show(); // تأكد من إظهار شاشة التسجيل دائماً
                this.BringToFront(); // جلبها للأمام
            }
        }

        private void BtnExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}