# Naming conventions

**Status: mandatory for every new file, type and member, whether a person or an AI agent writes it.**

## The rule in one line

**Code is English. Only what the user sees is Vietnamese.**

| Kind of text | Language | Examples |
|---|---|---|
| Identifiers: files, folders, namespaces, types, members, parameters, variables, enum values, config keys, DB tables, columns and constraints, migration names | English | `CustodyVoucher`, `PostAsync`, `IsSupervisingOfficer`, `AddOfficer` |
| Text shown to the user: form captions, labels, buttons, menus, grid headers, tooltips, message boxes, validation messages, user-facing error messages, printed reports, Excel headers, enum display names, seeded display names | Vietnamese, with full diacritics | `"Lưu"`, `"Mã cán bộ đã tồn tại."`, `"Biên nhận thu tiền"` |
| Text only developers see: code comments, XML docs, log messages, developer exceptions (`ArgumentException`, `InvalidOperationException`), test names | English | `// Row lock prevents a double post.` |

## Scope

- Every file, class, interface, enum, record, method, property, field, parameter and local variable follows this document. Refactor story R.1 renamed the code written before it, so the whole codebase is English.
- If a name is forced on you by an existing contract (an interface member you implement, a stored data code, a DB CHECK value), keep the forced name and say why in a short comment.
- Stories written before this convention name identifiers in Vietnamese (e.g. `KhoaMayForm`, `XacThucLaiAsync`, `"PhienLamViec"`). Translate them with the glossary (`LockScreenForm`, `ReauthenticateAsync`, `"Session"`). **This document wins over the story text.** List the translations in the story's Dev Agent Record.

## 1. Glossary: one English word per domain term

The specs in `document/` are written in Vietnamese. Use the English term below for every identifier, and use the **same word everywhere**: entity, table, DTO, service, form. Don't invent a synonym.

If you need a domain term that isn't listed, add a row here in the same change. Pick the plain English word an auditor would understand, and check that it doesn't collide with an existing row.

### Modules (folders and namespaces)

| Vietnamese | English |
|---|---|
| Lưu ký | `Custody/` |
| Hàng hóa | `Inventory/` |
| Danh mục | `MasterData/` |
| Hệ thống | `Administration/` (not `System`, which clashes with the .NET namespace) |
| Báo cáo | `Reporting/` |
| `Common/`, `Abstractions/` | unchanged |

The product name `LuuKyCanTin` (solution, projects, root namespace) is a brand name and stays.

### Domain terms

