# P4P Speech Database - Moa
Govindu Samarasinghe & Rodel Rojos

This project was proposed by the University of Auckland's ECE department. The aim of this project is to support 
speech and language researchers at the University of Auckland with the management of their speech data. The main motivation
is that most researchers resort to using the normal file system which is far too inefficient and tedious, considering that
most speech databases can reach in the thousands of files. This database platform will allow researchers to efficiently
access and organize their speech data and additionally will allow them to map their critical analysis to the corresponding speech data.

## Implementation
The project utilized the following technologies:
- MySQL database engine
    * The DBConnection.cs class is responsible for setting up the project.
    * MoaCore.cs handles most of the access and transfer of data from the business tier and the data tier
- Amazon Web Service Relational Database Service (AWS RDS)
    * In order to interchange the cloud solution, statements in the DBConnection class's constructor needs to be changed to 
    the cloud authentication details of your chosen cloud server.
    * To communicate with the database, the port number 3306 must be used. Inbound and outbound traffic must be allowed on that port
    on your firewall settings, including the AWS firewall.
- WPF (C#)
- Visual Studio 2013 (ide)

The architecture patterns:
- Three tier client server architecture
- Model View ViewModel 

## Features
- Datagrid to see the speech projects
- Datagrid to see the different speakers in the speech project (with their speech data)
- Datagrid to see the analysis associated to each of the speakers
- Normal database operations:
    * Adding speech data files, folder(s) and analysis
    * Deleting the spech data
    * Searching names and demographics
    * Sort by different options
- Generating template file
- Multi-threaded download - the user can download a particular project from he MySQL database into their normal file system in a similar format

## Setting up the development environment

To extend and add features to the application, the following needs to be executed:

- Firstly install MySQL for windows. Follow the instructions below.

http://dev.mysql.com/doc/refman/5.6/en/installing.html

- The recommended IDE is Visual Studio (2013 and above). Firstly clone this repository into a good location in the file system.
Then create a project in Visual Studio using this link: https://msdn.microsoft.com/en-us/library/754c3hy7.aspx

- The github clone url link :https://github.com/Govindalf/p4p.git
    * There are two working builds available. The first version can be accessed by checking out the commit id number
    This version uses an MVC approach and possesses all of the features just mentioned.
    
    * The second version can be accessed by checking out the release-MVVM branch.
    This version uses the more powerful MVVM approach. 

##Moa Operation

After running Moa, the main screen will allow users to perform all required operations.

 - The three datagrids visible in the centre of the screen can be used to navigate around the data, for projects, speech and analysis respectively. When a grid item is clicked, files related to the selected file wil be loaded into the other datagrids.
 - The centre datagrid data is grouped using the values specified in the group combo box, and is by Speaker by default. The centre grid can be grouped by selecting a value from the combo box.
 - The search box can be used to search the speech datagrid using a regular expression filter.
 - The panel of buttons at the top are used to execute various Moa functions and manipulate the data. From left to right:
    * Path settings = choose a folder, which will then be set as the default location for saving projects to. This must be done on the first use, before using the other fucntions.
    * Add folders = select a folder containing speaker files, or a project folder that contains other speaker folders, then upload it to the database. The storage will be done automatically.
    * Add files and delete files = self explanatory, adds speech files to the database, or deletes file(s) selected in the speech datagrid.
    * GFW Extraction = currently unimplemented, future work.
    * Generate Template = opens window to allow the creation of template file similar to Emu. Parameters can be entered, after which it will be automaticall generated.
    * WEBMAUS = link to the WEBMAUS website
    * Add analysis = adds a new analysis file to the database, and links it to a related age group from the speakers. Easily done using popup window.
    





