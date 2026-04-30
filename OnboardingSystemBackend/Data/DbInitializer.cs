using OnboardingSystem.Entities;
using OnboardingSystem.Services;

namespace OnboardingSystem.Data
{
    public static class DbInitializer
    {
        public static void Initialize(AppDbContext context, PasswordHasher? passwordHasher = null)
        {
            context.Database.EnsureCreated();

            context.Database.EnsureCreated();

            // Инициализируем PasswordHasher если не передан
            passwordHasher ??= new PasswordHasher();

            // --- Роли (Roles) ---
            if (!context.Roles.Any())
            {
                var roles = new Role[]
                {
                    new Role { RoleName = "Администратор системы" },
                    new Role { RoleName = "HR-специалист" },
                    new Role { RoleName = "Руководитель подразделения" },
                    new Role { RoleName = "Наставник" },
                    new Role { RoleName = "Новый сотрудник" }
                };

                foreach (var r in roles)
                {
                    context.Roles.Add(r);
                }
                context.SaveChanges();
            }

            // --- Подразделения (Departments) ---
            if (!context.Departments.Any())
            {
                var departments = new Department[]
                {
                    new Department { Name = "IT Отдел", ExternalId = "IT001" },
                    new Department { Name = "HR Отдел", ExternalId = "HR001" },
                    new Department { Name = "Бухгалтерия", ExternalId = "ACC001" }
                };
                context.Departments.AddRange(departments);
                context.SaveChanges();
            }
            
            // --- Пользователи (Users) ---
            if (!context.Users.Any())
            {
                var itDept = context.Departments.First(d => d.Name == "IT Отдел");
                var hrDept = context.Departments.First(d => d.Name == "HR Отдел");
                
                var adminRole = context.Roles.First(r => r.RoleName == "Администратор системы");
                var hrRole = context.Roles.First(r => r.RoleName == "HR-специалист");
                var empRole = context.Roles.First(r => r.RoleName == "Новый сотрудник");

                var mentorRole = context.Roles.First(r => r.RoleName == "Наставник");

                var users = new List<User>
                {
                    new User
                    {
                        FullName = "Системный Администратор",
                        Email = "admin@example.com",
                        HireDate = DateOnly.FromDateTime(DateTime.Now.AddYears(-1)),
                        OnboardingStatus = "Завершен", // Локализация
                        JobTitle = "Старший Администратор",
                        Department = itDept,
                        PasswordHash = passwordHasher.HashPassword("admin1234567")
                    },
                    new User
                    {
                        FullName = "Мария Ивановна (HR)",
                        Email = "hr@example.com",
                        HireDate = DateOnly.FromDateTime(DateTime.Now.AddMonths(-6)),
                        OnboardingStatus = "Завершен",
                        JobTitle = "HR Менеджер",
                        Department = hrDept,
                        PasswordHash = passwordHasher.HashPassword("hr1234567")
                    },
                    new User
                    {
                        FullName = "Иван Новичков",
                        Email = "new@example.com",
                        HireDate = DateOnly.FromDateTime(DateTime.Now),
                        OnboardingStatus = "В процессе",
                        JobTitle = "Младший Разработчик",
                        Department = itDept,
                        PasswordHash = passwordHasher.HashPassword("new1234567")
                    },
                    new User
                    {
                        FullName = "Петр Сергеевич",
                        Email = "mentor1@example.com",
                        HireDate = DateOnly.FromDateTime(DateTime.Now.AddYears(-3)),
                        OnboardingStatus = "Завершен",
                        JobTitle = "Старший Разработчик",
                        Department = itDept,
                        PasswordHash = passwordHasher.HashPassword("mMentor123")
                    },
                    new User
                    {
                        FullName = "Александр Валерьевич",
                        Email = "mentor2@example.com",
                        HireDate = DateOnly.FromDateTime(DateTime.Now.AddYears(-2)),
                        OnboardingStatus = "Завершен",
                        JobTitle = "Ведущий Разработчик",
                        Department = itDept,
                        PasswordHash = passwordHasher.HashPassword("mMentor123")
                    },
                    new User
                    {
                        FullName = "Елена Михайловна",
                        Email = "mentor3@example.com",
                        HireDate = DateOnly.FromDateTime(DateTime.Now.AddYears(-4)),
                        OnboardingStatus = "Завершен",
                        JobTitle = "Архитектор систем",
                        Department = itDept,
                        PasswordHash = passwordHasher.HashPassword("mentor1234")
                    }
                };

                context.Users.AddRange(users);
                context.SaveChanges();

                // Присвоение ролей
                users[0].Roles.Add(adminRole);
                users[1].Roles.Add(hrRole);
                users[2].Roles.Add(empRole);
                users[3].Roles.Add(mentorRole);
                users[4].Roles.Add(mentorRole);
                users[5].Roles.Add(mentorRole);
                context.SaveChanges();
            }

            // --- Модули (Modules) ---
            if (!context.Modules.Any())
            {
                var itDept = context.Departments.First(d => d.Name == "IT Отдел");

                var modules = new List<Module>
                {
                    new Module
                    {
                        Title = "Введение в компанию",
                        Description = "История, миссия и ценности нашей компании.",
                        Content = "## Добро пожаловать!\n\nНаша компания была основана в 2010 году...",
                        IsMandatory = true,
                        Department = itDept,
                        PassingScore = 80,
                        MaxAttempts = 3
                    },
                    new Module
                    {
                        Title = "Безопасность труда",
                        Description = "Основные правила безопасности на рабочем месте.",
                        Content = "## Правила безопасности\n\n1. Соблюдайте чистоту...\n2. Следите за проводами...",
                        IsMandatory = true,
                        Department = itDept,
                        PassingScore = 70,
                        MaxAttempts = 5
                    }
                };
                context.Modules.AddRange(modules);
                context.SaveChanges();

                // --- Вопросы (Questions) ---
                var welcomeModule = modules[0];
                var safetyModule = modules[1];

                var questions = new List<Question>
                {
                    // Вопросы для "Введение в компанию" (10+ вопросов)
                    new Question
                    {
                        Module = welcomeModule,
                        QuestionText = "В каком году была основана компания?",
                        AnswerOptions = new List<AnswerOption>
                        {
                            new AnswerOption { AnswerText = "2005", IsCorrect = false },
                            new AnswerOption { AnswerText = "2010", IsCorrect = true },
                            new AnswerOption { AnswerText = "2020", IsCorrect = false }
                        }
                    },
                    new Question
                    {
                        Module = welcomeModule,
                        QuestionText = "Какова наша главная ценность?",
                        AnswerOptions = new List<AnswerOption>
                        {
                            new AnswerOption { AnswerText = "Клиентоориентированность", IsCorrect = true },
                            new AnswerOption { AnswerText = "Скорость", IsCorrect = false },
                            new AnswerOption { AnswerText = "Бюрократия", IsCorrect = false }
                        }
                    },
                    new Question { Module = welcomeModule, QuestionText = "Кто является генеральным директором?", AnswerOptions = new List<AnswerOption> { new AnswerOption { AnswerText = "Иванов И.И.", IsCorrect = true }, new AnswerOption { AnswerText = "Петров П.П.", IsCorrect = false } } },
                    new Question { Module = welcomeModule, QuestionText = "Где находится головной офис?", AnswerOptions = new List<AnswerOption> { new AnswerOption { AnswerText = "Москва", IsCorrect = true }, new AnswerOption { AnswerText = "Санкт-Петербург", IsCorrect = false } } },
                    new Question { Module = welcomeModule, QuestionText = "Сколько сотрудников в компании?", AnswerOptions = new List<AnswerOption> { new AnswerOption { AnswerText = "Более 1000", IsCorrect = true }, new AnswerOption { AnswerText = "Менее 50", IsCorrect = false } } },
                    new Question { Module = welcomeModule, QuestionText = "Какой график работы?", AnswerOptions = new List<AnswerOption> { new AnswerOption { AnswerText = "9:00 - 18:00", IsCorrect = true }, new AnswerOption { AnswerText = "10:00 - 19:00", IsCorrect = false } } },
                    new Question { Module = welcomeModule, QuestionText = "Как часто выплачивается зарплата?", AnswerOptions = new List<AnswerOption> { new AnswerOption { AnswerText = "2 раза в месяц", IsCorrect = true }, new AnswerOption { AnswerText = "1 раз в месяц", IsCorrect = false } } },
                    new Question { Module = welcomeModule, QuestionText = "Есть ли дресс-код?", AnswerOptions = new List<AnswerOption> { new AnswerOption { AnswerText = "Да, строгий", IsCorrect = false }, new AnswerOption { AnswerText = "Нет, свободный стиль", IsCorrect = true } } },
                    new Question { Module = welcomeModule, QuestionText = "Можно ли работать удаленно?", AnswerOptions = new List<AnswerOption> { new AnswerOption { AnswerText = "Да, по согласованию", IsCorrect = true }, new AnswerOption { AnswerText = "Нет, только офис", IsCorrect = false } } },
                    new Question { Module = welcomeModule, QuestionText = "Как оформить отпуск?", AnswerOptions = new List<AnswerOption> { new AnswerOption { AnswerText = "Через портал", IsCorrect = true }, new AnswerOption { AnswerText = "Устно", IsCorrect = false } } },

                    // Вопросы для "Безопасность труда" (10+ вопросов)
                    new Question
                    {
                        Module = safetyModule,
                        QuestionText = "Что делать при пожаре?",
                        AnswerOptions = new List<AnswerOption>
                        {
                            new AnswerOption { AnswerText = "Бежать", IsCorrect = false },
                            new AnswerOption { AnswerText = "Звонить 101 и эвакуироваться", IsCorrect = true },
                            new AnswerOption { AnswerText = "Продолжать работать", IsCorrect = false }
                        }
                    },
                    new Question { Module = safetyModule, QuestionText = "Где находится огнетушитель?", AnswerOptions = new List<AnswerOption> { new AnswerOption { AnswerText = "В коридоре", IsCorrect = true }, new AnswerOption { AnswerText = "У директора", IsCorrect = false } } },
                    new Question { Module = safetyModule, QuestionText = "Кому сообщать о травме?", AnswerOptions = new List<AnswerOption> { new AnswerOption { AnswerText = "Руководителю", IsCorrect = true }, new AnswerOption { AnswerText = "Никому", IsCorrect = false } } },
                    new Question { Module = safetyModule, QuestionText = "Как часто проходить инструктаж?", AnswerOptions = new List<AnswerOption> { new AnswerOption { AnswerText = "Раз в полгода", IsCorrect = true }, new AnswerOption { AnswerText = "Никогда", IsCorrect = false } } },
                    new Question { Module = safetyModule, QuestionText = "Можно ли курить в офисе?", AnswerOptions = new List<AnswerOption> { new AnswerOption { AnswerText = "Нет, запрещено", IsCorrect = true }, new AnswerOption { AnswerText = "Да, везде", IsCorrect = false } } },
                    new Question { Module = safetyModule, QuestionText = "Что делать если ударило током?", AnswerOptions = new List<AnswerOption> { new AnswerOption { AnswerText = "Обратиться к врачу", IsCorrect = true }, new AnswerOption { AnswerText = "Терпеть", IsCorrect = false } } },
                    new Question { Module = safetyModule, QuestionText = "Где план эвакуации?", AnswerOptions = new List<AnswerOption> { new AnswerOption { AnswerText = "На стене у выхода", IsCorrect = true }, new AnswerOption { AnswerText = "В сейфе", IsCorrect = false } } },
                    new Question { Module = safetyModule, QuestionText = "Можно ли чинить проводку самому?", AnswerOptions = new List<AnswerOption> { new AnswerOption { AnswerText = "Нет, вызвать электрика", IsCorrect = true }, new AnswerOption { AnswerText = "Да, конечно", IsCorrect = false } } },
                    new Question { Module = safetyModule, QuestionText = "Номер скорой помощи?", AnswerOptions = new List<AnswerOption> { new AnswerOption { AnswerText = "103", IsCorrect = true }, new AnswerOption { AnswerText = "911", IsCorrect = false } } },
                    new Question { Module = safetyModule, QuestionText = "Нужна ли сменная обувь?", AnswerOptions = new List<AnswerOption> { new AnswerOption { AnswerText = "Рекомендуется", IsCorrect = true }, new AnswerOption { AnswerText = "Нет", IsCorrect = false } } }
                };

                context.Questions.AddRange(questions);
                context.SaveChanges();
            }

            // --- Achievements ---
            if (!context.Achievements.Any())
            {
                var achievements = new[]
                {
                    new Achievement { Title = "Первые шаги", Description = "Пройти первый модуль онбординга.", IconName = "🎯", ConditionKey = "FIRST_MODULE" },
                    new Achievement { Title = "Идеальный результат", Description = "Сдать тест на 100%.", IconName = "⭐", ConditionKey = "TEST_100" },
                    new Achievement { Title = "Знаток безопасности", Description = "Завершить модуль безопасности.", IconName = "🛡️", ConditionKey = "MODULE_SAFETY" },
                    new Achievement { Title = "Полный курс", Description = "Полностью завершить программу онбординга.", IconName = "🏆", ConditionKey = "ONBOARDING_DONE" },
                    new Achievement { Title = "Кандидат", Description = "Достичь 2 уровня.", IconName = "📈", ConditionKey = "LEVEL_2" },
                    new Achievement { Title = "Ветеран", Description = "Достичь 5 уровня.", IconName = "👑", ConditionKey = "LEVEL_5" },
                };
                context.Achievements.AddRange(achievements);
                context.SaveChanges();
            }

            // --- Checklist Items ---
            if (!context.ChecklistItems.Any())
            {
                var welcomeModule = context.Modules.FirstOrDefault(m => m.Title == "Введение в компанию");
                if (welcomeModule != null)
                {
                    var checklistItems = new[]
                    {
                        new ChecklistItem { ModuleId = welcomeModule.ModuleId, Text = "Получить рабочий пропуск у охраны", OrderIndex = 1, IsRequired = true },
                        new ChecklistItem { ModuleId = welcomeModule.ModuleId, Text = "Настроить корпоративную почту", OrderIndex = 2, IsRequired = true },
                        new ChecklistItem { ModuleId = welcomeModule.ModuleId, Text = "Заполнить профиль в кадровой системе", OrderIndex = 3, IsRequired = true },
                        new ChecklistItem { ModuleId = welcomeModule.ModuleId, Text = "Познакомиться с командой (ланч)", OrderIndex = 4, IsRequired = false }
                    };
                    context.ChecklistItems.AddRange(checklistItems);
                    context.SaveChanges();
                }
            }
            // --- FAQ Entries ---
            if (!context.FaqEntries.Any())
            {
                var faqEntries = new FaqEntry[]
                {
                    new FaqEntry { Question = "Где я могу найти свой график работы?", Answer = "Ваш индивидуальный график работы указан в трудовом договоре и доступен в личном кабинете в разделе 'Профиль'. Стандартный график для офиса: с 9:00 до 18:00.", Category = "Общее", DisplayOrder = 1 },
                    new FaqEntry { Question = "Как оформить отпуск?", Answer = "Отпуск оформляется через портал самообслуживания не менее чем за 2 недели. Сначала согласуйте даты с вашим руководителем.", Category = "HR", DisplayOrder = 2 },
                    new FaqEntry { Question = "Что делать, если сломался ноутбук?", Answer = "Немедленно создайте заявку в Service Desk или напишите в чат технической поддержки в Telegram.", Category = "Техника", DisplayOrder = 3 },
                    new FaqEntry { Question = "Где находится столовая?", Answer = "Столовая расположена на 2-м этаже бизнес-центра. Часы работы: с 12:00 до 16:00.", Category = "Офис", DisplayOrder = 4 },
                    new FaqEntry { Question = "Когда придет зарплата?", Answer = "Зарплата выплачивается дважды в месяц: 25-го числа (аванс) и 10-го числа следующего месяца (основная часть).", Category = "Финансы", DisplayOrder = 5 }
                };
                context.FaqEntries.AddRange(faqEntries);
                context.SaveChanges();
            }
        }
    }
}
