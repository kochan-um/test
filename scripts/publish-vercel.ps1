param(
  [string]$BuildDir = "Test/WebGLBuild",
  [string]$Branch = "vercel",
  [switch]$Force
)

$ErrorActionPreference = 'Stop'

function Expand-GzipFile([string]$inPath, [string]$outPath){
  Add-Type -AssemblyName System.IO.Compression.FileSystem | Out-Null
  $inStream = [System.IO.File]::OpenRead($inPath)
  try {
    $gz = New-Object System.IO.Compression.GZipStream($inStream, [System.IO.Compression.CompressionMode]::Decompress)
    $outStream = [System.IO.File]::Create($outPath)
    try { $gz.CopyTo($outStream) } finally { $outStream.Dispose() }
  } finally { $inStream.Dispose() }
}

Write-Host "==> Publishing $BuildDir to branch '$Branch'"

# Resolve repo root
$root = (& git rev-parse --show-toplevel)
if(-not $root){ throw 'Not a git repository.' }

# Validate build output
$src = Join-Path $root $BuildDir
$index = Join-Path $src 'index.html'
if(-not (Test-Path -LiteralPath $index)){
  throw "WebGL build not found at $index. Build first (Tools > Build > WebGL (Vercel))."
}

# Prepare worktree
$wt = Join-Path $env:TEMP ("vercel_pub_" + [Guid]::NewGuid().ToString('N'))
$hasLocal = (git show-ref --verify --quiet ("refs/heads/" + $Branch)); $hasLocal = ($LASTEXITCODE -eq 0)
if($hasLocal){
  git worktree add $wt $Branch | Out-Null
} else {
  git worktree add -B $Branch $wt | Out-Null
}

try {
  # Clear worktree except .git
  Get-ChildItem -Force -LiteralPath $wt | Where-Object { $_.Name -ne '.git' } | Remove-Item -Recurse -Force -ErrorAction SilentlyContinue

  # Copy build output into worktree root
  robocopy "$src" "$wt" /E /NFL /NDL /NJH /NJS /NC /NS /NP | Out-Null

  # Decompress .gz artifacts if present and retarget index.html
  $base = 'WebGLBuild'
  $buildOut = Join-Path $wt 'Build'
  $gzFramework = Join-Path $buildOut ($base + '.framework.js.gz')
  $gzData      = Join-Path $buildOut ($base + '.data.gz')
  $gzWasm      = Join-Path $buildOut ($base + '.wasm.gz')
  $framework   = Join-Path $buildOut ($base + '.framework.js')
  $data        = Join-Path $buildOut ($base + '.data')
  $wasm        = Join-Path $buildOut ($base + '.wasm')

  if(Test-Path $gzFramework){ Expand-GzipFile $gzFramework $framework; Remove-Item $gzFramework -Force }
  if(Test-Path $gzData){ Expand-GzipFile $gzData $data; Remove-Item $gzData -Force }
  if(Test-Path $gzWasm){ Expand-GzipFile $gzWasm $wasm; Remove-Item $gzWasm -Force }

  $idx = Join-Path $wt 'index.html'
  if(Test-Path $idx){
    $txt = Get-Content -LiteralPath $idx -Raw
    $txt = $txt -replace '\.framework\.js\.gz', '.framework.js'
    $txt = $txt -replace '\.data\.gz', '.data'
    $txt = $txt -replace '\.wasm\.gz', '.wasm'
    Set-Content -LiteralPath $idx -Value $txt -Encoding UTF8
  }

  # Ensure no vercel.json (default hosting)
  $vercelJson = Join-Path $wt 'vercel.json'
  if(Test-Path $vercelJson){ Remove-Item $vercelJson -Force }

  # Commit & push
  & git -C $wt add -A
  $commitMsg = "publish: WebGL build (script) " + (Get-Date -Format s)
  & git -C $wt commit -m $commitMsg | Out-String | Write-Host
  if($Force){
    & git -C $wt push -u origin $Branch -f | Out-String | Write-Host
  } else {
    & git -C $wt push -u origin $Branch | Out-String | Write-Host
  }
}
finally {
  try { git worktree remove "$wt" -f | Out-Null } catch {}
  try { Remove-Item -Recurse -Force "$wt" -ErrorAction SilentlyContinue } catch {}
}

Write-Host "==> Publish complete."

