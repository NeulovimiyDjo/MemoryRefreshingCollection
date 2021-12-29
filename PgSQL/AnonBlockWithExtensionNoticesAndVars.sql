DO $$
DECLARE
  i int = 3;
  r1 int = -1;
  r2 int = -1;
BEGIN
  CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
  i := 4;
  raise notice using message = 'i=' || i;

  EXECUTE 'do $x$'
  ' declare'
  ' x int = 77;'
  ' begin'
  ' insert into "t1" values(uuid_generate_v4(), x);'
  ' end; $x$';

  EXECUTE 'select count(*) from "t1" where "c2" = 77::text;'
  INTO r1;
  raise notice using message = 'r1=' || r1;

  EXECUTE 'select count(*) from "t1" where "c2" = $1;'
  INTO r2
  USING '77';
  raise notice using message = 'r2=' || r2;
END;
$$
--create table t1(c1 uuid, c2 text)
--select * from t1