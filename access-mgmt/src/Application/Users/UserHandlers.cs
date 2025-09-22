using Application.Abstractions;
using Application.Common;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Users
{
    // Create
    public sealed record CreateUserCommand(CreateUserDto Dto) : IRequest<Guid>;
    public sealed class CreateUserHandler(IAppDbContext db) : IRequestHandler<CreateUserCommand, Guid>
    {
        public async Task<Guid> Handle(CreateUserCommand request, CancellationToken ct)
        {
            // ensure group exists
            var exists = await db.UserGroups.AnyAsync(g => g.Id == request.Dto.UserGroupId, ct);
            if (!exists) throw new KeyNotFoundException("UserGroup not found");

            var entity = new User(request.Dto.FirstName, request.Dto.LastName, request.Dto.Email, request.Dto.UserGroupId, request.Dto.AttachedCustomerId);
            db.Users.Add(entity);
            await db.SaveChangesAsync(ct);
            return entity.Id;
        }
    }

    // Update
    public sealed record UpdateUserCommand(Guid Id, UpdateUserDto Dto) : IRequest;
    public sealed class UpdateUserHandler(IAppDbContext db) : IRequestHandler<UpdateUserCommand>
    {
        public async Task Handle(UpdateUserCommand req, CancellationToken ct)
        {
            var e = await db.Users.FirstOrDefaultAsync(x => x.Id == req.Id, ct) ?? throw new KeyNotFoundException("User not found");

            // minimal setters while keeping private setters in entity
            e.GetType().GetProperty("FirstName")!.SetValue(e, req.Dto.FirstName);
            e.GetType().GetProperty("LastName")!.SetValue(e, req.Dto.LastName);
            e.GetType().GetProperty("Email")!.SetValue(e, req.Dto.Email);
            e.GetType().GetProperty("UserGroupId")!.SetValue(e, req.Dto.UserGroupId);
            e.GetType().GetProperty("AttachedCustomerId")!.SetValue(e, req.Dto.AttachedCustomerId);

            await db.SaveChangesAsync(ct);
        }
    }

    // Delete
    public sealed record DeleteUserCommand(Guid Id) : IRequest;
    public sealed class DeleteUserHandler(IAppDbContext db) : IRequestHandler<DeleteUserCommand>
    {
        public async Task Handle(DeleteUserCommand req, CancellationToken ct)
        {
            var e = await db.Users.FirstOrDefaultAsync(x => x.Id == req.Id, ct) ?? throw new KeyNotFoundException("User not found");
            db.Users.Remove(e);
            await db.SaveChangesAsync(ct);
        }
    }

    // Get by id
    public sealed record GetUserByIdQuery(Guid Id) : IRequest<UserDto>;
    public sealed class GetUserByIdHandler(IAppDbContext db, IMapper mapper) : IRequestHandler<GetUserByIdQuery, UserDto>
    {
        public async Task<UserDto> Handle(GetUserByIdQuery req, CancellationToken ct)
            => await db.Users.Where(x => x.Id == req.Id)
                .Include(x => x.UserGroup)
                .ProjectTo<UserDto>(mapper.ConfigurationProvider)
                .SingleOrDefaultAsync(ct) ?? throw new KeyNotFoundException("User not found");
    }

    // List with paging/filter
    public sealed record ListUsersQuery(int Page = 1, int PageSize = 20, Guid? GroupId = null, string? Search = null, string? SortBy = "LastName", string SortDir = "asc")
        : IRequest<PagedResult<UserDto>>;

    public sealed class ListUsersHandler(IAppDbContext db, IMapper mapper) : IRequestHandler<ListUsersQuery, PagedResult<UserDto>>
    {
        public async Task<PagedResult<UserDto>> Handle(ListUsersQuery q, CancellationToken ct)
        {
            var query = db.Users.AsNoTracking().Include(u => u.UserGroup).AsQueryable();
            if (q.GroupId is Guid gid) query = query.Where(x => x.UserGroupId == gid);
            if (!string.IsNullOrWhiteSpace(q.Search))
            {
                var s = q.Search.Trim();
                query = query.Where(x => x.FirstName.Contains(s) || x.LastName.Contains(s) || x.Email.Contains(s));
            }

            query = (q.SortBy?.ToLowerInvariant(), q.SortDir.ToLowerInvariant()) switch
            {
                ("firstname", "desc") => query.OrderByDescending(x => x.FirstName),
                ("firstname", _) => query.OrderBy(x => x.FirstName),
                ("email", "desc") => query.OrderByDescending(x => x.Email),
                ("email", _) => query.OrderBy(x => x.Email),
                ("lastname", "desc") => query.OrderByDescending(x => x.LastName),
                _ => query.OrderBy(x => x.LastName),
            };

            var total = await query.CountAsync(ct);
            var items = await query.Skip((q.Page - 1) * q.PageSize).Take(q.PageSize)
                .ProjectTo<UserDto>(mapper.ConfigurationProvider)
                .ToListAsync(ct);

            return new(items, q.Page, q.PageSize, total);
        }
    }
}
