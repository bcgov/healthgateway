using System;

namespace HealthGateway.Models
{
    /// <summary>
    /// The Immunization record data model.
    /// </summary>
    public class ImmsDataModel
    {
        /// <summary>
        /// Gets or sets the date of immunization.
        /// </summary>
        public string Date { get; set; }

        /// <summary>
        /// Gets or sets the Vaccine given for the immunization.
        /// </summary>
        public string Vaccine { get; set; }

        /// <summary>
        /// Gets or sets the Dose of the Vaccine given for the immunization.
        /// </summary>
        public string Dose { get; set; }

        /// <summary>
        /// Gets or sets the Site on the patient where the Vaccine was given for the immunization.
        /// </summary>
        public string Site { get; set; }

        /// <summary>
        /// Gets or sets the Lot number of the Vaccine given for the immunization.
        /// </summary>
        public string Lot { get; set; }

        /// <summary>
        /// Gets or sets the Boost Date  for the next immunization.
        /// </summary>
        public string Boost { get; set; }

        /// <summary>
        /// Gets or sets the Trade Name of the vaccine for the immunization.
        /// </summary>
        public string TradeName { get; set; }

        /// <summary>
        /// Gets or sets the manufacturer of the vaccine for the immunization.
        /// </summary>
        public string Manufacturer { get; set; }

        /// <summary>
        /// Gets or sets the route of administration for the immunization.
        /// </summary>
        public string Route { get; set; }

        /// <summary>
        /// Gets or sets the premise/location where the immunization was administered.
        /// </summary>
        public string AdministeredAt { get; set; }

        /// <summary>
        /// Gets or sets who (person) or organization that administered the immunization.
        /// </summary>
        public string AdministeredBy { get; set; }
    }
}
