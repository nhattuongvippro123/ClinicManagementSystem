using ClinicManagementSystem.Data;  // truy cập namespace chứa ApplicationDbContext (DbContext)
using Microsoft.EntityFrameworkCore;   // cần để gọi .UseSqlServer(...) (extension method của EF Core)

var builder = WebApplication.CreateBuilder(args);
// Tạo WebApplicationBuilder: khởi tạo DI container (Services), Configuration (appsettings, env vars), Logging.
// 'args' lấy từ dòng lệnh nếu có.

// Add services to the container.
builder.Services.AddControllersWithViews();
// Đăng ký dịch vụ MVC: hỗ trợ Controllers + Views (Razor).
// Thêm nhiều service cần thiết cho MVC (model binding, validation, razor view engine...).

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
// Đăng ký ApplicationDbContext vào DI container (scope per request).
// .UseSqlServer(...) cấu hình EF Core dùng SQL Server với chuỗi kết nối lấy từ cấu hình (appsettings.json hoặc env).
// "DefaultConnection" là khóa trong appsettings.json -> "ConnectionStrings": { "DefaultConnection": "..." }

var app = builder.Build();
// Xây dựng WebApplication từ builder (tạo host, khởi tạo middleware pipeline skeleton).

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // Khi không ở môi trường Development -> dùng trang lỗi chung /Home/Error (không show stacktrace ra user).
    // Phục vụ cho production: người dùng thấy trang lỗi thân thiện.
    app.UseHsts();
    // HSTS (HTTP Strict Transport Security): bảo trình duyệt luôn kết nối qua HTTPS trong 1 khoảng thời gian.
    // Chỉ bật trên môi trường production để tránh cache HSTS trong dev.
}

app.UseHttpsRedirection();
// Middleware chuyển hướng từ http:// -> https:// (nếu có cấu hình port https).
// Lưu ý: nếu không có cấu hình https port sẽ có cảnh báo "Failed to determine the https port for redirect."


app.UseStaticFiles();
// Cho phép phục vụ file tĩnh (css, js, images) từ folder wwwroot.
// wwwroot là thư mục gốc cho file tĩnh trong ASP.NET Core.

app.UseRouting();
// Kích hoạt hệ thống routing (phân tích URL, xác định endpoint sẽ xử lý request).
// Sau khi gọi UseRouting, các middleware như Authentication/Authorization dựa trên route đã được match có thể hoạt động.


app.UseAuthorization();
// Middleware kiểm tra Authorization (chỉ cho phép truy cập nếu Policy/Role phù hợp).
// LƯU Ý: Nếu bạn cần Authentication (đăng nhập), phải gọi app.UseAuthentication() *trước* app.UseAuthorization().

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Đăng ký route mặc định cho controllers:
// Ví dụ: / -> HomeController.Index()
// /Patients/Edit/5 -> PatientsController.Edit(id=5)
// {id?} nghĩa là tham số id là optional.

app.Run();
// Khởi động web server (Kestrel) và bắt đầu lắng nghe request.
