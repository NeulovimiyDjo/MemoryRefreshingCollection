set PYTHON=C:\Program Files\Python39\python.exe

pip install mercurial
(rmdir fast-export /S /Q)>nul 2>&1
(rmdir tmp /S /Q)>nul 2>&1

git clone https://github.com/frej/fast-export.git

xcopy "..\..\.hg" "tmp\hg-repo\.hg\" /E /I /Q /H /Y /R
pushd tmp\hg-repo
hg update -C
popd


pushd tmp\hg-repo
hg log -r "closed()" -T "{branch}\n" | sort | uniq > ../closedBranches
hg log | grep user: | sort | uniq | sed 's/user: *//' > ../authorsAuto
copy ..\..\authors ..\authors
popd

git init tmp\converted
pushd tmp\converted
git config core.autocrlf false
git config core.ignoreCase false
bash ../../fast-export/hg-fast-export.sh -r ../hg-repo -A ../authors -e Windows-1251 --force
for /F "tokens=*" %%A in (../closedBranches) do git branch -D %%A
popd


pause