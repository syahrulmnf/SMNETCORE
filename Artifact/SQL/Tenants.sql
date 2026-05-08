CREATE TABLE Tenants (
    Id UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
    Name varchar(250) NOT NULL,
    Subdomain varchar(250) NOT NULL,
    ConnectionString varchar(Max) NOT NULL,
    JwtKey varchar(max) NOT NULL,
    JwtIssuer varchar(max) NOT NULL,
    JwtAudience varchar(max) NOT NULL
);