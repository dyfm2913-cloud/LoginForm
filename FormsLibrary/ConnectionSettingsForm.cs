using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Common.Models;
using SettingsManager;
using DatabaseManager;
using Common.Enums;

namespace FormsLibrary
{
    public partial class ConnectionSettingsForm : Form
    {
        private TextBox txtServer1, txtUser1, txtPassword1;
        private TextBox txtServer2, txtUser2, txtPassword2;
        private ComboBox cmbDatabase1, cmbDatabase2;
        private Button btnSave, btnUpdateConnection, btnClearSettings;
        private SettingsService _settingsService;
        private Label lblStatus;
        private bool _hasExistingSettings;

        // الأحداث للتواصل مع الفورم الرئيسي
        public event Action SettingsSaved;
        public event Action ReturnToLoginRequested;

        public ConnectionSettingsForm()
        {
            InitializeComponent();
            SetupForm();
            AddControls();
            LoadExistingSettings();
        }

        private void SetupForm()
        {
            this.Text = "إعدادات الاتصال بقواعد البيانات";
            this.Size = new Size(900, 650);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.RightToLeft = RightToLeft.Yes;
            this.RightToLeftLayout = true;
            this.BackColor = Color.FromArgb(245, 245, 245);

            // إضافة حدود مستديرة للنافذة
            this.Padding = new Padding(20);
        }

        private void AddControls()
        {
            // عنوان رئيسي
            Label lblTitle = new Label();
            lblTitle.Text = "⚙️ إعدادات الاتصال بقواعد البيانات";
            lblTitle.Location = new Point(300, 10);
            lblTitle.Size = new Size(300, 30);
            lblTitle.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            lblTitle.ForeColor = Color.DarkBlue;
            lblTitle.TextAlign = ContentAlignment.MiddleCenter;
            this.Controls.Add(lblTitle);

            // لوحة السيرفر الأول
            AddServerPanel(1, "قاعدة البيانات النظامية (المستخدمين)", Color.FromArgb(240, 248, 255), 50);

            // لوحة السيرفر الثاني
            AddServerPanel(2, "قاعدة البيانات التطبيقية (السندات)", Color.FromArgb(240, 255, 240), 300);

            // زر مسح جميع إعدادات الاتصال
            btnClearSettings = new Button();
            btnClearSettings.Text = "🗑️ مسح جميع إعدادات الاتصال";
            btnClearSettings.Location = new Point(50, 550);
            btnClearSettings.Size = new Size(180, 35);
            btnClearSettings.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            btnClearSettings.BackColor = Color.FromArgb(220, 20, 60);
            btnClearSettings.ForeColor = Color.White;
            btnClearSettings.FlatStyle = FlatStyle.Flat;
            btnClearSettings.Click += BtnClearSettings_Click;
            this.Controls.Add(btnClearSettings);

            // زر الحفظ
            btnSave = new Button();
            btnSave.Text = "💾 حفظ الإعدادات";
            btnSave.Location = new Point(250, 550);
            btnSave.Size = new Size(120, 35);
            btnSave.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            btnSave.BackColor = Color.FromArgb(34, 139, 34);
            btnSave.ForeColor = Color.White;
            btnSave.FlatStyle = FlatStyle.Flat;
            btnSave.Click += BtnSave_Click;
            this.Controls.Add(btnSave);

            // زر تحديث بيانات الاتصال
            btnUpdateConnection = new Button();
            btnUpdateConnection.Text = "🔄 تحديث بيانات الاتصال";
            btnUpdateConnection.Location = new Point(390, 550);
            btnUpdateConnection.Size = new Size(150, 35);
            btnUpdateConnection.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            btnUpdateConnection.BackColor = Color.FromArgb(255, 140, 0);
            btnUpdateConnection.ForeColor = Color.White;
            btnUpdateConnection.FlatStyle = FlatStyle.Flat;
            btnUpdateConnection.Click += BtnUpdateConnection_Click;
            this.Controls.Add(btnUpdateConnection);

            // شريط الحالة
            lblStatus = new Label();
            lblStatus.Text = "جاهز";
            lblStatus.Location = new Point(220, 560);
            lblStatus.Size = new Size(280, 20);
            lblStatus.Font = new Font("Segoe UI", 8);
            lblStatus.ForeColor = Color.DarkGreen;
            lblStatus.TextAlign = ContentAlignment.MiddleCenter;
            this.Controls.Add(lblStatus);
        }

