# Integration #

## Introduction ##

The expect method of integration with other systems is via the database using
the MySQL plugin.

THESE FORMATS ARE NOT FINAL.  USE WITH CAUTION.

## Database formats ##

The database table formats are as follows

### UserRegionEvents ###

This holds user login, logout and region entrance events (not triggered on
initial login region).  The format is

+----------+-------------+------+-----+---------+----------------+
| Field    | Type        | Null | Key | Default | Extra          |
+----------+-------------+------+-----+---------+----------------+
| Id       | int(11)     | NO   | PRI | NULL    | auto_increment |
| UserId   | varchar(80) | NO   |     | NULL    |                |
| UserName | varchar(80) | NO   |     | NULL    |                |
| Type     | varchar(10) | NO   |     | NULL    |                |
| Region   | varchar(80) | NO   |     | NULL    |                |
| DateTime | datetime    | NO   |     | NULL    |                |
| GridId   | varchar(36) | NO   |     | NULL    |                |
+----------+-------------+------+-----+---------+----------------+

Type is one of "login", "logout", or "enter".

### UserChatEvents ###

This holds user chat events.  The format is

+----------+-------------+------+-----+---------+----------------+
| Field    | Type        | Null | Key | Default | Extra          |
+----------+-------------+------+-----+---------+----------------+
| Id       | int(11)     | NO   | PRI | NULL    | auto_increment |
| UserId   | varchar(80) | NO   |     | NULL    |                |
| UserName | varchar(80) | NO   |     | NULL    |                |
| OriginX  | float       | NO   |     | NULL    |                |
| OriginY  | float       | NO   |     | NULL    |                |
| OriginZ  | float       | NO   |     | NULL    |                |
| Type     | varchar(10) | NO   |     | NULL    |                |
| Text     | text        | NO   |     | NULL    |                |
| Channel  | int(11)     | NO   |     | NULL    |                |
| Region   | varchar(80) | NO   |     | NULL    |                |
| DateTime | datetime    | NO   |     | NULL    |                |
| GridId   | varchar(36) | NO   |     | NULL    |                |
+----------+-------------+------+-----+---------+----------------+

Type is one of "whisper", "say", or "shout".

Channel corresponds to the Second Life protocol channel [CHANNEL].

### UserImEvents ###

This holds user instant message events.  The format is

+--------------+-------------+------+-----+---------+----------------+
| Field        | Type        | Null | Key | Default | Extra          |
+--------------+-------------+------+-----+---------+----------------+
| Id           | int(11)     | NO   | PRI | NULL    | auto_increment |
| UserId       | varchar(36) | NO   |     | NULL    |                |
| UserName     | varchar(80) | NO   |     | NULL    |                |
| ReceiverId   | varchar(36) | NO   |     | NULL    |                |
| ReceiverName | varchar(80) | NO   |     | NULL    |                |
| ReceiverType | varchar(10) | NO   |     | NULL    |                |
| Text         | text        | NO   |     | NULL    |                |
| Region       | varchar(80) | NO   |     | NULL    |                |
| DateTime     | datetime    | NO   |     | NULL    |                |
| GridId       | varchar(36) | NO   |     | NULL    |                |
+--------------+-------------+------+-----+---------+----------------+

ReceiverType is one of "user" or "group" and signifies whether the instant
message was sent to another user or to a group.

# References #

[CHANNEL] - http://wiki.secondlife.com/wiki/Chat_channel

vim: ts=4:sw=4:et:tw=80
