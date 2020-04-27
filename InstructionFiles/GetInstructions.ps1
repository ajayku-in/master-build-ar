Workflow Get-Instructions {
    param (
        $SetId = 0,
        $NumPages = 0
    )

    $UrlFormat = "https://lego.brickinstructions.com/{0}/{1}/{2:D3}.jpg"
    $SetGroupId = $SetId - ($SetId  % 1000);
    New-Item -ItemType Directory -Force -Path "./$SetId"
    foreach -parallel -throttlelimit 100 ( $i in 1..72 ) {
        Invoke-WebRequest -Uri ($UrlFormat -f $SetGroupId,$SetId,$i) -OutFile "$SetId/$i.jpg"
    }
    #for ($i=1; $i -le $NumPages; $i++) {   
    #}
}