| Vietnamese | English identifier |
|---|---|
| tiền gửi lưu ký | `CustodyDeposit` |
| lưu ký | `Custody` |
| căn tin | `Canteen` |
| đối tượng (detainee or prisoner) | `Inmate` |
| loại đối tượng | `InmateType` |
| tạm giữ, tạm giam | `PreTrialDetainee` |
| phạm nhân | `Prisoner` |
| buồng giam | `Cell` |
| người thân | `Relative` |
| chuyển trại | `FacilityTransfer` |
| chấp hành xong án | `SentenceCompleted` |
| cán bộ | `Officer` |
| quản giáo | `SupervisingOfficer` |
| đơn vị, thông tin đơn vị | `Facility`, `FacilityInfo` |
| người ký, cấu hình ký tên | `Signatory`, `SignatoryConfiguration` |
| người dùng | `User` |
| vai trò | `Role` |
| quyền | `Permission` |
| đăng nhập / đăng xuất | `SignIn` / `SignOut` |
| mật khẩu | `Password` |
| phiên làm việc | `Session` |
| khoá máy | `LockSession` (screen: `LockScreen`) |
| nhật ký thao tác | `AuditLog` |
| hành động (audit) | `AuditAction` |
| chứng từ | `Voucher` |
| số chứng từ | `VoucherNumber` |
| đếm số chứng từ | `VoucherCounter` |
| trạng thái chứng từ: nháp / đã ghi sổ / đã hủy | `VoucherStatus`: `Draft` / `Posted` / `Cancelled` |
| chứng từ lưu ký | `CustodyVoucher` |
| phiếu thu / phiếu chi | `Receipt` / `Payout` |
| loại phiếu | `VoucherType` |
| biên nhận thu | `DepositReceipt` |
| nghiệp vụ | `TransactionType` |
| hình thức: tiền mặt / chuyển khoản | `PaymentMethod`: `Cash` / `BankTransfer` |
| ghi sổ | `Post` (noun: `Posting`) |
| sổ cái lưu ký (the ledger engine) | `CustodyLedger` (`CustodyLedgerService`) |
| số dư, số dư lưu ký | `Balance`, `CustodyBalance` |
| quyết toán | `Settlement` |
| hàng hóa | `Product` |
| nhập hàng | `GoodsReceipt` |
| bán hàng, phiếu bán hàng | `Sale`, `SalesVoucher` |
| thẻ kho | `StockCard` |
| bình quân gia quyền | `WeightedAverageCost` |
| số tiền bằng chữ | `AmountInWords` |
| duyệt | `Approve` |
| mã / họ tên / số tiền / ngày | `Code` / `FullName` / `Amount` / `Date` |
| đang quản lý (trạng thái đối tượng) | `InCustody` |
| ngày vào / ngày ra / năm sinh | `AdmissionDate` / `ReleaseDate` / `BirthYear` |
| chức vụ | `Position` |
| lãnh đạo / kế toán / quản trị | `Leader` / `Accountant` / `Administrator` |
| cơ quan chủ quản / địa chỉ | `ParentAgencyName` / `Address` |
| người gửi / quan hệ | `Sender` / `Relationship` |
| số phiếu gốc | `SourceDocumentNumber` |
| số tài khoản | `AccountNumber` |
| không dấu (cột tìm kiếm) | suffix `WithoutDiacritics` (`FullNameWithoutDiacritics`) |
| nội dung (của chứng từ) | `Description` |
| lý do hủy / số lần in | `CancellationReason` / `PrintCount` |
| máy trạm | `Workstation` |
| tiền tố (số chứng từ) | `Prefix` |
| tăng / giảm tiền lưu ký (nhóm quyền LK-T, LK-C) | `CustodyIncrease` / `CustodyDecrease` |
| đọc "lẻ" / "linh" cho hàng chục bằng 0 | `ZeroTensStyle.Southern` / `ZeroTensStyle.Northern` |

### Other rules

1. Purely technical concepts are English as before: `AppDbContext`, `DatabaseMigrator`, `SystemClock`, `DotEnvFile`, `GlobalExceptionHandler`.
2. Never mix both languages in one identifier (`CustodyChungTu` is wrong).
3. No Vietnamese words, with or without diacritics, in new identifiers.
4. No abbreviations other than these well-known ones: `Id`, `Dto`, `Sql`, `Pdf`, `Db`, `Ui`, `ct` (a `CancellationToken`). Acronyms of three letters or more are PascalCase (`Pdf`, `Sql`, not `PDF`). Spec feature codes (`HT-01`) never appear in identifiers.

## 2. User-visible Vietnamese text

- Always write full diacritics, in the wording of the UX prototypes and specs.
- Never build display text from an identifier (`enumValue.ToString()`, `nameof(...)`, splitting PascalCase). The English name and the Vietnamese text are separate.
- **WinForms text** (captions, labels, buttons, headers) lives in the form's Designer file or constructor.
- **Messages from Application** (validation and business errors) are `const string` fields with an English name and a Vietnamese value, declared next to the code that uses them: `public const string DuplicateCodeMessage = "Mã cán bộ đã tồn tại.";`.
- **Enum display names**: one `<Enum>DisplayExtensions.ToDisplayText(this <Enum> value)` per enum in Application, as a `switch` that covers every value. UI and reports both use it. Example: `VoucherStatus.Posted.ToDisplayText()` → `"Đã ghi sổ"`.
- **Seed data and data codes**: display names stored in the DB (role names, permission names) are Vietnamese text. Stable codes that the specs define or that rows already store (permission codes like `HT.Xem`, role codes like `QUAN_TRI`, voucher prefixes like `BNT`, audit action codes like `Them`, sign-in event codes like `DangNhap`) are data, not identifiers. Keep them as they are and give the constant holding one an English name: `public const string ActionView = "Xem";`.
- Test data that represents user input may be Vietnamese (`"Nguyễn Văn A"`).

## 3. Files, folders and namespaces

