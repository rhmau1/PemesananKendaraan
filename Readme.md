daftar username - password
admin1 - Admin123
admin2 - Admin123
approver1 -Approver112
approver2 - Approver112

database version
Microsoft SQL Server 2022 (RTM) - 16.0.1000.6 (X64) 

.net version
8.0.100

.net framework version 4.8.9032.0

untuk login POST /Users/login dengan mengirimkan username dan password 

untuk input data kendaraan POST /Vehicles dengan mengirimkan data (hanya bisa dilakukan admin):
type = 0 untuk kendaraan angkut penumpang, 1 untuk kendaraan angkut barang
ownership = 0 untuk kendaraan milik perusahaan, 1 untuk kendaraan sewa
plate number = untuk nomor plat kendaraan
rental price per day = memiliki default 0, apabila kendaraan sewa maka bisa input harganya disini

untuk input data pengemudi POST /Drivers dengan mengirimkan data (hanya bisa dilakukan admin):
nama driver, dan nomor teleponnya

untuk input data pemesanan POST /Bookings dengan mengirimkan data (hanya bisa dilakukan admin):

    {
      "driver_id": 4,
      "vehicle_id": 3,
      "start_booking": "2024-10-20",
      "end_booking": "2024-10-20",
      "approvers": [
        {
          "user_id": 9,
          "approval_level": 1
        },
        {
          "user_id": 10,
          "approval_level": 2
        },
        {
          "user_id": 11,
          "approval_level": 3
        }
      ]
    }


untuk pihak yang menyetujui PUT /Approvals/{id} (hanya bisa dilakukan approver)

untuk melihat data pemakaian kendaraan GET /Vehicles/vehicles-usage

untuk laporan pemesanan yang di export ke excel GET /Bookings/export-booking-report