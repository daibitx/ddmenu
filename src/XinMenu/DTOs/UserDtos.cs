namespace XinMenu.DTOs;

#region User DTOs

public class UserProfileDto
{
    public long Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public string Role { get; set; } = string.Empty;
}

public class UpdateUserProfileRequest
{
    public string? AvatarUrl { get; set; }
}

public class ChangePasswordRequest
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

public class UploadAvatarResponse
{
    public string AvatarUrl { get; set; } = string.Empty;
}

#endregion
