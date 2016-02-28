#!/bin/sh

find . -type f -name '*.cs' ! -name '*.Designer.cs' -print0 | while IfS= read -r -d '' file; do
  if ! grep -q 'General Public License' "$file"
  then
    echo Processing cs \" "$file" \" 1>&2  
    cat cs_license.txt "$file" >"$file.new" && mv "$file.new" "$file"
  fi
done

find . -type f -name '*.xml' -print0 | while Ifs= read -r -d '' file; do
  if ! grep -q 'General Public License' "$file"
  then
    echo Processing Xml \" "$file" \" 1>&2
    cat xml_license.txt > "$file.new"; 
    cat "$file" | sed 1d >> "$file.new";
    mv "$file.new" "$file"
  fi
done
