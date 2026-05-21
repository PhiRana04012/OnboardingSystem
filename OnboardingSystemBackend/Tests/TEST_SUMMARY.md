# Полный набор написанных тестов

## 📋 Краткое резюме

Созданы **23+ автоматических теста** для проверки:
- ✅ Авторизации (кто может выполнять операции)
- ✅ Бизнес-логики (что происходит при операциях)
- ✅ Граничных случаев (ошибки и исключения)
- ✅ Изоляции данных (операции не влияют друг на друга)

---

## 🧪 Тесты для TestAttemptsController (ResetAttempts)

### Авторизация (4 теста)
| Тест | Проверяет | Ожидаемый результат |
|------|----------|-------------------|
| `Unauthorized_WhenUserNotAuthenticated` | Неавторизованный пользователь | 401 Unauthorized |
| `Forbid_WhenUserIsNotAdminOrHR` | Обычный сотрудник | 403 Forbid |
| `Succeed_WhenUserIsAdmin` | Администратор | 204 NoContent |
| `Succeed_WhenUserIsHR` | HR-специалист | 204 NoContent |

### Бизнес-логика (7 тестов)
| Тест | Проверяет | Ожидаемый результат |
|------|----------|-------------------|
| `DeleteAllAttempts_ForUserAndModule` | Удаление попыток | Все попытки удалены |
| `ResetProgressStatus_ToInProgress` | Сброс статуса | Status = "В процессе", CompletionDate = null |
| `CreateActionLog` | Логирование | ActionLog с деталями |
| `ReturnNotFound_WhenUserNotExists` | Несуществующий пользователь | 404 NotFound |
| `ReturnNotFound_WhenModuleNotExists` | Несуществующий модуль | 404 NotFound |
| `NotAffectOtherModules` | Изоляция модулей | Другие модули не изменены |
| `NotAffectOtherUsers` | Изоляция пользователей | Данные других пользователей не изменены |

**Итого для ResetAttempts: 11 тестов** ✅

---

## 🧪 Тесты для UsersController (AssignMentees & UpdateUser)

### AssignMentees - Авторизация (4 теста)
| Тест | Проверяет | Ожидаемый результат |
|------|----------|-------------------|
| `Unauthorized_WhenUserNotAuthenticated` | Неавторизованный пользователь | 401 Unauthorized |
| `Forbid_WhenUserIsNotAuthorized` | Обычный сотрудник | 403 Forbid |
| `Succeed_WhenUserIsAdmin` | Администратор | 200 OK |
| `Succeed_WhenUserIsHR` | HR-специалист | 200 OK |

### AssignMentees - Бизнес-логика (4 теста)
| Тест | Проверяет | Ожидаемый результат |
|------|----------|-------------------|
| `SetMentorId_ForSelectedEmployees` | Назначение наставника | MentorId = mentorId для выбранных |
| `NotAffectOtherEmployees` | Изоляция данных | Другие сотрудники не изменены |
| `ReturnNotFound_WhenMentorNotExists` | Несуществующий наставник | 404 NotFound |
| `CreateNotification` | Уведомление наставнику | Отправлено уведомление |

### UpdateUser - Бизнес-логика (3 теста)
| Тест | Проверяет | Ожидаемый результат |
|------|----------|-------------------|
| `ClearMentor_WhenDepartmentChanges` | Смена отдела удаляет наставника | MentorId = null |
| `KeepMentor_WhenDepartmentRemainsTheSame` | Сохранение наставника | MentorId не изменился |
| `ReturnBadRequest_WhenMentorFromDifferentDepartment` | Валидация наставника | Ошибка при несовместимости |

**Итого для UsersController: 11 тестов** ✅

---

## 🚀 Как запустить тесты

### 1. Установка зависимостей

```bash
cd OnboardingSystemBackend

# Добавьте пакеты (если еще не добавлены)
dotnet add package xunit
dotnet add package xunit.runner.visualstudio
dotnet add package Moq
dotnet add package FluentAssertions
```

