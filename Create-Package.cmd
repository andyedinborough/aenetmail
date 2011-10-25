rd "bin\Package\lib\net40" /s /q
rd "bin\Package\lib" /s /q
rd "bin\Package" /s /q
md "bin\Package\lib\net40"
copy "bin\Release\*.*" "bin\Package\lib\net40"
copy "AE.Net.Mail.nuspec" "bin\Package\AE.Net.Mail.nuspec"
".nuget\NuGet.exe" pack "bin\Package\AE.Net.Mail.nuspec" /o "bin\Package"