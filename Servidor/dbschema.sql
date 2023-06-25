-- Copyright (c) 2023 | Joao Matos, Joao Fernandes, Ruben Lisboa.
-- Check the end of the file for the extended copyright notice.
--
-- This file contains the SQL script used to generate the SQLite database
-- used for the project.
--
-- (SQLite does not support CREATE DATABASE in case you were wondering where that went)

-- USERS TABLE
-- uuid: unique identifier for the user
-- username: username of the user
-- password: hashed password of the user
-- salt: salt used to hash the password
-- isAuthenticated: boolean value to check if the user is authenticated
-- lastAuthentication: datetime of the last time the user was authenticated
-- accountCreation: datetime of the account creation
-- profilePicture: uuid of the profile picture of the user
CREATE TABLE IF NOT EXISTS `users` (
      `uuid` varchar(36) NOT NULL PRIMARY KEY,
      `username` varchar(255) NOT NULL UNIQUE,
      `password` BLOB NOT NULL,                 -- Have to use BLOB here to avoid encoding issues.
      `salt` BLOB NOT NULL,                     -- Same here (SQLite with it's limmited set of data types...)
      `isAuthenticated` boolean NOT NULL DEFAULT 0,
      `lastAuthentication` datetime DEFAULT NULL,
      `accountCreation` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
      `profilePicture` varchar(36) DEFAULT NULL,
      FOREIGN KEY (`profilePicture`) REFERENCES `files`(`uuid`)
);

-- FILES TABLE
-- uuid: unique identifier for the file
-- filename: name of the file
-- filesize: size of the file
-- filedata: blob containing the file data
-- uploadDate: datetime of the file upload
CREATE TABLE IF NOT EXISTS `files` (
      `uuid` varchar(36) NOT NULL PRIMARY KEY,
      `filename` varchar(255) NOT NULL,
      `filesize` INTEGER NOT NULL,
      `filedata` BLOB NOT NULL,
      `uploadDate` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- MESSAGES TABLE
-- id: unique identifier for the message
-- type: type of the message (text, image, file)
-- sender: uuid of the user that sent the message
-- channel: uuid of the channel where the message should be relayed
-- content: content of the message
-- timestamp: datetime of the message creation
-- file: uuid of the file if the message is of type file
--
-- Note: There are no enums in SQLite, so we need that constraint
CREATE TABLE IF NOT EXISTS `messages` (
      `id` INTEGER PRIMARY KEY AUTOINCREMENT,
      `type` TEXT CHECK(`type` IN ('text', 'file')) NOT NULL,
      `sender` varchar(36) NOT NULL,
      `channel` varchar(36) NOT NULL,
      `content` varchar(255) NOT NULL, 
      `timestamp` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
      `file` varchar(36) DEFAULT NULL,
      FOREIGN KEY (`sender`) REFERENCES `users`(`uuid`),
      FOREIGN KEY (`channel`) REFERENCES `channels`(`uuid`),
      FOREIGN KEY (`file`) REFERENCES `files`(`uuid`)
);

-- CHANNELS TABLE
-- uuid: unique identifier for the channel
-- name: name of the channel
-- description: description of the channel
-- creationDate: datetime of the channel creation
-- requestCount: number of requests made to the channel
-- lastRequest: datetime of the last request made to the channel
CREATE TABLE IF NOT EXISTS `channels` (
      `uuid` varchar(36) NOT NULL PRIMARY KEY,
      `name` varchar(255) NOT NULL,
      `description` varchar(255),
      `creationDate` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
      `requestCount` INTEGER NOT NULL DEFAULT '0',
      `lastRequest` datetime DEFAULT NULL
);

-- CHANNELS_USERS TABLE
-- channel: uuid of the channel
-- user: uuid of the user
CREATE TABLE IF NOT EXISTS `channels_users` (
      `id` INTEGER PRIMARY KEY AUTOINCREMENT,
      `channel` varchar(36) NOT NULL,
      `user` varchar(36) NOT NULL,
      FOREIGN KEY (`channel`) REFERENCES `channels`(`uuid`),
      FOREIGN KEY (`user`) REFERENCES `users`(`uuid`)
);

-- MIT License
-- 
-- Copyright (c) 2023 | João Matos, Joao Fernandes, Ruben Lisboa.
-- 
-- Permission is hereby granted, free of charge, to any person obtaining a copy
-- of this software and associated documentation files (the "Software"), to deal
-- in the Software without restriction, including without limitation the rights
-- to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
-- copies of the Software, and to permit persons to whom the Software is
-- furnished to do so, subject to the following conditions:
-- 
-- The above copyright notice and this permission notice shall be included in all
-- copies or substantial portions of the Software.
-- 
-- THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
-- IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
-- FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
-- AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
-- LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
-- OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
-- SOFTWARE.