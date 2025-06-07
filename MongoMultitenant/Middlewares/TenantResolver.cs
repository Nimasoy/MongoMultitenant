using MongoMultitenant.Services;

namespace MongoMultitenant.Middlewares
{
    public class TenantResolver
    {
        private readonly RequestDelegate _next;
        public TenantResolver(RequestDelegate next)
        {
            _next = next;
        }
        public async Task InvokeAsync(HttpContext context, ICurrentTenantService currentTenantService)
        {
            context.Request.Headers.TryGetValue("tenant", out var tenant);
            if (string.IsNullOrWhiteSpace(tenant))
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync("Tenant is required");
                return;
            }
            var isValid = await currentTenantService.CheckTenantAsync(tenant);
            if (!isValid)
            {
                context.Response.StatusCode = 403;
                await context.Response.WriteAsync("Invalid Tenant ID.");
                return;
            }
            currentTenantService.TenantId = tenant;
            await _next(context);
        }
    }

}
