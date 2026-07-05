# Tách WinUI Gallery thành nhiều project độc lập (Pilot)

> Tài liệu này ghi lại kiến trúc và quá trình triển khai thử nghiệm (pilot) việc tách WinUIGallery từ 1 exe duy nhất thành nhiều project độc lập: 1 Dashboard + các "Feature app" build ra exe riêng, dùng chung code qua các thư viện `Shared.*`.
>
> Kế hoạch chi tiết gốc (tiếng Anh) nằm tại: `C:\Users\Quang Tan\.claude\plans\enchanted-herding-kazoo.md`

## Bối cảnh

WinUIGallery hiện tại là **một exe duy nhất** chứa 120 sample page trải trên 19 category (định nghĩa trong `WinUIGallery/SampleSupport/Data/ControlInfoData.json`). Mục tiêu: tách thành:

- **App.Dashboard**: app chính, hiển thị danh sách category, bấm vào category nào sẽ **mở exe riêng** của category đó (qua `Process.Start`).
- **App.Feature\***: mỗi category (hoặc 1 phần category, trong pilot này) build thành **1 exe độc lập**, chạy được standalone.
- **Shared.\***: các class library dùng chung giữa Dashboard và các Feature app.

Quyết định đã chốt cho pilot:
- Kiến trúc: `App.Dashboard` + `App.Feature*` (exe) + `Shared.UI` / `Shared.Core` / `Shared.Services` / `Shared.Models` (class library).
- Phạm vi: chỉ làm **3 category thử nghiệm** trước, chưa làm hết 19 category.
- Đóng gói: **unpackaged only** (không MSIX) — tránh hẳn các vấn đề MSIX/launch-profile đã gặp trước đó.
- Project **WinUIGallery.csproj gốc giữ nguyên, không sửa gì** — việc tách là copy file, không di chuyển (additive, không phá vỡ project cũ).

## 3 category thử nghiệm

| Project | Category | Số item | Vì sao chọn |
|---|---|---|---|
| `App.FeatureButton` | `Button` (tách riêng từ nhóm "Basic input") | 1 | Trường hợp đơn giản nhất: không phụ thuộc chéo, không dùng chung SamplePages. |
| `App.FeatureNavigation` | `Navigation` | 5 (BreadcrumbBar, NavigationView, Pivot, SelectorBar, TabView) | Các sample này dùng chung các trang demo (`SamplePage1..7`, `TabViewWindowingSamplePage`,…) qua `SamplesNavigationPageMappings.cs` — kiểm chứng việc chia sẻ code qua `Shared.UI`. |
| `App.FeatureFundamentals` | `FundamentalsItem` | 7 (XamlResources, XamlStyles, Binding, Templates, CustomUserControls, CustomXamlConditionals, ScratchPad) | Nội dung nhiều hơn, có 1 sample (`CustomUserControls`) merge resource ở cấp App.xaml — kiểm chứng việc xử lý resource riêng của từng category. |

Chưa làm: 16 category còn lại, đóng gói MSIX, xoá bớt `WinUIGallery.csproj`, cập nhật `tests/*`.

## Cấu trúc thư mục mới

```
WinUI-Gallery-Extended.slnx        <- solution file mới (dùng để build tất cả cùng lúc)
src/
  Directory.Build.props            <- cấu hình MSBuild chung cho mọi project mới
  Shared/
    Shared.Models/     (class library — Category, ControlInfoData, IconData)
    Shared.Core/       (class library — ThemeHelper*, WindowHelper, FileLoader, NativeMethods,...)
    Shared.Services/   (class library — SettingsHelper, ControlInfoDataSourceBase, ThemeHelper*, AppBootstrap)
    Shared.UI/         (class library — Controls, Styles, Converters, Layouts, SamplePages dùng chung)
  App.Dashboard/        (exe — danh sách category + launcher)
  Features/
    App.FeatureButton/        (exe)
    App.FeatureNavigation/    (exe)
    App.FeatureFundamentals/  (exe)
WinUIGallery/                 <- app gốc, KHÔNG bị đụng vào
WinUIGallery.SourceGenerator/  <- generator gốc, tái sử dụng nguyên trạng cho mọi exe mới
```

