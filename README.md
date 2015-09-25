# P4P Speech Database - Moa

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
    This version uses the MVVM approach. When extending the application, this branch should
      ** Current Bugs: have to double click projects initially (doesn't affect the rest of the app)
    





