using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace Seravian.ActionFilters;

public class VerifiedDoctorOnlyAttribute : Attribute, IAsyncActionFilter
{
    private readonly ApplicationDbContext _dbContext;

    public VerifiedDoctorOnlyAttribute(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task OnActionExecutionAsync(
        ActionExecutingContext context,
        ActionExecutionDelegate next
    )
    {
        var httpContext = context.HttpContext;

        if (!httpContext.User.Identity?.IsAuthenticated ?? true)
        {
            context.Result = new ForbidResult();
            return;
        }

        var userId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

        // validate if the userId is a valid guid
        if (!Guid.TryParse(userId, out Guid doctorId))
        {
            context.Result = new ForbidResult();
            return;
        }

        var doctor = await _dbContext
            .Doctors.AsNoTracking()
            .FirstOrDefaultAsync(d => d.UserId == doctorId);

        if (doctor == null || doctor.VerifiedAtUtc is null)
        {
            context.Result = new ForbidResult();
            return;
        }

        await next(); // User is verified, continue to action
    }
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class UseVerifiedDoctorOnlyAttribute : TypeFilterAttribute
{
    public UseVerifiedDoctorOnlyAttribute()
        : base(typeof(VerifiedDoctorOnlyAttribute)) { }
}