*Lưu ý: `ThemeHelper` ban đầu định đặt ở `Shared.Core` nhưng phải chuyển sang `Shared.Services` vì nó phụ thuộc `SettingsHelper` (sẽ tạo vòng lặp phụ thuộc nếu để ở Core).

Chuỗi phụ thuộc (một chiều, không có vòng lặp): `Shared.Models` ← `Shared.Core` ← `Shared.Services` ← `Shared.UI` ← mọi exe.

## Những phát hiện kỹ thuật quan trọng (không tầm thường, phải đọc trước khi sửa)

1. **Chuỗi tra cứu tên page trong SourceGenerator bị hardcode.** `NavigationPageMapperGenerator.cs` luôn build chuỗi `"WinUIGallery.ControlPages." + UniqueId + "Page"` — **không** dựa theo namespace của project. Vì vậy mọi sample page copy sang exe mới **phải giữ nguyên namespace** `WinUIGallery.ControlPages` (và `WinUIGallery.SamplePages` cho các trang demo dùng chung), dù nằm ở project vật lý nào. Không cần sửa gì trong generator.

2. **`ControlInfoDataSource` gốc gọi trực tiếp `NavigationPageMappings.PageDictionary`** (được sinh ra riêng theo từng project compile). Thư viện dùng chung không thể "nhìn thấy" type này. Giải pháp: tách thành `Shared.Services/ControlInfoDataSourceBase.cs` (load JSON, các hàm truy vấn) + một class con mỏng (`ControlInfoDataSource : ControlInfoDataSourceBase`) đặt **riêng trong từng exe**, override để kiểm tra `NavigationPageMappings.PageDictionary` của chính exe đó.

3. **Các class `internal` phải đổi thành `public`** khi bị dùng chéo project: `EnumHelper`, `NativeMethods`, `FileLoader` (Shared.Core), `ListExtensions` (Shared.Services), `DoubleToThicknessConverter`, `MenuItemTemplateSelector`, `ActivityFeedLayout` (Shared.UI).

4. **Cùng `Company`/`AssemblyProduct` giữa mọi project** (đặt trong `src/Directory.Build.props`: `Company=Microsoft`, `AssemblyProduct=WinUIGalleryExtended`) để `ApplicationData.GetForUnpackaged(Publisher, ProductName)` trỏ về **cùng 1 chỗ lưu settings** cho mọi app — nghĩa là theme/favorite/recently-visited có thể đồng bộ giữa Dashboard và các Feature app. *(Đã xác minh qua đọc code `SettingsProviderFactory.cs`; chưa xác minh trực tiếp file/registry lưu ở đâu trong phiên làm việc này.)*

5. **Lỗi phát sinh ngoài kế hoạch ban đầu — xung đột version `WinRT.Runtime`**: `CommunityToolkit.WinUI.Animations`/`Converters` xung đột version với `Microsoft.WindowsAppSDK.Foundation` **chỉ khi nằm trong class library** (không xảy ra ở exe gốc). Sau nhiều cách thử (đổi OutputType, tắt AOT optimizer, gỡ version override,...) không có cách nào sửa được triệt để trong thời gian hợp lý → **giải pháp thực dụng**: viết lại 3 converter nhỏ (`CollectionVisibilityConverter`, `StringVisibilityConverter`, `NullToVisibilityConverter`) trong `Shared.UI/Converters` để thay thế, bỏ hẳn phụ thuộc `CommunityToolkit.WinUI.Animations`/`Converters` khỏi `Shared.UI`.

