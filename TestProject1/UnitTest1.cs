namespace TestProject1
{
    [TestClass]
    public class UnitTest1
    {
        [TestClass]
        public class AuthorizationPageTests
        {
            [TestMethod]
            public async Task TestAuthorization_Successful()
            {
                // Arrange
                var mainWindow = new MainWindow();
                var page = new AutorizationPage(mainWindow);
                page.loginTextBox.Text = "98887776655";
                page.passwordTextBox.Password = "admin123";

                // Act
                await page.Button_Click(null, null);

                // Assert
                Assert.AreEqual(typeof(HomePage), FrameManager.MainFrame.Content.GetType());
                Assert.AreEqual("Bearer", Properties.Settings.Default.Token.Substring(0, 6));
                Assert.AreEqual("admin", Properties.Settings.Default.Role);
            }

            [TestMethod]
            public async Task TestAuthorization_IncorrectCredentials()
            {
                // Arrange
                var mainWindow = new MainWindow();
                var page = new AutorizationPage(mainWindow);
                page.loginTextBox.Text = "incorrect";
                page.passwordTextBox.Password = "credentials";

                // Act
                await page.Button_Click(null, null);

                // Assert
                Assert.IsTrue(MessageBox.ShowCalls.Count > 0);
                Assert.IsTrue(MessageBox.ShowCalls.Exists(c => c.args[0].ToString().Contains("Неверный телефон или пароль")));
            }

            [TestMethod]
            public async Task TestAuthorization_UnauthorizedRole()
            {
                // Arrange
                var mainWindow = new MainWindow();
                var page = new AutorizationPage(mainWindow);
                page.loginTextBox.Text = "98887776655";
                page.passwordTextBox.Password = "admin123";
                // Simulate non-admin role
                Properties.Settings.Default.Role = "user";

                // Act
                await page.Button_Click(null, null);

                // Assert
                Assert.IsTrue(MessageBox.ShowCalls.Count > 0);
                Assert.IsTrue(MessageBox.ShowCalls.Exists(c => c.args[0].ToString().Contains("Доступ запрещен. У вас нет прав доступа для этого приложения.")));
            }

            [TestMethod]
            public async Task TestAuthorization_EmptyCredentials()
            {
                // Arrange
                var mainWindow = new MainWindow();
                var page = new AutorizationPage(mainWindow);
                page.loginTextBox.Text = "";
                page.passwordTextBox.Password = "";

                // Act
                await page.Button_Click(null, null);

                // Assert
                Assert.IsTrue(MessageBox.ShowCalls.Count > 0);
                Assert.IsTrue(MessageBox.ShowCalls.Exists(c => c.args[0].ToString().Contains("Пожалуйста, заполните все поля.")));
            }
        }
    }
}