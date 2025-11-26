# Решение проблем

## Ошибка 500 при входе в систему

### Симптомы
При попытке войти с ID пользователя появляется ошибка 500 (Internal Server Error).

### Возможные причины

#### 1. Пользователь не имеет связанного подразделения (Department)
**Проблема:** В контроллере `UsersController.cs` на строке 79 происходит обращение к `user.Department.Name`, но если `user.Department` равен `null`, возникает `NullReferenceException`.

**Решение на бэкенде:**
Измените метод `GetUser` в `UsersController.cs`:

```csharp
var userDto = new UserDto
{
    UserId = user.UserId,
    ExternalId = user.ExternalId,
    FullName = user.FullName,
    Email = user.Email,
    DepartmentId = user.DepartmentId,
    DepartmentName = user.Department?.Name ?? "Не указано", // Добавлена проверка на null
    MentorId = user.MentorId,
    MentorName = user.Mentor != null ? user.Mentor.FullName : null,
    HireDate = user.HireDate,
    OnboardingStatus = user.OnboardingStatus,
    Roles = user.Roles.Select(r => r.RoleName).ToList()
};
```

#### 2. База данных не инициализирована
**Проблема:** Таблицы в базе данных не созданы или миграции не применены.

**Решение:**
```bash
# В папке OnboardingSystemBackend
dotnet ef database update
```

#### 3. Проблема с подключением к базе данных
**Проблема:** Неправильная строка подключения или SQL Server не запущен.

**Решение:**
1. Проверьте строку подключения в `appsettings.json`
2. Убедитесь, что SQL Server запущен
3. Проверьте, что база данных существует

#### 4. Пользователь с таким ID не существует
**Проблема:** Введён несуществующий ID пользователя.

**Решение:**
- Проверьте существующие ID пользователей в базе данных
- Используйте Swagger UI (`http://localhost:5000/swagger`) для просмотра всех пользователей

### Как проверить проблему

1. **Проверьте логи бэкенда:**
   - Запустите бэкенд в режиме отладки
   - Посмотрите детали ошибки в консоли или логах

2. **Проверьте через Swagger:**
   - Откройте `http://localhost:5000/swagger`
   - Попробуйте вызвать `GET /api/users/{id}` напрямую
   - Посмотрите детали ошибки

3. **Проверьте базу данных:**
   ```sql
   -- Проверьте существование пользователя
   SELECT * FROM Users WHERE UserId = <ваш_id>
   
   -- Проверьте, что у пользователя есть DepartmentId
   SELECT u.UserId, u.FullName, u.DepartmentId, d.Name 
   FROM Users u
   LEFT JOIN Departments d ON u.DepartmentId = d.DepartmentId
   WHERE u.UserId = <ваш_id>
   ```

### Быстрое исправление (временное)

Если нужно быстро протестировать систему, создайте пользователя с правильными связями:

1. Убедитесь, что есть хотя бы одно подразделение в таблице `Departments`
2. Создайте пользователя через Swagger или напрямую в БД с указанием `DepartmentId`

### Проверка конфигурации

1. **Проверьте строку подключения:**
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=.\\SQLEXPRESS;Database=onboarding;Integrated Security=true;TrustServerCertificate=true;"
     }
   }
   ```

2. **Проверьте, что бэкенд запущен:**
   - Должен быть доступен на `http://localhost:5000`
   - Проверьте через браузер: `http://localhost:5000/swagger`

3. **Проверьте CORS:**
   - Убедитесь, что CORS настроен правильно в `Program.cs`

## Другие распространённые проблемы

### Ошибка подключения к API
**Симптомы:** "Не удалось подключиться к серверу"

**Решение:**
1. Убедитесь, что бэкенд запущен
2. Проверьте URL в `vite.config.js` (должен быть `http://localhost:5000`)
3. Проверьте, что порты не заняты другими приложениями

### CORS ошибки
**Симптомы:** "CORS policy" в консоли браузера

**Решение:**
В `Program.cs` бэкенда должна быть настройка:
```csharp
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});
```

### Проблемы со стилями
**Симптомы:** Интерфейс выглядит неправильно

**Решение:**
1. Убедитесь, что Tailwind CSS правильно настроен
2. Перезапустите dev-сервер: `npm run dev`
3. Очистите кэш браузера

## Получение помощи

Если проблема не решена:
1. Проверьте логи бэкенда для деталей ошибки
2. Проверьте консоль браузера (F12) для ошибок на фронтенде
3. Убедитесь, что все зависимости установлены (`npm install`)








