using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using Common.Models;
using DatabaseManager;

namespace FormsLibrary
{
    public partial class UsersManagementForm : Form
    {
        private DataGridView dataGridViewUsers;
        private Button btnAdd, btnEdit, btnDelete, btnRefresh, btnClose;
        private TextBox txtSearch;
        private Button btnSearch;
        private UserService _userService;
        private List<User> _usersList;

        public UsersManagementForm(UserService userService)
        {
            _userService = userService;
            InitializeComponent();
            SetupForm();
            AddControls();
            LoadUsers();
        }

        private void SetupForm()
        {
            this.Text = "إدارة المستخدمين";
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.RightToLeft = RightToLeft.Yes;
            this.RightToLeftLayout = true;
            this.BackColor = Color.White;
        }

        private void AddControls()
        {
            // شريط البحث
            AddSearchControls();

            // DataGridView لعرض المستخدمين
            AddDataGridView();

            // أزرار التحكم
            AddControlButtons();
        }

        private void AddSearchControls()
        {
            // Label البحث
            Label lblSearch = new Label();
            lblSearch.Text = "بحث:";
            lblSearch.Location = new Point(700, 20);
            lblSearch.Size = new Size(50, 25);
            lblSearch.Font = new Font("Tahoma", 9);
            lblSearch.TextAlign = ContentAlignment.MiddleRight;
            this.Controls.Add(lblSearch);

            // TextBox البحث
            txtSearch = new TextBox();
            txtSearch.Location = new Point(500, 20);
            txtSearch.Size = new Size(190, 25);
            txtSearch.Font = new Font("Tahoma", 9);
            txtSearch.TextChanged += TxtSearch_TextChanged;
            this.Controls.Add(txtSearch);

            // زر البحث
            btnSearch = new Button();
            btnSearch.Text = "بحث";
            btnSearch.Location = new Point(420, 20);
            btnSearch.Size = new Size(70, 25);
            btnSearch.Font = new Font("Tahoma", 9);
            btnSearch.BackColor = Color.LightBlue;
            btnSearch.Click += BtnSearch_Click;
            this.Controls.Add(btnSearch);
        }

        private void AddDataGridView()
        {
            dataGridViewUsers = new DataGridView();
            dataGridViewUsers.Location = new Point(20, 60);
            dataGridViewUsers.Size = new Size(760, 400);
            dataGridViewUsers.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            dataGridViewUsers.Font = new Font("Tahoma", 9);  // تم التصحيح هنا
            dataGridViewUsers.ReadOnly = true;
            dataGridViewUsers.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridViewUsers.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridViewUsers.RightToLeft = RightToLeft.Yes;

            // إضافة الأعمدة
            dataGridViewUsers.Columns.Add("UserID", "رقم المستخدم");
            dataGridViewUsers.Columns.Add("Username", "اسم المستخدم");
            dataGridViewUsers.Columns.Add("FullName", "الاسم الكامل");
            dataGridViewUsers.Columns.Add("IsActive", "مفعل");
            dataGridViewUsers.Columns.Add("CreatedDate", "تاريخ الإنشاء");

            // إعداد عرض الأعمدة
            dataGridViewUsers.Columns["UserID"].Width = 80;
            dataGridViewUsers.Columns["Username"].Width = 120;
            dataGridViewUsers.Columns["FullName"].Width = 200;
            dataGridViewUsers.Columns["IsActive"].Width = 60;
            dataGridViewUsers.Columns["CreatedDate"].Width = 120;

            this.Controls.Add(dataGridViewUsers);
        }

        private void AddControlButtons()
        {
            // زر الإضافة
            btnAdd = new Button();
            btnAdd.Text = "إضافة مستخدم";
            btnAdd.Location = new Point(600, 480);
            btnAdd.Size = new Size(100, 35);
            btnAdd.Font = new Font("Tahoma", 9, FontStyle.Bold);
            btnAdd.BackColor = Color.LightGreen;
            btnAdd.Click += BtnAdd_Click;
            this.Controls.Add(btnAdd);

            // زر التعديل
            btnEdit = new Button();
            btnEdit.Text = "تعديل";
            btnEdit.Location = new Point(490, 480);
            btnEdit.Size = new Size(100, 35);
            btnEdit.Font = new Font("Tahoma", 9);
            btnEdit.BackColor = Color.LightBlue;
            btnEdit.Click += BtnEdit_Click;
            this.Controls.Add(btnEdit);

            // زر الحذف
            btnDelete = new Button();
            btnDelete.Text = "حذف";
            btnDelete.Location = new Point(380, 480);
            btnDelete.Size = new Size(100, 35);
            btnDelete.Font = new Font("Tahoma", 9);
            btnDelete.BackColor = Color.LightCoral;
            btnDelete.Click += BtnDelete_Click;
            this.Controls.Add(btnDelete);

            // زر التحديث
            btnRefresh = new Button();
            btnRefresh.Text = "تحديث";
            btnRefresh.Location = new Point(270, 480);
            btnRefresh.Size = new Size(100, 35);
            btnRefresh.Font = new Font("Tahoma", 9);
            btnRefresh.BackColor = Color.LightYellow;
            btnRefresh.Click += BtnRefresh_Click;
            this.Controls.Add(btnRefresh);

            // زر الإغلاق
            btnClose = new Button();
            btnClose.Text = "إغلاق";
            btnClose.Location = new Point(160, 480);
            btnClose.Size = new Size(100, 35);
            btnClose.Font = new Font("Tahoma", 9);
            btnClose.BackColor = Color.LightGray;
            btnClose.Click += BtnClose_Click;
            this.Controls.Add(btnClose);
        }

