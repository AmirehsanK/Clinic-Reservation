using Clinic.Application.Services.Implementation;
using Clinic.Application.Services.Interfaces;
using Clinic.Data.Context;
using Clinic.Mvc;
using Clinic.Data.Entities;
using Clinic.Data.Repositories;
using GoogleReCaptcha.V3;
using GoogleReCaptcha.V3.Interface;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

#region MVC

// AutoValidateAntiforgeryToken applies antiforgery validation to every unsafe
// verb (POST/PUT/DELETE) without each action having to opt in. The views already
// emit __RequestVerificationToken; before this it was never checked.
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
});

// Lets the AJAX group-delete send its token in a header rather than a form field.
builder.Services.AddAntiforgery(options => options.HeaderName = "RequestVerificationToken");

#endregion

#region Database

// SQL Server unless Database:Provider says otherwise - see DatabaseProvider for
// the SQLite testing option.
var dbProvider = builder.Configuration["Database:Provider"] ?? DatabaseProvider.SqlServer;

builder.Services.AddDbContext<AppDbContext>(options =>
    DatabaseProvider.Configure(options, dbProvider, builder.Configuration.GetConnectionString("DefaultConnection")));

#endregion

#region Repositories

builder.Services.AddScoped<IGenericRepository<User>>(sp =>
    new GenericRepository<User>(sp.GetRequiredService<AppDbContext>(), sp.GetRequiredService<AppDbContext>().Set<User>()));
builder.Services.AddScoped<IGenericRepository<Patient>>(sp =>
    new GenericRepository<Patient>(sp.GetRequiredService<AppDbContext>(), sp.GetRequiredService<AppDbContext>().Set<Patient>()));
builder.Services.AddScoped<IGenericRepository<Reservation>>(sp =>
    new GenericRepository<Reservation>(sp.GetRequiredService<AppDbContext>(), sp.GetRequiredService<AppDbContext>().Set<Reservation>()));
builder.Services.AddScoped<IGenericRepository<ReserveRecord>>(sp =>
    new GenericRepository<ReserveRecord>(sp.GetRequiredService<AppDbContext>(), sp.GetRequiredService<AppDbContext>().Set<ReserveRecord>()));

#endregion

#region Application Services

builder.Services.AddMemoryCache();
builder.Services.AddScoped<IOtpService, OtpService>();
builder.Services.AddScoped<ISmsService, SmsService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IReservationService, ReservationService>();
builder.Services.AddScoped<IRecordService, RecordService>();

#endregion

#region Captcha

builder.Services.AddHttpClient<ICaptchaValidator, GoogleReCaptchaValidator>();

#endregion

#region Authentication

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/Login";
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
        options.SlidingExpiration = true;
    });

#endregion

var app = builder.Build();

#region Pipeline

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

#endregion

#region Database Bootstrap

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    context.Database.EnsureCreated();

    // Development convenience only: seed a default admin plus sample patients,
    // reservations and records so the app is usable and demonstrable immediately
    // after cloning, without requiring manual database access.
    // Do NOT rely on this in production - create data through the UI instead.
    if (app.Environment.IsDevelopment())
    {
        DbSeeder.Seed(context, app.Configuration, app.Logger);
    }
}

#endregion

app.Run();
