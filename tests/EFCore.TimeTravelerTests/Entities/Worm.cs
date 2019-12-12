using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Humanizer;

namespace EFCore.TimeTravelerTests
{
    public class Worm
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public Guid AppleId { get; set; }

        public Apple Apple { get; set; }

        public List<WormWeapon> Weapons { get; set; } = new List<WormWeapon>();

        public List<WormFriendship> FriendshipsA { get; set; } = new List<WormFriendship>();

        public List<WormFriendship> FriendshipsB { get; set; } = new List<WormFriendship>();

        public IEnumerable<WormFriendship> Friendships => FriendshipsA.Concat(FriendshipsB);

        public IEnumerable<string> FriendsNames =>
            Friendships.Select(friendship => friendship.WormA?.Name)
                .Concat(Friendships.Select(friendship => friendship.WormB?.Name))
                .Distinct()
                .Except(new []{ Name });


        public override string ToString()
        {
            return $"{Name}, {nameof(Weapons)}: {Weapons.Humanize()}, {nameof(Friendships)}: {Friendships.Humanize()}";
        }
    }
}
