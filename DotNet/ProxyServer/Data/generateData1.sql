
if OBJECT_ID('#TmpTest') is not null
begin
	drop table #TmpTest
end

create table #TmpTest
(
	Word nvarchar(128),
	IsValid bit
)

insert into #TmpTest values('baka',1)
insert into #TmpTest values('sldjf',0)
insert into #TmpTest values('kaka',1)
insert into #TmpTest values('faka',1)
insert into #TmpTest values('gomik',0)
insert into #TmpTest values('makaka',1)
insert into #TmpTest values('xbaka',1)
insert into #TmpTest values('xsldjf',0)
insert into #TmpTest values('xkaka',1)
insert into #TmpTest values('xfaka',1)
insert into #TmpTest values('xgomik',0)
insert into #TmpTest values('xmakaka',1)


select * from #TmpTest

drop table #TmpTest
