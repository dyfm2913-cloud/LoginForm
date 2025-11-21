using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using Common.Models;
using System.Linq;
using DatabaseManager;

namespace FormsLibrary
{
    public partial class PaymentVouchersForm : Form
    {
        private DataGridView dataGridViewSpending;
        private Panel containerPanel;
        private Label scrollStatusLabel;
        private Button btnNew, btnSave, btnRefresh, btnSearch, btnClose, btnDiagnostics;
        private SpendingService _spendingService;
        private List<SpendingView> _spendingList;
        private DatabaseService _databaseService;
        private bool _isNewMode = false;
        private User _currentUser;

        public PaymentVouchersForm(DatabaseService databaseService, User currentUser = null)
        {
            _databaseService = databaseService;
            _currentUser = currentUser;
            _spendingService = new SpendingService(databaseService);
            InitializeComponent();
            SetupForm();
            AddControls();
            LoadAllSpendingWithStatus();
        }

        private void SetupForm()
        {
            this.Text = "إدارة سندات الصرف - جميع السجلات";
            this.Size = new Size(1200, 700);
            this.MinimumSize = new Size(1000, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.WindowState = FormWindowState.Maximized;
            this.RightToLeft = RightToLeft.Yes;
            this.RightToLeftLayout = true;
            this.BackColor = Color.White;
            this.Font = new Font("Tahoma", 9);
            this.AutoSize = false;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MaximizeBox = true;
            this.MinimizeBox = true;
            this.Resize += PaymentVouchersForm_Resize;
        }

        private void AddControls()
        {
            // Panel للشريط العلوي
            Panel toolbarPanel = new Panel();
            toolbarPanel.Height = 60;
            toolbarPanel.Dock = DockStyle.Top;
            toolbarPanel.BackColor = Color.LightGray;
            toolbarPanel.BorderStyle = BorderStyle.FixedSingle;
            
            AddToolbar(toolbarPanel);
            this.Controls.Add(toolbarPanel);

            // DataGridView مع scroll bars محدثة
            AddDataGridViewWithScrollBars();
        }

        private void PaymentVouchersForm_Resize(object sender, EventArgs e)
        {
            if (containerPanel != null && dataGridViewSpending != null)
            {
                // تحديث حجم الـ Container Panel
                containerPanel.Location = new Point(10, 70);
                containerPanel.Size = new Size(this.ClientSize.Width - 20, Math.Min(400, this.ClientSize.Height - 120));
                
                // تحديث حجم DataGridView مع مساحة محسوبة للـ scroll bars
                int gridWidth = containerPanel.ClientSize.Width - 20;
                int gridHeight = 250; // ارتفاع أصغر للـ DataGridView
                
                dataGridViewSpending.Size = new Size(gridWidth, gridHeight);
                dataGridViewSpending.Location = new Point(10, 40);
                
                // تحديث موضع علامة حالة التمرير
                if (scrollStatusLabel != null)
                {
                    scrollStatusLabel.Location = new Point(10, this.ClientSize.Height - 30);
                    scrollStatusLabel.Width = this.ClientSize.Width - 20;
                }
            }
        }

        private void AddToolbar(Panel toolbarPanel)
        {
            btnNew = new Button();
            btnNew.Text = "جديد";
            btnNew.Location = new Point(20, 10);
            btnNew.Size = new Size(80, 30);
            btnNew.Font = new Font("Tahoma", 9, FontStyle.Bold);
            btnNew.BackColor = Color.LightGreen;
            btnNew.Click += BtnNew_Click;
            toolbarPanel.Controls.Add(btnNew);

            btnSave = new Button();
            btnSave.Text = "حفظ";
            btnSave.Location = new Point(110, 10);
            btnSave.Size = new Size(80, 30);
            btnSave.Font = new Font("Tahoma", 9);
            btnSave.BackColor = Color.LightBlue;
            btnSave.Enabled = false;
            btnSave.Click += BtnSave_Click;
            toolbarPanel.Controls.Add(btnSave);

            btnRefresh = new Button();
            btnRefresh.Text = "تحديث";
            btnRefresh.Location = new Point(200, 10);
            btnRefresh.Size = new Size(80, 30);
            btnRefresh.Font = new Font("Tahoma", 9);
            btnRefresh.BackColor = Color.LightYellow;
            btnRefresh.Click += BtnRefresh_Click;
            toolbarPanel.Controls.Add(btnRefresh);

            btnSearch = new Button();
            btnSearch.Text = "بحث متقدم";
            btnSearch.Location = new Point(290, 10);
            btnSearch.Size = new Size(100, 30);
            btnSearch.Font = new Font("Tahoma", 9);
            btnSearch.BackColor = Color.LightCyan;
            btnSearch.Click += BtnSearch_Click;
            toolbarPanel.Controls.Add(btnSearch);

            btnDiagnostics = new Button();
            btnDiagnostics.Text = "تشخيص";
            btnDiagnostics.Location = new Point(400, 10);
            btnDiagnostics.Size = new Size(80, 30);
            btnDiagnostics.Font = new Font("Tahoma", 9);
            btnDiagnostics.BackColor = Color.LightSalmon;
            btnDiagnostics.Click += BtnDiagnostics_Click;
            toolbarPanel.Controls.Add(btnDiagnostics);

            btnClose = new Button();
            btnClose.Text = "إغلاق";
            btnClose.Location = new Point(490, 10);
            btnClose.Size = new Size(80, 30);
            btnClose.Font = new Font("Tahoma", 9);
            btnClose.BackColor = Color.LightCoral;
            btnClose.Click += BtnClose_Click;
            toolbarPanel.Controls.Add(btnClose);
        }

        private void AddDataGridViewWithScrollBars()
        {
            // إنشاء Panel حاوي مع AutoScroll
            containerPanel = new Panel();
            containerPanel.Location = new Point(10, 70);
            containerPanel.Size = new Size(this.ClientSize.Width - 20, this.ClientSize.Height - 120);
            containerPanel.AutoScroll = true;
            containerPanel.BorderStyle = BorderStyle.Fixed3D;
            containerPanel.BackColor = Color.White;
            containerPanel.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            containerPanel.HorizontalScroll.Visible = true;
            containerPanel.VerticalScroll.Visible = true;
            containerPanel.HorizontalScroll.Enabled = true;
            containerPanel.VerticalScroll.Enabled = true;
            
            // إضافة label للتمرير مع CSS-like styling
            containerPanel.Controls.Add(CreateScrollIndicator());
            
            // إنشاء DataGridView
            dataGridViewSpending = new DataGridView();
            dataGridViewSpending.Location = new Point(10, 40);
            dataGridViewSpending.Size = new Size(containerPanel.Width - 20, 250);
            dataGridViewSpending.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            
            // إعدادات مرئية محسّنة للـ scroll bars
            dataGridViewSpending.Font = new Font("Tahoma", 9);
            dataGridViewSpending.ReadOnly = false;
            dataGridViewSpending.SelectionMode = DataGridViewSelectionMode.CellSelect;
            
            // إعدادات الـ scroll bars - دائم الظهور والوضوح
            dataGridViewSpending.ScrollBars = ScrollBars.Both;
            
            // إعدادات النقرات والتنقل
            dataGridViewSpending.AllowUserToAddRows = false;
            dataGridViewSpending.AllowUserToDeleteRows = false;
            dataGridViewSpending.AllowUserToResizeRows = true;
            dataGridViewSpending.AllowUserToResizeColumns = true;
            
            // إعدادات الألوان والتصميم
            dataGridViewSpending.EnableHeadersVisualStyles = false;
            dataGridViewSpending.BackgroundColor = Color.White;
            dataGridViewSpending.DefaultCellStyle.BackColor = Color.White;
            dataGridViewSpending.AlternatingRowsDefaultCellStyle.BackColor = Color.LightGray;
            dataGridViewSpending.DefaultCellStyle.SelectionBackColor = Color.DodgerBlue;
            dataGridViewSpending.DefaultCellStyle.SelectionForeColor = Color.White;
            
            // إعداد الشبكة
            dataGridViewSpending.GridColor = Color.DarkBlue;
            dataGridViewSpending.BorderStyle = BorderStyle.FixedSingle;
            
            // إضافة الأعمدة
            AddColumnsToDataGridView();
            
            // ربط الأحداث
            dataGridViewSpending.CellClick += DataGridViewSpending_CellClick;
            dataGridViewSpending.CellEndEdit += DataGridViewSpending_CellEndEdit;
            dataGridViewSpending.Scroll += DataGridViewSpending_Scroll;
            dataGridViewSpending.DataError += DataGridViewSpending_DataError;
            
            // إضافة إلى الـ Panel
            containerPanel.Controls.Add(dataGridViewSpending);
            this.Controls.Add(containerPanel);
            
            // إضافة status bar
            AddScrollStatusLabel();
        }

        private Panel CreateScrollIndicator()
        {
            Panel indicatorPanel = new Panel();
            indicatorPanel.Location = new Point(0, 0);
            indicatorPanel.Size = new Size(containerPanel.Width, 25);
            indicatorPanel.BackColor = Color.LightBlue;
            indicatorPanel.BorderStyle = BorderStyle.FixedSingle;
            indicatorPanel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            
            Label indicatorLabel = new Label();
            indicatorLabel.Text = "أدوات التمرير: شريط التمرير الجانبي والعمودي مفعلان دائماً";
            indicatorLabel.Location = new Point(10, 5);
            indicatorLabel.Size = new Size(indicatorPanel.Width - 20, 15);
            indicatorLabel.TextAlign = ContentAlignment.MiddleRight;
            indicatorLabel.Font = new Font("Tahoma", 8, FontStyle.Bold);
            indicatorLabel.ForeColor = Color.DarkBlue;
            
            indicatorPanel.Controls.Add(indicatorLabel);
            return indicatorPanel;
        }

        private void AddColumnsToDataGridView()
        {
            // إضافة جميع الأعمدة
            dataGridViewSpending.Columns.Add("ID", "ID");
            dataGridViewSpending.Columns.Add("TheNumber", "IDالرقم المرجعي");
            dataGridViewSpending.Columns.Add("الرقم", "الرقم");
            dataGridViewSpending.Columns.Add("التاريخ", "التاريخ");
            dataGridViewSpending.Columns.Add("طريقة_الصرف", "طريقة الصرف");
            dataGridViewSpending.Columns.Add("المبلغ", "المبلغ");
            dataGridViewSpending.Columns.Add("العملة", "العملة");
            dataGridViewSpending.Columns.Add("الصندوق", "الصندوق");
            dataGridViewSpending.Columns.Add("مبلغ_الحساب", "مبلغ الحساب");
            dataGridViewSpending.Columns.Add("عملة_الحساب", "عملة الحساب");
            dataGridViewSpending.Columns.Add("اسم_الحساب", "اسم الحساب");
            dataGridViewSpending.Columns.Add("ملاحظات", "ملاحظات");
            dataGridViewSpending.Columns.Add("رقم_المرجع", "رقم المرجع");
            dataGridViewSpending.Columns.Add("مناولة", "مناولة");
            dataGridViewSpending.Columns.Add("مركز_التكلفة", "مركز التكلفة");
            dataGridViewSpending.Columns.Add("المستخدم", "المستخدم");
            dataGridViewSpending.Columns.Add("الفرع", "الفرع");
            dataGridViewSpending.Columns.Add("وقت_الإدخال", "وقت الإدخال");
            dataGridViewSpending.Columns.Add("الطبعات", "الطبعات");
            dataGridViewSpending.Columns.Add("رقم_الشيك_الخاص", "رقم الشيك الخاص");
            dataGridViewSpending.Columns.Add("اسم_المفوض", "اسم المفوض");
            dataGridViewSpending.Columns.Add("الصرف_للمفوض", "الصرف للمفوض");
            dataGridViewSpending.Columns.Add("معتمد", "معتمد");
            dataGridViewSpending.Columns.Add("المعتمد", "المعتمد");
            dataGridViewSpending.Columns.Add("الفئات", "الفئات");

            // أزرار التعديل والحذف
            DataGridViewButtonColumn editColumn = new DataGridViewButtonColumn();
            editColumn.Name = "تعديل";
            editColumn.HeaderText = "تعديل";
            editColumn.Text = "تعديل";
            editColumn.UseColumnTextForButtonValue = true;
            editColumn.Width = 80;
            editColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
            dataGridViewSpending.Columns.Add(editColumn);

            DataGridViewButtonColumn deleteColumn = new DataGridViewButtonColumn();
            deleteColumn.Name = "حذف";
            deleteColumn.HeaderText = "حذف";
            deleteColumn.Text = "حذف";
            deleteColumn.UseColumnTextForButtonValue = true;
            deleteColumn.Width = 80;
            deleteColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
            dataGridViewSpending.Columns.Add(deleteColumn);

            // إعداد الأعمدة القابلة للتحرير - جميع الحقول قابلة للتعديل والحذف
            dataGridViewSpending.Columns["ID"].ReadOnly = true; // ID فقط للحماية

            // تنسيق الأعمدة الرقمية
            dataGridViewSpending.Columns["المبلغ"].DefaultCellStyle.Format = "N2";
            dataGridViewSpending.Columns["مبلغ_الحساب"].DefaultCellStyle.Format = "N2";
        }

        private void AddScrollStatusLabel()
        {
            scrollStatusLabel = new Label();
            scrollStatusLabel.Location = new Point(10, this.ClientSize.Height - 30);
            scrollStatusLabel.Size = new Size(this.ClientSize.Width - 20, 25);
            scrollStatusLabel.Text = "تم تحميل البيانات بنجاح - أدوات التمرير مفعلة";
            scrollStatusLabel.TextAlign = ContentAlignment.MiddleRight;
            scrollStatusLabel.Font = new Font("Tahoma", 9, FontStyle.Bold);
            scrollStatusLabel.BackColor = Color.LightBlue;
            scrollStatusLabel.ForeColor = Color.DarkBlue;
            scrollStatusLabel.BorderStyle = BorderStyle.FixedSingle;
            scrollStatusLabel.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            
            this.Controls.Add(scrollStatusLabel);
        }

        private void DataGridViewSpending_Scroll(object sender, ScrollEventArgs e)
        {
            UpdateScrollStatus(e);
        }

        private void UpdateScrollStatus(ScrollEventArgs e)
        {
            if (scrollStatusLabel != null && dataGridViewSpending != null)
            {
                string status = $"السجلات: {dataGridViewSpending.Rows.Count} | ";
                
                if (dataGridViewSpending.Rows.Count > 0)
                {
                    int firstVisible = dataGridViewSpending.FirstDisplayedScrollingRowIndex;
                    int lastVisible = firstVisible + dataGridViewSpending.DisplayedRowCount(true) - 1;
                    
                    status += $"عرض من {firstVisible + 1} إلى {lastVisible + 1}";
                    
                    // مؤشر حالة التمرير
                    if (dataGridViewSpending.Rows.Count > dataGridViewSpending.DisplayedRowCount(true))
                    {
                        status += " | ✓ شريط التمرير الجانبي متاح";
                    }
                    
                    if (dataGridViewSpending.Columns.Count * 150 > dataGridViewSpending.Width)
                    {
                        status += " | ✓ شريط التمرير السفلي متاح";
                    }
                }
                
                scrollStatusLabel.Text = status;
            }
        }

        private void DataGridViewSpending_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            // التنقل السلس بدون رسائل - يتم تحرير البيانات بصمت
            // لا يتم إظهار أي رسائل عند التنقل بين الأعمدة
        }

