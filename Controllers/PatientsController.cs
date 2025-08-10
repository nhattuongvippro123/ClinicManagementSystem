using ClinicManagementSystem.Data;
using ClinicManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;
//Microsoft.AspNetCore.Mvc chứa Controller và IActionResult.
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.Blazor;
//Microsoft.EntityFrameworkCore cho các phương thức EF như Include, ToListAsync, FirstOrDefaultAsync, v.v.

namespace ClinicManagementSystem.Controllers
{
    public class PatientsController : Controller
    //PatientsController : Controller — lớp controller kế thừa Controller, do đó có access tới helpers như View(), RedirectToAction(), ModelState, NotFound(), v.v.
    {
        private readonly ApplicationDbContext _context;
        //_context là biến private readonly kiểu ApplicationDbContext (EF Core DbContext). Được đưa vào qua Dependency Injection trong constructor. 
        public PatientsController(ApplicationDbContext context)
        {
            _context = context;
            //AddDbContext trong Program.cs phải đăng ký trước để DI cung cấp instance. DbContext đăng ký theo lifetime scoped → 1 instance / request.
        }

        // GET: Patients
        public async Task<IActionResult> Index(string searchString)
        //async Task<IActionResult> — method bất đồng bộ trả về IActionResult (thường View(...)).
        //async cho phép dùng await với các phương thức async của EF (ToListAsync()), giúp không chặn thread server.
        //string searchString — tham số sẽ được model binding tự động lấy từ query string hoặc form data (ví dụ URL /Patients?searchString=an).
        {
            var patients = from p in _context.Patients select p;
            // Tạo IQueryable<Patient> cơ sở; deferred execution (chưa truy vấn DB ngay). Dùng LINQ query expression, tương đương _context.Patients.
            // Ưu điểm: có thể tiếp tục ghép điều kiện để EF chuyển thành một SQL duy nhất.
            if (!string.IsNullOrEmpty(searchString))
            {
                patients = patients.Where(p =>
                    //được EF Core dịch thành SQL LIKE '%searchString%'
                    //Chú ý: case-sensitivity phụ thuộc vào collation của database (SQL Server mặc định không phân biệt hoa thường).
                    p.FullName.Contains(searchString) ||
                    p.PhoneNumber.Contains(searchString));
            }

            // patients.ToListAsync() — truy vấn DB và trả về danh sách(List<Patient>). await vì là bất đồng bộ.
            //View(model) trả model cho view Views/Patients/Index.cshtml. Nếu bạn không truyền tên view, MVC sẽ mặc định tìm view cùng tên action (Index).
            return View(await patients.ToListAsync());
        }

        // GET: Patients/Details/5

        //int? id — nullable int, vì route có thể không cung cấp id.
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
                //trả 404 nếu không có id.
            }

            var patient = await _context.Patients
                /* Eager loading: load luôn collection Appointments liên quan. Nếu Patient có 
                 * navigation property ICollection<Appointment> Appointments
                 * Include tránh lỗi N+1 khi view duyệt appointments.
                 */
                .Include(p => p.Appointments)
                //lấy bản ghi có Id tương ứng hoặc null nếu không tìm.
                .FirstOrDefaultAsync(m => m.Id == id);

