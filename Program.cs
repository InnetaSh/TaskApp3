


using System;
class Program
{
    static void Main()
    {
        var startTimeTotal = DateTime.Now;
      
        Random random = new Random();
        OrderManager orderManagers = new OrderManager();


        orderManagers.AddOrder(new Order(random.Next(0, 101), "iPhone", random.Next(2000, 10001), 1, "ожидает обработки"));
        orderManagers.AddOrder(new Order(2, "Samsung Galaxy", random.Next(2000, 10001), 2, "ожидает обработки"));
        orderManagers.AddOrder(new Order(3, "Google Pixel", random.Next(2000, 10001), 3, "ожидает обработки"));
        orderManagers.AddOrder(new Order(random.Next(0, 101), "Xiomi 14", random.Next(2000, 10001), 1, "ожидает обработки"));
        orderManagers.AddOrder(new Order(random.Next(0, 101), "Xiomi 15", random.Next(2000, 10001), 3, "ожидает обработки"));

        for (int i = 1; i <= 6; i++)
        {
            orderManagers.AddCourier(new Courier(i));
        }

        TaskManager taskManager = new TaskManager(orderManagers);
        taskManager.Process();

        var endTimeTotal = DateTime.Now;
        Console.WriteLine($"время обработки заказов {endTimeTotal.Subtract(startTimeTotal).TotalSeconds} секунд.");
    }

    class Order
    {
        public int OrderId { get; set; }
        public string OrderName { get; set; }

        public int ProcessingTime { get; set; }

        public int OrderPriority { get; set; }
        public string OrderStatus { get; set; }

        public Order(int id, string name, int processingTime, int priority, string status)
        {
            OrderId = id;
            OrderName = name;
            ProcessingTime = processingTime;
            OrderPriority = priority;
            OrderStatus = status;
        }
    }

    class Courier
    {
        public int CourierId { get; set; }
        public Order CurrentOrder { get; private set; }

        public Courier(int id)
        {
            CourierId = id;
        }
        public void Order(Order order)
        {
            CurrentOrder = order;
        }
        public void OrderProcesses()
        {
            if (CurrentOrder == null) return;

            Console.WriteLine($"Курьер id - { CourierId} начал обработку заказа: id - { CurrentOrder.OrderId +",",-4} название - { CurrentOrder.OrderName + ",",-20} приоритет - { CurrentOrder.OrderPriority}, статут - {CurrentOrder.OrderStatus}");
            CurrentOrder.OrderStatus = "обрабатывается...";
            Console.WriteLine($"заказ id - {CurrentOrder.OrderId} - {CurrentOrder.OrderStatus}");
            Thread.Sleep(CurrentOrder.ProcessingTime);
            CurrentOrder.OrderStatus = "завершение обработки.";
            Console.WriteLine($"Статут заказа id - {CurrentOrder.OrderId} изменен на \'{CurrentOrder.OrderStatus}\'");

            CurrentOrder = null;
        }
    }

    class OrderManager
    {
        public List<Order> orders;
        public List<Courier> couriers;

        public OrderManager()
        {
            orders = new List<Order>();
            couriers = new List<Courier>();
        }

        public void AddOrder(Order order)
        {
            orders.Add(order);
        }
        public void AddCourier(Courier courier)
        {
            couriers.Add(courier);
        }

        public List<Order> SortOrders()
        {
            return orders
                .Where(o => o.OrderStatus == "ожидает обработки")
                .OrderBy(o => o.OrderPriority)
                .ThenBy(o => o.OrderId)
                .ToList(); 
        }

        public List<Courier> SortCouriers()
        {
            return couriers
                .Where(c => c.CurrentOrder == null)
                .ToList();
        }
    }

    class TaskManager
    {
        public OrderManager orderManager { get; set; }

        static Semaphore semaphore = new Semaphore(3, 3);

        public TaskManager(OrderManager orderManager)
        {
            this.orderManager = orderManager;
        }

        public void Process()
        {
            var tasks = new List<Task>();
            var Orders = orderManager.SortOrders();

            foreach (var order in Orders)
            {
                semaphore.WaitOne();

                var courier = orderManager.SortCouriers().FirstOrDefault();
                if (courier != null)
                {
                    courier.Order(order);
                    var task = Task.Run(() =>
                    {
                        courier.OrderProcesses();
                        semaphore.Release();
                    });
                    tasks.Add(task);
                }
                else
                {
                    semaphore.Release();
                    break;
                }
            }

            Task.WaitAll(tasks.ToArray());
            Console.WriteLine("Все заказы обработаны.");
        }
    }
}