using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainLayer.ValueObjects
{

    public class TherapeuticArea
    {
        public string Name { get; private set; }

        private TherapeuticArea() { } // For EF Core

        public TherapeuticArea(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Therapeutic area name cannot be empty", nameof(name));

            Name = name;
        }

        public static TherapeuticArea Create(string name) => new(name);

        public override string ToString() => Name;

        public override bool Equals(object? obj)
        {
            if (obj is not TherapeuticArea other)
                return false;

            return Name.Equals(other.Name, StringComparison.OrdinalIgnoreCase);
        }

        public override int GetHashCode() => Name.GetHashCode();
    }

}
