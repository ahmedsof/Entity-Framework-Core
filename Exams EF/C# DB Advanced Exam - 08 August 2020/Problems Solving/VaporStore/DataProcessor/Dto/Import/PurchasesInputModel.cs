using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Xml.Serialization;
using VaporStore.Data.Models.Enums;

namespace VaporStore.DataProcessor.Dto.Import
{
    [XmlType("Purchase")]
    public class PurchasesInputModel
    {
        //•	Id – integer, Primary Key
        //•	Type – enumeration of type PurchaseType, with possible values(“Retail”, “Digital”) (required) 
        //•	ProductKey – text, which consists of 3 pairs of 4 uppercase Latin letters and digits, separated by dashes(ex. “ABCD-EFGH-1J3L”) (required)
        //•	Date – Date(required)
        //    •	CardId – integer, foreign key(required)
        //    •	Card – the purchase’s card(required)
        //    •	GameId – integer, foreign key(required)
        //    •	Game – the purchase’s game(required)
        
        [XmlAttribute("title")]
        [Required]
        public string Title { get; set; }

        [XmlElement("Type")]
        [Required]
        public PurchaseType? Type { get; set; }

        [XmlElement("Key")]
        [Required]
        [RegularExpression("[A-Z0-9]{4}-[A-Z0-9]{4}-[A-Z0-9]{4}")]
        public string Key { get; set; }

        [XmlElement("Card")]
        [RegularExpression("[0-9]{4} [0-9]{4} [0-9]{4} [0-9]{4}")]
        public string Card { get; set; }

        [XmlElement("Date")]
        [Required]
        //[DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
        public string Date { get; set; }


    //    <Purchase title = "Dungeon Warfare 2" >
    //        < Type > Digital </ Type >
    //    < Key > ZTZ3 - 0D2S-G4TJ</Key>
    //    <Card>1833 5024 0553 6211</Card>
    //    <Date>07/12/2016 05:49</Date>
    //    </Purchase>
    }
}
