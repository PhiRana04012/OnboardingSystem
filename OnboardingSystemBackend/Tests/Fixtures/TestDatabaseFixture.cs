using Microsoft.EntityFrameworkCore;
using OnboardingSystem.Data;
using OnboardingSystem.Entities;

namespace OnboardingSystem.Tests.Fixtures;

/// <summary>
/// Фикстура для создания тестовой БД в памяти
/// </summary>
public class TestDatabaseFixture : IDisposable
{
    public AppDbContext Context { get; }

    public TestDatabaseFixture()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        Context = new AppDbContext(options);
        Context.Database.EnsureCreated();
        
        SeedTestData();
    }

    private void SeedTestData()
    {
        // Роли
        var adminRole = new Role { RoleId = 1, RoleName = "Администратор системы" };
        var hrRole = new Role { RoleId = 2, RoleName = "HR-специалист" };
        var mentorRole = new Role { RoleId = 3, RoleName = "Наставник" };
        var employeeRole = new Role { RoleId = 4, RoleName = "Новый сотрудник" };
        
        Context.Roles.AddRange(adminRole, hrRole, mentorRole, employeeRole);

        // Должности
        var deptHeadPosition = new JobTitle 
        { 
            JobTitleId = 1, 
            Title = "Начальник отдела", 
            Description = "Руководитель отдела" 
        };
        var devPosition = new JobTitle 
        { 
            JobTitleId = 2, 
            Title = "Разработчик", 
            Description = "Программист" 
        };

        Context.JobTitles.AddRange(deptHeadPosition, devPosition);

        // Отделы
        var itDept = new Department { DepartmentId = 1, Name = "IT" };
        var hrDept = new Department { DepartmentId = 2, Name = "HR" };

        Context.Departments.AddRange(itDept, hrDept);

        // Пользователи
        var admin = new User
        {
            UserId = 1,
            ExternalId = "admin1",
            FullName = "Администратор Админов",
            Email = "admin@test.com",
            PasswordHash = "hashed_password",
            DepartmentId = 1,
            HireDate = DateTime.UtcNow.AddMonths(-12),
            OnboardingStatus = "Завершён",
            Roles = new List<Role> { adminRole }
        };

        var hr = new User
        {
            UserId = 2,
            ExternalId = "hr1",
            FullName = "Кадровик Иванов",
            Email = "hr@test.com",
            PasswordHash = "hashed_password",
            DepartmentId = 2,
            HireDate = DateTime.UtcNow.AddMonths(-12),
            OnboardingStatus = "Завершён",
            Roles = new List<Role> { hrRole }
        };

        var mentor = new User
        {
            UserId = 3,
            ExternalId = "mentor1",
            FullName = "Петров Иван",
            Email = "mentor@test.com",
            PasswordHash = "hashed_password",
            DepartmentId = 1,
            JobTitleId = 2,
            HireDate = DateTime.UtcNow.AddMonths(-12),
            OnboardingStatus = "Завершён",
            Roles = new List<Role> { mentorRole }
        };

        var newEmployee1 = new User
        {
            UserId = 4,
            ExternalId = "emp1",
            FullName = "Сидоров Сергей",
            Email = "employee1@test.com",
            PasswordHash = "hashed_password",
            DepartmentId = 1,
            JobTitleId = 2,
            HireDate = DateTime.UtcNow.AddDays(-10),
            OnboardingStatus = "В процессе",
            Roles = new List<Role> { employeeRole }
        };

        var newEmployee2 = new User
        {
            UserId = 5,
            ExternalId = "emp2",
            FullName = "Козлов Константин",
            Email = "employee2@test.com",
            PasswordHash = "hashed_password",
            DepartmentId = 1,
            JobTitleId = 2,
            HireDate = DateTime.UtcNow.AddDays(-5),
            OnboardingStatus = "В процессе",
            Roles = new List<Role> { employeeRole }
        };

        var anotherDeptEmployee = new User
        {
            UserId = 6,
            ExternalId = "emp3",
            FullName = "Волков Владимир",
            Email = "employee3@test.com",
            PasswordHash = "hashed_password",
            DepartmentId = 2,
            JobTitleId = 2,
            HireDate = DateTime.UtcNow.AddDays(-8),
            OnboardingStatus = "В процессе",
            Roles = new List<Role> { employeeRole }
        };

        Context.Users.AddRange(admin, hr, mentor, newEmployee1, newEmployee2, anotherDeptEmployee);

        // Модули
        var module1 = new Module
        {
            ModuleId = 1,
            Title = "Введение в компанию",
            Description = "Базовое ознакомление",
            DepartmentId = 1,
            IsMandatory = true,
            MaxAttempts = 3,
            PassingScore = 70,
            CreatedAt = DateTime.UtcNow.AddMonths(-3)
        };

        var module2 = new Module
        {
            ModuleId = 2,
            Title = "IT процессы",
            Description = "Понимание IT процессов",
            DepartmentId = 1,
            IsMandatory = true,
            MaxAttempts = 3,
            PassingScore = 75,
            CreatedAt = DateTime.UtcNow.AddMonths(-2)
        };

        Context.Modules.AddRange(module1, module2);

        // Попытки прохождения тестов
        var attempt1 = new TestAttempt
        {
            AttemptId = 1,
            UserId = 4,
            ModuleId = 1,
            AttemptDate = DateTime.UtcNow.AddDays(-3),
            AttemptNumber = 1,
            Score = 65,
            IsPassed = false
        };

        var attempt2 = new TestAttempt
        {
            AttemptId = 2,
            UserId = 4,
            ModuleId = 1,
            AttemptDate = DateTime.UtcNow.AddDays(-1),
            AttemptNumber = 2,
            Score = 85,
            IsPassed = true
        };

        Context.TestAttempts.AddRange(attempt1, attempt2);

        // Прогресс модулей
        var progress1 = new UserModuleProgress
        {
            UserId = 4,
            ModuleId = 1,
            Status = "Завершён",
            StartDate = DateTime.UtcNow.AddDays(-3),
            CompletionDate = DateTime.UtcNow.AddDays(-1)
        };

        Context.UserModuleProgresses.Add(progress1);

        Context.SaveChanges();
    }

    public void Dispose()
    {
        Context?.Dispose();
    }
}
