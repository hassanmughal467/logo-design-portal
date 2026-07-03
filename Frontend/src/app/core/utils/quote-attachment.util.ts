const IMAGE_EXT = /\.(png|jpe?g|gif|webp|bmp|svg)$/i;

export function isPreviewableQuoteAttachment(fileName: string): boolean {
  return IMAGE_EXT.test(fileName || '');
}

export function getPreviewableQuoteAttachments(attachments: string[] | undefined | null): string[] {
  return (attachments ?? []).filter(isPreviewableQuoteAttachment);
}
