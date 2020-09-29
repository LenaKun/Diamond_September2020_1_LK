#	
#	prebuild event command:
#
#	Powershell -File   ..\..\prebuild.ps1 $(TargetDatabase) ..\..\  
#

$dbname = $args[0]
$dir = $args[1]


$files = @( "Scripts/Pre-Deployment/Script.PreDeployment.sql",
		"Scripts\Post-Deployment\Script.PostDeployment.sql")

Foreach($f in $files){
	Write-Host "replacing :setvar DatabaseName $dbname in $f"
	$file = $dir + $f
	$content = Get-Content $file
	$content |
		ForEach-Object {
			$_ -replace ':setvar DatabaseName .*', (':setvar DatabaseName "' + "$dbname" + '"')
		} |
		Set-Content $file
}

