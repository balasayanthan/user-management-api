using Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public abstract class Person : Entity<Guid>
    {
        public string FirstName { get; private set; } = default!;
        public string LastName { get; private set; } = default!;
        public string Email { get; private set; } = default!;
        public string FullName => $"{FirstName} {LastName}";

        protected Person() { }
        protected Person(string first, string last, string email)
            => (FirstName, LastName, Email) = (first, last, email);
    }
    public sealed class Admin : Person
    {
        public string Privilege { get; private set; } = "Standard";
        public Admin(string first, string last, string email, string privilege)
            : base(first, last, email) => Privilege = privilege;
        private Admin() { }
    }
    public sealed class User : Person
    {
        public int? AttachedCustomerId { get; private set; }
        public Guid UserGroupId { get; private set; }
        public UserGroup UserGroup { get; private set; } = default!;
        public User(string first, string last, string email, Guid groupId, int? customerId = null)
            : base(first, last, email) { UserGroupId = groupId; AttachedCustomerId = customerId; }
        private User() { }
    }
    public sealed class UserGroup : Entity<Guid>
    {
        public string GroupName { get; private set; } = default!;
        public ICollection<User> Users { get; } = new List<User>();
        public ICollection<AccessRule> AccessRules { get; } = new List<AccessRule>();
        public UserGroup(string name) => GroupName = name;
        private UserGroup() { }
    }
    public sealed class AccessRule : Entity<Guid>
    {
        public Guid UserGroupId { get; private set; }
        public string RuleName { get; private set; } = default!;
        public bool Permission { get; private set; }
        public UserGroup UserGroup { get; private set; } = default!;
        public AccessRule(Guid groupId, string ruleName, bool permission)
            => (UserGroupId, RuleName, Permission) = (groupId, ruleName, permission);
        private AccessRule() { }
    }
}