        private void AddServerPanel(int serverNum, string title, Color backColor, int yPos)
        {
            // لوحة الخلفية
            Panel panel = new Panel();
            panel.Location = new Point(30, yPos);
            panel.Size = new Size(820, 200);
            panel.BackColor = backColor;
            panel.BorderStyle = BorderStyle.FixedSingle;
            this.Controls.Add(panel);

            // عنوان اللوحة
            Label lblTitle = new Label();
            lblTitle.Text = title;
            lblTitle.Location = new Point(10, 10);
            lblTitle.Size = new Size(300, 25);
            lblTitle.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            lblTitle.ForeColor = Color.DarkBlue;
            panel.Controls.Add(lblTitle);

            // إعدادات السيرفر
            if (serverNum == 1)
            {
                AddLabelAndControl(panel, 1, 50, "اسم السيرفر:", "txtServer1", out txtServer1, false);
                AddLabelAndControl(panel, 1, 90, "اسم المستخدم:", "txtUser1", out txtUser1, false);
                AddLabelAndControl(panel, 1, 130, "كلمة المرور:", "txtPassword1", out txtPassword1, true);
                AddLabelAndComboBox(panel, 1, 170, "قاعدة البيانات:", "cmbDatabase1", out cmbDatabase1);

                // زر اختبار الاتصال
                var btnTestConnection1 = new Button();
                btnTestConnection1.Text = "🔗 اختبار الاتصال";
                btnTestConnection1.Location = new Point(650, 160);
                btnTestConnection1.Size = new Size(120, 30);
                btnTestConnection1.Font = new Font("Segoe UI", 9);
                btnTestConnection1.BackColor = Color.FromArgb(30, 144, 255);
                btnTestConnection1.ForeColor = Color.White;
                btnTestConnection1.FlatStyle = FlatStyle.Flat;
                btnTestConnection1.Click += BtnTestConnection1_Click;
                panel.Controls.Add(btnTestConnection1);
            }
            else
            {
                AddLabelAndControl(panel, 2, 50, "اسم السيرفر:", "txtServer2", out txtServer2, false);
                AddLabelAndControl(panel, 2, 90, "اسم المستخدم:", "txtUser2", out txtUser2, false);
                AddLabelAndControl(panel, 2, 130, "كلمة المرور:", "txtPassword2", out txtPassword2, true);
                AddLabelAndComboBox(panel, 2, 170, "قاعدة البيانات:", "cmbDatabase2", out cmbDatabase2);

                // زر اختبار الاتصال
                var btnTestConnection2 = new Button();
                btnTestConnection2.Text = "🔗 اختبار الاتصال";
                btnTestConnection2.Location = new Point(650, 160);
                btnTestConnection2.Size = new Size(120, 30);
                btnTestConnection2.Font = new Font("Segoe UI", 9);
                btnTestConnection2.BackColor = Color.FromArgb(50, 205, 50);
                btnTestConnection2.ForeColor = Color.White;
                btnTestConnection2.FlatStyle = FlatStyle.Flat;
                btnTestConnection2.Click += BtnTestConnection2_Click;
                panel.Controls.Add(btnTestConnection2);
            }
        }

        private void AddLabelAndControl(Panel parentPanel, int serverNum, int yPos, string labelText, string controlName, out TextBox textBox, bool isPassword)
        {
            // Label
            Label label = new Label();
            label.Text = labelText;
            label.Location = new Point(20, yPos);
            label.Size = new Size(120, 20);
            label.Font = new Font("Segoe UI", 9);
            label.TextAlign = ContentAlignment.MiddleRight;
            parentPanel.Controls.Add(label);

            // TextBox
            textBox = new TextBox();
            textBox.Name = controlName;
            textBox.Location = new Point(150, yPos);
            textBox.Size = new Size(200, 25);
            textBox.Font = new Font("Segoe UI", 9);
            textBox.BorderStyle = BorderStyle.FixedSingle;

            if (isPassword)
                textBox.PasswordChar = '*';

            parentPanel.Controls.Add(textBox);
        }

