/** Human-readable job title from API string or JobTitleDto object. */
export function formatJobTitle(jobTitle) {
  if (!jobTitle) return null
  if (typeof jobTitle === 'string') return jobTitle
  return jobTitle.title || null
}
