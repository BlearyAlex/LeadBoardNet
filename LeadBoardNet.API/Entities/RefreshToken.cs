using System.ComponentModel.DataAnnotations;

namespace LeadBoard.Shared.Entities;

public class RefreshToken
{
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = default!;
    public ApplicationUser User { get; set; } = default!;
    
    // Hash (SHA-256) del token en claro. Nunca persistir el token en claro en BD.
    [Required]
    public string TokenHash { get; set; } = default!;

    // Metadatos para control de sesión/dispositivo (recomendado)
    public string? CreatedByIp { get; set; }
    public string? RevokedByIp { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime ExpiresAtUtc { get; set; }

    // Rotación: si se rota, guardamos cuándo se revocó y opcionalmente el motivo.
    public DateTime? RevokedAtUtc { get; set; }
    public string? ReplacedByTokenHash { get; set; }

    public bool IsRevoked => RevokedAtUtc.HasValue;
    public bool IsExpired => DateTime.UtcNow >= ExpiresAtUtc;
    public bool IsActive => !IsRevoked && !IsExpired;
}