/** First letters of name and surname, uppercased — e.g. "Jin-Woo Sung" → "JS". */
export function getInitials(name: string, surname: string): string {
  const first = name.trim()[0] ?? '';
  const second = surname.trim()[0] ?? '';
  const initials = `${first}${second}`.toUpperCase();
  return initials.length > 0 ? initials : '?';
}
