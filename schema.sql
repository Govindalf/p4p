-- Created by Vertabelo (http://vertabelo.com)
-- Last modification date: 2015-09-06 08:59:26.299




-- tables
-- Table Analysis
CREATE TABLE Analysis (
    AID varchar(255)    NOT NULL ,
    Description varchar(255) ,
    FileData mediumblob NOT NULL,
	FileType varchar(255) , 	
    CONSTRAINT Analysis_pk PRIMARY KEY (AID)
);

-- Table File
CREATE TABLE File (
    FID int    NOT NULL AUTO_INCREMENT,
    PID varchar(255)    NOT NULL ,
    Name varchar(255)    NOT NULL ,
    FileType varchar(255)    NOT NULL ,
    Speaker varchar(255)    NOT NULL ,
    CONSTRAINT File_pk PRIMARY KEY (FID)
);

-- Table File2Analysis
CREATE TABLE File2Analysis (
    File_FID int    NOT NULL ,
    Analysis_AID varchar(255)    NOT NULL ,
	
    CONSTRAINT File2Analysis_pk PRIMARY KEY (File_FID,Analysis_AID)
);

-- Table FileData
CREATE TABLE FileData (
    FID int    NOT NULL ,
    FileData mediumblob    NOT NULL ,
    CONSTRAINT FileData_pk PRIMARY KEY (FID)
);

-- Table Project
CREATE TABLE Project (
    PID varchar(255)    NOT NULL ,
    DateCreated date    NOT NULL ,
    Description varchar(500) ,
    CONSTRAINT Project_pk PRIMARY KEY (PID)
);