        private void LoadUsers()
        {
            try
            {
                _usersList = _userService.GetAllUsers();
                RefreshDataGridView();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في تحميل المستخدمين: {ex.Message}", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RefreshDataGridView()
        {
            dataGridViewUsers.Rows.Clear();

            foreach (var user in _usersList)
            {
                dataGridViewUsers.Rows.Add(
                    user.UserID,
                    user.Username,
                    user.FullName,
                    user.IsActive ? "نعم" : "لا",
                    user.CreatedDate.ToString("yyyy/MM/dd HH:mm")
                );
            }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            ShowUserForm(null); // null يعني إضافة مستخدم جديد
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            if (dataGridViewUsers.SelectedRows.Count == 0)
            {
                MessageBox.Show("يرجى اختيار مستخدم للتعديل", "تحذير", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int userID = (int)dataGridViewUsers.SelectedRows[0].Cells["UserID"].Value;
            User selectedUser = _usersList.Find(u => u.UserID == userID);

            if (selectedUser != null)
            {
                ShowUserForm(selectedUser);
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (dataGridViewUsers.SelectedRows.Count == 0)
            {
                MessageBox.Show("يرجى اختيار مستخدم للحذف", "تحذير", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int userID = (int)dataGridViewUsers.SelectedRows[0].Cells["UserID"].Value;
            string username = dataGridViewUsers.SelectedRows[0].Cells["Username"].Value.ToString();

            var result = MessageBox.Show($"هل أنت متأكد من حذف المستخدم '{username}'؟", "تأكيد الحذف",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                try
                {
                    bool success = _userService.DeleteUser(userID);
                    if (success)
                    {
                        MessageBox.Show("تم حذف المستخدم بنجاح", "نجاح", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadUsers();
                    }
                    else
                    {
                        MessageBox.Show("فشل في حذف المستخدم", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"خطأ في حذف المستخدم: {ex.Message}", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            LoadUsers();
        }

        private void BtnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void BtnSearch_Click(object sender, EventArgs e)
        {
            SearchUsers();
        }

        private void TxtSearch_TextChanged(object sender, EventArgs e)
        {
            // بحث فوري أثناء الكتابة
            SearchUsers();
        }

        private void SearchUsers()
        {
            try
            {
                string searchText = txtSearch.Text.Trim().ToLower();

                if (string.IsNullOrEmpty(searchText))
                {
                    RefreshDataGridView();
                    return;
                }

                // التأكد من أن _usersList ليست null
                if (_usersList == null || _usersList.Count == 0)
                {
                    MessageBox.Show("لا توجد بيانات للبحث فيها", "تحذير", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var filteredUsers = _usersList.FindAll(u =>
                    u.Username.ToLower().Contains(searchText) ||
                    u.FullName.ToLower().Contains(searchText) ||
                    u.UserID.ToString().Contains(searchText)
                );

                dataGridViewUsers.Rows.Clear();
                foreach (var user in filteredUsers)
                {
                    dataGridViewUsers.Rows.Add(
                        user.UserID,
                        user.Username,
                        user.FullName,
                        user.IsActive ? "نعم" : "لا",
                        user.CreatedDate.ToString("yyyy/MM/dd HH:mm")
                    );
                }

                // إظهار عدد النتائج
                if (filteredUsers.Count == 0)
                {
                    MessageBox.Show("لم يتم العثور على نتائج", "معلومة", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في البحث: {ex.Message}", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ShowUserForm(User user)
        {
            UserForm userForm = new UserForm(user, _userService);
            userForm.UserSaved += (s, e) => {
                LoadUsers(); // إعادة تحميل البيانات بعد الحفظ
            };
            userForm.ShowDialog();
        }
    }
}