6. **Lỗi phát sinh ngoài kế hoạch — exe unpackaged không khởi động được**: chạy exe hiện thông báo lỗi *"This application could not be started"*. Nguyên nhân: thiếu `<WindowsAppSdkSelfContained>true</WindowsAppSdkSelfContained>` trong csproj của từng exe (nhúng sẵn Windows App Runtime thay vì cần bootstrap/MSIX riêng). Sau khi thêm, app khởi động và hiển thị cửa sổ bình thường.

## Cách build

```powershell
# Build toàn bộ solution mới (tất cả 8 project)
dotnet build WinUI-Gallery-Extended.slnx -p:Platform=x64

# Build/chạy riêng 1 app
cd src/Features/App.FeatureButton
dotnet build -p:Platform=x64
# exe nằm ở: artifacts/bin/App.FeatureButton/x64/Debug/net9.0-windows10.0.22621.0/App.FeatureButton.exe
```

Project gốc `WinUIGallery/WinUIGallery.csproj` vẫn build và chạy bình thường, không bị ảnh hưởng.

## Cơ chế Dashboard mở Feature app (`Process.Start`)

`App.Dashboard/Services/FeatureAppLauncher.cs` xác định đường dẫn exe của category bằng cách lấy `AppContext.BaseDirectory` của chính Dashboard rồi **thay tên thư mục project** (`App.Dashboard` → `App.FeatureXxx`) trong đường dẫn, vì mọi project đều build ra cùng một cấu trúc `artifacts/bin/<TênProject>/<Platform>/<Config>/<TFM>/`.

Mapping category → exe hiện đang **hardcode** cho 3 category pilot (`BasicInput → App.FeatureButton`, `Navigation → App.FeatureNavigation`, `FundamentalsItem → App.FeatureFundamentals`). Lưu ý: `BasicInput` thật ra có 14 item, nhưng pilot chỉ tách riêng `Button` — đây là điểm đơn giản hoá tạm thời, cần làm rõ khi tách nốt category `BasicInput` đầy đủ.

## Đã kiểm chứng (verification)

- ✅ Build từng project riêng lẻ (`Shared.*`, cả 3 `App.Feature*`, `App.Dashboard`) — không lỗi.
- ✅ Build cả solution `WinUI-Gallery-Extended.slnx` cùng lúc — không lỗi.
- ✅ Build `WinUIGallery.csproj` gốc — vẫn thành công, chứng minh việc tách là additive.
- ✅ Cả 3 Feature app chạy standalone, hiển thị đúng cửa sổ (đã smoke-test bằng cách chạy exe và kiểm tra title cửa sổ).
- ✅ **End-to-end**: chạy Dashboard → dùng UI Automation bấm vào mục "Basic input" trong danh sách → xác nhận `App.FeatureButton.exe` thực sự được khởi chạy như một process riêng biệt.
- ⚠️ Đồng bộ settings (theme/favorite) giữa các app: cơ chế đã được xác minh qua đọc code, nhưng chưa xác minh trực tiếp bằng cách bật app A đổi theme rồi kiểm tra app B (do giới hạn thời gian phiên làm việc).

## Việc còn lại / giới hạn đã biết

- Cơ chế "category đã có exe riêng hay chưa" của Dashboard hiện chỉ là kiểm tra `File.Exists` đơn giản — khi tách hết 19 category nên đổi sang 1 file manifest nhỏ (`AvailableFeatures.json`) để không phải sửa code Dashboard mỗi lần thêm category.
- `tests/WinUIGallery.UnitTests` vẫn tham chiếu trực tiếp `WinUIGallery.csproj` — chưa động vào, cần xem lại khi nào rút gọn project gốc.
- Đóng gói MSIX chưa được xử lý (quyết định: chỉ làm unpackaged cho pilot).
- 16 category còn lại chưa tách — nhưng pattern (cấu trúc file, cách filter JSON, cách viết `ControlInfoDataSource` riêng cho từng exe,...) đã được kiểm chứng và có thể lặp lại y hệt.
