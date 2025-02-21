CREATE DATABASE acceloka_db
USE acceloka_db

CREATE TABLE tickets (
    id SERIAL PRIMARY KEY,
    nama_kategori VARCHAR(100) NOT NULL,
    kode_tiket VARCHAR(50) NOT NULL UNIQUE,
    nama_tiket VARCHAR(100) NOT NULL,
    tanggal_event TIMESTAMP NOT NULL,
    harga DECIMAL(10, 2) NOT NULL,
    sisa_quota INT NOT NULL
);

CREATE TABLE booked_tickets (
    id SERIAL PRIMARY KEY,
    kode_tiket VARCHAR(50) NOT NULL,
    quantity INT NOT NULL CHECK (quantity > 0),
    booked_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (kode_tiket) REFERENCES tickets(kode_tiket) ON DELETE CASCADE
);