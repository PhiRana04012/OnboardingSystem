# Интеграция Keycloak - Инструкция

## 📋 Что было реализовано

Полная интеграция Keycloak для системы онбординга с Single Sign-On (SSO):

### Бэкенд (.NET)
- ✅ Keycloak Admin сервис для управления пользователями
- ✅ JWT Bearer authentication для защиты API endpoints
- ✅ Автоматическое создание пользователя в Keycloak при регистрации
- ✅ Синхронизация обновления пользователя с Keycloak
- ✅ Удаление пользователя из Keycloak при удалении в системе
- ✅ Миграция БД для поля `KeycloakUserId`

### Фронтенд (Vue 3)
- ✅ Интеграция keycloak-js библиотеки
- ✅ Auth Store в Pinia с методами для Keycloak логина/логаута
- ✅ Обновленная страница логина с кнопкой входа через Keycloak
- ✅ Добавление JWT токена в заголовки всех API запросов
- ✅ Перехватчик для обновления токена перед истечением
- ✅ Демонстрационный режим как fallback

### Docker
- ✅ Keycloak контейнер (port 8080)
- ✅ PostgreSQL для хранилища Keycloak (port 5433)

---

## 🚀 Быстрый старт

### 1. Запустить сервисы через Docker Compose

```bash
cd c:\Users\Firan\ONBOARDING
docker-compose up -d
```

Это запустит:
- Keycloak на http://localhost:8080
- PostgreSQL для Keycloak
- Mailhog на http://localhost:8025

### 2. Инициализировать Keycloak

Откройте http://localhost:8080/admin в браузере

**Логин:**
- Username: `admin`
- Password: `admin`

### 3. Создать Realm

1. Нажмите на dropdown "Master" слева
2. Нажмите "Create Realm"
3. Введите имя: `onboarding`
4. Нажмите "Create"

### 4. Создать Client

1. В левом меню выберите "Clients"
2. Нажмите "Create client"
3. Введите Client ID: `onboarding-web`
4. Выберите Client type: "Public"
5. Нажмите "Next"
6. В "Access settings":
   - Valid redirect URIs: `http://localhost:3002/*`
   - Valid post logout redirect URIs: `http://localhost:3002/*`
7. Нажмите "Save"

### 5. Создать Client для бэкенда

1. Нажмите "Create client"
2. Введите Client ID: `onboarding-api`
3. Выберите Client type: "Public"
4. Нажмите "Save"

### 6. Запустить приложение

#### Бэкенд
```bash
cd c:\Users\Firan\ONBOARDING\OnboardingSystemBackend
dotnet run
```

Бэкенд запустится на http://localhost:5233

#### Фронтенд

Откройте новый терминал:
```bash
cd c:\Users\Firan\ONBOARDING\onboarding-frontend
npm install  # Первый раз
npm run dev
```

Приложение будет доступно на http://localhost:3002

---

## 🔐 Процесс авторизации

### 1. Администратор добавляет пользователя

**POST /api/users**
```json
{
  "fullName": "Иван Петров",
  "email": "ivan@company.com",
  "departmentId": 1,
  "hireDate": "2026-04-28",
  "roleIds": [1, 2]
}
```

**Что происходит:**
1. Бэкенд генерирует временный пароль
2. Создает пользователя в Keycloak
3. Сохраняет пользователя в БД с KeycloakUserId
4. Отправляет приветственное письмо (через Mailhog)

### 2. Пользователь входит в систему

1. Кликает "Войти через учетную запись"
2. Перенаправляется на Keycloak
3. Вводит email/пароль (или меняет временный пароль)
4. Возвращается в приложение с JWT токеном
5. Токен сохраняется в localStorage
6. Загружаются данные пользователя из БД

### 3. Все API запросы включают токен

```
Authorization: Bearer eyJhbGciOiJSUzI1NiIsInR5cC...
```

---

## 🔑 Ключевые файлы

