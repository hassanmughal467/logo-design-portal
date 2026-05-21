using LogoDesignPortal.Application.Helpers;

using Xunit;



namespace LogoDesignPortal.Application.Tests.Helpers;



public class UploadSecurityHelperTests

{

    [Theory]

    [InlineData("../../../etc/passwd", "passwd")]

    [InlineData(@"C:\temp\logo.png", "logo.png")]

    [InlineData("safe-name.jpg", "safe-name.jpg")]

    public void SanitizeOriginalFileName_StripsPathComponents(string input, string expectedBase)

    {

        var result = UploadSecurityHelper.SanitizeOriginalFileName(input, ".png");

        Assert.EndsWith(expectedBase.Contains('.') ? expectedBase : expectedBase + ".png", result, StringComparison.OrdinalIgnoreCase);

        Assert.DoesNotContain("..", result);

        Assert.DoesNotContain("\\", result);

    }



    [Fact]

    public void ValidateDeclaredContentType_RejectsMismatchedMime()

    {

        Assert.Throws<InvalidOperationException>(() =>

            UploadSecurityHelper.ValidateDeclaredContentType(".png", "application/pdf"));

    }



    [Fact]

    public void ValidateUploadFileName_RejectsPathTraversal()

    {

        Assert.Throws<InvalidOperationException>(() =>

            UploadSecurityHelper.ValidateUploadFileName("../../secret.png"));

    }



    [Fact]

    public void ValidateUploadFileName_RejectsBlockedExtension()

    {

        Assert.Throws<InvalidOperationException>(() =>

            UploadSecurityHelper.ValidateUploadFileName("payload.exe"));

    }



    [Fact]

    public void ValidateUploadFileName_RejectsDoubleExtension()

    {

        Assert.Throws<InvalidOperationException>(() =>

            UploadSecurityHelper.ValidateUploadFileName("logo.png.exe"));

    }



    [Fact]

    public void HasDoubleExtension_DetectsDecoyPattern_NotDottedNames()
    {
        Assert.True(UploadSecurityHelper.HasDoubleExtension("logo.png.exe"));
        Assert.False(UploadSecurityHelper.HasDoubleExtension("my.company.logo.png"));
        Assert.False(UploadSecurityHelper.HasDoubleExtension("logo.png"));
    }



    [Fact]

    public void ValidateMagicBytes_RejectsPdfContentWithPngExtension()

    {

        var pdfHeader = new byte[] { 0x25, 0x50, 0x44, 0x46, 0x2D, 0x31, 0x2E, 0x34 };

        using var stream = new MemoryStream(pdfHeader);

        Assert.Throws<InvalidOperationException>(() =>

            UploadSecurityHelper.ValidateMagicBytes(".png", stream));

    }

}


