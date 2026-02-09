param(
    [Parameter(Mandatory=$true)]
    [string]$BackgroundImagePath,
    [Parameter(Mandatory=$true)]
    [string]$DefaultAvatarPath,
    [Parameter(Mandatory=$true)]
    [string]$AvatarDirectory,
    [Parameter(Mandatory=$true)]
    [int]$Count
)

dotnet run "$PSScriptRoot\Yearbook.cs" -- generate-images $BackgroundImagePath $DefaultAvatarPath $AvatarDirectory "$PSScriptRoot\output\generated-recaps" $Count
