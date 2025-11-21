using System;
using System.Data.SqlClient;
using System.Collections.Generic;
using Common.Models;
using Common.Enums;

namespace DatabaseManager
{
    public class SpendingService
    {
        private readonly DatabaseService _dbService;

        public SpendingService(DatabaseService dbService)
        {
            _dbService = dbService;
        }

        // جلب جميع سندات الصرف مع بيانات كاملة من الفيو
        public List<SpendingView> GetAllSpending()
        {
            var spendingList = new List<SpendingView>();

            using (var connection = _dbService.GetConnection(DatabaseType.AppDatabase))
            {
                connection.Open();

                // محاولة الوصول للفيو أولاً
                string viewQuery = @"SELECT
                    vw.ID,
                    vw.TheNumber,
                    vw.الرقم,
                    vw.التاريخ,
                    vw.طريقة_الصرف,
                    vw.المبلغ,
                    vw.العملة,
                    vw.AccountID as الصندوق,
                    vw.ExchangeAmount as مبلغ_الحساب,
                    vw.ExchangeCurrency as عملة_الحساب,
                    vw.ExchangeAccountID as اسم_الحساب,
                    vw.ملاحظات,
                    vw.BranchID as الفرع,
                    vw.UserID,
                    vw.المستخدم,
                    vw.معتمد
                    FROM vw_SpendingView vw
                    ORDER BY vw.التاريخ DESC, vw.ID DESC";

                try
                {
                    using (var command = new SqlCommand(viewQuery, connection))
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            spendingList.Add(new SpendingView
                            {
                                ID = reader.IsDBNull(reader.GetOrdinal("ID")) ? 0 : reader.GetInt64(reader.GetOrdinal("ID")),
                                الرقم = reader.IsDBNull(reader.GetOrdinal("الرقم")) ? (long?)null : reader.GetInt64(reader.GetOrdinal("الرقم")),
                                التاريخ = reader.IsDBNull(reader.GetOrdinal("التاريخ")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("التاريخ")),
                                طريقة_الصرف = reader.IsDBNull(reader.GetOrdinal("طريقة_الصرف")) ? "نقدي" : reader.GetString(reader.GetOrdinal("طريقة_الصرف")),
                                المبلغ = reader.IsDBNull(reader.GetOrdinal("المبلغ")) ? (decimal?)null : reader.GetDecimal(reader.GetOrdinal("المبلغ")),
                                العملة = reader.IsDBNull(reader.GetOrdinal("العملة")) ? "دينار" : reader.GetString(reader.GetOrdinal("العملة")),
                                // تم تغيير اسم الحقل إلى ExchangeAccountID
                                المستخدم = reader.IsDBNull(reader.GetOrdinal("المستخدم")) ? "مستخدم غير محدد" : reader.GetString(reader.GetOrdinal("المستخدم")),
                                معتمد = reader.IsDBNull(reader.GetOrdinal("معتمد")) ? (bool?)null : reader.GetBoolean(reader.GetOrdinal("معتمد"))
                            });
                        }
                    }
                    return spendingList; // إذا نجح الفيو،_return البيانات
                }
                catch
                {
                    // إذا فشل الفيو، استخدم الاستعلام البديل من الجدول الأساسي
                    return GetSpendingFromBasicTable(connection);
                }
            }
        }

        // استعلام بديل من الجدول الأساسي إذا لم يوجد الفيو
        private List<SpendingView> GetSpendingFromBasicTable(SqlConnection connection)
        {
            var spendingList = new List<SpendingView>();

            string tableQuery = @"SELECT 
                s.ID,
                s.TheNumber as الرقم,
                s.TheDate as التاريخ,
                s.Amount as المبلغ,
                s.Notes as ملاحظات,
                -- قيم افتراضية للأعمدة المفقودة
                CASE WHEN s.TheMethod = 1 THEN 'نقدي'
                     WHEN s.TheMethod = 2 THEN 'شيك'
                     WHEN s.TheMethod = 3 THEN 'تحويل بنكي'
                     ELSE 'نقدي' END as طريقة_الصرف,
                CASE WHEN s.CurrencyID = 1 THEN 'دينار'
                     WHEN s.CurrencyID = 2 THEN 'دولار'
                     WHEN s.CurrencyID = 3 THEN 'يورو'
                     ELSE 'دينار' END as العملة,
                'حساب عام' as اسم_الحساب,
                'المستخدم الحالي' as المستخدم,
                0 as معتمد
                FROM tblSpending s
                ORDER BY s.TheDate DESC, s.ID DESC";

            using (var command = new SqlCommand(tableQuery, connection))
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    spendingList.Add(new SpendingView
                    {
                        ID = reader.IsDBNull(reader.GetOrdinal("ID")) ? 0 : reader.GetInt64(reader.GetOrdinal("ID")),
                        الرقم = reader.IsDBNull(reader.GetOrdinal("الرقم")) ? (long?)null : reader.GetInt64(reader.GetOrdinal("الرقم")),
                        التاريخ = reader.IsDBNull(reader.GetOrdinal("التاريخ")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("التاريخ")),
                        طريقة_الصرف = reader.IsDBNull(reader.GetOrdinal("طريقة_الصرف")) ? "نقدي" : reader.GetString(reader.GetOrdinal("طريقة_الصرف")),
                        المبلغ = reader.IsDBNull(reader.GetOrdinal("المبلغ")) ? (decimal?)null : reader.GetDecimal(reader.GetOrdinal("المبلغ")),
                        العملة = reader.IsDBNull(reader.GetOrdinal("العملة")) ? "دينار" : reader.GetString(reader.GetOrdinal("العملة")),
                        // تم إزالة اسم_الحساب واستبداله بحقول أخرى
                        المستخدم = reader.IsDBNull(reader.GetOrdinal("المستخدم")) ? "مستخدم غير محدد" : reader.GetString(reader.GetOrdinal("المستخدم")),
                        معتمد = reader.IsDBNull(reader.GetOrdinal("معتمد")) ? (bool?)null : Convert.ToBoolean(reader["معتمد"])
                    });
                }
            }

            return spendingList;
        }

        // إضافة سند صرف جديد
        public bool AddSpending(Spending spending)
        {
            try
            {
                using (var connection = _dbService.GetConnection(DatabaseType.AppDatabase))
                {
                    connection.Open();
                    string query = @"INSERT INTO tblSpending 
                        (TheNumber, TheDate, TheMethod, Amount, CurrencyID, AccountID, Notes, UserID, EnterTime) 
                        VALUES (@TheNumber, @TheDate, @TheMethod, @Amount, @CurrencyID, @AccountID, @Notes, @UserID, GETDATE())";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@TheNumber", spending.TheNumber);
                        command.Parameters.AddWithValue("@TheDate", spending.TheDate ?? DateTime.Now);
                        command.Parameters.AddWithValue("@TheMethod", spending.TheMethod ?? 1); // نقدي كقيمة افتراضية
                        command.Parameters.AddWithValue("@Amount", spending.Amount ?? 0);
                        command.Parameters.AddWithValue("@CurrencyID", spending.CurrencyID ?? 1); // دينار كقيمة افتراضية
                        command.Parameters.AddWithValue("@AccountID", spending.AccountID ?? 1); // حساب افتراضي
                        command.Parameters.AddWithValue("@Notes", spending.Notes ?? "");
                        command.Parameters.AddWithValue("@UserID", spending.UserID ?? 1); // مستخدم افتراضي

                        int result = command.ExecuteNonQuery();
                        return result > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"خطأ في إضافة سند الصرف: {ex.Message}");
            }
        }

        // تحديث سند صرف
        public bool UpdateSpending(Spending spending)
        {
            try
            {
                using (var connection = _dbService.GetConnection(DatabaseType.AppDatabase))
                {
                    connection.Open();
                    string query = @"UPDATE tblSpending 
                        SET TheDate = @TheDate, 
                            TheMethod = @TheMethod, 
                            Amount = @Amount, 
                            CurrencyID = @CurrencyID, 
                            AccountID = @AccountID, 
                            Notes = @Notes
                        WHERE ID = @ID";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@ID", spending.ID);
                        command.Parameters.AddWithValue("@TheDate", spending.TheDate ?? DateTime.Now);
                        command.Parameters.AddWithValue("@TheMethod", spending.TheMethod ?? 1);
                        command.Parameters.AddWithValue("@Amount", spending.Amount ?? 0);
                        command.Parameters.AddWithValue("@CurrencyID", spending.CurrencyID ?? 1);
                        command.Parameters.AddWithValue("@AccountID", spending.AccountID ?? 1);
                        command.Parameters.AddWithValue("@Notes", spending.Notes ?? "");

                        int result = command.ExecuteNonQuery();
                        return result > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"خطأ في تحديث سند الصرف: {ex.Message}");
            }
        }

        // حذف سند صرف
        public bool DeleteSpending(long id)
        {
            try
            {
                using (var connection = _dbService.GetConnection(DatabaseType.AppDatabase))
                {
                    connection.Open();
                    string query = "DELETE FROM tblSpending WHERE ID = @ID";

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
                throw new Exception($"خطأ في حذف سند الصرف: {ex.Message}");
            }
        }

        // جلب سند صرف واحد
        public SpendingView GetSpendingById(long id)
        {
            var spendingList = GetAllSpending();
            return spendingList.Find(s => s.ID == id);
        }

        // البحث في سندات الصرف
        public List<SpendingView> SearchSpending(string searchTerm)
        {
            var allSpending = GetAllSpending();
            
            if (string.IsNullOrWhiteSpace(searchTerm))
                return allSpending;

            return allSpending.FindAll(s =>
                (s.الرقم?.ToString().Contains(searchTerm) == true) ||
                (s.المبلغ?.ToString().Contains(searchTerm) == true) ||
                (s.ملاحظات?.Contains(searchTerm) == true) ||
                (s.المستخدم?.Contains(searchTerm) == true));
        }

        // جلب سندات الصرف بتاريخ معين
        public List<SpendingView> GetSpendingByDate(DateTime date)
        {
            var allSpending = GetAllSpending();
            return allSpending.FindAll(s => 
                s.التاريخ?.Date == date.Date);
        }
    }
}