using Bl;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace Tests.BL;

public class EmailServiceBLTests
{
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly Mock<IConfigurationSection> _smtpSettingsMock;
    private readonly Mock<IConfigurationSection> _appSettingsMock;
    private readonly Mock<ILogger<EmailServiceBL>> _loggerMock;
    private readonly EmailServiceBL _emailServiceBL;

    public EmailServiceBLTests()
    {
        _configurationMock = new Mock<IConfiguration>();
        _smtpSettingsMock = new Mock<IConfigurationSection>();
        _appSettingsMock = new Mock<IConfigurationSection>();
        _loggerMock = new Mock<ILogger<EmailServiceBL>>();

        _smtpSettingsMock.Setup(s => s["SmtpServer"]).Returns("smtp.example.com");
        _smtpSettingsMock.Setup(s => s["SmtpPort"]).Returns("587");
        _smtpSettingsMock.Setup(s => s["SmtpUser"]).Returns("user@example.com");
        _smtpSettingsMock.Setup(s => s["SmtpPass"]).Returns("password");
        _smtpSettingsMock.Setup(s => s["FromEmail"]).Returns("noreply@example.com");
        _smtpSettingsMock.Setup(s => s["FromName"]).Returns("Test App");

        _appSettingsMock.Setup(s => s["AppName"]).Returns("SmartEstimate");

        _configurationMock.Setup(c => c.GetSection("SmtpSettings")).Returns(_smtpSettingsMock.Object);
        _configurationMock.Setup(c => c.GetSection("AppSettings")).Returns(_appSettingsMock.Object);

        _emailServiceBL = new EmailServiceBL(_configurationMock.Object, _loggerMock.Object);
    }

    [Theory]
    [InlineData("<p>Test HTML</p>", "Test HTML")]
    [InlineData("<h1>Header</h1><p>Paragraph</p>", "Header Paragraph")]
    [InlineData("<br>Line break", "Line break")]
    public void HtmlToPlainText_ConvertsProperly(string html, string expectedText)
    {
        var result = typeof(EmailServiceBL)
            .GetMethod("HtmlToPlainText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .Invoke(_emailServiceBL, new object[] { html }) as string;

        Assert.Equal(expectedText, result);
    }

    [Fact]
    public void Constructor_MissingSmtpServer_ThrowsArgumentNullException()
    {
        var configMock = new Mock<IConfiguration>();
        var smtpSection = new Mock<IConfigurationSection>();
        smtpSection.Setup(s => s["SmtpServer"]).Returns((string)null);
        configMock.Setup(c => c.GetSection("SmtpSettings")).Returns(smtpSection.Object);

        var ex = Assert.Throws<ArgumentNullException>(() =>
            new EmailServiceBL(configMock.Object, _loggerMock.Object));

        Assert.Contains("SmtpServer configuration is missing", ex.Message);
    }

    [Fact]
    public void Constructor_MissingSmtpUser_ThrowsArgumentNullException()
    {
        var configMock = new Mock<IConfiguration>();
        var smtpSection = new Mock<IConfigurationSection>();
        smtpSection.Setup(s => s["SmtpServer"]).Returns("smtp.example.com");
        smtpSection.Setup(s => s["SmtpUser"]).Returns((string)null);
        configMock.Setup(c => c.GetSection("SmtpSettings")).Returns(smtpSection.Object);

        var ex = Assert.Throws<ArgumentNullException>(() =>
            new EmailServiceBL(configMock.Object, _loggerMock.Object));

        Assert.Contains("SmtpUser configuration is missing", ex.Message);
    }

    [Fact]
    public void Constructor_MissingSmtpPass_ThrowsArgumentNullException()
    {
        var configMock = new Mock<IConfiguration>();
        var smtpSection = new Mock<IConfigurationSection>();
        smtpSection.Setup(s => s["SmtpServer"]).Returns("smtp.example.com");
        smtpSection.Setup(s => s["SmtpUser"]).Returns("user@example.com");
        smtpSection.Setup(s => s["SmtpPass"]).Returns((string)null);
        configMock.Setup(c => c.GetSection("SmtpSettings")).Returns(smtpSection.Object);

        var ex = Assert.Throws<ArgumentNullException>(() =>
            new EmailServiceBL(configMock.Object, _loggerMock.Object));

        Assert.Contains("SmtpPass configuration is missing", ex.Message);
    }

    [Fact]
    public void Constructor_MissingFromEmail_ThrowsArgumentNullException()
    {
        var configMock = new Mock<IConfiguration>();
        var smtpSection = new Mock<IConfigurationSection>();
        smtpSection.Setup(s => s["SmtpServer"]).Returns("smtp.example.com");
        smtpSection.Setup(s => s["SmtpUser"]).Returns("user@example.com");
        smtpSection.Setup(s => s["SmtpPass"]).Returns("password");
        smtpSection.Setup(s => s["FromEmail"]).Returns((string)null);
        configMock.Setup(c => c.GetSection("SmtpSettings")).Returns(smtpSection.Object);

        var ex = Assert.Throws<ArgumentNullException>(() =>
            new EmailServiceBL(configMock.Object, _loggerMock.Object));

        Assert.Contains("FromEmail configuration is missing", ex.Message);
    }

    [Fact]
    public void Constructor_NullLogger_ThrowsArgumentNullException()
    {
        var ex = Assert.Throws<ArgumentNullException>(() =>
            new EmailServiceBL(_configurationMock.Object, null));

        Assert.Equal("logger", ex.ParamName);
    }
}