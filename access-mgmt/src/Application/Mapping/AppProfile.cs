using Application.AccessRules;
using Application.Groups;
using Application.Users;
using AutoMapper;
using Domain.Entities;

namespace Application.Mapping
{
    public sealed class AppProfile : Profile
    {
        public AppProfile()
        {
            CreateMap<User, UserDto>()
                .ForMember(d => d.GroupName, m => m.MapFrom(s => s.UserGroup.GroupName));

            CreateMap<UserDto, User>(); // rarely used; mostly map from Create/Update DTOs
            CreateMap<CreateUserDto, User>()
                .ConstructUsing(d => new User(d.FirstName, d.LastName, d.Email, d.UserGroupId, d.AttachedCustomerId));
            CreateMap<UpdateUserDto, User>(); // we’ll set properties manually to respect encapsulation

            CreateMap<UserGroup, GroupDto>();
            CreateMap<CreateGroupDto, UserGroup>()
                .ConstructUsing(d => new UserGroup(d.GroupName));
            CreateMap<UpdateGroupDto, UserGroup>();

            CreateMap<AccessRule, AccessRuleDto>();
            CreateMap<CreateAccessRuleDto, AccessRule>()
                .ConstructUsing(d => new AccessRule(d.UserGroupId, d.RuleName, d.Permission));
        }
    }
}
