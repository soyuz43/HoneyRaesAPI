\c HoneyRaes

-- Customers
INSERT INTO Customer (Name, Address) VALUES
('Jane Doe', '123 Elm Street'),
('John Smith', '456 Oak Avenue');

-- Employees
INSERT INTO Employee (Name, Specialty) VALUES
('Alice Johnson', 'Plumbing'),
('Bob Lee', 'Electrical');

-- ServiceTickets
INSERT INTO ServiceTicket (CustomerId, EmployeeId, Description, Emergency, DateCompleted)
VALUES
(1, 1, 'Fix leaking sink', FALSE, NULL),
(2, NULL, 'Install new ceiling fan', TRUE, NULL),
(1, 2, 'Repair outlet in kitchen', FALSE, '2024-12-15 10:30:00');
