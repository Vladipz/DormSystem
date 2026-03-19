SELECT 'CREATE DATABASE "auth-db"'
WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'auth-db')\gexec

SELECT 'CREATE DATABASE "events-db"'
WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'events-db')\gexec

SELECT 'CREATE DATABASE "rooms-db"'
WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'rooms-db')\gexec

SELECT 'CREATE DATABASE "inspections-db"'
WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'inspections-db')\gexec

SELECT 'CREATE DATABASE "notifications-db"'
WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'notifications-db')\gexec

SELECT 'CREATE DATABASE "telegram-db"'
WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'telegram-db')\gexec
