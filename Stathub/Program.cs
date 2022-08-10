using System;
using System.Collections.Generic;
using System.Linq;

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
        static readonly Dictionary<string, int> CachedCityDistance = new Dictionary<string, int>();

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

            var customers = new List<Customer>{
                new Customer { Name = "Mr. Fake", City = "New York", BirthDate = new DateTime(1981, 8, 1) },
                new Customer{ Name = "Nathan", City = "New York",BirthDate = new DateTime(1981, 8, 5)},
                new Customer{ Name = "Bob", City = "Boston",BirthDate = new DateTime(1981, 8, 4)},
                new Customer{ Name = "Cindy", City = "Chicago",BirthDate = new DateTime(1981, 8, 3)},
                new Customer{ Name = "Lisa", City = "Los Angeles",BirthDate = new DateTime(1981, 8, 2)},
            };

            foreach (var customer in customers)
            {
                Console.WriteLine($"================ {customer.Name} ================");
                Console.WriteLine("Task 1");
                SendEventThatOccuresWithinCustomrCity(customer, events);
                Console.WriteLine("\n\r");

                Console.WriteLine("Task 2 3 4 \n\r");
                SendFiveEventsToCustomerOrderedByDistanceToCustomer(customer, events);
                Console.WriteLine("\n\r");
                SendEventsToCustomerV2(customer, events, FilterBy.SmaeCityAndClosest, 5);
                Console.WriteLine("\n\r ClosestToBirthDay \n\r");
                SendEventsToCustomerV2(customer, events, FilterBy.ClosestToBirthDay);

                Console.WriteLine("\n\r Task 5 \n\r");
                SendEventsToCustomerV2(customer, events, FilterBy.PriceAsc);
                Console.WriteLine("\n\r");
            }
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
            return AlphebiticalDistance(fromCity, toCity);
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

        static int GetDistanceWithDictionaryCache(string from, string to)
        {
            var cacheKey = $"{from}-{to}";
            bool isKeyFound = CachedCityDistance.TryGetValue(cacheKey, out int distance);
            if (!isKeyFound)
            {
                var reverseCacheKey = $"{to}-{from}";
                bool isReverseKeyFound = CachedCityDistance.TryGetValue(reverseCacheKey, out distance);
                if (isReverseKeyFound)
                    return distance;
                try
                {
                    distance = GetDistance(from, to);
                    CachedCityDistance.Add(cacheKey, distance);
                    return distance;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An Exception has occured while calling GetDistance method params(fromCity:{from},toCity:{to}) with message:{ex.Message}");
                    return -1;
                }
            }
            return distance;
        }

        /// <summary>
        /// Send Events to customers based on filter enum with limit events option
        /// </summary>
        /// <param name="customer">Customer to send notifications to</param>
        /// <param name="events">List of events which we want to process</param>
        /// <param name="filterBy">The filter which we want to filter the events based on</param>
        /// <param name="limitEventsCount">Limit number of events to send to the customer</param>
        private static void SendEventsToCustomerV2(Customer customer, List<Event> events, FilterBy filterBy, int limitEventsCount = 0)
        {
            IEnumerable<CustomerEventNotification> eventsToSend = new List<CustomerEventNotification>();

            switch (filterBy)
            {
                case FilterBy.SameCity:
                    eventsToSend = events.Where(e => e.City == customer.City).Select(e => new CustomerEventNotification { Event = e });
                    break;
                case FilterBy.SmaeCityAndClosest:
                    eventsToSend = events.Select(e => new CustomerEventNotification { Event = e, Distance = GetDistanceWithDictionaryCache(customer.City, e.City) }).Where(x => x.Distance >= 0).OrderBy(x => x.Distance);
                    break;
                case FilterBy.PriceAsc:
                    eventsToSend = events.Select(e => new CustomerEventNotification { Event = e, Price = GetPrice(e), Distance = GetDistanceWithDictionaryCache(customer.City, e.City) }).OrderBy(x => x.Distance).ThenBy(x => x.Price);
                    break;
                case FilterBy.ClosestToBirthDay:
                    eventsToSend = events.Select(e => new CustomerEventNotification { Event = e, BirthDayDiffrance = GetMonthDayDiffrance(customer.BirthDate, e.Date) }).Where(x => x.BirthDayDiffrance >= 0).OrderBy(x => x.BirthDayDiffrance);
                    break;
            }

            if (limitEventsCount > 0)
                eventsToSend = eventsToSend.Take(limitEventsCount);

            foreach (var customerEvent in eventsToSend)
            {
                AddToEmail(customer, customerEvent.Event, customerEvent.Price);
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
                    eventDistanceToCustomer.Add(customerEvent.City, events.Where(x => x.City == customerEvent.City).Select(x => new CustomerEventNotification { Event = x, Distance = GetDistanceWithDictionaryCache(customer.City, x.City) }).ToList());
            }
            var eventsToSend = eventDistanceToCustomer.SelectMany(x => x.Value).Where(x => x.Distance >= 0).OrderBy(x => x.Distance).Take(5);
            foreach (var e in eventsToSend)
            {
                AddToEmail(customer, e.Event);
            }
        }
    }
}