using System;
using System.Drawing;
using System.Windows.Forms;
using Common.Models;
using System.Linq;
using DatabaseManager;
using SettingsManager;

namespace FormsLibrary
{
    public partial class MainForm : Form
    {
        private MenuStrip mainMenu;
        private ToolStripMenuItem managementMenu;
        private StatusStrip statusStrip;
        private TabControl tabControl;
        private User _currentUser;

        public MainForm(User user)
        {
            _currentUser = user;
            InitializeComponent();
            SetupForm();
            AddControls();
        }

        private void SetupForm()
        {
            this.Text = "النظام الرئيسي - إدارة السندات والمستندات";
            this.Size = new Size(1000, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.WindowState = FormWindowState.Maximized;
            this.RightToLeft = RightToLeft.Yes;
            this.RightToLeftLayout = true;
            this.BackColor = Color.White;
        }

        private void AddControls()
        {
            // شريط القوائم الرئيسي
            AddMainMenu();

            // شريط الحالة
            AddStatusStrip();

            // عنصر التحكم بالتبويبات
            AddTabControl();
        }



        private void AddMainMenu()
        {
            mainMenu = new MenuStrip();
            mainMenu.Font = new Font("Tahoma", 10);

            // قائمة إدارة السندات
            managementMenu = new ToolStripMenuItem("إدارة السندات");

            // عناصر القائمة المنسدلة
            var paymentVouchersItem = new ToolStripMenuItem("إدارة سندات الصرف");
            var receiptVouchersItem = new ToolStripMenuItem("إدارة سندات القبض");
            var simpleEntriesItem = new ToolStripMenuItem("إدارة القيود البسيطة");

            // إضافة عنصر إدارة المستخدمين الجديد
            var usersManagementItem = new ToolStripMenuItem("إدارة المستخدمين");
            var exitItem = new ToolStripMenuItem("خروج");

            // ربط الأحداث
            paymentVouchersItem.Click += PaymentVouchersItem_Click;
            receiptVouchersItem.Click += ReceiptVouchersItem_Click;
            simpleEntriesItem.Click += SimpleEntriesItem_Click;
            usersManagementItem.Click += UsersManagementItem_Click; // حدث جديد
            exitItem.Click += ExitItem_Click;

            // إضافة العناصر للقائمة
            managementMenu.DropDownItems.AddRange(new ToolStripItem[] {
        paymentVouchersItem,
        receiptVouchersItem,
        simpleEntriesItem,
        new ToolStripSeparator(),
        usersManagementItem, // إضافة العنصر الجديد
        new ToolStripSeparator(),
        exitItem
    });

            // قائمة المستخدم
            var userMenu = new ToolStripMenuItem($"المستخدم: {_currentUser.FullName}");
            var logoutItem = new ToolStripMenuItem("تسجيل الخروج");
            logoutItem.Click += LogoutItem_Click;
            userMenu.DropDownItems.Add(logoutItem);

            // إضافة القوائم لشريط القوائم
            mainMenu.Items.AddRange(new ToolStripItem[] {
        managementMenu,
        userMenu
    });

            this.Controls.Add(mainMenu);
            this.MainMenuStrip = mainMenu;
        }

        private void AddStatusStrip()
        {
            statusStrip = new StatusStrip();
            statusStrip.Font = new Font("Tahoma", 9);

            // رسالة ترحيب
            var welcomeLabel = new ToolStripStatusLabel();
            welcomeLabel.Text = $"مرحباً بك: {_currentUser.FullName} - {_currentUser.Username}";
            welcomeLabel.Font = new Font("Tahoma", 9, FontStyle.Bold);

            // معلومات قاعدة البيانات
            var databaseLabel = new ToolStripStatusLabel();
            databaseLabel.Text = GetDatabaseInfo();
            databaseLabel.BorderSides = ToolStripStatusLabelBorderSides.Left;
            databaseLabel.BorderStyle = Border3DStyle.Etched;

            // تاريخ النظام
            var dateLabel = new ToolStripStatusLabel();
            dateLabel.Text = DateTime.Now.ToString("yyyy/MM/dd HH:mm");
            dateLabel.BorderSides = ToolStripStatusLabelBorderSides.Left;
            dateLabel.BorderStyle = Border3DStyle.Etched;

            statusStrip.Items.AddRange(new ToolStripItem[] {
                welcomeLabel,
                databaseLabel,
                new ToolStripStatusLabel() { Spring = true }, // مسافة فارغة
                dateLabel
            });

            this.Controls.Add(statusStrip);
        }

        private string GetDatabaseInfo()
        {
            try
            {
                var settingsService = new SettingsService();
                var connectionSettings = settingsService.LoadSettings();

                if (connectionSettings?.AppDatabase != null)
                {
                    return $"قاعدة البيانات: {connectionSettings.AppDatabase.DatabaseName}";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"خطأ في جلب معلومات قاعدة البيانات: {ex.Message}");
            }

            return "قاعدة البيانات: غير محددة";
        }

        private void AddTabControl()
        {
            tabControl = new TabControl();
            tabControl.Location = new Point(10, 30);
            tabControl.Size = new Size(this.ClientSize.Width - 20, this.ClientSize.Height - 80);
            tabControl.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            tabControl.Font = new Font("Tahoma", 9);

            this.Controls.Add(tabControl);
        }

        private void PaymentVouchersItem_Click(object sender, EventArgs e)
        {
            OpenOrActivateTab("سندات الصرف", () =>
            {
                try
                {
                    var settingsService = new SettingsService();
                    var connectionSettings = settingsService.LoadSettings();

                    if (connectionSettings == null)
                    {
                        return new Form()
                        {
                            Text = "سندات الصرف - إعدادات مطلوبة",
                            BackColor = Color.LightYellow
                        };
                    }

                    var databaseService = new DatabaseService(connectionSettings);
                    return new PaymentVouchersForm(databaseService, _currentUser);
                }
                catch (Exception ex)
                {
                    return new Form()
                    {
                        Text = $"سندات الصرف - خطأ: {ex.Message}",
                        BackColor = Color.LightCoral
                    };
                }
            });
        }

        private void ReceiptVouchersItem_Click(object sender, EventArgs e)
        {
            OpenOrActivateTab("سندات القبض", () =>
            {
                try
                {
                    var settingsService = new SettingsService();
                    var connectionSettings = settingsService.LoadSettings();

                    if (connectionSettings == null)
                    {
                        return new Form()
                        {
                            Text = "سندات القبض - إعدادات مطلوبة",
                            BackColor = Color.LightYellow
                        };
                    }

                    var databaseService = new DatabaseService(connectionSettings);
                    return new ReceiptVouchersForm(databaseService, _currentUser);
                }
                catch (Exception ex)
                {
                    return new Form()
                    {
                        Text = $"سندات القبض - خطأ: {ex.Message}",
                        BackColor = Color.LightCoral
                    };
                }
            });
        }

        private void SimpleEntriesItem_Click(object sender, EventArgs e)
        {
            OpenOrActivateTab("القيود البسيطة", () =>
            {
                try
                {
                    var settingsService = new SettingsService();
                    var connectionSettings = settingsService.LoadSettings();

                    if (connectionSettings == null)
                    {
                        return new Form()
                        {
                            Text = "القيود البسيطة - إعدادات مطلوبة",
                            BackColor = Color.LightYellow
                        };
                    }

                    var databaseService = new DatabaseService(connectionSettings);
                    return new SimpleEntriesForm(databaseService, _currentUser);
                }
                catch (Exception ex)
                {
                    return new Form()
                    {
                        Text = $"القيود البسيطة - خطأ: {ex.Message}",
                        BackColor = Color.LightCoral
                    };
                }
            });
        }

        private void OpenOrActivateTab(string tabName, Func<Form> createForm)
        {
            // البحث إذا كان التبويب مفتوحاً بالفعل
            foreach (TabPage tabPage in tabControl.TabPages)
            {
                if (tabPage.Text == tabName)
                {
                    tabControl.SelectedTab = tabPage;
                    return;
                }
            }

            // إنشاء تبويب جديد
            TabPage newTab = new TabPage(tabName);
            Form form = createForm();
            form.TopLevel = false;
            form.FormBorderStyle = FormBorderStyle.None;
            form.Dock = DockStyle.Fill;
            form.Visible = true;

            newTab.Controls.Add(form);
            tabControl.TabPages.Add(newTab);
            tabControl.SelectedTab = newTab;
        }

        private void LogoutItem_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("هل تريد تسجيل الخروج؟", "تأكيد",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                this.Close();
            }
        }

        private void ExitItem_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("هل تريد الخروج من البرنامج؟", "تأكيد",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                Application.Exit();
            }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);
            // هنا سنعود لشاشة تسجيل الدخول لاحقاً
        }

        private void UsersManagementItem_Click(object sender, EventArgs e)
        {
            try
            {
                // إنشاء خدمة المستخدمين
                var settingsService = new SettingsService();
                var connectionSettings = settingsService.LoadSettings();

                if (connectionSettings == null)
                {
                    MessageBox.Show("يجب ضبط إعدادات الاتصال أولاً", "تحذير", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var databaseService = new DatabaseService(connectionSettings);
                var userService = new UserService(databaseService);

                // فتح شاشة إدارة المستخدمين
                UsersManagementForm usersForm = new UsersManagementForm(userService);
                usersForm.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في فتح شاشة إدارة المستخدمين: {ex.Message}", "خطأ",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}