| Rule | Example |
|---|---|
| One top-level type per file; the file name is the type name. | `OfficerService.cs` contains `OfficerService` |
| Generic type: file name without the type parameter. | `Result<T>` → `Result.cs` |
| WinForms designer partial: `<Form>.Designer.cs`. | `OfficerForm.Designer.cs` |
| Folders inside each project follow the business module (section 1). | `Custody/`, `Inventory/`, `MasterData/`, `Administration/`, `Reporting/`, plus `Common/`, `Abstractions/` |
| The namespace is the project name plus the folder path, as a file-scoped namespace. | `namespace LuuKyCanTin.Application.MasterData;` |
| Test files mirror the folder of the code they test. | `tests/LuuKyCanTin.Application.UnitTests/MasterData/OfficerServiceTests.cs` |
| Migrations: `dotnet ef` timestamp plus a PascalCase English description: verb + what changed. | `AddOfficer`, `AddFailedAttemptCountToUser` |
| Non-code docs: `kebab-case.md`. | `naming-conventions.md` |

## 4. Types by role

| Role | Pattern | Example |
|---|---|---|
| Entity (Domain) | `<Noun>`, singular | `Officer`, `CustodyVoucher` |
| Enum (Domain, always `: byte`) | `<Noun>`, singular; values PascalCase English | `AuditAction.Create`, `VoucherStatus.Posted` |
| Value object / pure rule (Domain) | `<Noun>` describing the concept | `AmountInWords`, `WeightedAverageCost` |
| Interface | `I` + the name of what it is or does | `IOfficerService`, `IAuditLogWriter`, `IClock` |
| Use-case service (Application) | `<Noun>Service`, with interface `I<Noun>Service` | `OfficerService`, `CustodyLedgerService` |
| Input model | `<Verb><Noun>Request` | `SaveOfficerRequest`, `PostReceiptRequest` |
| Validator | `<Request>Validator` | `SaveOfficerRequestValidator` |
| Output model | `<Noun>Dto` | `OfficerDto` |
| Report query result row | `<Report>Row` | `CustodyCashBookRow` |
| Business exception | `<Meaning>Exception` | `BusinessRuleException`, `PermissionDeniedException` |
| EF configuration | `<Entity>Configuration` | `OfficerConfiguration` |
| QuestPDF template | `<Document>Template` | `ReceiptTemplate` |
| Excel import/export | `<Noun>Importer` / `<Noun>Exporter` | `InmateImporter` |
| WinForms list form / edit dialog | `<Noun>Form` / `<Noun>EditForm` | `OfficerForm`, `OfficerEditForm` |
| MVP view interface | `I<Noun>View` / `I<Noun>EditView` | `IOfficerView` |
| Presenter | `<Noun>Presenter` / `<Noun>EditPresenter` | `OfficerPresenter` |
| Options bound from config | `<Area>Options` | `SessionOptions` |
| Extension methods | `<Thing>Extensions` | `EnumCheckExtensions` |
| Attribute | `<Name>Attribute` | `NotAuditedAttribute` |
| Test class | `<TypeUnderTest>Tests` | `OfficerServiceTests` |
| Test fixture / helper | `<Thing>Fixture`, `InMemory<Thing>` | `AppDatabaseFixture` |

Classes are `sealed` unless they are designed for inheritance.

## 5. Methods

| Rule | Example |
|---|---|
| PascalCase, starting with an English **verb**, then the object when the class doesn't already make it obvious. | `Add`, `Update`, `Cancel`, `Post`, `Approve`, `SearchAsync`, `GetActiveOfficersAsync` |
| Every method that returns `Task`/`ValueTask` ends in `Async` and takes `CancellationToken ct = default` as its last parameter (public) or `CancellationToken ct` (private). | `Task<OfficerDto> AddAsync(SaveOfficerRequest request, CancellationToken ct = default)` |
| A method returning `bool` reads as a yes/no question. | `HasSufficientBalance(...)`, `HasPermission(...)`, `CanCancel(...)` |
| Converters: `To<Target>`. Factories: `Create<Thing>`. | `ToDto`, `CreateDialog` |
| Use the standard verb set so the same action has one name everywhere. | `Add`, `Update`, `Delete`, `Cancel` (a voucher), `Save`, `Get` (one or a list), `Search`, `Validate`, `Calculate`, `Post`, `Approve`, `Print`, `Export`, `Import`, `Reload`, `Show` |
| Event handlers in a presenter or form: `On<Event>` or the action they perform. | `OnAddClicked`, `ReloadAsync` |

