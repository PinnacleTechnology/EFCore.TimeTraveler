using System;
using System.Collections.Generic;
using System.Text;

namespace EFCore.TimeTravelerTests
{
    public class WormWeapon
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public Guid WormId { get; set; }
        public Worm Worm { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
