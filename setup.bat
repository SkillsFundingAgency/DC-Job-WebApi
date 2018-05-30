echo "copying git hook to its folder"
del ".git\hooks\pre-commit.sample"  
xcopy "pre-commit" ".git\hooks\" /y
echo "All done"
pause

