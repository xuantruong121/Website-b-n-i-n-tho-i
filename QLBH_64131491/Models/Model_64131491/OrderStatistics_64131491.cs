using System;
using System.Collections.Generic;

namespace QLBH_64131491.Models.Model_64131491
{
    public class OrderStatistics_64131491
    {
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public int PendingOrders { get; set; }
        public int ShippedOrders { get; set; }
        public int CompletedOrders { get; set; }
        public int CancelledOrders { get; set; }
        public int CashPaymentCount { get; set; }
        public int BankTransferCount { get; set; }
        public int CreditCardCount { get; set; }
        public Dictionary<string, decimal> MonthlyRevenue { get; set; }
        public List<TopSellingProduct> TopSellingProducts { get; set; }

        public class TopSellingProduct
        {
            public string Brand { get; set; }
            public string Model { get; set; }
            public int TotalSold { get; set; }
            public decimal TotalRevenue { get; set; }
        }
    }
} 