        private void RefreshDataGridView(List<SpendingView> spendingList)
        {
            try
            {
                dataGridViewSpending.Rows.Clear();

                foreach (var spending in spendingList)
                {
                    // تحميل البيانات كما هي من قاعدة البيانات دون تغيير
                    dataGridViewSpending.Rows.Add(
                        spending.ID,
                        spending.TheNumber,
                        spending.الرقم,
                        spending.التاريخ?.ToString(),
                        spending.طريقة_الصرف,
                        spending.المبلغ,
                        spending.العملة,
                        spending.AccountID,
                        spending.ExchangeAmount,
                        spending.ExchangeCurrencyID,
                        spending.ExchangeAccountID,
                        spending.ملاحظات,
                        spending.RefernceNumber,
                        spending.Delivery,
                        spending.CostCenterID,
                        spending.المستخدم,
                        spending.BranchID,
                        spending.EnterTime?.ToString(),
                        spending.Prints,
                        spending.SpecialChequeNumber,
                        spending.CommissionerID,
                        spending.IsCommissioner?.ToString(),
                        spending.IsDepend?.ToString(),
                        spending.DependUserID,
                        spending.ChequeID
                    );
                }
                
                UpdateScrollStatus(new ScrollEventArgs(ScrollEventType.EndScroll, 0, ScrollOrientation.VerticalScroll));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في تحديث البيانات: {ex.Message}", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateStatusBar(int recordCount)
        {
            this.Text = $"إدارة سندات الصرف - عدد السجلات: {recordCount}";
            if (scrollStatusLabel != null)
            {
                scrollStatusLabel.Text = $"تم تحميل {recordCount} سجل بنجاح - أدوات التمرير مفعلة";
            }
        }

        private void DataGridViewSpending_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            if (e.ColumnIndex == dataGridViewSpending.Columns["تعديل"].Index)
            {
                long spendingID = (long)dataGridViewSpending.Rows[e.RowIndex].Cells["ID"].Value;
                SpendingView selectedSpending = _spendingList.Find(s => s.ID == spendingID);

                if (selectedSpending != null)
                {
                    EditSpending(selectedSpending);
                }
            }

            if (e.ColumnIndex == dataGridViewSpending.Columns["حذف"].Index)
            {
                long spendingID = (long)dataGridViewSpending.Rows[e.RowIndex].Cells["ID"].Value;
                long? voucherNumber = (long?)dataGridViewSpending.Rows[e.RowIndex].Cells["الرقم"].Value;

                DeleteSpending(spendingID, voucherNumber);
            }
        }

        private void BtnNew_Click(object sender, EventArgs e)
        {
            EnableEditMode();
            // Clear all rows and show empty grid - no default values
            dataGridViewSpending.Rows.Clear();
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            SaveNewSpending();
        }

        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            LoadAllSpendingWithStatus();
        }

