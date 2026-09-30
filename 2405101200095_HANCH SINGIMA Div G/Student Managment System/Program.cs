using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using StudentManagementSystem.Data;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<StudentManagementPortal.Data.ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<StudentManagementPortal.Data.ApplicationDbContext>();


// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

// Enable Identity UI Razor Pages (Register/Login/Logout)
app.MapRazorPages();

// Ensure roles exist and assign default roles to existing users
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    try
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var env = services.GetRequiredService<IHostEnvironment>();

        var roles = new[] { "Admin", "User" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
                logger.LogInformation("Created role '{Role}'", role);
            }
        }

        // Assign "User" role to any existing user without roles
        var allUsers = userManager.Users.ToList();
        foreach (var u in allUsers)
        {
            var userRoles = await userManager.GetRolesAsync(u);
            if (userRoles == null || userRoles.Count == 0)
            {
                await userManager.AddToRoleAsync(u, "User");
                logger.LogInformation("Assigned default role 'User' to {Email}", u.Email);
            }
        }

        // If no Admin exists, in Development promote the first user to Admin
        var admins = await userManager.GetUsersInRoleAsync("Admin");
        if ((admins == null || admins.Count == 0) && env.IsDevelopment())
        {
            var first = allUsers.FirstOrDefault();
            if (first != null)
            {
                await userManager.AddToRoleAsync(first, "Admin");
                logger.LogInformation("Promoted {Email} to Admin (development seed)", first.Email);
            }
        }
    }
    catch (Exception ex)
    {
        var logger2 = services.GetRequiredService<ILogger<Program>>();
        logger2.LogError(ex, "An error occurred creating roles");
    }
}

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
