# Code Standards

The Health Gateway team has enabled a number of code analysers as well as editor formatters and for the most part we expect each developer to not introduce any warnings into the code.  This document defines defactor and other standards that we want applied but do not enforce through automation.

Any exceptions that need to be introduced should be reviewed be explicity reviewed by the team.  When udpating code that was introduced prior to the introduction of the standard the developer should make best efforts in refactoring to accomadate the standard.

## General

* Configuration is preferred over constant values
* Any code reading more than 3 configuration values should use config model binding
* Controllers call services, services call delegates

## Vue/Frontend Application

## Server Application and Services

* All services must return a RequestResult object
* DB Models should be converted to front end model objects  
  This is due to DB model serialization issues

* Controllers should not throw exceptions to clients

## Backend

### Entity Framework

* EF Queries are to be expressed as Lambda expressions
* DB Delegates methods that write to the DB should always optionally commit
* DB Delegates should always return a DBResult object
* Any query expecting a resultset larger than 1000 records should support paging

### Batch Jobs
