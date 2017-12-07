﻿using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using System;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;

namespace Selenium_g_y_proj
{
    class Yandex:Utils,WebSite
    {
        public const int MAX_REFRESHES = 1;//количество обновлений страницы для выборки
        public String url = "http://yandex.ru";
        public ChromeDriver driver;
        public int mode = 0;

        public Yandex(String url,int mode = 0) : base()
        {
            this.mode = mode;
            this.url = url;

            var options = new ChromeOptions();
            options.AddArgument("no-sandbox");

            driver = new ChromeDriver(options);//открываем сам браузер

            driver.LocationContext.PhysicalLocation = new OpenQA.Selenium.Html5.Location(55.751244, 37.618423,152);

            driver.Manage().Window.Maximize();//открываем браузер на полный экран
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(180); //время ожидания компонента страницы после загрузки страницы
            if (this.mode == 1)
            {
                driver.Navigate().GoToUrl(this.url);//переходим по адресу поисковика
            }
        }

        public void open_settings()
        {
            IWebElement element = null;
            Actions actions = null;
            
            driver.Navigate().GoToUrl("https://yandex.ru/tune/geo/?retpath=https%3A%2F%2Fwww.yandex.ru%2F%3Fdomredir%3D1&nosync=1");
            element = driver.FindElement(By.Id("city__front-input"));
            actions = new Actions(driver);
            actions.MoveToElement(element).Click().Perform();
            element.Clear();
            element.SendKeys("Москва");
            System.Threading.Thread.Sleep(2000);
            element.SendKeys(Keys.Down);
            System.Threading.Thread.Sleep(2000);
            element.SendKeys(Keys.Enter);
            System.Threading.Thread.Sleep(2000);
            element = driver.FindElement(By.CssSelector(".form__save"));
            actions.MoveToElement(element).Click().Perform();

        }

        public void search(String keyword,int keyword_id)
        {

            VerifyPageIsLoaded(driver);

            IWebElement text = null;
            Actions actions = null;

            if (driver.Url.IndexOf("search") == -1)
            {
                driver.Navigate().GoToUrl(this.url);//переходим по адресу поисковика   
                text = driver.FindElement(By.Id("text"));
                actions = new Actions(driver);
                actions.MoveToElement(text).Click().Perform();

                text.SendKeys(keyword);//вводим искомое словосочетание
                System.Threading.Thread.Sleep(5000);//засыпаем, чтоб на нас не подумали что мы бот
                text.SendKeys(Keys.Enter);//жмем Enter для  отправки поискового запроса
                System.Threading.Thread.Sleep(5000);//засыпаем опять же, чтоб нас не раскрыли:D мы коварны     
            }
            //5раз обновляем страницу (пока так) чтоб выбрать разные варианты рекламы, в бд тоже нужно сделать структуру, которая бы сохраняла позицию выдачи рекламы в бд
            for (int j = 0; j < MAX_REFRESHES; j++)
            {
                int searchPos = 0;
                driver.Navigate().GoToUrl(this.url + "/search/?text=" + keyword);//переходим по адресу поисковика
                System.Threading.Thread.Sleep(1000);//засыпаем, чтоб на нас не подумали что мы бот

                if (isSelectorExist(By.CssSelector(".serp-adv-item")))
                {
                    //если реклама найдена           
                    //выбираем все блоки, которые относятся к рекламе 
                    foreach (IWebElement i in driver.FindElements(By.CssSelector(".serp-adv-item")))
                    {
                        ++searchPos;
                        String url = "";
                        String description = "";
                        try
                        {
                            url = i.FindElement(By.CssSelector(".link_outer_yes")).Text;
                            description = i.FindElement(By.CssSelector(".organic__title-wrapper")).Text;
                        }
                        catch (Exception e) { }

                        if (url.Trim() != "" || description.Trim() != "")
                        {
                            //разбираем рекламное сообщение
                            Console.WriteLine(url + "|" + description);

                            //формируем новую запись в бд
                            Keyword kw = new Keyword(MAX_REFRESHES);
                            kw.url = url;
                            kw.keyword_id = keyword_id;
                            kw.description = description;
                            kw.position[j] = (byte)searchPos;
                            kw.browser = (byte)Keyword.Browser.YANDEX;

                            if (isExist(kw))
                                Update(kw);
                            else
                                Insert(kw);
                        }
                    }
                }
            }
          
        }

        public void exit()
        {
            //закрываем драйвер и закрываем браузер
            driver.Close();
            driver.Quit();
        }

        public bool isSelectorExist(By selector)
        {
            return driver.FindElements(selector).Count == 0 ? false : true;
        }

    }
}
