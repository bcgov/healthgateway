using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using Xunit;
using System;
using System.IO;
using System.Reflection;

namespace HealthGateway.WebClient.Test.Functional
{
  public class Counter_Test : IDisposable
  {
    private IWebDriver driver;
    private string baseUrl;

    public Counter_Test()
    {
        baseUrl = "http://localhost:5000/";
        driver = new ChromeDriver(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
    }

    [Fact]
    public async void Should_Increment_Counter()
    {
      driver.Navigate().GoToUrl(baseUrl);
      WebDriverWait wait = new WebDriverWait(driver, System.TimeSpan.FromSeconds(15));
      wait.Until(driver => driver.FindElement(By.XPath("//a[@href='/counter']")));

      IWebElement menuItem = driver.FindElement(By.XPath("//a[@href='/counter']"));
      menuItem.Click();

      wait.Until(driver => driver.FindElement(By.Id("btnIncrement")));

      IWebElement incrementButton = driver.FindElement(By.Id("btnIncrement"));
      incrementButton.Click();

      IWebElement currentCount = driver.FindElement(By.Id("currentCount"));
      Assert.Equal("1", currentCount.Text);

      incrementButton.Click();
      Assert.Equal("2", currentCount.Text);
    }

    public void Dispose()
    {
        driver.Close();
    }

  }
}