        private void BtnSearch_Click(object sender, EventArgs e)
        {
            ShowAdvancedSearchDialog();
        }

        private void ShowAdvancedSearchDialog()
        {
            var searchForm = new Form();
            searchForm.Text = "البحث المتقدم - سندات الصرف";
            searchForm.Size = new Size(500, 300);
            searchForm.StartPosition = FormStartPosition.CenterScreen;
            searchForm.FormBorderStyle = FormBorderStyle.FixedDialog;
            searchForm.MaximizeBox = false;
            searchForm.RightToLeft = RightToLeft.Yes;
            searchForm.RightToLeftLayout = true;

            var lblSearch = new Label();
            lblSearch.Text = "البحث:";
            lblSearch.Location = new Point(50, 30);
            lblSearch.Size = new Size(80, 25);
            lblSearch.Font = new Font("Tahoma", 10, FontStyle.Bold);
            lblSearch.TextAlign = ContentAlignment.MiddleRight;
            searchForm.Controls.Add(lblSearch);

            var txtSearch = new TextBox();
            txtSearch.Location = new Point(50, 60);
            txtSearch.Size = new Size(350, 25);
            txtSearch.Font = new Font("Tahoma", 10);
            searchForm.Controls.Add(txtSearch);

            var lblFromDate = new Label();
            lblFromDate.Text = "من تاريخ:";
            lblFromDate.Location = new Point(50, 100);
            lblFromDate.Size = new Size(80, 25);
            lblFromDate.Font = new Font("Tahoma", 10);
            lblFromDate.TextAlign = ContentAlignment.MiddleRight;
            searchForm.Controls.Add(lblFromDate);

            var dtpFromDate = new DateTimePicker();
            dtpFromDate.Location = new Point(50, 130);
            dtpFromDate.Size = new Size(200, 25);
            dtpFromDate.Format = DateTimePickerFormat.Short;
            searchForm.Controls.Add(dtpFromDate);

            var lblToDate = new Label();
            lblToDate.Text = "إلى تاريخ:";
            lblToDate.Location = new Point(50, 165);
            lblToDate.Size = new Size(80, 25);
            lblToDate.Font = new Font("Tahoma", 10);
            lblToDate.TextAlign = ContentAlignment.MiddleRight;
            searchForm.Controls.Add(lblToDate);

            var dtpToDate = new DateTimePicker();
            dtpToDate.Location = new Point(50, 195);
            dtpToDate.Size = new Size(200, 25);
            dtpToDate.Format = DateTimePickerFormat.Short;
            dtpToDate.Value = DateTime.Now;
            searchForm.Controls.Add(dtpToDate);

            var btnSearch = new Button();
            btnSearch.Text = "بحث";
            btnSearch.Location = new Point(150, 230);
            btnSearch.Size = new Size(80, 30);
            btnSearch.Font = new Font("Tahoma", 10, FontStyle.Bold);
            btnSearch.BackColor = Color.LightGreen;
            btnSearch.Click += (s, args) =>
            {
                try
                {
                    var filteredResults = PerformAdvancedSearch(txtSearch.Text.Trim(), dtpFromDate.Value, dtpToDate.Value);
                    
                    if (filteredResults.Count > 0)
                    {
                        RefreshDataGridView(filteredResults);
                        UpdateStatusBar(filteredResults.Count);
                        searchForm.Close();
                        
                        MessageBox.Show($"تم العثور على {filteredResults.Count} نتيجة", "نتائج البحث",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("لم يتم العثور على نتائج", "لا توجد نتائج",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"خطأ في البحث: {ex.Message}", "خطأ",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };
            searchForm.Controls.Add(btnSearch);

            var btnCancel = new Button();
            btnCancel.Text = "إلغاء";
            btnCancel.Location = new Point(250, 230);
            btnCancel.Size = new Size(80, 30);
            btnCancel.Font = new Font("Tahoma", 10);
            btnCancel.BackColor = Color.LightGray;
            btnCancel.Click += (s, args) => searchForm.Close();
            searchForm.Controls.Add(btnCancel);

            searchForm.ShowDialog();
        }

        private List<SpendingView> PerformAdvancedSearch(string searchTerm, DateTime fromDate, DateTime toDate)
        {
            var allSpending = _spendingService.GetAllSpending();
            
            var filteredByDate = allSpending.Where(s =>
                s.التاريخ.HasValue &&
                s.التاريخ.Value.Date >= fromDate.Date &&
                s.التاريخ.Value.Date <= toDate.Date).ToList();

            if (string.IsNullOrWhiteSpace(searchTerm))
                return filteredByDate;

            return filteredByDate.Where(s =>
                (s.الرقم?.ToString().Contains(searchTerm) == true) ||
                (s.المبلغ?.ToString().Contains(searchTerm) == true) ||
                (s.ملاحظات?.Contains(searchTerm) == true) ||
                (s.المستخدم?.Contains(searchTerm) == true)
            ).ToList();
        }

        private void BtnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void LoadAllSpendingWithStatus()
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                this.Text = "إدارة سندات الصرف - جاري التحميل...";
                if (scrollStatusLabel != null)
                {
                    scrollStatusLabel.Text = "جاري تحميل البيانات... أدوات التمرير جاهزة";
                }

                _spendingList = _spendingService.GetAllSpending();
                RefreshDataGridView(_spendingList);
                UpdateStatusBar(_spendingList.Count);

                this.Text = $"إدارة سندات الصرف - جميع السجلات ({_spendingList.Count})";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في تحميل سندات الصرف: {ex.Message}", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Text = "إدارة سندات الصرف - خطأ في التحميل";
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void EnableEditMode()
        {
            _isNewMode = true;
            btnSave.Enabled = true;
            btnNew.Enabled = false;
            dataGridViewSpending.ReadOnly = false;
        }

        // Remove the ClearDataGridViewForNewEntry method as we don't want default values
        // The method is no longer needed

        private long GetNextVoucherNumber()
        {
            if (_spendingList.Count > 0)
            {
                long lastNumber = _spendingList.Max(s => s.الرقم ?? 0);
                return lastNumber + 1;
            }
            return 1;
        }

        private void SaveNewSpending()
        {
            try
            {
                if (dataGridViewSpending.Rows.Count == 0)
                {
                    MessageBox.Show("لا توجد بيانات لحفظها", "تحذير",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Check if required fields are filled
                var firstRow = dataGridViewSpending.Rows[0];
                if (firstRow.Cells["المبلغ"].Value == null ||
                    string.IsNullOrWhiteSpace(firstRow.Cells["المبلغ"].Value.ToString()) ||
                    Convert.ToDecimal(firstRow.Cells["المبلغ"].Value) <= 0)
                {
                    MessageBox.Show("يرجى إدخال مبلغ صحيح للسند", "تحذير",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var newSpending = new Spending
                {
                    TheNumber = GetNextVoucherNumber(),
                    TheDate = firstRow.Cells["التاريخ"].Value != null ?
                              DateTime.Parse(firstRow.Cells["التاريخ"].Value.ToString()) : DateTime.Now,
                    TheMethod = GetPaymentMethodId(firstRow.Cells["طريقة_الصرف"].Value?.ToString()),
                    Amount = Convert.ToDecimal(firstRow.Cells["المبلغ"].Value),
                    CurrencyID = GetCurrencyId(firstRow.Cells["العملة"].Value?.ToString()),
                    AccountID = firstRow.Cells["الصندوق"].Value != null ?
                                Convert.ToInt64(firstRow.Cells["الصندوق"].Value) : 1L,
                    Notes = firstRow.Cells["ملاحظات"].Value?.ToString() ?? "",
                    UserID = _currentUser?.UserID ?? 1,
                    EnterTime = DateTime.Now
                };

                bool success = _spendingService.AddSpending(newSpending);

                if (success)
                {
                    MessageBox.Show("تم حفظ السند الجديد بنجاح", "نجاح",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    
                    _isNewMode = false;
                    btnSave.Enabled = false;
                    btnNew.Enabled = true;
                    dataGridViewSpending.ReadOnly = true;

                    // Clear the grid and reload data from View
                    dataGridViewSpending.Rows.Clear();
                    LoadAllSpendingWithStatus();
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

        private void EditSpending(SpendingView spending)
        {
            MessageBox.Show($"سيتم تعديل السند رقم: {spending.الرقم}", "تعديل",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void DeleteSpending(long spendingID, long? voucherNumber)
        {
            var result = MessageBox.Show($"هل أنت متأكد من حذف سند الصرف رقم '{voucherNumber}'؟",
                "تأكيد الحذف", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                try
                {
                    bool success = _spendingService.DeleteSpending(spendingID);
                    if (success)
                    {
                        MessageBox.Show("تم حذف سند الصرف بنجاح", "نجاح",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadAllSpendingWithStatus();
                    }
                    else
                    {
                        MessageBox.Show("فشل في حذف سند الصرف", "خطأ",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"خطأ في حذف سند الصرف: {ex.Message}", "خطأ",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void DataGridViewSpending_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            e.ThrowException = false;
            System.Diagnostics.Debug.WriteLine($"DataGridView Error: {e.Exception?.Message}");
        }

        private long? GetPaymentMethodId(string methodName)
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
    }
}