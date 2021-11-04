
CREATE TABLE [FIAS_TABLE]
(
    [ID] [int] IDENTITY(1,1) NOT NULL,
    [FORMALNAME] [varchar](50) NOT NULL,
    [AOLEVEL] [varchar](10) NOT NULL,
    [PLAINCODE] [varchar](50) NOT NULL,
	CONSTRAINT [FIAS_PK] PRIMARY KEY ([ID])
)


DROP TABLE [FIAS_TABLE]


select * from [FIAS_TABLE]

delete from [FIAS_TABLE]


-- Very slow, use python script
INSERT INTO [FIAS_TABLE] (FORMALNAME, AOLEVEL, PLAINCODE)
SELECT DISTINCT
   MY_XML.obj.value('@FORMALNAME', 'VARCHAR(50)'),
   MY_XML.obj.value('@AOLEVEL', 'VARCHAR(10)'),
   MY_XML.obj.value('@PLAINCODE', 'VARCHAR(50)')
FROM
	(
		SELECT CAST(MY_XML1 AS xml)
		FROM OPENROWSET(BULK 'C:\tmp\fias_filtered1.xml', SINGLE_BLOB) AS T1(MY_XML1)
	) AS T(MY_XML)
    CROSS APPLY MY_XML.nodes('/AddressObjects/Object') AS MY_XML(obj);
