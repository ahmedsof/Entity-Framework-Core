﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace CarDealer.DataTransferObjects.Input
{
    [XmlType("Supplier")]
    public class SuppliersInputModel
    {
        [XmlElement("name")]
        public string Name { get; set; }

        [XmlElement("isImporter")]
        public bool IsImporter { get; set; }
        
        //<Supplier>
        //<name>3M Company</name>
        //<isImporter>true</isImporter>
        //</Supplier>
    }
}
