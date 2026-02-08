using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainLayer.ValueObjects
{

    public class Geography
    {
        public string Region { get; private set; }

        private Geography() { } // For EF Core

        public Geography(string region)
        {
            if (string.IsNullOrWhiteSpace(region))
                throw new ArgumentException("Region cannot be empty", nameof(region));

            Region = region;
        }

        public static Geography Create(string region) => new(region);

        public override string ToString() => Region;

        public override bool Equals(object? obj)
        {
            if (obj is not Geography other)
                return false;

            return Region.Equals(other.Region, StringComparison.OrdinalIgnoreCase);
        }

        public override int GetHashCode() => Region.GetHashCode();
    }

}
