
PowerShell -NoProfile -ExecutionPolicy Bypass -Command ".\UploadSupply.ps1"^
    "-repoUrl 'https://example.bbb/repository/app-raw-hosted'"^
    "-supplyFile './supply1.zip'"^
    "-module 'app'"^
    "-version '1.1'"^
    "-issues 'Issue-1','Issue-2','Issue-3'"^
    "-targets 'Test','Prod'"^
    "-release 'R1'"

pause