# Windows Batch Compilation Instructions

Bu doküman, System Manager uygulamasını Windows'ta .bat dosyası kullanarak derleme talimatlarını içerir.

## Derleme Seçenekleri

İki farklı derleme yöntemi sunulmaktadır:

### 1. Doğrudan CSC Derleyici (Önerilen) - `compile.bat`

**Özellikler:**
- MSBuild kullanmaz, doğrudan C# derleyicisi (csc.exe) kullanır
- Daha hızlı ve daha az bağımlılık gerektirir
- Sadece .NET Framework 4.8 yüklü olması yeterlidir

**Kullanım:**
```cmd
compile.bat
```

**Gereksinimler:**
- Windows işletim sistemi
- .NET Framework 4.8 yüklü olmalı
- Derleyici yolu: `C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe`

**Çıktı:**
- `SystemManager\bin\Release\SystemManager.exe`

### 2. MSBuild Derleyici (Alternatif) - `compile-simple.bat`

**Özellikler:**
- MSBuild kullanır (Visual Studio veya .NET SDK gerektirir)
- Daha fazla özellik sunar
- Proje dosyası (.csproj) bazlı derleme

**Kullanım:**
```cmd
compile-simple.bat
```

**Gereksinimler:**
- Visual Studio 2019/2022 VEYA
- .NET Framework SDK yüklü olmalı

## Derleme Adımları

### Adım 1: Gerekli Yazılımları Yükleyin

**Minimum Gereksinim (.NET Framework 4.8):**
1. .NET Framework 4.8'i indirin ve yükleyin
   - İndirme: https://dotnet.microsoft.com/download/dotnet-framework/net48
   - Runtime veya Developer Pack seçebilirsiniz

**Tam Özellikler İçin (Opsiyonel):**
1. Visual Studio 2019 veya 2022 Community Edition
   - İndirme: https://visualstudio.microsoft.com/downloads/
   - ".NET desktop development" workload'ını seçin

### Adım 2: Projeyi Derleyin

**Yöntem A - Doğrudan CSC (Önerilen):**
```cmd
# Proje klasörüne gidin
cd path\to\repo

# Derleme scriptini çalıştırın
compile.bat

# Sonuç: SystemManager\bin\Release\SystemManager.exe
```

**Yöntem B - MSBuild:**
```cmd
# Proje klasörüne gidin
cd path\to\repo

# Derleme scriptini çalıştırın
compile-simple.bat

# Sonuç: SystemManager\bin\Release\SystemManager.exe
```

### Adım 3: Uygulamayı Çalıştırın

```cmd
SystemManager\bin\Release\SystemManager.exe
```

## Derleme Hataları ve Çözümleri

### Hata: "csc.exe bulunamadı"

**Çözüm:**
1. .NET Framework 4.8'in yüklü olduğundan emin olun
2. Şu konumu kontrol edin: `C:\Windows\Microsoft.NET\Framework64\v4.0.30319\`
3. Eğer farklı bir konumdaysa, `compile.bat` dosyasındaki `DOTNET_FX` değişkenini güncelleyin

### Hata: "ResGen.exe bulunamadı"

**Çözüm:**
1. .NET Framework SDK yükleyin
2. Veya `compile-simple.bat` (MSBuild) kullanın

### Hata: "MSBuild bulunamadı"

**Çözüm:**
1. Visual Studio yükleyin
2. Veya `compile.bat` (CSC) kullanın

### Hata: Derleme başarılı ama çalışmıyor

**Çözüm:**
1. .NET Framework 4.8 Runtime'ın yüklü olduğundan emin olun
2. Antivirus yazılımınızı kontrol edin (EXE'yi engelliyor olabilir)
3. Yönetici olarak çalıştırmayı deneyin

## Manuel Derleme (İleri Seviye)

Eğer batch dosyaları çalışmazsa, manuel olarak derleyebilirsiniz:

```cmd
# Adım 1: Resource dosyalarını oluşturun
cd SystemManager
C:\Windows\Microsoft.NET\Framework64\v4.0.30319\ResGen.exe MainForm.resx obj\Release\SystemManager.MainForm.resources
C:\Windows\Microsoft.NET\Framework64\v4.0.30319\ResGen.exe Properties\Resources.resx obj\Release\SystemManager.Properties.Resources.resources

