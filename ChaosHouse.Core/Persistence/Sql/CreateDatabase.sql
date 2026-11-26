
IF DB_ID(N'ChaosHouseDb') IS NULL
BEGIN
    CREATE DATABASE ChaosHouseDb;
END;
GO

USE ChaosHouseDb;
GO

IF OBJECT_ID(N'dbo.HouseMember', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.HouseMember
    (
        Id NVARCHAR(100) NOT NULL PRIMARY KEY,
        Name NVARCHAR(200) NOT NULL,
        TypeName NVARCHAR(100) NOT NULL,
        Energy INT NOT NULL,
        SpookinessLevel INT NULL,
        CurrentGadget NVARCHAR(200) NULL,
        IsLocked BIT NULL,

        CONSTRAINT CK_HouseMember_Energy
            CHECK (Energy BETWEEN 0 AND 100)
    );
END;
GO

IF OBJECT_ID(N'dbo.HouseGroup', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.HouseGroup
    (
        Id NVARCHAR(100) NOT NULL PRIMARY KEY,
        Name NVARCHAR(200) NOT NULL
    );
END;
GO

IF OBJECT_ID(N'dbo.GroupMember', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.GroupMember
    (
        GroupId NVARCHAR(100) NOT NULL,
        MemberId NVARCHAR(100) NOT NULL,

        CONSTRAINT PK_GroupMember
            PRIMARY KEY (GroupId, MemberId),

        CONSTRAINT FK_GroupMember_Group
            FOREIGN KEY (GroupId)
            REFERENCES dbo.HouseGroup(Id)
            ON DELETE CASCADE,

        CONSTRAINT FK_GroupMember_Member
            FOREIGN KEY (MemberId)
            REFERENCES dbo.HouseMember(Id)
            ON DELETE CASCADE
    );
END;
GO

