//Import namespace chứa các lớp của EF Core như DbContext, DbSet, extension methods (UseSqlServer, AddDbContext…), và API cho truy vấn/điều chỉnh model.
//Cần nếu bạn muốn kế thừa DbContext và dùng các tính năng EF Core.

using ClinicManagementSystem.Models;
using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;

namespace ClinicManagementSystem.Data
//Gom các lớp liên quan đến data layer vào cùng namespace để tổ chức mã tốt hơn
{
    public class ApplicationDbContext : DbContext
    //ApplicationDbContext kế thừa DbContext — nghĩa là nó là một EF Core context.
    //DbContext là lớp trung gian giữa ứng dụng và cơ sở dữ liệu.
    //DbContext chịu trách nhiệm:

    //Quản lý kết nối DB.

    //Quản lý change tracker (theo dõi entity đã thay đổi).

    //Thực thi truy vấn LINQ sang SQL.

    //Áp dụng migrations khi gọi lệnh (thông qua CLI/PM Console).
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        //Constructor nhận DbContextOptions<ApplicationDbContext> từ dependency injection.
        //DbContextOptions chứa cấu hình: provider (SQL Server, SQLite...), chuỗi kết nối, options khác (lazy loading, command timeout...).

        //: base(options) gọi constructor cha (DbContext) truyền options vào, để DbContext biết cách kết nối.
        //Nguồn options: thường được cấu hình trong Program.cs với builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(...))
        public DbSet<Patient> Patients { get; set; }
        //DbSet<Patient> đại diện cho bảng Patients trong DB (tên bảng mặc định theo tên Patient hoặc Patients tuỳ convention).
        //Truy vấn: _context.Patients.Where(p => p.FullName.Contains("An"))
        //Thêm: _context.Patients.Add(newPatient); await _context.SaveChangesAsync();
        //Cập nhật/xoá: _context.Patients.Update(patient) / _context.Patients.Remove(patient)
        //DbSet còn cung cấp API như .Find(), .Add(), .Remove(), .AsNoTracking(), v.v.
        public DbSet<Appointment> Appointments { get; set; }
    }
}
//Khi thay đổi model (thêm/ xoá DbSet hoặc chỉnh class entity) ->
//Add-Migration <Name> → tạo migration
//Update-Database → áp dụng thay đổi vào DB

//Truy vấn: tracking vs no-tracking
//Mặc định, truy vấn trả về entity được tracking (EF sẽ theo dõi thay đổi).
//Dùng AsNoTracking() khi chỉ đọc để tăng hiệu năng:

//var list = await _context.Patients.AsNoTracking().ToListAsync();

//Loading related data
//Để lấy dữ liệu liên quan (ví dụ appointment kèm patient), dùng Include:
//var appointments = await _context.Appointments.Include(a => a.Patient).ToListAsync();
//Có 3 kiểu load: eager (Include), lazy (cần proxy), explicit (Entry(...).Reference(...).Load()).

//OnModelCreating & Fluent API
//Nếu cần config thêm (khóa chính, tên bảng, quan hệ 1-n, index...), override
//OnModelCreating(ModelBuilder modelBuilder) trong ApplicationDbContext.
//Ví dụ:

/*
 protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Patient>().ToTable("Patients");
    modelBuilder.Entity<Appointment>()
        .HasOne(a => a.Patient)
        .WithMany()
        .HasForeignKey(a => a.PatientId);
}
*/

//Transactions & SaveChanges

//SaveChanges() / SaveChangesAsync() thực thi các thay đổi (INSERT/UPDATE/DELETE) trong một transaction.

/* 
 DbContext = đại diện cho DB, quản lý truy vấn & thay đổi.

DbSet<TEntity> = bảng trong DB, dùng để truy vấn/thao tác entity.

DbContextOptions truyền vào qua DI (cấu hình provider & connection).

Khi thay đổi model, hãy dùng migrations để đồng bộ schema.

Dùng Include, AsNoTracking, transactions, và xử lý concurrency khi cần.
*/