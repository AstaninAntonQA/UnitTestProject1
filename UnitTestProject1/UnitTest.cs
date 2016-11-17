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
        public void MainTEST()
        {
            UserData.Initialization();
            ChromeOptions options = new ChromeOptions();
            options.AddArgument("--start-maximized");
            Chrome = new ChromeDriver(options);
            Chrome.Manage().Window.Maximize();
            Chrome.Navigate().GoToUrl("http://extremeshop.ru");
            IJavaScriptExecutor js = Chrome as IJavaScriptExecutor;
            // Start Testing /////////////////////////////////////
            // Login test ////


            try
            {
                LoginTest(UserData.Login, UserData.Pass);
                TestResult.WriteLine("Авторизация...OK");
            } catch(Exception Error)
            {
                ErrorRegistration(Error.Message);
                TestResult.WriteLine("Maybe an Error in Login or Password");               
            }


            // Goods to List ////
            try
            {
                Thread.Sleep(1000);
                Goods_to_Cart();
                TestResult.WriteLine("Товары добавлены в корзину...OK");
            } catch(Exception Error)
            {
                ErrorRegistration(Error.Message);
                TestResult.WriteLine("Goods adding to List...Fail");
            }



            try
            {
                Order();
                TestResult.WriteLine("Оформление заказа завершено...OK");
                UserData.OrderNumber = Chrome.FindElement(By.XPath(Mainpage.OrderNumberPath)).Text;
                int result = ToInt(UserData.OrderNumber);
                TestResult.WriteLine("Номер заказа: " + result);
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



        private void Order()
        {
            IJavaScriptExecutor js = Chrome as IJavaScriptExecutor;
            Thread.Sleep(10000);
            Actions action = new Actions(Chrome);

            //Переходим в корзину
            js.ExecuteScript("window.scrollBy(0,-600)", "");
            Chrome.FindElement(By.Id(Catalog.CartXPath)).Click();
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
            DeliverTimeSelect(1);
            try
            {
                DeliverTimeSelect(2);
                TestResult.WriteLine("Присутствует двойная доставка.");
            }
            catch { }

            //Выбираем способ оплаты
            Chrome.FindElement(By.XPath(Cart.PayTypePath(UserData.paytype))).Click();
            TestResult.WriteLine("Способ оплаты выбран");

            //Вводим промокод
            Chrome.FindElement(By.XPath(Cart.PromocodePath)).Click();
            Thread.Sleep(2000);
            Chrome.FindElement(By.XPath(Cart.PromocodePath)).SendKeys(Keys.Control + "a");
            Thread.Sleep(2000);
            Chrome.FindElement(By.XPath(Cart.PromocodePath)).SendKeys(Keys.Delete);
            Thread.Sleep(2000);
            Chrome.FindElement(By.XPath(Cart.PromocodePath)).SendKeys(UserData.Promo);
            TestResult.WriteLine("Промокод введен");

            //Вводим комментарий к заказу
            Chrome.FindElement(By.XPath(Cart.CommentPath)).SendKeys(UserData.Comment);
            TestResult.WriteLine("Комментарий введен");

            //Клик на кнопку "Оформить" в корзине
            js.ExecuteScript("window.scrollBy(0,600)", "");
            Chrome.FindElement(By.ClassName(Cart.CartOrderPath)).Click();
            Thread.Sleep(3000);

            //Клик на кнопку Оформить на окне "Проверьте данные"
            bool CheckWindowVisible = false;
            while (CheckWindowVisible == false)
            {
                try
                {
                    Chrome.FindElement(By.XPath(Cart.FinalOrderPath)).Click();
                    CheckWindowVisible = true;
                }
                catch
                {
                    CheckWindowVisible = false;
                    Thread.Sleep(1000);
                }
            }
        }



        private void DeliverTimeSelect(int i)
        {
            string DeliverTime;
            IJavaScriptExecutor js = Chrome as IJavaScriptExecutor;
            //Скроллим еще на 200рх
            js.ExecuteScript("window.scrollBy(0,200)", "");
            Thread.Sleep(3000);

            //Клик по первому выпадающему меню с датой доставки
            Chrome.FindElement(By.XPath("//div[@class='step-container']/div[" + i + "]/div/div[2]/div/div/div[2]")).Click();
            Thread.Sleep(3000);

            //выбираем первую дату из списка
            DeliverTime = Chrome.FindElement(By.XPath("//div[@class='step-container']/div[" + i + "]/div/div[2]/div/div[2]/div")).Text;
            Chrome.FindElement(By.XPath("//div[@class='step-container']/div[" + i + "]/div/div[2]/div/div[2]/div")).Click();

            TestResult.WriteLine("Выбрана дата: " + DeliverTime);
            Thread.Sleep(2000);

            //Клик по полю со временем доставки
            Chrome.FindElement(By.XPath("//div[@class='step-container']/div[" + i + "]/div/div[3]/div")).Click();
            Thread.Sleep(2000);

            //выбираем первое время из списка
            DeliverTime = Chrome.FindElement(By.XPath("//div[@class='step-container']/div[" + i + "]/div/div[3]/div/div[2]/div")).Text;
            Chrome.FindElement(By.XPath("//div[@class='step-container']/div[" + i + "]/div/div[3]/div/div[2]/div")).Click();

            TestResult.WriteLine("Выбрано время: " + DeliverTime);
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



        private void Goods_to_Cart()
        {
            if (UserData.random == 1)
            {
                TestResult.WriteLine("Выбран рандомный метод");
                RandomMethod();
            }
            else
            {
                TestResult.WriteLine("Выбран ручной метод");
                NormalMethod();
            }
        }



        private void NormalMethod()
        {
            TestResult.WriteLine("Кол-во категорий: " + UserData.Mcat.Length);
            Actions action = new Actions(Chrome);
            IJavaScriptExecutor js = Chrome as IJavaScriptExecutor;
            int G;
            int Category;
            int GoodsinLine = 0;
            for (int cat = 0; cat < UserData.Mcat.Length; cat++)
            {
                G = 1;
                Category = Convert.ToInt16(UserData.Mcat[cat]);
                Category = Category - 48;
                TestResult.WriteLine(Category);
                TestResult.WriteLine("Категория: " + UserData.Mcat[cat] + " - " + Mainpage.Categories(Category));
                for (int i = 0; i < 10; i++)
                {
                    js.ExecuteScript("window.scrollBy(0,-200)", "");
                }
                action.MoveToElement(Chrome.FindElement(By.LinkText(Mainpage.Categories(Category)))).Perform();
                Chrome.FindElement(By.LinkText(Mainpage.Categories(Category))).Click();
                for (int i = 0; i < 10; i++) //прокрутка страницы для подгрузки товара
                {
                    js.ExecuteScript("window.scrollBy(0,200)", ""); //Скролл вниз
                    Thread.Sleep(500); //постепенно, чтобы товар успел грузануться
                }
                for (int i = 0; i < 10; i++)
                {
                    js.ExecuteScript("window.scrollBy(0,-200)", "");
                }
                try
                {
                    //Добавление найденных товаров в список
                    while (G <= UserData.GoodsCount) 
                    {
                        Goods.Add(Chrome.FindElement(By.XPath(Catalog.GoodFromCatalog(G))));
                        TestResult.WriteLine("Товар добавлен " + G);
                        G++;
                    }
                }
                catch
                {
                    TestResult.WriteLine("Товаров в категории меньше планируемого");
                }
                TestResult.WriteLine("Добавлено товаров: " + (G-1));
                for (int i = 0; i < G; i++) //Кликаем у каждого товара на кнопку "Купить"
                {
                    if (GoodsinLine == 7) //после каждого ряда товаров скролим страницу на 200 пикселей
                    {
                        js.ExecuteScript("window.scrollBy(0,200)", "");
                        Thread.Sleep(300);
                        GoodsinLine = 0;
                    }
                    try
                    {
                        Goods[i].Click();
                        GoodsinLine++;
                        TestResult.WriteLine("Клик по товару " + (i + 1) + " получен");
                    }
                    catch
                    {
                        i = G;
                    }

                    Thread.Sleep(500);
                }
                Goods.Clear();
                Thread.Sleep(1000);
            }
        }



        private void RandomMethod()
        {
            int RNDcategories;
            int RNDcatCount;
            int RNDgoodsCount;

            Random rnd = new Random();
            RNDcatCount = rnd.Next(UserData.CatCount);
            RNDcatCount++;
            TestResult.WriteLine("Рандомно выбрано кол-во категорий: " + RNDcatCount);
            Actions action = new Actions(Chrome);
            IJavaScriptExecutor js = Chrome as IJavaScriptExecutor;
            for (int k = 0; k < RNDcatCount; k++)
            {
                int i;
                RNDcategories = rnd.Next(8);
                RNDgoodsCount = rnd.Next(UserData.GoodsCount);
                RNDgoodsCount++;
                TestResult.WriteLine("Рандомно выбрана категория: " + (RNDcategories+1)+ " - " + Mainpage.Categories(RNDcategories));
                TestResult.WriteLine("Рандомно выбрано кол-во товара с категории: " + RNDgoodsCount);
                for (i = 0; i < 10; i++)
                {
                    js.ExecuteScript("window.scrollBy(0,-200)", "");
                }
                Chrome.FindElement(By.LinkText(Mainpage.Categories(RNDcategories))).Click();

                for (i = 0; i < 10; i++) //прокрутка страницы для подгрузки товара
                {
                    js.ExecuteScript("window.scrollBy(0,200)", ""); //Скролл вниз
                    Thread.Sleep(500); //постепенно, чтобы товар успел грузануться
                }
                for (i = 0; i < 10; i++)
                {
                    js.ExecuteScript("window.scrollBy(0,-200)", "");
                }
                int G = 1;
                try
                {
                    while (G <= RNDgoodsCount) //Добавление найденных товаров в список
                    {
                        Goods.Add(Chrome.FindElement(By.XPath(Catalog.GoodFromCatalog(G))));
                        TestResult.WriteLine("Товар добавлен " + G);
                        G++;
                    }
                }
                catch
                {
                    TestResult.WriteLine("Товаров в категории меньше планируемого");
                    TestResult.WriteLine("Добавлено товаров: " + G);
                }
                int GoodsinLine = 0;
                for (i = 0; i < G; i++) //Кликаем у каждого товара на кнопку "Купить"
                {
                    if (GoodsinLine == 7) //после каждого ряда товаров скролим страницу на 200 пикселей
                    {
                        js.ExecuteScript("window.scrollBy(0,200)", "");
                        Thread.Sleep(300);
                        GoodsinLine = 0;
                    }
                    try
                    {
                        Goods[i].Click();
                        GoodsinLine++;
                        TestResult.WriteLine("Клик по товару " + (i+1) + " получен");
                    }
                    catch
                    {
                        i = G;
                    }

                    Thread.Sleep(500);
                }
                Goods.Clear();
            }
        }
    }
}