//using đưa namespace chứa các Data Annotation attributes ([Required], [Display], [DataType], …) vào phạm vi file để bạn có thể gắn các attribute này lên thuộc tính.
//Những attribute này dùng cho validation (kiểm tra dữ liệu) và metadata (hiển thị tên trường trong view, kiểu dữ liệu, v.v.).
using System.ComponentModel.DataAnnotations;

namespace ClinicManagementSystem.Models
{
    //public class Patient định nghĩa entity(một hàng trong bảng Patients).
    public class Patient
    {   //Theo convention của EF Core, thuộc tính tên Id (hoặc PatientId) được hiểu là primary key (khóa chính).
        //Kiểu int → EF mặc định sẽ tạo cột int và tự động đánh số (identity) khi chèn (auto-increment).
        //get; set; là auto-property: EF có thể đọc/ghi giá trị.
        public int Id { get; set; }
        //[Required] có nghĩa không được để trống. Nếu form gửi rỗng, ModelState.IsValid sẽ false.
        [Required]
        //Metadata dùng khi hiển thị label/column header trong Razor (ví dụ @Html.DisplayNameFor(m => m.FullName) sẽ hiện "Họ và tên").
        [Display(Name = "Họ và tên")]
        public string FullName { get; set; }

        [Display(Name = "Ngày sinh")]
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }

        [Display(Name = "Giới tính")]
        //Gender đơn giản là string. Bạn có thể thay bằng enum Gender { Male, Female, Other } để an toàn hơn.
        public string Gender { get; set; }

        [Display(Name = "Số điện thoại")]
        //dùng [RegularExpression("regex", ErrorMessage="...")] nếu muốn kiểm soát định dạng (VD: chỉ số VN).
        public string PhoneNumber { get; set; }

        [Display(Name = "Địa chỉ")]
        public string Address { get; set; }

        //Navigation property — biểu thị mối quan hệ 1 Patient có nhiều Appointment (one-to-many).
        //EF Core sẽ hiểu: Appointment cần có 1 FK (ví dụ PatientId) trỏ về Patient.Id.
        //Khi truy vấn Patient, có thể dùng .Include(p => p.Appointments) để lấy danh sách Appointment liên quan.
        // -> Dùng trong query: _context.Patients.Include(p => p.Appointments).FirstOrDefaultAsync(...) để eager-load các appointment.

        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>(); /* Khởi tạo collection mặc định để tránh NullReferenceException khi code phía client gọi patient.Appointments.Add(...) trước khi context attach.

                                                                                                Là best practice để navigation collection luôn sẵn sàng.*/

    }
}
/* 
 * Tên bảng & cột: EF sẽ đặt table name thường là Patients theo convention. Bạn có thể override bằng Fluent API modelBuilder.Entity<Patient>().ToTable("tblPatient").

Nullability: [Required] ảnh hưởng đến validation và có thể khiến EF tạo cột NOT NULL. Nhưng với string nếu nullable reference types bật thì behavior khác; luôn test migration trước khi deploy.

Mapping string → nvarchar(max): nếu muốn nvarchar(100), dùng [StringLength(100)] hoặc modelBuilder.

Quan hệ Patient ↔ Appointment: EF tạo FK ở Appointment (ví dụ PatientId) nếu Appointment có property đó hoặc bạn chỉ định bằng Fluent API.
Validation tốt hơn:

[Required(ErrorMessage = "Họ tên bắt buộc")]
[StringLength(200)]
public string FullName { get; set; }

[Phone]
[StringLength(15)]
public string PhoneNumber { get; set; }
Hiển thị thông báo rõ ràng và giới hạn độ dài để tránh lưu nvarchar(max) vô tội vạ.

Sử dụng ViewModel cho form:

Thay vì bind trực tiếp entity Patient trong Create/Edit view, dùng PatientCreateViewModel để tránh overposting và chỉ gửi những field cần thiết.

Format ngày trong view:

<input asp-for="DateOfBirth" class="form-control" type="date" />
hoặc dùng [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)] trên property để đảm bảo input date hiển thị đúng.

Index cho tìm kiếm nhanh:

Tạo index DB cho FullName và PhoneNumber nếu bạn hay search:
modelBuilder.Entity<Patient>()
    .HasIndex(p => p.FullName);
Xử lý JSON / API:

Nếu xuất model ra JSON (Web API), chú ý vòng lặp circular reference (Patient → Appointments → Patient). Bạn có thể:

Xóa navigation trong DTO, hoặc

Dùng [JsonIgnore] trên Appointments hoặc cấu hình ReferenceHandler.IgnoreCycles.

Cascade delete:

Mặc định EF có thể cấu hình cascade delete cho quan hệ required. Nếu bạn xoá patient, có thể cascade xoá appointments — kiểm soát bằng OnDelete(DeleteBehavior.Restrict) nếu muốn ngăn xoá tự động.

Concurrency:

Nếu nhiều user sửa cùng lúc, cân nhắc thêm RowVersion:

[Timestamp]
public byte[] RowVersion { get; set; }
Nullable / Domain design:

Quyết định thuộc tính nào bắt buộc vs optional. Ví dụ DateOfBirth có thể DateTime? nếu không bắt buộc.


 
 */

/* 
 Id = primary key tự động.

DataAnnotations ([Required], [Display], [DataType]) phục vụ validation + UI metadata.

Appointments là navigation property biểu thị one-to-many; khởi tạo bằng new List<Appointment>() để tránh null.

Dùng ViewModel cho form, thêm [Phone], [StringLength], và index DB cho cột hay tìm kiếm.

Khi thay đổi model, tạo migration và Update-Database để đồng bộ schema.*/