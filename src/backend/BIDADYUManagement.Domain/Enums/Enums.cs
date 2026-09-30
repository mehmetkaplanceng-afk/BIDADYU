namespace BIDADYUManagement.Domain.Enums;

public enum ComputerStatus
{
    Online = 1,
    Offline = 2,
    Pending = 3,        // Agent pending approval
    Maintenance = 4
}

public enum AgentRegistrationStatus
{
    Pending = 1,
    Approved = 2,
    Rejected = 3
}

public enum DeploymentAction
{
    Install = 1,
    Uninstall = 2,
    Update = 3,
    Repair = 4,
    CopyOnly = 5
}

public enum DeploymentStatus
{
    Pending = 1,
    Queued = 2,
    Running = 3,
    Completed = 4,      // All targets done (may have some failures)
    Failed = 5,         // All targets failed
    Cancelled = 6,
    PartialSuccess = 7  // Some success, some failures
}

public enum JobTargetStatus
{
    Pending = 1,
    WaitingForAgent = 2,
    Queued = 3,
    Downloading = 4,
    Installing = 5,
    Success = 6,
    Failed = 7,
    Timeout = 8,
    Cancelled = 9,
    Skipped = 10
}

public enum InstallerType
{
    Exe = 1,
    Msi = 2,
    Msix = 3,
    Script = 4      // PowerShell / batch
}

public enum Architecture
{
    X86 = 1,
    X64 = 2,
    Arm64 = 3,
    Any = 4
}

public enum NotificationType
{
    Info = 1,
    Warning = 2,
    Error = 3,
    Success = 4,
    Critical = 5
}

public static class Roles
{
    public const string SuperAdmin = "SuperAdmin";
    public const string SystemAdmin = "SystemAdmin";
    public const string ITOperator = "ITOperator";
    public const string Viewer = "Viewer";
}

public static class AuditActions
{
    // Auth
    public const string UserLogin = "USER_LOGIN";
    public const string UserLogout = "USER_LOGOUT";
    public const string UserLoginFailed = "USER_LOGIN_FAILED";

    // User management
    public const string CreateUser = "CREATE_USER";
    public const string UpdateUser = "UPDATE_USER";
    public const string DeleteUser = "DELETE_USER";
    public const string AssignRole = "ASSIGN_ROLE";

    // Software
    public const string CreateSoftware = "CREATE_SOFTWARE";
    public const string UpdateSoftware = "UPDATE_SOFTWARE";
    public const string DeleteSoftware = "DELETE_SOFTWARE";
    public const string CreateSoftwareVersion = "CREATE_SOFTWARE_VERSION";
    public const string UpdateSoftwareVersion = "UPDATE_SOFTWARE_VERSION";
    public const string DeleteSoftwareVersion = "DELETE_SOFTWARE_VERSION";
    public const string UploadPackage = "UPLOAD_PACKAGE";
    public const string DeletePackage = "DELETE_PACKAGE";
    public const string SetCurrentVersion = "SET_CURRENT_VERSION";

    // Computers
    public const string CreateComputer = "CREATE_COMPUTER";
    public const string UpdateComputer = "UPDATE_COMPUTER";
    public const string DeleteComputer = "DELETE_COMPUTER";

    // Agent
    public const string AgentRegistered = "AGENT_REGISTERED";
    public const string AgentApproved = "AGENT_APPROVED";
    public const string AgentRejected = "AGENT_REJECTED";
    public const string AgentDeleted = "AGENT_DELETED";

    // Deployment
    public const string CreateDeployment = "CREATE_DEPLOYMENT";
    public const string CancelDeployment = "CANCEL_DEPLOYMENT";
    public const string RetryDeployment = "RETRY_DEPLOYMENT";
    public const string DeploymentCompleted = "DEPLOYMENT_COMPLETED";

    // Policy
    public const string CreatePolicy = "CREATE_POLICY";
    public const string UpdatePolicy = "UPDATE_POLICY";
    public const string DeletePolicy = "DELETE_POLICY";
}
