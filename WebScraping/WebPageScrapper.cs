using HtmlAgilityPack;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;
using OpenQA.Selenium.DevTools;

namespace WebScraping
{
    public class WebPageScrapper
    {
        public string url1 { get; set; }
        public string url2 { get; set; }
        public string connectionString { get; set; }

        public string databaseName { get; set; }
        public string collectionName1 { get; set; }

        public string collectionName2 { get; set; }

        public IMongoCollection<SubscriptionBased> SubscriptionBasedCost { get; }

        public IMongoCollection<FlexTokenBased> FlexTokenBasedCost { get; }





        public WebPageScrapper() 
        {
            url1 = "https://www.autodesk.com/products?page=10";
            url2 = "https://www.autodesk.com/buying/flex/flex-rate-sheet";

            //connectionString = "mongodb://spark:spark@db-mongo-reportingsolution.openlm.com:27017/?authMechanism=DEFAULT"; // Replace with your MongoDB server details
            connectionString = "mongodb://127.0.0.1:27017";
            databaseName = "test"; // Replace with your database name
            collectionName1 = "SubscriptionBasedCost";
            collectionName2 = "FlexTokenBasedCost";

            var client = new MongoClient(connectionString);
            var database = client.GetDatabase(databaseName);
            SubscriptionBasedCost = database.GetCollection<SubscriptionBased>(collectionName1);
            FlexTokenBasedCost = database.GetCollection<FlexTokenBased>(collectionName2);

        }


        public void ShowResultsForSubscription()
        {
            using (IWebDriver driver = new ChromeDriver())
            {
                

                    // Navigate to the page
                    //driver.Navigate().GoToUrl("URL_OF_THE_PAGE");
                    driver.Navigate().GoToUrl(url1);

                    // Wait for dynamic content to load (you might need to adjust the waiting time)
                    System.Threading.Thread.Sleep(5000);

                    
                    IReadOnlyCollection<IWebElement> divElement = driver.FindElements(By.CssSelector("div.price:not(:has(div.wp-price-replaced)):not(:has(div.wp-gateway-after-price))"));
                    
                    foreach (IWebElement element in divElement)
                    {
                        if (!String.IsNullOrEmpty(element.Text))
                        {
                            var parentElement = element.FindElement(By.XPath(".."));

                            var image = parentElement.FindElement(By.CssSelector("a.wp-product-logo-link>div.wp-product-img>img"));

                        SubscriptionBasedCost.InsertOne(new SubscriptionBased()
                        {
                            ProductName = image.GetAttribute("alt"),
                            ProductPrice = element.Text
                        });

                        
                    }
                        
                    }
                



            }
        }

        public void ShowResultForFlex()
        {
            

            using (IWebDriver driver = new ChromeDriver())
            {
                // Navigate to the page
                //driver.Navigate().GoToUrl("URL_OF_THE_PAGE");
                driver.Navigate().GoToUrl(url2);

                // Wait for dynamic content to load (you might need to adjust the waiting time)
                System.Threading.Thread.Sleep(5000);

                IWebElement tableElement = driver.FindElement(By.CssSelector("table[aria-label='charged-per-day-table']"));


                // Find and extract content
                IReadOnlyCollection<IWebElement> contentElementPrice = tableElement.FindElements(By.CssSelector("td.MuiTableCell-root.charge-column.dhig-w-1\\/4"));

                IReadOnlyCollection<IWebElement> contentElementName = tableElement.FindElements(By.CssSelector("td.MuiTableCell-root.product-or-service-column.dhig-w-3\\/5 div.wrapper"));
                
                for(int i = 0; i< contentElementPrice.Count; i++)
                {
                    if (contentElementName.ToArray()[i] != null && contentElementPrice.ToArray()[i] != null)
                    {
                        FlexTokenBasedCost.InsertOne(new FlexTokenBased()
                        {
                            ProductName = contentElementName.ToArray()[i].Text,
                            ProductPrice = contentElementPrice.ToArray()[i].Text
                        });
                        
                        //Console.WriteLine($"Product Name :- {contentElementName.ToArray()[i].Text} ");
                    }
                    //if (contentElementPrice.ToArray()[i] != null)
                    //{
                    //    Console.WriteLine($"Product Price :- {contentElementPrice.ToArray()[i].Text} ");
                    //}


                    Console.WriteLine("----------------------------------");
                }
                
            }

        }

        //string url = "https://www.autodesk.com/products";


    }
}
