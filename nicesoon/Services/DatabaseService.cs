using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using nicesoon.Models;
namespace nicesoon.Services
{
    public class DatabaseService
    {
        private readonly SQLiteAsyncConnection _database;
        public string DatabasePath { get; }

        public DatabaseService()
        {
            var projectPath = @"C://practNicesoon//nicesoon//nicesoon//nicesoon.db";
            DatabasePath = Path.Combine(projectPath, "nicesoon.db");

            var flags = SQLiteOpenFlags.ReadWrite |
                        SQLiteOpenFlags.Create |
                        SQLiteOpenFlags.SharedCache;

            _database = new SQLiteAsyncConnection(DatabasePath, flags);
            InitializeDatabaseAsync().Wait();
        }

        private async Task InitializeDatabaseAsync()
        {
            try
            {
                await _database.CreateTableAsync<User>();
                await _database.CreateTableAsync<NightmareRecord>();

                Console.WriteLine("База данных инициализирована");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка инициализации БД: {ex.Message}");
                throw;
            }
        }

        public async Task<List<T>> GetAll<T>() where T : new()
        {
            return await _database.Table<T>().ToListAsync();
        }

        public async Task<T> GetById<T>(int id) where T : new()
        {
            return await _database.Table<T>()
                .FirstOrDefaultAsync(item => GetIdValue(item) == id);
        }

        public async Task<int> SaveAsync<T>(T item) where T : new()
        {
            var id = GetIdValue(item);

            if (id == 0)
            {
                return await _database.InsertAsync(item);
            }
            else
            {
                await _database.UpdateAsync(item);
                return id;
            }
        }

        public async Task<int> DeleteAsync<T>(T item) where T : new()
        {
            return await _database.DeleteAsync(item);
        }

        private int GetIdValue<T>(T item)
        {
            var idProperty = typeof(T).GetProperty("Id");
            if (idProperty == null)
                throw new InvalidOperationException($"Тип {typeof(T).Name} должен иметь свойство Id");

            return (int)idProperty.GetValue(item);
        }

        public async Task<List<NightmareRecord>> GetNightmaresByUserIdAsync(int userId)
        {
            return await _database.Table<NightmareRecord>()
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.RecordDate)
                .ToListAsync();
        }

        public async Task<List<NightmareRecord>> GetNightmaresByAnxietyAsync(int userId, AnxietyLevel anxietyLevel)
        {
            return await _database.Table<NightmareRecord>()
                .Where(r => r.UserId == userId && r.RecordAnxietyLevel == anxietyLevel)
                .OrderByDescending(r => r.RecordDate)
                .ToListAsync();
        }

        public async Task<List<NightmareRecord>> SearchNightmaresAsync(int userId, string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
                return await GetNightmaresByUserIdAsync(userId);

            return await _database.Table<NightmareRecord>()
                .Where(r => r.UserId == userId &&
                       (r.Title.Contains(searchText) || r.Content.Contains(searchText)))
                .OrderByDescending(r => r.RecordDate)
                .ToListAsync();
        }
        public async Task<List<NightmareRecord>> GetNightmaresByMonthAsync(int userId, int year, int month)
        {
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            return await _database.Table<NightmareRecord>()
                .Where(r => r.UserId == userId &&
                       r.RecordDate >= startDate && r.RecordDate <= endDate)
                .OrderByDescending(r => r.RecordDate)
                .ToListAsync();
        }
        public async Task<Dictionary<AnxietyLevel, int>> GetAnxietyStatsAsync(int userId)
        {
            var records = await GetNightmaresByUserIdAsync(userId);

            return Enum.GetValues(typeof(AnxietyLevel))
                .Cast<AnxietyLevel>()
                .ToDictionary(
                    level => level,
                    level => records.Count(r => r.RecordAnxietyLevel == level)
                );
        }

        //  пользователи
        public async Task<User> GetUserByPhoneAsync(string phone)
        {
            return await _database.Table<User>()
                .FirstOrDefaultAsync(u => u.Phone == phone);
        }

        public async Task<User> GetUserByIdAsync(int id)
        {
            return await _database.Table<User>()
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<int> SaveUserAsync(User user)
        {
            if (user.Id == 0)
            {
                user.CreatedAt = DateTime.Now;
                return await _database.InsertAsync(user);
            }
            else
            {
                return await _database.UpdateAsync(user);
            }
        }

        public async Task<bool> UserExistsAsync(string phone)
        {
            var user = await GetUserByPhoneAsync(phone);
            return user != null;
        }

        // логин
        public async Task<User> ValidateUserAsync(string phone, string password)
        {
            var user = await GetUserByPhoneAsync(phone);
            if (user == null) return null;
            return user.PasswordHash == password ? user : null;
        }
    }
}
