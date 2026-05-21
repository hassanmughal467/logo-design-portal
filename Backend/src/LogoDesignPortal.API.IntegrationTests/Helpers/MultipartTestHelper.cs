using System.Net.Http.Headers;

namespace LogoDesignPortal.API.IntegrationTests.Helpers;

public static class MultipartTestHelper
{
    private static readonly byte[] PngHeader =
        [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];

    public static ByteArrayContent CreateFileContent(byte[] bytes, string contentType) =>
        new(bytes) { Headers = { ContentType = new MediaTypeHeaderValue(contentType) } };

    public static MultipartFormDataContent CreateSingleFileUpload(
        Guid orderId,
        string fileName = "reference.png",
        string fileType = "Reference",
        byte[]? content = null)
    {
        var form = new MultipartFormDataContent();
        var fileBytes = content ?? PngHeader;
        var file = CreateFileContent(fileBytes, "image/png");
        form.Add(file, "file", fileName);
        form.Add(new StringContent(fileType), "fileType");
        return form;
    }

    public static Task<HttpResponseMessage> PostFileUploadAsync(
        HttpClient client,
        Guid orderId,
        string fileName,
        byte[] content,
        string contentType,
        string fileType = "Reference") =>
        client.PostAsync($"/api/files/upload/{orderId}", BuildUploadForm(fileName, content, contentType, fileType));

    private static MultipartFormDataContent BuildUploadForm(
        string fileName,
        byte[] content,
        string contentType,
        string fileType)
    {
        var form = new MultipartFormDataContent();
        form.Add(CreateFileContent(content, contentType), "file", fileName);
        form.Add(new StringContent(fileType), "fileType");
        return form;
    }

    public static MultipartFormDataContent CreateRevisionRequestForm(
        string instructions = "Please adjust the logo colors and spacing.",
        bool includePng = false)
    {
        var form = new MultipartFormDataContent();
        form.Add(new StringContent(instructions), "instructions");
        if (includePng)
        {
            form.Add(CreateFileContent(PngHeader, "image/png"), "files", "revision-ref.png");
        }
        return form;
    }
}
