using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace CarDealer.DataTransferObjects.Output
{
    [XmlType("customer")]
    public class TotalSalesCustomerOutputModel
    {
        [XmlAttribute("full-name")]
        public string FulName { get; set; }

        [XmlAttribute("bought-cars")]
        public int BoughtCars { get; set; }

        [XmlAttribute("spent-money")]
        public decimal SpentMoney { get; set; }

        //<customer full-name="Hai Everton" bought-cars="1" spent-money="2544.67" />
    }
}
