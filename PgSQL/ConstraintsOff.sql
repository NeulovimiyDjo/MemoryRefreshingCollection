DO $$
DECLARE
    r record;
BEGIN
    CREATE TABLE "_ForeignKeysToDisable" (
        "ThisTableName" text NOT NULL,
        "ForeignKeyName" text NOT NULL,
        "ThisColumnNames" text NOT NULL,
        "ReferencedTableName" text NOT NULL,
        "ReferencedColumnNames" text NOT NULL,
        "OnDelete" text NOT NULL,
        "OnUpdate" text NOT NULL
    );

    INSERT INTO "_ForeignKeysToDisable" (
        "ThisTableName",
        "ForeignKeyName",
        "ThisColumnNames",
        "ReferencedTableName",
        "ReferencedColumnNames",
        "OnDelete",
        "OnUpdate"
    )
    SELECT
        this_table.relname AS "ThisTableName",
        this_table_constraint.conname AS "ForeignKeyName",
        (
            SELECT string_agg('"' || u.column_name::text || '"', ',')
            FROM (
                SELECT this_columns.attname
                FROM (
                    SELECT ROW_NUMBER() OVER(), *
                    FROM UNNEST(this_table_constraint.conkey)
                ) this_columns_positions(col_pos, col_num)
                INNER JOIN pg_catalog.pg_attribute this_columns
                    ON this_columns.attrelid = this_table.oid
                        AND this_columns.attnum = this_columns_positions.col_num
                        AND NOT this_columns.attisdropped
                ORDER BY this_columns_positions.col_pos
            ) u(column_name)
        ) AS "ThisColumnNames",
        referenced_table.relname AS "ReferencedTableName",
        (
            SELECT string_agg('"' || u.column_name::text || '"', ',')
            FROM (
                SELECT referenced_columns.attname
                FROM (
                    SELECT ROW_NUMBER() OVER(), *
                    FROM UNNEST(this_table_constraint.conkey)
                ) this_columns_positions(col_pos, col_num)
                INNER JOIN LATERAL (
                    SELECT ROW_NUMBER() OVER(), *
                    FROM UNNEST(this_table_constraint.confkey)
                ) referenced_columns_positions(col_pos, col_num)
                    ON referenced_columns_positions.col_pos = this_columns_positions.col_pos
                INNER JOIN pg_catalog.pg_attribute referenced_columns
                    ON referenced_columns.attrelid = referenced_table.oid
                        AND referenced_columns.attnum = referenced_columns_positions.col_num
                        AND NOT referenced_columns.attisdropped
                ORDER BY referenced_columns_positions.col_pos
            ) u(column_name)
        ) AS "ReferencedColumnNames",
        CASE
            WHEN this_table_constraint.confdeltype = 'a' THEN 'NO ACTION'
            WHEN this_table_constraint.confdeltype = 'r' THEN 'RESTRICT'
            WHEN this_table_constraint.confdeltype = 'c' THEN 'CASCADE'
            WHEN this_table_constraint.confdeltype = 'd' THEN 'SET DEFAULT'
            WHEN this_table_constraint.confdeltype = 'n' THEN 'SET NULL'
        END AS "OnDelete",
        CASE
            WHEN this_table_constraint.confupdtype = 'a' THEN 'NO ACTION'
            WHEN this_table_constraint.confupdtype = 'r' THEN 'RESTRICT'
            WHEN this_table_constraint.confupdtype = 'c' THEN 'CASCADE'
            WHEN this_table_constraint.confupdtype = 'd' THEN 'SET DEFAULT'
            WHEN this_table_constraint.confupdtype = 'n' THEN 'SET NULL'
        END AS "OnUpdate"
    FROM pg_catalog.pg_class this_table
    INNER JOIN pg_catalog.pg_namespace this_table_ns
        ON this_table_ns.oid = this_table.relnamespace
    INNER JOIN pg_catalog.pg_constraint this_table_constraint
        ON this_table_constraint.conrelid = this_table.oid
    INNER JOIN pg_catalog.pg_class referenced_table
        ON referenced_table.oid = this_table_constraint.confrelid
    WHERE this_table.relkind = 'r'
        AND this_table_constraint.contype = 'f'
        AND this_table_ns.nspname IN (select current_schema());

    FOR r IN  (
        SELECT FORMAT(
            'ALTER TABLE %I DROP CONSTRAINT %I;',
            t."ThisTableName",
            t."ForeignKeyName"
        ) AS query_text
        FROM "_ForeignKeysToDisable" t
    )
    LOOP
        EXECUTE (r.query_text);
    END LOOP;
END $$;
