/** Список пользователей: админ/HR — все; начальник отдела — только свой отдел. */
export function filterUsersByAccess(users, authStore) {
  if (!users?.length) return users ?? []
  if (authStore.isDepartmentHead && !authStore.isFullAdmin && authStore.currentUser?.departmentId) {
    return users.filter(u => u.departmentId === authStore.currentUser.departmentId)
  }
  return users
}
