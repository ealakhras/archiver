﻿CREATE TABLE [dbo].[options] (
    [code]         NVARCHAR (50)  NOT NULL,
    [name]         NVARCHAR (250) NOT NULL,
    [value]        NVARCHAR (250) NULL,
    [description]  NVARCHAR (250) NULL,
    [creator]      NVARCHAR (150) CONSTRAINT [DF_options_creator] DEFAULT (suser_sname()) NOT NULL,
    [creationDate] DATETIME       CONSTRAINT [DF_options_creationDate] DEFAULT (getdate()) NULL,
    CONSTRAINT [PK_options] PRIMARY KEY CLUSTERED ([code] ASC)
);



