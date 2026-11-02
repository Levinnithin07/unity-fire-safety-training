$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$target = Join-Path $root 'FireSafetyTraining/Assets/ThirdParty'
$records = [System.Collections.Generic.List[object]]::new()
function Fetch($url, $path, $expectedMd5 = '') {
    New-Item -ItemType Directory -Force -Path (Split-Path $path -Parent) | Out-Null
    if (!(Test-Path $path)) { Invoke-WebRequest $url -OutFile $path -TimeoutSec 180 }
    $hash = (Get-FileHash $path -Algorithm MD5).Hash.ToLowerInvariant()
    if ($expectedMd5 -and $hash -ne $expectedMd5) { throw "Checksum mismatch: $path" }
    $records.Add([pscustomobject]@{url=$url; path=$path.Substring($root.Length+1); bytes=(Get-Item $path).Length; md5=$hash})
    Write-Output "Acquired: $(Split-Path $path -Leaf)"
}
foreach ($id in @('fire_alarm','korean_fire_extinguisher_01')) {
    $metaPath = Join-Path $root "SourceAssets/$id.files.json"
    Invoke-WebRequest "https://api.polyhaven.com/files/$id" -Headers @{'User-Agent'='FireSafetyAcademicProject/1.0'} -OutFile $metaPath
    $meta = Get-Content $metaPath -Raw | ConvertFrom-Json
    $fbx = $meta.fbx.'2k'.fbx
    Fetch $fbx.url (Join-Path $target "PolyHaven/$id/${id}_2k.fbx") $fbx.md5
    foreach ($file in $fbx.include.PSObject.Properties) {
        Fetch $file.Value.url (Join-Path $target "PolyHaven/$id/$($file.Name)") $file.Value.md5
    }
}
$tree = Invoke-RestMethod 'https://api.github.com/repos/lkewis/HDRP-Classroom/git/trees/main?recursive=1'
$tree | ConvertTo-Json -Depth 12 | Set-Content (Join-Path $root 'SourceAssets/classroom-tree.json')
foreach ($entry in $tree.tree) {
    if ($entry.path -eq 'Assets/__Project/Meshes/ClassroomSceneV4.fbx' -or $entry.path -match '^Assets/__Project/Textures/Classroom/.*\.png$') {
        $relative = $entry.path.Replace('Assets/__Project/','')
        Fetch "https://raw.githubusercontent.com/lkewis/HDRP-Classroom/$($tree.sha)/$($entry.path)" (Join-Path $target "Classroom/$relative")
    }
}
$records | ConvertTo-Json -Depth 5 | Set-Content (Join-Path $root 'Documentation/DownloadedAssets.json')
Write-Output "Downloaded and recorded $($records.Count) files."
