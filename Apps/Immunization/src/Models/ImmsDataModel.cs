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
    }
}
