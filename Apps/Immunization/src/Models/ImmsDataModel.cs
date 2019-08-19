using System;


namespace HealthGateway.Models
{
    /// <summary>
    /// The Immunization record data model.
    /// </summary>
    public class ImmsDataModel
    {
        public string Date { get; set; }
        public string Vaccine { get; set; }
        public string Dose { get; set; }
        public string Site { get; set; }
        public string Lot { get; set; }
        public string Boost { get; set; }
        public string TradeName { get; set; }
        public string Manufacturer { get; set; }
        public string Route { get; set; }
        public string AdministeredAt { get; set; }
        public string AdministeredBy { get; set; }
    }
}
