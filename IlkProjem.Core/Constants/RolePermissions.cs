using System.Collections.Generic;

namespace IlkProjem.Core.Constants;

public static class RolePermissions
{
    private static readonly Dictionary<string, List<string>> _rolePermissions = new()
    {
        [Roles.Admin] = new List<string>
        {
            Permissions.Customers.View, Permissions.Customers.Create, Permissions.Customers.Edit, Permissions.Customers.Delete,
            Permissions.Users.View, Permissions.Users.Create, Permissions.Users.Edit, Permissions.Users.Delete,
            Permissions.Files.View, Permissions.Files.Upload, Permissions.Files.Delete,
            Permissions.System.Manage,
            Permissions.Workflows.View, Permissions.Workflows.Create, Permissions.Workflows.Approve
        },
        [Roles.Manager] = new List<string>
        {
            Permissions.Customers.View, Permissions.Customers.Create, Permissions.Customers.Edit,
            Permissions.Files.View, Permissions.Files.Upload,
            Permissions.Workflows.View, Permissions.Workflows.Create, Permissions.Workflows.Approve
        },
        [Roles.Staff] = new List<string>
        {
            Permissions.Customers.View,
            Permissions.Files.View, Permissions.Files.Upload,
            Permissions.Workflows.View, Permissions.Workflows.Create
        },
        [Roles.Guest] = new List<string>
        {
            Permissions.Customers.View
        }
    };

    public static List<string> GetPermissionsForRole(string role)
    {
        if (string.IsNullOrEmpty(role)) return new List<string>();
        return _rolePermissions.TryGetValue(role, out var permissions) ? permissions : new List<string>();
    }
}
