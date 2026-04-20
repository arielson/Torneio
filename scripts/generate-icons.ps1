Add-Type -AssemblyName System.Drawing

$ErrorActionPreference = 'Stop'

function New-RoundedRectPath {
    param(
        [float]$X,
        [float]$Y,
        [float]$Width,
        [float]$Height,
        [float]$Radius
    )

    $path = New-Object System.Drawing.Drawing2D.GraphicsPath
    $diameter = $Radius * 2
    $path.AddArc($X, $Y, $diameter, $diameter, 180, 90)
    $path.AddArc($X + $Width - $diameter, $Y, $diameter, $diameter, 270, 90)
    $path.AddArc($X + $Width - $diameter, $Y + $Height - $diameter, $diameter, $diameter, 0, 90)
    $path.AddArc($X, $Y + $Height - $diameter, $diameter, $diameter, 90, 90)
    $path.CloseFigure()
    return $path
}

function Draw-MasterIcon {
    param([string]$OutputPath)

    $size = 1024
    $bmp = New-Object System.Drawing.Bitmap $size, $size
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
    $g.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
    $g.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
    $g.Clear([System.Drawing.Color]::Transparent)

    $bgPath = New-RoundedRectPath -X 32 -Y 32 -Width 960 -Height 960 -Radius 220
    $bgBrush = New-Object System.Drawing.Drawing2D.LinearGradientBrush(
        (New-Object System.Drawing.Rectangle 0, 0, $size, $size),
        [System.Drawing.ColorTranslator]::FromHtml('#0F766E'),
        [System.Drawing.ColorTranslator]::FromHtml('#115E59'),
        45
    )
    $g.FillPath($bgBrush, $bgPath)

    $waveBrush = New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(55, 255, 255, 255))
    $g.FillEllipse($waveBrush, 90, 690, 860, 250)
    $g.FillEllipse($waveBrush, 190, 730, 640, 180)

    $shadowBrush = New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(35, 0, 0, 0))
    $g.FillEllipse($shadowBrush, 250, 740, 520, 120)

    $gold = [System.Drawing.ColorTranslator]::FromHtml('#F4B942')
    $goldDark = [System.Drawing.ColorTranslator]::FromHtml('#D18A14')
    $goldBrush = New-Object System.Drawing.SolidBrush $gold
    $goldDarkBrush = New-Object System.Drawing.SolidBrush $goldDark
    $goldPen = New-Object System.Drawing.Pen $goldDark, 24
    $goldPen.LineJoin = [System.Drawing.Drawing2D.LineJoin]::Round

    $cupPath = New-Object System.Drawing.Drawing2D.GraphicsPath
    $cupPath.AddPolygon([System.Drawing.Point[]]@(
        (New-Object System.Drawing.Point(310, 270)),
        (New-Object System.Drawing.Point(714, 270)),
        (New-Object System.Drawing.Point(650, 540)),
        (New-Object System.Drawing.Point(374, 540))
    ))
    $g.FillPath($goldBrush, $cupPath)
    $g.DrawPath($goldPen, $cupPath)

    $g.DrawArc($goldPen, 204, 300, 170, 180, 255, 210)
    $g.DrawArc($goldPen, 650, 300, 170, 180, 75, 210)

    $g.FillRectangle($goldBrush, 468, 540, 92, 120)
    $g.FillRectangle($goldDarkBrush, 402, 660, 224, 52)
    $g.FillRectangle($goldBrush, 350, 712, 328, 78)

    $innerGlowBrush = New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(40, 255, 255, 255))
    $g.FillEllipse($innerGlowBrush, 380, 315, 265, 150)

    $starBrush = New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::White)
    $starPath = New-Object System.Drawing.Drawing2D.GraphicsPath
    $starPath.AddPolygon([System.Drawing.Point[]]@(
        (New-Object System.Drawing.Point(512, 335)),
        (New-Object System.Drawing.Point(548, 410)),
        (New-Object System.Drawing.Point(630, 420)),
        (New-Object System.Drawing.Point(570, 475)),
        (New-Object System.Drawing.Point(586, 555)),
        (New-Object System.Drawing.Point(512, 515)),
        (New-Object System.Drawing.Point(438, 555)),
        (New-Object System.Drawing.Point(454, 475)),
        (New-Object System.Drawing.Point(394, 420)),
        (New-Object System.Drawing.Point(476, 410))
    ))
    $g.FillPath($starBrush, $starPath)

    $shinePen = New-Object System.Drawing.Pen ([System.Drawing.Color]::FromArgb(70, 255, 255, 255)), 16
    $g.DrawArc($shinePen, 200, 100, 420, 260, 205, 80)

    $dir = Split-Path -Parent $OutputPath
    if (-not (Test-Path $dir)) {
        New-Item -ItemType Directory -Path $dir | Out-Null
    }

    $bmp.Save($OutputPath, [System.Drawing.Imaging.ImageFormat]::Png)

    $shinePen.Dispose()
    $innerGlowBrush.Dispose()
    $starBrush.Dispose()
    $goldPen.Dispose()
    $goldBrush.Dispose()
    $goldDarkBrush.Dispose()
    $shadowBrush.Dispose()
    $waveBrush.Dispose()
    $bgBrush.Dispose()
    $bgPath.Dispose()
    $cupPath.Dispose()
    $starPath.Dispose()
    $g.Dispose()
    $bmp.Dispose()
}

