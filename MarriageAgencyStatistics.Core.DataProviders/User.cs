using System;
using System.Runtime.Serialization;

namespace MarriageAgencyStatistics.Core.DataProviders
{
    public enum UserMode
    {
        Active = 0,
        Inactive = 1,
        Silent = 2
    }

    [Serializable]
    public class User : IEquatable<User>
    {
        [IgnoreDataMember] public string FirstName => Name.Split(null)[0];
        [IgnoreDataMember] public string LastName => Name.Split(null)[1];
        public string Name { get; set; }
        public UserMode UserMode { get; set; }
        
        public string ID { get; set; }

        public override string ToString()
        {
            return Name;
        }

        public bool Equals(User other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(ID, other.ID);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((User) obj);
        }

        public override int GetHashCode()
        {
            return (ID != null ? ID.GetHashCode() : 0);
        }

        public static bool operator ==(User left, User right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(User left, User right)
        {
            return !Equals(left, right);
        }
    }
}