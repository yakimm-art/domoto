CREATE DATABASE IF NOT EXISTS taskmanager;
CREATE USER IF NOT EXISTS 'taskmanager_user'@'localhost' IDENTIFIED BY 'manager';
GRANT ALL PRIVILEGES ON taskmanager.* TO 'taskmanager_user'@'localhost';
FLUSH PRIVILEGES;