function Resize-Png {
    param(
        [string]$SourcePath,
        [string]$OutputPath,
        [int]$Width,
        [int]$Height
    )

    $src = [System.Drawing.Image]::FromFile($SourcePath)
    $bmp = New-Object System.Drawing.Bitmap $Width, $Height
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
    $g.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
    $g.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
    $g.DrawImage($src, 0, 0, $Width, $Height)

    $dir = Split-Path -Parent $OutputPath
    if (-not (Test-Path $dir)) {
        New-Item -ItemType Directory -Path $dir | Out-Null
    }

    $bmp.Save($OutputPath, [System.Drawing.Imaging.ImageFormat]::Png)
    $g.Dispose()
    $bmp.Dispose()
    $src.Dispose()
}

$root = Split-Path -Parent $PSScriptRoot
$masterPath = Join-Path $root 'App\torneio_app\assets\branding\app-icon-1024.png'

Draw-MasterIcon -OutputPath $masterPath

$androidTargets = @{
    'App\torneio_app\android\app\src\main\res\mipmap-mdpi\ic_launcher.png' = 48
    'App\torneio_app\android\app\src\main\res\mipmap-hdpi\ic_launcher.png' = 72
    'App\torneio_app\android\app\src\main\res\mipmap-xhdpi\ic_launcher.png' = 96
    'App\torneio_app\android\app\src\main\res\mipmap-xxhdpi\ic_launcher.png' = 144
    'App\torneio_app\android\app\src\main\res\mipmap-xxxhdpi\ic_launcher.png' = 192
}

foreach ($item in $androidTargets.GetEnumerator()) {
    Resize-Png -SourcePath $masterPath -OutputPath (Join-Path $root $item.Key) -Width $item.Value -Height $item.Value
}

$iosTargets = @{
    'Icon-App-20x20@1x.png' = 20
    'Icon-App-20x20@2x.png' = 40
    'Icon-App-20x20@3x.png' = 60
    'Icon-App-29x29@1x.png' = 29
    'Icon-App-29x29@2x.png' = 58
    'Icon-App-29x29@3x.png' = 87
    'Icon-App-40x40@1x.png' = 40
    'Icon-App-40x40@2x.png' = 80
    'Icon-App-40x40@3x.png' = 120
    'Icon-App-60x60@2x.png' = 120
    'Icon-App-60x60@3x.png' = 180
    'Icon-App-76x76@1x.png' = 76
    'Icon-App-76x76@2x.png' = 152
    'Icon-App-83.5x83.5@2x.png' = 167
    'Icon-App-1024x1024@1x.png' = 1024
}

$iosDir = Join-Path $root 'App\torneio_app\ios\Runner\Assets.xcassets\AppIcon.appiconset'
foreach ($item in $iosTargets.GetEnumerator()) {
    Resize-Png -SourcePath $masterPath -OutputPath (Join-Path $iosDir $item.Key) -Width $item.Value -Height $item.Value
}

Resize-Png -SourcePath $masterPath -OutputPath (Join-Path $root 'Retaguarda\Torneio.Web\wwwroot\favicon.png') -Width 64 -Height 64

Write-Output "Master icon: $masterPath"
