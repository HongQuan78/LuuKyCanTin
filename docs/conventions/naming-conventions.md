# Naming conventions

**Status: mandatory for every new file, type and member, whether a person or an AI agent writes it.**

## Scope: new code only

- Every file, class, interface, enum, record, method, property, field, parameter and local variable **you create** must follow this document.
- Existing code that breaks these rules stays as it is. Don't rename it as a side effect of a story. Renames happen in a dedicated refactor story, so the diff stays reviewable and the audit-log payloads, DB columns and migrations don't change by accident.
- When you add a member to an existing class, the new member follows this document even if its neighbours don't.
- If a rule here conflicts with a name that an existing contract forces on you (an interface you implement, an EF column, a DB CHECK value), keep the forced name and say why in a short comment.

## 1. Language: Vietnamese domain, English technique

The name is built from two parts: a **domain stem** in Vietnamese and a **technical role** in English.

| Part | Language | Examples |
|---|---|---|
| Business concept (what it is about) | Vietnamese without diacritics, one capital per syllable | `ChungTuLuuKy`, `SoDuLuuKy`, `DoiTuong`, `PhieuBanHang`, `TheKho`, `CanBo` |
| Pattern / technical role (what kind of thing it is) | English, standard .NET suffix | `Service`, `Dto`, `Request`, `Validator`, `Presenter`, `Form`, `View`, `Configuration`, `Exception`, `Factory`, `Options`, `Extensions`, `Attribute`, `Tests` |

Rules:

1. Take the domain term from the specs in `document/` (feature list, DB design). Use the **same word everywhere**: the entity, the table, the DTO, the service and the form. Don't invent a synonym, and don't translate a domain term into English (`LuuKy`, never `Deposit` or `Custody`).
2. Drop diacritics and capitalize each syllable: *chứng từ lưu ký* → `ChungTuLuuKy`, *đối tượng* → `DoiTuong` (`đ` → `d`).
3. Purely technical concepts with no business meaning stay in English: `AppDbContext`, `DatabaseMigrator`, `SystemClock`, `DotEnvFile`, `GlobalExceptionHandler`.
4. Never mix both languages for one concept (`DepositChungTu` is wrong).
5. No abbreviations other than these well-known ones: `Id`, `Dto`, `Sql`, `Pdf`, `Db`, `Ui`, `ct` (a `CancellationToken`). Acronyms of three letters or more are PascalCase (`Pdf`, `Sql`, not `PDF`). Spec feature codes (`HT-01`) never appear in identifiers.

## 2. Files, folders and namespaces

| Rule | Example |
|---|---|
| One top-level type per file; the file name is the type name. | `CanBoService.cs` contains `CanBoService` |
| Generic type: file name without the type parameter. | `KetQua<T>` → `KetQua.cs` |
| WinForms designer partial: `<Form>.Designer.cs`. | `CanBoForm.Designer.cs` |
| Folders inside each project follow the business module. | `LuuKy/`, `HangHoa/`, `DanhMuc/`, `HeThong/`, `BaoCao/`, plus `Common/`, `Abstractions/` |
| The namespace is the project name plus the folder path, as a file-scoped namespace. | `namespace LuuKyCanTin.Application.DanhMuc;` |
| Test files mirror the folder of the code they test. | `tests/LuuKyCanTin.Application.UnitTests/DanhMuc/CanBoServiceTests.cs` |
| Migrations: `dotnet ef` timestamp plus a PascalCase description of the change, in English verb + domain stem. | `AddCanBo`, `AddSoLanSaiToNguoiDung` |
| Non-code docs: `kebab-case.md`. | `naming-conventions.md` |

## 3. Types by role

| Role | Pattern | Example |
|---|---|---|
| Entity (Domain) | `<Noun>`, singular | `CanBo`, `ChungTuLuuKy` |
| Enum (Domain, always `: byte`) | `<Noun>`, singular; values PascalCase Vietnamese | `HanhDong.Them`, `TrangThaiChungTu.DaGhiSo` |
| Value object / pure rule (Domain) | `<Noun>` describing the concept | `SoTienBangChu`, `BinhQuanGiaQuyen` |
| Interface | `I` + the name of what it is or does | `ICanBoService`, `IGhiNhatKy`, `IClock` |
| Use-case service (Application) | `<Noun>Service`, with interface `I<Noun>Service` | `CanBoService`, `GhiSoLuuKyService` |
| Input model | `<Verb><Noun>Request` | `LuuCanBoRequest`, `GhiSoPhieuThuRequest` |
| Validator | `<Request>Validator` | `LuuCanBoRequestValidator` |
| Output model | `<Noun>Dto` | `CanBoDto` |
| Report query result row | `<Report>Row` | `SoQuyLuuKyRow` |
| Business exception | `<Meaning>Exception`, Vietnamese meaning | `LoiNghiepVuException`, `KhongCoQuyenException` |
| EF configuration | `<Entity>Configuration` | `CanBoConfiguration` |
| QuestPDF template | `<Document>Template` | `PhieuThuTemplate` |
| Excel import/export | `<Noun>Importer` / `<Noun>Exporter` | `DoiTuongImporter` |
| WinForms list form / edit dialog | `<Noun>Form` / `<Noun>EditForm` | `CanBoForm`, `CanBoEditForm` |
| MVP view interface | `I<Noun>View` / `I<Noun>EditView` | `ICanBoView` |
| Presenter | `<Noun>Presenter` / `<Noun>EditPresenter` | `CanBoPresenter` |
| Options bound from config | `<Area>Options` | `AppOptions` |
| Extension methods | `<Thing>Extensions` | `EnumCheckExtensions` |
| Attribute | `<Name>Attribute` | `KhongGhiNhatKyAttribute` |
| Test class | `<TypeUnderTest>Tests` | `CanBoServiceTests` |
| Test fixture / helper | `<Thing>Fixture`, `InMemory<Thing>` | `AppDatabaseFixture` |

