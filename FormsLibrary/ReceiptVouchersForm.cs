using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using Common.Models;
using System.Linq;
using DatabaseManager;

namespace FormsLibrary
{
    public partial class ReceiptVouchersForm : Form
    {
        private DataGridView dataGridViewReceipts;
        private Button btnNew, btnSave, btnRefresh, btnSearch, btnClose, btnDiagnostics;
        private ReceiptService _receiptService;
        private List<ReceiptView> _receiptList;
        private DatabaseService _databaseService;
        private bool _isNewMode = false; // يُستخدم للتحكم في وضع الإدخال الجديد
        private User _currentUser; // إضافة المستخدم الحالي

        public ReceiptVouchersForm(DatabaseService databaseService, User currentUser = null)
        {
            _databaseService = databaseService;
            _currentUser = currentUser;
            _receiptService = new ReceiptService(databaseService);
            
            SetupForm();
            AddControls();
            LoadAllReceiptsWithStatus(); // تحميل جميع البيانات مع عرض الحالة
        }

        private void SetupForm()
        {
            this.Text = "إدارة سندات القبض - جميع السجلات";
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

            // DataGridView لعرض سندات القبض
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
            dataGridViewReceipts = new DataGridView();
            dataGridViewReceipts.Location = new Point(20, 60);
            dataGridViewReceipts.Size = new Size(1160, 600);
            dataGridViewReceipts.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dataGridViewReceipts.Font = new Font("Tahoma", 9);
            dataGridViewReceipts.ReadOnly = false;
            dataGridViewReceipts.SelectionMode = DataGridViewSelectionMode.CellSelect;
            dataGridViewReceipts.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridViewReceipts.RightToLeft = RightToLeft.Yes;
            dataGridViewReceipts.RowHeadersVisible = false;
            dataGridViewReceipts.AllowUserToAddRows = false;
            dataGridViewReceipts.EditMode = DataGridViewEditMode.EditOnEnter;

            // إضافة الأعمدة الأساسية
            dataGridViewReceipts.Columns.Add("ID", "ID");
            dataGridViewReceipts.Columns.Add("الرقم", "رقم السند");
            dataGridViewReceipts.Columns.Add("التاريخ", "التاريخ");
            dataGridViewReceipts.Columns.Add("طريقة_القبض", "طريقة القبض");
            dataGridViewReceipts.Columns.Add("المبلغ", "المبلغ");
            dataGridViewReceipts.Columns.Add("العملة", "العملة");
            dataGridViewReceipts.Columns.Add("اسم_الحساب", "الحساب");
            dataGridViewReceipts.Columns.Add("المستخدم", "المستخدم");
            dataGridViewReceipts.Columns.Add("معتمد", "معتمد");

            // إعداد الأعمدة القابلة للتحرير
            dataGridViewReceipts.Columns["ID"].ReadOnly = true; // ID غير قابل للتحرير
            dataGridViewReceipts.Columns["الرقم"].ReadOnly = true; // رقم السند غير قابل للتحرير
            dataGridViewReceipts.Columns["التاريخ"].ReadOnly = false; // التاريخ قابل للتحرير
            dataGridViewReceipts.Columns["طريقة_القبض"].ReadOnly = false; // طريقة القبض قابلة للتحرير
            dataGridViewReceipts.Columns["المبلغ"].ReadOnly = false; // المبلغ قابل للتحرير
            dataGridViewReceipts.Columns["العملة"].ReadOnly = false; // العملة قابلة للتحرير
            dataGridViewReceipts.Columns["اسم_الحساب"].ReadOnly = false; // الحساب قابل للتحرير
            dataGridViewReceipts.Columns["المستخدم"].ReadOnly = true; // المستخدم غير قابل للتحرير
            dataGridViewReceipts.Columns["معتمد"].ReadOnly = true; // معتمد غير قابل للتحرير

            // إضافة عمود التعديل
            DataGridViewButtonColumn editColumn = new DataGridViewButtonColumn();
            editColumn.Name = "تعديل";
            editColumn.HeaderText = "تعديل";
            editColumn.Text = "تعديل";
            editColumn.UseColumnTextForButtonValue = true;
            editColumn.Width = 60;
            editColumn.ReadOnly = true;
            dataGridViewReceipts.Columns.Add(editColumn);

            // إضافة عمود الحذف
            DataGridViewButtonColumn deleteColumn = new DataGridViewButtonColumn();
            deleteColumn.Name = "حذف";
            deleteColumn.HeaderText = "حذف";
            deleteColumn.Text = "حذف";
            deleteColumn.UseColumnTextForButtonValue = true;
            deleteColumn.Width = 60;
            deleteColumn.ReadOnly = true;
            dataGridViewReceipts.Columns.Add(deleteColumn);

            // تنسيق الأعمدة الرقمية
            dataGridViewReceipts.Columns["المبلغ"].DefaultCellStyle.Format = "N2";
            dataGridViewReceipts.Columns["المبلغ"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

            // ربط حدث النقر على الأزرار
            dataGridViewReceipts.CellClick += DataGridViewReceipts_CellClick;

            // ربط حدث عند انتهاء التحرير
            dataGridViewReceipts.CellEndEdit += DataGridViewReceipts_CellEndEdit;

            AddComboBoxColumns();

            this.Controls.Add(dataGridViewReceipts);
        }

        private void LoadAllReceiptsWithStatus()
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                this.Text = "إدارة سندات القبض - جاري التحميل...";

                _receiptList = _receiptService.GetAllReceipts();
                RefreshDataGridView(_receiptList);
                UpdateStatusBar(_receiptList.Count);

                this.Text = $"إدارة سندات القبض - جميع السجلات ({_receiptList.Count})";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في تحميل سندات القبض: {ex.Message}", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Text = "إدارة سندات القبض - خطأ في التحميل";
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void RefreshDataGridView(List<ReceiptView> receiptList)
        {
            dataGridViewReceipts.Rows.Clear();

            foreach (var receipt in receiptList)
            {
                dataGridViewReceipts.Rows.Add(
                    receipt.ID,
                    receipt.الرقم,
                    receipt.التاريخ?.ToString("yyyy/MM/dd"),
                    receipt.طريقة_القبض,
                    receipt.المبلغ,
                    receipt.العملة,
                    receipt.اسم_الحساب,
                    receipt.المستخدم,
                    receipt.معتمد.HasValue && receipt.معتمد.Value ? "نعم" : "لا"
                );
            }
        }

        private void UpdateStatusBar(int recordCount)
        {
            this.Text = $"إدارة سندات القبض - عدد السجلات: {recordCount}";
        }

        private void AddComboBoxColumns()
        {
            // Combobox لطريقة القبض
            DataGridViewComboBoxColumn methodColumn = new DataGridViewComboBoxColumn();
            methodColumn.Name = "طريقة_القبض";
            methodColumn.HeaderText = "طريقة القبض";
            methodColumn.Width = 120;

            // إضافة العناصر
            methodColumn.Items.Add("نقدي");
            methodColumn.Items.Add("شيك");
            methodColumn.Items.Add("تحويل بنكي");
            methodColumn.Items.Add("بطاقة ائتمان");

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

            // استبدال الأعمدة النصية
            if (dataGridViewReceipts.Columns.Contains("طريقة_القبض"))
            {
                int columnIndex = dataGridViewReceipts.Columns["طريقة_القبض"].Index;
                dataGridViewReceipts.Columns.Remove("طريقة_القبض");
                dataGridViewReceipts.Columns.Insert(columnIndex, methodColumn);
            }

            if (dataGridViewReceipts.Columns.Contains("العملة"))
            {
                int columnIndex = dataGridViewReceipts.Columns["العملة"].Index;
                dataGridViewReceipts.Columns.Remove("العملة");
                dataGridViewReceipts.Columns.Insert(columnIndex, currencyColumn);
            }
        }

        private long GetNextVoucherNumber()
        {
            // الحصول على آخر رقم سند وإضافة 1
            if (_receiptList.Count > 0)
            {
                long lastNumber = _receiptList.Max(r => r.الرقم ?? 0);
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
            dataGridViewReceipts.ReadOnly = false;

            // تظليل الصف الجديد للإشارة إلى وضع الإدخال
            if (dataGridViewReceipts.Rows.Count > 0)
            {
                dataGridViewReceipts.Rows[0].DefaultCellStyle.BackColor = Color.LightYellow;

                // جعل الأعمدة قابلة للتحرير في الصف الجديد فقط
                foreach (DataGridViewColumn column in dataGridViewReceipts.Columns)
                {
                    if (column.Name != "ID" && column.Name != "تعديل" && column.Name != "حذف")
                    {
                        dataGridViewReceipts.Rows[0].Cells[column.Index].ReadOnly = false;
                    }
                }
            }
        }

        private void ClearDataGridViewForNewEntry()
        {
            // إضافة صف جديد للإدخال
            dataGridViewReceipts.Rows.Insert(0,
                null, // ID
                GetNextVoucherNumber(), // الرقم
                DateTime.Today.ToString("yyyy/MM/dd"), // التاريخ
                "نقدي", // طريقة القبض - قيمة افتراضية
                0, // المبلغ
                "دينار", // العملة - قيمة افتراضية
                "", // الحساب - فارغ للإدخال
                "المستخدم الحالي", // المستخدم
                "لا", // معتمد
                "", // تعديل - سيتم تعطيله
                ""  // حذف - سيتم تعطيله
            );

            // تعطيل أزرار التعديل والحذف في الصف الجديد
            dataGridViewReceipts.Rows[0].Cells["تعديل"].ReadOnly = true;
            dataGridViewReceipts.Rows[0].Cells["حذف"].ReadOnly = true;

            // تفعيل التحرير للصف الجديد فقط
            for (int i = 1; i < dataGridViewReceipts.Rows.Count; i++)
            {
                dataGridViewReceipts.Rows[i].ReadOnly = true;
            }
        }

        private void SaveNewReceipt()
        {
            try
            {
                // التحقق من وجود بيانات في الصف الجديد
                if (dataGridViewReceipts.Rows.Count == 0 ||
                    dataGridViewReceipts.Rows[0].Cells["المبلغ"].Value == null ||
                    Convert.ToDecimal(dataGridViewReceipts.Rows[0].Cells["المبلغ"].Value) <= 0)
                {
                    MessageBox.Show("يرجى إدخال مبلغ صحيح للسند", "تحذير",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // إنشاء سند قبض جديد
                var newReceipt = new Receipt
                {
                    TheNumber = Convert.ToInt64(dataGridViewReceipts.Rows[0].Cells["الرقم"].Value),
                    TheDate = DateTime.Parse(dataGridViewReceipts.Rows[0].Cells["التاريخ"].Value.ToString()),
                    TheMethod = GetReceiptMethodId(dataGridViewReceipts.Rows[0].Cells["طريقة_القبض"].Value?.ToString()),
                    Amount = Convert.ToDecimal(dataGridViewReceipts.Rows[0].Cells["المبلغ"].Value),
                    CurrencyID = GetCurrencyId(dataGridViewReceipts.Rows[0].Cells["العملة"].Value?.ToString()),
                    AccountID = 1, // حساب افتراضي - سيتم تحديثه لاحقاً
                    Notes = dataGridViewReceipts.Rows[0].Cells["اسم_الحساب"].Value?.ToString() ?? "",
                    UserID = _currentUser?.UserID ?? 1,
                    EnterTime = DateTime.Now
                };

                // حفظ السند
                bool success = _receiptService.AddReceipt(newReceipt);

                if (success)
                {
                    MessageBox.Show("تم حفظ السند الجديد بنجاح", "نجاح",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    
                    // العودة للوضع العادي
                    _isNewMode = false;
                    btnSave.Enabled = false;
                    btnNew.Enabled = true;
                    dataGridViewReceipts.ReadOnly = true;

                    LoadAllReceiptsWithStatus(); // إعادة تحميل جميع البيانات
                }
                else
                {
                    MessageBox.Show("فشل في حفظ السند الجديد", "خطأ",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في حفظ السند: {ex.Message}", "خطأ",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void EditReceipt(ReceiptView receipt)
        {
            // سيتم تنفيذها في المرحلة القادمة
            MessageBox.Show($"سيتم تعديل السند رقم: {receipt.الرقم}", "تعديل",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void DeleteReceipt(long receiptID, long? voucherNumber)
        {
            var result = MessageBox.Show($"هل أنت متأكد من حذف سند القبض رقم '{voucherNumber}'؟",
                "تأكيد الحذف", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                try
                {
                    bool success = _receiptService.DeleteReceipt(receiptID);
                    if (success)
                    {
                        MessageBox.Show("تم حذف سند القبض بنجاح", "نجاح",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadAllReceiptsWithStatus();
                    }
                    else
                    {
                        MessageBox.Show("فشل في حذف سند القبض", "خطأ",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"خطأ في حذف سند القبض: {ex.Message}", "خطأ",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // دوال مساعدة لتحويل القيم
        private long? GetReceiptMethodId(string methodName)
        {
            switch (methodName?.ToLower())
            {
                case "نقدي": return 1;
                case "شيك": return 2;
                case "تحويل بنكي": return 3;
                case "بطاقة ائتمان": return 4;
                default: return 1;
            }
        }

        private long? GetCurrencyId(string currencyName)
        {
            switch (currencyName?.ToLower())
            {
                case "دينار": return 1;
                case "دولار": return 2;
                case "يورو": return 3;
                case "ريال": return 4;
                default: return 1;
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

        private void DataGridViewReceipts_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return; // التأكد من أن النقر ليس على الهيدر

            // النقر على زر التعديل
            if (e.ColumnIndex == dataGridViewReceipts.Columns["تعديل"].Index)
            {
                long receiptID = (long)dataGridViewReceipts.Rows[e.RowIndex].Cells["ID"].Value;
                ReceiptView selectedReceipt = _receiptList.Find(r => r.ID == receiptID);

                if (selectedReceipt != null)
                {
                    EditReceipt(selectedReceipt);
                }
            }

            // النقر على زر الحذف
            if (e.ColumnIndex == dataGridViewReceipts.Columns["حذف"].Index)
            {
                long receiptID = (long)dataGridViewReceipts.Rows[e.RowIndex].Cells["ID"].Value;
                long? voucherNumber = (long?)dataGridViewReceipts.Rows[e.RowIndex].Cells["الرقم"].Value;

                DeleteReceipt(receiptID, voucherNumber);
            }
        }

        private void DataGridViewReceipts_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            // الحصول على البيانات المُعدلة
            var row = dataGridViewReceipts.Rows[e.RowIndex];
            long receiptID = (long)row.Cells["ID"].Value;

            // البحث عن السند في القائمة
            var receipt = _receiptList.Find(r => r.ID == receiptID);
            if (receipt != null)
            {
                // تحديث البيانات المُعدلة
                switch (dataGridViewReceipts.Columns[e.ColumnIndex].Name)
                {
                    case "التاريخ":
                        if (DateTime.TryParse(row.Cells["التاريخ"].Value?.ToString(), out DateTime newDate))
                            receipt.التاريخ = newDate;
                        break;
                    case "المبلغ":
                        if (decimal.TryParse(row.Cells["المبلغ"].Value?.ToString(), out decimal newAmount))
                            receipt.المبلغ = newAmount;
                        break;
                    case "طريقة_القبض":
                        receipt.طريقة_القبض = row.Cells["طريقة_القبض"].Value?.ToString();
                        break;
                    case "العملة":
                        receipt.العملة = row.Cells["العملة"].Value?.ToString();
                        break;
                    case "اسم_الحساب":
                        receipt.اسم_الحساب = row.Cells["اسم_الحساب"].Value?.ToString();
                        break;
                }

                // إظهار رسالة تأكيد
                MessageBox.Show($"تم تعديل السند رقم {receipt.الرقم} بنجاح", "تم التعديل",
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
            SaveNewReceipt();
        }

        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            LoadAllReceiptsWithStatus(); // تحديث جميع السجلات مع عرض الحالة
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