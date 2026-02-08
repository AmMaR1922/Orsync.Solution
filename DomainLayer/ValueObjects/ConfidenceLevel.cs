using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainLayer.ValueObjects
{

    public class ConfidenceLevel
    {
        public string Level { get; private set; }

        private ConfidenceLevel() { } // For EF Core

        private ConfidenceLevel(string level)
        {
            Level = level;
        }

        public static ConfidenceLevel Low => new("Low");
        public static ConfidenceLevel Medium => new("Medium");
        public static ConfidenceLevel High => new("High");

        public static ConfidenceLevel Create(string level)
        {
            return level?.ToUpper() switch
            {
                "LOW" => Low,
                "MEDIUM" => Medium,
                "HIGH" => High,
                _ => Medium // Default to Medium
            };
        }

        public override string ToString() => Level;

        public override bool Equals(object? obj)
        {
            if (obj is not ConfidenceLevel other)
                return false;

            return Level.Equals(other.Level, StringComparison.OrdinalIgnoreCase);
        }

        public override int GetHashCode() => Level.GetHashCode();
    }

}