Classes are `sealed` unless they are designed for inheritance.

## 4. Methods

| Rule | Example |
|---|---|
| PascalCase, starting with a Vietnamese **verb**, then the object when the class doesn't already make it obvious. | `Them`, `Sua`, `Huy`, `GhiSo`, `Duyet`, `TimAsync`, `LayCanBoDangCongTacAsync` |
| Every method that returns `Task`/`ValueTask` ends in `Async` and takes `CancellationToken ct = default` as its last parameter (public) or `CancellationToken ct` (private). | `Task<CanBoDto> ThemAsync(LuuCanBoRequest request, CancellationToken ct = default)` |
| A method returning `bool` reads as a yes/no question. | `LaSoDuDu(...)`, `CoQuyen(...)`, `DuocPhepHuy(...)` |
| Converters: `Sang<Target>` / `Thanh<Target>`. Factories: `Tao<Thing>`. | `SangDto`, `TaoHopThoai` |
| Use the standard verb set so the same action has one name everywhere. | `Them` (add), `Sua` (edit), `Xoa` (delete), `Huy` (cancel a voucher), `Luu` (save), `Lay` (get one/list), `Tim` (search), `KiemTra` (validate/check), `Tinh` (calculate), `GhiSo` (post), `Duyet` (approve), `In` (print), `Xuat` (export), `Nhap` (import), `TaiLai` (reload), `Hien` (show) |
| Event handlers in a presenter or form: `On<Event>` or the action they perform. | `OnThemClicked`, `TaiLaiAsync` |
| Interface members of English framework contracts keep their English names. | `Dispose`, `SaveChangesAsync`, `Validate` |

## 5. Properties, fields, variables and parameters

| Element | Casing | Rule | Example |
|---|---|---|---|
| Property | PascalCase | A noun. | `MaCanBo`, `HoTen`, `SoTien` |
| `bool` property / variable | PascalCase / camelCase | Prefix `La` (is a), `Co` (has), `Dang` (currently), `Da` (already), `Phai` (must), `DuocPhep` (allowed). | `LaQuanGiao`, `DangCongTac`, `DaGhiSo`, `PhaiDoiMatKhau` |
| Event | PascalCase | Past tense or `<Thing>ThayDoi`; `…Clicked` for buttons. | `ThemClicked`, `TimKiemThayDoi` |
| Constant (`const`) and `static readonly` | PascalCase | No `UPPER_CASE`. Error-message constants start with `Loi`. | `LoiTrungMa`, `SoLanSaiToiDa` |
| Private instance field | `_camelCase` | Prefer primary-constructor parameters for injected services; use a field only when it must be reassigned or exposed. | `_view`, `_lanTim` |
| Parameter / local variable | camelCase | A meaningful noun; no one-letter names except loop indexes (`i`) and short LINQ lambdas (`c => c.Id`). | `maCanBo`, `tuKhoa`, `ketQua` |
| Collection | camelCase / PascalCase | `danhSach<Thing>` (Vietnamese has no plural), or `danhSach` when the type is obvious. | `danhSachCanBo` |
| Money | | Integer đồng, never `float`/`double`; the name says it is an amount: `SoTien…`. | `SoTienThu`, `SoDu` |
| Date / time | | Name says what moment it is: `Ngay…` for a date, `ThoiDiem…` for a timestamp. Value always comes from `IClock`. | `NgayChungTu`, `ThoiDiemGhiSo` |
| WinForms controls | camelCase | Type prefix + the property it edits: `txt`, `lbl`, `btn`, `chk`, `cbo`, `dtp`, `num`, `grd`. | `txtMaCanBo`, `btnLuu`, `grdDanhSach` |
| Generic type parameter | `T` or `T<Meaning>` | | `TEntity` |

Forbidden in new code: Hungarian notation outside WinForms controls (`strTen`, `iCount`), meaningless names (`data`, `temp`, `obj`, `item2`, `x` outside a lambda), English for a domain concept (`balance` instead of `soDu`), and diacritics in identifiers.

## 6. Tests

- Test class: `<TypeUnderTest>Tests`.
- Test method: `<Method>_<Scenario>_<ExpectedResult>`, with the method name as in the code (Vietnamese) and the scenario and result in English, so the test reads as a sentence: `Them_DuplicateCode_IsRejected`, `GhiSo_SoDuKhongDu_ThrowsLoiNghiepVu`.
- Arrange helpers: `Request(...)`, `SeedAsync(...)`, `Tao<Thing>(...)`.

## 7. Database

- Table name = entity name, singular (`CanBo`). Column name = property name.
- `DbSet` property = entity name, singular (`db.CanBo`).
- Index (unique or not): `IX_<Table>_<Column>`, which is also the EF default; check constraint: `CK_<Table>_<Column>` (what `HasEnumCheck` generates).

## Checklist for an AI agent before finishing a change

1. Every new file name matches the single type it contains, and sits in the right module folder.
2. Every new type uses a role pattern from section 3.
3. Every new method starts with a verb from section 4, and every async one ends in `Async` and takes `ct`.
4. Every new `bool` uses a `La/Co/Dang/Da/Phai/DuocPhep` prefix.
5. No existing name was renamed unless the story asks for it.
