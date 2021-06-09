$List = $(git status | findstr modified | findstr app.config)
echo $List

Foreach($file in $List)
{
    $filename = $(($file).Substring(10)).Trim()
    echo $filename
}