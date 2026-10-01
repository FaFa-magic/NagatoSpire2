param([Parameter(Mandatory)][string]$PackPath)
$ErrorActionPreference = 'Stop'
$stream = [IO.File]::Open($PackPath, [IO.FileMode]::Open, [IO.FileAccess]::ReadWrite)
try {
    $reader = [IO.BinaryReader]::new($stream, [Text.Encoding]::UTF8, $true)
    if ($reader.ReadUInt32() -ne 0x43504447 -or $reader.ReadUInt32() -ne 3) {
        throw 'Expected an unembedded Godot PCK v3.'
    }
    $stream.Position = 20
    if ($reader.ReadUInt32() -ne 2) { throw 'Expected an unencrypted PCK with a trailing directory.' }
    $null = $reader.ReadUInt64()
    $directoryOffset = $reader.ReadUInt64()
    $stream.Position = $directoryOffset
    $count = $reader.ReadUInt32()
    $entries = [Collections.Generic.List[byte[]]]::new()
    $paths = [Collections.Generic.List[string]]::new()
    $removed = [Collections.Generic.List[string]]::new()
    for ($i = 0; $i -lt $count; $i++) {
        $entryStart = $stream.Position
        $pathLength = $reader.ReadUInt32()
        if ($pathLength -gt 65536) { throw 'Invalid PCK resource path length.' }
        $path = [Text.Encoding]::UTF8.GetString($reader.ReadBytes($pathLength)).TrimEnd([char]0)
        $null = $reader.ReadBytes(36)
        $entryEnd = $stream.Position
        $stream.Position = $entryStart
        $bytes = $reader.ReadBytes([int]($entryEnd - $entryStart))
        if ($path -in @('project.binary', '.godot/extension_list.cfg', '.godot/global_script_class_cache.cfg', '.godot/uid_cache.bin')) {
            $removed.Add($path)
        } else {
            $entries.Add($bytes)
            $paths.Add($path)
        }
    }
    if ($stream.Position -ne $stream.Length) { throw 'Unexpected PCK trailer; refusing to rewrite.' }
    $stream.Position = $directoryOffset
    $writer = [IO.BinaryWriter]::new($stream, [Text.Encoding]::UTF8, $true)
    $writer.Write([uint32]$entries.Count)
    foreach ($entry in $entries) { $writer.Write($entry) }
    $writer.Flush()
    $stream.SetLength($stream.Position)
    Write-Output "Validated $($entries.Count) resources. Removed host-global entries: $($removed -join ', ')"
} finally { $stream.Dispose() }
