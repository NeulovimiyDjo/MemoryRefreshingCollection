DO $$
DECLARE
    r record;
BEGIN
    FOR r IN  (
        SELECT FORMAT(
            'ALTER TABLE %I ADD CONSTRAINT %I FOREIGN KEY (%s) REFERENCES %I (%s) ON DELETE %s ON UPDATE %s NOT VALID;',
            t."ThisTableName",
            t."ForeignKeyName",
            t."ThisColumnNames",
            t."ReferencedTableName",
            t."ReferencedColumnNames",
            t."OnDelete",
            t."OnUpdate"
        ) AS query_text
        FROM "_ForeignKeysToDisable" t
    )
    LOOP
        EXECUTE (r.query_text);
    END LOOP;

    DROP TABLE "_ForeignKeysToDisable";
END $$;