## 6. Properties, fields, variables and parameters

| Element | Casing | Rule | Example |
|---|---|---|---|
| Property | PascalCase | A noun. | `OfficerCode`, `FullName`, `Amount` |
| `bool` property / variable | PascalCase / camelCase | Prefix `Is`, `Has`, `Can` or `Must`. | `IsSupervisingOfficer`, `IsActive`, `IsPosted`, `MustChangePassword` |
| Event | PascalCase | Past tense or `<Thing>Changed`; `…Clicked` for buttons. | `AddClicked`, `SearchTextChanged` |
| Constant (`const`) and `static readonly` | PascalCase | No `UPPER_CASE`. User-facing message constants end in `Message`. | `DuplicateCodeMessage`, `MaxFailedAttempts` |
| Private instance field | `_camelCase` | Prefer primary-constructor parameters for injected services; use a field only when it must be reassigned or exposed. | `_view`, `_searchVersion` |
| Parameter / local variable | camelCase | A meaningful noun; no one-letter names except loop indexes (`i`) and short LINQ lambdas (`c => c.Id`). | `officerCode`, `keyword`, `result` |
| Collection | camelCase / PascalCase | Plural noun. | `officers`, `Vouchers` |
| Money | | Integer đồng, never `float`/`double`; the name says it is an amount: `…Amount` or `Balance…`. | `ReceiptAmount`, `BalanceBefore` |
| Date / time | | `…Date` for a date, `…At` for a timestamp. The value always comes from `IClock`. | `VoucherDate`, `PostedAt` |
| WinForms controls | camelCase | Type prefix + the property it edits: `txt`, `lbl`, `btn`, `chk`, `cbo`, `dtp`, `num`, `grd`. | `txtOfficerCode`, `btnSave`, `grdList` |
| Config key (`appsettings.json`) | PascalCase | English, matching the options class. | `"Session": { "IdleLockMinutes": 5 }` |
| Generic type parameter | `T` or `T<Meaning>` | | `TEntity` |

Forbidden in new code: Vietnamese words in identifiers, Hungarian notation outside WinForms controls (`strName`, `iCount`), meaningless names (`data`, `temp`, `obj`, `item2`, `x` outside a lambda), and diacritics in identifiers.

## 7. Tests

- Test class: `<TypeUnderTest>Tests`.
- Test method: `<Method>_<Scenario>_<ExpectedResult>`, all in English, so the test reads as a sentence: `Add_DuplicateCode_IsRejected`, `Post_InsufficientBalance_ThrowsBusinessRuleException`.
- Arrange helpers: `Request(...)`, `SeedAsync(...)`, `Create<Thing>(...)`.
- Assert user-facing messages against the message constant, not a copied Vietnamese literal.

## 8. Database

- Table name = entity name, singular (`Officer`). Column name = property name.
- `DbSet` property = entity name, singular (`db.Officer`).
- Index (unique or not): `IX_<Table>_<Column>`, which is also the EF default; check constraint: `CK_<Table>_<Column>` (what `HasEnumCheck` generates).
- `User` is a T-SQL keyword: write `[User]` in raw SQL.
- The audit log stores the table name and property names of the row it records, so rows written before R.1 keep their old Vietnamese names (`NhatKyThaoTac`, `ChungTuLuuKy`, `SoTien`, …). The log is append-only: never rewrite it. `LegacyAuditNames` (Application) maps those old names to the current ones for display. The action column stores fixed codes (`Them`, `Sua`, …) through `AuditActionCodes`, so old and new rows agree.

## Checklist for an AI agent before finishing a change

1. Every new identifier is English, uses the glossary term for each domain concept, and contains no Vietnamese word.
2. Every new piece of user-visible text is Vietnamese with full diacritics, and none of it is derived from an identifier.
3. Every new file name matches the single type it contains, and sits in the right module folder.
4. Every new type uses a role pattern from section 4.
5. Every new method starts with a verb from section 5, and every async one ends in `Async` and takes `ct`.
6. Every new `bool` uses an `Is/Has/Can/Must` prefix.
7. Any new domain term was added to the glossary.
