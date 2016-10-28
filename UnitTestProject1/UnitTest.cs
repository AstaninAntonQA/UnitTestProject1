using System;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using OpenQA.Selenium.Interactions;
using System.IO;
using XPath;
using Initialization;

namespace maintest
{
    [TestClass]
    public class UnitTest
    {
        ChromeDriver Chrome;
        public StreamWriter TestResult = new StreamWriter(@"D://TestResult.txt");
        mainpage Mainpage = new mainpage();
        catalog Catalog = new catalog();
        cart Cart = new cart();
        UserData UserData = new UserData();
        List<IWebElement> Goods = new List<IWebElement>();

        [TestMethod]
        public void Main()
        {
            UserData.Initialization();
            ChromeOptions options = new ChromeOptions();
            options.AddArgument("--start-maximized");
            Chrome = new ChromeDriver(options);
            Chrome.Navigate().GoToUrl("http://extremeshop.ru");
            IJavaScriptExecutor js = Chrome as IJavaScriptExecutor;
            // Start Testing /////////////////////////////////////
            // Login test ////
            try
            {
                LoginTest(UserData.Login, UserData.Pass);
                TestResult.WriteLine("Login...OK");
            } catch(Exception Error)
            {
                ErrorRegistration(Error.Message);
                TestResult.WriteLine("Maybe an Error in Login or Password");
                
            }
            Chrome.FindElement(By.LinkText(Mainpage.Categories(UserData.category))).Click();
            Thread.Sleep(500);
            // Goods to List ////
            try
            {
                GoodsList(UserData.GoodsCount);
                TestResult.WriteLine("Goods added to List...OK");
            } catch(Exception Error)
            {
                ErrorRegistration(Error.Message);
                TestResult.WriteLine("Goods adding to List...Fail");
                
                
            }
            
            js.ExecuteScript("window.scrollBy(0,200)", "");
            // Add to Cart ////
            try
            {
                AddToCart();
                TestResult.WriteLine("Goods added to Cart...OK");
            } catch(Exception Error)
            {
                ErrorRegistration(Error.Message);
                TestResult.WriteLine("Goods adding to List...Fail");
                
                
            }
            // comlete offer
            try
            {
                Order();
                TestResult.WriteLine("Order complete...OK");
                UserData.OrderNumber = Chrome.FindElement(By.XPath(Mainpage.OrderNumberPath)).Text;
                int result = ToInt(UserData.OrderNumber);
                TestResult.WriteLine("Order number: " + result);

            } catch(Exception Error)
            {
                ErrorRegistration(Error.Message);
                TestResult.WriteLine("Order complete...Fail");
                
                
            }
            TestResult.Close();
        }

        private void ErrorRegistration(string ErrorMessage)
        {
            TestResult.WriteLine(ErrorMessage);
        }