### Бэкенд
- [Program.cs](OnboardingSystemBackend/Program.cs) - JWT конфигурация
- [appsettings.json](OnboardingSystemBackend/appsettings.json) - Keycloak переменные
- [Services/KeycloakAdminService.cs](OnboardingSystemBackend/Services/KeycloakAdminService.cs) - Управление пользователями
- [Controllers/UsersController.cs](OnboardingSystemBackend/Controllers/UsersController.cs) - Интеграция при создании

### Фронтенд
- [stores/auth.js](onboarding-frontend/src/stores/auth.js) - Auth Store с Keycloak
- [api/index.js](onboarding-frontend/src/api/index.js) - Добавление токена в запросы
- [views/Login.vue](onboarding-frontend/src/views/Login.vue) - Страница логина
- [.env.development](onboarding-frontend/.env.development) - Переменные окружения

---

## 🧪 Тестирование

### Тестовые пользователи

Вы можете создать тестовых пользователей через админ панель Keycloak:

1. Перейдите на http://localhost:8080/admin
2. Выберите realm "onboarding"
3. Слева нажмите "Users"
4. Нажмите "Add user"
5. Заполните форму и создайте

### Демонстрационный режим

Если Keycloak недоступен, приложение предложит демонстрационный режим с логином по ID пользователя.

---

## 🔧 Конфигурация

### Бэкенд (appsettings.json)

```json
{
  "Keycloak": {
    "AdminUrl": "http://localhost:8080",
    "Realm": "onboarding",
    "AdminUser": "admin",
    "AdminPassword": "admin",
    "ClientId": "onboarding-api"
  }
}
```

### Фронтенд (.env.development)

```
VITE_KEYCLOAK_URL=http://localhost:8080
VITE_KEYCLOAK_REALM=onboarding
VITE_KEYCLOAK_CLIENT_ID=onboarding-web
```

---

## 📊 Архитектура потока

```
┌─────────────────────────────────┐
│     Администратор               │
│     Создает пользователя        │
└────────────┬────────────────────┘
             │
             ▼
┌─────────────────────────────────────────┐
│ Backend: POST /api/users                │
│  1. Генерирует пароль                  │
│  2. Создает в Keycloak                 │
│  3. Сохраняет в БД                     │
│  4. Отправляет письмо                  │
└────────────┬────────────────────────────┘
             │
             ▼
┌─────────────────────────────────┐
│     Новый сотрудник             │
│     Получает письмо с ссылкой   │
└────────────┬────────────────────┘
             │
             ▼
┌─────────────────────────────────┐
│  1. Кликает "Войти"             │
│  2. Перенаправляется на Keycloak│
│  3. Вводит пароль              │
│  4. Возвращается в приложение  │
│  5. JWT токен в localStorage   │
└────────────┬────────────────────┘
             │
             ▼
┌─────────────────────────────────┐
│  Доступ к защищенным ресурсам  │
│  Все запросы + Authorization    │
└─────────────────────────────────┘
```

---

## 🚨 Возможные проблемы и решения

### ❌ "Timeout when waiting for 3rd party check iframe message" ошибка

**Причины:**
1. Браузер блокирует доступ к localStorage из iframe (Tracking Prevention)
2. CORS не настроены правильно
3. Отсутствует файл `silent-check-sso.html`
4. Неверная конфигурация Keycloak realm

**Решение:**
1. **Создать файл `silent-check-sso.html`** (уже добавлен в `public/`)
2. **Обновить docker-compose.yml** с правильными CORS параметрами (уже сделано)
3. **Настроить браузер:**
   - **Chrome/Edge:** Отключите "Enhanced tracking prevention" для localhost
     - Settings → Privacy and security → Enhanced tracking prevention → Disable
   - **Firefox:** Выключите защиту от отслеживания для localhost
     - about:preferences → Privacy → Custom → Отключите "Tracking protection"
4. **Убедитесь, что Keycloak client настроен правильно:**

### 🔧 Правильная конфигурация Client в Keycloak

