using System;
using System.Collections.Generic;

namespace Rooms.API.Entities
{
    /// <summary>
    /// Represents a physical building in the dormitory system.
    /// </summary>
    public class Building
    {
        /// <summary>
        /// Gets or sets unique identifier for the building.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the building.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the address of the building.
        /// </summary>
        public string Address { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets total number of floors in the building.
        /// </summary>
        public int FloorsCount { get; set; }

        /// <summary>
        /// Gets or sets year when the building was constructed.
        /// </summary>
        public int YearBuilt { get; set; }

        /// <summary>
        /// Gets or sets contact information for the building administrator.
        /// </summary>
        public string AdministratorContact { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether indicates if the building is currently active/in use.
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Gets navigation property for the floors in this building.
        /// </summary>
        public ICollection<Floor> Floors { get; init; } = new List<Floor>();
    }
}