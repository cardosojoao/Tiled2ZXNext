# Run with PowerShell on .NET 10 after dotnet build.
$ErrorActionPreference = 'Stop'
$assemblyDir = Join-Path $PSScriptRoot '../bin/Debug/net10.0'
Add-Type -Path (Join-Path $assemblyDir 'TiledIO.dll')
Add-Type -Path (Join-Path $assemblyDir 'Tiled2dot8.dll')

function New-Property($name, $value) {
    $json = ConvertTo-Json -InputObject $value -Compress
    [TiledIO.Entities.Property]@{
        Name = $name
        Type = $(if ($value -is [string]) { 'string' } else { 'int' })
        Value = [System.Text.Json.JsonDocument]::Parse($json).RootElement.Clone()
    }
}

function New-Layer($name) {
    [TiledIO.Entities.Layer]@{
        Name = $name; Visible = $true; Type = 'objectgroup'
        Objects = [System.Collections.Generic.List[TiledIO.Entities.Object]]::new()
        Layers = [System.Collections.Generic.List[TiledIO.Entities.Layer]]::new()
        Properties = [System.Collections.Generic.List[TiledIO.Entities.Property]]::new()
    }
}

function Assert-Contains($output, $expected) {
    if (!$output.Contains($expected)) { throw "Missing '$expected' in:`n$output" }
}

$scene = [TiledIO.Entities.Scene]::new()
$sheet = [TiledIO.Entities.Tileset]@{
    Firstgid = 1; Lastgid = 32
    Properties = @(New-Property 'Name' 'sheet')
    Tiles = @([TiledIO.Entities.TileSetTile]@{
        ID = 7; Properties = @(New-Property 'Name' 'tile')
    })
}
$scene.Tilesets.Add($sheet)
$root = New-Layer 'static'
$group = New-Layer 'sprites'
$group.Properties.Add((New-Property 'Type' 1))
$root.Layers.Add($group)
# An object class called objectgroup is not a layer and must not recurse.
$group.Objects.Add([TiledIO.Entities.Object]@{
    Id = 1; Visible = $true; Type = 'objectgroup'; Gid = 8
})
$group.Objects.Add([TiledIO.Entities.Object]@{
    Id = 2; Visible = $true; Type = 'marker'
})
$child = New-Layer 'nested'
$child.Objects.Add([TiledIO.Entities.Object]@{
    Id = 3; Visible = $true; Type = 'Decoration'; Gid = 2
})
$child.Objects.Add([TiledIO.Entities.Object]@{
    Id = 4; Visible = $false; Gid = 8
})
$group.Layers.Add($child)
$hidden = New-Layer 'hidden'
$hidden.Visible = $false
$hidden.Objects.Add([TiledIO.Entities.Object]@{ Id = 5; Visible = $true; Gid = 8 })
$group.Layers.Add($hidden)
$output = [Tiled2dot8.ProcessSpriteStatic]::new($root, $scene, $null).Execute().ToString()
Assert-Contains $output "db `$02`t`t; Objects count"
Assert-Contains $output "dw `$000D`t`t; Block size"
Assert-Contains $output 'TILE_PATTERN_ID'
Assert-Contains $output 'SHEET_PATTERN_ID'
if ([regex]::Matches($output, ';\s+Sprite Static -').Count -ne 2) {
    throw 'Expected exactly two static sprite records.'
}

[Tiled2dot8.Controller]::Config.Offset = [Tiled2dot8.Offset]::new()
function Invoke-Component($gid, $properties) {
    $layer = New-Layer 'components'
    $layer.Properties.Add((New-Property 'Type' 17))
    $entity = New-Layer 'entity'
    $entity.Objects.Add([TiledIO.Entities.Object]@{
        Id = 10; Visible = $true; Type = 'SpriteComponent'
        Gid = $gid; Properties = $properties
    })
    $layer.Layers.Add($entity)
    [Tiled2dot8.Process.EEI.ProcessObjectComponents]::new($layer, $scene, $null).Execute().ToString()
}

foreach ($offset in @(@(8, 0), @(0, -4), @(8, -4), @(0, 0))) {
    $output = Invoke-Component $null @(
        (New-Property 'SpriteName' 'legacy')
        (New-Property 'SpriteIndex' 0)
        (New-Property 'OffSetX' $offset[0])
        (New-Property 'OffSetY' $offset[1])
    )
    Assert-Contains $output 'LEGACY_PATTERN_ID'
    $hasOffset = $output.Contains('; offset X.') -and $output.Contains('; offset Y.')
    if ($hasOffset -ne ($offset[0] -ne 0 -or $offset[1] -ne 0)) {
        throw "Incorrect offset serialization for $offset"
    }
    if ($hasOffset) {
        Assert-Contains $output "db `$60"
        Assert-Contains $output "dw `$000C`t`t; Block size"
    } else {
        Assert-Contains $output "db `$40"
        Assert-Contains $output "dw `$000A`t`t; Block size"
    }
}
$output = Invoke-Component 8 @((New-Property 'SpriteIndex' 0))
Assert-Contains $output 'TILE_PATTERN_ID'
$output = Invoke-Component 2 @((New-Property 'SpriteIndex' 0))
Assert-Contains $output 'SHEET_PATTERN_ID'
$output = Invoke-Component 0 @((New-Property 'SpriteIndex' 0), (New-Property 'SpriteName' 'legacy'))
Assert-Contains $output 'LEGACY_PATTERN_ID'
Write-Host 'Sprite regression checks passed.'
