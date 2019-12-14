using System;

namespace EFCore.TimeTravelerTests.Entities
{
    public class WormFriendship
    {
        public int Id { get; set; }

        public Guid WormAId { get; set; }

        public Guid WormBId { get; set; }

        public Worm WormA { get; set; }
        public Worm WormB { get; set; }

        public override string ToString()
        {
            return $"{WormA?.Name} ❤ {WormB?.Name}";
        }
    }
}
