# Hızlı Derleme Rehberi (Türkçe)

## En Basit Yöntem - 3 Adım!

### 1. Gereksinim
.NET Framework 4.8 yüklü olmalı (Windows 10'da genelde yüklüdür)

**Kontrol:** Dosya Gezgini'nde şu klasöre gidin:
```
C:\Windows\Microsoft.NET\Framework64\v4.0.30319\
```
Eğer bu klasör varsa, hazırsınız! Yoksa şuradan indirin:
https://dotnet.microsoft.com/download/dotnet-framework/net48

### 2. Derleme
Proje klasöründe `compile.bat` dosyasına çift tıklayın.

### 3. Çalıştırma
```
SystemManager\bin\Release\SystemManager.exe
```
dosyasına çift tıklayın.

## Hepsi Bu Kadar!

---

## Sorun Çözümleri

### "csc.exe bulunamadı" hatası
👉 .NET Framework 4.8'i yükleyin: https://dotnet.microsoft.com/download/dotnet-framework/net48

### "Derleme başarısız" hatası
👉 `compile-simple.bat` dosyasını deneyin (Visual Studio gerektirir)

### Uygulama açılmıyor
👉 Yönetici olarak çalıştırmayı deneyin (sağ tık → Yönetici olarak çalıştır)

---

## Alternatif Yöntem (Visual Studio varsa)

Eğer Visual Studio yüklüyse:

**Yöntem 1:** `SystemManager.sln` dosyasına çift tıklayın → F5'e basın

**Yöntem 2:** `compile-simple.bat` dosyasına çift tıklayın

---

## Önemli Notlar

✅ **Derleme Zamanı:** İlk derleme 30-60 saniye sürebilir

✅ **Çıktı Konumu:** `SystemManager\bin\Release\SystemManager.exe`

✅ **Gereksinimler:** Sadece .NET Framework 4.8

✅ **Windows Sürümü:** Windows 10 veya üzeri önerilir

---

## Detaylı Bilgi

Daha fazla bilgi için: `COMPILE_INSTRUCTIONS.md` dosyasına bakın

---

**Kolay Gelsin! 🚀**
