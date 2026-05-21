# Запуск тестов

## Установка зависимостей

Перейдите в корневую папку проекта и добавьте необходимые NuGet пакеты:

```bash
cd OnboardingSystemBackend
dotnet add package xunit
dotnet add package xunit.runner.visualstudio
dotnet add package Moq
dotnet add package FluentAssertions
```

## Запуск всех тестов

```bash
dotnet test
```

## Запуск тестов конкретного класса

```bash
# Тесты для TestAttemptsController
dotnet test --filter FullyQualifiedName~TestAttemptsControllerTests

# Тесты для UsersController
dotnet test --filter FullyQualifiedName~UsersControllerTests
```

## Запуск тестов с подробным выводом

```bash
dotnet test --verbosity detailed
```

## Запуск с покрытием кода (требует OpenCover)

```bash
dotnet test /p:CollectCoverage=true /p:CoverletOutput=TestResults/ /p:CoverletOutputFormat=lcov
```

## Структура тестов

### TestAttemptsControllerTests.cs

**Тесты авторизации:**
- `ResetAttempts_ShouldReturnUnauthorized_WhenUserNotAuthenticated` - неавторизованный пользователь
- `ResetAttempts_ShouldReturnForbid_WhenUserIsNotAdminOrHR` - обычный пользователь
- `ResetAttempts_ShouldSucceed_WhenUserIsAdmin` - администратор
- `ResetAttempts_ShouldSucceed_WhenUserIsHR` - HR-специалист

**Тесты бизнес-логики:**
- `ResetAttempts_ShouldDeleteAllAttempts_ForUserAndModule` - удаление всех попыток
- `ResetAttempts_ShouldResetProgressStatus_ToInProgress` - сброс статуса прогресса
- `ResetAttempts_ShouldCreateActionLog` - логирование операции
- `ResetAttempts_ShouldReturnNotFound_WhenUserNotExists` - ошибка при несуществующем пользователе
- `ResetAttempts_ShouldReturnNotFound_WhenModuleNotExists` - ошибка при несуществующем модуле
- `ResetAttempts_ShouldNotAffectOtherModules` - не влияет на другие модули
- `ResetAttempts_ShouldNotAffectOtherUsers` - не влияет на других пользователей

### UsersControllerTests.cs

**Тесты авторизации AssignMentees:**
- `AssignMentees_ShouldReturnUnauthorized_WhenUserNotAuthenticated` - неавторизованный пользователь
- `AssignMentees_ShouldReturnForbid_WhenUserIsNotAuthorized` - обычный пользователь
- `AssignMentees_ShouldSucceed_WhenUserIsAdmin` - администратор
- `AssignMentees_ShouldSucceed_WhenUserIsHR` - HR-специалист

**Тесты бизнес-логики AssignMentees:**
- `AssignMentees_ShouldSetMentorId_ForSelectedEmployees` - назначение наставника
- `AssignMentees_ShouldNotAffectOtherEmployees` - не влияет на других сотрудников
- `AssignMentees_ShouldReturnNotFound_WhenMentorNotExists` - ошибка при несуществующем наставнике
- `AssignMentees_ShouldCreateNotification` - создание уведомления

**Тесты бизнес-логики UpdateUser:**
- `UpdateUser_ShouldClearMentor_WhenDepartmentChanges` - удаление наставника при смене отдела
- `UpdateUser_ShouldKeepMentor_WhenDepartmentRemainsTheSame` - сохранение наставника
- `UpdateUser_ShouldReturnBadRequest_WhenMentorFromDifferentDepartment` - валидация наставника

## Анализ результатов

После выполнения тестов вы увидите:
- ✅ Пройденные тесты (PASSED)
- ❌ Провалившиеся тесты (FAILED)
- ⏭️ Пропущенные тесты (SKIPPED)

Все 23+ теста должны пройти успешно! 🎉
