using System;
using System.Data.SqlClient;
using System.Collections.Generic;
using Common.Models;
using Common.Enums;
using Security;

namespace DatabaseManager
{
    public class UserService
    {
        private readonly DatabaseService _dbService;

        public UserService(DatabaseService dbService)
        {
            _dbService = dbService;
        }

        // دالة المصادقة الحالية
        public User AuthenticateUser(string username, string password)
        {
            string hashedPassword = PasswordHasher.HashPassword(password);

            using (var connection = _dbService.GetConnection(DatabaseType.SystemDatabase))
            {
                connection.Open();
                string query = "SELECT UserID, Username, Password, FullName, IsActive, CreatedDate FROM Users WHERE Username = @Username AND IsActive = 1";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Username", username);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string storedPassword = reader["Password"].ToString();

                            if (PasswordHasher.VerifyPassword(password, storedPassword))
                            {
                                return new User
                                {
                                    UserID = (int)reader["UserID"],
                                    Username = reader["Username"].ToString(),
                                    Password = reader["Password"].ToString(),
                                    FullName = reader["FullName"].ToString(),
                                    IsActive = (bool)reader["IsActive"],
                                    CreatedDate = (DateTime)reader["CreatedDate"]
                                };
                            }
                        }
                    }
                }
            }
            return null;
        }

        // === دوال CRUD الجديدة ===

        // 1. جلب جميع المستخدمين
        public List<User> GetAllUsers()
        {
            var users = new List<User>();

            using (var connection = _dbService.GetConnection(DatabaseType.SystemDatabase))
            {
                connection.Open();
                string query = @"SELECT UserID, Username, Password, FullName, IsActive, CreatedDate 
                               FROM Users 
                               ORDER BY UserID";

                using (var command = new SqlCommand(query, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        users.Add(new User
                        {
                            UserID = (int)reader["UserID"],
                            Username = reader["Username"].ToString(),
                            Password = reader["Password"].ToString(),
                            FullName = reader["FullName"].ToString(),
                            IsActive = (bool)reader["IsActive"],
                            CreatedDate = (DateTime)reader["CreatedDate"]
                        });
                    }
                }
            }
            return users;
        }

        // 2. إضافة مستخدم جديد
        public bool AddUser(User user)
        {
            using (var connection = _dbService.GetConnection(DatabaseType.SystemDatabase))
            {
                connection.Open();
                string query = @"INSERT INTO Users (Username, Password, FullName, IsActive, CreatedDate)
                               VALUES (@Username, @Password, @FullName, @IsActive, GETDATE())";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Username", user.Username);
                    command.Parameters.AddWithValue("@Password", PasswordHasher.HashPassword(user.Password));
                    command.Parameters.AddWithValue("@FullName", user.FullName);
                    command.Parameters.AddWithValue("@IsActive", user.IsActive);

                    int result = command.ExecuteNonQuery();
                    return result > 0;
                }
            }
        }

        // 3. تحديث بيانات مستخدم
        public bool UpdateUser(User user)
        {
            using (var connection = _dbService.GetConnection(DatabaseType.SystemDatabase))
            {
                connection.Open();
                string query = @"UPDATE Users 
                               SET Username = @Username, 
                                   FullName = @FullName, 
                                   IsActive = @IsActive
                               WHERE UserID = @UserID";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@UserID", user.UserID);
                    command.Parameters.AddWithValue("@Username", user.Username);
                    command.Parameters.AddWithValue("@FullName", user.FullName);
                    command.Parameters.AddWithValue("@IsActive", user.IsActive);

                    int result = command.ExecuteNonQuery();
                    return result > 0;
                }
            }
        }

        // 4. حذف مستخدم
        public bool DeleteUser(int userID)
        {
            using (var connection = _dbService.GetConnection(DatabaseType.SystemDatabase))
            {
                connection.Open();
                string query = "DELETE FROM Users WHERE UserID = @UserID";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@UserID", userID);

                    int result = command.ExecuteNonQuery();
                    return result > 0;
                }
            }
        }

        // 5. البحث عن مستخدم بالاسم
        public User GetUserByUsername(string username)
        {
            using (var connection = _dbService.GetConnection(DatabaseType.SystemDatabase))
            {
                connection.Open();
                string query = @"SELECT UserID, Username, Password, FullName, IsActive, CreatedDate 
                               FROM Users 
                               WHERE Username = @Username";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Username", username);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new User
                            {
                                UserID = (int)reader["UserID"],
                                Username = reader["Username"].ToString(),
                                Password = reader["Password"].ToString(),
                                FullName = reader["FullName"].ToString(),
                                IsActive = (bool)reader["IsActive"],
                                CreatedDate = (DateTime)reader["CreatedDate"]
                            };
                        }
                    }
                }
            }
            return null;
        }

        // 6. تغيير كلمة مرور مستخدم
        public bool ChangePassword(int userID, string newPassword)
        {
            using (var connection = _dbService.GetConnection(DatabaseType.SystemDatabase))
            {
                connection.Open();
                string query = @"UPDATE Users 
                               SET Password = @Password 
                               WHERE UserID = @UserID";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@UserID", userID);
                    command.Parameters.AddWithValue("@Password", PasswordHasher.HashPassword(newPassword));

                    int result = command.ExecuteNonQuery();
                    return result > 0;
                }
            }
        }


        /// <summary>
        /// /////
        /// </summary>
        /// <returns></returns>
        // 7. إنشاء مستخدم افتراضي إذا لم يوجد مستخدمين
        public bool CreateDefaultUser()
        {
            using (var connection = _dbService.GetConnection(DatabaseType.SystemDatabase))
            {
                connection.Open();

                // التحقق إذا كان هناك مستخدمين بالفعل
                string checkQuery = "SELECT COUNT(*) FROM Users";
                using (var checkCommand = new SqlCommand(checkQuery, connection))
                {
                    int userCount = (int)checkCommand.ExecuteScalar();
                    if (userCount > 0)
                    {
                        return false; // يوجد مستخدمين بالفعل
                    }
                }

                // إنشاء المستخدم الافتراضي
                string insertQuery = @"INSERT INTO Users (Username, Password, FullName, IsActive, CreatedDate)
                             VALUES (@Username, @Password, @FullName, @IsActive, GETDATE())";

                using (var command = new SqlCommand(insertQuery, connection))
                {
                    command.Parameters.AddWithValue("@Username", "1");
                    command.Parameters.AddWithValue("@Password", PasswordHasher.HashPassword("1"));
                    command.Parameters.AddWithValue("@FullName", "مستخدم افتراضي");
                    command.Parameters.AddWithValue("@IsActive", true);

                    int result = command.ExecuteNonQuery();
                    return result > 0;
                }
            }
        }
    }
}