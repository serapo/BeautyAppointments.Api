# Beauty Appointments API 💇‍♀️💅

Bu proje, backend tarafında kendimi geliştirmek için yaptığım ilk API çalışmalarından biridir.
Amaç, bir güzellik merkezi için **randevu yönetim sistemi** geliştirmektir.  

## 🚀 Teknolojiler
- .NET 8 (ASP.NET Core Web API)
- Entity Framework Core (SQLite)
- Swagger / OpenAPI
- Docker (Render deploy için)

## 📂 Özellikler
- **CustomersController** → müşteri CRUD işlemleri
- **ServicesController** → hizmet CRUD işlemleri
- **AppointmentsController** → randevu oluşturma, güncelleme, iptal etme, silme
- **Seed Data** → başlangıçta örnek müşteri, hizmet ve randevu verileri

## 🔗 Canlı Demo
- **Swagger UI:** [https://beautyappointments-api.onrender.com/swagger](https://beautyappointments-api.onrender.com/swagger)  
- **Health Check:** [https://beautyappointments-api.onrender.com/](https://beautyappointments-api.onrender.com/)

## 🛠️ Lokal Çalıştırma
```bash
git clone https://github.com/<kullanici-adi>/beauty-appointments-api.git
cd beauty-appointments-api
dotnet run
