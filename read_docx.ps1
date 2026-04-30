Add-Type -AssemblyName System.IO.Compression.FileSystem
$docxPath = "e:\project Wed\Nh-m-Tan_Hung_Tung-lab8\Nh-m-Tan_Hung_Tung-main\LAB 5_Account Management_Final.docx"
$zip = [System.IO.Compression.ZipFile]::OpenRead($docxPath)
$entry = $zip.GetEntry("word/document.xml")
$reader = new-object System.IO.StreamReader($entry.Open())
$xml = $reader.ReadToEnd()
$reader.Close()
$zip.Dispose()
$text = $xml -replace '<[^>]+>', ' ' -replace '\s+', ' '
Write-Output $text