        private void AddLabelAndComboBox(Panel parentPanel, int serverNum, int yPos, string labelText, string controlName, out ComboBox comboBox)
        {
            // Label
            Label label = new Label();
            label.Text = labelText;
            label.Location = new Point(20, yPos);
            label.Size = new Size(120, 20);
            label.Font = new Font("Segoe UI", 9);
            label.TextAlign = ContentAlignment.MiddleRight;
            parentPanel.Controls.Add(label);

            // ComboBox
            comboBox = new ComboBox();
            comboBox.Name = controlName;
            comboBox.Location = new Point(150, yPos);
            comboBox.Size = new Size(200, 25);
            comboBox.Font = new Font("Segoe UI", 9);
            comboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox.FlatStyle = FlatStyle.Flat;

            parentPanel.Controls.Add(comboBox);
        }

        private void LoadExistingSettings()
        {
            _settingsService = new SettingsService();
            var settings = _settingsService.LoadSettings();

            if (settings != null)
            {
                _hasExistingSettings = true;

                // تحميل إعدادات السيرفر الأول
                if (settings.SystemDatabase != null)
                {
                    txtServer1.Text = settings.SystemDatabase.ServerName;
                    txtUser1.Text = settings.SystemDatabase.Username;
                    txtPassword1.Text = settings.SystemDatabase.Password;
                    cmbDatabase1.Text = settings.SystemDatabase.DatabaseName;
                }

                // تحميل إعدادات السيرفر الثاني
                if (settings.AppDatabase != null)
                {
                    txtServer2.Text = settings.AppDatabase.ServerName;
                    txtUser2.Text = settings.AppDatabase.Username;
                    txtPassword2.Text = settings.AppDatabase.Password;
                    cmbDatabase2.Text = settings.AppDatabase.DatabaseName;
                }
            }
            else
            {
                _hasExistingSettings = false;
            }

            // تحميل قواعد البيانات من السيرفرات
            LoadDatabasesFromServers();
        }

