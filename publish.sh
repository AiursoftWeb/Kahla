#!/bin/bash

# clean all built assets.
rm -rvf ./www/
rm -rvf ./dist/
npm run prod-electron

# generate package.json without dependencies.
cp ./package.json ./www/
if [[ "$OSTYPE" == "darwin"* ]]
then
    sed -i '' '6,$d' ./www/package.json
else
    sed -i '6,$d' ./www/package.json
fi
echo '  "main": "index.js"' >> ./www/package.json
echo '}' >> ./www/package.json

# call electron builder without publishing anything.
npm run pack

# cp package.json so that release server could read version number.
cp ./package.json ./dist/

# remove additional useless files

rm -rvf ./dist/win-unpacked
rm -rvf ./dist/linux-unpacked
rm -rvf ./dist/mac
