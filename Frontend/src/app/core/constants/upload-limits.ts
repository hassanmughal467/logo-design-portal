/** Matches API RequestSizeLimit and max combined attachment size per request (500 MiB). */
export const MAX_UPLOAD_BYTES = 500 * 1024 * 1024;

export function combinedFileBytes(files: File[]): number {
  return files.reduce((sum, f) => sum + f.size, 0);
}
