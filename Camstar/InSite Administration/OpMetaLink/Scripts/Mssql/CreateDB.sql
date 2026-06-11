USE [master]
GO

DECLARE @DatabaseName NVARCHAR(128);
DECLARE @FilePath NVARCHAR(260);
SET @DatabaseName = 'TEST'; -- Replace with your desired database name
SET @FilePath = 'C:\Program Files\Microsoft SQL Server\MSSQL16.MSSQLSERVER\MSSQL\DATA\'; -- Replace with your database path

DECLARE @SQL NVARCHAR(MAX);

-- Create Database
SET @SQL = '
    CREATE DATABASE [' + @DatabaseName + ']
    ON PRIMARY (
        NAME = [' + @DatabaseName + '_data],
        FILENAME = ''' + @FilePath + @DatabaseName + '_data.mdf'',
        SIZE = 8192KB,
        MAXSIZE = UNLIMITED,
        FILEGROWTH = 65536KB
    )
    LOG ON (
        NAME = [' + @DatabaseName + '_log],
        FILENAME = ''' + @FilePath + @DatabaseName + '_log.ldf'',
        SIZE = 8192KB,
        MAXSIZE = 2048GB,
        FILEGROWTH = 65536KB
    )';

BEGIN TRY
    EXEC sp_executesql @SQL;
    PRINT 'Database ' + @DatabaseName + ' created successfully.';
END TRY
BEGIN CATCH
    PRINT 'Error occurred: ' + ERROR_MESSAGE();
END CATCH;

-- Enable Full-Text Search if installed
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
BEGIN
    SET @SQL = 'EXEC [' + @DatabaseName + '].[dbo].[sp_fulltext_database] @action = ''enable'';';
    EXEC sp_executesql @SQL;
END

-- ANSI Settings
SET @SQL = 'ALTER DATABASE [' + @DatabaseName + '] SET ANSI_NULL_DEFAULT OFF; 
            ALTER DATABASE [' + @DatabaseName + '] SET ANSI_NULLS OFF; 
            ALTER DATABASE [' + @DatabaseName + '] SET ANSI_PADDING OFF; 
            ALTER DATABASE [' + @DatabaseName + '] SET ANSI_WARNINGS OFF;';
EXEC sp_executesql @SQL;

-- Transaction Settings
SET @SQL = 'ALTER DATABASE [' + @DatabaseName + '] SET ARITHABORT OFF; 
            ALTER DATABASE [' + @DatabaseName + '] SET AUTO_CLOSE OFF; 
            ALTER DATABASE [' + @DatabaseName + '] SET AUTO_SHRINK OFF; 
            ALTER DATABASE [' + @DatabaseName + '] SET AUTO_UPDATE_STATISTICS ON; 
            ALTER DATABASE [' + @DatabaseName + '] SET CURSOR_CLOSE_ON_COMMIT OFF; 
            ALTER DATABASE [' + @DatabaseName + '] SET CURSOR_DEFAULT GLOBAL; 
            ALTER DATABASE [' + @DatabaseName + '] SET CONCAT_NULL_YIELDS_NULL OFF; 
            ALTER DATABASE [' + @DatabaseName + '] SET NUMERIC_ROUNDABORT OFF; 
            ALTER DATABASE [' + @DatabaseName + '] SET QUOTED_IDENTIFIER OFF; 
            ALTER DATABASE [' + @DatabaseName + '] SET RECURSIVE_TRIGGERS OFF; 
            ALTER DATABASE [' + @DatabaseName + '] SET DISABLE_BROKER; 
            ALTER DATABASE [' + @DatabaseName + '] SET AUTO_UPDATE_STATISTICS_ASYNC OFF; 
            ALTER DATABASE [' + @DatabaseName + '] SET DATE_CORRELATION_OPTIMIZATION OFF; 
            ALTER DATABASE [' + @DatabaseName + '] SET TRUSTWORTHY OFF; 
            ALTER DATABASE [' + @DatabaseName + '] SET ALLOW_SNAPSHOT_ISOLATION OFF; 
            ALTER DATABASE [' + @DatabaseName + '] SET PARAMETERIZATION SIMPLE; 
            ALTER DATABASE [' + @DatabaseName + '] SET READ_COMMITTED_SNAPSHOT OFF; 
            ALTER DATABASE [' + @DatabaseName + '] SET HONOR_BROKER_PRIORITY OFF;';
EXEC sp_executesql @SQL;

-- Recovery Settings
SET @SQL = 'ALTER DATABASE [' + @DatabaseName + '] SET RECOVERY FULL;';
EXEC sp_executesql @SQL;

-- User and Durability Settings
SET @SQL = 'ALTER DATABASE [' + @DatabaseName + '] SET MULTI_USER; 
            ALTER DATABASE [' + @DatabaseName + '] SET DELAYED_DURABILITY = DISABLED; 
            ALTER DATABASE [' + @DatabaseName + '] SET ACCELERATED_DATABASE_RECOVERY = OFF;';
EXEC sp_executesql @SQL;

-- Filegroup and Stream Settings
SET @SQL = 'ALTER DATABASE [' + @DatabaseName + '] SET PAGE_VERIFY CHECKSUM; 
            ALTER DATABASE [' + @DatabaseName + '] SET DB_CHAINING OFF; 
            ALTER DATABASE [' + @DatabaseName + '] SET FILESTREAM(NON_TRANSACTED_ACCESS = OFF);';
EXEC sp_executesql @SQL;

-- Performance and Query Store Settings
SET @SQL = 'ALTER DATABASE [' + @DatabaseName + '] SET TARGET_RECOVERY_TIME = 60 SECONDS; 
            ALTER DATABASE [' + @DatabaseName + '] SET QUERY_STORE = OFF;';
EXEC sp_executesql @SQL;

-- Final Settings
SET @SQL = 'ALTER DATABASE [' + @DatabaseName + '] SET READ_WRITE;';
EXEC sp_executesql @SQL;
GO
