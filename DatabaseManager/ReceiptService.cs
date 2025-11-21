using System;
using System.Data.SqlClient;
using System.Collections.Generic;
using Common.Models;
using Common.Enums;

namespace DatabaseManager
{
    public class ReceiptService
    {
        private readonly DatabaseService _dbService;

        public ReceiptService(DatabaseService dbService)
        {
            _dbService = dbService;
        }

        // جلب جميع سندات القبض مع بيانات كاملة من الفيو
        public List<ReceiptView> GetAllReceipts()
        {
            var receiptList = new List<ReceiptView>();

            using (var connection = _dbService.GetConnection(DatabaseType.AppDatabase))
            {
                connection.Open();

                // محاولة الوصول للفيو أولاً
                string viewQuery = @"SELECT 
                    vw.ID,
                    vw.الرقم,
                    vw.التاريخ,
                    vw.طريقة_القبض,
                    vw.المبلغ,
                    vw.العملة,
                    vw.اسم_الحساب,
                    vw.المستخدم,
                    vw.معتمد
                    FROM vw_ReceiptView vw
                    ORDER BY vw.التاريخ DESC, vw.ID DESC";

                try
                {
                    using (var command = new SqlCommand(viewQuery, connection))
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            receiptList.Add(new ReceiptView
                            {
                                ID = reader.IsDBNull(reader.GetOrdinal("ID")) ? 0 : reader.GetInt64(reader.GetOrdinal("ID")),
                                الرقم = reader.IsDBNull(reader.GetOrdinal("الرقم")) ? (long?)null : reader.GetInt64(reader.GetOrdinal("الرقم")),
                                التاريخ = reader.IsDBNull(reader.GetOrdinal("التاريخ")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("التاريخ")),
                                طريقة_القبض = reader.IsDBNull(reader.GetOrdinal("طريقة_القبض")) ? "نقدي" : reader.GetString(reader.GetOrdinal("طريقة_القبض")),
                                المبلغ = reader.IsDBNull(reader.GetOrdinal("المبلغ")) ? (decimal?)null : reader.GetDecimal(reader.GetOrdinal("المبلغ")),
                                العملة = reader.IsDBNull(reader.GetOrdinal("العملة")) ? "دينار" : reader.GetString(reader.GetOrdinal("العملة")),
                                اسم_الحساب = reader.IsDBNull(reader.GetOrdinal("اسم_الحساب")) ? "" : reader.GetString(reader.GetOrdinal("اسم_الحساب")),
                                المستخدم = reader.IsDBNull(reader.GetOrdinal("المستخدم")) ? "مستخدم غير محدد" : reader.GetString(reader.GetOrdinal("المستخدم")),
                                معتمد = reader.IsDBNull(reader.GetOrdinal("معتمد")) ? (bool?)null : reader.GetBoolean(reader.GetOrdinal("معتمد"))
                            });
                        }
                    }
                    return receiptList; // إذا نجح الفيو، Return البيانات
                }
                catch
                {
                    // إذا فشل الفيو، استخدم الاستعلام البديل من الجدول الأساسي
                    return GetReceiptsFromBasicTable(connection);
                }
            }
        }

        // استعلام بديل من الجدول الأساسي إذا لم يوجد الفيو
        private List<ReceiptView> GetReceiptsFromBasicTable(SqlConnection connection)
        {
            var receiptList = new List<ReceiptView>();

            string tableQuery = @"SELECT 
                r.ID,
                r.TheNumber as الرقم,
                r.TheDate as التاريخ,
                r.Amount as المبلغ,
                r.Notes as ملاحظات,
                -- قيم افتراضية للأعمدة المفقودة
                CASE WHEN r.TheMethod = 1 THEN 'نقدي'
                     WHEN r.TheMethod = 2 THEN 'شيك'
                     WHEN r.TheMethod = 3 THEN 'تحويل بنكي'
                     ELSE 'نقدي' END as طريقة_القبض,
                CASE WHEN r.CurrencyID = 1 THEN 'دينار'
                     WHEN r.CurrencyID = 2 THEN 'دولار'
                     WHEN r.CurrencyID = 3 THEN 'يورو'
                     ELSE 'دينار' END as العملة,
                'حساب عام' as اسم_الحساب,
                'المستخدم الحالي' as المستخدم,
                0 as معتمد
                FROM tblReceipt r
                ORDER BY r.TheDate DESC, r.ID DESC";

            using (var command = new SqlCommand(tableQuery, connection))
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    receiptList.Add(new ReceiptView
                    {
                        ID = reader.IsDBNull(reader.GetOrdinal("ID")) ? 0 : reader.GetInt64(reader.GetOrdinal("ID")),
                        الرقم = reader.IsDBNull(reader.GetOrdinal("الرقم")) ? (long?)null : reader.GetInt64(reader.GetOrdinal("الرقم")),
                        التاريخ = reader.IsDBNull(reader.GetOrdinal("التاريخ")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("التاريخ")),
                        طريقة_القبض = reader.IsDBNull(reader.GetOrdinal("طريقة_القبض")) ? "نقدي" : reader.GetString(reader.GetOrdinal("طريقة_القبض")),
                        المبلغ = reader.IsDBNull(reader.GetOrdinal("المبلغ")) ? (decimal?)null : reader.GetDecimal(reader.GetOrdinal("المبلغ")),
                        العملة = reader.IsDBNull(reader.GetOrdinal("العملة")) ? "دينار" : reader.GetString(reader.GetOrdinal("العملة")),
                        اسم_الحساب = reader.IsDBNull(reader.GetOrdinal("اسم_الحساب")) ? "" : reader.GetString(reader.GetOrdinal("اسم_الحساب")),
                        المستخدم = reader.IsDBNull(reader.GetOrdinal("المستخدم")) ? "مستخدم غير محدد" : reader.GetString(reader.GetOrdinal("المستخدم")),
                        معتمد = reader.IsDBNull(reader.GetOrdinal("معتمد")) ? (bool?)null : Convert.ToBoolean(reader["معتمد"])
                    });
                }
            }

            return receiptList;
        }

        // إضافة سند قبض جديد
        public bool AddReceipt(Receipt receipt)
        {
            try
            {
                using (var connection = _dbService.GetConnection(DatabaseType.AppDatabase))
                {
                    connection.Open();
                    string query = @"INSERT INTO tblReceipt 
                        (TheNumber, TheDate, TheMethod, Amount, CurrencyID, AccountID, Notes, UserID, EnterTime) 
                        VALUES (@TheNumber, @TheDate, @TheMethod, @Amount, @CurrencyID, @AccountID, @Notes, @UserID, GETDATE())";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@TheNumber", receipt.TheNumber);
                        command.Parameters.AddWithValue("@TheDate", receipt.TheDate ?? DateTime.Now);
                        command.Parameters.AddWithValue("@TheMethod", receipt.TheMethod ?? 1); // نقدي كقيمة افتراضية
                        command.Parameters.AddWithValue("@Amount", receipt.Amount ?? 0);
                        command.Parameters.AddWithValue("@CurrencyID", receipt.CurrencyID ?? 1); // دينار كقيمة افتراضية
                        command.Parameters.AddWithValue("@AccountID", receipt.AccountID ?? 1); // حساب افتراضي
                        command.Parameters.AddWithValue("@Notes", receipt.Notes ?? "");
                        command.Parameters.AddWithValue("@UserID", receipt.UserID ?? 1); // مستخدم افتراضي

                        int result = command.ExecuteNonQuery();
                        return result > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"خطأ في إضافة سند القبض: {ex.Message}");
            }
        }

        // تحديث سند قبض
        public bool UpdateReceipt(Receipt receipt)
        {
            try
            {
                using (var connection = _dbService.GetConnection(DatabaseType.AppDatabase))
                {
                    connection.Open();
                    string query = @"UPDATE tblReceipt 
                        SET TheDate = @TheDate, 
                            TheMethod = @TheMethod, 
                            Amount = @Amount, 
                            CurrencyID = @CurrencyID, 
                            AccountID = @AccountID, 
                            Notes = @Notes
                        WHERE ID = @ID";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@ID", receipt.ID);
                        command.Parameters.AddWithValue("@TheDate", receipt.TheDate ?? DateTime.Now);
                        command.Parameters.AddWithValue("@TheMethod", receipt.TheMethod ?? 1);
                        command.Parameters.AddWithValue("@Amount", receipt.Amount ?? 0);
                        command.Parameters.AddWithValue("@CurrencyID", receipt.CurrencyID ?? 1);
                        command.Parameters.AddWithValue("@AccountID", receipt.AccountID ?? 1);
                        command.Parameters.AddWithValue("@Notes", receipt.Notes ?? "");

                        int result = command.ExecuteNonQuery();
                        return result > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"خطأ في تحديث سند القبض: {ex.Message}");
            }
        }

        // حذف سند قبض
        public bool DeleteReceipt(long id)
        {
            try
            {
                using (var connection = _dbService.GetConnection(DatabaseType.AppDatabase))
                {
                    connection.Open();
                    string query = "DELETE FROM tblReceipt WHERE ID = @ID";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@ID", id);
                        int result = command.ExecuteNonQuery();
                        return result > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"خطأ في حذف سند القبض: {ex.Message}");
            }
        }
        

        // جلب سند قبض واحد
        public ReceiptView GetReceiptById(long id)
        {
            var receiptList = GetAllReceipts();
            return receiptList.Find(r => r.ID == id);
        }

        // البحث في سندات القبض
        public List<ReceiptView> SearchReceipts(string searchTerm)
        {
            var allReceipts = GetAllReceipts();
            
            if (string.IsNullOrWhiteSpace(searchTerm))
                return allReceipts;

            return allReceipts.FindAll(r =>
                (r.الرقم?.ToString().Contains(searchTerm) == true) ||
                (r.المبلغ?.ToString().Contains(searchTerm) == true) ||
                (r.ملاحظات?.Contains(searchTerm) == true) ||
                (r.المستخدم?.Contains(searchTerm) == true));
        }

        // جلب سندات القبض بتاريخ معين
        public List<ReceiptView> GetReceiptsByDate(DateTime date)
        {
            var allReceipts = GetAllReceipts();
            return allReceipts.FindAll(r => 
                r.التاريخ?.Date == date.Date);
        }
    }
}