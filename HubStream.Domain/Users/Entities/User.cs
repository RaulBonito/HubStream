using HubStream.Domain.Users.Events;
using HubStream.Domain.Users.ValueObjects;
using HubStream.Shared.Kernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace HubStream.Domain.Users.Entities
{
    public class User : AggregateRoot<Identifier>, IAuditableEntity, ISoftDelete
    {
        public FullName FullName { get; private set; }
        public Email Email { get; private set; }
        public string PasswordHash { get; private set; }
        public UserProfile Profile { get; private set; }
        public bool IsStreamer { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? UpdatedAt { get; private set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }

        public User(Identifier id,
                    FullName fullName,
                    Email email,
                    string passwordHash) : base(id)
        {
            FullName = fullName;
            Email = email;
            PasswordHash = passwordHash;
            IsStreamer = false;
            CreatedAt = DateTime.UtcNow;
            IsDeleted = false;
        }

        public static User Register(FullName fullName, Email email, string passwordHash)
        {
            var user = new User(Identifier.New(), fullName, email, passwordHash);

            user.AddDomainEvent(new UserRegistered(user.Id, email.Value, fullName.Value));

            return user;
        }

        public void UpdateProfile(string bio, string bannerUrl, string profilePicUrl)
        {
            if (Profile == null)
            {
                Profile = new UserProfile(bio, bannerUrl, profilePicUrl);
            }
            else
            {
                Profile = Profile with { Bio = bio, BannerUrl = bannerUrl, ProfilePicUrl = profilePicUrl };
            }

            UpdatedAt = DateTime.UtcNow;
        }

        public void PromoteToMentor()
        {
            if (!IsStreamer)
            {
                IsStreamer = true;
                UpdatedAt = DateTime.UtcNow;
            }

            var userPromotedToMentorEvent = new UserPromotedToStreamer(Id);
            AddDomainEvent(userPromotedToMentorEvent);

        }

        public void Delete()
        {
            if (!IsDeleted)
            {
                IsDeleted = true;
                DeletedAt = DateTime.UtcNow;
            }
        }
    }
}