1. Откройте Keycloak admin: http://localhost:8080/admin
2. Выберите realm: "onboarding"
3. Слева: Clients → onboarding-web → Settings:
   
   **Основные параметры:**
   - Client ID: `onboarding-web`
   - Client type: `Public` ✓
   
   **Access settings (вкладка):**
   - Root URL: `http://localhost:3002`
   - Valid redirect URIs: 
     ```
     http://localhost:3002/*
     http://localhost:3002
     ```
   - Valid post logout redirect URIs:
     ```
     http://localhost:3002/*
     http://localhost:3002
     ```
   - Web origins:
     ```
     http://localhost:3002
     ```
   
   **Advanced settings (вкладка):**
   - Proof Key for Public Clients (PKCE): `ON`
   - Authorization Code Flow Enabled: `ON`
   - Standard Flow Enabled: `ON`
   - Implicit flow enabled: `OFF`
   - Direct access grants enabled: `OFF`

4. Нажмите "Save"

### ❌ "Tracking Prevention blocked access to storage"

**Решение для локальной разработки:**

#### Chrome / Edge
```
Откройте chrome://settings/content/cookies
Нажмите "Add" в разделе "Allow"
Введите: [*.]localhost:3002
Сохраните
```

#### Firefox
```
about:preferences#privacy
Найдите "Enhanced Tracking Protection"
Нажмите "Manage Exceptions"
Добавьте: http://localhost:3002
Сохраните
```

#### Safari (macOS)
```
Preferences → Privacy
Отключите "Prevent cross-site tracking"
Или добавьте localhost в исключения
```

### ✅ Проверка конфигурации

Откройте браузер DevTools (F12) и проверьте:

```javascript
// В консоли выполните:
console.log(localStorage.getItem('auth_token')); // Должен быть токен после логина
console.log(localStorage.getItem('currentUser')); // Должны быть данные пользователя
```

### 🧪 Отладка Keycloak инициализации

В файле `src/stores/auth.js` добавьте логирование:

```javascript
const initKeycloak = async () => {
  try {
    console.log('🔍 Keycloak config:', {
      url: import.meta.env.VITE_KEYCLOAK_URL,
      realm: import.meta.env.VITE_KEYCLOAK_REALM,
      clientId: import.meta.env.VITE_KEYCLOAK_CLIENT_ID
    });
    
    // ... остальной код
  } catch (error) {
    console.error('❌ Keycloak init error:', error);
    throw error;
  }
}
```

### 🐳 Перезапуск контейнеров

Если ничего не помогает, пересоздайте контейнеры:

```bash
# Остановить все контейнеры
docker-compose down -v

# Пересобрать образы
docker-compose build --no-cache

# Запустить
docker-compose up -d

# Проверить логи
docker logs onboarding_keycloak
```

### 🚨 Keycloak не стартует
```bash
# Проверьте логи
docker logs onboarding_keycloak

# Остановите и удалите контейнеры
docker-compose down -v

# Перезапустите
docker-compose up -d
```

### 401 Unauthorized при запросах
- Проверьте, что токен сохранен в localStorage
- Проверьте конфигурацию Keycloak в бэкенде
- Убедитесь, что JWT подпись правильная

### CORS ошибки
- Проверьте конфигурацию CORS в Program.cs
- Убедитесь, что фронтенд работает на http://localhost:3002

---

## 📚 Дополнительная информация

- [Keycloak Documentation](https://www.keycloak.org/documentation)
- [keycloak-js Library](https://github.com/keycloak/keycloak-js)
- [JWT Bearer in ASP.NET Core](https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.authentication.jwtbearer)

---

## ✅ Чек-лист для продакшена

- [ ] Изменить пароли admin в .env
- [ ] Включить HTTPS (RequireHttpsMetadata = true)
- [ ] Настроить EMAIL сервер для отправки писем
- [ ] Создать и настроить роли в Keycloak
- [ ] Настроить политики паролей
- [ ] Включить двухфакторную аутентификацию (опционально)
- [ ] Настроить логирование и мониторинг
- [ ] Провести security аудит

---

**Дата создания:** 28.04.2026  
**Версия:** 1.0
