﻿using System;
using System.Collections.Generic;
using System.Text;

namespace CarDealer.DTO
{
    public class SalesInputModel
    {
        public int CarId { get; set; }
        public int CustomerId { get; set; }
        public decimal Discount { get; set; }


        //"carId": 234,
        //"customerId": 23,
        //"discount": 50
    }
}
