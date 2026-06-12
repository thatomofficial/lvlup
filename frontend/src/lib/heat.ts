/**
 * Maps a day's completion count onto a heatmap intensity level (0-3),
 * mirroring GitHub-style contribution shading.
 */
export function heatLevel(completions: number): 0 | 1 | 2 | 3 {
  if (completions <= 0) {
    return 0;
  }
  if (completions === 1) {
    return 1;
  }
  if (completions <= 3) {
    return 2;
  }
  return 3;
}
