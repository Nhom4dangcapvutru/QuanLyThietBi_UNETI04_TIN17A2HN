using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace QuanLyThietBi_UNETI04_DHTI17A2HN.Data;

// EF tools tạo Migration mà không chạy seed hoặc chỉnh dữ liệu nghiệp vụ.
public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var directory = Directory.GetCurrentDirectory();
        if (!File.Exists(Path.Combine(directory, "appsettings.json")))
            directory = Path.Combine(directory, "QuanLyThietBi_UNETI04_DHTI17A2HN");
        var config = new ConfigurationBuilder().SetBasePath(directory)
            .AddJsonFile("appsettings.json").AddEnvironmentVariables().Build();
        return new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(config.GetConnectionString("DefaultConnection")).Options);
    }
}
