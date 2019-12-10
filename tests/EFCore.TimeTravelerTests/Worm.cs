using System;
using System.Collections.Generic;
using System.Text;

namespace EFCore.TimeTravelerTests
{
    public class Worm
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public Guid AppleId { get; set; }

        public Apple Apple { get; set; }
    }
}
