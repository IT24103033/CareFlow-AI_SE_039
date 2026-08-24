using System;
using System.Collections.Generic;

namespace CareFlowAI.API.Models
{
    public class Ward
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string WardNumber { get; set; } = string.Empty;
        public string WardType { get; set; } = string.Empty; // e.g., ICU, General
        public int Capacity { get; set; }
        public int OccupiedBeds { get; set; }
    }
}