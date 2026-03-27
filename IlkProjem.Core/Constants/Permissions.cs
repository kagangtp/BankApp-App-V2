namespace IlkProjem.Core.Constants;

/// <summary>
/// Sistemdeki tüm granüler yetkilerin (permissions) listesi.
/// </summary>
public static class Permissions
{
    public static class Customers
    {
        public const string View = "Customers.View";
        public const string Create = "Customers.Create";
        public const string Edit = "Customers.Edit";
        public const string Delete = "Customers.Delete";
    }

    public static class Users
    {
        public const string View = "Users.View";
        public const string Create = "Users.Create";
        public const string Edit = "Users.Edit";
        public const string Delete = "Users.Delete";
    }

    public static class Files
    {
        public const string Upload = "Files.Upload";
        public const string Delete = "Files.Delete";
        public const string View = "Files.View";
    }

    public static class System
    {
        public const string Manage = "System.Manage";
    }
}