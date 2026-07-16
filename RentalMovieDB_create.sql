-- tables
-- Table: Customer
CREATE TABLE Customer (
    customer_id int  NOT NULL IDENTITY(1,1),
    first_name nvarchar(100)  NOT NULL,
    last_name nvarchar(200)  NOT NULL,
    CONSTRAINT Customer_pk PRIMARY KEY  (customer_id)
);

-- Table: Movie
CREATE TABLE Movie (
    movie_id int  NOT NULL IDENTITY(1,1),
    title nvarchar(200)  NOT NULL,
    release_date datetime  NOT NULL,
    price_per_day decimal(10,2)  NOT NULL,
    CONSTRAINT Movie_pk PRIMARY KEY  (movie_id)
);

-- Table: Rental
CREATE TABLE Rental (
    rental_id int  NOT NULL IDENTITY(1,1),
    rental_date datetime  NOT NULL,
    return_date datetime  NULL,
    customer_id int  NOT NULL,
    status_id int  NOT NULL,
    CONSTRAINT Rental_pk PRIMARY KEY  (rental_id)
);

-- Table: Rental_Item
CREATE TABLE Rental_Item (
    rental_id int  NOT NULL,
    movie_id int  NOT NULL,
    price_at_rental decimal(10,2)  NOT NULL,
    CONSTRAINT Rental_Item_pk PRIMARY KEY  (rental_id,movie_id)
);

-- Table: Status
CREATE TABLE Status (
    status_id int  NOT NULL IDENTITY(1,1),
    name nvarchar(200)  NOT NULL,
    CONSTRAINT Status_pk PRIMARY KEY  (status_id)
);

-- foreign keys
-- Reference: Rental_Customer (table: Rental)
ALTER TABLE Rental ADD CONSTRAINT Rental_Customer
    FOREIGN KEY (customer_id)
    REFERENCES Customer (customer_id);

-- Reference: Rental_Item_Movie (table: Rental_Item)
ALTER TABLE Rental_Item ADD CONSTRAINT Rental_Item_Movie
    FOREIGN KEY (movie_id)
    REFERENCES Movie (movie_id);

-- Reference: Rental_Item_Rental (table: Rental_Item)
ALTER TABLE Rental_Item ADD CONSTRAINT Rental_Item_Rental
    FOREIGN KEY (rental_id)
    REFERENCES Rental (rental_id);

-- Reference: Rental_Status (table: Rental)
ALTER TABLE Rental ADD CONSTRAINT Rental_Status
    FOREIGN KEY (status_id)
    REFERENCES Status (status_id);

-- End of file.

-- Insert data into Status
INSERT INTO Status (name) VALUES
(N'Rented'),
(N'Returned'),
(N'Late');

-- Insert data into Customer
INSERT INTO Customer (first_name, last_name) VALUES
(N'Alice', N'Johnson'),
(N'Bob', N'Smith'),
(N'Charlie', N'Davis');

-- Insert data into Movie
INSERT INTO Movie (title, release_date, price_per_day) VALUES
(N'Inception', '2010-07-16', 3.99),
(N'The Matrix', '1999-03-31', 2.99),
(N'Interstellar', '2014-11-07', 4.49),
(N'The Godfather', '1972-03-24', 2.49),
(N'Avengers: Endgame', '2019-04-26', 4.99);

-- Insert data into Rental
INSERT INTO Rental (rental_date, return_date, customer_id, status_id) VALUES
('2025-04-25T10:00:00', '2025-04-28T15:30:00', 1, 2),
('2025-05-01T14:00:00', NULL, 2, 1),
('2025-04-30T18:45:00', '2025-05-03T10:00:00', 3, 2),
('2025-05-03T12:15:00', NULL, 1, 1);

-- Insert data into Rental_Item
INSERT INTO Rental_Item (rental_id, movie_id, price_at_rental) VALUES
(1, 1, 3.99),
(1, 2, 2.99),
(2, 3, 4.49),
(3, 4, 2.49),
(4, 5, 4.99);