DO
$$
BEGIN
    IF EXISTS (
        SELECT 1 
        FROM   information_schema.tables 
        WHERE  table_schema = 'public'
        AND    table_name = '__EFMigrationsHistory'
    ) THEN
		CREATE TABLE IF NOT EXISTS gateway."__EFMigrationsHistory" (
			"MigrationId" character varying(150) NOT NULL,
			"ProductVersion" character varying(32) NOT NULL,
		CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId"));
        -- Copy data
        INSERT INTO gateway."__EFMigrationsHistory"
        	SELECT * FROM public."__EFMigrationsHistory";
    END IF;
END
$$;