        private void LoginTest(string Login, string Pass)
        {
            Chrome.FindElement(By.LinkText("Вход")).Click();
            Thread.Sleep(2000);
            Chrome.FindElement(By.Id("phone")).SendKeys(Login);
            Chrome.FindElement(By.Id("loginform-password")).SendKeys(Pass);
            Chrome.FindElement(By.Id("loginform-password")).SendKeys(Keys.Enter);
            Thread.Sleep(3000);
            Chrome.FindElement(By.LinkText("Выйти"));
        }
        private void GoodsList(int GoodsCount)
        {
            int i;
            IJavaScriptExecutor js = Chrome as IJavaScriptExecutor;
            for (i = 0; i < 10; i++) //прокрутка страницы для подгрузки товара
            {
                js.ExecuteScript("window.scrollBy(0,200)", ""); //Скролл вниз
                Thread.Sleep(500); //постепенно, чтобы товар успел грузануться
            }
            for (i = 0; i < 10; i++)
            {
                js.ExecuteScript("window.scrollBy(0,-200)", "");
            }
            i = 1;

            while (i <= GoodsCount) //Добавление найденных товаров в список
            {
                Goods.Add(Chrome.FindElement(By.XPath(Catalog.GoodFromCatalog(i))));
                i++;
            }
        }
        private void AddToCart()
        {
            TestResult.WriteLine("GoodsCount: " + UserData.GoodsCount);
            int i;
            int GoodsinLine = 0;
            IJavaScriptExecutor js = Chrome as IJavaScriptExecutor;
            for (i = 0; i < UserData.GoodsCount; i++) //Кликаем у каждого товара на кнопку "Купить"
            {
                if (GoodsinLine == 7) //после каждого ряда товаров скролим страницу на 200 пикселей
                {
                    js.ExecuteScript("window.scrollBy(0,200)", "");
                    Thread.Sleep(300);
                    GoodsinLine = 0;
                }
                Goods[i].Click();
                GoodsinLine++;
                Thread.Sleep(500);
            }
        }
        private void Order()
        {
            IJavaScriptExecutor js = Chrome as IJavaScriptExecutor;
            Thread.Sleep(10000);
            Actions action = new Actions(Chrome);

            //Скроллим страницу вверх до корзины
            action.MoveToElement(Chrome.FindElement(By.XPath(Catalog.CartXPath))).Perform();

            //Переходим в корзину
            Chrome.FindElement(By.XPath(Catalog.CartXPath)).Click();
            Chrome.Manage().Timeouts().ImplicitlyWait(TimeSpan.FromSeconds(2));

            //Скроллим корзину до поля адреса и смещаемся еще на 200 px чтобы не мешала кнопка Мастера
            action.MoveToElement(Chrome.FindElement(By.XPath(Cart.DeliverAdressPath(UserData.adress)))).Perform();
            js.ExecuteScript("window.scrollBy(0,200)", "");

            //Кликаем по выбранному адресу
            Chrome.FindElement(By.XPath(Cart.DeliverAdressPath(UserData.adress))).Click();
            Thread.Sleep(1500);

            //Проверяем на всплывающее окно о стоимости доставки и если оно есть - закрываем его
            try
            {
                Chrome.FindElement(By.XPath("//div[@class='modal-header']/button")).Click();
            }
            catch { }
            Thread.Sleep(1500);

            //Выбираем время доставки
            DeliverTimeSelect();

            //Выбираем способ оплаты
            Chrome.FindElement(By.XPath(Cart.PayTypePath(UserData.paytype))).Click();

            //Вводим промокод
            Chrome.FindElement(By.XPath(Cart.PromocodePath)).Click();
            Thread.Sleep(2000);
            Chrome.FindElement(By.XPath(Cart.PromocodePath)).SendKeys(Keys.Control + "a");
            Thread.Sleep(2000);
            Chrome.FindElement(By.XPath(Cart.PromocodePath)).SendKeys(Keys.Delete);
            Thread.Sleep(2000);
            Chrome.FindElement(By.XPath(Cart.PromocodePath)).SendKeys(UserData.Promo);

            //Вводим комментарий к заказу
            Chrome.FindElement(By.XPath(Cart.CommentPath)).SendKeys(UserData.Comment);

            //Клик на кнопку "Оформить" в корзине
            Chrome.FindElement(By.XPath(Cart.CartOrderPath)).Click();
            Thread.Sleep(3000);

            //Клик на кнопку Оформить на окне "Проверьте данные"
            Chrome.FindElement(By.XPath(Cart.FinalOrderPath)).Click();
            Thread.Sleep(12000);
        }

        private void DeliverTimeSelect()
        {
            IJavaScriptExecutor js = Chrome as IJavaScriptExecutor;
            //Скроллим еще на 200рх
            js.ExecuteScript("window.scrollBy(0,200)", "");
            Thread.Sleep(3000);

            //Клик по первому выпадающему меню с датой доставки
            Chrome.FindElement(By.XPath("//div[@class='time-item']/div[2]/div/div/div[2]")).Click();
            Thread.Sleep(3000);

            //выбираем первую дату из списка
            Chrome.FindElement(By.XPath("//div[@class='time-item']/div[2]/div/div[2]/div")).Click();
            Thread.Sleep(2000);

            //Клик по полю со временем доставки
            Chrome.FindElement(By.XPath("//div[@class='time-item']/div[3]/div")).Click();
            Thread.Sleep(2000);

            //выбираем первое время из списка
            Chrome.FindElement(By.XPath("//div[@class='time-item']/div[3]/div/div[2]/div[1]")).Click();
            Thread.Sleep(2000);
        }

        private int ToInt(string str)
        {
            int i;
            string final_number_str = "";
            for(i=0; i<str.Length; i++)
            {
                if (str[i]>= '0' && str[i] <= '9')
                {
                    final_number_str = final_number_str + str[i];
                }
            }
            UserData.final_number = Convert.ToInt32(final_number_str);
            return UserData.final_number;
        }
    }
}