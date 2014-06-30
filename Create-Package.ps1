$name = "AE.Net.Mail";

function create-nuspec() {    
		$spec = get-text "$name.nuspec"
		$spec = $spec.Replace("#version#", (get-version("bin\release\$name.dll")))
		$spec = $spec.Replace("#message#", (get-text(".git\COMMIT_EDITMSG")))
		
		$spec | out-file "bin\Package\AE.Net.Mail.nuspec"
}

function get-text($file) {
		return [string]::join([environment]::newline, (get-content -path $file))
}

function get-version($file) {
		$ANOTHERONE = resolve-path .
		$file = (join-path "$ANOTHERONE" "$file")
		return [System.Diagnostics.FileVersionInfo]::GetVersionInfo($file).FileVersion
}

function build($ver) {
	del "bin\Release" -recurse
	$flag = "NET" + $ver.Replace(".", "")

	$msbuild_ver = "v4.0.30319"
	if($ver -eq "3.5") { $msbuild_ver = "v3.5" }

	$msbuild = "C:\Windows\Microsoft.NET\Framework\$msbuild_ver\msbuild.exe"
	$temp = get-text "$name.csproj"
	$temp = $temp.Replace("<TargetFrameworkVersion>v4.0</TargetFrameworkVersion>", "<TargetFrameworkVersion>v$ver</TargetFrameworkVersion>")
	set-content "$name-$ver.csproj" $temp
	cmd /c "$msbuild $name-$ver.csproj /p:Configuration=Release /p:DefineConstants=""RELEASE;$flag"""
	rm "$name-$ver.csproj"
}

function deploy($ver) {
	$dir = "net" + $ver.Replace(".", "")
	md "bin\Package\lib\$dir\"
	build $ver
	copy "bin\Release\*.*" "bin\Package\lib\$dir\"
}

del "bin\Package" -recurse
#deploy "3.5"
deploy "4.0"
deploy "4.5"
#deploy "4.5.2"

create-nuspec
.nuget\NuGet.exe pack "bin\Package\$name.nuspec" /o "bin\Package"