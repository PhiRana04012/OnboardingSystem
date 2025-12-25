using OnboardingSystem.Entities;

namespace OnboardingSystem.Data
{
    public static class DbInitializer
    {
        public static void Initialize(AppDbContext context)
        {
            context.Database.EnsureCreated();

            // Look for any roles.
            if (context.Roles.Any())
            {
                return;   // DB has been seeded
            }

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
                // Проверка, чтобы не дублировать, если вдруг запускаем повторно
                if (!context.Roles.Any(existing => existing.RoleName == r.RoleName))
                {
                    context.Roles.Add(r);
                }
            }
            context.SaveChanges();

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

                var users = new List<User>
                {
                    new User
                    {
                        FullName = "Системный Администратор",
                        Email = "admin@example.com",
                        HireDate = DateOnly.FromDateTime(DateTime.Now.AddYears(-1)),
                        OnboardingStatus = "Завершен", // Локализация
                        JobTitle = "Старший Администратор",
                        Department = itDept
                    },
                    new User
                    {
                        FullName = "Мария Ивановна (HR)",
                        Email = "hr@example.com",
                        HireDate = DateOnly.FromDateTime(DateTime.Now.AddMonths(-6)),
                        OnboardingStatus = "Завершен",
                        JobTitle = "HR Менеджер",
                        Department = hrDept
                    },
                    new User
                    {
                        FullName = "Иван Новичков",
                        Email = "new@example.com",
                        HireDate = DateOnly.FromDateTime(DateTime.Now),
                        OnboardingStatus = "В процессе",
                        JobTitle = "Младший Разработчик",
                        Department = itDept
                    }
                };

                context.Users.AddRange(users);
                context.SaveChanges();

                // Присвоение ролей
                users[0].Roles.Add(adminRole);
                users[1].Roles.Add(hrRole);
                users[2].Roles.Add(empRole);
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
        }
    }
}
