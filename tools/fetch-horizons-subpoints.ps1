<#
.SYNOPSIS
  Fetches reference sub-observer and sub-solar points from the public JPL Horizons API and writes
  tests/AstroCore.Tests/Data/horizons-subpoints.json.

.DESCRIPTION
  One-off tool (network needed only when it is run). Targets: 301 Moon, 499 Mars, 10 Sun, observed
  from the geocentre (CENTER=500@399), QUANTITIES 14 (observer sub-lon/lat), 15 (sub-solar lon/lat)
  and 21 (one-way light time). Dates are 00:00 UTC.

  For each target the JSON keeps Horizons' own notes: the cartographic frame and longitude sense from
  the header ("Target pole/equ" line) and the latitude type and light-time wording from the column
  footnotes, so the tests can convert to AstroCore's planetocentric, east-positive convention.

  Run from the repository root:  pwsh tools/fetch-horizons-subpoints.ps1
#>
[CmdletBinding()]
param(
    [string]$OutFile = (Join-Path $PSScriptRoot '..\tests\AstroCore.Tests\Data\horizons-subpoints.json')
)

$ErrorActionPreference = 'Stop'

$api = 'https://ssd.jpl.nasa.gov/api/horizons.api'
$userAgent = 'AstroCore-tests'
$dates = @('2000-01-01', '2010-07-15', '2020-03-20', '2024-04-08', '2026-11-10', '2035-09-01')
$targets = @(
    @{ Command = '301'; BodyId = 'moon' },
    @{ Command = '499'; BodyId = 'mars' },
    @{ Command = '10';  BodyId = 'sun' }
)

$unixEpoch = [datetime]::new(1970, 1, 1, 0, 0, 0, [DateTimeKind]::Utc)
$jds = foreach ($d in $dates) {
    $utc = [datetime]::ParseExact($d, 'yyyy-MM-dd', [Globalization.CultureInfo]::InvariantCulture,
        [Globalization.DateTimeStyles]'AssumeUniversal, AdjustToUniversal')
    '{0:F1}' -f (2440587.5 + ($utc - $unixEpoch).TotalDays)
}
$tlist = ($jds | ForEach-Object { "'$_'" }) -join ' '

function Get-Footnote([string]$text, [string]$column) {
    # Footnote block for a column, e.g. "'ObsSub-LON, ObsSub-LAT,' = ... Units: DEGREES DEGREES"
    $m = [regex]::Match($text, "'$column-LON, $column-LAT,' =\s*(.*?)Units:", 'Singleline')
    if (-not $m.Success) { throw "Footnote for $column not found" }
    return (($m.Groups[1].Value -replace '\s+', ' ').Trim())
}

function Get-LongitudeSense([string]$note) {
    $m = [regex]::Match($note, 'Positive longitude is to the (EAST|WEST)', 'IgnoreCase')
    if (-not $m.Success) { throw "Longitude sense not stated in: $note" }
    return $m.Groups[1].Value.ToLowerInvariant()
}

function Get-LatitudeType([string]$note) {
    if ($note -match 'planetodetic') { return 'planetodetic' }
    if ($note -match 'planetocentric') { return 'planetocentric' }
    throw "Latitude type not stated in: $note"
}

function ConvertTo-NullableDouble([string]$s) {
    $s = $s.Trim()
    if ($s -eq 'n.a.' -or $s -eq '') { return $null }
    return [double]::Parse($s, [Globalization.CultureInfo]::InvariantCulture)
}

