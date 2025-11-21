using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using Common.Models;
using System.Linq;
using DatabaseManager;

namespace FormsLibrary
{
    public partial class SimpleEntriesForm : Form
    {
        private DataGridView dataGridViewEntries;
        private Button btnNew, btnSave, btnRefresh, btnSearch, btnClose, btnDiagnostics;
        private SimpleEntriesService _simpleEntriesService;
        private List<SimpleEntryView> _entryList;
        private DatabaseService _databaseService;
        private bool _isNewMode = false; // يُستخدم للتحكم في وضع الإدخال الجديد
        private User _currentUser; // إضافة المستخدم الحالي

        public SimpleEntriesForm(DatabaseService databaseService, User currentUser = null)
        {
            _databaseService = databaseService;
            _currentUser = currentUser;
            _simpleEntriesService = new SimpleEntriesService(databaseService);
            
            SetupForm();
            AddControls();
            LoadAllEntriesWithStatus(); // تحميل جميع البيانات مع عرض الحالة
        }

        private void SetupForm()
        {
            this.Text = "إدارة القيود البسيطة - جميع السجلات";
            this.Size = new Size(1200, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.WindowState = FormWindowState.Maximized;
            this.RightToLeft = RightToLeft.Yes;
            this.RightToLeftLayout = true;
            this.BackColor = Color.White;
        }

        private void AddControls()
        {
            // شريط الأدوات العلوي
            AddToolbar();

            // DataGridView لعرض القيود البسيطة
            AddDataGridView();
        }

        private void AddToolbar()
        {
            Panel toolbar = new Panel();
            toolbar.Location = new Point(0, 0);
            toolbar.Size = new Size(1200, 50);
            toolbar.BackColor = Color.LightGray;
            toolbar.BorderStyle = BorderStyle.FixedSingle;

            // زر جديد
            btnNew = new Button();
            btnNew.Text = "جديد";
            btnNew.Location = new Point(20, 10);
            btnNew.Size = new Size(80, 30);
            btnNew.Font = new Font("Tahoma", 9, FontStyle.Bold);
            btnNew.BackColor = Color.LightGreen;
            btnNew.Click += BtnNew_Click;
            toolbar.Controls.Add(btnNew);

            // زر حفظ
            btnSave = new Button();
            btnSave.Text = "حفظ";
            btnSave.Location = new Point(110, 10);
            btnSave.Size = new Size(80, 30);
            btnSave.Font = new Font("Tahoma", 9);
            btnSave.BackColor = Color.LightBlue;
            btnSave.Enabled = false; // غير مفعل إلا في وضع جديد
            btnSave.Click += BtnSave_Click;
            toolbar.Controls.Add(btnSave);

            // زر تحديث
            btnRefresh = new Button();
            btnRefresh.Text = "تحديث";
            btnRefresh.Location = new Point(200, 10);
            btnRefresh.Size = new Size(80, 30);
            btnRefresh.Font = new Font("Tahoma", 9);
            btnRefresh.BackColor = Color.LightYellow;
            btnRefresh.Click += BtnRefresh_Click;
            toolbar.Controls.Add(btnRefresh);

            // زر بحث
            btnSearch = new Button();
            btnSearch.Text = "بحث متقدم";
            btnSearch.Location = new Point(290, 10);
            btnSearch.Size = new Size(100, 30);
            btnSearch.Font = new Font("Tahoma", 9);
            btnSearch.BackColor = Color.LightCyan;
            btnSearch.Click += BtnSearch_Click;
            toolbar.Controls.Add(btnSearch);

            // زر تشخيص
            btnDiagnostics = new Button();
            btnDiagnostics.Text = "تشخيص";
            btnDiagnostics.Location = new Point(400, 10);
            btnDiagnostics.Size = new Size(80, 30);
            btnDiagnostics.Font = new Font("Tahoma", 9);
            btnDiagnostics.BackColor = Color.LightSalmon;
            btnDiagnostics.Click += BtnDiagnostics_Click;
            toolbar.Controls.Add(btnDiagnostics);

            // زر إغلاق
            btnClose = new Button();
            btnClose.Text = "إغلاق";
            btnClose.Location = new Point(490, 10);
            btnClose.Size = new Size(80, 30);
            btnClose.Font = new Font("Tahoma", 9);
            btnClose.BackColor = Color.LightCoral;
            btnClose.Click += BtnClose_Click;
            toolbar.Controls.Add(btnClose);

            this.Controls.Add(toolbar);
        }

        private void AddDataGridView()
        {
            dataGridViewEntries = new DataGridView();
            dataGridViewEntries.Location = new Point(20, 60);
            dataGridViewEntries.Size = new Size(1160, 600);
            dataGridViewEntries.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dataGridViewEntries.Font = new Font("Tahoma", 9);
            dataGridViewEntries.ReadOnly = false;
            dataGridViewEntries.SelectionMode = DataGridViewSelectionMode.CellSelect;
            dataGridViewEntries.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridViewEntries.RightToLeft = RightToLeft.Yes;
            dataGridViewEntries.RowHeadersVisible = false;
            dataGridViewEntries.AllowUserToAddRows = false;
            dataGridViewEntries.EditMode = DataGridViewEditMode.EditOnEnter;

            // إضافة الأعمدة الأساسية
            dataGridViewEntries.Columns.Add("ID", "ID");
            dataGridViewEntries.Columns.Add("الرقم", "رقم القيد");
            dataGridViewEntries.Columns.Add("التاريخ", "التاريخ");
            dataGridViewEntries.Columns.Add("الوصف", "الوصف");
            dataGridViewEntries.Columns.Add("الحساب_المدين", "الحساب المدين");
            dataGridViewEntries.Columns.Add("الحساب_الدائن", "الحساب الدائن");
            dataGridViewEntries.Columns.Add("المبلغ", "المبلغ");
            dataGridViewEntries.Columns.Add("العملة", "العملة");
            dataGridViewEntries.Columns.Add("المستخدم", "المستخدم");

            // إعداد الأعمدة القابلة للتحرير
            dataGridViewEntries.Columns["ID"].ReadOnly = true; // ID غير قابل للتحرير
            dataGridViewEntries.Columns["الرقم"].ReadOnly = true; // رقم القيد غير قابل للتحرير
            dataGridViewEntries.Columns["التاريخ"].ReadOnly = false; // التاريخ قابل للتحرير
            dataGridViewEntries.Columns["الوصف"].ReadOnly = false; // الوصف قابل للتحرير
            dataGridViewEntries.Columns["الحساب_المدين"].ReadOnly = false; // الحساب المدين قابل للتحرير
            dataGridViewEntries.Columns["الحساب_الدائن"].ReadOnly = false; // الحساب الدائن قابل للتحرير
            dataGridViewEntries.Columns["المبلغ"].ReadOnly = false; // المبلغ قابل للتحرير
            dataGridViewEntries.Columns["العملة"].ReadOnly = false; // العملة قابلة للتحرير
            dataGridViewEntries.Columns["المستخدم"].ReadOnly = true; // المستخدم غير قابل للتحرير

            // إضافة عمود التعديل
            DataGridViewButtonColumn editColumn = new DataGridViewButtonColumn();
            editColumn.Name = "تعديل";
            editColumn.HeaderText = "تعديل";
            editColumn.Text = "تعديل";
            editColumn.UseColumnTextForButtonValue = true;
            editColumn.Width = 60;
            editColumn.ReadOnly = true;
            dataGridViewEntries.Columns.Add(editColumn);

            // إضافة عمود الحذف
            DataGridViewButtonColumn deleteColumn = new DataGridViewButtonColumn();
            deleteColumn.Name = "حذف";
            deleteColumn.HeaderText = "حذف";
            deleteColumn.Text = "حذف";
            deleteColumn.UseColumnTextForButtonValue = true;
            deleteColumn.Width = 60;
            deleteColumn.ReadOnly = true;
            dataGridViewEntries.Columns.Add(deleteColumn);

            // تنسيق الأعمدة الرقمية
            dataGridViewEntries.Columns["المبلغ"].DefaultCellStyle.Format = "N2";
            dataGridViewEntries.Columns["المبلغ"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

            // ربط حدث النقر على الأزرار
            dataGridViewEntries.CellClick += DataGridViewEntries_CellClick;

            // ربط حدث عند انتهاء التحرير
            dataGridViewEntries.CellEndEdit += DataGridViewEntries_CellEndEdit;

            AddComboBoxColumns();

            this.Controls.Add(dataGridViewEntries);
        }

        private void LoadAllEntriesWithStatus()
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                this.Text = "إدارة القيود البسيطة - جاري التحميل...";

                _entryList = _simpleEntriesService.GetAllSimpleEntries();
                RefreshDataGridView(_entryList);
                UpdateStatusBar(_entryList.Count);

                this.Text = $"إدارة القيود البسيطة - جميع السجلات ({_entryList.Count})";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في تحميل القيود البسيطة: {ex.Message}", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Text = "إدارة القيود البسيطة - خطأ في التحميل";
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void RefreshDataGridView(List<SimpleEntryView> entryList)
        {
            dataGridViewEntries.Rows.Clear();

            foreach (var entry in entryList)
            {
                dataGridViewEntries.Rows.Add(
                    entry.ID,
                    entry.الرقم,
                    entry.التاريخ?.ToString("yyyy/MM/dd"),
                    entry.الوصف,
                    entry.الحساب_المدين,
                    entry.الحساب_الدائن,
                    entry.المبلغ,
                    entry.العملة,
                    entry.المستخدم
                );
            }
        }

        private void UpdateStatusBar(int recordCount)
        {
            this.Text = $"إدارة القيود البسيطة - عدد السجلات: {recordCount}";
        }

        private void AddComboBoxColumns()
        {
            // Combobox للعملة
            DataGridViewComboBoxColumn currencyColumn = new DataGridViewComboBoxColumn();
            currencyColumn.Name = "العملة";
            currencyColumn.HeaderText = "العملة";
            currencyColumn.Width = 80;

            // إضافة العناصر
            currencyColumn.Items.Add("دينار");
            currencyColumn.Items.Add("دولار");
            currencyColumn.Items.Add("يورو");
            currencyColumn.Items.Add("ريال");

            // استبدال العمود النصي
            if (dataGridViewEntries.Columns.Contains("العملة"))
            {
                int columnIndex = dataGridViewEntries.Columns["العملة"].Index;
                dataGridViewEntries.Columns.Remove("العملة");
                dataGridViewEntries.Columns.Insert(columnIndex, currencyColumn);
            }
        }

        private long GetNextEntryNumber()
        {
            // الحصول على آخر رقم قيد وإضافة 1
            if (_entryList.Count > 0)
            {
                long lastNumber = _entryList.Max(e => e.الرقم ?? 0);
                return lastNumber + 1;
            }
            return 1;
        }

        private void EnableEditMode()
        {
            _isNewMode = true;
            btnSave.Enabled = true;
            btnNew.Enabled = false;

            // جعل جميع الأعمدة قابلة للتحرير في الصف الجديد
            dataGridViewEntries.ReadOnly = false;

            // تظليل الصف الجديد للإشارة إلى وضع الإدخال
            if (dataGridViewEntries.Rows.Count > 0)
            {
                dataGridViewEntries.Rows[0].DefaultCellStyle.BackColor = Color.LightYellow;

                // جعل الأعمدة قابلة للتحرير في الصف الجديد فقط
                foreach (DataGridViewColumn column in dataGridViewEntries.Columns)
                {
                    if (column.Name != "ID" && column.Name != "تعديل" && column.Name != "حذف")
                    {
                        dataGridViewEntries.Rows[0].Cells[column.Index].ReadOnly = false;
                    }
                }
            }
        }

        private void ClearDataGridViewForNewEntry()
        {
            // إضافة صف جديد للإدخال
            dataGridViewEntries.Rows.Insert(0,
                null, // ID
                GetNextEntryNumber(), // الرقم
                DateTime.Today.ToString("yyyy/MM/dd"), // التاريخ
                "", // الوصف
                "", // الحساب المدين
                "", // الحساب الدائن
                0, // المبلغ
                "دينار", // العملة - قيمة افتراضية
                "المستخدم الحالي", // المستخدم
                "", // تعديل - سيتم تعطيله
                ""  // حذف - سيتم تعطيله
            );

            // تعطيل أزرار التعديل والحذف في الصف الجديد
            dataGridViewEntries.Rows[0].Cells["تعديل"].ReadOnly = true;
            dataGridViewEntries.Rows[0].Cells["حذف"].ReadOnly = true;

            // تفعيل التحرير للصف الجديد فقط
            for (int i = 1; i < dataGridViewEntries.Rows.Count; i++)
            {
                dataGridViewEntries.Rows[i].ReadOnly = true;
            }
        }

        private void SaveNewEntry()
        {
            try
            {
                // التحقق من وجود بيانات في الصف الجديد
                if (dataGridViewEntries.Rows.Count == 0 ||
                    dataGridViewEntries.Rows[0].Cells["المبلغ"].Value == null ||
                    Convert.ToDecimal(dataGridViewEntries.Rows[0].Cells["المبلغ"].Value) <= 0)
                {
                    MessageBox.Show("يرجى إدخال مبلغ صحيح للقيد", "تحذير",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(dataGridViewEntries.Rows[0].Cells["الوصف"].Value?.ToString()))
                {
                    MessageBox.Show("يرجى إدخال وصف للقيد", "تحذير",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // إنشاء قيد بسيط جديد
                var newEntry = new SimpleEntry
                {
                    TheNumber = Convert.ToInt64(dataGridViewEntries.Rows[0].Cells["الرقم"].Value),
                    TheDate = DateTime.Parse(dataGridViewEntries.Rows[0].Cells["التاريخ"].Value.ToString()),
                    Description = dataGridViewEntries.Rows[0].Cells["الوصف"].Value?.ToString() ?? "",
                    AccountFrom = dataGridViewEntries.Rows[0].Cells["الحساب_المدين"].Value?.ToString() ?? "",
                    AccountTo = dataGridViewEntries.Rows[0].Cells["الحساب_الدائن"].Value?.ToString() ?? "",
                    Amount = Convert.ToDecimal(dataGridViewEntries.Rows[0].Cells["المبلغ"].Value),
                    Currency = dataGridViewEntries.Rows[0].Cells["العملة"].Value?.ToString() ?? "دينار",
                    Notes = "",
                    UserID = _currentUser?.UserID ?? 1,
                    EnterTime = DateTime.Now
                };

                // حفظ القيد
                bool success = _simpleEntriesService.AddSimpleEntry(newEntry);

                if (success)
                {
                    MessageBox.Show("تم حفظ القيد الجديد بنجاح", "نجاح",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    
                    // العودة للوضع العادي
                    _isNewMode = false;
                    btnSave.Enabled = false;
                    btnNew.Enabled = true;
                    dataGridViewEntries.ReadOnly = true;

                    LoadAllEntriesWithStatus(); // إعادة تحميل جميع البيانات
                }
                else
                {
                    MessageBox.Show("فشل في حفظ القيد الجديد", "خطأ",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في حفظ القيد: {ex.Message}", "خطأ",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void EditEntry(SimpleEntryView entry)
        {
            // سيتم تنفيذها في المرحلة القادمة
            MessageBox.Show($"سيتم تعديل القيد رقم: {entry.الرقم}", "تعديل",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void DeleteEntry(long entryID, long? entryNumber)
        {
            var result = MessageBox.Show($"هل أنت متأكد من حذف القيد رقم '{entryNumber}'؟",
                "تأكيد الحذف", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                try
                {
                    bool success = _simpleEntriesService.DeleteSimpleEntry(entryID);
                    if (success)
                    {
                        MessageBox.Show("تم حذف القيد بنجاح", "نجاح",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadAllEntriesWithStatus();
                    }
                    else
                    {
                        MessageBox.Show("فشل في حذف القيد", "خطأ",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"خطأ في حذف القيد: {ex.Message}", "خطأ",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BtnDiagnostics_Click(object sender, EventArgs e)
        {
            try
            {
                var diagnostics = new DatabaseDiagnostics(_databaseService);
                string diagnosticReport = diagnostics.RunDiagnostics();
                
                // إنشاء نافذة عرض التقرير
                var reportForm = new Form();
                reportForm.Text = "تقرير تشخيص قاعدة البيانات";
                reportForm.Size = new Size(800, 600);
                reportForm.StartPosition = FormStartPosition.CenterScreen;
                reportForm.RightToLeft = RightToLeft.Yes;
                reportForm.RightToLeftLayout = true;

                var textBox = new TextBox();
                textBox.Multiline = true;
                textBox.ScrollBars = ScrollBars.Both;
                textBox.Dock = DockStyle.Fill;
                textBox.Font = new Font("Courier New", 9);
                textBox.Text = diagnosticReport;
                textBox.ReadOnly = true;

                reportForm.Controls.Add(textBox);
                reportForm.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في تشغيل التشخيص: {ex.Message}", "خطأ",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DataGridViewEntries_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return; // التأكد من أن النقر ليس على الهيدر

            // النقر على زر التعديل
            if (e.ColumnIndex == dataGridViewEntries.Columns["تعديل"].Index)
            {
                long entryID = (long)dataGridViewEntries.Rows[e.RowIndex].Cells["ID"].Value;
                SimpleEntryView selectedEntry = _entryList.Find(entry => entry.ID == entryID);

                if (selectedEntry != null)
                {
                    EditEntry(selectedEntry);
                }
            }

            // النقر على زر الحذف
            if (e.ColumnIndex == dataGridViewEntries.Columns["حذف"].Index)
            {
                long entryID = (long)dataGridViewEntries.Rows[e.RowIndex].Cells["ID"].Value;
                long? entryNumber = (long?)dataGridViewEntries.Rows[e.RowIndex].Cells["الرقم"].Value;

                DeleteEntry(entryID, entryNumber);
            }
        }

        private void DataGridViewEntries_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            // الحصول على البيانات المُعدلة
            var row = dataGridViewEntries.Rows[e.RowIndex];
            long entryID = (long)row.Cells["ID"].Value;

            // البحث عن القيد في القائمة
            var entry = _entryList.Find(item => item.ID == entryID);
            if (entry != null)
            {
                // تحديث البيانات المُعدلة
                switch (dataGridViewEntries.Columns[e.ColumnIndex].Name)
                {
                    case "التاريخ":
                        if (DateTime.TryParse(row.Cells["التاريخ"].Value?.ToString(), out DateTime newDate))
                            entry.التاريخ = newDate;
                        break;
                    case "المبلغ":
                        if (decimal.TryParse(row.Cells["المبلغ"].Value?.ToString(), out decimal newAmount))
                            entry.المبلغ = newAmount;
                        break;
                    case "الوصف":
                        entry.الوصف = row.Cells["الوصف"].Value?.ToString();
                        break;
                    case "الحساب_المدين":
                        entry.الحساب_المدين = row.Cells["الحساب_المدين"].Value?.ToString();
                        break;
                    case "الحساب_الدائن":
                        entry.الحساب_الدائن = row.Cells["الحساب_الدائن"].Value?.ToString();
                        break;
                    case "العملة":
                        entry.العملة = row.Cells["العملة"].Value?.ToString();
                        break;
                }

                // إظهار رسالة تأكيد
                MessageBox.Show($"تم تعديل القيد رقم {entry.الرقم} بنجاح", "تم التعديل",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void BtnNew_Click(object sender, EventArgs e)
        {
            EnableEditMode();
            ClearDataGridViewForNewEntry();
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            SaveNewEntry();
        }

        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            LoadAllEntriesWithStatus(); // تحديث جميع السجلات مع عرض الحالة
        }

        private void BtnSearch_Click(object sender, EventArgs e)
        {
            // سيتم تنفيذها في المرحلة 2
            MessageBox.Show("سيتم فتح نافذة البحث المتقدم في المرحلة القادمة", "بحث متقدم",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}