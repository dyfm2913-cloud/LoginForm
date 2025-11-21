using System;
using System.Data.SqlClient;
using System.Collections.Generic;
using Common.Models;
using Common.Enums;

namespace DatabaseManager
{
    public class SimpleEntriesService
    {
        private readonly DatabaseService _dbService;

        public SimpleEntriesService(DatabaseService dbService)
        {
            _dbService = dbService;
        }

        // جلب جميع القيود البسيطة مع بيانات كاملة
        public List<SimpleEntryView> GetAllSimpleEntries()
        {
            var entryList = new List<SimpleEntryView>();

            using (var connection = _dbService.GetConnection(DatabaseType.AppDatabase))
            {
                connection.Open();

                // محاولة الوصول للفيو أولاً
                string viewQuery = @"SELECT 
                    vw.ID,
                    vw.الرقم,
                    vw.التاريخ,
                    vw.الوصف,
                    vw.الحساب_المدين,
                    vw.الحساب_الدائن,
                    vw.المبلغ,
                    vw.العملة,
                    vw.المستخدم
                    FROM vw_SimpleEntryView vw
                    ORDER BY vw.التاريخ DESC, vw.ID DESC";

                try
                {
                    using (var command = new SqlCommand(viewQuery, connection))
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            entryList.Add(new SimpleEntryView
                            {
                                ID = reader.IsDBNull(reader.GetOrdinal("ID")) ? 0 : reader.GetInt64(reader.GetOrdinal("ID")),
                                الرقم = reader.IsDBNull(reader.GetOrdinal("الرقم")) ? (long?)null : reader.GetInt64(reader.GetOrdinal("الرقم")),
                                التاريخ = reader.IsDBNull(reader.GetOrdinal("التاريخ")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("التاريخ")),
                                الوصف = reader.IsDBNull(reader.GetOrdinal("الوصف")) ? "" : reader.GetString(reader.GetOrdinal("الوصف")),
                                الحساب_المدين = reader.IsDBNull(reader.GetOrdinal("الحساب_المدين")) ? "" : reader.GetString(reader.GetOrdinal("الحساب_المدين")),
                                الحساب_الدائن = reader.IsDBNull(reader.GetOrdinal("الحساب_الدائن")) ? "" : reader.GetString(reader.GetOrdinal("الحساب_الدائن")),
                                المبلغ = reader.IsDBNull(reader.GetOrdinal("المبلغ")) ? (decimal?)null : reader.GetDecimal(reader.GetOrdinal("المبلغ")),
                                العملة = reader.IsDBNull(reader.GetOrdinal("العملة")) ? "دينار" : reader.GetString(reader.GetOrdinal("العملة")),
                                المستخدم = reader.IsDBNull(reader.GetOrdinal("المستخدم")) ? "مستخدم غير محدد" : reader.GetString(reader.GetOrdinal("المستخدم"))
                            });
                        }
                    }
                    return entryList; // إذا نجح الفيو، Return البيانات
                }
                catch
                {
                    // إذا فشل الفيو، استخدم الاستعلام البديل من الجدول الأساسي
                    return GetEntriesFromBasicTable(connection);
                }
            }
        }

        // استعلام بديل من الجدول الأساسي إذا لم يوجد الفيو
        private List<SimpleEntryView> GetEntriesFromBasicTable(SqlConnection connection)
        {
            var entryList = new List<SimpleEntryView>();

            string tableQuery = @"SELECT 
                e.ID,
                e.TheNumber as الرقم,
                e.TheDate as التاريخ,
                e.Description as الوصف,
                e.AccountFrom as الحساب_المدين,
                e.AccountTo as الحساب_الدائن,
                e.Amount as المبلغ,
                e.Currency as العملة,
                e.Notes as ملاحظات,
                'المستخدم الحالي' as المستخدم
                FROM tblSimpleEntry e
                ORDER BY e.TheDate DESC, e.ID DESC";

            using (var command = new SqlCommand(tableQuery, connection))
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    entryList.Add(new SimpleEntryView
                    {
                        ID = reader.IsDBNull(reader.GetOrdinal("ID")) ? 0 : reader.GetInt64(reader.GetOrdinal("ID")),
                        الرقم = reader.IsDBNull(reader.GetOrdinal("الرقم")) ? (long?)null : reader.GetInt64(reader.GetOrdinal("الرقم")),
                        التاريخ = reader.IsDBNull(reader.GetOrdinal("التاريخ")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("التاريخ")),
                        الوصف = reader.IsDBNull(reader.GetOrdinal("الوصف")) ? "" : reader.GetString(reader.GetOrdinal("الوصف")),
                        الحساب_المدين = reader.IsDBNull(reader.GetOrdinal("الحساب_المدين")) ? "" : reader.GetString(reader.GetOrdinal("الحساب_المدين")),
                        الحساب_الدائن = reader.IsDBNull(reader.GetOrdinal("الحساب_الدائن")) ? "" : reader.GetString(reader.GetOrdinal("الحساب_الدائن")),
                        المبلغ = reader.IsDBNull(reader.GetOrdinal("المبلغ")) ? (decimal?)null : reader.GetDecimal(reader.GetOrdinal("المبلغ")),
                        العملة = reader.IsDBNull(reader.GetOrdinal("العملة")) ? "دينار" : reader.GetString(reader.GetOrdinal("العملة")),
                        المستخدم = reader.IsDBNull(reader.GetOrdinal("المستخدم")) ? "مستخدم غير محدد" : reader.GetString(reader.GetOrdinal("المستخدم"))
                    });
                }
            }

            return entryList;
        }

        // إضافة قيد بسيط جديد
        public bool AddSimpleEntry(SimpleEntry entry)
        {
            try
            {
                using (var connection = _dbService.GetConnection(DatabaseType.AppDatabase))
                {
                    connection.Open();
                    string query = @"INSERT INTO tblSimpleEntry 
                        (TheNumber, TheDate, Description, AccountFrom, AccountTo, Amount, Currency, Notes, UserID, EnterTime) 
                        VALUES (@TheNumber, @TheDate, @Description, @AccountFrom, @AccountTo, @Amount, @Currency, @Notes, @UserID, GETDATE())";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@TheNumber", entry.TheNumber);
                        command.Parameters.AddWithValue("@TheDate", entry.TheDate ?? DateTime.Now);
                        command.Parameters.AddWithValue("@Description", entry.Description ?? "");
                        command.Parameters.AddWithValue("@AccountFrom", entry.AccountFrom ?? "");
                        command.Parameters.AddWithValue("@AccountTo", entry.AccountTo ?? "");
                        command.Parameters.AddWithValue("@Amount", entry.Amount ?? 0);
                        command.Parameters.AddWithValue("@Currency", entry.Currency ?? "دينار");
                        command.Parameters.AddWithValue("@Notes", entry.Notes ?? "");
                        command.Parameters.AddWithValue("@UserID", entry.UserID ?? 1);

                        int result = command.ExecuteNonQuery();
                        return result > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"خطأ في إضافة القيد البسيط: {ex.Message}");
            }
        }

        // تحديث قيد بسيط
        public bool UpdateSimpleEntry(SimpleEntry entry)
        {
            try
            {
                using (var connection = _dbService.GetConnection(DatabaseType.AppDatabase))
                {
                    connection.Open();
                    string query = @"UPDATE tblSimpleEntry 
                        SET TheDate = @TheDate, 
                            Description = @Description, 
                            AccountFrom = @AccountFrom, 
                            AccountTo = @AccountTo, 
                            Amount = @Amount, 
                            Currency = @Currency,
                            Notes = @Notes
                        WHERE ID = @ID";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@ID", entry.ID);
                        command.Parameters.AddWithValue("@TheDate", entry.TheDate ?? DateTime.Now);
                        command.Parameters.AddWithValue("@Description", entry.Description ?? "");
                        command.Parameters.AddWithValue("@AccountFrom", entry.AccountFrom ?? "");
                        command.Parameters.AddWithValue("@AccountTo", entry.AccountTo ?? "");
                        command.Parameters.AddWithValue("@Amount", entry.Amount ?? 0);
                        command.Parameters.AddWithValue("@Currency", entry.Currency ?? "دينار");
                        command.Parameters.AddWithValue("@Notes", entry.Notes ?? "");

                        int result = command.ExecuteNonQuery();
                        return result > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"خطأ في تحديث القيد البسيط: {ex.Message}");
            }
        }

        // حذف قيد بسيط
        public bool DeleteSimpleEntry(long id)
        {
            try
            {
                using (var connection = _dbService.GetConnection(DatabaseType.AppDatabase))
                {
                    connection.Open();
                    string query = "DELETE FROM tblSimpleEntry WHERE ID = @ID";

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
                throw new Exception($"خطأ في حذف القيد البسيط: {ex.Message}");
            }
        }
        

        // جلب قيد بسيط واحد
        public SimpleEntryView GetSimpleEntryById(long id)
        {
            var entryList = GetAllSimpleEntries();
            return entryList.Find(e => e.ID == id);
        }

        // البحث في القيود البسيطة
        public List<SimpleEntryView> SearchSimpleEntries(string searchTerm)
        {
            var allEntries = GetAllSimpleEntries();
            
            if (string.IsNullOrWhiteSpace(searchTerm))
                return allEntries;

            return allEntries.FindAll(e =>
                (e.الرقم?.ToString().Contains(searchTerm) == true) ||
                (e.الوصف?.Contains(searchTerm) == true) ||
                (e.الحساب_المدين?.Contains(searchTerm) == true) ||
                (e.الحساب_الدائن?.Contains(searchTerm) == true) ||
                (e.المستخدم?.Contains(searchTerm) == true));
        }

        // جلب القيود البسيطة بتاريخ معين
        public List<SimpleEntryView> GetSimpleEntriesByDate(DateTime date)
        {
            var allEntries = GetAllSimpleEntries();
            return allEntries.FindAll(e => 
                e.التاريخ?.Date == date.Date);
        }
    }
}