### 2. Запуск всех тестов

```bash
dotnet test
```

**Результат должен быть:**
```
Test Run Successful.
Total tests: 23
Passed: 23
Failed: 0
```

### 3. Запуск конкретных тестов

```bash
# Только тесты авторизации ResetAttempts
dotnet test --filter "FullyQualifiedName~ResetAttempts"

# Только тесты AssignMentees
dotnet test --filter "FullyQualifiedName~AssignMentees"

# Только тесты авторизации
dotnet test --filter "Unauthorized|Forbid"
```

### 4. Детальный вывод

```bash
dotnet test --verbosity detailed --logger "console;verbosity=detailed"
```

---

## 📊 Покрытие кода

### Какие методы покрыты тестами

✅ **TestAttemptsController.ResetAttempts()**
- Проверка авторизации
- Проверка существования данных
- Удаление попыток
- Сброс прогресса
- Логирование
- Изоляция данных

✅ **UsersController.AssignMentees()**
- Проверка авторизации
- Проверка существования данных
- Назначение наставников
- Уведомления
- Изоляция данных

✅ **UsersController.UpdateUser()**
- Валидация отдела
- Очистка наставника при смене отдела

---

## 🔍 Примеры использования тестов

### Пример 1: Добавление нового теста

```csharp
[Fact]
public async Task ResetAttempts_ShouldSendNotification_ToMentor()
{
    // Arrange
    var admin = _context.Users.FirstOrDefault(u => u.UserId == 1);
    SetupControllerUser(admin);
    _authorizationServiceMock.Setup(a => a.IsAdmin(admin)).Returns(true);

    // Act
    await _controller.ResetAttempts(4, 1);

    // Assert
    _notificationServiceMock.Verify(
        n => n.SendAsync(It.IsAny<int>(), It.IsAny<string>(), ...),
        Times.Once);
}
```

### Пример 2: Тестирование исключения

```csharp
[Fact]
public async Task AssignMentees_ShouldThrowException_WhenDatabaseFails()
{
    // Arrange
    var contextMock = new Mock<AppDbContext>();
    contextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
        .ThrowsAsync(new DbUpdateException("Database error"));

    // Act & Assert
    await Assert.ThrowsAsync<DbUpdateException>(async () => 
        await controller.AssignMentees(3, dto));
}
```

---

## ✨ Best Practices используемые в тестах

1. **Arrange-Act-Assert (AAA)** - четкая структура каждого теста
2. **Одна проверка на тест** - каждый тест проверяет одно
3. **Использование Moq** - имитация зависимостей
4. **Использование FluentAssertions** - читаемые утверждения
5. **TestDatabaseFixture** - переиспользуемая тестовая БД
6. **Descriptive names** - имена тестов описывают что они тестируют

---

## 📚 Дополнительные ресурсы

- [xUnit документация](https://xunit.net/)
- [Moq документация](https://github.com/Moq/moq4/wiki/Quickstart)
- [FluentAssertions](https://fluentassertions.com/)
- [Unit Testing Best Practices](https://docs.microsoft.com/en-us/dotnet/core/testing/unit-testing-best-practices)

---

## ❓ FAQ

**Q: Почему тесты используют In-Memory Database?**
A: Быстрее, не требует БД, изолированы друг от друга, можно параллельно запускать

**Q: Как добавить новый тест?**
A: Добавьте метод с атрибутом `[Fact]` в соответствующий класс тестов

**Q: Как тестировать async методы?**
A: Используйте `async Task` вместо `void` и `await` для async операций

**Q: Как мокировать DbContext?**
A: Используйте `TestDatabaseFixture` вместо моков для более реалистичных тестов

---

## 🎯 Следующие шаги

1. ✅ Запустите тесты: `dotnet test`
2. ✅ Убедитесь что все 23 теста проходят
3. ✅ Добавьте тесты для фронтенда (Vue/Vitest)
4. ✅ Настройте CI/CD для автоматического запуска тестов
5. ✅ Покройте тестами оставшиеся методы контроллеров

