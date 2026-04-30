using BCrypt.Net;

namespace OnboardingSystem.Services
{
    /// <summary>
    /// Хешер паролей с использованием BCrypt
    /// BCrypt автоматически добавляет salt и сложность работает регулируется work factor
    /// </summary>
    public class PasswordHasher
    {
        private const int WorkFactor = 12; // 12 = 2^12 итераций

        /// <summary>
        /// Создать хеш пароля
        /// </summary>
        public string HashPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Пароль не может быть пустым", nameof(password));

            if (password.Length < 8)
                throw new ArgumentException("Пароль должен быть минимум 8 символов", nameof(password));

            return BCrypt.Net.BCrypt.HashPassword(password, workFactor: WorkFactor);
        }

        /// <summary>
        /// Проверить пароль против хеша
        /// </summary>
        public bool VerifyPassword(string password, string hash)
        {
            if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(hash))
                return false;

            try
            {
                return BCrypt.Net.BCrypt.Verify(password, hash);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Проверить нужно ли перехешировать пароль (если work factor изменился)
        /// </summary>
        public bool NeedsRehash(string hash)
        {
            try
            {
                // Проверяем, соответствует ли хеш текущему work factor
                var currentHash = HashPassword("test");
                // Извлекаем cost из обоих хешей и сравниваем
                return hash.Substring(0, 4) != currentHash.Substring(0, 4);
            }
            catch
            {
                return true; // Если ошибка, лучше перехешировать
            }
        }
    }
}
