/** Minimal valid PNG header for upload security tests. */
export const PNG_HEADER = Buffer.from([
  0x89, 0x50, 0x4e, 0x47, 0x0d, 0x0a, 0x1a, 0x0a,
]);

export const PDF_MAGIC = Buffer.from([0x25, 0x50, 0x44, 0x46]);

export const EXE_MAGIC = Buffer.from([0x4d, 0x5a]);

/** Oversized payload (~6 MB) for limit tests — adjust if server max differs. */
export function oversizedBuffer(sizeMb = 6): Buffer {
  return Buffer.alloc(sizeMb * 1024 * 1024, 0);
}