$bodies = foreach ($t in $targets) {
    $query = [ordered]@{
        format      = 'text'
        COMMAND     = "'$($t.Command)'"
        OBJ_DATA    = "'NO'"
        MAKE_EPHEM  = "'YES'"
        EPHEM_TYPE  = "'OBSERVER'"
        CENTER      = "'500@399'"
        TLIST       = $tlist
        TLIST_TYPE  = "'JD'"
        TIME_TYPE   = "'UT'"
        QUANTITIES  = "'14,15,21'"
        ANG_FORMAT  = "'DEG'"
        EXTRA_PREC  = "'YES'"
        CSV_FORMAT  = "'YES'"
    }
    $qs = ($query.GetEnumerator() | ForEach-Object { "$($_.Key)=$([uri]::EscapeDataString($_.Value))" }) -join '&'
    $text = Invoke-RestMethod -Uri "$api`?$qs" -UserAgent $userAgent -Method Get
    if ($text -notmatch '\$\$SOE') { throw "Horizons returned no ephemeris for $($t.Command):`n$text" }

    $name = [regex]::Match($text, 'Target body name:\s*(.+?)\s{2,}').Groups[1].Value.Trim()
    $source = [regex]::Match($text, 'Target body name:.*?\{source:\s*([^}]+)\}').Groups[1].Value.Trim()
    $pole = [regex]::Match($text, 'Target pole/equ\s*:\s*(\S+)\s*\{([^}]+)\}')
    $radii = [regex]::Match($text, 'Target radii\s*:\s*([^k]+)km').Groups[1].Value.Trim()
    $obsNote = Get-Footnote $text 'ObsSub'
    $sunNote = Get-Footnote $text 'SunSub'
    $ltNote = ([regex]::Match($text, "'1-way_down_LT,' =\s*(.*?)Units:\s*(\S+)", 'Singleline'))
    $ltUnits = $ltNote.Groups[2].Value.Trim()
    if ($ltUnits -ne 'MINUTES') { throw "Unexpected light-time units: $ltUnits" }

    $obsSense = Get-LongitudeSense $obsNote
    $sunSense = Get-LongitudeSense $sunNote
    if ($obsSense -ne $sunSense) { throw "Longitude sense differs between columns for $name" }
    $headerSense = if ($pole.Groups[2].Value -match 'West') { 'west' } elseif ($pole.Groups[2].Value -match 'East') { 'east' } else { 'unknown' }
    if ($headerSense -ne $obsSense) { throw "Header and footnote longitude sense disagree for $name" }

    $body = [regex]::Match($text, '\$\$SOE\s*(.*?)\s*\$\$EOE', 'Singleline').Groups[1].Value
    $lines = $body -split "`n" | Where-Object { $_.Trim() -ne '' }
    if ($lines.Count -ne $dates.Count) { throw "Expected $($dates.Count) rows for $name, got $($lines.Count)" }
    $i = 0
    $rows = foreach ($line in $lines) {
        $f = $line.Split(',')
        $expectedTag = [datetime]::ParseExact($dates[$i], 'yyyy-MM-dd', [Globalization.CultureInfo]::InvariantCulture).ToString('yyyy-MMM-dd 00:00:00', [Globalization.CultureInfo]::InvariantCulture)
        if (-not $f[0].Trim().StartsWith($expectedTag)) { throw "Row $i for ${name}: expected $expectedTag, got $($f[0].Trim())" }
        # Columns: date, solar-presence flag, lunar-presence flag, ObsSub-LON, ObsSub-LAT, SunSub-LON, SunSub-LAT, 1-way_down_LT
        [ordered]@{
            observerUtc        = "$($dates[$i])T00:00:00Z"
            horizonsTimeTag    = $f[0].Trim()
            lightTimeMinutes   = ConvertTo-NullableDouble $f[7]
            subObserverLonDeg  = ConvertTo-NullableDouble $f[3]
            subObserverLatDeg  = ConvertTo-NullableDouble $f[4]
            subSolarLonDeg     = ConvertTo-NullableDouble $f[5]
            subSolarLatDeg     = ConvertTo-NullableDouble $f[6]
        }
        $i++
    }

    [ordered]@{
        target              = $t.Command
        bodyId              = $t.BodyId
        name                = $name
        ephemerisSource     = $source
        cartographicFrame   = $pole.Groups[1].Value
        headerLongitudeNote = "{$($pole.Groups[2].Value.Trim())}"
        longitudeSense      = $obsSense
        latitudeType        = Get-LatitudeType $obsNote
        horizonsRadiiKm     = $radii
        lightTimeUnits      = 'minutes'
        subObserverNote     = $obsNote
        subSolarNote        = $sunNote
        lightTimeNote       = (($ltNote.Groups[1].Value -replace '\s+', ' ').Trim())
        rows                = @($rows)
    }
}

$doc = [ordered]@{
    description = 'JPL Horizons observer-table reference values for AstroCore HorizonsSubPointTests. Values are exactly as Horizons prints them (its own longitude sense and latitude type); see each body''s notes.'
    api         = $api
    request     = "EPHEM_TYPE=OBSERVER, CENTER=500@399 (geocentre), QUANTITIES='14,15,21', ANG_FORMAT=DEG, EXTRA_PREC=YES, TIME_TYPE=UT, TLIST (JD UTC) = $($jds -join ', ')"
    fetchedUtc  = (Get-Date).ToUniversalTime().ToString('yyyy-MM-ddTHH:mm:ssZ')
    bodies      = @($bodies)
}

$dir = Split-Path $OutFile -Parent
if (-not (Test-Path $dir)) { New-Item -ItemType Directory -Force $dir | Out-Null }
$doc | ConvertTo-Json -Depth 6 | Set-Content -Path $OutFile -Encoding utf8NoBOM
Write-Host "Wrote $OutFile"
