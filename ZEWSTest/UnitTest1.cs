using ZEWS.Xaml;
using ZEWS;


namespace ZEWSTest
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public async Task TestSuccessfulAuthentication()
        {
            // Arrange
            var mainWindow = new MainWindow();
            var page = new AutorizationPage(mainWindow);
            page.loginTextBox = "valid_phone_number";
            page.passwordTextBox = "valid_password";

            // Act
            await page.Button_Click(null, null);

            // Assert
            Assert.IsInstanceOfType(FrameManager.MainFrame.Content, typeof(HomePage));
        }

    }
}