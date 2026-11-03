
using QuanLyThietBi_UNETI04_DHTI17A2HN.ViewModels;

namespace QuanLyThietBi_UNETI04_DHTI17A2HN.Repositories
{
    public interface IThongKeRepository
    {
        Task<ThongKeViewModel> GetThongKeAsync();
    }
}
