-- Проверка данных в таблицах ролей и пользователей
SELECT 'Роли' AS [Проверка], COUNT(*) AS [Количество] FROM Roles
UNION ALL
SELECT 'Пользователи', COUNT(*) FROM Users
UNION ALL
SELECT 'Связи User-Role', COUNT(*) FROM UserRoles;

-- Просмотр всех ролей
SELECT 'РОЛИ:' AS [Раздел];
SELECT RoleID, RoleName FROM Roles;

-- Просмотр пользователей и их ролей
SELECT 'ПОЛЬЗОВАТЕЛИ И ИХ РОЛИ:' AS [Раздел];
SELECT 
    u.UserID,
    u.FullName,
    u.Email,
    CASE 
        WHEN COUNT(r.RoleID) = 0 THEN '❌ БЕЗ РОЛЕЙ'
        ELSE STRING_AGG(r.RoleName, ', ')
    END AS [Роли]
FROM Users u
LEFT JOIN UserRoles ur ON u.UserID = ur.UserID
LEFT JOIN Roles r ON ur.RoleID = r.RoleID
GROUP BY u.UserID, u.FullName, u.Email
ORDER BY u.UserID;

-- Проверка связей в UserRoles
SELECT 'ТАБЛИЦА USERROLES:' AS [Раздел];
SELECT 
    ur.UserID,
    ur.RoleID,
    u.FullName,
    r.RoleName
FROM UserRoles ur
LEFT JOIN Users u ON ur.UserID = u.UserID
LEFT JOIN Roles r ON ur.RoleID = r.RoleID;
