
function create-nuspec() {    
    $spec = get-text "AE.Net.Mail.nuspec"
    $spec = $spec.Replace("#version#", (get-version("bin\release\ae.net.mail.dll")))
    $spec = $spec.Replace("#message#", (get-text(".git\COMMIT_EDITMSG")))
    
    $spec | out-file "bin\Package\AE.Net.Mail.nuspec"
}

function get-text($file) {
    return [string]::join([environment]::newline, (get-content -path $file))
}

function get-version($file) {
    $file = [system.io.path]::combine([environment]::currentdirectory, $file)
    return [System.Diagnostics.FileVersionInfo]::GetVersionInfo($file).FileVersion
}

del "bin\Package" -recurse
md "bin\Package\lib\net40" 
copy "bin\Release\*.*" "bin\Package\lib\net40"
create-nuspec
.nuget\NuGet.exe pack "bin\Package\AE.Net.Mail.nuspec" /o "bin\Package"