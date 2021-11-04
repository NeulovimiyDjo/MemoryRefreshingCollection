DO $$
DECLARE
    r record;
BEGIN
    FOR r IN  (
        SELECT FORMAT(
            'ALTER TABLE %I VALIDATE CONSTRAINT %I;',
            t."ThisTableName",
            t."ForeignKeyName"
        ) AS query_text
        FROM (
            SELECT
                this_table.relname AS "ThisTableName",
                this_table_constraint.conname AS "ForeignKeyName"
            FROM pg_catalog.pg_class this_table
            INNER JOIN pg_catalog.pg_namespace this_table_ns
                ON this_table_ns.oid = this_table.relnamespace
            INNER JOIN pg_catalog.pg_constraint this_table_constraint
                ON this_table_constraint.conrelid = this_table.oid
            INNER JOIN pg_catalog.pg_class referenced_table
                ON referenced_table.oid = this_table_constraint.confrelid
            WHERE this_table.relkind = 'r'
                AND this_table_constraint.contype = 'f'
                AND this_table_ns.nspname IN (select current_schema())
        ) t
    )
    LOOP
        EXECUTE (r.query_text);
    END LOOP;
END $$;