        private void LoadDatabasesFromServers()
        {
            try
            {
                // تحميل قواعد البيانات للسيرفر الأول
                if (!string.IsNullOrWhiteSpace(txtServer1.Text) &&
                    !string.IsNullOrWhiteSpace(txtUser1.Text))
                {
                    var databases1 = GetDatabasesFromServer(txtServer1.Text, txtUser1.Text, txtPassword1.Text);
                    if (databases1.Count > 0)
                    {
                        cmbDatabase1.Items.Clear();
                        cmbDatabase1.Items.AddRange(databases1.ToArray());

                        // إذا كان هناك اسم قاعدة بيانات محفوظ، تأكد من أنه موجود في القائمة واختره
                        if (!string.IsNullOrWhiteSpace(cmbDatabase1.Text))
                        {
                            if (!cmbDatabase1.Items.Contains(cmbDatabase1.Text))
                            {
                                cmbDatabase1.Items.Insert(0, cmbDatabase1.Text);
                            }
                            cmbDatabase1.SelectedItem = cmbDatabase1.Text;
                        }
                    }
                }

                // تحميل قواعد البيانات للسيرفر الثاني
                if (!string.IsNullOrWhiteSpace(txtServer2.Text) &&
                    !string.IsNullOrWhiteSpace(txtUser2.Text))
                {
                    var databases2 = GetDatabasesFromServer(txtServer2.Text, txtUser2.Text, txtPassword2.Text);
                    if (databases2.Count > 0)
                    {
                        cmbDatabase2.Items.Clear();
                        cmbDatabase2.Items.AddRange(databases2.ToArray());

                        // إذا كان هناك اسم قاعدة بيانات محفوظ، تأكد من أنه موجود في القائمة واختره
                        if (!string.IsNullOrWhiteSpace(cmbDatabase2.Text))
                        {
                            if (!cmbDatabase2.Items.Contains(cmbDatabase2.Text))
                            {
                                cmbDatabase2.Items.Insert(0, cmbDatabase2.Text);
                            }
                            cmbDatabase2.SelectedItem = cmbDatabase2.Text;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // في حالة فشل التحميل، نترك القوائم فارغة
                System.Diagnostics.Debug.WriteLine($"خطأ في تحميل قواعد البيانات: {ex.Message}");
            }
        }

        private void ConfigureButtonVisibility()
        {
            if (_hasExistingSettings)
            {
                // إذا كانت هناك إعدادات موجودة، إخفاء زر الحفظ وإظهار زر التحديث
                btnSave.Visible = false;
                if (btnUpdateConnection != null)
                    btnUpdateConnection.Visible = true;
            }
            else
            {
                // إذا لم تكن هناك إعدادات، إظهار زر الحفظ وإخفاء زر التحديث
                btnSave.Visible = true;
                if (btnUpdateConnection != null)
                    btnUpdateConnection.Visible = false;
            }
        }

        private void BtnTestConnection1_Click(object sender, EventArgs e)
        {
            TestConnection(1);
        }

        private void BtnTestConnection2_Click(object sender, EventArgs e)
        {
            TestConnection(2);
        }

        private void TestConnection(int serverNumber)
        {
            try
            {
                // التحقق من وجود بيانات السيرفر المطلوبة (ثلاثة باراميترات فقط)
                string serverName = serverNumber == 1 ? txtServer1.Text : txtServer2.Text;
                string username = serverNumber == 1 ? txtUser1.Text : txtUser2.Text;
                string password = serverNumber == 1 ? txtPassword1.Text : txtPassword2.Text;

                if (string.IsNullOrWhiteSpace(serverName) || string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                {
                    MessageBox.Show("يرجى إدخال اسم السيرفر واسم المستخدم وكلمة المرور", "تحذير", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // اختبار الاتصال باستخدام الباراميترات الثلاثة فقط
                bool success = TestServerConnection(serverName, username, password);

                if (success)
                {
                    MessageBox.Show("تم الاتصال بنجاح!", "نجاح", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // تحميل قواعد البيانات من السيرفر المتصل مباشرة
                    var databases = GetDatabasesFromServer(serverName, username, password);
                    if (databases.Count > 0)
                    {
                        if (serverNumber == 1)
                        {
                            cmbDatabase1.Items.Clear();
                            cmbDatabase1.Items.AddRange(databases.ToArray());

                            // إذا كان هناك اسم قاعدة بيانات محفوظ، تأكد من أنه موجود في القائمة
                            if (!string.IsNullOrWhiteSpace(cmbDatabase1.Text))
                            {
                                if (!cmbDatabase1.Items.Contains(cmbDatabase1.Text))
                                {
                                    cmbDatabase1.Items.Insert(0, cmbDatabase1.Text);
                                }
                                cmbDatabase1.SelectedItem = cmbDatabase1.Text;
                            }
                        }
                        else
                        {
                            cmbDatabase2.Items.Clear();
                            cmbDatabase2.Items.AddRange(databases.ToArray());

                            // إذا كان هناك اسم قاعدة بيانات محفوظ، تأكد من أنه موجود في القائمة
                            if (!string.IsNullOrWhiteSpace(cmbDatabase2.Text))
                            {
                                if (!cmbDatabase2.Items.Contains(cmbDatabase2.Text))
                                {
                                    cmbDatabase2.Items.Insert(0, cmbDatabase2.Text);
                                }
                                cmbDatabase2.SelectedItem = cmbDatabase2.Text;
                            }
                        }
                    }
                }
                else
                {
                    MessageBox.Show("فشل الاتصال بالسيرفر", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشل الاتصال: {ex.Message}", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool TestServerConnection(string serverName, string username, string password)
        {
            try
            {
                string connectionString = $"Server={serverName};User Id={username};Password={password};";

                using (var connection = new System.Data.SqlClient.SqlConnection(connectionString))
                {
                    connection.Open();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        private ConnectionSettings GetCurrentSettings()
        {
            if (string.IsNullOrWhiteSpace(txtServer1.Text) || string.IsNullOrWhiteSpace(cmbDatabase1.Text))
            {
                MessageBox.Show("يرجى إدخال إعدادات السيرفر الأول", "تحذير", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return null;
            }

            return new ConnectionSettings
            {
                SystemDatabase = new DatabaseConfig
                {
                    ServerName = txtServer1.Text,
                    Username = txtUser1.Text,
                    Password = txtPassword1.Text,
                    DatabaseName = cmbDatabase1.Text
                },
                AppDatabase = new DatabaseConfig
                {
                    ServerName = txtServer2.Text,
                    Username = txtUser2.Text,
                    Password = txtPassword2.Text,
                    DatabaseName = cmbDatabase2.Text
                }
            };
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            try
            {
                var settings = GetCurrentSettings();
                if (settings == null) return;

                // حفظ جميع البيانات بما في ذلك قيم الكومبوبوكس
                settings.SystemDatabase.DatabaseName = cmbDatabase1.Text;
                settings.AppDatabase.DatabaseName = cmbDatabase2.Text;

                _settingsService.SaveSettings(settings);
                MessageBox.Show("تم حفظ الإعدادات بنجاح", "نجاح", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // إطلاق event أن الإعدادات تم حفظها
                SettingsSaved?.Invoke();

                // بعد الحفظ الأول، إخفاء زر الحفظ وإظهار زر التحديث
                _hasExistingSettings = true;
                ConfigureButtonVisibility();

                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في حفظ الإعدادات: {ex.Message}", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnUpdateConnection_Click(object sender, EventArgs e)
        {
            try
            {
                var settings = GetCurrentSettings();
                if (settings == null) return;

                // حفظ جميع البيانات بما في ذلك قيم الكومبوبوكس
                settings.SystemDatabase.DatabaseName = cmbDatabase1.Text;
                settings.AppDatabase.DatabaseName = cmbDatabase2.Text;

                _settingsService.SaveSettings(settings);
                MessageBox.Show("تم تحديث بيانات الاتصال بنجاح", "نجاح", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // إطلاق event أن الإعدادات تم حفظها
                SettingsSaved?.Invoke();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في تحديث بيانات الاتصال: {ex.Message}", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnClearSettings_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show(
                "هل أنت متأكد من مسح جميع إعدادات الاتصال؟\n\nسيتم حذف جميع البيانات المحفوظة ولا يمكن التراجع عن هذا الإجراء.",
                "تأكيد المسح",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button2);

            if (result == DialogResult.Yes)
            {
                try
                {
                    // مسح الإعدادات من الذاكرة
                    _settingsService.ClearSettings();

                    // مسح الحقول من النموذج
                    txtServer1.Text = "";
                    txtUser1.Text = "";
                    txtPassword1.Text = "";
                    cmbDatabase1.Text = "";

                    txtServer2.Text = "";
                    txtUser2.Text = "";
                    txtPassword2.Text = "";
                    cmbDatabase2.Text = "";

                    // تحديث الحالة
                    _hasExistingSettings = false;
                    ConfigureButtonVisibility();

                    MessageBox.Show("تم مسح جميع إعدادات الاتصال بنجاح", "نجاح", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"خطأ في مسح الإعدادات: {ex.Message}", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }


        private List<string> GetDatabasesFromServer(string serverName, string username, string password)
        {
            var databases = new List<string>();

            try
            {
                string connectionString = $"Server={serverName};User Id={username};Password={password};";

                using (var connection = new System.Data.SqlClient.SqlConnection(connectionString))
                {
                    connection.Open();

                    string query = "SELECT name FROM sys.databases WHERE database_id > 4 ORDER BY name";

                    using (var command = new System.Data.SqlClient.SqlCommand(query, connection))
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            databases.Add(reader["name"].ToString());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // في حالة فشل الاتصال، نعيد قائمة فارغة
                System.Diagnostics.Debug.WriteLine($"خطأ في جلب قواعد البيانات: {ex.Message}");
            }

            return databases;
        }
    }
}