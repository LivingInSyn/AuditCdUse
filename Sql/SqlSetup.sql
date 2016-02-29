#NOT INCLUDED: 
#	creating a user with insert rights to this database

CREATE DATABASE IF NOT EXIST cddb;
use cddb;

CREATE TABLE IF NOT EXIST cd_events(
EventId int unsigned auto_increment primary key,
MachineName varchar(40),
EventDateTime DateTime,
UserName varchar(40),
CdTitle varchar(255)
);

