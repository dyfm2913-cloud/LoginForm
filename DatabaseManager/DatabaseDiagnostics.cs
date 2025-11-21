using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using Common.Models;
using Common.Enums;

namespace DatabaseManager
{
    public class DatabaseDiagnostics
    {
        private readonly DatabaseService _dbService;

        public DatabaseDiagnostics(DatabaseService dbService)
        {
            _dbService = dbService;
        }

        public string RunDiagnostics()
        {
            var result = new System.Text.StringBuilder();
            result.AppendLine("=== تقرير تشخيص قاعدة البيانات ===");
            result.AppendLine($"التوقيت: {DateTime.Now:yyyy/MM/dd HH:mm:ss}");
            result.AppendLine();

            // فحص الاتصال بقاعدة البيانات النظامية
            result.AppendLine("1. فحص قاعدة البيانات النظامية:");
            TestSystemDatabase(result);

            // فحص الاتصال بقاعدة البيانات التطبيقية
            result.AppendLine("2. فحص قاعدة البيانات التطبيقية:");
            TestAppDatabase(result);

            // فحص جدول سندات الصرف
            result.AppendLine("3. فحص جدول سندات الصرف:");
            TestSpendingTable(result);

            // فحص البيانات في الجدول
            result.AppendLine("4. فحص البيانات في جدول سندات الصرف:");
            CheckSpendingData(result);

            return result.ToString();
        }

        private void TestSystemDatabase(System.Text.StringBuilder result)
        {
            try
            {
                using (var connection = _dbService.GetConnection(DatabaseType.SystemDatabase))
                {
                    connection.Open();
                    result.AppendLine("   ✓ الاتصال ناجح");
                    
                    // فحص وجود جدول المستخدمين
                    string checkUsersTable = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Users'";
                    using (var command = new SqlCommand(checkUsersTable, connection))
                    {
                        int tableExists = (int)command.ExecuteScalar();
                        if (tableExists > 0)
                        {
                            result.AppendLine("   ✓ جدول Users موجود");
                            
                            // فحص عدد المستخدمين
                            string countUsers = "SELECT COUNT(*) FROM Users";
                            using (var countCommand = new SqlCommand(countUsers, connection))
                            {
                                int userCount = (int)countCommand.ExecuteScalar();
                                result.AppendLine($"   ℹ عدد المستخدمين: {userCount}");
                            }
                        }
                        else
                        {
                            result.AppendLine("   ✗ جدول Users غير موجود");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                result.AppendLine($"   ✗ فشل الاتصال: {ex.Message}");
            }
        }

        private void TestAppDatabase(System.Text.StringBuilder result)
        {
            try
            {
                using (var connection = _dbService.GetConnection(DatabaseType.AppDatabase))
                {
                    connection.Open();
                    result.AppendLine("   ✓ الاتصال ناجح");
                    
                    // فحص وجود جدول سندات الصرف
                    string checkTable = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'tblSpending'";
                    using (var command = new SqlCommand(checkTable, connection))
                    {
                        int tableExists = (int)command.ExecuteScalar();
                        if (tableExists > 0)
                        {
                            result.AppendLine("   ✓ جدول tblSpending موجود");
                            
                            // فحص وجود الفيو
                            string checkView = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.VIEWS WHERE TABLE_NAME = 'vw_SpendingView'";
                            using (var viewCommand = new SqlCommand(checkView, connection))
                            {
                                int viewExists = (int)viewCommand.ExecuteScalar();
                                if (viewExists > 0)
                                {
                                    result.AppendLine("   ✓ فاريو vw_SpendingView موجود");
                                }
                                else
                                {
                                    result.AppendLine("   ⚠ فاريو vw_SpendingView غير موجود - سيتم استخدام الجدول الأساسي");
                                }
                            }
                        }
                        else
                        {
                            result.AppendLine("   ✗ جدول tblSpending غير موجود");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                result.AppendLine($"   ✗ فشل الاتصال: {ex.Message}");
            }
        }

        private void TestSpendingTable(System.Text.StringBuilder result)
        {
            try
            {
                using (var connection = _dbService.GetConnection(DatabaseType.AppDatabase))
                {
                    connection.Open();
                    
                    // فحص بنية الجدول
                    string schemaQuery = @"
                        SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE 
                        FROM INFORMATION_SCHEMA.COLUMNS 
                        WHERE TABLE_NAME = 'tblSpending' 
                        ORDER BY ORDINAL_POSITION";
                    
                    using (var command = new SqlCommand(schemaQuery, connection))
                    using (var reader = command.ExecuteReader())
                    {
                        result.AppendLine("   بنية جدول tblSpending:");
                        while (reader.Read())
                        {
                            string columnName = reader["COLUMN_NAME"].ToString();
                            string dataType = reader["DATA_TYPE"].ToString();
                            string isNullable = reader["IS_NULLABLE"].ToString();
                            result.AppendLine($"     - {columnName} ({dataType}) - {(isNullable == "YES" ? " nullable" : " not null")}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                result.AppendLine($"   ✗ خطأ في فحص البنية: {ex.Message}");
            }
        }

        private void CheckSpendingData(System.Text.StringBuilder result)
        {
            try
            {
                using (var connection = _dbService.GetConnection(DatabaseType.AppDatabase))
                {
                    connection.Open();

                    // فحص عدد السجلات
                    string countQuery = "SELECT COUNT(*) FROM tblSpending";
                    int recordCount = 0;
                    using (var command = new SqlCommand(countQuery, connection))
                    {
                        recordCount = (int)command.ExecuteScalar();
                        result.AppendLine($"   ℹ عدد السجلات في الجدول: {recordCount}");
                    }

                    if (recordCount > 0)
                    {
                        // عرض عينة من البيانات
                        string sampleQuery = "SELECT TOP 3 ID, TheNumber, TheDate, Amount, Notes FROM tblSpending ORDER BY ID DESC";
                        using (var command = new SqlCommand(sampleQuery, connection))
                        using (var reader = command.ExecuteReader())
                        {
                            result.AppendLine("   عينة من البيانات:");
                            int rowNum = 1;
                            while (reader.Read())
                            {
                                result.AppendLine($"     السجل {rowNum}: ID={reader["ID"]}, Number={reader["TheNumber"]}, Date={reader["TheDate"]}, Amount={reader["Amount"]}");
                                rowNum++;
                            }
                        }
                    }
                    else
                    {
                        result.AppendLine("   ⚠ الجدول فارغ - لا توجد سندات صرف");
                    }
                }
            }
            catch (Exception ex)
            {
                result.AppendLine($"   ✗ خطأ في فحص البيانات: {ex.Message}");
            }
        }
    }
}