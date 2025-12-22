using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using nicesoon.Models;
using Microsoft.Data.Sqlite;
using SQLite;

namespace nicesoon.Services
{
    public class LocalStorageService
    {
        private SQLiteAsyncConnection _database;

        public LocalStorageService()
        {
            InitializeDatabase();
        }

        private async void InitializeDatabase()
        {
            try
            {
                if (_database == null)
                {
                    var databasePath = Path.Combine(FileSystem.AppDataDirectory, "nicesoon.db3");
                    _database = new SQLiteAsyncConnection(databasePath);

                    // Создаем таблицы
                    await _database.CreateTableAsync<User>();
                    await _database.CreateTableAsync<NightmareRecord>();
                    await _database.CreateTableAsync<DialogMessage>();

                    Console.WriteLine("База данных инициализирована");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка инициализации БД: {ex.Message}");
            }
        }

        // === РАБОТА С ПОЛЬЗОВАТЕЛЯМИ ===

        // Сохранить/обновить пользователя
        public async Task SaveUserAsync(User user)
        {
            try
            {
                if (user.Id == 0)
                {
                    // Новый пользователь
                    await _database.InsertAsync(user);
                }
                else
                {
                    // Обновление существующего
                    await _database.UpdateAsync(user);
                }

                // Также сохраняем в SecureStorage для быстрого доступа
                await SecureStorage.SetAsync("current_user_id", user.Id.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка сохранения пользователя: {ex.Message}");
            }
        }

        // Получить текущего пользователя
        public async Task<User> GetCurrentUserAsync()
        {
            try
            {
                // Получаем ID из SecureStorage
                var userIdStr = await SecureStorage.GetAsync("current_user_id");
                if (int.TryParse(userIdStr, out int userId))
                {
                    return await _database.Table<User>().FirstOrDefaultAsync(u => u.Id == userId);
                }

                // Если нет в SecureStorage, берем первого пользователя
                return await _database.Table<User>().FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка получения пользователя: {ex.Message}");
                return null;
            }
        }

        // Удалить пользователя
        public async Task<bool> DeleteUserAsync(int userId)
        {
            try
            {
                await _database.DeleteAsync<User>(userId);
                SecureStorage.Remove("current_user_id");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка удаления пользователя: {ex.Message}");
                return false;
            }
        }

        // Проверить существование пользователя
        public async Task<bool> UserExistsAsync(string phone)
        {
            try
            {
                var user = await _database.Table<User>().FirstOrDefaultAsync(u => u.Phone == phone);
                return user != null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка проверки пользователя: {ex.Message}");
                return false;
            }
        }

        // Получить пользователя по телефону
        public async Task<User> GetUserByPhoneAsync(string phone)
        {
            try
            {
                return await _database.Table<User>().FirstOrDefaultAsync(u => u.Phone == phone);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка получения пользователя: {ex.Message}");
                return null;
            }
        }

        // === РАБОТА С ЗАПИСЯМИ СНОВ ===

        // Сохранить запись сна
        public async Task<int> SaveRecordAsync(NightmareRecord record)
        {
            try
            {
                if (record.Id == 0)
                {
                    return await _database.InsertAsync(record);
                }
                else
                {
                    await _database.UpdateAsync(record);
                    return record.Id;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка сохранения записи: {ex.Message}");
                return -1;
            }
        }

        // Получить все записи пользователя
        public async Task<List<NightmareRecord>> GetRecordsAsync(int userId)
        {
            try
            {
                return await _database.Table<NightmareRecord>()
                    .Where(r => r.UserId == userId)
                    .OrderByDescending(r => r.RecordDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка получения записей: {ex.Message}");
                return new List<NightmareRecord>();
            }
        }

        // Получить запись по ID
        public async Task<NightmareRecord> GetRecordAsync(int recordId)
        {
            try
            {
                return await _database.Table<NightmareRecord>()
                    .FirstOrDefaultAsync(r => r.Id == recordId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка получения записи: {ex.Message}");
                return null;
            }
        }

        // Удалить запись
        public async Task<bool> DeleteRecordAsync(int recordId)
        {
            try
            {
                await _database.DeleteAsync<NightmareRecord>(recordId);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка удаления записи: {ex.Message}");
                return false;
            }
        }

        // === РАБОТА С СООБЩЕНИЯМИ ЧАТА ===

        // Сохранить сообщение чата
        public async Task<int> SaveMessageAsync(DialogMessage message)
        {
            try
            {
                if (message.Id == 0)
                {
                    return await _database.InsertAsync(message);
                }
                else
                {
                    await _database.UpdateAsync(message);
                    return message.Id;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка сохранения сообщения: {ex.Message}");
                return -1;
            }
        }

        // Получить историю чата для записи
        public async Task<List<DialogMessage>> GetChatHistoryAsync(int recordId)
        {
            try
            {
                return await _database.Table<DialogMessage>()
                    .Where(m => m.RecordId == recordId)
                    .OrderBy(m => m.Timestamp)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка получения истории чата: {ex.Message}");
                return new List<DialogMessage>();
            }
        }

        // === ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ ===

        // Очистить все данные пользователя
        public async Task ClearUserDataAsync(int userId)
        {
            try
            {
                // Удаляем записи пользователя
                await _database.Table<NightmareRecord>()
                    .DeleteAsync(r => r.UserId == userId);

                // Удаляем сообщения через связанные записи
                var records = await GetRecordsAsync(userId);
                foreach (var record in records)
                {
                    await _database.Table<DialogMessage>()
                        .DeleteAsync(m => m.RecordId == record.Id);
                }

                // Удаляем пользователя
                await DeleteUserAsync(userId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка очистки данных: {ex.Message}");
            }
        }

        // Создать демо-данные для тестирования
        public async Task CreateDemoDataAsync(int userId)
        {
            try
            {
                // Демо-запись 1
                var record1 = new NightmareRecord
                {
                    UserId = userId,
                    Title = "Бег в темноте",
                    Content = "Мне снилось, что я бегу по бесконечному темному коридору. Стены были влажные и холодные. Слышал шаги за спиной, но не мог развернуться посмотреть.",
                    RecordDate = DateTime.Now.AddDays(-3),
                    CreatedAt = DateTime.Now.AddDays(-3),
                    Emotions = new List<string> { "Страх", "Тревога", "Паника" }
                };

                await SaveRecordAsync(record1);

                // Демо-запись 2
                var record2 = new NightmareRecord
                {
                    UserId = userId,
                    Title = "Падение с высоты",
                    Content = "Стоял на краю небоскреба, ветер сильно дул. Внезапно почувствовал, как теряю равновесие и начал падать. Проснулся прямо перед ударом о землю.",
                    RecordDate = DateTime.Now.AddDays(-1),
                    CreatedAt = DateTime.Now.AddDays(-1),
                    Emotions = new List<string> { "Беспомощность", "Ужас" }
                };

                await SaveRecordAsync(record2);

                Console.WriteLine("Демо-данные созданы");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка создания демо-данных: {ex.Message}");
            }
        }
    }
}
