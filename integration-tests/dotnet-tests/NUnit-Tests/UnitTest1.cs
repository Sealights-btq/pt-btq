using System;
using System.Net.Http;
using System.Threading.Tasks;
using NUnit.Framework;
using System.Collections.Generic;

namespace FrontendTests.Tests
{
    [TestFixture]
    public class FrontendTests
    {
        private HttpClient client;
        private List<string> products = new List<string>
        {
            "0PUK6V6EV0",
            "1YMWWN1N4O",
            "2ZYFJ3GM2N",
            "66VCHSJNUP"
        };
        private Random random = new Random();

        [SetUp]
        public void Setup()
        {
            client = new HttpClient { BaseAddress = new Uri(Environment.GetEnvironmentVariable("machine_dns") ?? "") };
        }

        [Test]
        public async Task TestLoad()
        {
            var tasks = new List<Task>();
            for (int i = 0; i < 10; i++)
            {
                tasks.Add(Task.Run(() => TestSession()));
            }
            await Task.WhenAll(tasks);
        }

        private void TestSession()
        {
            var order = new Action[]
            {
                TestIndex,
                TestSetCurrency,
                TestBrowseProduct,
                TestAddToCart,
                TestViewCart,
                TestAddToCart,
                TestCheckout
            };

            foreach (var test in order)
            {
                test();
            }
        }

        [Test]
        public void TestBadRequests()
        {
            var response = client.GetAsync(client.BaseAddress + "/product/89").Result;
            Assert.AreEqual(500, (int)response.StatusCode);

            var formData = new FormUrlEncodedContent(new[] { new KeyValuePair<string, string>("currency_code", "not a currency") });
            response = client.PostAsync(client.BaseAddress + "/setCurrency", formData).Result;
            Assert.AreEqual(405, (int)response.StatusCode);
        }

        [Test]
        public void TestIndex()
        {
            var response = client.GetAsync("/").Result;
            Assert.AreEqual(200, (int)response.StatusCode);
        }

        [Test]
        public void TestSetCurrency()
        {
            foreach (var currency in new[] { "EUR", "USD", "JPY", "CAD" })
            {
                var formData = new FormUrlEncodedContent(new[] { new KeyValuePair<string, string>("currency_code", currency) });
                var response = client.PostAsync("/setCurrency", formData).Result;
                Assert.AreEqual(200, (int)response.StatusCode);
            }
        }

        [Test]
        public void TestBrowseProduct()
        {
            foreach (var product in products)
            {
                var response = client.GetAsync($"/product/{product}").Result;
                Assert.AreEqual(200, (int)response.StatusCode);
            }
        }

        [Test]
        public void TestViewCart()
        {
            var response = client.GetAsync("/cart").Result;
            Assert.AreEqual(200, (int)response.StatusCode);

            response = client.PostAsync("/cart/empty", null).Result;
            Assert.AreEqual(200, (int)response.StatusCode);
        }

        [Test]
        public void TestAddToCart()
        {
            foreach (var product in products)
            {
                var response = client.GetAsync($"/product/{product}").Result;
                Assert.AreEqual(200, (int)response.StatusCode);

                var formData = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("product_id", product),
                    new KeyValuePair<string, string>("quantity", random.Next(1, 6).ToString())
                });

                response = client.PostAsync("/cart", formData).Result;
                Assert.AreEqual(200, (int)response.StatusCode);
            }
        }

        [Test]
        public void TestCheckout()
        {
            foreach (var product in products)
            {
                var formData = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("product_id", product),
                    new KeyValuePair<string, string>("quantity", random.Next(1, 6).ToString()),
                    new KeyValuePair<string, string>("email", "someone@example.com"),
                    new KeyValuePair<string, string>("street_address", "1600 Amphitheatre Parkway"),
                    new KeyValuePair<string, string>("zip_code", "94043"),
                    new KeyValuePair<string, string>("city", "Mountain View"),
                    new KeyValuePair<string, string>("state", "CA"),
                    new KeyValuePair<string, string>("country", "United States"),
                    new KeyValuePair<string, string>("credit_card_number", "4432-8015-6152-0454"),
                    new KeyValuePair<string, string>("credit_card_expiration_month", "1"),
                    new KeyValuePair<string, string>("credit_card_expiration_year", "2039"),
                    new KeyValuePair<string, string>("credit_card_cvv", "672")
                });

                var response = client.PostAsync("/cart/checkout", formData).Result;
                Assert.AreEqual(200, (int)response.StatusCode);
            }
        }
    }
}