            //Kiểm tra tiếp if (patient == null) return NotFound(); — an toàn nếu id không tồn tại.
            if (patient == null)
            {
                return NotFound();
            }
            // trả view Details với model là Patient.
            return View(patient);
        }

        // GET: Patients/Create
        public IActionResult Create()
        {
            //Trả view rỗng để hiển thị form tạo bệnh nhân. (View thường chứa <form asp-action="Create">).
            return View();
        }

        // POST: Patients/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        //  chỉ xử lý POST requests (form submit).
        [HttpPost]
        // bắt buộc token chống CSRF, tương tác với @Html.AntiForgeryToken() trong view. Bảo mật quan trọng.
        [ValidateAntiForgeryToken]

        //Model binding: MVC tự map các giá trị form (input name trùng thuộc tính) vào object patient.
        /*hạn chế những thuộc tính nào được bind — giúp chống overposting attack (kẻ tấn công gửi thêm trường không mong muốn như IsAdmin=true). 
         */
        public async Task<IActionResult> Create([Bind("Id,FullName,DateOfBirth,Gender,PhoneNumber,Address")] Patient patient)
        {
            //kiểm tra validation dựa trên DataAnnotations trong model (ví dụ [Required]) và các binding lỗi (như định dạng ngày). Nếu không hợp lệ, trả lại view để hiển thị lỗi.
            if (ModelState.IsValid)
            {
                // thêm entity vào ChangeTracker và commit vào DB; SaveChangesAsync thực thi SQL INSERT.
                _context.Add(patient);
                await _context.SaveChangesAsync();
                //redirect đến action Index (PRG pattern tránh resubmit form khi refresh).
                return RedirectToAction(nameof(Index));
            }
            //Nếu ModelState không hợp lệ, return View(patient) sẽ render lại form với validation messages.
            return View(patient);
        }
        //Nên hash password hoặc không lưu sensitive data trong patient (không liên quan nhưng lưu ý).
        //Dùng ViewModel tránh overposting thay vì rely on[Bind].


        // GET: Patients/Edit/5
        //
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            //FindAsync(id) — tìm theo primary key. FindAsync ưu việt vì nếu entity đã được tracked trong context nó trả ngay mà không query DB
            var patient = await _context.Patients.FindAsync(id);
            if (patient == null)
            {
                return NotFound();
            }
            //Trả view Edit với model patient để form hiển thị giá trị hiện tại.
            return View(patient);
        }

        // POST: Patients/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FullName,DateOfBirth,Gender,PhoneNumber,Address")] Patient patient)
        {
            //Kiểm tra id route có khớp patient.Id để tránh mismatch tampered id.
            if (id != patient.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    //đánh dấu toàn bộ entity là Modified. EF sẽ phát sinh UPDATE cho tất cả các trường thay đổi. Cẩn trọng: nếu bạn không muốn ghi đè các trường khác
                    //(ai đó thay đổi field bên ngoài), nên fetch entity từ DB rồi set từng property cần thay đổi (patch)
                    _context.Update(patient);
                    // thực thi update vào DB, Execute SQL UPDATE.
                    await _context.SaveChangesAsync();
                    
                }
                //xảy ra khi có concurrency token (RowVersion) hoặc update conflict. Catch và kiểm tra PatientExists — nếu record đã bị xóa thì trả NotFound, còn không thì rethrow để nó được log/hiện lỗi.
                catch (DbUpdateConcurrencyException)
                {
                    if (!PatientExists(patient.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                //Sau thành công redirect về Index.
                return RedirectToAction(nameof(Index));
            }
            return View(patient);
        }

        // GET: Patients/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            //Hiển thị trang xác nhận xoá. Lấy entity bằng FirstOrDefaultAsync (cũng có thể dùng FindAsync).
            var patient = await _context.Patients
                .FirstOrDefaultAsync(m => m.Id == id);
            if (patient == null)
            {
                return NotFound();
            }
            //Trả view Delete để user confirm.
            return View(patient);
        }

        // POST: Patients/Delete/5
        //ActionName("Delete") — vì HTTP POST action đặt tên khác (DeleteConfirmed) nhưng route vẫn dùng Delete. Giữ style scaffold mặc định.
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            //FindAsync(id) lấy entity, nếu null(có thể đã bị xóa) thì skip remove.
            var patient = await _context.Patients.FindAsync(id);
            if (patient != null)
            {
                _context.Patients.Remove(patient);// đánh dấu xoá
            }
            //SaveChangesAsync() thực thi DELETE.
            await _context.SaveChangesAsync();
            //Redirect về Index sau khi xóa
            return RedirectToAction(nameof(Index));
        }

        private bool PatientExists(int id)
        {
            //Kiểm tra tồn tại bản ghi bằng Any — translate thành SELECT CASE WHEN EXISTS ... hiệu quả.
            return _context.Patients.Any(e => e.Id == id);
        }
    }
}

/* Async Everywhere: bạn dùng async/await + EF Core *Async — đúng chuẩn cho web app để không blocking thread pool.

Model Binding & Validation: ModelState.IsValid kiểm tra data annotations; nhớ hiển thị lỗi trong View bằng @Html.ValidationMessageFor.

CSRF Protection: [ValidateAntiForgeryToken] + @Html.AntiForgeryToken() trong form là bắt buộc cho POST.

Chống Overposting: [Bind] giúp, nhưng tốt hơn là dùng ViewModel hoặc DTO chứa đúng các field cần thiết.

Eager Loading: Include khi cần data liên quan; tránh N+1.

Use AsNoTracking() cho read-only queries để tăng hiệu năng.

Error Handling / Concurrency: xử lý DbUpdateConcurrencyException khi app multi-user.

Authorization: thêm [Authorize] trên controller hoặc action để bảo vệ; thêm roles nếu cần: [Authorize(Roles="Admin,Doctor")].

Paging & Sorting: Index nên hỗ trợ phân trang khi dữ liệu lớn (Skip/Take).

Search cải thiện: chuẩn hóa searchString (trim, lower) và dùng EF.Functions.Like để kiểm soát pattern.

Security: validate inputs, tránh lộ stacktrace (Production config), dùng parameterized queries (EF mặc định làm được).

Performance: tạo index trên cột tìm kiếm (FullName, PhoneNumber) ở DB để search nhanh.

 */
