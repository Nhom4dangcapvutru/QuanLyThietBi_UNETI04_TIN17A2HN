using Microsoft.EntityFrameworkCore;
using QuanLyThietBi_UNETI04_DHTI17A2HN.Data;
using QuanLyThietBi_UNETI04_DHTI17A2HN.Services;
using QuanLyThietBi_UNETI04_DHTI17A2HN.Repositories;

var builder = WebApplication.CreateBuilder(args);

// 1. Đăng ký CSDL SQL Server với EF Core 10
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Đăng ký Session (Bắt buộc cho SV 1 làm phân quyền)
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// 3. Đăng ký Service nghiệp vụ cho Module 4 (Xử lý sửa chữa)
builder.Services.AddScoped<IQuyTrinhSuaChuaService, QuyTrinhSuaChuaService>();

// Repository cho Module 2 - Quản lý thiết bị
builder.Services.AddScoped<IThietBiRepository, ThietBiRepository>();

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Cập nhật cấu trúc bằng Migration trước, sau đó mới tạo dữ liệu mẫu nếu chưa có.
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        context.Database.Migrate();
        DbInitializer.Initialize(context);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogCritical(ex, "Không thể cập nhật cấu trúc hoặc khởi tạo Database. Ứng dụng dừng để tránh chạy với dữ liệu chưa sẵn sàng.");
        throw;
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

// 4. Kích hoạt Session middleware
app.UseSession();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