# Adım 2: Derleyin
C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe ^
  /target:winexe ^
  /out:bin\Release\SystemManager.exe ^
  /optimize+ ^
  /platform:anycpu ^
  /r:System.dll ^
  /r:System.Core.dll ^
  /r:System.Management.dll ^
  /r:System.Windows.Forms.dll ^
  /r:System.Drawing.dll ^
  /resource:obj\Release\SystemManager.MainForm.resources ^
  /resource:obj\Release\SystemManager.Properties.Resources.resources ^
  Program.cs MainForm.cs MainForm.Designer.cs ^
  (diğer tüm .cs dosyaları...)

# Adım 3: Config dosyasını kopyalayın
copy App.config bin\Release\SystemManager.exe.config
```

## Derleme Çıktısı

Başarılı derleme sonrası şu dosyalar oluşturulur:

```
SystemManager\bin\Release\
├── SystemManager.exe           # Ana uygulama
├── SystemManager.exe.config    # Konfigürasyon dosyası
└── SystemManager.pdb           # Debug sembolleri (opsiyonel)
```

## Dağıtım

Uygulamayı dağıtmak için:

1. `SystemManager\bin\Release\` klasöründeki tüm dosyaları kopyalayın
2. Hedef bilgisayarda .NET Framework 4.8 yüklü olmalı
3. İsteğe bağlı olarak bir installer oluşturabilirsiniz (WiX, InstallShield, vb.)

## Performans İpuçları

### Hızlı Derleme
- `compile.bat` kullanın (MSBuild'den daha hızlı)
- SSD kullanın
- Antivirus'ü geçici olarak devre dışı bırakın

### Temiz Derleme
Önceki derleme dosyalarını temizlemek için:
```cmd
# Tüm bin ve obj klasörlerini silin
rmdir /s /q SystemManager\bin
rmdir /s /q SystemManager\obj
```

## Sorun Giderme

### Debug Modu
Debug modunda derlemek için `compile.bat` dosyasını düzenleyin:
- `/optimize+` → `/optimize-` değiştirin
- `/debug:pdbonly` → `/debug:full` ekleyin

### Verbose Çıktı
Daha fazla detay için:
- `compile.bat` dosyasında `/v:minimal` → `/v:detailed` değiştirin
- Veya MSBuild kullanın: `compile-simple.bat`

### Paralel Derleme
MSBuild ile daha hızlı derleme:
```cmd
msbuild SystemManager.sln /p:Configuration=Release /m
```

## Ek Bilgiler

### Sistem Gereksinimleri
- **İşletim Sistemi:** Windows 10 veya üzeri
- **.NET Framework:** 4.8
- **RAM:** Minimum 512 MB
- **Disk:** 100 MB boş alan

### Derleme Süresi
- İlk derleme: ~30-60 saniye
- Sonraki derlemeler: ~10-20 saniye

### Çıktı Boyutu
- SystemManager.exe: ~100-200 KB (bağımlılıklar hariç)
- Toplam dağıtım: ~1-2 MB

## Yardım ve Destek

Sorunlarla karşılaşırsanız:

1. **Hata Mesajlarını Kontrol Edin:** Batch dosyası detaylı hata mesajları gösterir
2. **Gereksinimleri Kontrol Edin:** .NET Framework 4.8 yüklü mü?
3. **Yolu Kontrol Edin:** Dosya yollarında Türkçe karakter var mı? (sorun yaratabilir)
4. **Yönetici Yetkisi:** Bazı işlemler yönetici yetkisi gerektirebilir

## Özet

**En Basit Yöntem:**
```cmd
# Sadece .NET Framework 4.8 yükleyin
# Sonra:
compile.bat
```

**Tam Özellikli Yöntem:**
```cmd
# Visual Studio yükleyin
# Sonra:
compile-simple.bat
```

Her iki yöntem de aynı sonucu üretir: `SystemManager\bin\Release\SystemManager.exe`

---

**Son Güncelleme:** Aralık 2025  
**Hedef Framework:** .NET Framework 4.8  
**Derleme Sistemi:** Windows Batch Script
