﻿using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Selenium_g_y_proj
{
    class Yandex:DBConection,WebSite
    {
        public String url = "http://yandex.ru";
        public ChromeDriver driver;

        public Yandex(String url)
        {
            this.url = url;
            driver = new ChromeDriver();//открываем сам браузер
            
            driver.Manage().Window.Maximize();//открываем браузер на полный экран
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(30); //время ожидания компонента страницы после загрузки страницы
        }

        public void search(String keyword,int keyword_id)
        {
            driver.Navigate().GoToUrl(this.url);//переходим по адресу поисковика

            IWebElement text = driver.FindElement(By.Id("lst-ib"));
            Actions actions = new Actions(driver);
            actions.MoveToElement(text).Click().Perform();  
                
            text.SendKeys(keyword);//вводим искомое словосочетание
            System.Threading.Thread.Sleep(5000);//засыпаем, чтоб на нас не подумали что мы бот
            text.SendKeys(Keys.Enter);//жмем Enter для  отправки поискового запроса
            System.Threading.Thread.Sleep(5000);//засыпаем опять же, чтоб нас не раскрыли:D мы коварны     
                                                
            //5раз обновляем страницу (пока так) чтоб выбрать разные варианты рекламы, в бд тоже нужно сделать структуру, которая бы сохраняла позицию выдачи рекламы в бд
            for (int j = 0; j < 5; j++)
            {
                int searchPos = 0;
                System.Threading.Thread.Sleep(2000);
                driver.Navigate().Refresh();//обновление страницы, чтоб выбрать больше вариантов рекламы
                //выбираем все блоки, которые относятся к рекламе 
                foreach (IWebElement i in driver.FindElements(By.CssSelector(".ads-ad")))
                {
                    ++searchPos;
                    String url = i.FindElement(By.CssSelector("._Jwu")).Text;
                    String description = i.FindElement(By.CssSelector("._WGk")).Text;
                    //разбираем рекламное сообщение
                    Console.WriteLine(url+" "+ description);
                    
                    //формируем новую запись в бд
                    Keyword kw = new Keyword();
                    kw.url = url;
                    kw.keyword_id = keyword_id;
                    kw.description = description;
                    kw.position[j] = (byte)searchPos;
                    kw.browser = Keyword.Browser.GOOGLE;

                    if (isExist(kw))
                        Update(kw);
                    else
                        Insert(kw);

                }

            }
            exit();
        }

        public void exit()
        {
            //закрываем драйвер и закрываем браузер
            driver.Close();
            driver.Quit();
            //Environment.Exit(0);
        }

      
    }
}
