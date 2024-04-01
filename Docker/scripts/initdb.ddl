IF NOT EXISTS(SELECT * FROM sys.databases WHERE name = 'bamboozlers')
    BEGIN
        CREATE DATABASE [bamboozlers]
    END
GO
USE [bamboozlers]
GO