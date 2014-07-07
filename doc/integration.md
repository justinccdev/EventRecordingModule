# Integration #

## Introduction ##

The expect method of integration with other systems is via the database using
the MySQL plugin.

THESE FORMATS ARE NOT FINAL.  USE WITH CAUTION.

## Database formats ##

The database table formats are as follows

### Events ###

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

This holds user chat events (not instant messages).  The format is

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

# References #

[CHANNEL] - http://wiki.secondlife.com/wiki/Chat_channel

vim: ts=4:sw=4:et:tw=80
