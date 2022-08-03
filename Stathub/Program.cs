using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Stathub
{
    public class Event
    {
        public string Name { get; set; }
        public string City { get; set; }
        public DateTime Date { get; set; }
    }

    public class CustomerEventNotification
    {
        public Event Event { get; set; }
        public int? Price { get; set; }
        public int? Distance { get; set; }
        public int? BirthDayDiffrance { get; set; }
    }

    public class Customer
    {
        public string Name { get; set; }
        public string City { get; set; }
        public DateTime BirthDate { get; set; }
    }

    public enum FilterBy
    {
        SameCity,
        SmaeCityAndClosest,
        PriceAsc,
        ClosestToBirthDay,
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            var events = new List<Event>
            {
                new Event{ Name = "Phantom of the Opera", City = "New York",Date = new DateTime(2022,8,1)},
                new Event{ Name = "Metallica", City = "Los Angeles",Date = new DateTime(2022,8,2)},
                new Event{ Name = "Metallica", City = "New York",Date = new DateTime(2022,8,1)},
                new Event{ Name = "Metallica", City = "Boston",Date = new DateTime(2022,8,3)},
                new Event{ Name = "LadyGaGa", City = "New York",Date = new DateTime(2022,8,1)},
                new Event{ Name = "LadyGaGa", City = "Boston",Date = new DateTime(2022,7,31)},
                new Event{ Name = "LadyGaGa", City = "Chicago",Date = new DateTime(2022,8,5)},
                new Event{ Name = "LadyGaGa", City = "San Francisco",Date = new DateTime(2022,8,4)},
                new Event{ Name = "LadyGaGa", City = "Washington",Date = new DateTime(2022,8,5)}
            };

            var customer = new Customer { Name = "Mr. Fake", City = "New York", BirthDate = new DateTime(1981, 8, 1) };
            TimeSpan timeTaken;
            Console.WriteLine("Task 1");
            var timer = new Stopwatch();
            timer.Start();
            SendEventThatOccuresWithinCustomrCity(customer, events);
            timer.Stop();
            timeTaken = timer.Elapsed;
            Console.WriteLine("Time taken: " + timeTaken.ToString(@"m\:ss\.fff"));
            timer.Restart();
            Console.WriteLine("\n\r");

            Console.WriteLine("Task 2 3 4");
            timer.Start();
            SendFiveEventsToCustomerOrderedByDistanceToCustomer(customer, events);
            timer.Stop();
            timeTaken = timer.Elapsed;
            Console.WriteLine("Time taken: " + timeTaken.ToString(@"m\:ss\.fff"));
            timer.Restart();
            Console.WriteLine("\n\r");
            timer.Start();
            SendEventsToCustomerV2(customer, events, FilterBy.SmaeCityAndClosest, 5);
            timer.Stop();
            timeTaken = timer.Elapsed;
            Console.WriteLine("Time taken: " + timeTaken.ToString(@"m\:ss\.fff"));
            timer.Restart();
            Console.WriteLine("\n\r ClosestToBirthDay \n\r");
            timer.Start();
            SendEventsToCustomerV2(customer, events, FilterBy.ClosestToBirthDay);
            timer.Stop();
            timeTaken = timer.Elapsed;
            Console.WriteLine("Time taken: " + timeTaken.ToString(@"m\:ss\.fff"));
            timer.Restart();
            Console.WriteLine("\n\r");


            Console.WriteLine("Task 5");
            timer.Start();
            SendEventsToCustomerV2(customer, events, FilterBy.PriceAsc);
            timer.Stop();
            timeTaken = timer.Elapsed;
            Console.WriteLine("Time taken: " + timeTaken.ToString(@"m\:ss\.fff"));
            timer.Restart();
            Console.WriteLine("\n\r");
            Console.ReadLine();
        }
        // You do not need to know how these methods work
        #region Codebase
        static void AddToEmail(Customer c, Event e, int? price = null)
        {
            var distance = GetDistance(c.City, e.City);
            Console.Out.WriteLine($"{c.Name}: {e.Name} in {e.City}"
            + (distance > 0 ? $" ({distance} miles away)" : "")
            + (price.HasValue ? $" for ${price}" : ""));
        }
        static int GetPrice(Event e)
        {
            return (AlphebiticalDistance(e.City, "") + AlphebiticalDistance(e.Name, "")) / 10;
        }
        static int GetMonthDayDiffrance(DateTime baseDate, DateTime compareDate)
        {
            try
            {
                baseDate = new DateTime(compareDate.Year, baseDate.Month, baseDate.Day);
                var diffranceInDays = Convert.ToInt32((compareDate - baseDate).TotalDays);
                return diffranceInDays;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An Excection has occured with the following message {ex.Message}");
                return -1;
            }
        }
        static int GetDistance(string fromCity, string toCity)
        {
            try
            {
                return AlphebiticalDistance(fromCity, toCity);
            }
            catch (Exception)
            {
                Console.WriteLine($"An Exception has occured while calling GetDistance method params(fromCity:{fromCity},toCity:{toCity}");
                return -1;
            }
        }
        private static int AlphebiticalDistance(string s, string t)
        {
            var result = 0;
            var i = 0;
            for (i = 0; i < Math.Min(s.Length, t.Length); i++)
            {
                // Console.Out.WriteLine($"loop 1 i={i} {s.Length} {t.Length}");
                result += Math.Abs(s[i] - t[i]);
            }
            for (; i < Math.Max(s.Length, t.Length); i++)
            {
                // Console.Out.WriteLine($"loop 2 i={i} {s.Length} {t.Length}");
                result += s.Length > t.Length ? s[i] : t[i];
            }
            return result;
        }
        #endregion

        private static void SendEventsToCustomerV2(Customer customer, List<Event> events, FilterBy filterBy, int limitEventsCount = 0)
        {
            IEnumerable<CustomerEventNotification> eventsToSend = null;

            switch (filterBy)
            {
                case FilterBy.SameCity:
                    eventsToSend = events.Where(e => e.City == customer.City).Select(e => new CustomerEventNotification { Event = e });
                    break;
                case FilterBy.SmaeCityAndClosest:
                    eventsToSend = events.Select(e => new CustomerEventNotification { Event = e, Distance = GetDistance(customer.City, e.City) }).Where(x => x.Distance >= 0).OrderBy(x => x.Distance);
                    break;
                case FilterBy.PriceAsc:
                    eventsToSend = events.Select(e => new CustomerEventNotification { Event = e, Price = GetPrice(e) }).OrderBy(x => x.Price);
                    break;
                case FilterBy.ClosestToBirthDay:
                    eventsToSend = events.Select(e => new CustomerEventNotification { Event = e, BirthDayDiffrance = GetMonthDayDiffrance(customer.BirthDate, e.Date) }).Where(x => x.BirthDayDiffrance >= 0).OrderBy(x => x.BirthDayDiffrance);
                    break;
                default:
                    break;
            }

            if (limitEventsCount > 0)
                eventsToSend = eventsToSend.Take(limitEventsCount);

            foreach (var customerEvent in eventsToSend)
            {
                AddToEmail(customer, customerEvent.Event);
            }
        }

        /// <summary>
        /// find out all events that are in cities of customer then add to email.
        /// </summary>
        /// <param name="customer"></param>
        /// <param name="events">All Events Ocuuring</param>
        private static void SendEventThatOccuresWithinCustomrCity(Customer customer, List<Event> events)
        {
            var eventsWithinCustomerCity = events.FindAll(e => e.City == customer.City);
            eventsWithinCustomerCity.ForEach(e =>
            {
                AddToEmail(customer, e);
            });
        }

        /// <summary>
        /// send closest 5 events to the customer city
        /// </summary>
        /// <param name="customer"></param>
        /// <param name="events"></param>
        private static void SendFiveEventsToCustomerOrderedByDistanceToCustomer(Customer customer, List<Event> events)
        {
            var eventDistanceToCustomer = new Dictionary<string, List<CustomerEventNotification>>();
            foreach (var customerEvent in events)
            {
                if (!eventDistanceToCustomer.ContainsKey(customerEvent.City))
                    eventDistanceToCustomer.Add(customerEvent.City,events.Where(x => x.City == customerEvent.City).Select(x => new CustomerEventNotification { Event = x,Distance = GetDistance(customer.City, x.City) }).ToList());
            }
            var eventsToSend = eventDistanceToCustomer.SelectMany(x => x.Value).OrderBy(x => x.Distance).Take(5);
            foreach (var e in eventsToSend)
            {
                AddToEmail(customer,e.Event);
            }
        }
    }
}
/*
var customers = new List<Customer>{
new Customer{ Name = "Nathan", City = "New York"},
new Customer{ Name = "Bob", City = "Boston"},
new Customer{ Name = "Cindy", City = "Chicago"},
new Customer{ Name = "Lisa", City = "Los Angeles"}
};
*/
