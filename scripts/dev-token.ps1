<#
.SYNOPSIS
  Mints a dev JWT (HS256) for trying the API locally.
.USAGE
  powershell -File scripts/dev-token.ps1 -UserId "<guid>"
  powershell -File scripts/dev-token.ps1 -UserId "<guid>" -SigningKey "<your dev key>"
#>
param(
    [Parameter(Mandatory = $true)][string]$UserId,
    [string]$SigningKey = "dev-signing-key-0123456789abcdef0123456789abcdef",
    [string]$Issuer = "coregym-dev",
    [string]$Audience = "coregym-api"
)

function b64url([byte[]]$bytes) {
    [Convert]::ToBase64String($bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_')
}

$header = b64url ([System.Text.Encoding]::UTF8.GetBytes('{"alg":"HS256","typ":"JWT"}'))
$claims = @{ sub = $UserId; iss = $Issuer; aud = $Audience; exp = [DateTimeOffset]::UtcNow.AddHours(8).ToUnixTimeSeconds() }
$payload = b64url ([System.Text.Encoding]::UTF8.GetBytes(($claims | ConvertTo-Json -Compress)))
$hmac = [System.Security.Cryptography.HMACSHA256]::new([System.Text.Encoding]::UTF8.GetBytes($SigningKey))
$signature = b64url ($hmac.ComputeHash([System.Text.Encoding]::UTF8.GetBytes("$header.$payload")))

Write-Output "$header.$payload.$signature"
