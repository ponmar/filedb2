﻿namespace FileDB.Migrators;

public interface IMigrator<T>
{
    public T Migrate(T config, T defaultValues);
}
