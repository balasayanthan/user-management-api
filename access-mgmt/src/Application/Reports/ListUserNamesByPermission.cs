using Application.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Reports
{
    public sealed record ListUserNamesByPermissionQuery(bool Permission, string? RuleName = null) : IRequest<IReadOnlyList<string>>;

    public sealed class ListUserNamesByPermissionHandler(IAppDbContext db) : IRequestHandler<ListUserNamesByPermissionQuery, IReadOnlyList<string>>
    {
        public async Task<IReadOnlyList<string>> Handle(ListUserNamesByPermissionQuery q, CancellationToken ct)
        {
            var names = await db.Users
                .Where(u => u.UserGroup.AccessRules.Any(r => r.Permission == q.Permission &&
                             (q.RuleName == null || r.RuleName == q.RuleName)))
                .Select(u => u.FirstName + " " + u.LastName)
                .OrderBy(n => n)
                .ToListAsync(ct);

            return names;
        }
    }
}
