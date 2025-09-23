using Application.Abstractions;
using Application.Common.Exceptions;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.AccessRules
{
    public sealed record AddAccessRuleCommand(CreateAccessRuleDto Dto) : IRequest<Guid>;
    public sealed class AddAccessRuleHandler(IAppDbContext db) : IRequestHandler<AddAccessRuleCommand, Guid>
    {
        public async Task<Guid> Handle(AddAccessRuleCommand req, CancellationToken ct)
        {
            var group = await db.UserGroups.Include(g => g.AccessRules)
                         .FirstOrDefaultAsync(g => g.Id == req.Dto.UserGroupId, ct)
                         ?? throw new KeyNotFoundException("Group not found");

            if (group.AccessRules.Any(r => r.RuleName == req.Dto.RuleName))
                throw new ConflictException("Rule already exists for group");

            var rule = new AccessRule(req.Dto.UserGroupId, req.Dto.RuleName, req.Dto.Permission);
            db.AccessRules.Add(rule);
            await db.SaveChangesAsync(ct);
            return rule.Id;
        }
    }

    public sealed record RemoveAccessRuleCommand(Guid GroupId, Guid RuleId) : IRequest;
    public sealed class RemoveAccessRuleHandler(IAppDbContext db) : IRequestHandler<RemoveAccessRuleCommand>
    {
        public async Task Handle(RemoveAccessRuleCommand req, CancellationToken ct)
        {
            var rule = await db.AccessRules.FirstOrDefaultAsync(r => r.Id == req.RuleId && r.UserGroupId == req.GroupId, ct)
                       ?? throw new KeyNotFoundException("Rule not found");
            db.AccessRules.Remove(rule);
            await db.SaveChangesAsync(ct);
        }
    }

    public sealed record ListRulesByGroupQuery(Guid GroupId) : IRequest<IReadOnlyList<AccessRuleDto>>;
    public sealed class ListRulesByGroupHandler(IAppDbContext db, IMapper mapper) : IRequestHandler<ListRulesByGroupQuery, IReadOnlyList<AccessRuleDto>>
    {
        public async Task<IReadOnlyList<AccessRuleDto>> Handle(ListRulesByGroupQuery req, CancellationToken ct)
            => await db.AccessRules.Where(r => r.UserGroupId == req.GroupId)
                .OrderBy(r => r.RuleName)
                .ProjectTo<AccessRuleDto>(mapper.ConfigurationProvider)
                .ToListAsync(ct);
    }
}
