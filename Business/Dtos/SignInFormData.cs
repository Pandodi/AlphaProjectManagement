﻿namespace Business.Dtos;

public class SignInFormData
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
    public bool RememberMe { get; set; }
}
