using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using nicesoon.Models;
namespace nicesoon.Services
{
    public class AuthService
    {
        private readonly DatabaseService _dbService;
        private User _currentUser;

        public event EventHandler AuthStateChanged;

        public User CurrentUser => _currentUser;
        public bool IsAuthenticated => _currentUser != null;

        public AuthService()
        {

        }
        //public AuthService(DatabaseService dbService)
        //{
        //    _dbService = dbService;
        //}

        // Логин
        public async Task<bool> LoginAsync(string phone, string password)
        {
            try
            {
                var cleanPhone = CleanPhoneNumber(phone);

                var user = await _dbService.ValidateUserAsync(cleanPhone, password);

                if (user != null)
                {
                    _currentUser = user;
                    user.LastLogin = DateTime.Now;
                    await _dbService.SaveUserAsync(user);

                    await SecureStorage.SetAsync("user_id", user.Id.ToString());
                    await SecureStorage.SetAsync("user_phone", user.Phone);

                    AuthStateChanged?.Invoke(this, EventArgs.Empty);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка входа: {ex.Message}");
                return false;
            }
        }

        // Регистрация
        public async Task<bool> RegisterAsync(string phone, string username, string password)
        {
            try
            {
                var cleanPhone = CleanPhoneNumber(phone);

                if (await _dbService.UserExistsAsync(cleanPhone))
                    return false;

                var user = new User
                {
                    Phone = cleanPhone,
                    Username = username,
                    PasswordHash = password,
                    CreatedAt = DateTime.Now
                };

                await _dbService.SaveUserAsync(user);

                return await LoginAsync(phone, password);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка регистрации: {ex.Message}");
                return false;
            }
        }

        // Выход
        public void Logout()
        {
            _currentUser = null;
            SecureStorage.Remove("user_id");
            SecureStorage.Remove("user_phone");
            AuthStateChanged?.Invoke(this, EventArgs.Empty);
        }

        // Автологин
        public async Task<bool> TryAutoLoginAsync()
        {
            try
            {
                var userIdStr = await SecureStorage.GetAsync("user_id");
                if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
                    return false;

                _currentUser = await _dbService.GetUserByIdAsync(userId);

                if (_currentUser != null)
                {
                    AuthStateChanged?.Invoke(this, EventArgs.Empty);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка автологина: {ex.Message}");
                return false;
            }
        }
        private string CleanPhoneNumber(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return phone;

            var digits = new string(phone.Where(char.IsDigit).ToArray());

            if (digits.StartsWith("7") || digits.StartsWith("8"))
            {
                return "+7" + digits.Substring(1);
            }
            else if (digits.StartsWith("+7"))
            {
                return digits;
            }
            else if (digits.Length == 10)
            {
                return "+7" + digits;
            }

            return phone;
        }
    }
}
