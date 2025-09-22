using Application.Abstractions;
using Application.Common;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Groups
{
    public sealed record CreateGroupCommand(CreateGroupDto Dto) : IRequest<Guid>;
    public sealed class CreateGroupHandler(IAppDbContext db) : IRequestHandler<CreateGroupCommand, Guid>
    {
        public async Task<Guid> Handle(CreateGroupCommand req, CancellationToken ct)
        {
            if (await db.UserGroups.AnyAsync(g => g.GroupName == req.Dto.GroupName, ct))
                throw new InvalidOperationException("Group name already exists");
            var e = new UserGroup(req.Dto.GroupName);
            db.UserGroups.Add(e);
            await db.SaveChangesAsync(ct);
            return e.Id;
        }
    }

    public sealed record UpdateGroupCommand(Guid Id, UpdateGroupDto Dto) : IRequest;
    public sealed class UpdateGroupHandler(IAppDbContext db) : IRequestHandler<UpdateGroupCommand>
    {
        public async Task Handle(UpdateGroupCommand req, CancellationToken ct)
        {
            var e = await db.UserGroups.FirstOrDefaultAsync(x => x.Id == req.Id, ct) ?? throw new KeyNotFoundException("Group not found");
            e.GetType().GetProperty("GroupName")!.SetValue(e, req.Dto.GroupName);
            await db.SaveChangesAsync(ct);
        }
    }

    public sealed record DeleteGroupCommand(Guid Id) : IRequest;
    public sealed class DeleteGroupHandler(IAppDbContext db) : IRequestHandler<DeleteGroupCommand>
    {
        public async Task Handle(DeleteGroupCommand req, CancellationToken ct)
        {
            var e = await db.UserGroups.Include(g => g.Users).FirstOrDefaultAsync(x => x.Id == req.Id, ct)
                    ?? throw new KeyNotFoundException("Group not found");
            if (e.Users.Any()) throw new InvalidOperationException("Cannot delete a group with users.");
            db.UserGroups.Remove(e);
            await db.SaveChangesAsync(ct);
        }
    }

    public sealed record GetGroupByIdQuery(Guid Id) : IRequest<GroupDto>;
    public sealed class GetGroupByIdHandler(IAppDbContext db, IMapper mapper) : IRequestHandler<GetGroupByIdQuery, GroupDto>
    {
        public async Task<GroupDto> Handle(GetGroupByIdQuery req, CancellationToken ct)
            => await db.UserGroups.Where(x => x.Id == req.Id)
               .ProjectTo<GroupDto>(mapper.ConfigurationProvider)
               .SingleOrDefaultAsync(ct) ?? throw new KeyNotFoundException("Group not found");
    }

    public sealed record ListGroupsQuery(int Page = 1, int PageSize = 50) : IRequest<PagedResult<GroupDto>>;
    public sealed class ListGroupsHandler(IAppDbContext db, IMapper mapper) : IRequestHandler<ListGroupsQuery, PagedResult<GroupDto>>
    {
        public async Task<PagedResult<GroupDto>> Handle(ListGroupsQuery q, CancellationToken ct)
        {
            var query = db.UserGroups.AsNoTracking().OrderBy(x => x.GroupName);
            var total = await query.CountAsync(ct);
            var items = await query.Skip((q.Page - 1) * q.PageSize).Take(q.PageSize)
                .ProjectTo<GroupDto>(mapper.ConfigurationProvider).ToListAsync(ct);
            return new(items, q.Page, q.PageSize, total);
        }
    }
}
