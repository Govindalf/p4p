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
    FID int    NOT NULL ,
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





-- foreign keys
-- Reference:  File2Analysis_Analysis (table: File2Analysis)


ALTER TABLE File2Analysis ADD CONSTRAINT File2Analysis_Analysis FOREIGN KEY File2Analysis_Analysis (Analysis_AID)
    REFERENCES Analysis (AID);
-- Reference:  File2Analysis_File (table: File2Analysis)


ALTER TABLE File2Analysis ADD CONSTRAINT File2Analysis_File FOREIGN KEY File2Analysis_File (File_FID)
    REFERENCES File (FID);
-- Reference:  FileData_File (table: FileData)


ALTER TABLE FileData ADD CONSTRAINT FileData_File FOREIGN KEY FileData_File (FID)
    REFERENCES File (FID);
-- Reference:  File_Project (table: File)


ALTER TABLE File ADD CONSTRAINT File_Project FOREIGN KEY File_Project (PID)
    REFERENCES Project (PID);



-- End of file.

