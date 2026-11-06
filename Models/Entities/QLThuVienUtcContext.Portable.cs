using Microsoft.EntityFrameworkCore;

namespace BTL_LTWEB.Models.Entities;
public partial class QLThuVienUtcContext
{
    partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TaiKhoan>().Ignore(x => x.VaiTro);
        if (Database.IsSqlite())
        {
            foreach (var entity in modelBuilder.Model.GetEntityTypes())
                foreach (var p in entity.GetProperties())
                    if (p.GetDefaultValueSql() != null)
                        p.SetDefaultValueSql("CURRENT_TIMESTAMP");
        }
    }
}
